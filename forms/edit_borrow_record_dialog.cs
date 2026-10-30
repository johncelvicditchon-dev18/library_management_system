using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the BORROWED BOOKS table. It loads the
    /// loan and the borrower row, then writes the edited values back to
    /// borrow_transactions_table and borrowers_table.
    /// </summary>
    public sealed class EditBorrowRecordDialog : LibraryDialog
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
        private readonly DialogField _statusField;
        private readonly DialogField _borrowerField;
        private readonly DialogField _idNumberField;
        private readonly DialogField _programField;
        private readonly DialogField _yearLevelField;
        private readonly DialogField _instituteField;
        private readonly Label _loanInfoLabel;
        private readonly BorrowRecord _record;

        public Book? SelectedBook { get; private set; }
        public string BorrowerName { get; private set; } = string.Empty;
        public string BorrowerIdNumber { get; private set; } = string.Empty;
        public string BorrowerProgram { get; private set; } = string.Empty;
        public string BorrowerYearLevel { get; private set; } = string.Empty;
        public string BorrowerInstitute { get; private set; } = string.Empty;
        public DateTime DateBorrowed { get; private set; } = DateTime.Now;

        /// <summary>Null means the loan is open (BORROWED).</summary>
        public DateTime? ReturnedOn { get; private set; }

        public EditBorrowRecordDialog(BorrowRecord record, IReadOnlyList<Book> books)
            : base("EDIT BORROW RECORD", "Change the loan or the borrower details, then press UPDATE RECORD.", 900, 700)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            _record = record;

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
            _dateField.DateBox.MinDate = DateTime.Today.AddYears(-10);
            _dateField.DateBox.MaxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            _dateField.DateBox.Value = record.DateBorrowed;
            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;

            _bookField = DialogField.ForChoice("BOOK TITLE");
            _bookField.Location = new Point(leftX, 120);
            _bookField.Width = columnWidth;
            _bookField.ChoiceBox.DisplayMember = "DisplayText";

            int selectedIndex = -1;
            if (books != null)
            {
                for (int i = 0; i < books.Count; i++)
                {
                    _bookField.ChoiceBox.Items.Add(new BookChoice(books[i]));

                    if (books[i].BookId == record.BookId && selectedIndex < 0)
                    {
                        selectedIndex = i;
                    }
                }
            }

            if (selectedIndex < 0 && _bookField.ChoiceBox.Items.Count > 0)
            {
                selectedIndex = 0;
            }

            if (selectedIndex >= 0)
            {
                _bookField.ChoiceBox.SelectedIndex = selectedIndex;
            }

            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;

            _statusField = DialogField.ForChoice("LOAN STATUS");
            _statusField.Location = new Point(leftX, 210);
            _statusField.Width = columnWidth;
            _statusField.ChoiceBox.Items.AddRange(new object[] { "BORROWED", "RETURNED" });
            _statusField.ChoiceBox.Text = record.DateReturned.HasValue ? "RETURNED" : "BORROWED";
            _statusField.ChoiceBox.SelectedIndexChanged += StatusField_SelectedIndexChanged;

            _loanInfoLabel = new Label();
            _loanInfoLabel.AutoSize = false;
            _loanInfoLabel.BackColor = Color.Transparent;
            _loanInfoLabel.Font = new Font("Segoe UI", 8.25F);
            _loanInfoLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _loanInfoLabel.Location = new Point(leftX + 2, 306);
            _loanInfoLabel.Size = new Size(columnWidth, 40);

            _borrowerField = DialogField.ForText("BORROWER NAME");
            _borrowerField.Location = new Point(rightX, 30);
            _borrowerField.Width = columnWidth;
            _borrowerField.Value = record.BorrowerName;

            _idNumberField = DialogField.ForText("ID NUMBER");
            _idNumberField.Location = new Point(rightX, 120);
            _idNumberField.Width = columnWidth;
            _idNumberField.Input.MaxLength = 50;
            _idNumberField.Value = record.BorrowerIdNumber == "—" ? string.Empty : record.BorrowerIdNumber;

            _programField = DialogField.ForText("PROGRAM (OPTIONAL)");
            _programField.Location = new Point(rightX, 210);
            _programField.Width = columnWidth;
            _programField.Input.MaxLength = 100;
            _programField.Value = record.BorrowerProgram;

            _yearLevelField = DialogField.ForChoice("YEAR LEVEL (OPTIONAL)");
            _yearLevelField.Location = new Point(rightX, 300);
            _yearLevelField.Width = columnWidth;
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            _yearLevelField.ChoiceBox.SelectedIndex = YearLevelIndex(record.BorrowerYearLevel);

            _instituteField = DialogField.ForText("INSTITUTE (OPTIONAL)");
            _instituteField.Location = new Point(rightX, 390);
            _instituteField.Width = columnWidth;
            _instituteField.Input.MaxLength = 100;
            _instituteField.Value = record.BorrowerInstitute;

            ContentPanel.Controls.Add(loanCaption);
            ContentPanel.Controls.Add(borrowerCaption);
            ContentPanel.Controls.Add(_loanInfoLabel);
            ContentPanel.Controls.Add(_dateField);
            ContentPanel.Controls.Add(_bookField);
            ContentPanel.Controls.Add(_statusField);
            ContentPanel.Controls.Add(_borrowerField);
            ContentPanel.Controls.Add(_idNumberField);
            ContentPanel.Controls.Add(_programField);
            ContentPanel.Controls.Add(_yearLevelField);
            ContentPanel.Controls.Add(_instituteField);

            PrimaryButton.Text = "UPDATE RECORD";
            PrimaryButton.Click += UpdateRecord_Click;

            Shown += EditBorrowRecordDialog_Shown;
            UpdateLoanInfo();
        }

        private void EditBorrowRecordDialog_Shown(object? sender, EventArgs e)
        {
            _borrowerField.FocusInput();
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

        private static int YearLevelIndex(string value)
        {
            for (int i = 0; i < YearLevels.Length; i++)
            {
                if (string.Equals(YearLevels[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        private void BookField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateLoanInfo();
        }

        private void DateBox_ValueChanged(object? sender, EventArgs e)
        {
            UpdateLoanInfo();
        }

        private void StatusField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateLoanInfo();
        }

        private void UpdateLoanInfo()
        {
            if (_loanInfoLabel == null)
            {
                return;
            }

            bool returned = string.Equals(_statusField.ChoiceBox.Text, "RETURNED", StringComparison.OrdinalIgnoreCase);

            if (returned)
            {
                DateTime returnedOn = _record.DateReturned ?? DateTime.Now;
                _loanInfoLabel.Text = "THIS LOAN IS MARKED AS RETURNED ON " +
                                      returnedOn.ToString("MMMM d, yyyy  h:mm tt").ToUpperInvariant() +
                                      ".\r\nTHE BOOK COUNTS AS AVAILABLE ON THE SHELF AGAIN.";
            }
            else
            {
                DateTime start = _dateField.DateBox.Value;
                _loanInfoLabel.Text = "ONE DAY IS ADDED EVERY 24 HOURS.\r\nDAY 1 STARTS ON " +
                                      start.AddDays(1).ToString("MMMM d, yyyy  h:mm tt").ToUpperInvariant() + ".";
            }
        }

        private Book? SelectedBookFromList()
        {
            BookChoice? choice = _bookField.ChoiceBox.SelectedItem as BookChoice;
            return choice == null ? null : choice.Book;
        }

        private void UpdateRecord_Click(object? sender, EventArgs e)
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
                _bookField.ShowError("Choose the book for this loan.");
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

            DateTime dateBorrowed = _dateField.DateBox.Value;
            bool returned = string.Equals(_statusField.ChoiceBox.Text, "RETURNED", StringComparison.OrdinalIgnoreCase);
            DateTime? returnedOn = null;

            if (returned)
            {
                // An existing return date is kept; a loan switched to RETURNED is
                // closed now.
                returnedOn = _record.DateReturned ?? DateTime.Now;

                if (returnedOn.Value < dateBorrowed)
                {
                    _dateField.ShowError("The borrow date must be earlier than the return date.");
                    _dateField.FocusInput();
                    return;
                }
            }

            try
            {
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
                DateBorrowed = dateBorrowed;
                ReturnedOn = returnedOn;
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
