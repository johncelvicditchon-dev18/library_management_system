using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Translucent close chip drawn on top of the dialog header. Public so the
    /// Windows Forms designer can create it inside each dialog designer file.
    /// </summary>
    public sealed class DialogCloseButton : Control
    {
        private bool _hover;
        private bool _onLightBackground;

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

        /// <summary>
        /// The compact header paints a white band, so the close chip switches to
        /// dark ink instead of the white ink used over the green header.
        /// </summary>
        public bool OnLightBackground
        {
            get { return _onLightBackground; }
            set
            {
                if (_onLightBackground == value)
                {
                    return;
                }

                _onLightBackground = value;
                Invalidate();
            }
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
                Color fillColor = _onLightBackground
                    ? (_hover ? Color.FromArgb(38, 15, 23, 42) : Color.FromArgb(16, 15, 23, 42))
                    : (_hover ? Color.FromArgb(64, 255, 255, 255) : Color.FromArgb(30, 255, 255, 255));

                using (SolidBrush fill = new SolidBrush(fillColor))
                {
                    g.FillPath(fill, path);
                }

                Color borderColor = _onLightBackground
                    ? Color.FromArgb(46, 15, 23, 42)
                    : Color.FromArgb(60, 255, 255, 255);

                using Pen border = new Pen(borderColor, 1f);
                g.DrawPath(border, path);
            }

            TextRenderer.DrawText(
                g,
                "✕",
                Font,
                bounds,
                _onLightBackground ? Color.FromArgb(51, 65, 85) : Color.White,
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

    /// <summary>
    /// Shared chrome (green header, rounded white body, close button and footer
    /// buttons) used by every library pop up. The dialogs keep this behaviour here
    /// instead of in a base form so each dialog stays a plain, editable Form.
    /// </summary>
    internal static class DialogChrome
    {
        public const int HeaderHeight = 104;
        public const int FooterHeight = 78;
        public const int CornerRadius = 20;
        public const int SideMargin = 32;

        // Compact chrome used by the dialogs that match REQUEST TO BORROW: a small
        // green gradient strip at the very top and a white header underneath.
        public const int AccentHeight = 8;

        public static void EnablePainting(Form form)
        {
            ControlStyles styles = ControlStyles.UserPaint |
                                   ControlStyles.AllPaintingInWmPaint |
                                   ControlStyles.OptimizedDoubleBuffer |
                                   ControlStyles.ResizeRedraw;

            // SetStyle is protected on Control, so reach it through reflection from
            // this shared helper instead of duplicating it in every dialog.
            System.Reflection.MethodInfo? setStyle = typeof(Control).GetMethod(
                "SetStyle",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            setStyle?.Invoke(form, new object[] { styles, true });

            // Every borderless pop up that uses this chrome also gets the soft,
            // even shadow. It is attached here so each dialog inherits it.
            DialogShadow.Attach(form);
        }

        /// <summary>
        /// Applies the runtime title, subtitle and size to the chrome that the
        /// designer file created with its default geometry.
        /// </summary>
        public static void ApplyParameters(Form form, string title, string? subtitle, int width, int height,
            Panel contentPanel, Panel footerDivider, Label titleLabel, Label? subtitleLabel,
            Control closeButton, RoundedButton primary, RoundedButton secondary)
        {
            form.ClientSize = new Size(width, height);
            form.Text = title;

            contentPanel.Location = new Point(0, HeaderHeight);
            contentPanel.Size = new Size(width, Math.Max(1, height - HeaderHeight - FooterHeight));

            footerDivider.Location = new Point(0, height - FooterHeight);
            footerDivider.Size = new Size(width, 1);

            titleLabel.Location = new Point(SideMargin, 22);
            titleLabel.Size = new Size(width - 130, 32);
            titleLabel.Text = (title ?? string.Empty).ToUpperInvariant();

            // Pop ups that keep only a heading pass null here.
            if (subtitleLabel != null)
            {
                subtitleLabel.Location = new Point(SideMargin + 2, 60);
                subtitleLabel.Size = new Size(width - 140, 22);
                subtitleLabel.Text = subtitle ?? string.Empty;
            }

            closeButton.Location = new Point(width - SideMargin - 34, 20);

            secondary.Location = new Point(width - SideMargin - 168 - 12 - 120, height - FooterHeight + 16);
            primary.Location = new Point(width - SideMargin - 168, height - FooterHeight + 16);
        }

        /// <summary>
        /// Switches the title, subtitle and close chip to the colours that read on
        /// the compact white header.
        /// </summary>
        public static void ApplyCompactHeader(
            Form form,
            Label titleLabel,
            Label? subtitleLabel,
            DialogCloseButton closeButton)
        {
            titleLabel.ForeColor = Color.FromArgb(15, 23, 42);

            if (subtitleLabel != null)
            {
                subtitleLabel.ForeColor = Color.FromArgb(100, 116, 139);
            }

            closeButton.OnLightBackground = true;
        }

        public static void ApplyRoundedRegion(Form form)
        {
            if (form.Width <= 0 || form.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = DrawHelper.CreateRoundedPath(new Rectangle(0, 0, form.Width, form.Height), CornerRadius);
            Region? previous = form.Region;
            form.Region = new Region(path);
            previous?.Dispose();
        }

        public static void PaintBackground(Form form, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (form.Width <= 2 || form.Height <= 2)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, form.Width, form.Height);
            using (GraphicsPath card = DrawHelper.CreateRoundedPath(bounds, CornerRadius))
            {
                using (SolidBrush body = new SolidBrush(Color.White))
                {
                    g.FillPath(body, card);
                }

                GraphicsState state = g.Save();
                g.SetClip(new Rectangle(0, 0, form.Width, HeaderHeight), CombineMode.Intersect);

                using (LinearGradientBrush header = new LinearGradientBrush(
                           new Rectangle(0, 0, form.Width, HeaderHeight),
                           Color.FromArgb(6, 78, 59),
                           Color.FromArgb(22, 163, 74),
                           LinearGradientMode.Horizontal))
                {
                    g.FillPath(header, card);
                }

                using (SolidBrush glow = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    g.FillEllipse(glow, form.Width - 250, -120, 320, 320);
                }

                g.Restore(state);

                using (Pen border = new Pen(Color.FromArgb(203, 213, 225), 1.2f))
                {
                    g.DrawPath(border, card);
                }
            }
        }

        /// <summary>
        /// Compact chrome that matches the REQUEST TO BORROW dialog: a white card
        /// with a thin green gradient strip across the top instead of a tall green
        /// header band.
        /// </summary>
        public static void PaintCompactBackground(Form form, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (form.Width <= 2 || form.Height <= 2)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, form.Width, form.Height);
            using (GraphicsPath card = DrawHelper.CreateRoundedPath(bounds, CornerRadius))
            {
                using (SolidBrush body = new SolidBrush(Color.White))
                {
                    g.FillPath(body, card);
                }

                GraphicsState state = g.Save();
                g.SetClip(new Rectangle(0, 0, form.Width, AccentHeight), CombineMode.Intersect);

                using (LinearGradientBrush accent = new LinearGradientBrush(
                           new Rectangle(0, 0, form.Width, AccentHeight),
                           Color.FromArgb(74, 222, 128),
                           Color.FromArgb(5, 150, 105),
                           LinearGradientMode.Horizontal))
                {
                    g.FillPath(accent, card);
                }

                g.Restore(state);

                using (Pen border = new Pen(Color.FromArgb(203, 213, 225), 1.2f))
                {
                    g.DrawPath(border, card);
                }
            }
        }

        public static void ShowAlert(string? message, string? caption, MessageBoxIcon icon)
        {
            MessageBox.Show(
                message ?? string.Empty,
                caption ?? string.Empty,
                MessageBoxButtons.OK,
                icon);
        }

        /// <summary>Drag-to-move helper shared by the dialog forms.</summary>
        public sealed class DragMover
        {
            private bool _dragging;
            private Point _dragStart;
            private Point _formStart;

            public void Start(Form form, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                _dragging = true;
                _dragStart = Cursor.Position;
                _formStart = form.Location;
            }

            public void Move(Form form, MouseEventArgs e)
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
                form.Location = new Point(
                    _formStart.X + (now.X - _dragStart.X),
                    _formStart.Y + (now.Y - _dragStart.Y));
            }

            public void End()
            {
                _dragging = false;
            }
        }
    }
}
