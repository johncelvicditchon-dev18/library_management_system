using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
namespace oop_project
{
    public partial class Signup : authentication
    {
        protected override string FormName
        {
            get { return "SIGNUP"; }
        }

        // The institute dropdown is filled from `institute_table`; this list is only used
        // when MySQL is unreachable so the form can still be seen.
        private static readonly string[] FallbackInstitutes = { "ICS", "IAS", "ICJE", "IBFS", "IHS", "ITE" };
        private static readonly string[] YearLevels = { "1ST YEAR", "2ND YEAR", "3RD YEAR", "4TH YEAR" };

        // institute code -> institute_id, learned from institute_table at start up.
        private readonly Dictionary<string, int> _instituteIds = new Dictionary<string, int>();
        private readonly List<ProgramRow> _programs = new List<ProgramRow>();

        // LIBRARY STAFF PROFILE: the LIBRARIAN (system admin) and STAFF roles also
        // record the office the person works in. The OFFICE row is serialized in
        // signup.designer.cs so it shows on the design surface; the role driven
        // layout below still moves and hides it like the other designer fields.

        // A dropdown that has not been answered yet shows a hint instead of an empty box.
        private const string RolePlaceholder = "SELECT ROLE";
        private const string YearPlaceholder = "SELECT YEAR";
        private const string InstitutePlaceholder = "SELECT INSTITUTE";

        // LAYOUT: the card uses a 600px content grid.
        //   THIRD = 185px (three per row), HALF = 290px (two per row), WIDE = 600px.
        private const int CardLeftPadding = 50;
        private const int ThirdWidth = 185;
        private const int HalfWidth = 290;
        private const int WideWidth = 600;
        private const int Gutter = 20;
        private const int RowStep = 96;
        private const int FieldOffset = 22;
        private const int FieldHeight = 44;
        private const int ErrorOffset = 68;
        private const int ErrorHeight = 16;
        private const int ButtonGap = 14;
        private const int ButtonHeight = 52;
        private const int BlockTop = 100;
        private const int BlockBottom = 731;

        // How much room a field takes in a row of the grid.
        private enum FieldSpan
        {
            Third,
            Half,
            Wide
        }

        public Signup()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            signup_label.BringToFront();

            // The role list comes from the factory, so a role added there shows up here.
            role_option.Items.Clear();
            role_option.Items.Add(RolePlaceholder);
            role_option.Items.AddRange(PersonFactory.Roles);
            role_option.SelectedIndex = 0;
            LoadInstitutes();
            yearLevel_option.Items.Clear();
            yearLevel_option.Items.Add(YearPlaceholder);
            yearLevel_option.Items.AddRange(YearLevels);
            yearLevel_option.SelectedIndex = 0;
            LoadPrograms();

            fullname_input.TextChanged += text_input_to_uppercase;
            // WARNING: every input keeps its own red message under the field.
            fullname_input.Leave += fullname_input_Leave;
            role_option.SelectedIndexChanged += role_option_SelectedIndexChanged;
            role_option.Leave += role_option_Leave;
            lastName_input.TextChanged += text_input_to_uppercase;
            lastName_input.Leave += lastName_input_Leave;
            firstName_input.TextChanged += text_input_to_uppercase;
            firstName_input.Leave += firstName_input_Leave;
            middleInitial_input.TextChanged += middle_initial_input_TextChanged;
            schoolId_input.TextChanged += text_input_to_uppercase;
            schoolId_input.Leave += schoolId_input_Leave;
            program_option.Leave += program_option_Leave;
            yearLevel_option.Leave += yearLevel_option_Leave;
            yearLevel_option.SelectedIndexChanged += yearLevel_option_SelectedIndexChanged;
            institute_option.Leave += institute_option_Leave;
            institute_option.SelectedIndexChanged += institute_option_SelectedIndexChanged;
            contact_input.TextChanged += contact_input_TextChanged;
            contact_input.Leave += contact_input_Leave;
            address_input.TextChanged += address_input_TextChanged;
            address_input.Leave += address_input_Leave;
            office_input.TextChanged += text_input_to_uppercase;
            office_input.Leave += office_input_Leave;
            username_input.TextChanged += username_input_TextChanged;
            username_input.Leave += username_input_Leave;
            password_input.TextChanged += password_input_TextChanged;
            password_input.Leave += password_input_Leave;
            confirm_input.TextChanged += confirm_input_TextChanged;
            confirm_input.Leave += confirm_input_Leave;

            // ROLE DRIVEN FORM: only the fields that belong to the chosen role are shown.
            ApplyRoleFields();

            // FULL SCREEN: the sign up page opens maximised and F11 toggles a borderless view.
            ScreenMode.Apply(this);
        }

