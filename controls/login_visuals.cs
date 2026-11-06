using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace oop_project
{
    internal static class DrawHelper
    {
        public static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = Math.Max(2, radius * 2);

            if (bounds.Width <= diameter || bounds.Height <= diameter)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    [ToolboxItem(true)]
    [Description("Panel that paints a vertical two-color gradient with soft light circles.")]
    public class GradientPanel : Panel
    {
        private Color _gradientTop = Color.FromArgb(6, 78, 59);
        private Color _gradientBottom = Color.FromArgb(22, 163, 74);

        public Color GradientTop
        {
            get => _gradientTop;
            set { _gradientTop = value; Invalidate(); }
        }

        public Color GradientBottom
        {
            get => _gradientBottom;
            set { _gradientBottom = value; Invalidate(); }
        }

        public GradientPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = _gradientTop;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (var brush = new LinearGradientBrush(rect, _gradientTop, _gradientBottom, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            using (var glow = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
            {
                g.FillEllipse(glow, rect.Width * 0.42f, -rect.Width * 0.38f, rect.Width * 1.05f, rect.Width * 1.05f);
            }

            using (var glow = new SolidBrush(Color.FromArgb(16, 255, 255, 255)))
            {
                g.FillEllipse(glow, -rect.Width * 0.38f, rect.Height * 0.60f, rect.Width * 0.95f, rect.Width * 0.95f);
            }

            using (var ring = new Pen(Color.FromArgb(30, 255, 255, 255), 1.5f))
            {
                g.DrawEllipse(ring, rect.Width * 0.58f, rect.Height * 0.04f, rect.Width * 0.55f, rect.Width * 0.55f);
            }
        }
    }

    [ToolboxItem(true)]
    [Description("Panel with a warm orange gradient background.")]
    public sealed class OrangeGradientPanel : GradientPanel
    {
        public OrangeGradientPanel()
        {
            GradientTop = Color.FromArgb(249, 115, 22);
            GradientBottom = Color.FromArgb(194, 65, 12);
        }
    }

    [ToolboxItem(true)]
    [Description("Green gradient panel with an orange gradient circle.")]
    public sealed class MixedGradientPanel : GradientPanel
    {
        private Color _circleColor = Color.FromArgb(249, 115, 22);

        [Category("Appearance")]
        [Description("Color of the gradient circle drawn on the panel.")]
        public Color CircleColor
        {
            get => _circleColor;
            set { _circleColor = value; Invalidate(); }
        }

        public MixedGradientPanel()
        {
            GradientTop = Color.FromArgb(6, 78, 59);
            GradientBottom = Color.FromArgb(22, 163, 74);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (var brush = new LinearGradientBrush(rect, GradientTop, GradientBottom, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            float size = Math.Min(rect.Width, rect.Height) * 0.8f;
            if (size < 4f)
            {
                return;
            }

            var circle = new RectangleF((rect.Width - size) / 2f, (rect.Height - size) / 2f, size, size);

            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(Inflate(circle, size * 0.25f));
                using var glow = new PathGradientBrush(glowPath)
                {
                    CenterColor = Color.FromArgb(70, _circleColor),
                    SurroundColors = new[] { Color.FromArgb(0, _circleColor) }
                };
                g.FillPath(glow, glowPath);
            }

            float inner = size * 0.72f;
            var innerRect = new RectangleF(
                circle.X + (circle.Width - inner) / 2f,
                circle.Y + (circle.Height - inner) / 2f,
                inner,
                inner);

            using (var fill = new LinearGradientBrush(
                       innerRect,
                       Color.FromArgb(253, 186, 116),
                       _circleColor,
                       LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(fill, innerRect);
            }

            using (var ring = new Pen(Color.FromArgb(110, 255, 255, 255), 2f))
            {
                g.DrawEllipse(ring, innerRect);
            }
        }

        private static RectangleF Inflate(RectangleF bounds, float amount)
        {
            return new RectangleF(
                bounds.X - amount,
                bounds.Y - amount,
                bounds.Width + amount * 2f,
                bounds.Height + amount * 2f);
        }
    }

    public enum LogoKind
    {
        Tcgc,
        Lrc
    }

    [ToolboxItem(true)]
    public class LogoBadge : Control
    {
        private Image? _logo;
        private LogoKind _logoKind = LogoKind.Tcgc;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Logo
        {
            get => _logo;
            set { _logo = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(LogoKind.Tcgc)]
        [Description("Selects which embedded logo the badge displays.")]
        public LogoKind LogoKind
        {
            get => _logoKind;
            set
            {
                if (_logoKind == value)
                {
                    return;
                }

                _logoKind = value;
                ApplyLogoKind();
            }
        }

        public LogoBadge() : this(LogoKind.Tcgc)
        {
        }

        protected LogoBadge(LogoKind logoKind)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(270, 270);
            _logoKind = logoKind;
            _logo = LoadEmbeddedLogo(GetFileName(logoKind));
        }

        private static string GetFileName(LogoKind logoKind)
        {
            return logoKind == LogoKind.Lrc ? "lrc_logo.png" : "tcgc_logo.png";
        }

        private void ApplyLogoKind()
        {
            Image? image = LoadEmbeddedLogo(GetFileName(_logoKind));
            if (image is null)
            {
                return;
            }

            _logo?.Dispose();
            _logo = image;
            Invalidate();
        }

        public static Image? LoadEmbeddedLogo(string fileName)
        {
            System.Reflection.Assembly assembly = typeof(LogoBadge).Assembly;
            string? resourceName = Array.Find(
                assembly.GetManifestResourceNames(),
                name => name.EndsWith("logos." + fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                return null;
            }

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return null;
            }

            using var source = Image.FromStream(stream);
            return new Bitmap(source);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(new RectangleF(-6f, -6f, Width + 12f, Height + 12f));
                using var glow = new PathGradientBrush(glowPath)
                {
                    CenterColor = Color.FromArgb(30, 255, 255, 255),
                    SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) }
                };
                g.FillPath(glow, glowPath);
            }

            if (_logo is null || Width <= 0 || Height <= 0)
            {
                return;
            }

            float scale = Math.Min((float)Width / _logo.Width, (float)Height / _logo.Height);
            int width = Math.Max(1, (int)(_logo.Width * scale));
            int height = Math.Max(1, (int)(_logo.Height * scale));

            g.DrawImage(_logo, (Width - width) / 2, (Height - height) / 2, width, height);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logo?.Dispose();
                _logo = null;
            }

            base.Dispose(disposing);
        }
    }

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TcgcLogoBadge), "TcgcLogoBadge.bmp")]
    [Description("Displays the TCGC logo with a soft glow.")]
    public sealed class TcgcLogoBadge : LogoBadge
    {
        public TcgcLogoBadge() : base(LogoKind.Tcgc)
        {
        }
    }

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(LrcLogoBadge), "LrcLogoBadge.bmp")]
    [Description("Displays the LRC logo with a soft glow.")]
    public sealed class LrcLogoBadge : LogoBadge
    {
        public LrcLogoBadge() : base(LogoKind.Lrc)
        {
        }
    }

    /// <summary>
    /// Small icon drawn inside a <see cref="FieldPanel"/>. The glyph comes from
    /// the Segoe MDL2 Assets font that Windows already ships with.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Small Segoe MDL2 Assets icon drawn inside a rounded field.")]
    public sealed class FieldIcon : Control
    {
        private string _glyph = string.Empty;
        private Color _iconColor = Color.FromArgb(100, 116, 139);

        public FieldIcon()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);

            Font = new Font("Segoe MDL2 Assets", 11F);
            TabStop = false;
            Size = new Size(24, 24);
            Cursor = Cursors.Default;
        }

        /// <summary>The Segoe MDL2 Assets character, e.g. \uE77B for the user icon.</summary>
        public string Glyph
        {
            get { return _glyph; }
            set
            {
                _glyph = value ?? string.Empty;
                Invalidate();
            }
        }

        public Color IconColor
        {
            get { return _iconColor; }
            set
            {
                _iconColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(
                e.Graphics,
                _glyph,
                Font,
                ClientRectangle,
                _iconColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }
    }

    public sealed class FieldPanel : Panel
    {
        private bool _focused;
        private bool _hasError;
        private FieldIcon? _icon;

        public Color FillColor { get; set; } = Color.FromArgb(243, 244, 246);
        public Color BorderColor { get; set; } = Color.FromArgb(229, 231, 235);
        public Color FocusColor { get; set; } = Color.FromArgb(22, 163, 74);
        public int CornerRadius { get; set; } = 10;

        /// <summary>The icon attached to this field, or null when it has none.</summary>
        public FieldIcon? Icon
        {
            get { return _icon; }
        }

        // An invalid field keeps its red border even while the cursor sits in it,
        // so the inline warning and the field always agree.
        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    Invalidate();
                }
            }
        }

        public bool IsFocused
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    Invalidate();
                }
            }
        }

        public FieldPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        /// <summary>
        /// Puts a Segoe MDL2 Assets glyph at the left of the rounded box and moves
        /// the text box / dropdown beside it. The eye button of a password field
        /// keeps its place on the right.
        /// </summary>
        public void AttachIcon(string glyph)
        {
            if (_icon != null || string.IsNullOrEmpty(glyph))
            {
                return;
            }

            _icon = new FieldIcon
            {
                Glyph = glyph,
                IconColor = Color.FromArgb(100, 116, 139),
                BackColor = FillColor,
                Location = new Point(14, Math.Max(0, (Height - 24) / 2)),
                Size = new Size(24, 24),
                TabStop = false
            };

            Controls.Add(_icon);

            Control? input = null;

            foreach (Control child in Controls)
            {
                if (child is TextBox || child is ComboBox)
                {
                    input = child;
                    break;
                }
            }

            if (input == null)
            {
                return;
            }

            _icon.Click += (s, e) => input.Focus();

            int left = input is ComboBox ? 40 : 44;
            int rightLimit = Width - 16;

            foreach (Control child in Controls)
            {
                if (child is PasswordToggle toggle)
                {
                    rightLimit = Math.Min(rightLimit, toggle.Left - 12);
                }
            }

            input.Left = left;
            input.Width = Math.Max(40, rightLimit - left);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = DrawHelper.CreateRoundedPath(rect, CornerRadius);

            using (var fill = new SolidBrush(FillColor))
            {
                g.FillPath(fill, path);
            }

            using var border = new Pen(_focused && !_hasError ? FocusColor : BorderColor, _focused && !_hasError ? 1.8f : 1f);
            g.DrawPath(border, path);
        }
    }

    public sealed class RoundedButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public int CornerRadius { get; set; } = 12;
        public Color NormalColor { get; set; } = Color.FromArgb(22, 163, 74);
        public Color HoverColor { get; set; } = Color.FromArgb(21, 128, 61);
        public Color PressedColor { get; set; } = Color.FromArgb(20, 83, 45);

        // Text colour used when the button is disabled. Kept as a property so a
        // light button (such as the copy stepper) stays readable when it greys out.
        public Color DisabledTextColor { get; set; } = Color.FromArgb(248, 250, 252);

        // Optional glyph drawn before the text (Segoe Fluent Icons / MDL2 Assets).
        // The fallback glyph is used on Windows 10, where Segoe Fluent Icons is
        // not installed and the primary glyph may be missing.
        public string IconGlyph { get; set; } = string.Empty;
        public string IconFallbackGlyph { get; set; } = string.Empty;
        public float IconSize { get; set; } = 16f;

        private static bool _iconFontResolved;
        private static bool _fluentIconsAvailable;
        private static string _iconFontFamily = "Segoe MDL2 Assets";

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.White);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = DrawHelper.CreateRoundedPath(rect, CornerRadius);

            Color fill = !Enabled
                ? Color.FromArgb(203, 213, 225)
                : _pressed ? PressedColor : _hover ? HoverColor : NormalColor;

            Color top = Blend(fill, Color.White, !Enabled ? 0.06f : _pressed ? 0.03f : 0.12f);
            Color bottom = Blend(fill, Color.Black, 0.10f);

            using (var brush = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            var inner = new Rectangle(rect.X + 1, rect.Y + 1, Math.Max(0, rect.Width - 2), Math.Max(0, rect.Height - 2));
            using var innerPath = DrawHelper.CreateRoundedPath(inner, Math.Max(1, CornerRadius - 1));
            using var sheen = new Pen(Color.FromArgb(46, 255, 255, 255), 1f);
            g.DrawPath(sheen, innerPath);

            if (string.IsNullOrEmpty(IconGlyph))
            {
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    rect,
                    Enabled ? ForeColor : DisabledTextColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis);
            }
            else
            {
                PaintIcon(g);
            }
        }

        private void PaintIcon(Graphics g)
        {
            ResolveIconFont();

            string glyph = _fluentIconsAvailable || string.IsNullOrEmpty(IconFallbackGlyph)
                ? IconGlyph
                : IconFallbackGlyph;

            using Font iconFont = new Font(_iconFontFamily, IconSize, FontStyle.Regular, GraphicsUnit.Pixel);

            Size iconSize = TextRenderer.MeasureText(g, glyph, iconFont, Size.Empty, TextFormatFlags.NoPadding);
            Size textSize = TextRenderer.MeasureText(g, Text, Font, Size.Empty, TextFormatFlags.NoPadding);

            int gap = 8;
            int totalWidth = iconSize.Width + gap + textSize.Width;
            int left = Math.Max(6, (Width - totalWidth) / 2);

            Color color = Enabled ? ForeColor : DisabledTextColor;

            TextRenderer.DrawText(
                g,
                glyph,
                iconFont,
                new Rectangle(left, 0, iconSize.Width, Height),
                color,
                TextFormatFlags.NoPadding |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.Left);

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(left + iconSize.Width + gap, 0, Math.Max(0, Width - left - iconSize.Width - gap - 6), Height),
                color,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        private static void ResolveIconFont()
        {
            if (_iconFontResolved)
            {
                return;
            }

            _iconFontResolved = true;

            try
            {
                using InstalledFontCollection fonts = new InstalledFontCollection();

                foreach (FontFamily family in fonts.Families)
                {
                    if (string.Equals(family.Name, "Segoe Fluent Icons", StringComparison.OrdinalIgnoreCase))
                    {
                        _fluentIconsAvailable = true;
                        _iconFontFamily = "Segoe Fluent Icons";
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // Fall back to the MDL2 Assets font that ships with Windows 10.
            }
        }

        private static Color Blend(Color from, Color to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            return Color.FromArgb(
                (int)Math.Round(from.A + (to.A - from.A) * amount),
                (int)Math.Round(from.R + (to.R - from.R) * amount),
                (int)Math.Round(from.G + (to.G - from.G) * amount),
                (int)Math.Round(from.B + (to.B - from.B) * amount));
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

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }
    }
}
