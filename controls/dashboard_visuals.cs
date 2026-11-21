using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    [ToolboxItem(true)]
    [Description("Flat dashboard navigation item that draws a gradient underline when it is selected.")]
    public sealed class NavTab : Control
    {
        private bool _selected;
        private bool _hover;

        public Color SelectedTextColor { get; set; } = Color.FromArgb(4, 120, 87);
        public Color NormalTextColor { get; set; } = Color.FromArgb(100, 116, 139);
        public Color HoverTextColor { get; set; } = Color.FromArgb(13, 148, 136);
        public Color HoverFillColor { get; set; } = Color.FromArgb(240, 253, 244);
        public Color IndicatorStartColor { get; set; } = Color.FromArgb(22, 163, 74);
        public Color IndicatorEndColor { get; set; } = Color.FromArgb(16, 185, 129);

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether this navigation item is the active section.")]
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value)
                {
                    return;
                }

                _selected = value;
                Invalidate();
            }
        }

        public NavTab()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width < 10 || Height < 10)
            {
                return;
            }

            if (_hover && !_selected)
            {
                var hover = new Rectangle(2, 6, Width - 5, Height - 15);
                using var hoverPath = DrawHelper.CreateRoundedPath(hover, 10);
                using var hoverBrush = new SolidBrush(HoverFillColor);
                g.FillPath(hoverBrush, hoverPath);
            }

            Color textColor = _selected ? SelectedTextColor : _hover ? HoverTextColor : NormalTextColor;

            var textRect = new Rectangle(0, 4, Width, Height - 12);
            TextRenderer.DrawText(
                g,
                Text ?? string.Empty,
                Font,
                textRect,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);

            if (_selected)
            {
                int barWidth = Math.Min(58, Math.Max(24, Width - 34));
                var bar = new Rectangle((Width - barWidth) / 2, Height - 7, barWidth, 3);

                using var barPath = DrawHelper.CreateRoundedPath(bar, 2);
                using var barBrush = new LinearGradientBrush(
                    bar,
                    IndicatorStartColor,
                    IndicatorEndColor,
                    LinearGradientMode.Horizontal);

                g.FillPath(barBrush, barPath);
            }
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
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }

    [ToolboxItem(true)]
    [Description("Circular avatar that shows a profile picture or the initials of the signed in person.")]
    public sealed class AvatarBadge : Control
    {
        private string _initials = "?";
        private Image? _picture;

        public Color FillTopColor { get; set; } = Color.FromArgb(34, 197, 94);
        public Color FillBottomColor { get; set; } = Color.FromArgb(4, 90, 66);
        public Color InitialsColor { get; set; } = Color.White;
        public Color RingColor { get; set; } = Color.FromArgb(70, 255, 255, 255);

        [Category("Appearance")]
        [DefaultValue("?")]
        [Description("One or two letters shown inside the avatar.")]
        public string Initials
        {
            get => _initials;
            set
            {
                string text = string.IsNullOrWhiteSpace(value) ? "?" : value.ToUpperInvariant();
                if (_initials == text)
                {
                    return;
                }

                _initials = text;
                Invalidate();
            }
        }

        /// <summary>
        /// Optional portrait. When it is set the avatar shows the picture cropped
        /// to the circle, otherwise it falls back to the initials. The image stays
        /// owned by the caller.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Picture
        {
            get => _picture;
            set
            {
                if (ReferenceEquals(_picture, value))
                {
                    return;
                }

                _picture = value;
                Invalidate();
            }
        }

        public AvatarBadge()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(104, 104);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (Width < 8 || Height < 8)
            {
                return;
            }

            int size = Math.Min(Width, Height) - 4;
            var circle = new Rectangle((Width - size) / 2, (Height - size) / 2, size, size);

            using var path = new GraphicsPath();
            path.AddEllipse(circle);

            if (_picture != null)
            {
                // Cover-crop the portrait inside the circle, so no letterbox bars
                // show around a photo with a different aspect ratio.
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float scale = Math.Max(
                    (float)circle.Width / _picture.Width,
                    (float)circle.Height / _picture.Height);

                int width = Math.Max(circle.Width, (int)Math.Ceiling(_picture.Width * scale));
                int height = Math.Max(circle.Height, (int)Math.Ceiling(_picture.Height * scale));

                Rectangle target = new Rectangle(
                    circle.Left + (circle.Width - width) / 2,
                    circle.Top + (circle.Height - height) / 2,
                    width,
                    height);

                GraphicsState state = g.Save();
                g.SetClip(path);
                g.DrawImage(_picture, target);
                g.Restore(state);
            }
            else
            {
                using (var fill = new LinearGradientBrush(circle, FillTopColor, FillBottomColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(fill, path);
                }

                using var font = new Font("Segoe UI", Math.Max(9f, size * 0.30f), FontStyle.Bold, GraphicsUnit.Pixel);
                TextRenderer.DrawText(
                    g,
                    _initials,
                    font,
                    circle,
                    InitialsColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }

            using (var ring = new Pen(RingColor, 2f))
            {
                g.DrawEllipse(ring, circle);
            }
        }
    }

    [ToolboxItem(true)]
    [Description("Rounded card that shows a short message for dashboard sections without data.")]
    public sealed class EmptyState : Control
    {
        public Color FillColor { get; set; } = Color.FromArgb(248, 250, 252);
        public Color BorderColor { get; set; } = Color.FromArgb(226, 232, 240);
        public Color TitleColor { get; set; } = Color.FromArgb(100, 116, 139);
        public Color CaptionColor { get; set; } = Color.FromArgb(148, 163, 184);
        public int CornerRadius { get; set; } = 20;

        public string Title { get; set; } = "MODULE";
        public string Caption { get; set; } = "Nothing to show yet.";

        public EmptyState()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width < 20 || Height < 20)
            {
                return;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = DrawHelper.CreateRoundedPath(rect, CornerRadius);

            using (var fill = new SolidBrush(FillColor))
            {
                g.FillPath(fill, path);
            }

            using (var border = new Pen(BorderColor, 1.4f))
            {
                g.DrawPath(border, path);
            }

            int centerY = Height / 2;

            using (var titleFont = new Font("Segoe UI", 12F, FontStyle.Bold))
            {
                var titleRect = new Rectangle(30, centerY - 44, Width - 60, 34);
                TextRenderer.DrawText(
                    g,
                    Title ?? string.Empty,
                    titleFont,
                    titleRect,
                    TitleColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            }

            using (var captionFont = new Font("Segoe UI", 9.5F))
            {
                var captionRect = new Rectangle(60, centerY - 4, Math.Max(40, Width - 120), 60);
                TextRenderer.DrawText(
                    g,
                    Caption ?? string.Empty,
                    captionFont,
                    captionRect,
                    CaptionColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top |
                    TextFormatFlags.WordBreak);
            }
        }
    }
}
