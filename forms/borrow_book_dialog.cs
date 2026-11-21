using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        // The institute and program boxes are drop downs filled from these lists,
        // so the borrower details always match the codes the school uses.
        private static readonly string[] FallbackInstitutes = { "ICS", "IAS", "ICJE", "IBFS", "IHS", "ITE" };

        // PROGRAM LIST: the programs of the school, grouped by the institute that
        // offers them. Choosing an institute narrows the program box to its own
        // programs; leaving the institute blank lists every program instead.
        private static readonly Dictionary<string, string[]> FallbackPrograms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ICS", new[] { "BSCS" } },
                { "IAS", new[] { "AB COMM", "AB ENGLISH", "AB POLSCI" } },
                { "ICJE", new[] { "BS-CRIM", "BSISM" } },
                { "IBFS", new[] { "BSBA-FM", "BSBA-HRM", "BSBA-MM", "BSOA" } },
                { "IHS", new[] { "BS-MIDWIFERY" } },
                { "ITE", new[] { "BEED", "BSED-ENGLISH", "BSED-FILIPINO", "BSED-MATH", "BSED-SOCSTUD" } }
            };

        private static readonly string[] YearLevels = { "1ST YEAR", "2ND YEAR", "3RD YEAR", "4TH YEAR" };

        // ROLE LIST: the role decides which borrower boxes are required. A student
        // needs the school details, a staff member needs the office, and a faculty
        // member needs the institute and program.
        private static readonly string[] Roles = { "STUDENT", "STAFF", "FACULTY" };

        private const string StudentRole = "STUDENT";
        private const string StaffRole = "STAFF";
        private const string FacultyRole = "FACULTY";

        // Every borrower box uses the same two column grid: the two columns start
        // here and each row is one field taller than the one above it.
        private const int GridLeft = 638;
        private const int GridTop = 44;
        private const int GridStepX = 295;
        private const int GridStepY = 90;

        // The boxes are optional, so the first row is a placeholder that reads as
        // "not answered" instead of leaving an empty drop down.
        private const string NotSpecified = "NOT SPECIFIED";

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        // How many copies of a title are still on the shelf. The catalog supplies
        // the real number (copies owned minus open loans); the design time dialog
        // falls back to the copies the book owns.
        private readonly Func<Book, int> _availableCopies;

        public Book? SelectedBook { get; private set; }
        public string BorrowerRole { get; private set; } = StudentRole;
        public string BorrowerOffice { get; private set; } = string.Empty;
        public string BorrowerContactNumber { get; private set; } = string.Empty;
        public string BorrowerLastName { get; private set; } = string.Empty;
        public string BorrowerFirstName { get; private set; } = string.Empty;
        public string BorrowerMiddleName { get; private set; } = string.Empty;
        public string BorrowerName { get; private set; } = string.Empty;
        public string BorrowerIdNumber { get; private set; } = string.Empty;
        public string BorrowerProgram { get; private set; } = string.Empty;
        public string BorrowerYearLevel { get; private set; } = string.Empty;
        public string BorrowerInstitute { get; private set; } = string.Empty;
        public int CopiesToBorrow { get; private set; } = 1;
        public DateTime DateBorrowed { get; private set; } = DateTime.Now;

        /// <summary>Design time constructor with an empty book list.</summary>
        public BorrowBookDialog()
            : this(Array.Empty<Book>())
        {
        }

        public BorrowBookDialog(IReadOnlyList<Book> availableBooks)
            : this(availableBooks, null)
        {
        }

        /// <summary>
        /// <paramref name="availableCopies"/> reports how many copies of a title can
        /// still be borrowed, so the COPIES box can stop at that number.
        /// </summary>
        public BorrowBookDialog(IReadOnlyList<Book> availableBooks, Func<Book, int>? availableCopies)
        {
            _availableCopies = availableCopies ?? (book => book.Quantity);

            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "BORROW A BOOK",
                "Record the loan and the borrower details in one step.", 1240, 700,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);
            DialogChrome.ApplyCompactHeader(this, _titleLabel, _subtitleLabel, _closeButton);

            _dateField.DateBox.MinDate = DateTime.Today.AddYears(-1);
            _dateField.DateBox.MaxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            _dateField.DateBox.Value = DateTime.Now;

            FillInstituteChoices();
            FillYearLevelChoices();
            RefreshProgramChoices();
            FillRoleChoices();

            if (availableBooks != null)
            {
                foreach (Book book in availableBooks)
                {
                    _bookField.ChoiceBox.Items.Add(new BookChoice(book));
                }
            }

            _dateField.DateBox.ValueChanged += DateBox_ValueChanged;
            _bookField.ChoiceBox.SelectedIndexChanged += BookField_SelectedIndexChanged;
            _instituteField.ChoiceBox.SelectedIndexChanged += InstituteField_SelectedIndexChanged;
            _roleField.ChoiceBox.SelectedIndexChanged += RoleField_SelectedIndexChanged;
            _copiesField.Input.KeyPress += CopiesField_KeyPress;
            _contactNumberField.Input.KeyPress += ContactNumberField_KeyPress;
            _lastNameField.Input.TextChanged += UppercaseInput_TextChanged;
            _firstNameField.Input.TextChanged += UppercaseInput_TextChanged;
            _middleNameField.Input.TextChanged += UppercaseInput_TextChanged;
            _officeField.Input.TextChanged += UppercaseInput_TextChanged;
            _primaryButton.Click += BorrowBook_Click;

            ApplyRoleFields();

            Shown += BorrowBookDialog_Shown;
            UpdateLoanInfo();
        }

        private void BorrowBookDialog_Shown(object? sender, EventArgs e)
        {
            if (_bookField.ChoiceBox.Items.Count > 0)
            {
                _bookField.ChoiceBox.SelectedIndex = 0;
                _lastNameField.FocusInput();
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
            UpdateCopiesInput();
            UpdateLoanInfo();
        }

        /// <summary>
        /// COPIES TO BORROW: a typed box, not a list. The librarian types how many
        /// copies the borrower takes and the box accepts only the copies that are
        /// still available. Picking another book keeps a number that still fits and
        /// falls back to one copy when it no longer does.
        /// </summary>
        private void UpdateCopiesInput()
        {
            Book? book = SelectedBookFromList();
            int available = book == null ? 0 : Math.Max(0, _availableCopies(book));

            _copiesField.Caption = "COPIES TO BORROW (" +
                                   available.ToString(CultureInfo.InvariantCulture) + " AVAILABLE)";

            if (available <= 0)
            {
                _copiesField.Value = string.Empty;
                return;
            }

            int typed;
            bool typedNumber = int.TryParse(_copiesField.Value.Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out typed);

            if (!typedNumber || typed < 1 || typed > available)
            {
                _copiesField.Value = "1";
            }
        }

        /// <summary>The typed box only accepts digits, so a count is never letters.</summary>
        private void CopiesField_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private int SelectedCopies()
        {
            int copies;
            return int.TryParse(_copiesField.Value.Trim(), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out copies) ? copies : 0;
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
            _roleField.ClearError();
            _bookField.ClearError();
            _contactNumberField.ClearError();
            _lastNameField.ClearError();
            _firstNameField.ClearError();
            _middleNameField.ClearError();
            _idNumberField.ClearError();
            _officeField.ClearError();
            _instituteField.ClearError();
            _programField.ClearError();
            _yearLevelField.ClearError();
            _copiesField.ClearError();

            Book? book = SelectedBookFromList();
            string role = SelectedRole();
            bool student = string.Equals(role, StudentRole, StringComparison.OrdinalIgnoreCase);
            bool staff = string.Equals(role, StaffRole, StringComparison.OrdinalIgnoreCase);
            bool faculty = string.Equals(role, FacultyRole, StringComparison.OrdinalIgnoreCase);

            string lastName = _lastNameField.Value.Trim();
            string firstName = _firstNameField.Value.Trim();
            string middleName = _middleNameField.Value.Trim();
            string contactNumber = _contactNumberField.Value.Trim();
            string idNumber = _idNumberField.Value.Trim();
            string office = _officeField.Value.Trim();
            string institute = SelectedInstituteCode();
            string program = SelectedProgram();
            string yearLevel = SelectedYearLevel();
            int copies = SelectedCopies();
            int available = book == null ? 0 : Math.Max(0, _availableCopies(book));

            DialogField? firstInvalid = null;

            if (book == null)
            {
                _bookField.ShowError("Choose the book that is being borrowed.");
                firstInvalid = _bookField;
            }

            if (role.Length == 0)
            {
                _roleField.ShowError("Choose the role of the borrower.");
                firstInvalid = firstInvalid ?? _roleField;
            }

            if (lastName.Length == 0)
            {
                _lastNameField.ShowError("Last name is required.");
                firstInvalid = firstInvalid ?? _lastNameField;
            }

            if (firstName.Length == 0)
            {
                _firstNameField.ShowError("First name is required.");
                firstInvalid = firstInvalid ?? _firstNameField;
            }

            // A contact number is required for every role, so the borrower can be
            // reached about the loan whatever the role is.
            if (contactNumber.Length == 0)
            {
                _contactNumberField.ShowError("Contact number is required.");
                firstInvalid = firstInvalid ?? _contactNumberField;
            }

            // STUDENT: the ID number and the school details identify the borrower.
            if (student && idNumber.Length == 0)
            {
                _idNumberField.ShowError("ID number is required for a student.");
                firstInvalid = firstInvalid ?? _idNumberField;
            }

            // STUDENT and FACULTY: both belong to an institute and a program.
            if ((student || faculty) && institute.Length == 0)
            {
                _instituteField.ShowError("Institute is required for this role.");
                firstInvalid = firstInvalid ?? _instituteField;
            }

            if ((student || faculty) && program.Length == 0)
            {
                _programField.ShowError("Program is required for this role.");
                firstInvalid = firstInvalid ?? _programField;
            }

            if (student && yearLevel.Length == 0)
            {
                _yearLevelField.ShowError("Year level is required for a student.");
                firstInvalid = firstInvalid ?? _yearLevelField;
            }

            // STAFF: the office is the only school detail the role needs.
            if (staff && office.Length == 0)
            {
                _officeField.ShowError("Office is required for a staff member.");
                firstInvalid = firstInvalid ?? _officeField;
            }

            // The box is typed by hand, so the count is checked here: a number is
            // required, at least one, and never more than the shelf still has. The
            // availability can also change while the pop up is open.
            if (book != null)
            {
                if (available <= 0)
                {
                    _copiesField.ShowError("No copy of that book is available to borrow.");
                    firstInvalid = firstInvalid ?? _copiesField;
                }
                else if (_copiesField.Value.Trim().Length == 0)
                {
                    _copiesField.ShowError("Enter how many copies are being borrowed.");
                    firstInvalid = firstInvalid ?? _copiesField;
                }
                else if (copies < 1)
                {
                    _copiesField.ShowError("Enter a whole number of copies, at least 1.");
                    firstInvalid = firstInvalid ?? _copiesField;
                }
                else if (copies > available)
                {
                    _copiesField.ShowError("Only " + available.ToString(CultureInfo.InvariantCulture) +
                                           " copy(ies) are still available.");
                    firstInvalid = firstInvalid ?? _copiesField;
                }
            }

            if (firstInvalid != null)
            {
                firstInvalid.FocusInput();
                return;
            }

            try
            {
                // The model validates the lengths the same way the MySQL columns do,
                // so a value that cannot be stored never reaches the database.
                Borrower borrower = new Borrower(
                    lastName,
                    firstName,
                    middleName,
                    idNumber,
                    program,
                    yearLevel,
                    institute,
                    role,
                    office,
                    contactNumber);

                SelectedBook = book;
                BorrowerRole = borrower.Role;
                BorrowerOffice = borrower.Office;
                BorrowerContactNumber = borrower.ContactNumber;
                BorrowerLastName = borrower.LastName;
                BorrowerFirstName = borrower.FirstName;
                BorrowerMiddleName = borrower.MiddleName;
                BorrowerName = borrower.Name;
                BorrowerIdNumber = borrower.IdNumber;
                BorrowerProgram = borrower.Program;
                BorrowerYearLevel = borrower.YearLevel;
                BorrowerInstitute = borrower.Institute;
                CopiesToBorrow = copies;
                DateBorrowed = _dateField.DateBox.Value;
            }
            catch (ArgumentException ex)
            {
                DialogChrome.ShowAlert(ex.Message.ToUpperInvariant(), "INVALID BORROWER", MessageBoxIcon.Warning);
                _lastNameField.FocusInput();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // INSTITUTE BOX: the codes of the six institutes, with a placeholder on top
        // because the borrower details are optional.
        private void FillInstituteChoices()
        {
            _instituteField.ChoiceBox.Items.Clear();
            _instituteField.ChoiceBox.Items.Add(NotSpecified);
            _instituteField.ChoiceBox.Items.AddRange(FallbackInstitutes);
            _instituteField.ChoiceBox.SelectedIndex = 0;
        }

        private void FillRoleChoices()
        {
            _roleField.ChoiceBox.Items.Clear();
            _roleField.ChoiceBox.Items.AddRange(Roles);
            _roleField.ChoiceBox.SelectedIndex = 0;
        }

        private void FillYearLevelChoices()
        {
            _yearLevelField.ChoiceBox.Items.Clear();
            _yearLevelField.ChoiceBox.Items.Add(NotSpecified);
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            _yearLevelField.ChoiceBox.SelectedIndex = 0;
        }

        private string SelectedRole()
        {
            string role = _roleField.ChoiceBox.Text.Trim().ToUpperInvariant();
            return role.Length == 0 ? StudentRole : role;
        }

        private void RoleField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyRoleFields();
        }

        /// <summary>
        /// The ROLE box decides which boxes this borrower has to fill in: a STUDENT
        /// needs the ID number and the school details, a STAFF member needs the
        /// office, and a FACULTY member needs the institute and program. Every box
        /// stays on the grid, so the arrangement is always the same; a box the role
        /// does not ask for is captioned "(NOT REQUIRED)" and locked, so it can never
        /// collect a value that the role does not own.
        /// </summary>
        private void ApplyRoleFields()
        {
            string role = SelectedRole();
            bool student = string.Equals(role, StudentRole, StringComparison.OrdinalIgnoreCase);
            bool staff = string.Equals(role, StaffRole, StringComparison.OrdinalIgnoreCase);
            bool faculty = string.Equals(role, FacultyRole, StringComparison.OrdinalIgnoreCase);

            bool needsIdNumber = student;
            bool needsSchoolDetails = student || faculty;
            bool needsYearLevel = student;
            bool needsOffice = staff;

            // A required box has a plain caption; a box the role does not ask for is
            // marked NOT REQUIRED and cannot be typed in.
            _idNumberField.Caption = needsIdNumber ? "ID NUMBER" : "ID NUMBER (NOT REQUIRED)";
            _instituteField.Caption = needsSchoolDetails ? "INSTITUTE" : "INSTITUTE (NOT REQUIRED)";
            _programField.Caption = needsSchoolDetails ? "PROGRAM" : "PROGRAM (NOT REQUIRED)";
            _yearLevelField.Caption = needsYearLevel ? "YEAR LEVEL" : "YEAR LEVEL (NOT REQUIRED)";
            _officeField.Caption = needsOffice ? "OFFICE" : "OFFICE (NOT REQUIRED)";
            _contactNumberField.Caption = "CONTACT NUMBER";

            SetFieldEnabled(_idNumberField, needsIdNumber);
            SetFieldEnabled(_instituteField, needsSchoolDetails);
            SetFieldEnabled(_programField, needsSchoolDetails);
            SetFieldEnabled(_yearLevelField, needsYearLevel);
            SetFieldEnabled(_officeField, needsOffice);

            ClearInactiveRoleValues(needsIdNumber, needsSchoolDetails, needsYearLevel, needsOffice);
            ClearInactiveRoleErrors(needsIdNumber, needsSchoolDetails, needsYearLevel, needsOffice);
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
        /// Empties a box the role has just locked, so a value typed for a previous
        /// role is never saved against this one. The drop downs fall back to the
        /// "NOT SPECIFIED" row.
        /// </summary>
        private void ClearInactiveRoleValues(
            bool needsIdNumber,
            bool needsSchoolDetails,
            bool needsYearLevel,
            bool needsOffice)
        {
            if (!needsIdNumber)
            {
                _idNumberField.Value = string.Empty;
            }

            if (!needsYearLevel)
            {
                SelectPlaceholder(_yearLevelField);
            }

            if (!needsSchoolDetails)
            {
                SelectPlaceholder(_instituteField);
                SelectPlaceholder(_programField);
            }

            if (!needsOffice)
            {
                _officeField.Value = string.Empty;
            }
        }

        /// <summary>Points a drop down at its "NOT SPECIFIED" row.</summary>
        private static void SelectPlaceholder(DialogField field)
        {
            if (field.ChoiceBox.Items.Count > 0 && field.ChoiceBox.SelectedIndex != 0)
            {
                field.ChoiceBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Puts every borrower box on the same two column grid, in the same order
        /// for every role. YEAR LEVEL and OFFICE share the last row as two equal
        /// cells, so the arrangement always fills the grid evenly.
        /// </summary>
        private void ArrangeBorrowerFields()
        {
            DialogField[] fields =
            {
                _roleField, _contactNumberField,
                _lastNameField, _firstNameField,
                _middleNameField, _idNumberField,
                _instituteField, _programField,
                _yearLevelField, _officeField
            };

            int slot = 0;

            foreach (DialogField field in fields)
            {
                if (!field.Visible)
                {
                    continue;
                }

                field.Location = new Point(
                    GridLeft + (slot % 2) * GridStepX,
                    GridTop + (slot / 2) * GridStepY);
                slot++;
            }
        }

        /// <summary>
        /// A box the role no longer asks for cannot be answered, so a warning left on
        /// it is cleared when the ROLE box changes.
        /// </summary>
        private void ClearInactiveRoleErrors(
            bool needsIdNumber,
            bool needsSchoolDetails,
            bool needsYearLevel,
            bool needsOffice)
        {
            if (!needsIdNumber)
            {
                _idNumberField.ClearError();
            }

            if (!needsYearLevel)
            {
                _yearLevelField.ClearError();
            }

            if (!needsSchoolDetails)
            {
                _instituteField.ClearError();
                _programField.ClearError();
            }

            if (!needsOffice)
            {
                _officeField.ClearError();
            }
        }

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

        /// <summary>
        /// Keeps LAST NAME, FIRST NAME, MIDDLE NAME and OFFICE in capitals while the
        /// user types, so the text that is read on BORROW BOOK is already the text
        /// that borrowers_table stores. Running on TextChanged also covers a paste.
        /// </summary>
        private void UppercaseInput_TextChanged(object? sender, EventArgs e)
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

        private void InstituteField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshProgramChoices();
        }

        /// <summary>
        /// Shows the programs of the chosen institute. With no institute chosen the
        /// box lists every program, so the list is never empty. A program that is
        /// still offered by the new institute stays selected.
        /// </summary>
        private void RefreshProgramChoices()
        {
            string current = SelectedProgram();
            string institute = SelectedInstituteCode();
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _programField.ChoiceBox.BeginUpdate();
            _programField.ChoiceBox.Items.Clear();
            _programField.ChoiceBox.Items.Add(NotSpecified);

            foreach (KeyValuePair<string, string[]> group in FallbackPrograms)
            {
                if (institute.Length > 0 && !string.Equals(group.Key, institute, StringComparison.OrdinalIgnoreCase))
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

            int index = current.Length == 0 ? 0 : _programField.ChoiceBox.FindStringExact(current);
            _programField.ChoiceBox.SelectedIndex = index < 0 ? 0 : index;
        }

        private string SelectedInstituteCode()
        {
            return ChoiceValue(_instituteField);
        }

        private string SelectedProgram()
        {
            return ChoiceValue(_programField);
        }

        /// <summary>Reads a drop down, turning the placeholder into an empty value.</summary>
        private static string ChoiceValue(DialogField field)
        {
            string text = field.ChoiceBox.Text.Trim();
            return text.Length == 0 || string.Equals(text, NotSpecified, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : text;
        }

        private string SelectedYearLevel()
        {
            return ChoiceValue(_yearLevelField);
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
