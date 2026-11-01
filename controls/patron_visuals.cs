using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project
{
    /// <summary>
    /// Shared painting helpers for the patron portal cards. Every card is a
    /// rounded white sheet on the light page background, the same way the
    /// library catalog reference lays the covers out in a grid.
    /// </summary>
    internal static class CardPaint
    {
        public const int Radius = 18;

        public static void DrawSurface(
            Graphics graphics,
            Size size,
            Color parentColor,
            Color fill,
            Color border,
            int radius = Radius)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(parentColor);

            if (size.Width < 4 || size.Height < 4)
            {
                return;
            }

            Rectangle bounds = new Rectangle(1, 1, size.Width - 3, size.Height - 3);
            using GraphicsPath path = DrawHelper.CreateRoundedPath(bounds, radius);

            using (SolidBrush brush = new SolidBrush(fill))
            {
                graphics.FillPath(brush, path);
            }

            using Pen pen = new Pen(border, 1.2f);
            graphics.DrawPath(pen, path);
        }

        public static void DrawStatusAccent(Graphics graphics, Size size, Color color)
        {
            if (size.Height < 24)
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bar = new Rectangle(0, 10, 6, size.Height - 21);

            using GraphicsPath path = DrawHelper.CreateRoundedPath(bar, 3);
            using SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, path);
        }
    }

    /// <summary>
    /// Green banner across the top of the patron portal. The gradient runs from
    /// the dark institutional green on the left to the bright green on the right,
    /// like the reference screen, with two very soft highlights.
    /// </summary>
    public sealed class PatronHeaderPanel : Panel
    {
        public Color StartColor { get; set; } = Color.FromArgb(5, 84, 63);
        public Color EndColor { get; set; } = Color.FromArgb(22, 163, 74);

        public PatronHeaderPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = StartColor;
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

            using (LinearGradientBrush brush = new LinearGradientBrush(
                       rect, StartColor, EndColor, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, rect);
            }

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
            {
                g.FillEllipse(glow, rect.Width * 0.55f, -rect.Height * 0.9f, rect.Width * 0.55f, rect.Width * 0.55f);
            }

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
            {
                g.FillEllipse(glow, -rect.Width * 0.12f, rect.Height * 0.4f, rect.Width * 0.38f, rect.Width * 0.38f);
            }
        }
    }

    /// <summary>
    /// Book cover drawn inside a rounded frame. The thumbnail is rebuilt only
    /// when the box or the stored bytes change, so scrolling stays smooth.
    /// </summary>
    public sealed class CoverView : Control
    {
        private byte[]? _bytes;
        private Image? _bitmap;
        private Size _bitmapSize;

        public byte[]? CoverBytes
        {
            get => _bytes;
            set
            {
                if (ReferenceEquals(_bytes, value))
                {
                    return;
                }

                _bytes = value;
                DropBitmap();
                Invalidate();
            }
        }

        public Color BorderColor { get; set; } = Color.FromArgb(226, 232, 240);

        public CoverView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.White;
            TabStop = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            DropBitmap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width < 4 || bounds.Height < 4)
            {
                return;
            }

            using GraphicsPath path = DrawHelper.CreateRoundedPath(bounds, 10);

            using (SolidBrush fill = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.FillPath(fill, path);
            }

            Image image = EnsureBitmap();
            GraphicsState state = g.Save();
            g.SetClip(path);
            g.DrawImage(image, new Rectangle(1, 1, bounds.Width - 1, bounds.Height - 1));
            g.Restore(state);

            using Pen pen = new Pen(BorderColor, 1.2f);
            g.DrawPath(pen, path);
        }

        private Image EnsureBitmap()
        {
            if (_bitmap == null || _bitmapSize != ClientSize)
            {
                DropBitmap();
                _bitmap = BookImage.CreateThumbnail(_bytes, Math.Max(1, Width - 2), Math.Max(1, Height - 2), true);
                _bitmapSize = ClientSize;
            }

            return _bitmap;
        }

        private void DropBitmap()
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSize = Size.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DropBitmap();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// One catalog card: cover on the left, title / author / published / copies
    /// on the right, then the green REQUEST TO BORROW button and the neutral
    /// BOOK DESCRIPTION button underneath, exactly like the reference layout.
    /// </summary>
    public sealed class BookCard : Panel
    {
        public event EventHandler? RequestClicked;
        public event EventHandler? DescriptionClicked;

        private readonly CoverView _cover = new CoverView();
        private readonly Label _title = new Label();
        private readonly Label _author = new Label();
        private readonly Label _published = new Label();
        private readonly Label _copies = new Label();
        private readonly Label _availability = new Label();
        private readonly RoundedButton _request = new RoundedButton();
        private readonly RoundedButton _description = new RoundedButton();

        private Book? _book;
        private bool _pending;
        private bool _onLoan;
        private int _available;

        public Book? Book => _book;

        public BookCard()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Margin = new Padding(0, 0, 16, 16);
            Height = 258;
            Padding = new Padding(18);

            _cover.BackColor = Color.White;

            _title.AutoEllipsis = false;
            _title.BackColor = Color.White;
            _title.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            _title.ForeColor = Color.FromArgb(15, 23, 42);
            _title.TextAlign = ContentAlignment.TopLeft;

            _author.AutoEllipsis = true;
            _author.BackColor = Color.White;
            _author.Font = new Font("Segoe UI", 8.75F);
            _author.ForeColor = Color.FromArgb(71, 85, 105);

            _published.AutoEllipsis = true;
            _published.BackColor = Color.White;
            _published.Font = new Font("Segoe UI", 8.5F);
            _published.ForeColor = Color.FromArgb(100, 116, 139);

            _copies.AutoEllipsis = true;
            _copies.BackColor = Color.White;
            _copies.Font = new Font("Segoe UI", 8.5F);
            _copies.ForeColor = Color.FromArgb(100, 116, 139);

            _availability.AutoEllipsis = true;
            _availability.BackColor = Color.White;
            _availability.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _availability.ForeColor = Color.FromArgb(4, 120, 87);

            _request.CornerRadius = 10;
            _request.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _request.NormalColor = Color.FromArgb(22, 163, 74);
            _request.HoverColor = Color.FromArgb(21, 128, 61);
            _request.PressedColor = Color.FromArgb(20, 83, 45);
            _request.Text = "REQUEST TO BORROW";
            _request.Click += delegate { RequestClicked?.Invoke(this, EventArgs.Empty); };

            _description.CornerRadius = 10;
            _description.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _description.ForeColor = Color.FromArgb(30, 41, 59);
            _description.NormalColor = Color.FromArgb(226, 232, 240);
            _description.HoverColor = Color.FromArgb(203, 213, 225);
            _description.PressedColor = Color.FromArgb(148, 163, 184);
            _description.Text = "BOOK DESCRIPTION";
            _description.Click += delegate { DescriptionClicked?.Invoke(this, EventArgs.Empty); };

            Controls.Add(_cover);
            Controls.Add(_title);
            Controls.Add(_author);
            Controls.Add(_published);
            Controls.Add(_copies);
            Controls.Add(_availability);
            Controls.Add(_request);
            Controls.Add(_description);
        }

        public void Bind(Book book, int available, bool pending, bool onLoan)
        {
            _book = book;
            _available = available;
            _pending = pending;
            _onLoan = onLoan;

            _cover.CoverBytes = book.CoverImage;
            _title.Text = book.Title.ToUpperInvariant();
            _author.Text = "AUTHOR: " + book.Author.ToUpperInvariant();
            _published.Text = "PUBLISHED: " + book.YearText;
            _copies.Text = "COPIES: " + book.Quantity.ToString(CultureInfo.InvariantCulture) +
                           "     AVAILABLE: " + available.ToString(CultureInfo.InvariantCulture);

            if (pending)
            {
                _availability.Text = "●  REQUEST PENDING";
                _availability.ForeColor = Color.FromArgb(180, 83, 9);
                _request.Text = "REQUEST PENDING";
                _request.Enabled = false;
            }
            else if (onLoan)
            {
                _availability.Text = "●  ALREADY ON LOAN";
                _availability.ForeColor = Color.FromArgb(180, 83, 9);
                _request.Text = "ALREADY BORROWED";
                _request.Enabled = false;
            }
            else if (available <= 0)
            {
                _availability.Text = "●  NO COPY AVAILABLE";
                _availability.ForeColor = Color.FromArgb(185, 28, 28);
                _request.Text = "NO COPY AVAILABLE";
                _request.Enabled = false;
            }
            else
            {
                _availability.Text = "●  AVAILABLE — READY TO REQUEST";
                _availability.ForeColor = Color.FromArgb(4, 120, 87);
                _request.Text = "REQUEST TO BORROW";
                _request.Enabled = true;
            }

            LayoutContent();
            Invalidate();
        }

        public void UpdateState(int available, bool pending, bool onLoan)
        {
            if (_book == null)
            {
                return;
            }

            Bind(_book, available, pending, onLoan);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            const int padding = 18;
            const int gap = 16;

            int innerWidth = Math.Max(120, Width - padding * 2);
            int coverWidth = innerWidth >= 470 ? 136 : 112;
            int coverHeight = (int)Math.Round(coverWidth * 1.45);
            coverHeight = Math.Min(coverHeight, Height - padding * 2);

            _cover.SetBounds(padding, padding, coverWidth, coverHeight);

            int infoLeft = padding + coverWidth + gap;
            int infoWidth = Math.Max(90, Width - padding - infoLeft);

            _title.SetBounds(infoLeft, padding - 2, infoWidth, 56);
            _author.SetBounds(infoLeft, padding + 56, infoWidth, 20);
            _published.SetBounds(infoLeft, padding + 76, infoWidth, 20);
            _copies.SetBounds(infoLeft, padding + 96, infoWidth, 20);
            _availability.SetBounds(infoLeft, padding + 120, infoWidth, 20);

            int buttonHeight = 36;
            int bottom = Height - padding - buttonHeight;

            if (infoWidth >= 330)
            {
                int buttonWidth = (infoWidth - 10) / 2;
                _request.SetBounds(infoLeft, bottom, buttonWidth, buttonHeight);
                _description.SetBounds(infoLeft + buttonWidth + 10, bottom, buttonWidth, buttonHeight);
            }
            else
            {
                _description.SetBounds(infoLeft, bottom, infoWidth, buttonHeight);
                _request.SetBounds(infoLeft, bottom - buttonHeight - 8, infoWidth, buttonHeight);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CardPaint.DrawSurface(
                e.Graphics,
                Size,
                Color.FromArgb(241, 245, 249),
                Color.White,
                Color.FromArgb(226, 232, 240));
            base.OnPaint(e);
        }
    }

    /// <summary>
    /// Small rounded summary block used above the catalog / request / loan lists.
    /// </summary>
    public sealed class StatCard : Control
    {
        private string _caption = "TOTAL";
        private string _value = "0";
        private Color _accentColor = Color.FromArgb(22, 163, 74);
        private Color _valueColor = Color.FromArgb(15, 23, 42);

        public string Caption
        {
            get => _caption;
            set { _caption = value ?? string.Empty; Invalidate(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value ?? string.Empty; Invalidate(); }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public Color ValueColor
        {
            get => _valueColor;
            set { _valueColor = value; Invalidate(); }
        }

        public StatCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(241, 245, 249);
            Height = 78;
            Margin = new Padding(0, 0, 14, 0);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            CardPaint.DrawSurface(
                g,
                Size,
                Color.FromArgb(241, 245, 249),
                Color.White,
                Color.FromArgb(226, 232, 240),
                14);

            if (Width < 60 || Height < 30)
            {
                return;
            }

            Rectangle accent = new Rectangle(16, 16, 5, Height - 32);
            using (GraphicsPath path = DrawHelper.CreateRoundedPath(accent, 3))
            using (SolidBrush brush = new SolidBrush(AccentColor))
            {
                g.FillPath(brush, path);
            }

            using Font valueFont = new Font("Segoe UI", 18F, FontStyle.Bold);
            TextRenderer.DrawText(
                g,
                Value ?? "0",
                valueFont,
                new Rectangle(34, 8, Width - 46, 38),
                ValueColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            using Font captionFont = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            TextRenderer.DrawText(
                g,
                (Caption ?? string.Empty).ToUpperInvariant(),
                captionFont,
                new Rectangle(34, 43, Width - 46, 24),
                Color.FromArgb(100, 116, 139),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>
    /// One row of MY REQUESTS: the requested title, the date, the librarian's
    /// note and a status pill, with a cancel action while the request is pending.
    /// </summary>
    public sealed class RequestCard : Panel
    {
        public event EventHandler? CancelClicked;

        private readonly Label _title = new Label();
        private readonly Label _meta = new Label();
        private readonly Label _requested = new Label();
        private readonly Label _processed = new Label();
        private readonly Label _note = new Label();
        private readonly PillBadge _status = new PillBadge();
        private readonly RoundedButton _cancel = new RoundedButton();

        private readonly BorrowRequestRow _request;

        public BorrowRequestRow Request => _request;

        public RequestCard(BorrowRequestRow request)
        {
            _request = request;

            DoubleBuffered = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Height = 138;
            Margin = new Padding(0, 0, 0, 14);
            Padding = new Padding(24, 16, 20, 16);

            _title.AutoEllipsis = true;
            _title.BackColor = Color.White;
            _title.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _title.ForeColor = Color.FromArgb(15, 23, 42);
            _title.Text = request.BookTitle.ToUpperInvariant();

            _meta.AutoEllipsis = true;
            _meta.BackColor = Color.White;
            _meta.Font = new Font("Segoe UI", 8.5F);
            _meta.ForeColor = Color.FromArgb(100, 116, 139);
            _meta.Text = "REQUEST #" + request.RequestId.ToString(CultureInfo.InvariantCulture) +
                         "     •     AUTHOR: " + request.BookAuthor.ToUpperInvariant() +
                         "     •     YEAR: " + request.BookYearText;

            _requested.AutoEllipsis = true;
            _requested.BackColor = Color.White;
            _requested.Font = new Font("Segoe UI", 8.5F);
            _requested.ForeColor = Color.FromArgb(71, 85, 105);
            _requested.Text = "REQUESTED: " + request.RequestDateText;

            _processed.AutoEllipsis = true;
            _processed.BackColor = Color.White;
            _processed.Font = new Font("Segoe UI", 8.5F);
            _processed.ForeColor = Color.FromArgb(71, 85, 105);
            _processed.Text = "PROCESSED: " + request.ProcessedDateText;

            _note.AutoEllipsis = true;
            _note.BackColor = Color.White;
            _note.Font = new Font("Segoe UI", 8.5F);
            _note.ForeColor = Color.FromArgb(100, 116, 139);
            _note.Text = "NOTE: " + request.NoteText.ToUpperInvariant();

            Color pillFill;
            Color pillBorder;
            Color pillText;

            if (request.IsApproved)
            {
                pillFill = Color.FromArgb(220, 252, 231);
                pillBorder = Color.FromArgb(134, 239, 172);
                pillText = Color.FromArgb(4, 120, 87);
            }
            else if (request.IsRejected)
            {
                pillFill = Color.FromArgb(254, 226, 226);
                pillBorder = Color.FromArgb(252, 165, 165);
                pillText = Color.FromArgb(185, 28, 28);
            }
            else if (request.IsPending)
            {
                pillFill = Color.FromArgb(254, 243, 199);
                pillBorder = Color.FromArgb(253, 230, 138);
                pillText = Color.FromArgb(180, 83, 9);
            }
            else
            {
                pillFill = Color.FromArgb(241, 245, 249);
                pillBorder = Color.FromArgb(226, 232, 240);
                pillText = Color.FromArgb(100, 116, 139);
            }

            _status.BackColor = Color.White;
            _status.FillColor = pillFill;
            _status.BorderColor = pillBorder;
            _status.TextColor = pillText;
            _status.Text = request.Status;
            _status.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            _cancel.CornerRadius = 9;
            _cancel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _cancel.NormalColor = Color.FromArgb(254, 226, 226);
            _cancel.HoverColor = Color.FromArgb(252, 165, 165);
            _cancel.PressedColor = Color.FromArgb(239, 68, 68);
            _cancel.ForeColor = Color.FromArgb(185, 28, 28);
            _cancel.Text = "CANCEL REQUEST";
            _cancel.Size = new Size(150, 34);
            _cancel.Visible = request.IsPending;
            _cancel.Click += delegate { CancelClicked?.Invoke(this, EventArgs.Empty); };

            Controls.Add(_title);
            Controls.Add(_meta);
            Controls.Add(_requested);
            Controls.Add(_processed);
            Controls.Add(_note);
            Controls.Add(_status);
            Controls.Add(_cancel);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            const int left = 24;
            int width = Math.Max(160, Width - left - 22);
            int reserved = _cancel.Visible ? 190 : 160;
            int textWidth = Math.Max(120, width - reserved);

            _title.SetBounds(left, 14, textWidth, 26);
            _meta.SetBounds(left, 40, textWidth, 20);
            _requested.SetBounds(left, 66, Math.Max(140, textWidth / 2), 20);
            _processed.SetBounds(left + Math.Max(150, textWidth / 2), 66, Math.Max(140, textWidth / 2), 20);
            _note.SetBounds(left, 88, textWidth, 20);

            _status.Left = Math.Max(left, Width - 20 - _status.Width);
            _status.Top = 16;

            _cancel.Left = Math.Max(left, Width - 20 - _cancel.Width);
            _cancel.Top = Height - 16 - _cancel.Height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CardPaint.DrawSurface(
                e.Graphics,
                Size,
                Color.FromArgb(241, 245, 249),
                Color.White,
                Color.FromArgb(226, 232, 240),
                14);

            Color accent = _request.IsApproved
                ? Color.FromArgb(22, 163, 74)
                : _request.IsRejected
                    ? Color.FromArgb(239, 68, 68)
                    : _request.IsPending
                        ? Color.FromArgb(245, 158, 11)
                        : Color.FromArgb(148, 163, 184);

            CardPaint.DrawStatusAccent(e.Graphics, Size, accent);
            base.OnPaint(e);
        }
    }

    /// <summary>
    /// One row of MY BORROWED BOOKS: the cover, the title, when it was taken
    /// out, how many days it has run and whether it is back on the shelf.
    /// </summary>
    public sealed class LoanCard : Panel
    {
        private readonly CoverView _cover = new CoverView();
        private readonly Label _title = new Label();
        private readonly Label _meta = new Label();
        private readonly Label _borrowed = new Label();
        private readonly Label _days = new Label();
        private readonly Label _returned = new Label();
        private readonly PillBadge _status = new PillBadge();

        public BorrowRecord Record { get; }

        public LoanCard(BorrowRecord record, byte[]? coverBytes)
        {
            Record = record;

            DoubleBuffered = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Height = 132;
            Margin = new Padding(0, 0, 0, 14);
            Padding = new Padding(22, 14, 20, 14);

            _cover.BackColor = Color.White;
            _cover.CoverBytes = coverBytes;

            _title.AutoEllipsis = true;
            _title.BackColor = Color.White;
            _title.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _title.ForeColor = Color.FromArgb(15, 23, 42);
            _title.Text = record.BookTitle.ToUpperInvariant();

            _meta.AutoEllipsis = true;
            _meta.BackColor = Color.White;
            _meta.Font = new Font("Segoe UI", 8.5F);
            _meta.ForeColor = Color.FromArgb(100, 116, 139);
            _meta.Text = "AUTHOR: " + record.Author.ToUpperInvariant() +
                         "     •     YEAR: " + record.BookYearText +
                         "     •     LOAN #" + record.BorrowId.ToString(CultureInfo.InvariantCulture);

            _borrowed.BackColor = Color.White;
            _borrowed.Font = new Font("Segoe UI", 8.5F);
            _borrowed.ForeColor = Color.FromArgb(71, 85, 105);
            _borrowed.Text = "BORROWED: " + record.DateBorrowedText;

            _days.BackColor = Color.White;
            _days.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _days.ForeColor = Color.FromArgb(4, 120, 87);
            _days.Text = record.DaysBorrowedText;

            _returned.BackColor = Color.White;
            _returned.Font = new Font("Segoe UI", 8.5F);
            _returned.ForeColor = Color.FromArgb(71, 85, 105);
            _returned.Text = "RETURNED: " + record.DateReturnedText;

            bool returned = record.DateReturned.HasValue;
            _status.BackColor = Color.White;
            _status.FillColor = returned ? Color.FromArgb(241, 245, 249) : Color.FromArgb(220, 252, 231);
            _status.BorderColor = returned ? Color.FromArgb(226, 232, 240) : Color.FromArgb(134, 239, 172);
            _status.TextColor = returned ? Color.FromArgb(100, 116, 139) : Color.FromArgb(4, 120, 87);
            _status.Text = returned ? "RETURNED" : "ON LOAN";
            _status.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            Controls.Add(_cover);
            Controls.Add(_title);
            Controls.Add(_meta);
            Controls.Add(_borrowed);
            Controls.Add(_days);
            Controls.Add(_returned);
            Controls.Add(_status);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            const int left = 22;
            const int gap = 16;

            int coverWidth = 66;
            int coverHeight = Math.Min(94, Height - 28);
            _cover.SetBounds(left, 14, coverWidth, coverHeight);

            int textLeft = left + coverWidth + gap;
            int width = Math.Max(160, Width - textLeft - 22);

            _title.SetBounds(textLeft, 12, Math.Max(120, width - 150), 26);
            _meta.SetBounds(textLeft, 38, Math.Max(120, width - 150), 20);
            _borrowed.SetBounds(textLeft, 66, Math.Max(130, width / 3), 20);
            _days.SetBounds(textLeft + Math.Max(140, width / 3), 66, 110, 20);
            _returned.SetBounds(textLeft + Math.Max(140, width / 3) + 116, 66, Math.Max(130, width / 3), 20);

            _status.Left = Math.Max(textLeft, Width - 20 - _status.Width);
            _status.Top = 14;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CardPaint.DrawSurface(
                e.Graphics,
                Size,
                Color.FromArgb(241, 245, 249),
                Color.White,
                Color.FromArgb(226, 232, 240),
                14);

            CardPaint.DrawStatusAccent(
                e.Graphics,
                Size,
                Record.DateReturned.HasValue
                    ? Color.FromArgb(148, 163, 184)
                    : Color.FromArgb(22, 163, 74));

            base.OnPaint(e);
        }
    }

    /// <summary>
    /// One row of MY ATTENDANCE: the day, the exact time the librarian scanned
    /// the patron's QR pass at the library office entrance, and the gate name.
    /// </summary>
    public sealed class AttendanceCard : Panel
    {
        private readonly Label _date = new Label();
        private readonly Label _time = new Label();
        private readonly Label _day = new Label();
        private readonly Label _gate = new Label();
        private readonly PillBadge _status = new PillBadge();

        public AttendanceRow Attendance { get; }

        public AttendanceCard(AttendanceRow attendance)
        {
            Attendance = attendance;

            DoubleBuffered = true;
            BackColor = Color.FromArgb(241, 245, 249);
            Height = 96;
            Margin = new Padding(0, 0, 0, 12);
            Padding = new Padding(24, 14, 20, 14);

            _date.AutoEllipsis = true;
            _date.BackColor = Color.White;
            _date.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _date.ForeColor = Color.FromArgb(15, 23, 42);
            _date.Text = attendance.DateText;

            _time.AutoEllipsis = true;
            _time.BackColor = Color.White;
            _time.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _time.ForeColor = Color.FromArgb(4, 120, 87);
            _time.Text = "TIME IN: " + attendance.TimeText;

            _day.AutoEllipsis = true;
            _day.BackColor = Color.White;
            _day.Font = new Font("Segoe UI", 8.5F);
            _day.ForeColor = Color.FromArgb(100, 116, 139);
            _day.Text = attendance.ScanTime.DayOfWeek.ToString().ToUpperInvariant();

            _gate.AutoEllipsis = true;
            _gate.BackColor = Color.White;
            _gate.Font = new Font("Segoe UI", 8.5F);
            _gate.ForeColor = Color.FromArgb(71, 85, 105);
            _gate.Text = "GATE: " + attendance.Gate.ToUpperInvariant();

            _status.BackColor = Color.White;
            _status.FillColor = Color.FromArgb(220, 252, 231);
            _status.BorderColor = Color.FromArgb(134, 239, 172);
            _status.TextColor = Color.FromArgb(4, 120, 87);
            _status.Text = "RECORDED";
            _status.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            Controls.Add(_date);
            Controls.Add(_time);
            Controls.Add(_day);
            Controls.Add(_gate);
            Controls.Add(_status);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }

        private void LayoutContent()
        {
            const int left = 24;

            int width = Math.Max(160, Width - left - 22);
            int textWidth = Math.Max(120, width - 140);

            _date.SetBounds(left, 12, textWidth, 28);
            _time.SetBounds(left, 42, Math.Max(140, textWidth / 2), 22);
            _day.SetBounds(left + Math.Max(150, textWidth / 2), 42, Math.Max(90, textWidth / 2), 22);
            _gate.SetBounds(left, 66, textWidth, 20);

            _status.Left = Math.Max(left, Width - 20 - _status.Width);
            _status.Top = 16;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CardPaint.DrawSurface(
                e.Graphics,
                Size,
                Color.FromArgb(241, 245, 249),
                Color.White,
                Color.FromArgb(226, 232, 240),
                14);

            CardPaint.DrawStatusAccent(e.Graphics, Size, Color.FromArgb(22, 163, 74));
            base.OnPaint(e);
        }
    }

}
