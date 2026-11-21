using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
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
        private static readonly string[] YearLevels =
        {
            "1ST YEAR",
            "2ND YEAR",
            "3RD YEAR",
            "4TH YEAR",
            "5TH YEAR"
        };

        // INSTITUTE DROPDOWN: the codes are read from institute_table; this list
        // keeps the registrar's order and is used when MySQL is unreachable.
        private static readonly string[] FallbackInstitutes = { "ICS", "ICJE", "ITE", "IAS", "IBFS", "IHS" };

        // PROGRAM DROPDOWN: the programs of the school grouped by the institute
        // that offers them, the same list the SIGNUP form shows. Picking an
        // institute narrows the program drop down to its own programs.
        private static readonly Dictionary<string, string[]> FallbackPrograms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ICS", new[] { "BSCS" } },
                { "ICJE", new[] { "BS-CRIM", "BSISM" } },
                { "ITE", new[] { "BEED", "BSED-ENGLISH", "BSED-FILIPINO", "BSED-MATH", "BSED-SOCSTUD" } },
                { "IAS", new[] { "AB COMM", "AB ENGLISH", "AB POLSCI" } },
                { "IBFS", new[] { "BSBA-FM", "BSBA-HRM", "BSBA-MM", "BSOA" } },
                { "IHS", new[] { "BS-MIDWIFERY" } }
            };

        // The drop downs open on a hint instead of an empty box, so "not answered"
        // reads clearly and the required checks can treat it as empty.
        private const string InstitutePlaceholder = "SELECT INSTITUTE";
        private const string ProgramPlaceholder = "SELECT PROGRAM";

        // Shared geometry. A DialogField is 88 px tall (caption + box + inline
        // error), so a 100 px pitch keeps a clear gap between one error line and
        // the caption of the field below it.
        private const int FirstRowTop = 30;
        private const int RowPitch = 100;
        private const int FieldHeight = 88;
        private const int GridGap = 24;
        private const int StudentWidth = 900;
        private const int CompactWidth = 520;
        private const int CompactFieldWidth = 448;

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();
        private readonly AccountRow _row;

        // institute code -> institute_id, learned from institute_table, and the
        // program rows of program_table. Both feed the connected drop downs.
        private readonly Dictionary<string, long> _instituteIds =
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private readonly List<SchoolProgram> _programs = new List<SchoolProgram>();

        // The role of the row never changes while the dialog is open, so the three
        // profile flags are resolved once and reused by the layout and the save.
        private readonly bool _isStudent;
        private readonly bool _isFaculty;
        private readonly bool _isStaff;

        // True for a LIBRARIAN (or for an account without a profile row): the dialog
        // only edits the name and the username and explains that below them.
        private readonly bool _showProfileNote;

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
                "ICS",
                "09171234567",
                "SAMPLE ADDRESS"))
        {
        }

        public EditAccountDialog(AccountRow row)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();

            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            _row = row;

            _isStudent = row.HasProfile && string.Equals(row.Role, "STUDENT", StringComparison.OrdinalIgnoreCase);
            _isFaculty = row.HasProfile && string.Equals(row.Role, "FACULTY", StringComparison.OrdinalIgnoreCase);
            _isStaff = string.Equals(row.Role, "STAFF", StringComparison.OrdinalIgnoreCase);
            _showProfileNote = !(_isStudent || _isFaculty || _isStaff);

            LoadRowValues(row);
            LoadSchoolChoices(row);

            // The account tables keep the name and the office in capitals, so both
            // fields convert what is typed (or pasted) as it arrives.
            UppercaseWhileTyping(_nameField.Input);
            UppercaseWhileTyping(_officeField.Input);

            _primaryButton.Click += UpdateAccount_Click;

            ApplyLayout();

            Shown += EditAccountDialog_Shown;
        }

        private void LoadRowValues(AccountRow row)
        {
            _nameField.Value = Clean(row.Name).ToUpperInvariant();
            _usernameField.Value = Clean(row.Username);

            _schoolIdField.Value = Clean(row.SchoolId);

            string yearLevel = Clean(row.YearLevel);
            int yearIndex = Array.FindIndex(YearLevels,
                delegate (string item) { return string.Equals(item, yearLevel, StringComparison.OrdinalIgnoreCase); });

            if (_yearLevelField.ChoiceBox.Items.Count == 0)
            {
                _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            }

            _yearLevelField.ChoiceBox.SelectedIndex = yearIndex >= 0 ? yearIndex : 0;

            _contactField.Value = Clean(row.ContactNumber);
            _officeField.Value = Clean(row.Office).ToUpperInvariant();
        }

        // INSTITUTE + PROGRAM: the two drop downs are connected the same way they
        // are on the SIGNUP form. The institute list comes from institute_table
        // and the program list is rebuilt from program_table every time another
        // institute is chosen.
        private void LoadSchoolChoices(AccountRow row)
        {
            LoadInstitutes(Clean(row.Institute).ToUpperInvariant());
            LoadPrograms(Clean(row.Program));

            _instituteField.ChoiceBox.SelectedIndexChanged += InstituteField_SelectedIndexChanged;
        }

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
                                : Convert.ToInt64(reader.GetValue(0), CultureInfo.InvariantCulture);
                            string code = reader.IsDBNull(1)
                                ? string.Empty
                                : (Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty).Trim().ToUpperInvariant();

                            if (code.Length == 0)
                            {
                                continue;
                            }

                            codes.Add(code);

                            if (id > 0)
                            {
                                _instituteIds[code] = id;
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
            box.Items.AddRange(OrderInstituteCodes(codes).ToArray());

            int index = FindItem(box, current);

            if (index < 0 && current.Length > 0)
            {
                // Keep the stored code visible even if institute_table no longer
                // lists it, so the value is never silently lost.
                box.Items.Add(current);
                index = box.Items.Count - 1;
            }

            box.SelectedIndex = index >= 0 ? index : 0;
        }

        // The registrar shows the institutes in this order, so the drop down does
        // not depend on the order the rows happen to have in institute_table.
        private static List<string> OrderInstituteCodes(List<string> codes)
        {
            List<string> ordered = new List<string>(codes.Count);

            foreach (string preferred in FallbackInstitutes)
            {
                int index = codes.FindIndex(
                    delegate (string code) { return string.Equals(code, preferred, StringComparison.OrdinalIgnoreCase); });

                if (index >= 0)
                {
                    ordered.Add(codes[index]);
                    codes.RemoveAt(index);
                }
            }

            ordered.AddRange(codes);
            return ordered;
        }

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
                            SchoolProgram program = new SchoolProgram();
                            program.Name = reader.IsDBNull(1)
                                ? string.Empty
                                : (Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty).Trim();
                            program.InstituteId = reader.IsDBNull(2)
                                ? 0
                                : Convert.ToInt64(reader.GetValue(2), CultureInfo.InvariantCulture);

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

            RefreshProgramChoices(current, true);
        }

        /// <summary>
        /// Shows the programs of the chosen institute. The stored program is kept
        /// while another institute is edited only when that institute also offers
        /// it, which is what makes the two drop downs behave as one pair.
        /// </summary>
        private void RefreshProgramChoices(string current, bool keepStoredValue)
        {
            long instituteId = SelectedInstituteId();
            string instituteCode = SelectedInstituteCode();
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ComboBox box = _programField.ChoiceBox;
            box.BeginUpdate();
            box.Items.Clear();
            box.Items.Add(ProgramPlaceholder);

            foreach (SchoolProgram program in _programs)
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

            if (index < 0 && keepStoredValue && current.Length > 0 &&
                !string.Equals(current, ProgramPlaceholder, StringComparison.OrdinalIgnoreCase))
            {
                // A program the school already stores but program_table no longer
                // lists stays selected instead of being replaced by the hint.
                box.Items.Add(current);
                index = box.Items.Count - 1;
            }

            box.SelectedIndex = index >= 0 ? index : 0;
        }

        private void InstituteField_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Another institute owns another program list, so a program that the
            // new institute does not offer is dropped rather than kept by mistake.
            RefreshProgramChoices(_programField.ChoiceBox.Text, false);
        }

        private string SelectedInstituteCode()
        {
            ComboBox box = _instituteField.ChoiceBox;
            return box.SelectedIndex <= 0 ? string.Empty : (box.Text ?? string.Empty).Trim().ToUpperInvariant();
        }

        private long SelectedInstituteId()
        {
            long id;
            return _instituteIds.TryGetValue(SelectedInstituteCode(), out id) ? id : 0;
        }

        private string SelectedProgram()
        {
            ComboBox box = _programField.ChoiceBox;
            return box.SelectedIndex <= 0 ? string.Empty : (box.Text ?? string.Empty).Trim();
        }

        private static int FindItem(ComboBox box, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            for (int i = 0; i < box.Items.Count; i++)
            {
                if (string.Equals(Convert.ToString(box.Items[i], CultureInfo.InvariantCulture), text,
                                  StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private sealed class SchoolProgram
        {
            public string Name = string.Empty;
            public long InstituteId;
        }

        private void EditAccountDialog_Shown(object? sender, EventArgs e)
        {
            _nameField.FocusInput();
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "—" ? string.Empty : value.Trim();
        }

        /// <summary>
        /// CharacterCasing uppercases typing; this second pass also turns pasted or
        /// programmed text into capitals and keeps the caret in place.
        /// </summary>
        private static void UppercaseWhileTyping(TextBox box)
        {
            box.CharacterCasing = CharacterCasing.Upper;
            box.TextChanged += Uppercase_TextChanged;
        }

        private static void Uppercase_TextChanged(object? sender, EventArgs e)
        {
            if (sender is not TextBox box)
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

        // The profile block only makes sense for a role that owns a profile row in
        // MySQL: student_info_table, faculty_info_table or staff_info_table.
        // FACULTY only keeps the institute (no program) and the LIBRARIAN only
        // keeps the account links (no office).
        private void ApplyLayout()
        {
            _schoolIdField.Visible = _isStudent;
            _yearLevelField.Visible = _isStudent;
            _contactField.Visible = _isStudent;
            _programField.Visible = _isStudent;
            _instituteField.Visible = _isStudent || _isFaculty;
            _officeField.Visible = _isStaff;
            _profileNote.Visible = _showProfileNote;

            if (_isStudent)
            {
                LayoutStudentGrid();
            }
            else
            {
                LayoutCompactColumn();
            }
        }

        /// <summary>
        /// STUDENT fills a two column grid of equal fields: six fields in three
        /// rows plus the contact number closing the grid across both columns. The
        /// institute sits above the program, because choosing it narrows the
        /// program drop down.
        /// </summary>
        private void LayoutStudentGrid()
        {
            int margin = DialogChrome.SideMargin;
            int columnWidth = (StudentWidth - (margin * 2) - GridGap) / 2;
            int left = margin;
            int right = margin + columnWidth + GridGap;
            int row2 = FirstRowTop + RowPitch;
            int row3 = FirstRowTop + (RowPitch * 2);
            int row4 = FirstRowTop + (RowPitch * 3);

            _nameField.SetBounds(left, FirstRowTop, columnWidth, FieldHeight);
            _schoolIdField.SetBounds(right, FirstRowTop, columnWidth, FieldHeight);

            _usernameField.SetBounds(left, row2, columnWidth, FieldHeight);
            _instituteField.SetBounds(right, row2, columnWidth, FieldHeight);

            _yearLevelField.SetBounds(left, row3, columnWidth, FieldHeight);
            _programField.SetBounds(right, row3, columnWidth, FieldHeight);

            _contactField.SetBounds(left, row4, (columnWidth * 2) + GridGap, FieldHeight);

            int contentHeight = row4 + FieldHeight + FirstRowTop;

            ApplyChrome(StudentWidth, contentHeight,
                "Change the account or its student profile, then press UPDATE ACCOUNT.");
        }

        /// <summary>
        /// LIBRARIAN, FACULTY and STAFF edit the account plus at most one profile
        /// value, so they get a small single column dialog instead of the wide
        /// student grid.
        /// </summary>
        private void LayoutCompactColumn()
        {
            int left = (CompactWidth - CompactFieldWidth) / 2;
            int second = FirstRowTop + RowPitch;
            int third = FirstRowTop + (RowPitch * 2);

            _nameField.SetBounds(left, FirstRowTop, CompactFieldWidth, FieldHeight);
            _usernameField.SetBounds(left, second, CompactFieldWidth, FieldHeight);

            DialogField? extraField = null;

            if (_isFaculty)
            {
                extraField = _instituteField;
            }
            else if (_isStaff)
            {
                extraField = _officeField;
            }

            int contentHeight;

            if (extraField != null)
            {
                extraField.SetBounds(left, third, CompactFieldWidth, FieldHeight);
                contentHeight = third + FieldHeight + FirstRowTop;
            }
            else
            {
                contentHeight = second + FieldHeight + FirstRowTop;
            }

            if (_showProfileNote)
            {
                _profileNote.Text = _row.IsLibrarian
                    ? "THE LIBRARIAN ACCOUNT HAS NO PROGRAM, YEAR LEVEL, INSTITUTE OR\r\nOFFICE. ONLY THE NAME AND USERNAME CAN BE EDITED."
                    : "THIS ACCOUNT HAS NO STUDENT, FACULTY OR STAFF PROFILE ROW,\r\nSO ONLY THE NAME AND USERNAME CAN BE EDITED.";

                int noteTop = second + FieldHeight + 14;
                _profileNote.SetBounds(left, noteTop, CompactFieldWidth, 52);
                contentHeight = noteTop + 52 + FirstRowTop;
            }

            ApplyChrome(CompactWidth, contentHeight,
                "Change the details, then press UPDATE ACCOUNT.");
        }

        private void ApplyChrome(int width, int contentHeight, string subtitle)
        {
            int height = DialogChrome.HeaderHeight + contentHeight + DialogChrome.FooterHeight;

            DialogChrome.ApplyParameters(this, "EDIT ACCOUNT", subtitle, width, height,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);
            DialogChrome.ApplyCompactHeader(this, _titleLabel, _subtitleLabel, _closeButton);
            DialogChrome.ApplyRoundedRegion(this);
        }

        private void UpdateAccount_Click(object? sender, EventArgs e)
        {
            _nameField.ClearError();
            _usernameField.ClearError();
            _schoolIdField.ClearError();
            _programField.ClearError();
            _instituteField.ClearError();
            _officeField.ClearError();

            string name = _nameField.Value.Trim().ToUpperInvariant();
            string username = _usernameField.Value.Trim();

            // Role and status are intentionally not editable in this dialog. They
            // are read from the selected row and written back unchanged, so the
            // ACCESS action remains the only place that changes them.
            string role = _row.Role;
            string status = _row.Status;
            string schoolId = _schoolIdField.Value.Trim();
            string program = SelectedProgram();
            string institute = SelectedInstituteCode();
            string contact = _contactField.Value.Trim();
            string office = _officeField.Value.Trim().ToUpperInvariant();

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

            if (_isStudent)
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

            if ((_isStudent || _isFaculty) && institute.Length == 0)
            {
                _instituteField.ShowError("Institute is required.");
                isValid = false;
            }

            if (_isStaff && office.Length == 0)
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
                else if (_isStudent && schoolId.Length == 0)
                {
                    _schoolIdField.FocusInput();
                }
                else if (_isStudent && program.Length == 0)
                {
                    _programField.FocusInput();
                }
                else if (_isStaff && office.Length == 0)
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
            Program = _isStudent ? program : string.Empty;
            YearLevel = _yearLevelField.ChoiceBox.Text;
            Institute = institute;
            ContactNumber = contact;
            Office = _isStaff ? office : string.Empty;

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
