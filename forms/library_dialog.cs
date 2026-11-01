using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Base class for every dashboard pop up. It paints the green TCGC header, the
    /// rounded white body, the close button and the footer buttons, so the child
    /// dialogs only add their own fields in <see cref="ContentPanel"/>.
    /// </summary>
    public partial class LibraryDialog : Form
    {
        protected const int HeaderHeight = 104;
        protected const int FooterHeight = 78;
        private const int CornerRadius = 20;
        private const int SideMargin = 32;

        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private DialogCloseButton _closeButton = null!;
        private Panel _footerDivider = null!;
        private RoundedButton _primaryButton = null!;
        private RoundedButton _secondaryButton = null!;
        private Panel _contentPanel = null!;

        private bool _dragging;
        private Point _dragStart;
        private Point _formStart;

        protected Panel ContentPanel
        {
            get { return _contentPanel; }
        }

        protected Label TitleLabel
        {
            get { return _titleLabel; }
        }

        protected Label SubtitleLabel
        {
            get { return _subtitleLabel; }
        }

        protected RoundedButton PrimaryButton
        {
            get { return _primaryButton; }
        }

        protected RoundedButton SecondaryButton
        {
            get { return _secondaryButton; }
        }

        protected int ContentWidth
        {
            get { return Math.Max(200, ClientSize.Width - SideMargin * 2); }
        }

        /// <summary>
        /// Design time constructor. The Windows Forms designer needs a parameterless
        /// constructor on the base type so the pop ups can be rendered on the surface.
        /// </summary>
        public LibraryDialog()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            InitializeComponent();
        }

        public LibraryDialog(string title, string subtitle, int width, int height)
            : this()
        {
            ApplyDialogParameters(title, subtitle, width, height);
        }

        /// <summary>
        /// Applies the runtime title, subtitle and size to the chrome that the
        /// designer file created with its default 640 x 780 geometry.
        /// </summary>
        private void ApplyDialogParameters(string title, string subtitle, int width, int height)
        {
            ClientSize = new Size(width, height);
            Text = title;

            _contentPanel.Location = new Point(0, HeaderHeight);
            _contentPanel.Size = new Size(width, Math.Max(1, height - HeaderHeight - FooterHeight));

            _footerDivider.Location = new Point(0, height - FooterHeight);
            _footerDivider.Size = new Size(width, 1);

            _titleLabel.Location = new Point(SideMargin, 22);
            _titleLabel.Size = new Size(width - 130, 32);
            _titleLabel.Text = (title ?? string.Empty).ToUpperInvariant();

            _subtitleLabel.Location = new Point(SideMargin + 2, 60);
            _subtitleLabel.Size = new Size(width - 140, 22);
            _subtitleLabel.Text = subtitle ?? string.Empty;

            _closeButton.Location = new Point(width - SideMargin - 34, 20);

            _secondaryButton.Location = new Point(width - SideMargin - 168 - 12 - 120, height - FooterHeight + 16);
            _primaryButton.Location = new Point(width - SideMargin - 168, height - FooterHeight + 16);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedRegion();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyRoundedRegion();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return parameters;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width <= 2 || Height <= 2)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            using (GraphicsPath card = DrawHelper.CreateRoundedPath(bounds, CornerRadius))
            {
                using (SolidBrush body = new SolidBrush(Color.White))
                {
                    g.FillPath(body, card);
                }

                GraphicsState state = g.Save();
                g.SetClip(new Rectangle(0, 0, Width, HeaderHeight), CombineMode.Intersect);

                using (LinearGradientBrush header = new LinearGradientBrush(
                           new Rectangle(0, 0, Width, HeaderHeight),
                           Color.FromArgb(6, 78, 59),
                           Color.FromArgb(22, 163, 74),
                           LinearGradientMode.Horizontal))
                {
                    g.FillPath(header, card);
                }

                using (SolidBrush glow = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillEllipse(glow, Width - 250, -120, 320, 320);
                }

                g.Restore(state);

                using (Pen border = new Pen(Color.FromArgb(203, 213, 225), 1.2f))
                {
                    g.DrawPath(border, card);
                }
            }
        }

        protected static void ShowAlert(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(
                message ?? string.Empty,
                caption ?? string.Empty,
                MessageBoxButtons.OK,
                icon);
        }

        private void ApplyRoundedRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using GraphicsPath path = DrawHelper.CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
            Region? previous = Region;
            Region = new Region(path);
            previous?.Dispose();
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DragStart(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _dragging = true;
            _dragStart = Cursor.Position;
            _formStart = Location;
        }

        private void DragMove(object? sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            if (Control.MouseButtons != MouseButtons.Left)
            {
                _dragging = false;
                return;
            }

            Point now = Cursor.Position;
            Location = new Point(
                _formStart.X + (now.X - _dragStart.X),
                _formStart.Y + (now.Y - _dragStart.Y));
        }

        private void DragEnd(object? sender, MouseEventArgs e)
        {
            _dragging = false;
        }
    }

    /// <summary>Translucent close chip drawn on top of the dialog header.</summary>
    internal sealed class DialogCloseButton : Control
    {
        private bool _hover;

        public DialogCloseButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width < 4 || Height < 4)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = DrawHelper.CreateRoundedPath(bounds, Math.Min(10, Height / 2)))
            {
                using (SolidBrush fill = new SolidBrush(_hover
                           ? Color.FromArgb(64, 255, 255, 255)
                           : Color.FromArgb(30, 255, 255, 255)))
                {
                    g.FillPath(fill, path);
                }

                using Pen border = new Pen(Color.FromArgb(60, 255, 255, 255), 1f);
                g.DrawPath(border, path);
            }

            TextRenderer.DrawText(
                g,
                "✕",
                Font,
                bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
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
    }
}
