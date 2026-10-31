using System;
using System.Drawing;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the BOOKS table. It loads the values
    /// of the selected row and writes the edited ones back to books_table.
    /// </summary>
    public sealed class EditBookDialog : LibraryDialog
    {
        private readonly DialogField _titleField;
        private readonly DialogField _authorField;
        private readonly DialogField _yearField;
        private readonly DialogField _quantityField;
        private readonly BookCoverPicker _coverPicker;

        public string BookTitle { get; private set; } = string.Empty;
        public string Author { get; private set; } = string.Empty;
        public int Year { get; private set; }
        public int Quantity { get; private set; }
        public byte[]? CoverImage { get; private set; }

        public EditBookDialog(Book book)
            : base("EDIT BOOK", "Change the row and its picture, then press UPDATE BOOK.", 640, 780)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            int width = ContentWidth;

            _titleField = DialogField.ForText("BOOK TITLE");
            _titleField.Location = new Point(32, 16);
            _titleField.Width = width;
            _titleField.Value = book.Title;

            _authorField = DialogField.ForText("AUTHOR");
            _authorField.Location = new Point(32, 104);
            _authorField.Width = width;
            _authorField.Value = book.Author;

            _yearField = DialogField.ForText("YEAR PUBLISHED");
            _yearField.Location = new Point(32, 192);
            _yearField.Width = width;
            _yearField.Input.MaxLength = 4;
            _yearField.Input.KeyPress += DigitsOnly_KeyPress;
            _yearField.Value = book.Year > 0 ? book.Year.ToString() : string.Empty;

            _quantityField = DialogField.ForText("QUANTITY (COPIES)");
            _quantityField.Location = new Point(32, 280);
            _quantityField.Width = width;
            _quantityField.Input.MaxLength = 4;
            _quantityField.Input.KeyPress += DigitsOnly_KeyPress;
            _quantityField.Value = (book.Quantity > 0 ? book.Quantity : 1).ToString();

            _coverPicker = new BookCoverPicker();
            _coverPicker.Location = new Point(32, 372);
            _coverPicker.Width = width;
            _coverPicker.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _coverPicker.LoadImage(book.CoverImage);

            Label hint = new Label();
            hint.AutoSize = false;
            hint.BackColor = Color.Transparent;
            hint.Font = new Font("Segoe UI", 8.25F);
            hint.ForeColor = Color.FromArgb(148, 163, 184);
            hint.Location = new Point(34, 552);
            hint.Size = new Size(width, 40);
            hint.Text = "BOOK ID #" + book.BookId + "     •     DATE ADDED: " + book.DateAdded +
                        "\r\nTHE ID AND THE DATE ADDED NEVER CHANGE.";

            ContentPanel.Controls.Add(hint);
            ContentPanel.Controls.Add(_coverPicker);
            ContentPanel.Controls.Add(_titleField);
            ContentPanel.Controls.Add(_authorField);
            ContentPanel.Controls.Add(_yearField);
            ContentPanel.Controls.Add(_quantityField);

            PrimaryButton.Text = "UPDATE BOOK";
            PrimaryButton.Click += UpdateBook_Click;

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
                ShowAlert(ex.Message.ToUpperInvariant(), "INVALID BOOK", MessageBoxIcon.Warning);
                _yearField.FocusInput();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
