using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    // ENCAPSULATION: a small eye button that sits inside a password field.
    // Clicking it switches the linked text box between hidden characters and
    // plain text, so the show/hide logic lives in one reusable control.
    [ToolboxItem(true)]
    [Description("Eye button that shows or hides the characters of a password text box.")]
    public sealed class PasswordToggle : Control
    {
        private static readonly Color IdleColor = Color.FromArgb(107, 114, 128);
        private static readonly Color HoverColor = Color.FromArgb(22, 163, 74);
        private static readonly Color ActiveColor = Color.FromArgb(21, 128, 61);

        private readonly ToolTip _tooltip = new ToolTip();
        private TextBox? _input;
        private bool _revealed;
        private bool _hover;

        public PasswordToggle()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(243, 244, 246);
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(26, 26);
            _tooltip.InitialDelay = 400;
            _tooltip.ReshowDelay = 100;
            UpdateTooltip();
        }

        // True while the password is readable.
        [Browsable(false)]
        public bool Revealed
        {
            get { return _revealed; }
        }

        // Links the text box this button controls. The text box keeps its own
        // UseSystemPasswordChar value until the button is clicked.
        public void Attach(TextBox? input)
        {
            _input = input;
            _revealed = input != null && !input.UseSystemPasswordChar;
            UpdateTooltip();
            Invalidate();
        }

        private void UpdateTooltip()
        {
            string text = _revealed ? "Hide password" : "Show password";
            AccessibleName = text;
            _tooltip.SetToolTip(this, text);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_input == null)
            {
                return;
            }

            _revealed = !_revealed;
            _input.UseSystemPasswordChar = !_revealed;
            UpdateTooltip();
            Invalidate();

            // Give the focus back so the user can keep typing.
            if (_input.CanFocus)
            {
                _input.Focus();
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

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color color = _hover ? HoverColor : _revealed ? ActiveColor : IdleColor;
            float cx = Width / 2f;
            float cy = Height / 2f;

            // Almond shaped eye, drawn with two mirrored bezier curves.
            float halfWidth = Math.Max(6f, Math.Min(Width, Height) / 2f - 4f);
            float halfHeight = halfWidth * 0.62f;

            using (GraphicsPath eye = new GraphicsPath())
            {
                eye.StartFigure();
                eye.AddBezier(
                    cx - halfWidth, cy,
                    cx - halfWidth * 0.55f, cy - halfHeight,
                    cx + halfWidth * 0.55f, cy - halfHeight,
                    cx + halfWidth, cy);
                eye.AddBezier(
                    cx + halfWidth, cy,
                    cx + halfWidth * 0.55f, cy + halfHeight,
                    cx - halfWidth * 0.55f, cy + halfHeight,
                    cx - halfWidth, cy);
                eye.CloseFigure();

                using Pen outline = new Pen(color, 1.6f);
                g.DrawPath(outline, eye);
            }

            // Pupil.
            float pupil = Math.Max(2f, halfHeight * 0.55f);
            using (SolidBrush fill = new SolidBrush(color))
            {
                g.FillEllipse(fill, cx - pupil, cy - pupil, pupil * 2, pupil * 2);
            }

            // A hidden password gets a slash, so the state is readable at a glance.
            if (!_revealed)
            {
                using Pen slash = new Pen(color, 1.7f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                g.DrawLine(slash, cx - halfWidth, cy + halfHeight + 1f, cx + halfWidth, cy - halfHeight - 1f);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
