using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;
using static oop_project.Authentication;
namespace oop_project
{
    public partial class Signup : Form
    {
        private readonly Authentication.WindowLayout _layout = new Authentication.WindowLayout();

        // The institute dropdown is filled from `institute_table`; this list is only used
        // when MySQL is unreachable so the form can still be seen.
        private static readonly string[] FallbackInstitutes = { "ICS", "IAS", "ICJE", "IBFS", "IHS", "ITE" };

        // PROGRAM LIST: the programs of the school, grouped by the institute that
        // offers them. They are shown even before program_table has a row for them
        // (and when MySQL is unreachable), so the common programs are always one
        // click away. Picking one registers it in program_table on submit.
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

        private static readonly string[] YearLevels = { "1ST YEAR", "2ND YEAR", "3RD YEAR", "4TH YEAR" };

        // institute code -> institute_id, learned from institute_table at start up.
        private readonly Dictionary<string, int> _instituteIds = new Dictionary<string, int>();
        private readonly List<ProgramRow> _programs = new List<ProgramRow>();

        // LIBRARY STAFF PROFILE: only the STAFF role records the office the person
        // works in. The LIBRARIAN account is the system admin and no longer asks
        // for an office. The OFFICE row is serialized in signup.designer.cs so it
        // shows on the design surface; the role driven layout hides it when needed.

        // A dropdown that has not been answered yet shows a hint instead of an empty box.
        private const string RolePlaceholder = "SELECT ROLE";
        private const string YearPlaceholder = "SELECT YEAR";
        private const string InstitutePlaceholder = "SELECT INSTITUTE";

        // LAYOUT: the card uses a 600px content grid. Every field is the normal
        // 290px wide, so two fields share a row, and the card height follows the
        // number of rows (a small role form gets a shorter white card).
        private const int CardLeftPadding = 50;
        private const int ColumnWidth = 290;
        private const int ColumnsPerRow = 2;
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
        // Space kept below the button for the footer ("Already have an account?").
        private const int CardFooterHeight = 91;
        private const int CardFooterInset = 54;
        private const int CardTopMargin = 16;

        public Signup()
        {
            InitializeComponent();
            closeButton.Height = closeButton.Width;
            closeButton.BringToFront();
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
        // The built-in list is appended after the database rows, without repeating
        // a program that program_table already returned.
        private void RefreshProgramOptions()
        {
            string current = program_option.Text;
            int instituteId = SelectedInstituteId;
            string instituteCode = SelectedInstituteCode;
            HashSet<string> listed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            program_option.BeginUpdate();
            program_option.Items.Clear();
            foreach (ProgramRow row in _programs)
            {
                if (instituteId == 0 || row.InstituteId == instituteId)
                {
                    program_option.Items.Add(row);
                    listed.Add(row.Name);
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
                        program_option.Items.Add(new ProgramRow { Id = 0, Name = name, InstituteId = instituteId });
                    }
                }
            }
            program_option.EndUpdate();
            program_option.Text = current;
        }

        // The institute code as typed in the dropdown, e.g. ICS. It is used to pick
        // the built-in programs that belong to that institute.
        private string SelectedInstituteCode
        {
            get
            {
                return institute_option.SelectedIndex <= 0
                    ? string.Empty
                    : (institute_option.Text ?? string.Empty).Trim().ToUpperInvariant();
            }
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
        private bool _cardLayoutReady;

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
            _layout.Capture(new Size(1482, 853), LayoutControls());
            _layout.Apply(this, gradientPanel1, LayoutControls());

            // The white card can now be sized and centred for its content.
            _cardLayoutReady = true;
            ApplyFieldLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _layout.Apply(this, gradientPanel1, LayoutControls());

            // RESPONSIVE CARD: keep the card centred when the window changes size.
            if (_cardLayoutReady && roundedCard1 != null)
            {
                ApplyFieldLayout();
            }
        }

        // ENCAPSULATION: one input + its label + its own warning line are treated
        // as a single row item, so the layout code never repeats the same coordinates.
        private sealed class SignupField
        {
            public SignupField(Label label, FieldPanel field, Label error)
            {
                Label = label;
                Field = field;
                Error = error;
            }

            public Label Label { get; }
            public FieldPanel Field { get; }
            public Label Error { get; }
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

        // Only the STAFF role keeps an office. The LIBRARIAN account is the system
        // admin, so signup no longer asks it for an office.
        private bool HasOfficeProfile
        {
            get { return IsStaff; }
        }

        // STUDENT, FACULTY and STAFF are asked for a contact number.
        private bool NeedsContact
        {
            get { return IsStudent || IsFaculty || IsStaff; }
        }

