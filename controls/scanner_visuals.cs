using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>Small colour helpers shared by the scanner controls.</summary>
    internal static class ScannerPaint
    {
        public static Color Alpha(Color color, int alpha)
        {
            return Color.FromArgb(Math.Clamp(alpha, 0, 255), color.R, color.G, color.B);
        }
    }

    /// <summary>
    /// Soft, near-white background for the attendance kiosk. Two very light
    /// green and orange glows keep the large white cards from looking flat.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Soft gradient backdrop used behind the QR attendance scanner.")]
    public sealed class ScannerBackdrop : Control
    {
        public ScannerBackdrop()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.FromArgb(247, 250, 248);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (var gradient = new LinearGradientBrush(
                       rect,
                       Color.White,
                       Color.FromArgb(237, 247, 241),
                       LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradient, rect);
            }

            PaintGlow(g, rect, Color.FromArgb(16, 185, 129), 26, 0.62f, 0.78f, -0.14f);
            PaintGlow(g, rect, Color.FromArgb(249, 115, 22), 13, 0.70f, 0.06f, 0.94f);
        }

        private static void PaintGlow(
            Graphics g, Rectangle rect, Color color, int alpha,
            float sizeFactor, float centerX, float centerY)
        {
            float size = Math.Max(rect.Width, rect.Height) * sizeFactor;
            var bounds = new RectangleF(
                rect.Width * centerX - size / 2f,
                rect.Height * centerY - size / 2f,
                size,
                size);

            using var path = new GraphicsPath();
            path.AddEllipse(bounds);

            using var brush = new PathGradientBrush(path)
            {
                CenterColor = ScannerPaint.Alpha(color, alpha),
                SurroundColors = new[] { ScannerPaint.Alpha(color, 0) }
            };

            g.FillPath(brush, path);
        }
    }

    /// <summary>
    /// Square camera preview. The frame is cropped ("cover") so the video fills
    /// the square without letterbox bars, and a soft aiming frame with a moving
    /// scan line is drawn on top while the camera is live.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Square camera preview with a live QR aiming frame and animated scan line.")]
    public sealed class ScannerCameraView : Control
    {
        private readonly System.Windows.Forms.Timer _scanTimer;
        private Image? _frame;
        private bool _scanning;
        private float _phase;
        private int _direction = 1;
        private string _placeholderTitle = "CAMERA STARTING";
        private string _placeholderNote = "LOOKING FOR AN AVAILABLE CAMERA";

        public ScannerCameraView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(9, 15, 28);

            _scanTimer = new System.Windows.Forms.Timer { Interval = 33 };
            _scanTimer.Tick += ScanTimer_Tick;
        }

        /// <summary>The current camera frame. The previous frame is disposed when it is replaced.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Frame
        {
            get => _frame;
            set
            {
                if (ReferenceEquals(_frame, value))
                {
                    return;
                }

                Image? previous = _frame;
                _frame = value;
                previous?.Dispose();

                UpdateAnimation();
                Invalidate();
            }
        }

        /// <summary>Whether the camera is delivering frames and the scan overlay should run.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Scanning
        {
            get => _scanning;
            set
            {
                if (_scanning == value)
                {
                    return;
                }

                _scanning = value;
                UpdateAnimation();
                Invalidate();
            }
        }

        public void ClearFrame()
        {
            Frame = null;
        }

        /// <summary>Shows the camera icon and a two-line message instead of a video frame.</summary>
        public void ShowPlaceholder(string title, string note)
        {
            Frame = null;
            _placeholderTitle = string.IsNullOrWhiteSpace(title) ? "CAMERA STARTING" : title.Trim();
            _placeholderNote = string.IsNullOrWhiteSpace(note) ? string.Empty : note.Trim();
            Scanning = false;
            Invalidate();
        }

        private void ScanTimer_Tick(object? sender, EventArgs e)
        {
            if (!_scanning || _frame is null || !Visible || IsDisposed)
            {
                _scanTimer.Stop();
                return;
            }

            _phase += 0.011f * _direction;

            if (_phase >= 1f)
            {
                _phase = 1f;
                _direction = -1;
            }
            else if (_phase <= 0f)
            {
                _phase = 0f;
                _direction = 1;
            }

            Invalidate();
        }

        private void UpdateAnimation()
        {
            if (_scanning && _frame != null && Visible && IsHandleCreated && !IsDisposed)
            {
                if (!_scanTimer.Enabled)
                {
                    _scanTimer.Start();
                }
            }
            else
            {
                _scanTimer.Stop();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateAnimation();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // A child window is not clipped by the rounded region of its parent,
            // so the view clips itself to the same corner radius. Without this the
            // preview would square off the corners of the rounded camera card.
            if (Width > 8 && Height > 8)
            {
                using GraphicsPath path = DrawHelper.CreateRoundedPath(
                    new Rectangle(0, 0, Width, Height),
                    24);

                Region? previous = Region;
                Region = new Region(path);
                previous?.Dispose();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle rect = ClientRectangle;
            if (rect.Width < 8 || rect.Height < 8)
            {
                return;
            }

            using (var background = new SolidBrush(Color.FromArgb(9, 15, 28)))
            {
                g.FillRectangle(background, rect);
            }

            if (_frame is null)
            {
                PaintPlaceholder(g, rect);
            }
            else
            {
                PaintFrame(g, rect);
            }

            if (_frame != null && _scanning)
            {
                PaintAimOverlay(g, rect);
            }

            PaintFrameEdge(g, rect);

            if (_frame != null)
            {
                PaintLiveChip(g, rect);
            }
        }

        private void PaintFrame(Graphics g, Rectangle rect)
        {
            Image? frame = _frame;
            if (frame is null || frame.Width <= 0 || frame.Height <= 0)
            {
                return;
            }

            // Cover the square instead of fitting inside it: the preview looks
            // like a phone scanner and never shows letterbox bars.
            float scale = Math.Max((float)rect.Width / frame.Width, (float)rect.Height / frame.Height);
            int width = Math.Max(rect.Width, (int)Math.Ceiling(frame.Width * scale));
            int height = Math.Max(rect.Height, (int)Math.Ceiling(frame.Height * scale));

            var target = new Rectangle(
                rect.Left + (rect.Width - width) / 2,
                rect.Top + (rect.Height - height) / 2,
                width,
                height);

            g.DrawImage(frame, target);

            // Soft top scrim so the LIVE chip stays readable on bright frames.
            var band = new Rectangle(rect.Left, rect.Top, rect.Width, Math.Max(1, rect.Height / 5));
            using var shade = new LinearGradientBrush(
                band,
                Color.FromArgb(88, 2, 6, 23),
                Color.FromArgb(0, 2, 6, 23),
                LinearGradientMode.Vertical);
            g.FillRectangle(shade, band);
        }

        private void PaintPlaceholder(Graphics g, Rectangle rect)
        {
            using (var glowPath = new GraphicsPath())
            {
                float size = Math.Min(rect.Width, rect.Height) * 0.82f;
                glowPath.AddEllipse(
                    (rect.Width - size) / 2f,
                    (rect.Height - size) / 2f,
                    size,
                    size);

                using var glow = new PathGradientBrush(glowPath)
                {
                    CenterColor = Color.FromArgb(30, 16, 185, 129),
                    SurroundColors = new[] { Color.FromArgb(0, 16, 185, 129) }
                };

                g.FillPath(glow, glowPath);
            }

            int iconWidth = Math.Max(Px(48), rect.Width / 7);
            int iconHeight = (int)Math.Round(iconWidth * 0.68f);

            var icon = new Rectangle(
                rect.Left + (rect.Width - iconWidth) / 2,
                rect.Top + rect.Height / 2 - iconHeight - Px(22),
                iconWidth,
                iconHeight);

            PaintCameraGlyph(g, icon, Color.FromArgb(71, 85, 105));

            using var titleFont = new Font("Segoe UI Semibold", 13F);
            using var noteFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            var titleRect = new Rectangle(rect.Left + Px(24), icon.Bottom + Px(18), rect.Width - Px(48), Px(28));
            TextRenderer.DrawText(
                g,
                _placeholderTitle,
                titleFont,
                titleRect,
                Color.FromArgb(226, 232, 240),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis);

            var noteRect = new Rectangle(rect.Left + Px(24), titleRect.Bottom + Px(1), rect.Width - Px(48), Px(22));
            TextRenderer.DrawText(
                g,
                _placeholderNote,
                noteFont,
                noteRect,
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis);
        }

        private static void PaintCameraGlyph(Graphics g, Rectangle bounds, Color color)
        {
            if (bounds.Width < 8 || bounds.Height < 8)
            {
                return;
            }

            float thickness = Math.Max(2.4f, bounds.Width * 0.05f);

            using var pen = new Pen(color, thickness)
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            var body = new RectangleF(
                bounds.Left + thickness,
                bounds.Top + bounds.Height * 0.25f,
                bounds.Width - thickness * 2f,
                bounds.Height * 0.68f);

            int radius = Math.Max(4, (int)Math.Round(bounds.Height * 0.14f));
            using (var bodyPath = DrawHelper.CreateRoundedPath(Rectangle.Round(body), radius))
            {
                g.DrawPath(pen, bodyPath);
            }

            float bumpWidth = bounds.Width * 0.32f;
            var bump = new RectangleF(
                bounds.Left + (bounds.Width - bumpWidth) / 2f,
                bounds.Top + bounds.Height * 0.10f,
                bumpWidth,
                bounds.Height * 0.22f);

            using (var bumpPath = DrawHelper.CreateRoundedPath(Rectangle.Round(bump), Math.Max(3, radius / 2)))
            {
                g.DrawPath(pen, bumpPath);
            }

            float lens = bounds.Width * 0.28f;
            g.DrawEllipse(
                pen,
                body.Left + (body.Width - lens) / 2f,
                body.Top + (body.Height - lens) / 2f,
                lens,
                lens);
        }

        private void PaintAimOverlay(Graphics g, Rectangle rect)
        {
            int inset = (int)Math.Round(Math.Min(rect.Width, rect.Height) * 0.155f);
            Rectangle aim = Rectangle.Inflate(rect, -inset, -inset);

            if (aim.Width < 60 || aim.Height < 60)
            {
                return;
            }

            // Dim everything outside the aiming square so the QR code in the
            // middle of the frame is the only bright thing on screen.
            using (var mask = new GraphicsPath())
            {
                mask.FillMode = FillMode.Alternate;
                mask.AddRectangle(rect);
                mask.AddRectangle(aim);

                using var dim = new SolidBrush(Color.FromArgb(62, 2, 6, 23));
                g.FillPath(dim, mask);
            }

            int length = Math.Max(22, (int)Math.Round(aim.Width * 0.13f));
            float thickness = Math.Max(3.4f, aim.Width * 0.0105f);
            float offset = thickness / 2f + 1f;

            float left = aim.Left + offset;
            float top = aim.Top + offset;
            float right = aim.Right - offset;
            float bottom = aim.Bottom - offset;

            using var brackets = new GraphicsPath();

            brackets.AddLine(left, top + length, left, top);
            brackets.AddLine(left, top, left + length, top);

            brackets.AddLine(right - length, top, right, top);
            brackets.AddLine(right, top, right, top + length);

            brackets.AddLine(right, bottom - length, right, bottom);
            brackets.AddLine(right, bottom, right - length, bottom);

            brackets.AddLine(left + length, bottom, left, bottom);
            brackets.AddLine(left, bottom, left, bottom - length);

            using (var glowPen = new Pen(Color.FromArgb(46, 52, 211, 153), thickness * 2.6f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
                g.DrawPath(glowPen, brackets);
            }

            using (var bracketPen = new Pen(Color.FromArgb(236, 255, 255, 255), thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
                g.DrawPath(bracketPen, brackets);
            }

            float y = aim.Top + aim.Height * _phase;
            int bandHalf = Math.Max(10, (int)Math.Round(aim.Height * 0.05f));

            var band = new RectangleF(aim.Left, y - bandHalf, aim.Width, bandHalf * 2f);
            if (band.Height > 1f)
            {
                using var lineBrush = new LinearGradientBrush(
                    band,
                    Color.FromArgb(0, 52, 211, 153),
                    Color.FromArgb(0, 52, 211, 153),
                    LinearGradientMode.Vertical);

                lineBrush.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(0, 52, 211, 153),
                        Color.FromArgb(168, 52, 211, 153),
                        Color.FromArgb(0, 52, 211, 153)
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };

                g.FillRectangle(lineBrush, band);
            }

            using var core = new Pen(
                Color.FromArgb(232, 209, 250, 229),
                Math.Max(1.4f, thickness * 0.48f));

            g.DrawLine(core, aim.Left + thickness, y, aim.Right - thickness, y);
        }

        private void PaintLiveChip(Graphics g, Rectangle rect)
        {
            using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            Size textSize = TextRenderer.MeasureText(
                g, "LIVE", font, Size.Empty, TextFormatFlags.NoPadding);

            int height = textSize.Height + Px(11);
            int width = textSize.Width + Px(38);

            var chip = new Rectangle(rect.Left + Px(16), rect.Top + Px(16), width, height);

            using (var path = DrawHelper.CreateRoundedPath(chip, height / 2))
            {
                using (var fill = new SolidBrush(Color.FromArgb(148, 2, 6, 23)))
                {
                    g.FillPath(fill, path);
                }

                using var border = new Pen(Color.FromArgb(46, 255, 255, 255), 1f);
                g.DrawPath(border, path);
            }

            float pulse = (float)((Math.Sin(Environment.TickCount / 360.0) + 1.0) / 2.0);
            int dotSize = Px(7);
            var dot = new Rectangle(
                chip.Left + Px(13),
                chip.Top + (chip.Height - dotSize) / 2,
                dotSize,
                dotSize);

            var halo = dot;
            halo.Inflate(dotSize / 2, dotSize / 2);

            using (var haloBrush = new SolidBrush(Color.FromArgb((int)(42 + 52 * pulse), 52, 211, 153)))
            {
                g.FillEllipse(haloBrush, halo);
            }

            using (var dotBrush = new SolidBrush(Color.FromArgb(52, 211, 153)))
            {
                g.FillEllipse(dotBrush, dot);
            }

            var textRect = new Rectangle(
                dot.Right + Px(7),
                chip.Top,
                chip.Right - dot.Right - Px(9),
                chip.Height);

            TextRenderer.DrawText(
                g,
                "LIVE",
                font,
                textRect,
                Color.FromArgb(236, 253, 245),
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }

        private static void PaintFrameEdge(Graphics g, Rectangle rect)
        {
            using var pen = new Pen(Color.FromArgb(34, 255, 255, 255), 1f);
            g.DrawRectangle(pen, rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
        }

        private int Px(int value)
        {
            return (int)Math.Round(value * (DeviceDpi / 96f));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanTimer.Stop();
                _scanTimer.Dispose();

                _frame?.Dispose();
                _frame = null;
            }

            base.Dispose(disposing);
        }
    }

    public enum ScannerTone
    {
        Idle,
        Busy,
        Success,
        Duplicate,
        Error
    }

    /// <summary>
    /// Result card under the camera: a large account name, the role / ID / time
    /// line and a colour-coded outcome badge (checked in, already recorded,
    /// refused...). The colours change with the <see cref="ScannerTone"/>.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Rounded result card that shows the scanned account and the scan outcome.")]
    public sealed class ScannerResultCard : Control
    {
        private sealed class Palette
        {
            public Color Background { get; init; }
            public Color Border { get; init; }
            public Color Accent { get; init; }
            public Color IconFill { get; init; }
            public Color Heading { get; init; }
            public Color Detail { get; init; }
        }

        private readonly System.Windows.Forms.Timer _spinner;
        private float _spin;
        private ScannerTone _tone = ScannerTone.Idle;
        private string _heading = "WAITING FOR QR CODE";
        private string _detail = "POINT THE LIBRARY QR CODE AT THE CAMERA";
        private string _badge = "READY";

        public ScannerResultCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;

            _spinner = new System.Windows.Forms.Timer { Interval = 40 };
            _spinner.Tick += Spinner_Tick;
        }

        public ScannerTone Tone => _tone;

        public void ShowIdle(string heading, string detail, string badge = "READY")
        {
            Apply(ScannerTone.Idle, heading, detail, badge);
        }

        public void ShowBusy(string heading, string detail, string badge = "VERIFYING")
        {
            Apply(ScannerTone.Busy, heading, detail, badge);
        }

        public void ShowSuccess(string heading, string detail, string badge = "CHECKED IN")
        {
            Apply(ScannerTone.Success, heading, detail, badge);
        }

        public void ShowDuplicate(string heading, string detail, string badge = "ALREADY RECORDED")
        {
            Apply(ScannerTone.Duplicate, heading, detail, badge);
        }

        public void ShowError(string heading, string detail, string badge = "NOT RECORDED")
        {
            Apply(ScannerTone.Error, heading, detail, badge);
        }

        private void Apply(ScannerTone tone, string heading, string detail, string badge)
        {
            _tone = tone;
            _heading = string.IsNullOrWhiteSpace(heading) ? "WAITING FOR QR CODE" : heading.Trim();
            _detail = string.IsNullOrWhiteSpace(detail) ? string.Empty : detail.Trim();
            _badge = string.IsNullOrWhiteSpace(badge) ? string.Empty : badge.Trim();

            if (tone == ScannerTone.Busy && Visible)
            {
                _spinner.Start();
            }
            else
            {
                _spinner.Stop();
            }

            Invalidate();
        }

        private void Spinner_Tick(object? sender, EventArgs e)
        {
            if (_tone != ScannerTone.Busy || !Visible || IsDisposed)
            {
                _spinner.Stop();
                return;
            }

            _spin = (_spin + 14f) % 360f;
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && _tone == ScannerTone.Busy)
            {
                _spinner.Start();
            }
            else
            {
                _spinner.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width < 40 || rect.Height < 30)
            {
                return;
            }

            float scale = DeviceDpi / 96f;
            Palette palette = GetPalette(_tone);

            int radius = (int)Math.Round(22 * scale);
            using (var path = DrawHelper.CreateRoundedPath(rect, radius))
            {
                using (var fill = new SolidBrush(palette.Background))
                {
                    g.FillPath(fill, path);
                }

                using var border = new Pen(palette.Border, 1.4f);
                g.DrawPath(border, path);
            }

            int iconSize = Math.Clamp(
                (int)Math.Round(rect.Width * 0.117),
                (int)Math.Round(40 * scale),
                (int)Math.Round(58 * scale));

            var iconRect = new Rectangle(
                (int)Math.Round(24 * scale),
                (rect.Height - iconSize) / 2,
                iconSize,
                iconSize);

            using (var iconFill = new SolidBrush(palette.IconFill))
            {
                g.FillEllipse(iconFill, iconRect);
            }

            using (var iconRing = new Pen(ScannerPaint.Alpha(palette.Accent, 48), 1.2f))
            {
                g.DrawEllipse(iconRing, iconRect);
            }

            PaintGlyph(g, iconRect, palette.Accent, rect);

            int textLeft = iconRect.Right + (int)Math.Round(20 * scale);
            int textRight = rect.Right - (int)Math.Round(22 * scale);
            int detailWidth = Math.Max(40, textRight - textLeft);

            using var headingFont = new Font(
                "Segoe UI Semibold",
                Math.Clamp(Width / 29f, 12.5f, 17f),
                FontStyle.Bold);

            using var detailFont = new Font(
                "Segoe UI",
                Math.Clamp(Width / 55f, 7.5f, 9f),
                FontStyle.Bold);

            int headingHeight = (int)Math.Round(32 * scale);
            int detailHeight = (int)Math.Round(40 * scale);
            int contentHeight = headingHeight + (int)Math.Round(2 * scale) + detailHeight;
            int top = Math.Max((int)Math.Round(10 * scale), (rect.Height - contentHeight) / 2);

            int headingWidth = detailWidth;

            if (_badge.Length > 0)
            {
                using var badgeFont = new Font("Segoe UI", 8F, FontStyle.Bold);

                Size badgeText = TextRenderer.MeasureText(
                    g, _badge, badgeFont, Size.Empty, TextFormatFlags.NoPadding);

                int badgeWidth = badgeText.Width + (int)Math.Round(26 * scale);
                int badgeHeight = (int)Math.Round(26 * scale);

                var badge = new Rectangle(
                    textRight - badgeWidth,
                    top + (headingHeight - badgeHeight) / 2,
                    badgeWidth,
                    badgeHeight);

                // The badge only appears when the account name still gets a
                // comfortable amount of room; the status pill also carries the
                // outcome, so a narrow card stays readable without it.
                int nameWidth = badge.Left - (int)Math.Round(16 * scale) - textLeft;

                if (nameWidth >= (int)Math.Round(190 * scale))
                {
                    headingWidth = nameWidth;

                    using (var badgePath = DrawHelper.CreateRoundedPath(badge, badgeHeight / 2))
                    {
                        using (var fill = new SolidBrush(ScannerPaint.Alpha(palette.Accent, 28)))
                        {
                            g.FillPath(fill, badgePath);
                        }

                        using var border = new Pen(ScannerPaint.Alpha(palette.Accent, 105), 1f);
                        g.DrawPath(border, badgePath);
                    }

                    TextRenderer.DrawText(
                        g,
                        _badge,
                        badgeFont,
                        badge,
                        palette.Accent,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding);
                }
            }

            var headingRect = new Rectangle(textLeft, top, Math.Max(40, headingWidth), headingHeight);
            TextRenderer.DrawText(
                g,
                _heading,
                headingFont,
                headingRect,
                palette.Heading,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);

            var detailRect = new Rectangle(
                textLeft,
                headingRect.Bottom + (int)Math.Round(2 * scale),
                detailWidth,
                detailHeight);

            TextRenderer.DrawText(
                g,
                _detail,
                detailFont,
                detailRect,
                palette.Detail,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.WordBreak |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);
        }

        private void PaintGlyph(Graphics g, Rectangle circle, Color accent, Rectangle card)
        {
            float scale = circle.Width / 58f;
            float centerX = circle.Left + circle.Width / 2f;
            float centerY = circle.Top + circle.Height / 2f;
            float thickness = Math.Max(2.4f, 3.4f * scale);

            switch (_tone)
            {
                case ScannerTone.Success:
                    using (var pen = new Pen(accent, thickness)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round,
                        LineJoin = LineJoin.Round
                    })
                    using (var check = new GraphicsPath())
                    {
                        check.AddLine(centerX - 9f * scale, centerY + 0.4f * scale, centerX - 2.6f * scale, centerY + 7f * scale);
                        check.AddLine(centerX - 2.6f * scale, centerY + 7f * scale, centerX + 10f * scale, centerY - 7f * scale);
                        g.DrawPath(pen, check);
                    }
                    break;

                case ScannerTone.Error:
                    using (var pen = new Pen(accent, thickness)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    })
                    {
                        g.DrawLine(pen, centerX - 7f * scale, centerY - 7f * scale, centerX + 7f * scale, centerY + 7f * scale);
                        g.DrawLine(pen, centerX + 7f * scale, centerY - 7f * scale, centerX - 7f * scale, centerY + 7f * scale);
                    }
                    break;

                case ScannerTone.Duplicate:
                    using (var pen = new Pen(accent, thickness)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    })
                    {
                        g.DrawLine(pen, centerX, centerY - 9f * scale, centerX, centerY + 1.5f * scale);

                        using var dot = new SolidBrush(accent);
                        float dotSize = Math.Max(3f, 3.4f * scale);
                        g.FillEllipse(dot, centerX - dotSize / 2f, centerY + 7.4f * scale - dotSize / 2f, dotSize, dotSize);
                    }
                    break;

                case ScannerTone.Busy:
                    var box = new RectangleF(
                        centerX - 10f * scale,
                        centerY - 10f * scale,
                        20f * scale,
                        20f * scale);

                    using (var track = new Pen(ScannerPaint.Alpha(accent, 46), thickness * 0.85f)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    })
                    {
                        g.DrawArc(track, box, 0f, 360f);
                    }

                    using (var arc = new Pen(accent, thickness)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    })
                    {
                        g.DrawArc(arc, box, _spin, 104f);
                        g.DrawArc(arc, box, _spin + 180f, 104f);
                    }
                    break;

                default:
                    PaintQrGlyph(g, circle, accent, scale);
                    break;
            }
        }

        private static void PaintQrGlyph(Graphics g, Rectangle circle, Color accent, float scale)
        {
            float size = 24f * scale;
            float left = circle.Left + (circle.Width - size) / 2f;
            float top = circle.Top + (circle.Height - size) / 2f;
            float cell = size / 7f;
            float finder = cell * 3f;

            using var pen = new Pen(accent, Math.Max(1.5f, 1.7f * scale))
            {
                Alignment = PenAlignment.Inset
            };

            g.DrawRectangle(pen, left, top, finder, finder);
            g.DrawRectangle(pen, left + size - finder, top, finder, finder);
            g.DrawRectangle(pen, left, top + size - finder, finder, finder);

            using var brush = new SolidBrush(accent);
            float module = Math.Max(1.6f, cell * 0.95f);

            g.FillRectangle(brush, left + cell * 4.2f, top + cell * 4.2f, module, module);
            g.FillRectangle(brush, left + cell * 5.4f, top + cell * 4.9f, module, module);
            g.FillRectangle(brush, left + cell * 4.6f, top + cell * 5.8f, module, module);
            g.FillRectangle(brush, left + cell * 5.8f, top + cell * 6.0f, module, module);
        }

        private static Palette GetPalette(ScannerTone tone)
        {
            switch (tone)
            {
                case ScannerTone.Success:
                    return new Palette
                    {
                        Background = Color.FromArgb(240, 253, 244),
                        Border = Color.FromArgb(187, 247, 208),
                        Accent = Color.FromArgb(5, 150, 105),
                        IconFill = Color.FromArgb(220, 252, 231),
                        Heading = Color.FromArgb(6, 78, 59),
                        Detail = Color.FromArgb(4, 120, 87)
                    };

                case ScannerTone.Duplicate:
                    return new Palette
                    {
                        Background = Color.FromArgb(255, 251, 235),
                        Border = Color.FromArgb(253, 230, 138),
                        Accent = Color.FromArgb(180, 83, 9),
                        IconFill = Color.FromArgb(254, 243, 199),
                        Heading = Color.FromArgb(120, 53, 15),
                        Detail = Color.FromArgb(180, 83, 9)
                    };

                case ScannerTone.Error:
                    return new Palette
                    {
                        Background = Color.FromArgb(254, 242, 242),
                        Border = Color.FromArgb(254, 202, 202),
                        Accent = Color.FromArgb(185, 28, 28),
                        IconFill = Color.FromArgb(254, 226, 226),
                        Heading = Color.FromArgb(127, 29, 29),
                        Detail = Color.FromArgb(185, 28, 28)
                    };

                case ScannerTone.Busy:
                    return new Palette
                    {
                        Background = Color.FromArgb(248, 250, 252),
                        Border = Color.FromArgb(226, 232, 240),
                        Accent = Color.FromArgb(2, 132, 199),
                        IconFill = Color.FromArgb(224, 242, 254),
                        Heading = Color.FromArgb(15, 23, 42),
                        Detail = Color.FromArgb(71, 85, 105)
                    };

                default:
                    return new Palette
                    {
                        Background = Color.White,
                        Border = Color.FromArgb(226, 232, 240),
                        Accent = Color.FromArgb(100, 116, 139),
                        IconFill = Color.FromArgb(241, 245, 249),
                        Heading = Color.FromArgb(15, 23, 42),
                        Detail = Color.FromArgb(100, 116, 139)
                    };
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spinner.Stop();
                _spinner.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Small capsule under the result card that carries the live instruction
    /// ("point the pass at the camera") and every scan status message.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Rounded status capsule with a coloured dot and a one-line message.")]
    public sealed class ScannerStatusPill : Control
    {
        private string _message = "CAMERA STARTING...";
        private Color _accentColor = Color.FromArgb(148, 163, 184);
        private Color _textColor = Color.FromArgb(71, 85, 105);

        public ScannerStatusPill()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;

            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        [Category("Appearance")]
        public string Message
        {
            get => _message;
            set
            {
                _message = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width < 20 || rect.Height < 12)
            {
                return;
            }

            using (var path = DrawHelper.CreateRoundedPath(rect, rect.Height / 2))
            {
                using (var fill = new SolidBrush(Color.FromArgb(246, 255, 255, 255)))
                {
                    g.FillPath(fill, path);
                }

                using var border = new Pen(Color.FromArgb(226, 232, 240), 1.2f);
                g.DrawPath(border, path);
            }

            float scale = DeviceDpi / 96f;
            int dotSize = Math.Max(6, (int)Math.Round(8 * scale));
            int gap = (int)Math.Round(10 * scale);
            int padding = (int)Math.Round(18 * scale);

            Size textSize = TextRenderer.MeasureText(
                g, _message, Font, Size.Empty, TextFormatFlags.NoPadding);

            int groupWidth = dotSize + gap + textSize.Width;
            int available = rect.Width - padding * 2;
            int startX = groupWidth <= available ? (rect.Width - groupWidth) / 2 : padding;

            var dot = new Rectangle(
                startX,
                rect.Top + (rect.Height - dotSize) / 2,
                dotSize,
                dotSize);

            var halo = dot;
            halo.Inflate((int)Math.Round(4 * scale), (int)Math.Round(4 * scale));

            using (var haloBrush = new SolidBrush(ScannerPaint.Alpha(_accentColor, 52)))
            {
                g.FillEllipse(haloBrush, halo);
            }

            using (var dotBrush = new SolidBrush(_accentColor))
            {
                g.FillEllipse(dotBrush, dot);
            }

            var textRect = new Rectangle(
                dot.Right + gap,
                rect.Top,
                Math.Max(10, rect.Right - dot.Right - gap - padding),
                rect.Height);

            TextRenderer.DrawText(
                g,
                _message,
                Font,
                textRect,
                _textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);
        }
    }

    /// <summary>Round close button used in the kiosk corner.</summary>
    [ToolboxItem(true)]
    [Description("Circular close button for the kiosk window.")]
    public sealed class ScannerCloseButton : Control
    {
        private bool _hover;
        private bool _pressed;

        public ScannerCloseButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            if (rect.Width < 10 || rect.Height < 10)
            {
                return;
            }

            rect.Width -= 1;
            rect.Height -= 1;

            Color fill = _pressed
                ? Color.FromArgb(254, 226, 226)
                : _hover ? Color.FromArgb(254, 242, 242) : Color.White;

            Color border = _hover ? Color.FromArgb(254, 202, 202) : Color.FromArgb(226, 232, 240);
            Color glyph = _hover ? Color.FromArgb(185, 28, 28) : Color.FromArgb(100, 116, 139);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);

                using (var fillBrush = new SolidBrush(fill))
                {
                    g.FillPath(fillBrush, path);
                }

                using (var borderPen = new Pen(border, 1.2f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            int inset = Math.Max(11, rect.Width / 3);
            using var pen = new Pen(glyph, 2.1f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            g.DrawLine(pen, rect.Left + inset, rect.Top + inset, rect.Right - inset, rect.Bottom - inset);
            g.DrawLine(pen, rect.Right - inset, rect.Top + inset, rect.Left + inset, rect.Bottom - inset);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }
    }
}
