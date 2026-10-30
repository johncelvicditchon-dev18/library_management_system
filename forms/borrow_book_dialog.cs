using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form used by the BORROW BOOK button. The inputs mirror the MySQL
    /// tables: date_borrow and the book for borrow_transactions_table, and the
    /// borrower details for borrowers_table.
    /// </summary>
    public sealed class BorrowBookDialog : LibraryDialog
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

        private readonly DialogField _dateField;
        private readonly DialogField _bookField;
        private readonly DialogField _authorField;
        private readonly DialogField _borrowerField;
        private readonly DialogField _idNumberField;
        private readonly DialogField _programField;
        private readonly DialogField _yearLevelField;
        private readonly DialogField _instituteField;
        private readonly Label _loanInfoLabel;

        public Book? SelectedBook { get; private set; }
        public string BorrowerName { get; private set; } = string.Empty;
        public string BorrowerIdNumber { get; private set; } = string.Empty;
        public string BorrowerProgram { get; private set; } = string.Empty;
        public string BorrowerYearLevel { get; private set; } = string.Empty;
        public string BorrowerInstitute { get; private set; } = string.Empty;
        public DateTime DateBorrowed { get; private set; } = DateTime.Now;

        public BorrowBookDialog(IReadOnlyList<Book> availableBooks)
            : base("BORROW A BOOK", "Record the loan and the borrower details in one step.", 900, 700)
        {
            const int leftX = 32;
            const int rightX = 468;
            const int columnWidth = 400;

            Label loanCaption = CreateSectionLabel("LOAN DETAILS (borrow_transactions_table)");
            loanCaption.Location = new Point(leftX + 2, 4);

            Label borrowerCaption = CreateSectionLabel("BORROWER DETAILS (borrowers_table)");
            borrowerCaption.Location = new Point(rightX + 2, 4);

            _dateField = DialogField.ForDate("DATE AND TIME OF BORROW");
            _dateField.Location = new Point(leftX, 30);
            _dateField.Width = columnWidth;
            _dateField.DateBox.CustomFormat = "MMMM d, yyyy   h:mm tt";
            _dateField.DateBox.ShowUpDown = true;
            _dateField.DateBox.MinDate = DateTime.Today.AddYears(-1);
            _dateField.DateBox.MaxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            _dateField.DateBox.Value = DateTime.Now;
            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;

            _bookField = DialogField.ForChoice("BOOK TITLE");
            _bookField.Location = new Point(leftX, 120);
            _bookField.Width = columnWidth;
            _bookField.ChoiceBox.DisplayMember = "DisplayText";

            if (availableBooks != null)
            {
                foreach (Book book in availableBooks)
                {
                    _bookField.ChoiceBox.Items.Add(new BookChoice(book));
                }
            }

            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;

            _authorField = DialogField.ForReadOnlyText("AUTHOR");
            _authorField.Location = new Point(leftX, 210);
            _authorField.Width = columnWidth;

            _loanInfoLabel = new Label();
            _loanInfoLabel.AutoSize = false;
            _loanInfoLabel.BackColor = Color.Transparent;
            _loanInfoLabel.Font = new Font("Segoe UI", 8.25F);
            _loanInfoLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _loanInfoLabel.Location = new Point(leftX + 2, 306);
            _loanInfoLabel.Size = new Size(columnWidth, 36);
            _loanInfoLabel.Text = "THE LOAN HAS NO DUE DATE.\r\nTHE DAYS BORROWED ARE COUNTED FROM THIS DATE.";

            _borrowerField = DialogField.ForText("BORROWER NAME");
            _borrowerField.Location = new Point(rightX, 30);
            _borrowerField.Width = columnWidth;

            _idNumberField = DialogField.ForText("ID NUMBER");
            _idNumberField.Location = new Point(rightX, 120);
            _idNumberField.Width = columnWidth;
            _idNumberField.Input.MaxLength = 50;

            _programField = DialogField.ForText("PROGRAM (OPTIONAL)");
            _programField.Location = new Point(rightX, 210);
            _programField.Width = columnWidth;
            _programField.Input.MaxLength = 100;

            _yearLevelField = DialogField.ForChoice("YEAR LEVEL (OPTIONAL)");
            _yearLevelField.Location = new Point(rightX, 300);
            _yearLevelField.Width = columnWidth;
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);

            _instituteField = DialogField.ForText("INSTITUTE (OPTIONAL)");
            _instituteField.Location = new Point(rightX, 390);
            _instituteField.Width = columnWidth;
            _instituteField.Input.MaxLength = 100;

            ContentPanel.Controls.Add(loanCaption);
            ContentPanel.Controls.Add(borrowerCaption);
            ContentPanel.Controls.Add(_loanInfoLabel);
            ContentPanel.Controls.Add(_dateField);
            ContentPanel.Controls.Add(_bookField);
            ContentPanel.Controls.Add(_authorField);
            ContentPanel.Controls.Add(_borrowerField);
            ContentPanel.Controls.Add(_idNumberField);
            ContentPanel.Controls.Add(_programField);
            ContentPanel.Controls.Add(_yearLevelField);
            ContentPanel.Controls.Add(_instituteField);

            PrimaryButton.Text = "BORROW BOOK";
            PrimaryButton.Click += BorrowBook_Click;

            Shown += BorrowBookDialog_Shown;
            UpdateLoanInfo();
        }

        private void BorrowBookDialog_Shown(object? sender, EventArgs e)
        {
            if (_bookField.ChoiceBox.Items.Count > 0)
            {
                _bookField.ChoiceBox.SelectedIndex = 0;
                _yearLevelField.ChoiceBox.SelectedIndex = 0;
                _borrowerField.FocusInput();
            }
            else
            {
                _bookField.ShowError("Add a book to the catalog before recording a borrow.");
            }
        }

        private static Label CreateSectionLabel(string text)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(100, 116, 139);
            label.Location = new Point(0, 0);
            label.Size = new Size(410, 18);
            label.Text = text;

            return label;
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
                ShowAlert(ex.Message.ToUpperInvariant(), "INVALID BORROWER", MessageBoxIcon.Warning);
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
    }
}
