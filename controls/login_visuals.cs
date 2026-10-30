using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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

    public sealed class FieldPanel : Panel
    {
        private bool _focused;

        public Color FillColor { get; set; } = Color.FromArgb(243, 244, 246);
        public Color BorderColor { get; set; } = Color.FromArgb(229, 231, 235);
        public Color FocusColor { get; set; } = Color.FromArgb(22, 163, 74);
        public int CornerRadius { get; set; } = 10;

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

            using var border = new Pen(_focused ? FocusColor : BorderColor, _focused ? 1.8f : 1f);
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

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                rect,
                Enabled ? ForeColor : Color.FromArgb(248, 250, 252),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
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
