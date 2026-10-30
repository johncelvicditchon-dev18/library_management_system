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
    public class LibraryDialog : Form
    {
        protected const int HeaderHeight = 104;
        protected const int FooterHeight = 78;
        private const int CornerRadius = 20;
        private const int SideMargin = 32;

        private readonly Label _titleLabel;
        private readonly Label _subtitleLabel;
        private readonly DialogCloseButton _closeButton;
        private readonly Panel _footerDivider;
        private readonly RoundedButton _primaryButton;
        private readonly RoundedButton _secondaryButton;
        private readonly Panel _contentPanel;

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

        public LibraryDialog(string title, string subtitle, int width, int height)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(width, height);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = title;

            _contentPanel = new Panel();
            _contentPanel.BackColor = Color.White;
            _contentPanel.Location = new Point(0, HeaderHeight);
            _contentPanel.Size = new Size(width, Math.Max(1, height - HeaderHeight - FooterHeight));
            _contentPanel.TabIndex = 3;

            _footerDivider = new Panel();
            _footerDivider.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, height - FooterHeight);
            _footerDivider.Size = new Size(width, 1);
            _footerDivider.TabIndex = 4;

            _titleLabel = new Label();
            _titleLabel.AutoSize = false;
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.White;
            _titleLabel.Location = new Point(SideMargin, 22);
            _titleLabel.Size = new Size(width - 130, 32);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = (title ?? string.Empty).ToUpperInvariant();
            _titleLabel.MouseDown += DragStart;
            _titleLabel.MouseMove += DragMove;
            _titleLabel.MouseUp += DragEnd;

            _subtitleLabel = new Label();
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.BackColor = Color.Transparent;
            _subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            _subtitleLabel.ForeColor = Color.FromArgb(209, 250, 229);
            _subtitleLabel.Location = new Point(SideMargin + 2, 60);
            _subtitleLabel.Size = new Size(width - 140, 22);
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = subtitle ?? string.Empty;
            _subtitleLabel.MouseDown += DragStart;
            _subtitleLabel.MouseMove += DragMove;
            _subtitleLabel.MouseUp += DragEnd;

            _closeButton = new DialogCloseButton();
            _closeButton.Location = new Point(width - SideMargin - 34, 20);
            _closeButton.Size = new Size(34, 34);
            _closeButton.TabIndex = 2;
            _closeButton.Click += CloseButton_Click;

            _secondaryButton = new RoundedButton();
            _secondaryButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _secondaryButton.CornerRadius = 12;
            _secondaryButton.DialogResult = DialogResult.Cancel;
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.Location = new Point(width - SideMargin - 168 - 12 - 120, height - FooterHeight + 16);
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(120, 46);
            _secondaryButton.TabIndex = 6;
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;

            _primaryButton = new RoundedButton();
            _primaryButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _primaryButton.CornerRadius = 12;
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.Location = new Point(width - SideMargin - 168, height - FooterHeight + 16);
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 46);
            _primaryButton.TabIndex = 5;
            _primaryButton.Text = "SAVE";
            _primaryButton.UseVisualStyleBackColor = false;

            AcceptButton = _primaryButton;
            CancelButton = _secondaryButton;

            Controls.Add(_contentPanel);
            Controls.Add(_footerDivider);
            Controls.Add(_titleLabel);
            Controls.Add(_subtitleLabel);
            Controls.Add(_closeButton);
            Controls.Add(_primaryButton);
            Controls.Add(_secondaryButton);

            MouseDown += DragStart;
            MouseMove += DragMove;
            MouseUp += DragEnd;
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
