using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Self service pop up opened by the EDIT MY INFO button of the PROFILE tab.
    /// The signed in person may change every personal detail that belongs to the
    /// account: the name and username of account_table plus the profile values
    /// of student_info_table / faculty_info_table / staff_info_table /
    /// librarian_info_table. The role, the status and the date created stay read
    /// only, because only the library staff may change them.
    /// </summary>
    public sealed partial class MyProfileDialog : Form
    {
        private const int FormWidth = 900;
        private const int LeftColumn = 34;
        private const int RightColumn = 468;
        private const int ColumnWidth = 400;
        private const int FieldHeight = 88;
        private const int FirstFieldTop = 22;
        private const int FieldStep = 90;

        // The dropdowns are filled from `institute_table` and `program_table`. The
        // built-in lists are only used when MySQL is unreachable, so the form can
        // still be opened and shows the same choices as the signup form.
        private static readonly string[] FallbackInstitutes = { "ICS", "IAS", "ICJE", "IBFS", "IHS", "ITE" };

        private static readonly Dictionary<string, string[]> FallbackPrograms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ICS", new[] { "BSCS" } },
                { "IAS", new[] { "AB-COM", "AB-ENGLISH" } },
                { "ICJE", new[] { "BS-CRIMINOLOGY", "BS-ISM" } },
                { "IBFS", new[] { "BSBA-HRMA", "BSBA-FM", "BSOA" } },
                { "IHS", new[] { "BS-MIDWIFERY" } },
                { "ITE", new[] { "BSED-MATH", "BSED-FILIPINO", "BSED-ENGLISH", "BSED-SOCTUD" } }
            };

        private const string InstitutePlaceholder = "SELECT INSTITUTE";
        private const string ProgramPlaceholder = "SELECT PROGRAM";

        private static readonly string[] YearLevels =
        {
            "1ST YEAR",
            "2ND YEAR",
            "3RD YEAR",
            "4TH YEAR",
            "5TH YEAR",
            "GRADUATE"
        };

        // institute code -> institute_id, learned from institute_table.
        private readonly Dictionary<string, int> _instituteIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<ProgramChoice> _programs = new List<ProgramChoice>();

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();
        private readonly AccountRow _row;

        public string FullName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string SchoolId { get; private set; } = string.Empty;
        public string Program { get; private set; } = string.Empty;
        public string YearLevel { get; private set; } = string.Empty;
        public string Institute { get; private set; } = string.Empty;
        public string ContactNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string Office { get; private set; } = string.Empty;

        /// <summary>Design time constructor with a sample student account.</summary>
        public MyProfileDialog()
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
                "ICS",
                "09171234567",
                "SAMPLE ADDRESS"))
        {
        }

        public MyProfileDialog(AccountRow row)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();

            _row = row ?? throw new ArgumentNullException(nameof(row));

            _nameField.Value = Clean(row.Name);
            _usernameField.Value = Clean(row.Username);
            _contactField.Value = Clean(row.ContactNumber);
            _addressField.Value = Clean(row.Address);
            _schoolIdField.Value = Clean(row.SchoolId);
            _officeField.Value = Clean(row.Office);

            // INSTITUTE before PROGRAM: the program list follows the institute,
            // exactly like the signup form.
            LoadInstitutes(Clean(row.Institute));
            LoadPrograms(Clean(row.Program));
            _instituteField.ChoiceBox.SelectedIndexChanged += InstituteField_SelectedIndexChanged;

            EnsureYearLevels(row.YearLevel);

            ConfigureFields();

            int contentHeight = LayoutFields();
            int formHeight = DialogChrome.HeaderHeight + contentHeight + DialogChrome.FooterHeight;

            DialogChrome.ApplyParameters(this, "MY PROFILE", null,
                FormWidth, formHeight,
                _contentPanel, _footerDivider, _titleLabel, null,
                _closeButton, _saveButton, _cancelButton);
            DialogChrome.ApplyCompactHeader(this, _titleLabel, null, _closeButton);

            // Only the heading is kept, so it is centred in the white header.
            _titleLabel.Top = (DialogChrome.HeaderHeight - _titleLabel.Height) / 2;

            _saveButton.Click += SaveButton_Click;
            Shown += MyProfileDialog_Shown;
        }

        private void MyProfileDialog_Shown(object? sender, EventArgs e)
        {
            _nameField.FocusInput();
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }

        private bool IsStudent
        {
            get { return string.Equals(_row.Role, "STUDENT", StringComparison.OrdinalIgnoreCase); }
        }

        private bool IsFaculty
        {
            get { return string.Equals(_row.Role, "FACULTY", StringComparison.OrdinalIgnoreCase); }
        }

        private bool IsStaff
        {
            get
            {
                return string.Equals(_row.Role, "STAFF", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(_row.Role, "OFFICER", StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool IsLibrarian
        {
            get
            {
                return string.Equals(_row.Role, "LIBRARIAN", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(_row.Role, "ADMIN", StringComparison.OrdinalIgnoreCase);
            }
        }

        // A profile row is only updated when the account actually owns one, so a
        // legacy account without a student row never writes an orphan profile.
        private bool HasStudentProfile
        {
            get { return _row.HasProfile && IsStudent; }
        }

        private bool CanEditSchoolId
        {
            get { return HasStudentProfile; }
        }

        private bool CanEditProgram
        {
            get { return HasStudentProfile; }
        }

        private bool CanEditYearLevel
        {
            get { return HasStudentProfile; }
        }

        // The student and the faculty profiles keep the institute through a
        // foreign key. The staff / librarian profiles are created on first save,
        // so their own fields are always offered.
        private bool CanEditInstitute
        {
            get { return HasStudentProfile || IsFaculty; }
        }

        private bool CanEditOffice
        {
            get { return IsStaff; }
        }

        private bool CanEditContact
        {
            get { return HasStudentProfile || IsFaculty || IsStaff || IsLibrarian; }
        }

        private bool CanEditAddress
        {
            get { return HasStudentProfile; }
        }

        private void EnsureYearLevels(string current)
        {
            if (_yearLevelField.ChoiceBox.Items.Count == 0)
            {
                _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            }

            string wanted = Clean(current);
            int index = Array.FindIndex(YearLevels,
                delegate (string item) { return string.Equals(item, wanted, StringComparison.OrdinalIgnoreCase); });

            if (index < 0 && wanted.Length > 0)
            {
                _yearLevelField.ChoiceBox.Items.Add(wanted);
                index = _yearLevelField.ChoiceBox.Items.Count - 1;
            }

            _yearLevelField.ChoiceBox.SelectedIndex = index >= 0 ? index : 0;
        }

        // INSTITUTE DROPDOWN: filled from institute_table, with the built-in codes
        // as a fallback so the form still works when MySQL is unreachable.
        private void LoadInstitutes(string current)
        {
            List<string> codes = new List<string>();
            _instituteIds.Clear();

            try
            {
                using (MySqlConnection connection = Authentication.OpenConnection())
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(
                               "SELECT `institute_id`, `institute_code` FROM `institute_table` ORDER BY `institute_id`;",
                               connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read() && reader.FieldCount > 1)
                        {
                            long id = reader.IsDBNull(0)
                                ? 0
                                : Convert.ToInt64(reader.GetValue(0), System.Globalization.CultureInfo.InvariantCulture);
                            string code = reader.IsDBNull(1)
                                ? string.Empty
                                : (Convert.ToString(reader.GetValue(1), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty).Trim().ToUpperInvariant();

                            if (code.Length == 0)
                            {
                                continue;
                            }

                            codes.Add(code);

                            if (id > 0)
                            {
                                _instituteIds[code] = (int)id;
                            }
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                codes.Clear();
                _instituteIds.Clear();
            }

            if (codes.Count == 0)
            {
                codes.AddRange(FallbackInstitutes);
            }

            ComboBox box = _instituteField.ChoiceBox;
            box.Items.Clear();
            box.Items.Add(InstitutePlaceholder);
            box.Items.AddRange(codes.ToArray());

            int index = FindItem(box, current);

            if (index < 0 && current.Length > 0)
            {
                // Keep the stored code visible even if institute_table no longer
                // lists it, so the value is never silently lost.
                box.Items.Add(current.ToUpperInvariant());
                index = box.Items.Count - 1;
            }

            box.SelectedIndex = index >= 0 ? index : 0;
        }

        // PROGRAM DROPDOWN: reads the program_table rows once and then shows only
        // the programs of the selected institute, exactly like signup.
        private void LoadPrograms(string current)
        {
            _programs.Clear();

            try
            {
                using (MySqlConnection connection = Authentication.OpenConnection())
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(
                               "SELECT `program_id`, `program_name`, `institute_id` FROM `program_table` ORDER BY `program_name`;",
                               connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read() && reader.FieldCount > 2)
                        {
                            ProgramChoice program = new ProgramChoice();
                            program.Name = reader.IsDBNull(1)
                                ? string.Empty
                                : (Convert.ToString(reader.GetValue(1), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty).Trim();
                            program.InstituteId = reader.IsDBNull(2)
                                ? 0
                                : Convert.ToInt64(reader.GetValue(2), System.Globalization.CultureInfo.InvariantCulture);

                            if (program.Name.Length > 0)
                            {
                                _programs.Add(program);
                            }
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                _programs.Clear();
            }

            RefreshProgramOptions(current, true);
        }

        private void RefreshProgramOptions(string current, bool keepStoredValue)
        {
            int instituteId = SelectedInstituteId;
            string instituteCode = SelectedInstituteCode;
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ComboBox box = _programField.ChoiceBox;
            box.BeginUpdate();
            box.Items.Clear();
            box.Items.Add(ProgramPlaceholder);

            foreach (ProgramChoice program in _programs)
            {
                if (instituteId == 0 || program.InstituteId == instituteId)
                {
                    if (listed.Add(program.Name))
                    {
                        box.Items.Add(program.Name);
                    }
                }
            }

            foreach (KeyValuePair<string, string[]> group in FallbackPrograms)
            {
                if (instituteId != 0 && !string.Equals(group.Key, instituteCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (string name in group.Value)
                {
                    if (listed.Add(name))
                    {
                        box.Items.Add(name);
                    }
                }
            }

            box.EndUpdate();

            int index = FindItem(box, current);

            if (index < 0 && keepStoredValue && current.Length > 0 && !string.Equals(current, ProgramPlaceholder, StringComparison.OrdinalIgnoreCase))
            {
                box.Items.Add(current);
                index = box.Items.Count - 1;
            }

            box.SelectedIndex = index >= 0 ? index : 0;
        }

        private void InstituteField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // A new institute owns a different program list, so the previous
            // program is only kept when the new institute also offers it.
            RefreshProgramOptions(_programField.ChoiceBox.Text, false);
        }

        private static int FindItem(ComboBox box, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            for (int index = 1; index < box.Items.Count; index++)
            {
                if (string.Equals(Convert.ToString(box.Items[index]), text, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private string SelectedInstituteCode
        {
            get
            {
                ComboBox box = _instituteField.ChoiceBox;

                return box.SelectedIndex <= 0
                    ? string.Empty
                    : (box.Text ?? string.Empty).Trim().ToUpperInvariant();
            }
        }

        private int SelectedInstituteId
        {
            get
            {
                string code = SelectedInstituteCode;
                int id;

                return code.Length > 0 && _instituteIds.TryGetValue(code, out id) ? id : 0;
            }
        }

        private sealed class ProgramChoice
        {
            public string Name = string.Empty;
            public long InstituteId;
        }

        private void ConfigureFields()
        {
            _contactField.Visible = CanEditContact;
            _addressField.Visible = CanEditAddress;
            _schoolIdField.Visible = CanEditSchoolId;
            _programField.Visible = CanEditProgram;
            _yearLevelField.Visible = CanEditYearLevel;
            _instituteField.Visible = CanEditInstitute;
            _officeField.Visible = CanEditOffice;
        }

        // Places the visible fields in two columns (like the EDIT ACCOUNT pop up)
        // and returns the content height the chrome needs. The list is built from
        // the role instead of the Visible flag because a control inside a form
        // that is not shown yet reports Visible = false.
        private int LayoutFields()
        {
            List<DialogField> fields = new List<DialogField>
            {
                _nameField,
                _usernameField
            };

            if (CanEditContact)
            {
                fields.Add(_contactField);
            }

            if (CanEditAddress)
            {
                fields.Add(_addressField);
            }

            if (CanEditSchoolId)
            {
                fields.Add(_schoolIdField);
            }

            // INSTITUTE comes before PROGRAM because a program belongs to an
            // institute, so the program list follows the institute choice.
            if (CanEditInstitute)
            {
                fields.Add(_instituteField);
            }

            if (CanEditProgram)
            {
                fields.Add(_programField);
            }

            if (CanEditYearLevel)
            {
                fields.Add(_yearLevelField);
            }

            if (CanEditOffice)
            {
                fields.Add(_officeField);
            }

            for (int index = 0; index < fields.Count; index++)
            {
                int column = index % 2;
                int row = index / 2;

                fields[index].Location = new Point(
                    column == 0 ? LeftColumn : RightColumn,
                    FirstFieldTop + (row * FieldStep));
                fields[index].Size = new Size(ColumnWidth, FieldHeight);
            }

            int rows = (fields.Count + 1) / 2;
            return FirstFieldTop + (rows * FieldStep) + 18;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            ClearErrors();

            string name = _nameField.Value.Trim();
            string username = _usernameField.Value.Trim();
            string program = _programField.Value.Trim();
            string institute = SelectedInstituteCode;

            if (name.Length == 0)
            {
                Fail(_nameField, "Full name is required.");
                return;
            }

            if (username.Length == 0)
            {
                Fail(_usernameField, "Username is required.");
                return;
            }

            if (username.IndexOf(' ') >= 0)
            {
                Fail(_usernameField, "Username cannot contain spaces.");
                return;
            }

            if (CanEditSchoolId && _schoolIdField.Value.Trim().Length == 0)
            {
                Fail(_schoolIdField, "School ID is required for a student.");
                return;
            }

            if (CanEditInstitute && institute.Length == 0)
            {
                Fail(_instituteField, "Institute is required.");
                return;
            }

            if (CanEditProgram && (program.Length == 0 ||
                                   string.Equals(program, ProgramPlaceholder, StringComparison.OrdinalIgnoreCase)))
            {
                Fail(_programField, "Program is required for a student.");
                return;
            }

            if (CanEditOffice && _officeField.Value.Trim().Length == 0)
            {
                Fail(_officeField, "Office is required for a staff account.");
                return;
            }

            FullName = name;
            Username = username;
            SchoolId = CanEditSchoolId ? _schoolIdField.Value.Trim() : string.Empty;
            Program = CanEditProgram ? program : string.Empty;
            YearLevel = CanEditYearLevel ? _yearLevelField.Value.Trim() : string.Empty;
            Institute = CanEditInstitute ? institute : string.Empty;
            ContactNumber = CanEditContact ? _contactField.Value.Trim() : string.Empty;
            Address = CanEditAddress ? _addressField.Value.Trim() : string.Empty;
            Office = CanEditOffice ? _officeField.Value.Trim() : string.Empty;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ClearErrors()
        {
            _nameField.ClearError();
            _usernameField.ClearError();
            _schoolIdField.ClearError();
            _programField.ClearError();
            _instituteField.ClearError();
            _officeField.ClearError();
        }

        private static void Fail(DialogField field, string message)
        {
            field.ShowError(message);
            field.FocusInput();
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
