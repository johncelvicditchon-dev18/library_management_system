using System;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the BOOKS table. It loads the values
    /// of the selected row and writes the edited ones back to books_table.
    /// </summary>
    public sealed partial class EditBookDialog : Form
    {
        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        public string BookTitle { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;
        public int Year { get; private set; }
        public int Quantity { get; private set; }
        public byte[]? CoverImage { get; private set; }

        /// <summary>Design time constructor with a sample book.</summary>
        public EditBookDialog()
            : this(new Book("SAMPLE BOOK TITLE", "SAMPLE AUTHOR", 2024, 3))
        {
        }

        public EditBookDialog(Book book)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "EDIT BOOK",
                "Change the row and its picture, then press UPDATE BOOK.", 640, 780,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);

            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            _titleField.Value = book.Title;
            _authorField.Value = book.Author;
            _yearField.Value = book.Year > 0 ? book.Year.ToString() : string.Empty;
            _quantityField.Value = (book.Quantity > 0 ? book.Quantity : 1).ToString();
            _coverPicker.LoadImage(book.CoverImage);

            _hintLabel.Text = "BOOK ID #" + book.BookId + "     •     DATE ADDED: " + book.DateAdded +
                              "\r\nTHE ID AND THE DATE ADDED NEVER CHANGE.";

            _yearField.Input.KeyPress += DigitsOnly_KeyPress;
            _quantityField.Input.KeyPress += DigitsOnly_KeyPress;
            _primaryButton.Click += UpdateBook_Click;

            Shown += EditBookDialog_Shown;
        }

        private void EditBookDialog_Shown(object? sender, EventArgs e)
        {
            _titleField.FocusInput();
        }

        private static void DigitsOnly_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UpdateBook_Click(object? sender, EventArgs e)
        {
            _titleField.ClearError();
            _authorField.ClearError();
            _yearField.ClearError();
            _quantityField.ClearError();

            string title = _titleField.Value.Trim();
            string author = _authorField.Value.Trim();
            string yearText = _yearField.Value.Trim();
            string quantityText = _quantityField.Value.Trim();

            bool isValid = true;

            if (title.Length == 0)
            {
                _titleField.ShowError("Book title is required.");
                isValid = false;
            }

            if (author.Length == 0)
            {
                _authorField.ShowError("Author is required.");
                isValid = false;
            }

            int year = 0;
            if (yearText.Length == 0)
            {
                _yearField.ShowError("Year is required.");
                isValid = false;
            }
            else if (!int.TryParse(yearText, out year))
            {
                _yearField.ShowError("Enter the year as a number, e.g. 2024.");
                isValid = false;
            }

            int quantity = 0;
            if (quantityText.Length == 0)
            {
                _quantityField.ShowError("Quantity is required.");
                isValid = false;
            }
            else if (!int.TryParse(quantityText, out quantity) || quantity < 1)
            {
                _quantityField.ShowError("Enter the number of copies, e.g. 3.");
                isValid = false;
            }

            if (!isValid)
            {
                if (title.Length == 0)
                {
                    _titleField.FocusInput();
                }
                else if (author.Length == 0)
                {
                    _authorField.FocusInput();
                }
                else if (yearText.Length == 0 || year == 0)
                {
                    _yearField.FocusInput();
                }
                else
                {
                    _quantityField.FocusInput();
                }

                return;
            }

            // The Book model owns the same rules the ADD pop up uses, so the edited
            // values are checked here before books_table is touched.
            try
            {
                Book check = new Book(title, author, year, quantity, _coverPicker.ImageBytes);

                BookTitle = check.Title;
                Author = check.Author;
                Year = check.Year;
                Quantity = check.Quantity;
                CoverImage = check.CoverImage;
            }
            catch (ArgumentException ex)
            {
                DialogChrome.ShowAlert(ex.Message.ToUpperInvariant(), "INVALID BOOK", MessageBoxIcon.Warning);
                _yearField.FocusInput();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DragStart(object? sender, MouseEventArgs e)
        {
            _drag.Start(this, e);
        }

        private void DragMove(object? sender, MouseEventArgs e)
        {
            _drag.Move(this, e);
        }

        private void DragEnd(object? sender, MouseEventArgs e)
        {
            _drag.End();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            DialogChrome.ApplyRoundedRegion(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DialogChrome.ApplyRoundedRegion(this);
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
                DialogChrome.ApplyDropShadow(parameters);
                return parameters;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            DialogChrome.PaintBackground(this, e);
        }
    }
}