        // Looks the institutes up in the database so signup, dashboard and reports
        // all use the same codes. Falls back to the built-in list when MySQL is down.
        private void LoadInstitutes()
        {
            List<string> codes = new List<string>();
            _instituteIds.Clear();

            try
            {
                using (MySqlConnection conn = OpenConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT `institute_id`, `institute_code` FROM `institute_table` ORDER BY `institute_id`;", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read() && reader.FieldCount > 1)
                        {
                            long id = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader.GetValue(0), CultureInfo.InvariantCulture);
                            string code = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty;
                            code = code.Trim().ToUpperInvariant();
                            if (code.Length == 0) { continue; }
                            codes.Add(code);
                            if (id > 0) { _instituteIds[code] = (int)id; }
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

            institute_option.Items.Clear();
            institute_option.Items.Add(InstitutePlaceholder);
            institute_option.Items.AddRange(codes.ToArray());
            institute_option.SelectedIndex = 0;
        }

        // PROGRAM LOOKUP: program_table owns the program names and institute_table
        // owns the institutes; student_info_table / faculty_info_table only store
        // program_id, so a program is written once and reused by every account.
        private void LoadPrograms()
        {
            _programs.Clear();

            try
            {
                using (MySqlConnection conn = OpenConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT `program_id`, `program_name`, `institute_id` FROM `program_table` ORDER BY `program_name`;", conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read() && reader.FieldCount > 2)
                        {
                            ProgramRow row = new ProgramRow();
                            row.Id = Convert.ToInt64(reader.GetValue(0), CultureInfo.InvariantCulture);
                            row.Name = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture) ?? string.Empty;
                            row.InstituteId = Convert.ToInt64(reader.GetValue(2), CultureInfo.InvariantCulture);
                            if (row.Name.Length > 0) { _programs.Add(row); }
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                _programs.Clear();
            }

            RefreshProgramOptions();
        }

        // Shows the programs of the chosen institute. The box stays editable, so a
        // program that is not listed yet can be typed and is registered on submit.
        private void RefreshProgramOptions()
        {
            string current = program_option.Text;
            int instituteId = SelectedInstituteId;

            program_option.BeginUpdate();
            program_option.Items.Clear();
            foreach (ProgramRow row in _programs)
            {
                if (instituteId == 0 || row.InstituteId == instituteId)
                {
                    program_option.Items.Add(row);
                }
            }
            program_option.EndUpdate();
            program_option.Text = current;
        }

        private int SelectedInstituteId
        {
            get
            {
                string code = institute_option.SelectedIndex <= 0
                    ? string.Empty
                    : (institute_option.Text ?? string.Empty).Trim().ToUpperInvariant();
                int id;
                return _instituteIds.TryGetValue(code, out id) ? id : 0;
            }
        }

        private sealed class ProgramRow
        {
            public long Id;
            public string Name = string.Empty;
            public long InstituteId;

            public override string ToString()
            {
                return Name;
            }
        }

        // Keeps the card, the hero and the fields in the same composition when maximised.
        // The inputs are children of the card now, so moving the card moves the whole form.
        private Control[]? _layoutControls;

        private Control[] LayoutControls()
        {
            return _layoutControls ??= new Control[]
            {
                roundedCard1,
                lrcLogoBadge1,
                brandLabel,
                taglineLabel
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CaptureDesignLayout(new Size(1482, 853), LayoutControls());
            ApplyCenteredLayout(gradientPanel1, LayoutControls());
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyCenteredLayout(gradientPanel1, LayoutControls());
        }

        // ENCAPSULATION: one input + its label + its own warning line are treated
        // as a single row item, so the layout code never repeats the same coordinates.
        private sealed class SignupField
        {
            public SignupField(Label label, FieldPanel field, Label error, FieldSpan span)
            {
                Label = label;
                Field = field;
                Error = error;
                Span = span;
            }

            public Label Label { get; }
            public FieldPanel Field { get; }
            public Label Error { get; }
            public FieldSpan Span { get; }
        }

        private string SelectedRole
        {
            get
            {
                return role_option.SelectedIndex <= 0
                    ? string.Empty
                    : (role_option.Text ?? string.Empty).Trim().ToUpperInvariant();
            }
        }

        private bool IsStudent
        {
            get { return SelectedRole == "STUDENT"; }
        }

        private bool IsFaculty
        {
            get { return SelectedRole == "FACULTY"; }
        }

        private bool IsStaff
        {
            get { return SelectedRole == "STAFF"; }
        }

        private bool IsLibrarian
        {
            get { return SelectedRole == "LIBRARIAN"; }
        }

        // Both library roles keep an office, so they share the same input.
        private bool HasOfficeProfile
        {
            get { return IsStaff || IsLibrarian; }
        }

        // Shows the fields that belong to the selected role and hides the rest.
        // STUDENT gets the full profile, every other role gets one FULL NAME.
        private void ApplyRoleFields()
        {
            bool student = IsStudent;
            bool faculty = IsFaculty;

            ShowField(lastNameLabel, lastNameField, lastNameError, student);
            ShowField(firstNameLabel, firstNameField, firstNameError, student);
            ShowField(middleInitialLabel, middleInitialField, middleInitialError, student);
            ShowField(schoolIdLabel, schoolIdField, schoolIdError, student);
            ShowField(yearLevelLabel, yearLevelField, yearLevelError, student);
            ShowField(contactLabel, contactField, contactError, student);
            ShowField(addressLabel, addressField, addressError, student);
            ShowField(programLabel, programField, programError, student || faculty);
            ShowField(instituteLabel, instituteField, instituteError, student || faculty);
            ShowField(fullNameLabel, fullNameField, fullNameError, !student);
            ShowField(officeLabel, officeField, officeError, HasOfficeProfile);

            ApplyFieldLayout();
        }

        private static void ShowField(Label label, FieldPanel field, Label error, bool visible)
        {
            label.Visible = visible;
            field.Visible = visible;

            if (!visible)
            {
                // A hidden field must not keep an old value or an old warning.
                ClearField(field);
                SetFieldError(error, field, null);
            }
        }

        private static void ClearField(FieldPanel field)
        {
            if (field.Controls.Count == 0)
            {
                return;
            }

            Control input = field.Controls[0];
            if (input is TextBox textBox)
            {
                textBox.Clear();
            }
            else if (input is ComboBox comboBox)
            {
                if (comboBox.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    // Index 0 is the placeholder ("SELECT ..."), not a real answer.
                    comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
                }
                else
                {
                    // PROGRAM is editable, so it is cleared by emptying the text.
                    comboBox.Text = string.Empty;
                }
            }
        }

        // LAYOUT: packs the visible fields into rows of the 600px grid and centres
        // the block vertically inside the card, so a 6-row student form and a 3-row
        // staff form both look deliberate instead of leaving a hole in the middle.
        private void ApplyFieldLayout()
        {
            List<SignupField> items = VisibleFields();
            List<List<SignupField>> rows = new List<List<SignupField>>();
            List<SignupField> currentRow = new List<SignupField>();
            int used = 0;

            foreach (SignupField item in items)
            {
                int width = SpanWidth(item.Span);

                if (currentRow.Count > 0 && used + Gutter + width > WideWidth)
                {
                    rows.Add(currentRow);
                    currentRow = new List<SignupField>();
                    used = 0;
                }

                used += (currentRow.Count == 0 ? width : Gutter + width);
                currentRow.Add(item);
            }

            if (currentRow.Count > 0)
            {
                rows.Add(currentRow);
            }

            int blockHeight = (rows.Count - 1) * RowStep + ErrorOffset + ErrorHeight + ButtonGap + ButtonHeight;
            int available = BlockBottom - BlockTop;
            int startY = BlockTop + Math.Max(0, (available - blockHeight) / 2);

            for (int r = 0; r < rows.Count; r++)
            {
                int y = startY + r * RowStep;
                int x = CardLeftPadding;

                foreach (SignupField item in rows[r])
                {
                    int width = SpanWidth(item.Span);

                    item.Label.Location = new Point(x, y);
                    item.Field.Location = new Point(x, y + FieldOffset);
                    item.Field.Size = new Size(width, FieldHeight);
                    item.Error.Location = new Point(x, y + ErrorOffset);
                    item.Error.Size = new Size(width, ErrorHeight);
                    FitInput(item.Field, item.Span == FieldSpan.Wide, width);

                    x += width + Gutter;
                }
            }

            int lastRowY = startY + (rows.Count - 1) * RowStep;
            create_button.Location = new Point(CardLeftPadding, lastRowY + ErrorOffset + ErrorHeight + ButtonGap);
            create_button.Size = new Size(WideWidth, ButtonHeight);
        }

        private static int SpanWidth(FieldSpan span)
        {
            switch (span)
            {
                case FieldSpan.Wide:
                    return WideWidth;
                case FieldSpan.Half:
                    return HalfWidth;
                default:
                    return ThirdWidth;
            }
        }

        // Keeps the text box / dropdown inside its rounded panel. The inner width is
        // derived from the panel so a wider column never leaves a gap or clips the input.
        private static void FitInput(FieldPanel field, bool wide, int width)
        {
            if (field.Controls.Count == 0)
            {
                return;
            }

            Control input = field.Controls[0];
            if (input is ComboBox comboBox)
            {
                comboBox.Location = new Point(wide ? 18 : 12, 5);
                comboBox.Size = new Size(width - (wide ? 36 : 24), 33);
            }
            else
            {
                input.Location = new Point(wide ? 22 : 16, 9);
                input.Size = new Size(width - (wide ? 44 : 32), 26);
            }
        }

        // The fields that are shown for the selected role, in reading order.
        private List<SignupField> VisibleFields()
        {
            List<SignupField> items = new List<SignupField>
            {
                new SignupField(roleLabel, roleField, roleError, FieldSpan.Wide)
            };

            if (IsStudent)
            {
                items.Add(new SignupField(lastNameLabel, lastNameField, lastNameError, FieldSpan.Third));
                items.Add(new SignupField(firstNameLabel, firstNameField, firstNameError, FieldSpan.Third));
                items.Add(new SignupField(middleInitialLabel, middleInitialField, middleInitialError, FieldSpan.Third));
                items.Add(new SignupField(schoolIdLabel, schoolIdField, schoolIdError, FieldSpan.Third));
                items.Add(new SignupField(yearLevelLabel, yearLevelField, yearLevelError, FieldSpan.Third));
                items.Add(new SignupField(contactLabel, contactField, contactError, FieldSpan.Third));
                items.Add(new SignupField(programLabel, programField, programError, FieldSpan.Half));
                items.Add(new SignupField(instituteLabel, instituteField, instituteError, FieldSpan.Half));
                items.Add(new SignupField(addressLabel, addressField, addressError, FieldSpan.Wide));
            }
            else
            {
                items.Add(new SignupField(fullNameLabel, fullNameField, fullNameError, FieldSpan.Wide));

                if (HasOfficeProfile)
                {
                    items.Add(new SignupField(officeLabel, officeField, officeError, FieldSpan.Wide));
                }

                if (IsFaculty)
                {
                    items.Add(new SignupField(programLabel, programField, programError, FieldSpan.Half));
                    items.Add(new SignupField(instituteLabel, instituteField, instituteError, FieldSpan.Half));
                }
            }

            items.Add(new SignupField(usernameLabel, usernameField, usernameError, FieldSpan.Third));
            items.Add(new SignupField(passwordLabel, passwordField, passwordError, FieldSpan.Third));
            items.Add(new SignupField(confirmLabel, confirmField, confirmError, FieldSpan.Third));
            return items;
        }

        private bool isFormattingName;
        private void text_input_to_uppercase(object? sender, EventArgs e)
        {
            TextBox? box = sender as TextBox;
            if (box == null)
            {
                return;
            }

            if (!isFormattingName)
            {
                string current = box.Text;
                string upper = current.ToUpperInvariant();
                if (upper != current)
                {
                    int caret = box.SelectionStart;
                    isFormattingName = true;
                    try
                    {
                        box.Text = upper;
                        box.SelectionStart = caret > upper.Length ? upper.Length : caret;
                    }
                    finally
                    {
                        isFormattingName = false;
                    }
                }
            }

            if (box == fullname_input && fullNameError.Visible) { ValidateFullName(); }
            else if (box == lastName_input && lastNameError.Visible) { ValidateLastName(); }
            else if (box == firstName_input && firstNameError.Visible) { ValidateFirstName(); }
            else if (box == schoolId_input && schoolIdError.Visible) { ValidateSchoolId(); }
            else if (box == office_input && officeError.Visible) { ValidateOffice(); }
        }

        // The middle initial is a single letter, so it gets its own tiny formatter.
        private void middle_initial_input_TextChanged(object? sender, EventArgs e)
        {
            string current = middleInitial_input.Text;
            string upper = current.ToUpperInvariant();
            int caret = middleInitial_input.SelectionStart;

            if (upper != current)
            {
                middleInitial_input.Text = upper;
                middleInitial_input.SelectionStart = caret > upper.Length ? upper.Length : caret;
            }
        }

        // ENCAPSULATION: each input owns its own validation, so the warning is shown
        // under the exact field that is wrong instead of one shared dialog.
        private bool ValidateFullName()
        {
            if (IsStudent)
            {
                SetFieldError(fullNameError, fullNameField, null);
                return true;
            }
            if (fullname_input.Text.Trim().Length == 0)
            {
                SetFieldError(fullNameError, fullNameField, "Full name is required");
                return false;
            }
            SetFieldError(fullNameError, fullNameField, null);
            return true;
        }
        private bool ValidateRole()
        {
            if (role_option.SelectedIndex <= 0)
            {
                SetFieldError(roleError, roleField, "Role is required");
                return false;
            }
            SetFieldError(roleError, roleField, null);
            return true;
        }

        // OFFICE belongs to the LIBRARIAN / STAFF profile, so only those signups
        // require it.
        private bool ValidateOffice()
        {
            if (!HasOfficeProfile)
            {
                SetFieldError(officeError, officeField, null);
                return true;
            }
            if (office_input.Text.Trim().Length == 0)
            {
                SetFieldError(officeError, officeField, "Office is required");
                return false;
            }
            SetFieldError(officeError, officeField, null);
            return true;
        }
        private bool ValidateLastName()
        {
            if (!IsStudent)
            {
                SetFieldError(lastNameError, lastNameField, null);
                return true;
            }
            if (lastName_input.Text.Trim().Length == 0)
            {
                SetFieldError(lastNameError, lastNameField, "Last name is required");
                return false;
            }
            SetFieldError(lastNameError, lastNameField, null);
            return true;
        }
        private bool ValidateFirstName()
        {
            if (!IsStudent)
            {
                SetFieldError(firstNameError, firstNameField, null);
                return true;
            }
            if (firstName_input.Text.Trim().Length == 0)
            {
                SetFieldError(firstNameError, firstNameField, "First name is required");
                return false;
            }
            SetFieldError(firstNameError, firstNameField, null);
            return true;
        }
        private bool ValidateSchoolId()
        {
            if (!IsStudent)
            {
                SetFieldError(schoolIdError, schoolIdField, null);
                return true;
            }
            if (schoolId_input.Text.Trim().Length == 0)
            {
                SetFieldError(schoolIdError, schoolIdField, "School ID is required");
                return false;
            }
            SetFieldError(schoolIdError, schoolIdField, null);
            return true;
        }
        // PROGRAM is required for a student and optional for a faculty member.
        private bool ValidateProgram()
        {
            if (!IsStudent && !IsFaculty)
            {
                SetFieldError(programError, programField, null);
                return true;
            }
            if (IsStudent && program_option.Text.Trim().Length == 0)
            {
                SetFieldError(programError, programField, "Program is required");
                return false;
            }
            SetFieldError(programError, programField, null);
            return true;
        }
        private bool ValidateYearLevel()
        {
            if (!IsStudent)
            {
                SetFieldError(yearLevelError, yearLevelField, null);
                return true;
            }
            if (yearLevel_option.SelectedIndex <= 0)
            {
                SetFieldError(yearLevelError, yearLevelField, "Year level is required");
                return false;
            }
            SetFieldError(yearLevelError, yearLevelField, null);
            return true;
        }
        private bool ValidateInstitute()
        {
            if (!IsStudent && !IsFaculty)
            {
                SetFieldError(instituteError, instituteField, null);
                return true;
            }
            if (institute_option.SelectedIndex <= 0)
            {
                SetFieldError(instituteError, instituteField, "Institute / office is required");
                return false;
            }
            SetFieldError(instituteError, instituteField, null);
            return true;
        }
        private bool ValidateContact()
        {
            if (!IsStudent)
            {
                SetFieldError(contactError, contactField, null);
                return true;
            }
            string text = contact_input.Text.Trim();
            if (text.Length == 0)
            {
                SetFieldError(contactError, contactField, "Contact is required");
                return false;
            }
            if (!IsPhoneNumber(text))
            {
                SetFieldError(contactError, contactField, "Enter a valid number");
                return false;
            }
            SetFieldError(contactError, contactField, null);
            return true;
        }
        private bool ValidateAddress()
        {
            if (!IsStudent)
            {
                SetFieldError(addressError, addressField, null);
                return true;
            }
            if (address_input.Text.Trim().Length == 0)
            {
                SetFieldError(addressError, addressField, "Address is required");
                return false;
            }
            SetFieldError(addressError, addressField, null);
            return true;
        }
        private bool ValidateUsername()
        {
            if (username_input.Text.Trim().Length == 0)
            {
                SetFieldError(usernameError, usernameField, "Username is required");
                return false;
            }
            SetFieldError(usernameError, usernameField, null);
            return true;
        }
        private bool ValidatePassword()
        {
            if (password_input.Text.Length == 0)
            {
                SetFieldError(passwordError, passwordField, "Password is required");
                return false;
            }
            SetFieldError(passwordError, passwordField, null);
            return true;
        }
        private bool ValidateConfirmPassword()
        {
            if (confirm_input.Text.Length == 0)
            {
                SetFieldError(confirmError, confirmField, "Confirm your password");
                return false;
            }
            if (confirm_input.Text != password_input.Text)
            {
                SetFieldError(confirmError, confirmField, "Passwords do not match");
                return false;
            }
            // The two passwords are identical: confirm it in green instead of clearing the row.
            SetFieldSuccess(confirmError, confirmField, "Password match");
            return true;
        }

        // Digits, spaces, dashes and one optional plus sign, 7 to 15 digits.
        private static bool IsPhoneNumber(string text)
        {
            int digits = 0;
            foreach (char c in text)
            {
                if (char.IsDigit(c)) { digits++; continue; }
                if (c == ' ' || c == '-' || c == '+') { continue; }
                return false;
            }
            return digits >= 7 && digits <= 15;
        }

        // The red message appears when the field loses focus while empty,
        // and disappears as soon as the field is corrected.
        private void fullname_input_Leave(object? sender, EventArgs e) { ValidateFullName(); }
        private void role_option_Leave(object? sender, EventArgs e) { ValidateRole(); }
        private void role_option_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (roleError.Visible) { ValidateRole(); }

            // ROLE DRIVEN FORM: picking STUDENT or FACULTY adds the matching inputs.
            ApplyRoleFields();
        }
        private void lastName_input_Leave(object? sender, EventArgs e) { ValidateLastName(); }
        private void firstName_input_Leave(object? sender, EventArgs e) { ValidateFirstName(); }
        private void schoolId_input_Leave(object? sender, EventArgs e) { ValidateSchoolId(); }
        private void office_input_Leave(object? sender, EventArgs e) { ValidateOffice(); }
        private void program_option_Leave(object? sender, EventArgs e) { ValidateProgram(); }
        private void yearLevel_option_Leave(object? sender, EventArgs e) { ValidateYearLevel(); }
        private void yearLevel_option_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (yearLevelError.Visible) { ValidateYearLevel(); }
        }
        private void institute_option_Leave(object? sender, EventArgs e) { ValidateInstitute(); }
        private void institute_option_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (instituteError.Visible) { ValidateInstitute(); }

            // A program belongs to an institute, so the list follows the choice.
            RefreshProgramOptions();
        }
        private void contact_input_TextChanged(object? sender, EventArgs e)
        {
            if (contactError.Visible) { ValidateContact(); }
        }
        private void contact_input_Leave(object? sender, EventArgs e) { ValidateContact(); }
        private void address_input_TextChanged(object? sender, EventArgs e)
        {
            if (addressError.Visible) { ValidateAddress(); }
        }
        private void address_input_Leave(object? sender, EventArgs e) { ValidateAddress(); }
        private void username_input_TextChanged(object? sender, EventArgs e)
        {
            if (usernameError.Visible) { ValidateUsername(); }
        }
        private void username_input_Leave(object? sender, EventArgs e) { ValidateUsername(); }
        private void password_input_TextChanged(object? sender, EventArgs e)
        {
            if (passwordError.Visible) { ValidatePassword(); }
            if (confirm_input.Text.Length > 0 && confirm_input.Text == password_input.Text)
            {
                SetFieldSuccess(confirmError, confirmField, "Password match");
            }
            else if (confirmError.Visible) { ValidateConfirmPassword(); }
        }
        private void password_input_Leave(object? sender, EventArgs e) { ValidatePassword(); }
        private void confirm_input_TextChanged(object? sender, EventArgs e)
        {
            if (confirm_input.Text.Length > 0 && confirm_input.Text == password_input.Text)
            {
                SetFieldSuccess(confirmError, confirmField, "Password match");
            }
            else if (confirmError.Visible) { ValidateConfirmPassword(); }
        }
        private void confirm_input_Leave(object? sender, EventArgs e) { ValidateConfirmPassword(); }


        private void create_button_Click(object sender, EventArgs e)
        {
            bool roleOk = ValidateRole();
            bool fullNameOk = ValidateFullName();
            bool officeOk = ValidateOffice();
            bool lastNameOk = ValidateLastName();
            bool firstNameOk = ValidateFirstName();
            bool schoolIdOk = ValidateSchoolId();
            bool programOk = ValidateProgram();
            bool yearLevelOk = ValidateYearLevel();
            bool instituteOk = ValidateInstitute();
            bool contactOk = ValidateContact();
            bool addressOk = ValidateAddress();
            bool usernameOk = ValidateUsername();
            bool passwordOk = ValidatePassword();
            bool confirmOk = ValidateConfirmPassword();

            if (!roleOk || !fullNameOk || !officeOk || !lastNameOk || !firstNameOk || !schoolIdOk || !programOk ||
                !yearLevelOk || !instituteOk || !contactOk || !addressOk || !usernameOk || !passwordOk || !confirmOk)
            {
                if (!roleOk) { role_option.Focus(); }
                else if (!fullNameOk) { fullname_input.Focus(); }
                else if (!officeOk) { office_input.Focus(); }
                else if (!lastNameOk) { lastName_input.Focus(); }
                else if (!firstNameOk) { firstName_input.Focus(); }
                else if (!schoolIdOk) { schoolId_input.Focus(); }
                else if (!programOk) { program_option.Focus(); }
                else if (!yearLevelOk) { yearLevel_option.Focus(); }
                else if (!instituteOk) { institute_option.Focus(); }
                else if (!contactOk) { contact_input.Focus(); }
                else if (!addressOk) { address_input.Focus(); }
                else if (!usernameOk) { username_input.Focus(); }
                else if (!passwordOk) { password_input.Focus(); }
                else { confirm_input.Focus(); }
                return;
            }

            string role = SelectedRole;
            string username = username_input.Text.Trim();
            string password = password_input.Text;
            string office = HasOfficeProfile ? office_input.Text.Trim() : string.Empty;

            Person person;
            try
            {
                person = PersonFactory.Create(role);
            }
            catch (ArgumentException ex)
            {
                ShowMessage(ex.Message, "INVALID ROLE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // INHERITANCE: only a Student carries the student profile.
            Student? student = person as Student;
            if (student != null)
            {
                student.LastName = lastName_input.Text;
                student.FirstName = firstName_input.Text;
                student.MiddleInitial = middleInitial_input.Text;
                student.SchoolId = schoolId_input.Text;
                student.Program = program_option.Text;
                student.YearLevel = yearLevel_option.Text;
                student.Institute = institute_option.Text;
                student.ContactNumber = contact_input.Text;
                student.Address = address_input.Text;
                person.FullName = student.ComposeDisplayName();
            }
            else
            {
                person.FullName = fullname_input.Text;
                if (IsFaculty)
                {
                    person.Program = program_option.Text;
                    person.Institute = institute_option.Text;
                }
            }
            person.Username = username;

            using (MySqlConnection conn = OpenConnection())
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string accountQuery = "INSERT INTO account_table (`name`, `username`, `password`) VALUES (@name, @username, @password);";
                            long accountId;
                            using (MySqlCommand cmd = new MySqlCommand(accountQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@name", person.FullName);
                                cmd.Parameters.AddWithValue("@username", username);
                                cmd.Parameters.AddWithValue("@password", password);
                                cmd.ExecuteNonQuery();
                                accountId = cmd.LastInsertedId;
                            }

                            string roleQuery = "INSERT INTO account_role_table (account_id, `role`, `status`, date_created) VALUES (@accountId, @role, 'PENDING', UPPER(DATE_FORMAT(NOW(), '%M %e, %Y')));";
                            long roleId;
                            using (MySqlCommand cmd = new MySqlCommand(roleQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@accountId", accountId);
                                cmd.Parameters.AddWithValue("@role", person.Role);
                                cmd.ExecuteNonQuery();
                                roleId = cmd.LastInsertedId;
                            }

                            if (student != null)
                            {
                                InsertStudentProfile(conn, transaction, accountId, student);
                            }
                            else if (IsFaculty)
                            {
                                InsertFacultyProfile(conn, transaction, accountId, person.Program, person.Institute);
                            }
                            else if (IsStaff)
                            {
                                InsertStaffProfile(conn, transaction, accountId, roleId, office);
                            }
                            else if (IsLibrarian)
                            {
                                InsertLibrarianProfile(conn, transaction, accountId, roleId, office);
                            }

                            transaction.Commit();
                            ShowMessage("SUCCESSFULLY CREATED!. NEWLY CREATED ACCOUNT NEED APPROVAL FROM THE ADMIN.", "SIGNUP SUCCESSFUL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Login loginForm = new Login();
                            loginForm.Show();
                            this.Hide();
                        }
                        catch (MySqlException ex) when (ex.Number == 1062)
                        {
                            try { transaction.Rollback(); } catch { }
                            if (ex.Message.IndexOf("school_id", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                ShowMessage("THAT SCHOOL ID IS ALREADY REGISTERED. PLEASE CHECK YOUR ID NUMBER.", "SCHOOL ID TAKEN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                schoolId_input.Focus();
                                schoolId_input.SelectAll();
                            }
                            else
                            {
                                ShowMessage("THAT USERNAME IS ALREADY TAKEN. PLEASE CHOOSE ANOTHER ONE.", "USERNAME TAKEN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                username_input.Focus();
                                username_input.SelectAll();
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { }
                            ShowMessage("FAILED TO CREATE ACCOUNT.\n" + ex.Message, "SIGNUP ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowError("COULD NOT CONNECT TO MYSQL.\n" + ex.Message);
                }
            }
        }

        // NORMALISATION: the student details are stored once, in student_info_table,
        // which points at account_table, institute_table and program_table by key.
        private static void InsertStudentProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, Student student)
        {
            long instituteId = ResolveInstituteId(conn, transaction, student.Institute);
            long programId = ResolveProgramId(conn, transaction, student.Program, instituteId);

            const string sql = "INSERT INTO `student_info_table` " +
                               "(`account_id`, `school_id`, `last_name`, `first_name`, `middle_initial`, `program_id`, `year_level`, `institute_id`, `contact_number`, `address`) " +
                               "VALUES (@accountId, @schoolId, @lastName, @firstName, @middleInitial, @programId, @yearLevel, @instituteId, @contactNumber, @address);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@schoolId", student.SchoolId);
                cmd.Parameters.AddWithValue("@lastName", student.LastName);
                cmd.Parameters.AddWithValue("@firstName", student.FirstName);
                cmd.Parameters.AddWithValue("@middleInitial", string.IsNullOrEmpty(student.MiddleInitial) ? (object)DBNull.Value : student.MiddleInitial);
                cmd.Parameters.AddWithValue("@programId", programId);
                cmd.Parameters.AddWithValue("@yearLevel", student.YearLevel);
                cmd.Parameters.AddWithValue("@instituteId", instituteId);
                cmd.Parameters.AddWithValue("@contactNumber", student.ContactNumber);
                cmd.Parameters.AddWithValue("@address", student.Address);
                cmd.ExecuteNonQuery();
            }
        }

        // NORMALISATION: a faculty member gets one profile row, so the program and
        // institute they picked are not lost (the program stays optional for them).
        private static void InsertFacultyProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, string program, string institute)
        {
            long instituteId = ResolveInstituteId(conn, transaction, institute);
            object programId = string.IsNullOrEmpty(program)
                ? (object)DBNull.Value
                : ResolveProgramId(conn, transaction, program, instituteId);

            const string sql = "INSERT INTO `faculty_info_table` (`account_id`, `program_id`, `institute_id`) " +
                               "VALUES (@accountId, @programId, @instituteId);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@programId", programId);
                cmd.Parameters.AddWithValue("@instituteId", instituteId);
                cmd.ExecuteNonQuery();
            }
        }

        // NORMALISATION: a staff member gets one profile row that keeps the office
        // they were assigned to. The row links to account_table (account_id) and to
        // the STAFF row in account_role_table (role_id), so the role and status are
        // never copied into this table.
        private static void InsertStaffProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, long roleId, string office)
        {
            const string sql = "INSERT INTO `staff_info_table` (`account_id`, `role_id`, `office`) " +
                               "VALUES (@accountId, @roleId, @office);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@roleId", roleId);
                cmd.Parameters.AddWithValue("@office", office.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the librarian (system admin) gets the same profile shape.
        // librarian_info_table links to account_table and account_role_table, so the
        // office is stored once and the LIBRARIAN role stays only in the role table.
        private static void InsertLibrarianProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, long roleId, string office)
        {
            const string sql = "INSERT INTO `librarian_info_table` (`account_id`, `role_id`, `office`) " +
                               "VALUES (@accountId, @roleId, @office);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@roleId", roleId);
                cmd.Parameters.AddWithValue("@office", office.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        // PROGRAM LOOKUP: returns the id of an existing program or registers the
        // typed one for that institute, so the tables keep a single FK instead of text.
        private static long ResolveProgramId(MySqlConnection conn, MySqlTransaction transaction, string name, long instituteId)
        {
            const string sql = "INSERT INTO `program_table` (`program_name`, `institute_id`) VALUES (@name, @instituteId) " +
                               "ON DUPLICATE KEY UPDATE `program_id` = LAST_INSERT_ID(`program_id`);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@name", (name ?? string.Empty).Trim().ToUpperInvariant());
                cmd.Parameters.AddWithValue("@instituteId", instituteId);
                cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
        }

        private static long ResolveInstituteId(MySqlConnection conn, MySqlTransaction transaction, string code)
        {
            const string sql = "SELECT `institute_id` FROM `institute_table` WHERE `institute_code` = @code LIMIT 1;";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@code", code);
                object? result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("UNKNOWN INSTITUTE '" + code + "'.");
                }
                return Convert.ToInt64(result, CultureInfo.InvariantCulture);
            }
        }

        private void signup_label_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.Show();
            this.Hide();
        }
        private void label1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.Show();
            this.Hide();
        }
    }
}
