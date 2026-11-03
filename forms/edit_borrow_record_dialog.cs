using System;
using System.Collections.Generic;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the BORROWED BOOKS table. It loads the
    /// loan and the borrower row, then writes the edited values back to
    /// borrow_transactions_table and borrowers_table.
    /// </summary>
    public sealed partial class EditBorrowRecordDialog : Form
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

        /// <summary>Design time constructor with a sample loan record.</summary>
        public EditBorrowRecordDialog()
            : this(
                new BorrowRecord(
                    0,
                    0,
                    "SAMPLE BOOK TITLE",
                    "SAMPLE AUTHOR",
                    2024,
                    0,
                    "SAMPLE BORROWER",
                    "2024-0001",
                    "BS INFORMATION TECHNOLOGY",
                    "3RD YEAR",
                    "INSTITUTE OF COMPUTING",
                    DateTime.Now,
                    null),
                Array.Empty<Book>())
        {
        }

        public EditBorrowRecordDialog(BorrowRecord record, IReadOnlyList<Book> books)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "EDIT BORROW RECORD",
                "Change the loan or the borrower details, then press UPDATE RECORD.", 900, 700,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);

            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            _record = record;

            _dateField.DateBox.MinDate = DateTime.Today.AddYears(-10);
            _dateField.DateBox.MaxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            _dateField.DateBox.Value = record.DateBorrowed;

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

            // The designer file fills these lists; refill them defensively so a
            // designer re-serialization can never leave the dialog without items.
            if (_statusField.ChoiceBox.Items.Count == 0)
            {
                _statusField.ChoiceBox.Items.AddRange(new object[] { "BORROWED", "RETURNED" });
            }

            if (_yearLevelField.ChoiceBox.Items.Count == 0)
            {
                _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            }

            _statusField.ChoiceBox.Text = record.DateReturned.HasValue ? "RETURNED" : "BORROWED";

            _borrowerField.Value = record.BorrowerName;
            _idNumberField.Value = record.BorrowerIdNumber == "—" ? string.Empty : record.BorrowerIdNumber;
            _programField.Value = record.BorrowerProgram;
            _yearLevelField.ChoiceBox.SelectedIndex = YearLevelIndex(record.BorrowerYearLevel);
            _instituteField.Value = record.BorrowerInstitute;

            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;
            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;
            _statusField.ChoiceBox.SelectedIndexChanged += StatusField_SelectedIndexChanged;
            _primaryButton.Click += UpdateRecord_Click;

            Shown += EditBorrowRecordDialog_Shown;
            UpdateLoanInfo();
        }

        private void EditBorrowRecordDialog_Shown(object? sender, EventArgs e)
        {
            _borrowerField.FocusInput();
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
