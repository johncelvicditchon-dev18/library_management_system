using System;
using System.Drawing;
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
    public sealed class EditAccountDialog : LibraryDialog
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

        private readonly DialogField _nameField;
        private readonly DialogField _usernameField;
        private readonly DialogField _roleField;
        private readonly DialogField _statusField;
        private readonly DialogField _schoolIdField;
        private readonly DialogField _programField;
        private readonly DialogField _yearLevelField;
        private readonly DialogField _instituteField;
        private readonly DialogField _contactField;
        private readonly DialogField _officeField;
        private readonly Label _profileNote;
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

        public EditAccountDialog(AccountRow row)
            : base("EDIT ACCOUNT", "Change the account or its profile, then press UPDATE ACCOUNT.", 900, 700)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            _row = row;

            const int leftX = 32;
            const int rightX = 468;
            const int columnWidth = 400;

            Label accountCaption = CreateSectionLabel("ACCOUNT (account_table + account_role_table)");
            accountCaption.Location = new Point(leftX + 2, 4);

            Label profileCaption = CreateSectionLabel("PROFILE (student_info_table / faculty_info_table / staff_info_table)");
            profileCaption.Location = new Point(rightX + 2, 4);

            _nameField = DialogField.ForText("FULL NAME");
            _nameField.Location = new Point(leftX, 30);
            _nameField.Width = columnWidth;
            _nameField.Input.MaxLength = 250;
            _nameField.Value = row.Name;

            _usernameField = DialogField.ForText("USERNAME");
            _usernameField.Location = new Point(leftX, 120);
            _usernameField.Width = columnWidth;
            _usernameField.Input.MaxLength = 250;
            _usernameField.Value = row.Username == "—" ? string.Empty : row.Username;

            _roleField = DialogField.ForChoice("ROLE");
            _roleField.Location = new Point(leftX, 210);
            _roleField.Width = columnWidth;
            _roleField.ChoiceBox.Items.AddRange(Roles);
            EnsureChoice(_roleField, Roles, row.Role);
            _roleField.ChoiceBox.SelectedIndexChanged += RoleField_SelectedIndexChanged;

            _statusField = DialogField.ForChoice("STATUS");
            _statusField.Location = new Point(leftX, 300);
            _statusField.Width = columnWidth;
            _statusField.ChoiceBox.Items.AddRange(Statuses);
            EnsureChoice(_statusField, Statuses, row.Status);

            _schoolIdField = DialogField.ForText("SCHOOL ID");
            _schoolIdField.Location = new Point(rightX, 30);
            _schoolIdField.Width = columnWidth;
            _schoolIdField.Input.MaxLength = 30;
            _schoolIdField.Value = Clean(row.SchoolId);

            _programField = DialogField.ForText("PROGRAM");
            _programField.Location = new Point(rightX, 120);
            _programField.Width = columnWidth;
            _programField.Input.MaxLength = 120;
            _programField.Value = Clean(row.Program);

            _yearLevelField = DialogField.ForChoice("YEAR LEVEL");
            _yearLevelField.Location = new Point(rightX, 210);
            _yearLevelField.Width = columnWidth;
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);

            string yearLevel = Clean(row.YearLevel);
            int yearIndex = Array.FindIndex(YearLevels,
                delegate (string item) { return string.Equals(item, yearLevel, StringComparison.OrdinalIgnoreCase); });
            _yearLevelField.ChoiceBox.SelectedIndex = yearIndex >= 0 ? yearIndex : 0;

            _instituteField = DialogField.ForText("INSTITUTE CODE");
            _instituteField.Location = new Point(rightX, 300);
            _instituteField.Width = columnWidth;
            _instituteField.Input.MaxLength = 10;
            _instituteField.Value = Clean(row.Institute);

            _contactField = DialogField.ForText("CONTACT NUMBER");
            _contactField.Location = new Point(rightX, 390);
            _contactField.Width = columnWidth;
            _contactField.Input.MaxLength = 20;
            _contactField.Value = Clean(row.ContactNumber);

            _officeField = DialogField.ForText("OFFICE");
            _officeField.Location = new Point(rightX, 30);
            _officeField.Width = columnWidth;
            _officeField.Input.MaxLength = 120;
            _officeField.Value = Clean(row.Office);

            _profileNote = new Label();
            _profileNote.AutoSize = false;
            _profileNote.BackColor = Color.Transparent;
            _profileNote.Font = new Font("Segoe UI", 8.25F);
            _profileNote.ForeColor = Color.FromArgb(148, 163, 184);
            _profileNote.Location = new Point(rightX + 2, 40);
            _profileNote.Size = new Size(columnWidth, 60);
            _profileNote.Text = "THIS ACCOUNT HAS NO STUDENT, FACULTY, STAFF OR LIBRARIAN\r\n" +
                                "PROFILE ROW, SO ONLY THE ACCOUNT DETAILS CAN BE EDITED.";

            ContentPanel.Controls.Add(accountCaption);
            ContentPanel.Controls.Add(profileCaption);
            ContentPanel.Controls.Add(_nameField);
            ContentPanel.Controls.Add(_usernameField);
            ContentPanel.Controls.Add(_roleField);
            ContentPanel.Controls.Add(_statusField);
            ContentPanel.Controls.Add(_schoolIdField);
            ContentPanel.Controls.Add(_programField);
            ContentPanel.Controls.Add(_yearLevelField);
            ContentPanel.Controls.Add(_instituteField);
            ContentPanel.Controls.Add(_contactField);
            ContentPanel.Controls.Add(_officeField);
            ContentPanel.Controls.Add(_profileNote);

            PrimaryButton.Text = "UPDATE ACCOUNT";
            PrimaryButton.Click += UpdateAccount_Click;

            Shown += EditAccountDialog_Shown;
            ApplyProfileVisibility();
        }

        private void EditAccountDialog_Shown(object? sender, EventArgs e)
        {
            _nameField.FocusInput();
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

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }

        private static void EnsureChoice(DialogField field, string[] values, string current)
        {
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
        private void ApplyProfileVisibility()
        {
            string role = _roleField.ChoiceBox.Text;

            bool isStudent = _row.HasProfile && string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase);
            bool isFaculty = _row.HasProfile && string.Equals(role, "FACULTY", StringComparison.OrdinalIgnoreCase);
            bool isStaff = string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase);
            bool isLibrarian = string.Equals(role, "LIBRARIAN", StringComparison.OrdinalIgnoreCase);
            bool hasOffice = isStaff || isLibrarian;

            _schoolIdField.Visible = isStudent;
            _yearLevelField.Visible = isStudent;
            _contactField.Visible = isStudent;
            _programField.Visible = isStudent || isFaculty;
            _instituteField.Visible = isStudent || isFaculty;
            _officeField.Visible = hasOffice;
            _profileNote.Visible = !(isStudent || isFaculty || hasOffice);

            if (isFaculty)
            {
                // Faculty rows only keep the program and the institute, so the two
                // boxes move up instead of leaving empty gaps.
                _programField.Top = 30;
                _instituteField.Top = 120;
            }
            else
            {
                _programField.Top = 120;
                _instituteField.Top = 300;
            }

            if (hasOffice)
            {
                // The office takes the first row of the profile column.
                _officeField.Top = 30;
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
            bool showProfile = _programField.Visible;

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

            if (showProfile && string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase))
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

            if (showProfile && institute.Length == 0)
            {
                _instituteField.ShowError("Institute code is required.");
                isValid = false;
            }

            if (_officeField.Visible && office.Length == 0)
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
                else if (showProfile && schoolId.Length == 0)
                {
                    _schoolIdField.FocusInput();
                }
                else if (showProfile && program.Length == 0)
                {
                    _programField.FocusInput();
                }
                else if (_officeField.Visible && office.Length == 0)
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
            Program = program;
            YearLevel = _yearLevelField.ChoiceBox.Text;
            Institute = institute;
            ContactNumber = contact;
            Office = office;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
