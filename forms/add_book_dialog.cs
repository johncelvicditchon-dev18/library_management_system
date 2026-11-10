using System;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form used by the ADD BOOK button. The inputs match books_table:
    /// book_title, author, year_published, quantity and the date_added stamp.
    /// </summary>
    public sealed partial class AddBookDialog : Form
    {
        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        /// <summary>The validated book, available after the dialog returns OK.</summary>
        public Book? CreatedBook { get; private set; }

        public AddBookDialog()
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "ADD NEW BOOK",
                "Register a new title and attach its picture.", 640, 780,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);
            DialogChrome.ApplyCompactHeader(this, _titleLabel, _subtitleLabel, _closeButton);

            // The date stamp is runtime data, so it is filled after the designer
            // has created the label.
            _hintLabel.Text = "DATE ADDED: " + Book.TodayText() +
                              "\r\nNEW TITLES ARE SAVED AS AVAILABLE ON THE SHELF.";

            _yearField.Input.KeyPress += DigitsOnly_KeyPress;
            _quantityField.Input.KeyPress += DigitsOnly_KeyPress;
            _primaryButton.Click += SaveBook_Click;

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
                CreatedBook = new Book(title, author, year, quantity, _coverPicker.ImageBytes);
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            DialogChrome.PaintCompactBackground(this, e);
        }
    }
}
