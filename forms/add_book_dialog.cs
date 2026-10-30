using System;
using System.Drawing;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form used by the ADD BOOK button. The inputs match books_table:
    /// book_title, author, year_published, quantity and the date_added stamp.
    /// </summary>
    public sealed class AddBookDialog : LibraryDialog
    {
        private readonly DialogField _titleField;
        private readonly DialogField _authorField;
        private readonly DialogField _yearField;
        private readonly DialogField _quantityField;

        /// <summary>The validated book, available after the dialog returns OK.</summary>
        public Book? CreatedBook { get; private set; }

        public AddBookDialog()
            : base("ADD NEW BOOK", "Register a new title in the library catalog.", 640, 600)
        {
            int width = ContentWidth;

            _titleField = DialogField.ForText("BOOK TITLE");
            _titleField.Location = new Point(32, 16);
            _titleField.Width = width;

            _authorField = DialogField.ForText("AUTHOR");
            _authorField.Location = new Point(32, 104);
            _authorField.Width = width;

            _yearField = DialogField.ForText("YEAR PUBLISHED");
            _yearField.Location = new Point(32, 192);
            _yearField.Width = width;
            _yearField.Input.MaxLength = 4;
            _yearField.Input.KeyPress += DigitsOnly_KeyPress;

            _quantityField = DialogField.ForText("QUANTITY (COPIES)");
            _quantityField.Location = new Point(32, 280);
            _quantityField.Width = width;
            _quantityField.Input.MaxLength = 4;
            _quantityField.Input.KeyPress += DigitsOnly_KeyPress;
            _quantityField.Value = "1";

            Label hint = new Label();
            hint.AutoSize = false;
            hint.BackColor = Color.Transparent;
            hint.Font = new Font("Segoe UI", 8.25F);
            hint.ForeColor = Color.FromArgb(148, 163, 184);
            hint.Location = new Point(34, 372);
            hint.Size = new Size(width, 40);
            hint.Text = "DATE ADDED: " + Book.TodayText() + "\r\nNEW TITLES ARE SAVED AS AVAILABLE ON THE SHELF.";

            ContentPanel.Controls.Add(hint);
            ContentPanel.Controls.Add(_titleField);
            ContentPanel.Controls.Add(_authorField);
            ContentPanel.Controls.Add(_yearField);
            ContentPanel.Controls.Add(_quantityField);

            PrimaryButton.Text = "SAVE BOOK";
            PrimaryButton.Click += SaveBook_Click;

            Shown += AddBookDialog_Shown;
        }

        private void AddBookDialog_Shown(object? sender, EventArgs e)
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

        private void SaveBook_Click(object? sender, EventArgs e)
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

            try
            {
                CreatedBook = new Book(title, author, year, quantity);
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
