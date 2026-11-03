using System;
using System.Collections.Generic;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form used by the BORROW BOOK button. The inputs mirror the MySQL
    /// tables: date_borrow and the book for borrow_transactions_table, and the
    /// borrower details for borrowers_table.
    /// </summary>
    public sealed partial class BorrowBookDialog : Form
    {
        private static readonly string[] YearLevels =
        {
            "NOT SPECIFIED",
            "1ST YEAR",
            "2ND YEAR",
            "3RD YEAR",
            "4TH YEAR",
            "5TH YEAR",
            "GRADUATE"
        };

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        public Book? SelectedBook { get; private set; }
        public string BorrowerName { get; private set; } = string.Empty;
        public string BorrowerIdNumber { get; private set; } = string.Empty;
        public string BorrowerProgram { get; private set; } = string.Empty;
        public string BorrowerYearLevel { get; private set; } = string.Empty;
        public string BorrowerInstitute { get; private set; } = string.Empty;
        public DateTime DateBorrowed { get; private set; } = DateTime.Now;

        /// <summary>Design time constructor with an empty book list.</summary>
        public BorrowBookDialog()
            : this(Array.Empty<Book>())
        {
        }

        public BorrowBookDialog(IReadOnlyList<Book> availableBooks)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "BORROW A BOOK",
                "Record the loan and the borrower details in one step.", 900, 700,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);

            _dateField.DateBox.MinDate = DateTime.Today.AddYears(-1);
            _dateField.DateBox.MaxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            _dateField.DateBox.Value = DateTime.Now;

            if (availableBooks != null)
            {
                foreach (Book book in availableBooks)
                {
                    _bookField.ChoiceBox.Items.Add(new BookChoice(book));
                }
            }

            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;
            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;
            _primaryButton.Click += BorrowBook_Click;

            Shown += BorrowBookDialog_Shown;
            UpdateLoanInfo();
        }

        private void BorrowBookDialog_Shown(object? sender, EventArgs e)
        {
            if (_bookField.ChoiceBox.Items.Count > 0)
            {
                _bookField.ChoiceBox.SelectedIndex = 0;

                if (_yearLevelField.ChoiceBox.Items.Count > 0)
                {
                    _yearLevelField.ChoiceBox.SelectedIndex = 0;
                }

                _borrowerField.FocusInput();
            }
            else
            {
                _bookField.ShowError("Add a book to the catalog before recording a borrow.");
            }
        }

        private void BookField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Book? book = SelectedBookFromList();
            _authorField.Value = book == null ? string.Empty : book.Author;
            UpdateLoanInfo();
        }

        private void DateBox_ValueChanged(object? sender, EventArgs e)
        {
            UpdateLoanInfo();
        }

        private Book? SelectedBookFromList()
        {
            BookChoice? choice = _bookField.ChoiceBox.SelectedItem as BookChoice;
            return choice == null ? null : choice.Book;
        }

        private void UpdateLoanInfo()
        {
            DateTime start = _dateField.DateBox.Value;
            _loanInfoLabel.Text = "ONE DAY IS ADDED EVERY 24 HOURS.\r\nDAY 1 STARTS ON " +
                                  start.AddDays(1).ToString("MMMM d, yyyy  h:mm tt").ToUpperInvariant() + ".";
        }

        private void BorrowBook_Click(object? sender, EventArgs e)
        {
            _bookField.ClearError();
            _borrowerField.ClearError();
            _idNumberField.ClearError();

            Book? book = SelectedBookFromList();
            string borrowerName = _borrowerField.Value.Trim();
            string idNumber = _idNumberField.Value.Trim();

            bool isValid = true;

            if (book == null)
            {
                _bookField.ShowError("Choose the book that is being borrowed.");
                isValid = false;
            }

            if (borrowerName.Length == 0)
            {
                _borrowerField.ShowError("Borrower name is required.");
                isValid = false;
            }

            if (idNumber.Length == 0)
            {
                _idNumberField.ShowError("ID number is required and must be unique.");
                isValid = false;
            }

            if (!isValid)
            {
                if (book == null)
                {
                    _bookField.FocusInput();
                }
                else if (borrowerName.Length == 0)
                {
                    _borrowerField.FocusInput();
                }
                else
                {
                    _idNumberField.FocusInput();
                }

                return;
            }

            try
            {
                // The model validates the lengths the same way the MySQL columns do,
                // so a value that cannot be stored never reaches the database.
                Borrower borrower = new Borrower(
                    borrowerName,
                    idNumber,
                    _programField.Value,
                    SelectedYearLevel(),
                    _instituteField.Value);

                SelectedBook = book;
                BorrowerName = borrower.Name;
                BorrowerIdNumber = borrower.IdNumber;
                BorrowerProgram = borrower.Program;
                BorrowerYearLevel = borrower.YearLevel;
                BorrowerInstitute = borrower.Institute;
                DateBorrowed = _dateField.DateBox.Value;
            }
            catch (ArgumentException ex)
            {
                DialogChrome.ShowAlert(ex.Message.ToUpperInvariant(), "INVALID BORROWER", MessageBoxIcon.Warning);
                _idNumberField.FocusInput();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private string SelectedYearLevel()
        {
            string selected = _yearLevelField.ChoiceBox.Text;

            return string.Equals(selected, "NOT SPECIFIED", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : selected;
        }

        /// <summary>Wraps a book so the drop down can show a friendly, readable line.</summary>
        private sealed class BookChoice
        {
            public Book Book { get; private set; }

            public string DisplayText
            {
                get { return Book.DisplayText; }
            }

            public BookChoice(Book book)
            {
                Book = book;
            }

            public override string ToString()
            {
                return DisplayText;
            }
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
