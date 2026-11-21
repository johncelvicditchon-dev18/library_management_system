using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the BORROWED BOOKS table. It loads the
    /// borrow record and the borrower row, then writes the edited values back to
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
            "4TH YEAR"
        };

        // INSTITUTE and PROGRAM are drop downs filled from the same school lists the
        // BORROW BOOK pop up uses, so an edited record keeps the codes the school
        // recognizes. A row saved before the drop downs existed keeps its stored
        // text as its own row, so editing never blanks an old value.
        private static readonly string[] Institutes = { "ICS", "IAS", "ICJE", "IBFS", "IHS", "ITE" };

        private static readonly Dictionary<string, string[]> Programs =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ICS", new[] { "BSCS" } },
                { "IAS", new[] { "AB COMM", "AB ENGLISH", "AB POLSCI" } },
                { "ICJE", new[] { "BS-CRIM", "BSISM" } },
                { "IBFS", new[] { "BSBA-FM", "BSBA-HRM", "BSBA-MM", "BSOA" } },
                { "IHS", new[] { "BS-MIDWIFERY" } },
                { "ITE", new[] { "BEED", "BSED-ENGLISH", "BSED-FILIPINO", "BSED-MATH", "BSED-SOCSTUD" } }
            };

        private const string NotSpecified = "NOT SPECIFIED";

        // ROLE: the role decides which borrower details this pop up must collect.
        // A STAFF row needs an office, a STUDENT row needs the ID number and the
        // school details, and a FACULTY row needs the institute and the program.
        private const int BorrowerGridLeft = 570;
        private const int BorrowerGridTop = 44;
        private const int BorrowerGridStepY = 90;
        private const int BorrowerHalfWidth = 244;
        private const int BorrowerHalfStepX = 254;
        private const int BorrowerFullWidth = 498;

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();
        private readonly BorrowRecord _record;
        private readonly string _role;

        public Book? SelectedBook { get; private set; }
        public string BorrowerName { get; private set; } = string.Empty;
        public string BorrowerIdNumber { get; private set; } = string.Empty;
        public string BorrowerProgram { get; private set; } = string.Empty;
        public string BorrowerYearLevel { get; private set; } = string.Empty;
        public string BorrowerInstitute { get; private set; } = string.Empty;
        public string BorrowerRole { get; private set; } = BorrowRecord.StudentRole;
        public string BorrowerOffice { get; private set; } = string.Empty;
        public string BorrowerContactNumber { get; private set; } = string.Empty;
        public DateTime DateBorrowed { get; private set; } = DateTime.Now;

        /// <summary>Null means the borrow record is still open (BORROWED).</summary>
        public DateTime? ReturnedOn { get; private set; }

        /// <summary>Design time constructor with a sample borrow record.</summary>
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
                "Change the borrow record or the borrower details, then press UPDATE RECORD.", 1100, 700,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);
            DialogChrome.ApplyCompactHeader(this, _titleLabel, _subtitleLabel, _closeButton);

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

            FillYearLevelChoices();

            _statusField.ChoiceBox.Text = record.DateReturned.HasValue ? "RETURNED" : "BORROWED";

            _borrowerField.Value = record.BorrowerName.ToUpperInvariant();
            _idNumberField.Value = record.BorrowerIdNumber == "—" ? string.Empty : record.BorrowerIdNumber;

            // The role is worked out from the borrower row (or from the details the
            // row kept when it was saved before the role was stored), then shown in
            // a read only box so the librarian knows which details are required.
            _role = record.Role;
            _roleField.Value = _role;
            _officeField.Value = record.BorrowerOffice;
            _contactField.Value = record.BorrowerContactNumber;
            _contactField.Input.KeyPress += ContactNumberField_KeyPress;
            ApplyRoleFields();

            FillInstituteChoices();
            SelectInstitute(record.BorrowerInstitute);
            FillProgramChoices();
            SelectProgram(record.BorrowerProgram);
            _yearLevelField.ChoiceBox.SelectedIndex = YearLevelIndex(record.BorrowerYearLevel);

            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;
            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;
            _statusField.ChoiceBox.SelectedIndexChanged += StatusField_SelectedIndexChanged;
            _instituteField.ChoiceBox.SelectedIndexChanged += InstituteField_SelectedIndexChanged;
            _borrowerField.Input.TextChanged += BorrowerField_TextChanged;
            _officeField.Input.TextChanged += BorrowerField_TextChanged;
            _primaryButton.Click += UpdateRecord_Click;

            Shown += EditBorrowRecordDialog_Shown;
            UpdateBorrowInfo();
        }

        private void EditBorrowRecordDialog_Shown(object? sender, EventArgs e)
        {
            _borrowerField.FocusInput();
        }

        /// <summary>
        /// Captions the borrower boxes after the role of the record and locks the
        /// ones the role does not use: a required box has a plain caption, and a box
        /// the role does not ask for is marked (NOT REQUIRED) and disabled. A locked
        /// box keeps whatever value the row already stored, so an edit never blanks
        /// a detail the role does not own.
        /// </summary>
        private void ApplyRoleFields()
        {
            bool student = string.Equals(_role, BorrowRecord.StudentRole, StringComparison.OrdinalIgnoreCase);
            bool faculty = string.Equals(_role, BorrowRecord.FacultyRole, StringComparison.OrdinalIgnoreCase);
            bool staff = string.Equals(_role, BorrowRecord.StaffRole, StringComparison.OrdinalIgnoreCase);

            bool needsIdNumber = student;
            bool needsSchoolDetails = student || faculty;
            bool needsYearLevel = student;
            bool needsOffice = staff;

            _idNumberField.Caption = needsIdNumber ? "ID NUMBER" : "ID NUMBER (NOT REQUIRED)";
            _instituteField.Caption = needsSchoolDetails ? "INSTITUTE" : "INSTITUTE (NOT REQUIRED)";
            _programField.Caption = needsSchoolDetails ? "PROGRAM" : "PROGRAM (NOT REQUIRED)";
            _yearLevelField.Caption = needsYearLevel ? "YEAR LEVEL" : "YEAR LEVEL (NOT REQUIRED)";
            _officeField.Caption = needsOffice ? "OFFICE" : "OFFICE (NOT REQUIRED)";
            _contactField.Caption = "CONTACT NUMBER";

            // A box the role does not ask for is locked, so it can never collect a
            // value for the wrong role. A locked box keeps the value it already
            // holds, so an edit never blanks a detail the row stored before.
            SetFieldEnabled(_idNumberField, needsIdNumber);
            SetFieldEnabled(_instituteField, needsSchoolDetails);
            SetFieldEnabled(_programField, needsSchoolDetails);
            SetFieldEnabled(_yearLevelField, needsYearLevel);
            SetFieldEnabled(_officeField, needsOffice);

            ArrangeBorrowerFields();
        }

        /// <summary>
        /// Locks a box the current role does not use. A disabled box cannot be typed
        /// in, cannot be focused and is skipped by the TAB key.
        /// </summary>
        private static void SetFieldEnabled(DialogField field, bool enabled)
        {
            if (field.Enabled != enabled)
            {
                field.Enabled = enabled;
            }
        }

        /// <summary>
        /// Arranges BORROWER DETAILS on the same two column grid for every role:
        /// the name fills the first row, then ID NUMBER / ROLE, INSTITUTE / PROGRAM
        /// and YEAR LEVEL / OFFICE share the rows under it.
        /// </summary>
        private void ArrangeBorrowerFields()
        {
            _borrowerField.Location = new Point(BorrowerGridLeft, BorrowerGridTop);
            _borrowerField.Width = BorrowerFullWidth;

            int firstRow = BorrowerGridTop + BorrowerGridStepY;
            int secondRow = firstRow + BorrowerGridStepY;
            int thirdRow = secondRow + BorrowerGridStepY;
            int fourthRow = thirdRow + BorrowerGridStepY;

            PlaceBorrowerField(_idNumberField, 0, firstRow);
            PlaceBorrowerField(_roleField, 1, firstRow);
            PlaceBorrowerField(_instituteField, 0, secondRow);
            PlaceBorrowerField(_programField, 1, secondRow);
            PlaceBorrowerField(_yearLevelField, 0, thirdRow);
            PlaceBorrowerField(_officeField, 1, thirdRow);

            // CONTACT NUMBER: every role must have one, so it takes the full width
            // of its own row under the other borrower details.
            _contactField.Location = new Point(BorrowerGridLeft, fourthRow);
            _contactField.Width = BorrowerFullWidth;
        }

        private static void PlaceBorrowerField(DialogField field, int column, int top)
        {
            field.Location = new Point(BorrowerGridLeft + column * BorrowerHalfStepX, top);
            field.Width = BorrowerHalfWidth;
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

        private void FillYearLevelChoices()
        {
            _yearLevelField.ChoiceBox.BeginUpdate();
            _yearLevelField.ChoiceBox.Items.Clear();
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            _yearLevelField.ChoiceBox.EndUpdate();
        }

        /// <summary>INSTITUTE list: the school codes plus the "NOT SPECIFIED" row.</summary>
        private void FillInstituteChoices()
        {
            _instituteField.ChoiceBox.BeginUpdate();
            _instituteField.ChoiceBox.Items.Clear();
            _instituteField.ChoiceBox.Items.Add(NotSpecified);
            _instituteField.ChoiceBox.Items.AddRange(Institutes);
            _instituteField.ChoiceBox.EndUpdate();
        }

        /// <summary>
        /// PROGRAM list: the programs of the chosen institute, or every program when
        /// no known institute is picked. Choosing another institute narrows the list
        /// to its own programs.
        /// </summary>
        private void FillProgramChoices()
        {
            string institute = SelectedChoice(_instituteField);
            bool knownInstitute = institute.Length > 0 && Programs.ContainsKey(institute);
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _programField.ChoiceBox.BeginUpdate();
            _programField.ChoiceBox.Items.Clear();
            _programField.ChoiceBox.Items.Add(NotSpecified);

            foreach (KeyValuePair<string, string[]> group in Programs)
            {
                if (knownInstitute && !string.Equals(group.Key, institute, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (string program in group.Value)
                {
                    if (listed.Add(program))
                    {
                        _programField.ChoiceBox.Items.Add(program);
                    }
                }
            }

            _programField.ChoiceBox.EndUpdate();
            _programField.ChoiceBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Picks the stored institute. A value that is not one of the school codes
        /// (a row saved before the drop down existed) is added as its own row, so the
        /// edit never turns a real institute into "NOT SPECIFIED".
        /// </summary>
        private void SelectInstitute(string value)
        {
            SelectStoredValue(_instituteField, value);
        }

        /// <summary>
        /// Picks the stored program. Like the institute, a value the school list does
        /// not know is kept as a row of its own.
        /// </summary>
        private void SelectProgram(string value)
        {
            SelectStoredValue(_programField, value);
        }

        private static void SelectStoredValue(DialogField field, string value)
        {
            string text = (value ?? string.Empty).Trim();
            int index = text.Length == 0 ? 0 : field.ChoiceBox.FindStringExact(text);

            if (index < 0 && text.Length > 0)
            {
                field.ChoiceBox.Items.Add(text);
                index = field.ChoiceBox.Items.Count - 1;
            }

            field.ChoiceBox.SelectedIndex = index < 0 ? 0 : index;
        }

        /// <summary>Reads a drop down, turning the placeholder into an empty value.</summary>
        private static string SelectedChoice(DialogField field)
        {
            string text = field.ChoiceBox.Text.Trim();
            return text.Length == 0 || string.Equals(text, NotSpecified, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : text;
        }

        /// <summary>
        /// Choosing another institute rebuilds the program list. The program falls
        /// back to "NOT SPECIFIED" when the new institute does not offer it.
        /// </summary>
        private void InstituteField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string current = SelectedChoice(_programField);
            FillProgramChoices();

            int index = current.Length == 0 ? 0 : _programField.ChoiceBox.FindStringExact(current);
            _programField.ChoiceBox.SelectedIndex = index < 0 ? 0 : index;
        }

        private void BookField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBorrowInfo();
        }

        private void DateBox_ValueChanged(object? sender, EventArgs e)
        {
            UpdateBorrowInfo();
        }

        private void StatusField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBorrowInfo();
        }

        /// <summary>
        /// Keeps BORROWER NAME and OFFICE in capitals while the user types, so the
        /// values written to borrowers_table are already the text on the screen.
        /// Running on TextChanged also covers a paste.
        /// </summary>
        private void BorrowerField_TextChanged(object? sender, EventArgs e)
        {
            if (!(sender is TextBox box))
            {
                return;
            }

            string upper = box.Text.ToUpperInvariant();

            if (string.Equals(upper, box.Text, StringComparison.Ordinal))
            {
                return;
            }

            int caret = box.SelectionStart;
            box.Text = upper;
            box.SelectionStart = Math.Min(caret, box.Text.Length);
        }

        /// <summary>
        /// The contact box accepts only the characters a phone number uses, the same
        /// rule the BORROW BOOK pop up applies.
        /// </summary>
        private void ContactNumberField_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) ||
                char.IsDigit(e.KeyChar) ||
                e.KeyChar == '+' ||
                e.KeyChar == '-' ||
                e.KeyChar == ' ' ||
                e.KeyChar == '(' ||
                e.KeyChar == ')')
            {
                return;
            }

            e.Handled = true;
        }

        private void UpdateBorrowInfo()
        {
            bool returned = string.Equals(_statusField.ChoiceBox.Text, "RETURNED", StringComparison.OrdinalIgnoreCase);

            if (returned)
            {
                DateTime returnedOn = _record.DateReturned ?? DateTime.Now;
                _borrowInfoLabel.Text = "THIS BORROW RECORD IS MARKED AS RETURNED ON " +
                                        returnedOn.ToString("MMMM d, yyyy  h:mm tt").ToUpperInvariant() +
                                        ".\r\nTHE BOOK COUNTS AS AVAILABLE ON THE SHELF AGAIN.";
            }
            else
            {
                DateTime start = _dateField.DateBox.Value;
                _borrowInfoLabel.Text = "ONE DAY IS ADDED EVERY 24 HOURS.\r\nDAY 1 STARTS ON " +
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
            _instituteField.ClearError();
            _programField.ClearError();
            _yearLevelField.ClearError();
            _officeField.ClearError();
            _contactField.ClearError();

            Book? book = SelectedBookFromList();
            string borrowerName = _borrowerField.Value.Trim();
            string idNumber = _idNumberField.Value.Trim();
            string institute = SelectedChoice(_instituteField);
            string program = SelectedChoice(_programField);
            string yearLevel = SelectedYearLevel();
            string office = _officeField.Value.Trim();
            string contactNumber = _contactField.Value.Trim();

            // The role of this record decides which details are required:
            //   STUDENT : ID NUMBER, INSTITUTE, PROGRAM, YEAR LEVEL
            //   FACULTY : INSTITUTE, PROGRAM
            //   STAFF   : OFFICE
            bool student = string.Equals(_role, BorrowRecord.StudentRole, StringComparison.OrdinalIgnoreCase);
            bool faculty = string.Equals(_role, BorrowRecord.FacultyRole, StringComparison.OrdinalIgnoreCase);
            bool staff = string.Equals(_role, BorrowRecord.StaffRole, StringComparison.OrdinalIgnoreCase);
            bool needsSchoolDetails = student || faculty;

            DialogField? firstInvalid = null;

            if (book == null)
            {
                _bookField.ShowError("Choose the book for this borrow record.");
                firstInvalid = _bookField;
            }

            if (borrowerName.Length == 0)
            {
                _borrowerField.ShowError("Borrower name is required.");
                firstInvalid = firstInvalid ?? _borrowerField;
            }

            // A contact number is required for every role, so the library can reach
            // the borrower about the loan. This also completes rows saved when the
            // contact number was still optional.
            if (contactNumber.Length == 0)
            {
                _contactField.ShowError("Contact number is required.");
                firstInvalid = firstInvalid ?? _contactField;
            }

            if (student && idNumber.Length == 0)
            {
                _idNumberField.ShowError("ID number is required for a student.");
                firstInvalid = firstInvalid ?? _idNumberField;
            }

            if (needsSchoolDetails && institute.Length == 0)
            {
                _instituteField.ShowError("Institute is required for this role.");
                firstInvalid = firstInvalid ?? _instituteField;
            }

            if (needsSchoolDetails && program.Length == 0)
            {
                _programField.ShowError("Program is required for this role.");
                firstInvalid = firstInvalid ?? _programField;
            }

            if (student && yearLevel.Length == 0)
            {
                _yearLevelField.ShowError("Year level is required for a student.");
                firstInvalid = firstInvalid ?? _yearLevelField;
            }

            if (staff && office.Length == 0)
            {
                _officeField.ShowError("Office is required for a staff member.");
                firstInvalid = firstInvalid ?? _officeField;
            }

            if (firstInvalid != null)
            {
                firstInvalid.FocusInput();
                return;
            }

            DateTime dateBorrowed = _dateField.DateBox.Value;
            bool returned = string.Equals(_statusField.ChoiceBox.Text, "RETURNED", StringComparison.OrdinalIgnoreCase);
            DateTime? returnedOn = null;

            if (returned)
            {
                // An existing return date is kept; a borrow record switched to
                // RETURNED is closed now.
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
                    program,
                    yearLevel,
                    institute);

                borrower.ApplyRoleDetails(_role, office, contactNumber);

                SelectedBook = book;
                BorrowerName = borrower.Name;
                BorrowerIdNumber = borrower.IdNumber;
                BorrowerProgram = borrower.Program;
                BorrowerYearLevel = borrower.YearLevel;
                BorrowerInstitute = borrower.Institute;
                BorrowerRole = borrower.Role;
                BorrowerOffice = borrower.Office;
                BorrowerContactNumber = borrower.ContactNumber;
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            DialogChrome.PaintCompactBackground(this, e);
        }
    }
}
