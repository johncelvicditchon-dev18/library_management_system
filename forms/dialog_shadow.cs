using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Soft drop shadow drawn behind a borderless pop up, the same weight on the
    /// top, bottom, left and right. CS_DROPSHADOW (the class style Windows uses for
    /// menus) only paints a shadow on the right and bottom, which is why the pop
    /// ups looked uneven. This helper keeps a separate layered window behind the
    /// dialog: it paints a black ring with per pixel alpha, follows the dialog when
    /// it moves and disappears with it.
    /// </summary>
    internal sealed class DialogShadow : IDisposable
    {
        private const int MaxAlpha = 20;
        private const int BlurSteps = 12;
        private const int BaseMargin = 18;
        private const int BaseRadius = 20;

        private const int WsExLayered = 0x00080000;
        private const int WsExTransparent = 0x00000020;
        private const int WsExNoActivate = 0x08000000;
        private const int WsExToolWindow = 0x00000080;

        private const int SwpNoMove = 0x0002;
        private const int SwpNoSize = 0x0001;
        private const int SwpNoActivate = 0x0010;

        private const int UlwAlpha = 0x00000002;
        private const byte AcSrcOver = 0x00;
        private const byte AcSrcAlpha = 0x01;

        // One shadow per dialog, so calling Attach twice never doubles it.
        private static readonly ConditionalWeakTable<Form, DialogShadow> Shadows =
            new ConditionalWeakTable<Form, DialogShadow>();

        private readonly Form _dialog;
        private readonly ShadowWindow _window = new ShadowWindow();

        private int _margin = BaseMargin;
        private int _radius = BaseRadius;
        private bool _disposed;

        private DialogShadow(Form dialog)
        {
            _dialog = dialog;

            dialog.Shown += Dialog_Shown;
            dialog.LocationChanged += Dialog_LocationChanged;
            dialog.SizeChanged += Dialog_SizeChanged;
            dialog.VisibleChanged += Dialog_VisibleChanged;
            dialog.FormClosed += Dialog_Closed;
            dialog.Disposed += Dialog_Closed;
        }

        /// <summary>Adds the soft shadow to a pop up; safe to call more than once.</summary>
        public static void Attach(Form dialog)
        {
            if (dialog == null)
            {
                return;
            }

            Shadows.GetValue(dialog, form => new DialogShadow(form));
        }

        private void Dialog_Shown(object? sender, EventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            // The handle now exists, so the real monitor DPI is known.
            float scale = Math.Max(1f, _dialog.DeviceDpi / 96f);
            _margin = Math.Max(8, (int)Math.Round(BaseMargin * scale));
            _radius = Math.Max(6, (int)Math.Round(BaseRadius * scale));

            // A top level window without an owner is pushed behind the dashboard's
            // owner group, which hides the shadow completely. Sharing the dialog's
            // owner keeps both windows in the same group, so the shadow can sit
            // directly below the dialog and still above the dashboard.
            if (_dialog.Owner != null)
            {
                _window.Owner = _dialog.Owner;
            }

            // A shadow must sit in the same z band as its dialog: topmost dialogs
            // need a topmost shadow, ordinary dialogs an ordinary one.
            _window.TopMost = _dialog.TopMost;

            _window.Show();
            Render();
            PlaceBehindDialog();
        }

        private void Dialog_LocationChanged(object? sender, EventArgs e)
        {
            PlaceBehindDialog();
        }

        private void Dialog_SizeChanged(object? sender, EventArgs e)
        {
            if (_disposed || !_window.Visible)
            {
                return;
            }

            Render();
            PlaceBehindDialog();
        }

        private void Dialog_VisibleChanged(object? sender, EventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            if (_dialog.Visible && _dialog.WindowState != FormWindowState.Minimized)
            {
                if (!_window.Visible)
                {
                    _window.Show();
                    Render();
                }

                PlaceBehindDialog();
            }
            else
            {
                _window.Hide();
            }
        }

        private void Dialog_Closed(object? sender, EventArgs e)
        {
            Dispose();
        }

        /// <summary>Draws the shadow larger than the dialog and puts it just behind it.</summary>
        private void PlaceBehindDialog()
        {
            if (_disposed || !_window.Visible || _dialog.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _window.Location = new Point(_dialog.Left - _margin, _dialog.Top - _margin);

            // Put the shadow directly below its dialog, never above another window.
            SetWindowPos(
                _window.Handle,
                _dialog.Handle,
                0,
                0,
                0,
                0,
                SwpNoMove | SwpNoSize | SwpNoActivate);
        }

        private void Render()
        {
            int width = _dialog.Width + (_margin * 2);
            int height = _dialog.Height + (_margin * 2);

            if (width <= 4 || height <= 4)
            {
                return;
            }

            if (_window.Size != new Size(width, height))
            {
                _window.Size = new Size(width, height);
            }

            using Bitmap bitmap = CreateShadowBitmap(width, height);
            ApplyBitmap(bitmap);
        }

        private Bitmap CreateShadowBitmap(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // The area the dialog itself covers; the shadow rings grow outwards.
                Rectangle card = new Rectangle(
                    _margin, _margin, width - (_margin * 2), height - (_margin * 2));

                for (int step = BlurSteps; step >= 1; step--)
                {
                    int alpha = (int)Math.Round(
                        MaxAlpha * Math.Pow(1.0 - ((step - 1) / (double)BlurSteps), 2.4));

                    if (alpha <= 0)
                    {
                        continue;
                    }

                    Rectangle ring = Rectangle.Inflate(card, step, step);
                    int radius = _radius + step;

                    using GraphicsPath path = DrawHelper.CreateRoundedPath(ring, radius);
                    using SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                    g.FillPath(brush, path);
                }

                // Remove the centre so the shadow only shows around the card.
                using (GraphicsPath inner = DrawHelper.CreateRoundedPath(card, _radius))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;

                    using SolidBrush clear = new SolidBrush(Color.Transparent);
                    g.FillPath(clear, inner);
                }
            }

            return bitmap;
        }

        private void ApplyBitmap(Bitmap bitmap)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memoryDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmap = SelectObject(memoryDc, hBitmap);

                Size size = new Size(bitmap.Width, bitmap.Height);
                Point source = new Point(0, 0);
                Point destination = new Point(_window.Left, _window.Top);

                BlendFunction blend = new BlendFunction
                {
                    BlendOp = AcSrcOver,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AcSrcAlpha
                };

                UpdateLayeredWindow(
                    _window.Handle,
                    screenDc,
                    ref destination,
                    ref size,
                    memoryDc,
                    ref source,
                    0,
                    ref blend,
                    UlwAlpha);
            }
            catch (Exception)
            {
                // A machine without layered window support simply has no shadow.
                _window.Hide();
            }
            finally
            {
                if (oldBitmap != IntPtr.Zero)
                {
                    SelectObject(memoryDc, oldBitmap);
                }

                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }

                if (memoryDc != IntPtr.Zero)
                {
                    DeleteDC(memoryDc);
                }

                if (screenDc != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, screenDc);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _dialog.Shown -= Dialog_Shown;
            _dialog.LocationChanged -= Dialog_LocationChanged;
            _dialog.SizeChanged -= Dialog_SizeChanged;
            _dialog.VisibleChanged -= Dialog_VisibleChanged;
            _dialog.FormClosed -= Dialog_Closed;
            _dialog.Disposed -= Dialog_Closed;

            try
            {
                _window.Hide();
                _window.Dispose();
            }
            catch (Exception)
            {
                // The window is already gone; nothing else to release.
            }
        }

        /// <summary>Borderless layered window that never takes focus or clicks.</summary>
        private sealed class ShadowWindow : Form
        {
            public ShadowWindow()
            {
                AutoScaleMode = AutoScaleMode.None;
                BackColor = Color.Black;
                Enabled = false;
                FormBorderStyle = FormBorderStyle.None;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                TabStop = false;
                Text = string.Empty;
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams parameters = base.CreateParams;
                    parameters.ExStyle |= WsExLayered | WsExTransparent | WsExNoActivate | WsExToolWindow;
                    return parameters;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // The pixels come from UpdateLayeredWindow, not from WM_PAINT.
            }

            protected override void OnPaint(PaintEventArgs e)
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            ref Point pptDst,
            ref Size psize,
            IntPtr hdcSrc,
            ref Point pptSrc,
            int crKey,
            ref BlendFunction pblend,
            int dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            int uFlags);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
