using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>
    /// One number on the 5 to 1 rating scale, painted as a rounded pill instead of
    /// a stock radio button. It still derives from <see cref="RadioButton"/>, so the
    /// survey code keeps reading <see cref="RadioButton.Checked"/> and the arrow
    /// keys still move the selection inside a row, but the little circle is
    /// replaced by a polished pill: white with a light border when untouched, a
    /// pale green hover state, and a green gradient pill with a soft outer ring
    /// once the respondent picks it.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Rounded 5 to 1 rating pill: white when untouched, green gradient when picked.")]
    public sealed class RatingPill : RadioButton
    {
        // Space kept around the pill so the selected ring has room to be drawn
        // without being clipped by the control bounds.
        private const int RingInset = 2;

        private int _cornerRadius = 11;
        private bool _hover;

        private Color _idleFillColor = Color.White;
        private Color _idleBorderColor = Color.FromArgb(226, 232, 240);
        private Color _idleTextColor = Color.FromArgb(71, 85, 105);

        private Color _hoverFillColor = Color.FromArgb(240, 253, 244);
        private Color _hoverBorderColor = Color.FromArgb(110, 231, 183);
        private Color _hoverTextColor = Color.FromArgb(4, 120, 87);

        private Color _selectedStartColor = Color.FromArgb(34, 197, 94);
        private Color _selectedEndColor = Color.FromArgb(21, 128, 61);
        private Color _selectedEdgeColor = Color.FromArgb(22, 101, 52);
        private Color _selectedTextColor = Color.White;
        private Color _ringColor = Color.FromArgb(48, 22, 163, 74);

        private Color _disabledFillColor = Color.FromArgb(241, 245, 249);
        private Color _disabledTextColor = Color.FromArgb(148, 163, 184);

        public RatingPill()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            AutoSize = false;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            Size = new Size(42, 42);
            TabStop = true;
            TextAlign = ContentAlignment.MiddleCenter;
        }

        [Category("Appearance")]
        [DefaultValue(11)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = Math.Max(1, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color IdleFillColor
        {
            get => _idleFillColor;
            set { _idleFillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color IdleBorderColor
        {
            get => _idleBorderColor;
            set { _idleBorderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color IdleTextColor
        {
            get => _idleTextColor;
            set { _idleTextColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color HoverFillColor
        {
            get => _hoverFillColor;
            set { _hoverFillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color HoverBorderColor
        {
            get => _hoverBorderColor;
            set { _hoverBorderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color HoverTextColor
        {
            get => _hoverTextColor;
            set { _hoverTextColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color SelectedStartColor
        {
            get => _selectedStartColor;
            set { _selectedStartColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color SelectedEndColor
        {
            get => _selectedEndColor;
            set { _selectedEndColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color SelectedEdgeColor
        {
            get => _selectedEdgeColor;
            set { _selectedEdgeColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color SelectedTextColor
        {
            get => _selectedTextColor;
            set { _selectedTextColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Soft halo drawn around the pill that currently holds the answer.")]
        public Color RingColor
        {
            get => _ringColor;
            set { _ringColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color DisabledFillColor
        {
            get => _disabledFillColor;
            set { _disabledFillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color DisabledTextColor
        {
            get => _disabledTextColor;
            set { _disabledTextColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.White);

            if (Width < 12 || Height < 12)
            {
                return;
            }

            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);
            Rectangle pill = Rectangle.Inflate(outer, -RingInset, -RingInset);

            bool picked = Checked && Enabled;
            bool hovered = _hover && Enabled && !Checked;

            // Soft green halo around the answer that is currently selected.
            if (picked)
            {
                using GraphicsPath ringPath = DrawHelper.CreateRoundedPath(outer, _cornerRadius + RingInset);
                using Pen ringPen = new Pen(_ringColor, 2f);
                g.DrawPath(ringPen, ringPath);
            }

            using (GraphicsPath path = DrawHelper.CreateRoundedPath(pill, _cornerRadius))
            {
                if (picked)
                {
                    using LinearGradientBrush fill = new LinearGradientBrush(
                        pill, _selectedStartColor, _selectedEndColor, LinearGradientMode.Vertical);
                    g.FillPath(fill, path);
                }
                else
                {
                    Color fillColor = !Enabled
                        ? _disabledFillColor
                        : hovered ? _hoverFillColor : _idleFillColor;

                    using SolidBrush fill = new SolidBrush(fillColor);
                    g.FillPath(fill, path);
                }

                Color borderColor = !Enabled
                    ? _idleBorderColor
                    : picked ? _selectedEdgeColor : hovered ? _hoverBorderColor : _idleBorderColor;

                using (Pen border = new Pen(borderColor, picked ? 1.2f : 1.1f))
                {
                    g.DrawPath(border, path);
                }

                // A one pixel sheen along the top edge makes the picked pill read
                // as a raised, glossy control instead of a flat block of colour.
                if (picked)
                {
                    Rectangle sheen = Rectangle.Inflate(pill, -1, -1);
                    using GraphicsPath sheenPath = DrawHelper.CreateRoundedPath(
                        sheen, Math.Max(1, _cornerRadius - 1));
                    using Pen sheenPen = new Pen(Color.FromArgb(56, 255, 255, 255), 1f);
                    g.DrawPath(sheenPen, sheenPath);
                }
            }

            Color textColor = !Enabled
                ? _disabledTextColor
                : picked ? _selectedTextColor : hovered ? _hoverTextColor : _idleTextColor;

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                pill,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);

            // Keyboard users still get a classic focus rectangle, drawn only when
            // the focus came from the keyboard so a mouse click stays clean.
            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(
                    g,
                    Rectangle.Inflate(pill, -4, -4),
                    textColor,
                    picked ? _selectedEndColor : _idleFillColor);
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

        protected override void OnCheckedChanged(EventArgs e)
        {
            Invalidate();
            base.OnCheckedChanged(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            Invalidate();
            base.OnTextChanged(e);
        }
    }
}
