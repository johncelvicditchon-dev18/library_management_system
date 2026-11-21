using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project
{
    /// <summary>
    /// Compact sign out control for the dashboard headers: a translucent glass pill
    /// with a door and exit arrow drawn as vectors, so the action reads at a glance
    /// without a text label taking up header space. Hovering and pressing it fill the
    /// pill with the sign out danger colour, and the icon stays centred at any size.
    /// </summary>
    public sealed class SignOutButton : Control
    {
        private readonly ToolTip _toolTip = new ToolTip();

        private bool _hover;
        private bool _pressed;

        public SignOutButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(46, 46);
            TabStop = true;
            AccessibleName = "SIGN OUT";
            AccessibleRole = AccessibleRole.PushButton;

            _toolTip.SetToolTip(this, "SIGN OUT");
        }

        // The glass surface is the default look; the two danger colours are used
        // while the pointer is on the control so the destructive action stands out.
        public int CornerRadius { get; set; } = 14;
        public Color IconColor { get; set; } = Color.White;
        public Color HoverColor { get; set; } = Color.FromArgb(194, 65, 12);
        public Color PressedColor { get; set; } = Color.FromArgb(154, 52, 18);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using GraphicsPath pill = DrawHelper.CreateRoundedPath(bounds, CornerRadius);

            // The parent gradient shows through the resting pill, so the button sits
            // in the header instead of looking like a pasted white rectangle.
            if (_pressed || _hover)
            {
                using SolidBrush fill = new SolidBrush(_pressed ? PressedColor : HoverColor);
                graphics.FillPath(fill, pill);
            }
            else
            {
                using SolidBrush glass = new SolidBrush(Color.FromArgb(28, 255, 255, 255));
                graphics.FillPath(glass, pill);
            }

            using (Pen edge = new Pen(
                Color.FromArgb(_hover || _pressed ? 110 : 70, 255, 255, 255), 1f))
            {
                graphics.DrawPath(edge, pill);
            }

            if (Focused)
            {
                Rectangle ringBounds = Rectangle.Inflate(bounds, -3, -3);

                using GraphicsPath ring = DrawHelper.CreateRoundedPath(
                    ringBounds, Math.Max(2, CornerRadius - 3));
                using Pen ringPen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.5f);

                graphics.DrawPath(ringPen, ring);
            }

            PaintSignOutIcon(graphics, bounds, _hover || _pressed ? Color.White : IconColor);
        }

        /// <summary>
        /// Paints the door and the exit arrow from a 20 x 20 design grid, scaled to
        /// the control, so the icon stays crisp at any button size.
        /// </summary>
        private void PaintSignOutIcon(Graphics graphics, Rectangle bounds, Color color)
        {
            float icon = Math.Min(bounds.Width, bounds.Height) * 0.46f;
            float left = bounds.Left + ((bounds.Width - icon) / 2f);
            float top = bounds.Top + ((bounds.Height - icon) / 2f);
            float unit = icon / 20f;

            using Pen pen = new Pen(color, Math.Max(1.5f, 2f * unit))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using GraphicsPath door = new GraphicsPath();
            door.AddLine(left + (7f * unit), top + (3f * unit), left + (3f * unit), top + (3f * unit));
            door.AddLine(left + (3f * unit), top + (3f * unit), left + (3f * unit), top + (17f * unit));
            door.AddLine(left + (3f * unit), top + (17f * unit), left + (7f * unit), top + (17f * unit));
            graphics.DrawPath(pen, door);

            using GraphicsPath arrow = new GraphicsPath();
            arrow.AddLine(left + (7.5f * unit), top + (10f * unit), left + (17f * unit), top + (10f * unit));
            arrow.AddLine(left + (13.2f * unit), top + (6.2f * unit), left + (17f * unit), top + (10f * unit));
            arrow.AddLine(left + (17f * unit), top + (10f * unit), left + (13.2f * unit), top + (13.8f * unit));
            graphics.DrawPath(pen, arrow);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _pressed = true;
            Focus();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!_pressed)
            {
                return;
            }

            _pressed = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }
}
