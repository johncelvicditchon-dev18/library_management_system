
using AForge.Video;
using AForge.Video.DirectShow;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using oop_project.Models;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace library_management_system.forms
{
    /// <summary>
    /// Automatic QR attendance scanner.
    ///
    /// The form follows the reference layout: a large title, the camera feed in a
    /// white rounded card and the scanned account name in the green display card
    /// underneath. There are no buttons and no camera picker: the first available
    /// camera starts as soon as the form is shown and every frame is scanned
    /// automatically.
    ///
    /// The QR text is handed to <see cref="AttendanceDirectory.RecordEntry"/>, so
    /// only registered ACTIVE accounts are recorded. A repeated scan of the same
    /// pass is ignored for a minute instead of flooding the display.
    /// </summary>
    public partial class qr_code_scanner : Form
    {
        private static readonly Color SuccessGreen = Color.FromArgb(5, 150, 105);
        private static readonly Color MutedText = Color.FromArgb(148, 163, 184);
        private static readonly Color WarningText = Color.FromArgb(180, 83, 9);
        private static readonly Color DangerText = Color.FromArgb(185, 28, 28);

        private readonly AttendanceDirectory _attendanceDirectory = new AttendanceDirectory();
        private readonly object _scanGate = new object();

        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;
        private BarcodeReader? barcodeReader;

        private System.Windows.Forms.Timer? _cameraProbeTimer;
        private System.Windows.Forms.Timer? _displayTimer;
        private int _cameraIndex;
        private volatile bool _frameSeen;
        private DateTime _cameraProbeDeadline;

        private volatile bool isClosing;
        private volatile bool isScanning;
        private volatile bool isBusy;
        private int isProcessingFrame;

        private DateTime _nextScanTime = DateTime.MinValue;
        private string _lastCode = string.Empty;
        private DateTime _lastCodeTime = DateTime.MinValue;

        public qr_code_scanner()
        {
            InitializeComponent();
            InitializeBarcodeReader();

            // Soft shadows sit in the padding around the two white cards.
            cameraArea.Paint += (sender, args) => PaintCardShadow(args.Graphics, cameraCard.Bounds);
            nameArea.Paint += (sender, args) => PaintCardShadow(args.Graphics, nameCard.Bounds);
            cameraCard.Resize += (sender, args) => cameraArea.Invalidate();
            nameCard.Resize += (sender, args) => nameArea.Invalidate();

            // The probe timer checks whether the camera that was just opened really
            // delivers frames. Virtual cameras (for example OBS when it is not
            // running) can start without sending a single frame, so the next real
            // camera is tried automatically after a short timeout.
            _cameraProbeTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _cameraProbeTimer.Tick += CameraProbeTimer_Tick;

            // The scanned name and date/time stay on screen for two seconds, then
            // the card clears itself so the next person can scan immediately.
            _displayTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _displayTimer.Tick += DisplayTimer_Tick;

            FormClosing += qr_code_scanner_FormClosing;

            // Kiosk layout: the scanner opens on the whole screen so the camera and
            // the scanned name have the same proportions as the reference layout.
            oop_project.ScreenMode.Apply(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // AUTOMATIC START: opening the form is enough to detect and start the
            // camera. No camera list and no start / stop buttons are shown.
            StartAutomaticCamera();
        }

        // ==========================================
        // BARCODE READER
        // ==========================================

        private void InitializeBarcodeReader()
        {
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,

                Options = new DecodingOptions
                {
                    TryHarder = true,
                    TryInverted = true,

                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.QR_CODE
                    }
                }
            };
        }

        // ==========================================
        // AUTOMATIC CAMERA DETECTION
        // ==========================================

        private void StartAutomaticCamera()
        {
            if (isClosing || isScanning)
            {
                return;
            }

            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            }
            catch (Exception ex)
            {
                ShowCameraUnavailable("CAMERA DETECTION FAILED: " + ex.Message);
                return;
            }

            if (videoDevices.Count == 0)
            {
                ShowCameraUnavailable("NO CAMERA DETECTED. CONNECT A CAMERA AND REOPEN THE SCANNER.");
                return;
            }

            SetStatus("DETECTING CAMERA...", MutedText);

            // A device can start without delivering a single frame (for example a
            // virtual camera when its source application is closed). Each camera is
            // probed for frames and the next one is tried when it stays silent.
            TryCameraAt(0);
        }

        private void TryCameraAt(int index)
        {
            if (isClosing)
            {
                return;
            }

            if (videoDevices == null || index >= videoDevices.Count)
            {
                _cameraProbeTimer?.Stop();
                ShowCameraUnavailable("NO WORKING CAMERA WAS FOUND. CONNECT A CAMERA AND REOPEN THE SCANNER.");
                return;
            }

            _cameraIndex = index;
            _frameSeen = false;
            _cameraProbeDeadline = DateTime.UtcNow.AddSeconds(2.5);

            if (!TryStartCamera(index))
            {
                TryCameraAt(index + 1);
                return;
            }

            SetStatus(
                "CONNECTING TO CAMERA " + (index + 1).ToString() +
                " OF " + videoDevices.Count.ToString() + "...",
                MutedText);

            _cameraProbeTimer?.Stop();
            _cameraProbeTimer?.Start();
        }

        private bool TryStartCamera(int index)
        {
            VideoCaptureDevice? source = null;

            try
            {
                source = new VideoCaptureDevice(videoDevices![index].MonikerString);
                source.NewFrame += VideoSource_NewFrame;

                videoSource = source;
                isScanning = true;

                source.Start();
                return true;
            }
            catch
            {
                isScanning = false;

                if (source != null)
                {
                    source.NewFrame -= VideoSource_NewFrame;

                    try
                    {
                        if (source.IsRunning)
                        {
                            source.SignalToStop();
                            source.WaitForStop();
                        }
                    }
                    catch
                    {
                        // The device may have failed before it started.
                    }
                }

                if (ReferenceEquals(videoSource, source))
                {
                    videoSource = null;
                }

                return false;
            }
        }

        private void CameraProbeTimer_Tick(object? sender, EventArgs e)
        {
            if (isClosing)
            {
                _cameraProbeTimer?.Stop();
                return;
            }

            if (_frameSeen)
            {
                _cameraProbeTimer?.Stop();
                SetStatus("CAMERA ACTIVE  •  POINT THE QR PASS TOWARD THE CAMERA.", MutedText);
                return;
            }

            if (DateTime.UtcNow < _cameraProbeDeadline)
            {
                return;
            }

            // This device started but never delivered a frame. Move to the next one.
            _cameraProbeTimer?.Stop();
            StopCamera();
            TryCameraAt(_cameraIndex + 1);
        }

        private void ShowCameraUnavailable(string message)
        {
            _displayTimer?.Stop();

            cameraPlaceholder.Text = "CAMERA NOT DETECTED";
            cameraPlaceholder.ForeColor = Color.FromArgb(156, 163, 175);
            cameraPlaceholder.Visible = true;
            cameraPlaceholder.BringToFront();

            nameLabel.Text = "SCANNER OFFLINE";
            nameLabel.ForeColor = DangerText;

            SetStatus(message, DangerText);
        }

        // ==========================================
        // CAMERA FRAMES AND AUTOMATIC SCANNING
        // ==========================================

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (isClosing || !isScanning)
            {
                return;
            }

            // The probe timer uses this flag to confirm that the opened device is a
            // real, working camera instead of a virtual camera with no output.
            _frameSeen = true;

            // A frame is still being processed: skipping one keeps the preview smooth.
            if (Interlocked.Exchange(ref isProcessingFrame, 1) == 1)
            {
                return;
            }

            Bitmap? frame = null;
            Bitmap? previewFrame = null;

            try
            {
                frame = (Bitmap)eventArgs.Frame.Clone();
                previewFrame = (Bitmap)frame.Clone();

                ShowPreview(previewFrame);
                previewFrame = null;

                if (!isClosing && !isBusy)
                {
                    DecodeFrame(frame);
                }
            }
            catch
            {
                // Ignore a bad frame and keep the camera running.
            }
            finally
            {
                frame?.Dispose();
                previewFrame?.Dispose();

                Interlocked.Exchange(ref isProcessingFrame, 0);
            }
        }

        private void ShowPreview(Bitmap image)
        {
            if (!cameraPreview.IsHandleCreated || cameraPreview.IsDisposed)
            {
                image.Dispose();
                return;
            }

            try
            {
                cameraPreview.BeginInvoke(new Action(() =>
                {
                    if (isClosing || cameraPreview.IsDisposed)
                    {
                        image.Dispose();
                        return;
                    }

                    Image? oldImage = cameraPreview.Image;

                    cameraPreview.Image = image;
                    oldImage?.Dispose();

                    if (cameraPlaceholder.Visible)
                    {
                        cameraPlaceholder.Visible = false;
                    }
                }));
            }
            catch
            {
                image.Dispose();
            }
        }

        private void DecodeFrame(Bitmap frame)
        {
            // Decode at a steady pace instead of on every single camera frame.
            lock (_scanGate)
            {
                if (DateTime.UtcNow < _nextScanTime)
                {
                    return;
                }

                _nextScanTime = DateTime.UtcNow.AddMilliseconds(350);
            }

            Result? result = barcodeReader?.Decode(frame);

            if (result == null || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            string code = result.Text.Trim();
            bool accept;

            lock (_scanGate)
            {
                DateTime now = DateTime.UtcNow;

                // The same pass is not sent to MySQL again for a minute after it was
                // recorded. A refused pass is retried, so a database hiccup does not
                // lock the user out for a whole minute.
                accept = !(string.Equals(_lastCode, code, StringComparison.Ordinal) &&
                           (now - _lastCodeTime).TotalSeconds < 60);

                _nextScanTime = now.AddSeconds(2);
            }

            if (!accept || isClosing || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(() => ProcessScan(code)));
        }

        // ==========================================
        // RECORD THE SCAN
        // ==========================================

        private async void ProcessScan(string qrText)
        {
            if (isClosing || isBusy)
            {
                return;
            }

            isBusy = true;
            SetStatus("VERIFYING THE QR PASS...", MutedText);

            try
            {
                string code = qrText;

                AttendanceScanResult scan = await Task.Run(
                    () => _attendanceDirectory.RecordEntry(code));

                if (isClosing || IsDisposed)
                {
                    return;
                }

                lock (_scanGate)
                {
                    _lastCode = code;
                    _lastCodeTime = DateTime.UtcNow;
                }

                ShowScan(scan);
            }
            catch (ArgumentException ex)
            {
                ShowRefusalIfOpen(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowRefusalIfOpen(ex.Message);
            }
            catch (MySqlException ex)
            {
                ShowRefusalIfOpen("MYSQL IS OFFLINE, SO THE ATTENDANCE COULD NOT BE SAVED. " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowRefusalIfOpen("THE QR PASS COULD NOT BE RECORDED. " + ex.Message);
            }
            finally
            {
                if (!isClosing && !IsDisposed)
                {
                    isBusy = false;
                }
            }
        }

        private void ShowScan(AttendanceScanResult scan)
        {
            AttendanceRow row = scan.Attendance;

            nameLabel.Text = row.Name.ToUpperInvariant();
            nameLabel.ForeColor = SuccessGreen;

            // Only the date and time are shown next to the name, without the role.
            SetStatus(row.StampText, scan.AlreadyRecorded ? WarningText : SuccessGreen);
            RestartDisplayTimer();
        }

        private void ShowRefusalIfOpen(string message)
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            nameLabel.Text = "SCAN REFUSED";
            nameLabel.ForeColor = DangerText;

            SetStatus((message ?? string.Empty).ToUpperInvariant(), DangerText);
            RestartDisplayTimer();
        }

        private void RestartDisplayTimer()
        {
            _displayTimer?.Stop();
            _displayTimer?.Start();
        }

        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            _displayTimer?.Stop();

            if (isClosing || IsDisposed)
            {
                return;
            }

            // Clear the previous result so the next person can scan right away.
            nameLabel.Text = "WAITING FOR QR CODE...";
            nameLabel.ForeColor = SuccessGreen;

            if (isScanning && _frameSeen)
            {
                SetStatus("CAMERA ACTIVE  •  POINT THE QR CODE TOWARD THE CAMERA.", MutedText);
            }
            else if (isScanning)
            {
                SetStatus("CONNECTING TO CAMERA...", MutedText);
            }
            else
            {
                SetStatus("CAMERA NOT ACTIVE", MutedText);
            }
        }

        private void SetStatus(string message, Color color)
        {
            statusLabel.Text = (message ?? string.Empty).ToUpperInvariant();
            statusLabel.ForeColor = color;
        }

        // ==========================================
        // CARD SHADOW
        // ==========================================

        private static void PaintCardShadow(Graphics graphics, Rectangle card)
        {
            if (card.Width < 10 || card.Height < 10)
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (int spread = 8; spread >= 1; spread--)
            {
                Rectangle shadow = card;
                shadow.Inflate(spread, spread);
                shadow.Offset(0, 3);

                int alpha = 3 + (8 - spread) * 2;

                using GraphicsPath path = oop_project.DrawHelper.CreateRoundedPath(shadow, 18 + spread);
                using SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 15, 23, 42));

                graphics.FillPath(brush, path);
            }
        }

        // ==========================================
        // FORM CLOSING
        // ==========================================

        private void StopCamera()
        {
            isScanning = false;

            if (videoSource == null)
            {
                return;
            }

            VideoCaptureDevice oldSource = videoSource;
            videoSource = null;

            oldSource.NewFrame -= VideoSource_NewFrame;

            try
            {
                if (oldSource.IsRunning)
                {
                    oldSource.SignalToStop();
                    oldSource.WaitForStop();
                }
            }
            catch
            {
                // The camera may already be stopped.
            }
        }

        private void qr_code_scanner_FormClosing(object? sender, FormClosingEventArgs e)
        {
            isClosing = true;

            _cameraProbeTimer?.Stop();
            _cameraProbeTimer?.Dispose();
            _cameraProbeTimer = null;

            _displayTimer?.Stop();
            _displayTimer?.Dispose();
            _displayTimer = null;

            StopCamera();

            if (cameraPreview.Image != null)
            {
                cameraPreview.Image.Dispose();
                cameraPreview.Image = null;
            }

            barcodeReader = null;
        }
    }
}
