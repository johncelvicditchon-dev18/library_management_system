using System;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by the EDIT icon of the account tables (librarian / staff,
    /// faculty and student). It loads the account row plus the profile row behind it
    /// and writes the edited values back to account_table, account_role_table and
    /// student_info_table / faculty_info_table / staff_info_table / librarian_info_table.
    /// </summary>
    public sealed partial class EditAccountDialog : Form
    {
        private static readonly string[] Roles =
        {
            "LIBRARIAN",
            "STAFF",
            "FACULTY",
            "STUDENT"
        };

        private static readonly string[] Statuses =
        {
            "ACTIVE",
            "PENDING",
            "INACTIVE",
            "REJECTED"
        };

        private static readonly string[] YearLevels =
        {
            "1ST YEAR",
            "2ND YEAR",
            "3RD YEAR",
            "4TH YEAR",
            "5TH YEAR",
            "GRADUATE"
        };

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();
        private readonly AccountRow _row;

        public string AccountName { get; private set; } = string.Empty;
        public string AccountUsername { get; private set; } = string.Empty;
        public string Role { get; private set; } = string.Empty;
        public string Status { get; private set; } = string.Empty;
        public string SchoolId { get; private set; } = string.Empty;
        public string Program { get; private set; } = string.Empty;
        public string YearLevel { get; private set; } = string.Empty;
        public string Institute { get; private set; } = string.Empty;
        public string ContactNumber { get; private set; } = string.Empty;
        public string Office { get; private set; } = string.Empty;

        /// <summary>Design time constructor with a sample student account.</summary>
        public EditAccountDialog()
            : this(new AccountRow(
                0,
                "SAMPLE ACCOUNT",
                "SAMPLE.USER",
                "STUDENT",
                "ACTIVE",
                "JANUARY 1, 2026",
                "2024-0001",
                "BS INFORMATION TECHNOLOGY",
                "3RD YEAR",
                "INSTITUTE OF COMPUTING",
                "09171234567",
                "SAMPLE ADDRESS"))
        {
        }

        public EditAccountDialog(AccountRow row)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "EDIT ACCOUNT",
                "Change the account or its profile, then press UPDATE ACCOUNT.", 900, 700,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);

            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            _row = row;

            _nameField.Value = row.Name;
            _usernameField.Value = row.Username == "—" ? string.Empty : row.Username;

            EnsureChoice(_roleField, Roles, row.Role);
            EnsureChoice(_statusField, Statuses, row.Status);

            _schoolIdField.Value = Clean(row.SchoolId);
            _programField.Value = Clean(row.Program);

            string yearLevel = Clean(row.YearLevel);
            int yearIndex = Array.FindIndex(YearLevels,
                delegate (string item) { return string.Equals(item, yearLevel, StringComparison.OrdinalIgnoreCase); });

            if (_yearLevelField.ChoiceBox.Items.Count == 0)
            {
                _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            }

            _yearLevelField.ChoiceBox.SelectedIndex = yearIndex >= 0 ? yearIndex : 0;

            _instituteField.Value = Clean(row.Institute);
            _contactField.Value = Clean(row.ContactNumber);
            _officeField.Value = Clean(row.Office);

            _roleField.ChoiceBox.SelectedIndexChanged += RoleField_SelectedIndexChanged;
            _primaryButton.Click += UpdateAccount_Click;

            Shown += EditAccountDialog_Shown;
            ApplyProfileVisibility();
        }

        private void EditAccountDialog_Shown(object? sender, EventArgs e)
        {
            _nameField.FocusInput();
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }

        private static void EnsureChoice(DialogField field, string[] values, string current)
        {
            // The designer file fills these lists, but keep the dialog working even
            // if a designer re-serialization drops the item list.
            if (field.ChoiceBox.Items.Count == 0)
            {
                field.ChoiceBox.Items.AddRange(values);
            }

            string wanted = Clean(current);

            int index = Array.FindIndex(values,
                delegate (string item) { return string.Equals(item, wanted, StringComparison.OrdinalIgnoreCase); });

            if (index < 0 && wanted.Length > 0)
            {
                field.ChoiceBox.Items.Add(wanted);
                index = field.ChoiceBox.Items.Count - 1;
            }

            field.ChoiceBox.SelectedIndex = index >= 0 ? index : 0;
        }

        // The profile block only makes sense for a role that owns a profile row in
        // MySQL: student_info_table, faculty_info_table or staff_info_table.
        // FACULTY only keeps the institute (no program) and the LIBRARIAN only
        // keeps the account links (no office).
        private void ApplyProfileVisibility()
        {
            string role = _roleField.ChoiceBox.Text;

            bool isStudent = _row.HasProfile && string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase);
            bool isFaculty = _row.HasProfile && string.Equals(role, "FACULTY", StringComparison.OrdinalIgnoreCase);
            bool isStaff = string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase);
            bool isLibrarian = string.Equals(role, "LIBRARIAN", StringComparison.OrdinalIgnoreCase);

            _schoolIdField.Visible = isStudent;
            _yearLevelField.Visible = isStudent;
            _contactField.Visible = isStudent;
            _programField.Visible = isStudent;
            _instituteField.Visible = isStudent || isFaculty;
            _officeField.Visible = isStaff;
            _profileNote.Visible = !(isStudent || isFaculty || isStaff);

            _profileNote.Text = isLibrarian
                ? "THE LIBRARIAN ACCOUNT KEEPS NO EXTRA PROFILE DETAILS.\r\nEDIT THE NAME, USERNAME, ROLE AND STATUS ON THE LEFT."
                : "THIS ACCOUNT HAS NO STUDENT, FACULTY OR STAFF\r\nPROFILE ROW, SO ONLY THE ACCOUNT DETAILS CAN BE EDITED.";

            if (isFaculty)
            {
                // Faculty has no program, so the institute takes the first row.
                _instituteField.Top = 45;
            }
            else if (isStaff)
            {
                // The office is the only profile value a staff account edits.
                _officeField.Top = 45;
            }
            else
            {
                _programField.Top = 135;
                _instituteField.Top = 315;
            }
        }

        private void RoleField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyProfileVisibility();
        }

        private void UpdateAccount_Click(object? sender, EventArgs e)
        {
            _nameField.ClearError();
            _usernameField.ClearError();
            _schoolIdField.ClearError();
            _programField.ClearError();
            _instituteField.ClearError();
            _officeField.ClearError();

            string name = _nameField.Value.Trim();
            string username = _usernameField.Value.Trim();
            string role = _roleField.ChoiceBox.Text;
            string status = _statusField.ChoiceBox.Text;
            string schoolId = _schoolIdField.Value.Trim();
            string program = _programField.Value.Trim();
            string institute = _instituteField.Value.Trim();
            string contact = _contactField.Value.Trim();
            string office = _officeField.Value.Trim();

            bool isStudent = _row.HasProfile && string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase);
            bool isFaculty = _row.HasProfile && string.Equals(role, "FACULTY", StringComparison.OrdinalIgnoreCase);
            bool isStaff = string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase);

            bool isValid = true;

            if (name.Length == 0)
            {
                _nameField.ShowError("Full name is required.");
                isValid = false;
            }

            if (username.Length == 0)
            {
                _usernameField.ShowError("Username is required.");
                isValid = false;
            }

            if (isStudent)
            {
                if (schoolId.Length == 0)
                {
                    _schoolIdField.ShowError("School ID is required for a student.");
                    isValid = false;
                }

                if (program.Length == 0)
                {
                    _programField.ShowError("Program is required for a student.");
                    isValid = false;
                }
            }

            if ((isStudent || isFaculty) && institute.Length == 0)
            {
                _instituteField.ShowError("Institute code is required.");
                isValid = false;
            }

            if (isStaff && office.Length == 0)
            {
                _officeField.ShowError("Office is required for a staff account.");
                isValid = false;
            }

            if (!isValid)
            {
                if (name.Length == 0)
                {
                    _nameField.FocusInput();
                }
                else if (username.Length == 0)
                {
                    _usernameField.FocusInput();
                }
                else if (isStudent && schoolId.Length == 0)
                {
                    _schoolIdField.FocusInput();
                }
                else if (isStudent && program.Length == 0)
                {
                    _programField.FocusInput();
                }
                else if (isStaff && office.Length == 0)
                {
                    _officeField.FocusInput();
                }
                else
                {
                    _instituteField.FocusInput();
                }

                return;
            }

            AccountName = name;
            AccountUsername = username;
            Role = role;
            Status = status;
            SchoolId = schoolId;
            Program = isStudent ? program : string.Empty;
            YearLevel = _yearLevelField.ChoiceBox.Text;
            Institute = institute;
            ContactNumber = contact;
            Office = isStaff ? office : string.Empty;

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
