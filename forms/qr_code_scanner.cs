using AForge.Video;
using AForge.Video.DirectShow;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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
    /// The page is a self-service kiosk: the institution header sits at the top,
    /// the live camera preview is a large square card in the middle and the
    /// scanned account is shown on the green result card underneath. The first
    /// available camera starts as soon as the form is shown and every frame is
    /// scanned automatically, so there are no start or stop buttons.
    ///
    /// The QR text is handed to <see cref="AttendanceDirectory.RecordEntry"/>, so
    /// only registered ACTIVE accounts are recorded. A repeated scan of the same
    /// code is ignored for a minute instead of flooding the display.
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
        private System.Windows.Forms.Timer? _clockTimer;

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

            DoubleBuffered = true;

            InitializeBarcodeReader();

            // Soft shadows sit in the padding around the two floating cards.
            cameraArea.Paint += (sender, args) => PaintCardShadow(args.Graphics, cameraCard.Bounds);
            resultArea.Paint += (sender, args) => PaintCardShadow(args.Graphics, resultCard.Bounds);
            cameraCard.Resize += (sender, args) => cameraArea.Invalidate();
            resultCard.Resize += (sender, args) => resultArea.Invalidate();

            // The probe timer checks whether the camera that was just opened really
            // delivers frames. Virtual cameras (for example OBS when it is not
            // running) can start without sending a single frame, so the next real
            // camera is tried automatically after a short timeout.
            _cameraProbeTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _cameraProbeTimer.Tick += CameraProbeTimer_Tick;

            // The scanned name and date/time stay on screen for a few seconds,
            // then the card clears itself so the next person can scan immediately.
            _displayTimer = new System.Windows.Forms.Timer { Interval = 2800 };
            _displayTimer.Tick += DisplayTimer_Tick;

            // Kiosk clock in the top right corner.
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += ClockTimer_Tick;

            FormClosing += qr_code_scanner_FormClosing;

            UpdateClock();
            _clockTimer.Start();

            // Kiosk layout: the scanner opens on the whole screen so the square
            // camera has as much room as the monitor allows.
            oop_project.ScreenMode.Apply(this, new Size(1000, 700));
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            ArrangeLayout();

            // AUTOMATIC START: opening the form is enough to detect and start the
            // camera. No camera list and no start / stop buttons are shown.
            StartAutomaticCamera();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeLayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Esc closes the kiosk even when the camera view has the focus.
            if (keyData == Keys.Escape && !isClosing)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ==========================================
        // RESPONSIVE SQUARE LAYOUT
        // ==========================================

        /// <summary>
        /// Places the header, the square camera card, the result card and the
        /// status capsule. The camera keeps a 1:1 aspect ratio and grows to the
        /// largest square that fits the window, so it is always as big as
        /// possible without ever becoming a stretched rectangle.
        /// </summary>
        private void ArrangeLayout()
        {
            if (backdrop is null || cameraView is null || cameraCard is null ||
                ClientSize.Width < 100 || ClientSize.Height < 100)
            {
                return;
            }

            int w = ClientSize.Width;
            int h = ClientSize.Height;

            float dpi = DeviceDpi / 96f;
            int px(int value) => (int)Math.Round(value * dpi);

            int padX = px(24);
            int padTop = px(18);
            int padBottom = px(18);

            // ---- Compact app bar: logos + brand on the left, the page title in
            // the middle and the clock on the right. Keeping the header to a
            // single row leaves far more height for the camera.
            int rowTop = padTop;
            int rowHeight = px(70);

            int logoSize = Math.Clamp(px(44), px(34), px(54));
            int logoTop = rowTop + (rowHeight - logoSize) / 2;

            tcgcLogoBadge.SetBounds(padX, logoTop, logoSize, logoSize);
            lrcLogoBadge.SetBounds(tcgcLogoBadge.Right + px(8), logoTop, logoSize, logoSize);

            int clockWidth = TextRenderer.MeasureText(
                clockLabel.Text,
                clockLabel.Font,
                Size.Empty,
                TextFormatFlags.NoPadding).Width + px(12);

            clockWidth = Math.Clamp(clockWidth, px(150), px(280));

            int clockLeft = w - padX - clockWidth;
            clockLabel.SetBounds(clockLeft, rowTop + (rowHeight - px(24)) / 2, clockWidth, px(24));

            // Brand block directly after the two seals.
            int brandLeft = lrcLogoBadge.Right + px(14);
            int brandWidth = px(184);
            int brandTop = rowTop + (rowHeight - px(38)) / 2;

            brandLabel.SetBounds(brandLeft, brandTop, brandWidth, px(20));
            brandSubLabel.SetBounds(brandLeft, brandTop + px(20) + px(1), brandWidth, px(17));

            // Title + subtitle are centred on the window whenever there is room,
            // and are pushed aside instead of overlapping the brand or the clock.
            int titleTextWidth = TextRenderer.MeasureText(
                titleLabel.Text,
                titleLabel.Font,
                Size.Empty,
                TextFormatFlags.NoPadding).Width;

            int subtitleTextWidth = TextRenderer.MeasureText(
                subtitleLabel.Text,
                subtitleLabel.Font,
                Size.Empty,
                TextFormatFlags.NoPadding).Width;

            int titleHeight = px(42);
            int subtitleHeight = px(20);
            int textBlockTop = rowTop + (rowHeight - titleHeight - subtitleHeight) / 2;

            int textBlockWidth = Math.Max(titleTextWidth, subtitleTextWidth) + px(40);
            int minTextLeft = brandLeft + brandWidth + px(16);
            int maxTextLeft = clockLeft - px(16) - textBlockWidth;
            int textLeft = Math.Clamp((w - textBlockWidth) / 2, minTextLeft, Math.Max(minTextLeft, maxTextLeft));

            titleLabel.SetBounds(textLeft, textBlockTop, textBlockWidth, titleHeight);
            subtitleLabel.SetBounds(textLeft, textBlockTop + titleHeight, textBlockWidth, subtitleHeight);

            int headerBottom = rowTop + rowHeight;

            // ---- Camera / result / status stack. The camera side is computed
            // from the height that is left, then capped by the available width,
            // so the card stays square and is never clipped.
            int shadowPad = px(14);
            int gapHeader = px(14);
            int gapCamera = px(18);
            int gapStatus = px(12);
            int statusHeight = px(44);
            int resultHeight = Math.Clamp((int)Math.Round(h * 0.125), px(104), px(132));

            int availableHeight = h - headerBottom - gapHeader - resultHeight -
                                  gapCamera - statusHeight - gapStatus - padBottom;
            int availableWidth = w - padX * 2 - shadowPad * 2;

            int cardSide = Math.Min(availableHeight, availableWidth);
            cardSide = Math.Max(px(300), cardSide);

            int cardLeft = (w - cardSide) / 2;
            int cameraTop = headerBottom + gapHeader;

            cameraArea.SetBounds(
                cardLeft - shadowPad,
                cameraTop - shadowPad,
                cardSide + shadowPad * 2,
                cardSide + shadowPad * 2);

            cameraCard.SetBounds(shadowPad, shadowPad, cardSide, cardSide);

            int resultTop = cameraTop + cardSide + gapCamera;

            resultArea.SetBounds(
                cardLeft - shadowPad,
                resultTop - shadowPad,
                cardSide + shadowPad * 2,
                resultHeight + shadowPad * 2);

            resultCard.SetBounds(shadowPad, shadowPad, cardSide, resultHeight);

            int statusTop = resultTop + resultHeight + gapStatus;
            statusPill.SetBounds(cardLeft, statusTop, cardSide, statusHeight);

            backdrop.Invalidate();
            cameraArea.Invalidate();
            resultArea.Invalidate();
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

            cameraView.ShowPlaceholder("DETECTING CAMERA", "LOOKING FOR AN AVAILABLE CAMERA");

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

            cameraView.ShowPlaceholder(
                "CONNECTING TO CAMERA",
                "CAMERA " + (index + 1).ToString(CultureInfo.InvariantCulture) +
                " OF " + videoDevices.Count.ToString(CultureInfo.InvariantCulture));

            if (!TryStartCamera(index))
            {
                TryCameraAt(index + 1);
                return;
            }

            SetStatus(
                "CONNECTING TO CAMERA " + (index + 1).ToString(CultureInfo.InvariantCulture) +
                " OF " + videoDevices.Count.ToString(CultureInfo.InvariantCulture) + "...",
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
                SetStatus("CAMERA ACTIVE  •  POINT THE QR CODE TOWARD THE CAMERA.", MutedText);
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
            message = (message ?? string.Empty).ToUpperInvariant();

            _displayTimer?.Stop();

            cameraView.ShowPlaceholder("CAMERA NOT DETECTED", "CONNECT A CAMERA AND REOPEN THE SCANNER");

            resultCard.ShowError("SCANNER OFFLINE", message, "OFFLINE");
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
            if (!cameraView.IsHandleCreated || cameraView.IsDisposed)
            {
                image.Dispose();
                return;
            }

            try
            {
                cameraView.BeginInvoke(new Action(() =>
                {
                    if (isClosing || cameraView.IsDisposed)
                    {
                        image.Dispose();
                        return;
                    }

                    // The control disposes the previous frame when it is replaced.
                    cameraView.Frame = image;
                    cameraView.Scanning = isScanning;
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

                // The same code is not sent to MySQL again for a minute after it was
                // recorded. A refused code is retried, so a database hiccup does not
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

            resultCard.ShowBusy("VERIFYING QR CODE", "CHECKING THE LIBRARY ACCOUNT", "VERIFYING");
            SetStatus("VERIFYING THE QR CODE...", MutedText);

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
                ShowRefusalIfOpen("THE QR CODE COULD NOT BE RECORDED. " + ex.Message);
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
            string name = row.Name.ToUpperInvariant();

            // Role, ID number and the server timestamp form one readable line.
            // Faculty and staff read N/A in the ID NUMBER slot, like the table.
            string detail = (row.Role + "  •  " + row.DisplayIdNumber + "  •  " + row.StampText)
                .ToUpperInvariant();

            if (scan.AlreadyRecorded)
            {
                resultCard.ShowDuplicate(name, detail, "ALREADY RECORDED");
                SetStatus("ALREADY RECORDED  •  " + row.TimeText, WarningText);
            }
            else
            {
                resultCard.ShowSuccess(name, detail, "CHECKED IN");
                SetStatus("ATTENDANCE RECORDED  •  " + row.TimeText, SuccessGreen);
            }

            RestartDisplayTimer();
        }

        private void ShowRefusalIfOpen(string message)
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            message = (message ?? string.Empty).ToUpperInvariant();

            resultCard.ShowError("SCAN REFUSED", message, "NOT RECORDED");
            SetStatus(message, DangerText);
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
            resultCard.ShowIdle("WAITING FOR QR CODE", "POINT THE LIBRARY QR CODE AT THE CAMERA", "READY");

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
            statusPill.Message = (message ?? string.Empty).ToUpperInvariant();
            statusPill.AccentColor = color;
            statusPill.TextColor = color == MutedText
                ? Color.FromArgb(71, 85, 105)
                : color;
        }

        private void UpdateClock()
        {
            clockLabel.Text = DateTime.Now
                .ToString("ddd, MMM d  •  h:mm tt", CultureInfo.InvariantCulture)
                .ToUpperInvariant();
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
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
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            for (int spread = 12; spread >= 1; spread--)
            {
                Rectangle shadow = card;
                shadow.Inflate(spread, spread);
                shadow.Offset(0, 3 + spread / 2);

                int alpha = 2 + (13 - spread);

                using GraphicsPath path = oop_project.DrawHelper.CreateRoundedPath(shadow, 24 + spread);
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
            cameraView.Scanning = false;

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

            _clockTimer?.Stop();
            _clockTimer?.Dispose();
            _clockTimer = null;

            StopCamera();

            cameraView.ClearFrame();

            barcodeReader = null;
        }
    }
}
