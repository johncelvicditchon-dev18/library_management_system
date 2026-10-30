using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    [ToolboxItem(true)]
    [Description("Full-window photographic backdrop with warm cinematic scrims and a soft halo and drop shadow behind the hero card.")]
    public sealed class WelcomeBackdrop : Control
    {
        private const string PhotoResourceName = "background_green.png";

        private readonly Image? _photo;
        private Rectangle _cardBounds = Rectangle.Empty;

        [Category("Appearance")]
        [Description("Bounds of the hero card. The backdrop uses it to place the glow and the drop shadow.")]
        public Rectangle CardBounds
        {
            get => _cardBounds;
            set
            {
                if (_cardBounds == value)
                {
                    return;
                }

                _cardBounds = value;
                Invalidate();
            }
        }

        public WelcomeBackdrop()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(17, 32, 26);
            _photo = LoadPhoto(PhotoResourceName);
        }

        /// <summary>Loads the embedded campus photo without depending on its folder path.</summary>
        private static Image? LoadPhoto(string fileName)
        {
            System.Reflection.Assembly assembly = typeof(WelcomeBackdrop).Assembly;
            string? resourceName = Array.Find(
                assembly.GetManifestResourceNames(),
                name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                return null;
            }

            using System.IO.Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return null;
            }

            using Image source = Image.FromStream(stream);
            return new Bitmap(source);
        }

        private float DpiScale
        {
            get
            {
                int dpi = DeviceDpi;
                return dpi <= 0 ? 1f : dpi / 96f;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            if (_photo is null)
            {
                PaintFallback(g, rect);
            }
            else
            {
                PaintPhoto(g, rect);
            }

            PaintScrims(g, rect);
            PaintCardHalo(g);
            PaintCardShadow(g);
        }

        /// <summary>Draws the campus photo edge to edge, cropping instead of stretching.</summary>
        private void PaintPhoto(Graphics g, Rectangle rect)
        {
            if (_photo is null)
            {
                return;
            }

            float scale = Math.Max((float)rect.Width / _photo.Width, (float)rect.Height / _photo.Height);
            int width = Math.Max(rect.Width, (int)Math.Ceiling(_photo.Width * scale));
            int height = Math.Max(rect.Height, (int)Math.Ceiling(_photo.Height * scale));

            var target = new Rectangle(
                rect.Left + (rect.Width - width) / 2,
                rect.Top + (rect.Height - height) / 2,
                width,
                height);

            g.DrawImage(_photo, target);
        }

        /// <summary>Green gradient used only if the embedded photo cannot be loaded.</summary>
        private static void PaintFallback(Graphics g, Rectangle rect)
        {
            using (var gradient = new LinearGradientBrush(
                       rect,
                       Color.FromArgb(22, 163, 74),
                       Color.FromArgb(5, 61, 50),
                       LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(gradient, rect);
            }
        }

        /// <summary>
        /// Warm washes that keep the photo vivid but push the card forward:
        /// a light top scrim, a deeper bottom scrim for the credit line and a soft vignette.
        /// </summary>
        private static void PaintScrims(Graphics g, Rectangle rect)
        {
            using (var wash = new SolidBrush(Color.FromArgb(16, 26, 18, 12)))
            {
                g.FillRectangle(wash, rect);
            }

            int bandHeight = Math.Max(1, rect.Height / 2);

            var topBand = new Rectangle(rect.X, rect.Y, rect.Width, bandHeight);
            using (var top = new LinearGradientBrush(
                       topBand,
                       Color.FromArgb(88, 12, 22, 17),
                       Color.FromArgb(0, 12, 22, 17),
                       LinearGradientMode.Vertical))
            {
                g.FillRectangle(top, topBand);
            }

            var bottomBand = new Rectangle(rect.X, rect.Bottom - bandHeight, rect.Width, bandHeight);
            using (var bottom = new LinearGradientBrush(
                       bottomBand,
                       Color.FromArgb(0, 8, 20, 15),
                       Color.FromArgb(158, 8, 20, 15),
                       LinearGradientMode.Vertical))
            {
                g.FillRectangle(bottom, bottomBand);
            }

            using var path = new GraphicsPath();
            path.AddRectangle(rect);

            using var vignette = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(0, 0, 0, 0),
                SurroundColors = new[] { Color.FromArgb(104, 6, 14, 10) },
                FocusScales = new PointF(0.60f, 0.60f)
            };

            g.FillPath(vignette, path);
        }

        /// <summary>Warm light behind the card so it separates from the photo even in bright areas.</summary>
        private void PaintCardHalo(Graphics g)
        {
            if (_cardBounds.Width <= 0 || _cardBounds.Height <= 0)
            {
                return;
            }

            float spread = 110f * DpiScale;
            float spreadY = 100f * DpiScale;

            var glow = new RectangleF(
                _cardBounds.X - spread,
                _cardBounds.Y - spreadY,
                _cardBounds.Width + spread * 2f,
                _cardBounds.Height + spreadY * 2f);

            using var path = new GraphicsPath();
            path.AddEllipse(glow);

            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(34, 255, 249, 240),
                SurroundColors = new[] { Color.FromArgb(0, 255, 249, 240) },
                FocusScales = new PointF(0.62f, 0.62f)
            };

            g.FillPath(brush, path);
        }

        /// <summary>Layered ambient shadow plus a tighter contact shadow under the card.</summary>
        private void PaintCardShadow(Graphics g)
        {
            if (_cardBounds.Width <= 0 || _cardBounds.Height <= 0)
            {
                return;
            }

            float scale = DpiScale;
            int cornerRadius = Math.Max(10, (int)Math.Round(30 * scale));

            for (int layer = 9; layer >= 1; layer--)
            {
                int spread = (int)Math.Round(layer * 5 * scale);

                var bounds = new Rectangle(
                    _cardBounds.X - spread,
                    _cardBounds.Y - spread + (int)Math.Round(18 * scale),
                    _cardBounds.Width + spread * 2,
                    _cardBounds.Height + spread * 2);

                using var path = DrawHelper.CreateRoundedPath(bounds, cornerRadius + spread);
                using var brush = new SolidBrush(Color.FromArgb(7, 8, 26, 19));
                g.FillPath(brush, path);
            }

            for (int layer = 4; layer >= 1; layer--)
            {
                int spread = (int)Math.Round(layer * 2 * scale);

                var bounds = new Rectangle(
                    _cardBounds.X - spread,
                    _cardBounds.Y - spread + (int)Math.Round(9 * scale),
                    _cardBounds.Width + spread * 2,
                    _cardBounds.Height + spread * 2);

                using var path = DrawHelper.CreateRoundedPath(bounds, cornerRadius + spread);
                using var brush = new SolidBrush(Color.FromArgb(12, 5, 18, 13));
                g.FillPath(brush, path);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _photo?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    [ToolboxItem(true)]
    [Description("White card with rounded corners. The corners are clipped so the parent background shows through.")]
    public sealed class RoundedCard : Panel
    {
        private int _cornerRadius = 28;
        private Color _borderColor = Color.FromArgb(241, 245, 249);

        [Category("Appearance")]
        [DefaultValue(28)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = Math.Max(0, value);
                ApplyRegion();
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        public RoundedCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            ApplyRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 4 || Height < 4)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = DrawHelper.CreateRoundedPath(rect, Math.Max(1, _cornerRadius - 1));
            using var pen = new Pen(_borderColor, 1.4f);
            e.Graphics.DrawPath(pen, path);
        }

        private void ApplyRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using var path = DrawHelper.CreateRoundedPath(new Rectangle(0, 0, Width, Height), _cornerRadius);
            Region? previous = Region;
            Region = new Region(path);
            previous?.Dispose();
        }
    }

    [ToolboxItem(true)]
    [Description("Small rounded capsule used for the welcome eyebrow text.")]
    public sealed class PillBadge : Control
    {
        private Color _fillColor = Color.FromArgb(255, 247, 237);
        private Color _borderColor = Color.FromArgb(254, 215, 170);
        private Color _textColor = Color.FromArgb(194, 65, 12);
        private int _cornerRadius = 15;
        private int _horizontalPadding = 18;
        private int _verticalPadding = 7;

        [Category("Appearance")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(15)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Layout")]
        [DefaultValue(18)]
        public int HorizontalPadding
        {
            get => _horizontalPadding;
            set { _horizontalPadding = Math.Max(0, value); AdjustSize(); }
        }

        [Category("Layout")]
        [DefaultValue(7)]
        public int VerticalPadding
        {
            get => _verticalPadding;
            set { _verticalPadding = Math.Max(0, value); AdjustSize(); }
        }

        public PillBadge()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            AutoSize = true;
            TabStop = false;
            Cursor = Cursors.Default;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size textSize = TextRenderer.MeasureText(Text ?? string.Empty, Font);

            return new Size(
                textSize.Width + _horizontalPadding * 2,
                Math.Max(1, textSize.Height) + _verticalPadding * 2);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            AdjustSize();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            AdjustSize();
        }

        private void AdjustSize()
        {
            Size preferred = GetPreferredSize(Size.Empty);
            if (Size != preferred)
            {
                Size = preferred;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width < 4 || Height < 4)
            {
                return;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = DrawHelper.CreateRoundedPath(rect, Math.Min(_cornerRadius, Height / 2));

            using (var fill = new SolidBrush(_fillColor))
            {
                g.FillPath(fill, path);
            }

            using (var border = new Pen(_borderColor, 1f))
            {
                g.DrawPath(border, path);
            }

            TextRenderer.DrawText(
                g,
                Text ?? string.Empty,
                Font,
                rect,
                _textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }
    }

    [ToolboxItem(true)]
    [Description("Short rounded gradient rule used as a brand accent under the logo row.")]
    public sealed class AccentBar : Control
    {
        private Color _startColor = Color.FromArgb(251, 146, 60);
        private Color _endColor = Color.FromArgb(194, 65, 12);

        [Category("Appearance")]
        public Color StartColor
        {
            get => _startColor;
            set { _startColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color EndColor
        {
            get => _endColor;
            set { _endColor = value; Invalidate(); }
        }

        public AccentBar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width < 2 || Height < 1)
            {
                return;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = DrawHelper.CreateRoundedPath(rect, Math.Max(1, Height / 2));

            using (var brush = new LinearGradientBrush(rect, _startColor, _endColor, LinearGradientMode.Horizontal))
            {
                g.FillPath(brush, path);
            }
        }
    }
}