        // Shows the fields that belong to the selected role and hides the rest.
        // STUDENT gets the full profile; FACULTY gives an institute and a contact
        // number; STAFF also gives an office; LIBRARIAN only needs the name.
        private void ApplyRoleFields()
        {
            bool student = IsStudent;

            ShowField(lastNameLabel, lastNameField, lastNameError, student);
            ShowField(firstNameLabel, firstNameField, firstNameError, student);
            ShowField(middleInitialLabel, middleInitialField, middleInitialError, student);
            ShowField(schoolIdLabel, schoolIdField, schoolIdError, student);
            ShowField(yearLevelLabel, yearLevelField, yearLevelError, student);
            ShowField(contactLabel, contactField, contactError, NeedsContact);
            ShowField(addressLabel, addressField, addressError, student);
            ShowField(programLabel, programField, programError, student);
            ShowField(instituteLabel, instituteField, instituteError, student || IsFaculty);
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

        // LAYOUT: every visible field is the same width, so the block is a clean
        // two-column grid. The white card is resized to fit the rows that are
        // actually shown, then centred vertically in the window.
        private void ApplyFieldLayout()
        {
            List<SignupField> items = VisibleFields();
            int rows = Math.Max(1, (items.Count + ColumnsPerRow - 1) / ColumnsPerRow);

            for (int index = 0; index < items.Count; index++)
            {
                int row = index / ColumnsPerRow;
                int column = index % ColumnsPerRow;
                int x = CardLeftPadding + column * (ColumnWidth + Gutter);
                int y = BlockTop + row * RowStep;
                SignupField item = items[index];

                item.Label.Location = new Point(x, y);
                item.Field.Location = new Point(x, y + FieldOffset);
                item.Field.Size = new Size(ColumnWidth, FieldHeight);
                item.Error.Location = new Point(x, y + ErrorOffset);
                item.Error.Size = new Size(ColumnWidth, ErrorHeight);
                FitInput(item.Field, ColumnWidth);
            }

            int buttonTop = BlockTop + (rows - 1) * RowStep + ErrorOffset + ErrorHeight + ButtonGap;
            create_button.Location = new Point(CardLeftPadding, buttonTop);
            create_button.Size = new Size(WideWidth, ButtonHeight);

            // RESPONSIVE CARD: fewer rows make the card shorter, more rows make it
            // taller, and the footer stays pinned to the bottom of the card.
            int cardHeight = buttonTop + ButtonHeight + CardFooterHeight;
            roundedCard1.Height = cardHeight;

            int footerTop = cardHeight - CardFooterInset;
            footerPrompt.Top = footerTop;
            signin_label.Top = footerTop;

            // The window layout owns the card's X. Centre its Y only once the
            // design position has been captured, so the capture stays accurate.
            if (_cardLayoutReady)
            {
                int available = gradientPanel1.ClientSize.Height;
                int top = Math.Max(CardTopMargin, (available - cardHeight) / 2);
                roundedCard1.Top = top;
            }
        }

        // Keeps the text box / dropdown evenly inside its rounded panel.
        private static void FitInput(FieldPanel field, int width)
        {
            if (field.Controls.Count == 0)
            {
                return;
            }

            Control input = field.Controls[0];
            if (input is ComboBox comboBox)
            {
                comboBox.Location = new Point(12, 5);
                comboBox.Size = new Size(width - 24, 33);
            }
            else
            {
                input.Location = new Point(16, 9);
                input.Size = new Size(width - 32, 26);
            }
        }

        // The fields that are shown for the selected role, in reading order.
        // INSTITUTE comes before PROGRAM because a program belongs to an institute.
        private List<SignupField> VisibleFields()
        {
            List<SignupField> items = new List<SignupField>
            {
                new SignupField(roleLabel, roleField, roleError)
            };

            if (IsStudent)
            {
                items.Add(new SignupField(lastNameLabel, lastNameField, lastNameError));
                items.Add(new SignupField(firstNameLabel, firstNameField, firstNameError));
                items.Add(new SignupField(middleInitialLabel, middleInitialField, middleInitialError));
                items.Add(new SignupField(schoolIdLabel, schoolIdField, schoolIdError));
                items.Add(new SignupField(yearLevelLabel, yearLevelField, yearLevelError));
                items.Add(new SignupField(contactLabel, contactField, contactError));
                items.Add(new SignupField(instituteLabel, instituteField, instituteError));
                items.Add(new SignupField(programLabel, programField, programError));
                items.Add(new SignupField(addressLabel, addressField, addressError));
            }
            else
            {
                items.Add(new SignupField(fullNameLabel, fullNameField, fullNameError));

                if (HasOfficeProfile)
                {
                    items.Add(new SignupField(officeLabel, officeField, officeError));
                }

                if (IsFaculty)
                {
                    // FACULTY only has an institute; it is not asked for a program.
                    items.Add(new SignupField(instituteLabel, instituteField, instituteError));
                }

                if (IsFaculty || IsStaff)
                {
                    items.Add(new SignupField(contactLabel, contactField, contactError));
                }
            }

            items.Add(new SignupField(usernameLabel, usernameField, usernameError));
            items.Add(new SignupField(passwordLabel, passwordField, passwordError));
            items.Add(new SignupField(confirmLabel, confirmField, confirmError));
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

        // OFFICE belongs to the STAFF profile only, so only a staff signup
        // requires it.
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
        // PROGRAM belongs to the STUDENT profile only; FACULTY no longer has one.
        private bool ValidateProgram()
        {
            if (!IsStudent)
            {
                SetFieldError(programError, programField, null);
                return true;
            }
            if (program_option.Text.Trim().Length == 0)
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
            if (!NeedsContact)
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
            string contact = NeedsContact ? contact_input.Text.Trim() : string.Empty;

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
                student.ContactNumber = contact;
                student.Address = address_input.Text;
                person.FullName = student.ComposeDisplayName();
            }
            else
            {
                person.FullName = fullname_input.Text;
                person.ContactNumber = contact;
                if (IsFaculty)
                {
                    person.Institute = institute_option.Text;
                }
            }
            person.Username = username;

            using (MySqlConnection conn = OpenConnection())
            {
                try
                {
                    conn.Open();

                    // CONTACT NUMBER: keep the older database in step with the
                    // profile tables before the rows are written.
                    EnsureContactNumberColumns(conn);
                    EnsureProfileColumnsRemoved(conn);

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
                                InsertFacultyProfile(conn, transaction, accountId, person.Institute, contact);
                            }
                            else if (IsStaff)
                            {
                                InsertStaffProfile(conn, transaction, accountId, roleId, office, contact);
                            }
                            else if (IsLibrarian)
                            {
                                InsertLibrarianProfile(conn, transaction, accountId, roleId);
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
                    ShowError("SIGNUP", "COULD NOT CONNECT TO MYSQL.\n" + ex.Message);
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

        // NORMALISATION: a faculty member gets one profile row with the institute
        // they picked and their contact number. FACULTY has no program, so the row
        // no longer stores a program_id.
        private static void InsertFacultyProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, string institute, string contact)
        {
            long instituteId = ResolveInstituteId(conn, transaction, institute);

            const string sql = "INSERT INTO `faculty_info_table` (`account_id`, `institute_id`, `contact_number`) " +
                               "VALUES (@accountId, @instituteId, @contactNumber);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@instituteId", instituteId);
                cmd.Parameters.AddWithValue("@contactNumber", contact);
                cmd.ExecuteNonQuery();
            }
        }

        // NORMALISATION: a staff member gets one profile row that keeps the office
        // they were assigned to and the contact number they gave. The row links to
        // account_table (account_id) and to the STAFF row in account_role_table
        // (role_id), so the role and status are never copied into this table.
        private static void InsertStaffProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, long roleId, string office, string contact)
        {
            const string sql = "INSERT INTO `staff_info_table` (`account_id`, `role_id`, `office`, `contact_number`) " +
                               "VALUES (@accountId, @roleId, @office, @contactNumber);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@roleId", roleId);
                cmd.Parameters.AddWithValue("@office", office.Trim());
                cmd.Parameters.AddWithValue("@contactNumber", contact);
                cmd.ExecuteNonQuery();
            }
        }

        // NORMALISATION: the librarian (system admin) gets a profile row that only
        // links the account to its role row. No office is recorded any more.
        private static void InsertLibrarianProfile(MySqlConnection conn, MySqlTransaction transaction, long accountId, long roleId)
        {
            const string sql = "INSERT INTO `librarian_info_table` (`account_id`, `role_id`) " +
                               "VALUES (@accountId, @roleId);";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@accountId", accountId);
                cmd.Parameters.AddWithValue("@roleId", roleId);
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
            loginForm.FormClosed += (s, args) => Close();
            loginForm.Show();
            this.Hide();
        }
        private void label1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.FormClosed += (s, args) => Close();
            loginForm.Show();
            this.Hide();
        }

        private void closeButton_Click(object? sender, EventArgs e)
        {
            Welcome welcomeForm = new Welcome();
            welcomeForm.Show();
            this.Hide();
        }
    }
}
