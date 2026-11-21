using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
        private const int PhotoSectionHeight = 128;

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

        private AvatarBadge _photoPreview = null!;
        private Label _photoHeading = null!;
        private Label _photoHint = null!;
        private RoundedButton _uploadPhotoButton = null!;
        private RoundedButton _removePhotoButton = null!;
        private Image? _photoImage;
        private byte[]? _photoBytes;
        private bool _photoChanged;

        public string FullName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string SchoolId { get; private set; } = string.Empty;
        public string Program { get; private set; } = string.Empty;
        public string YearLevel { get; private set; } = string.Empty;
        public string Institute { get; private set; } = string.Empty;
        public string ContactNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string Office { get; private set; } = string.Empty;

        /// <summary>The new portrait as a small JPEG, or null when it was removed.</summary>
        public byte[]? ProfilePicture { get; private set; }

        /// <summary>True only when the photo was uploaded or removed in this dialog.</summary>
        public bool ProfilePictureChanged { get; private set; }

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

            // FULL NAME, ADDRESS and the STAFF OFFICE are kept in uppercase, so
            // the text boxes convert each typed letter while the rest of the app
            // keeps using the same uppercase style. CharacterCasing only affects
            // keys the user types, so the stored values are upper cased as well.
            _nameField.Input.CharacterCasing = CharacterCasing.Upper;
            _addressField.Input.CharacterCasing = CharacterCasing.Upper;
            _officeField.Input.CharacterCasing = CharacterCasing.Upper;

            _nameField.Value = Clean(row.Name).ToUpperInvariant();
            _usernameField.Value = Clean(row.Username);
            _contactField.Value = Clean(row.ContactNumber);
            _addressField.Value = Clean(row.Address).ToUpperInvariant();
            _schoolIdField.Value = Clean(row.SchoolId);
            _officeField.Value = Clean(row.Office).ToUpperInvariant();

            // INSTITUTE before PROGRAM: the program list follows the institute,
            // exactly like the signup form.
            LoadInstitutes(Clean(row.Institute));
            LoadPrograms(Clean(row.Program));
            _instituteField.ChoiceBox.SelectedIndexChanged += InstituteField_SelectedIndexChanged;

            EnsureYearLevels(row.YearLevel);

            ConfigureFields();
            BuildPhotoSection();
            LoadPhoto(row.ProfilePicture);

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
            get { return string.Equals(_row.Role, "STAFF", StringComparison.OrdinalIgnoreCase); }
        }

        private bool IsLibrarian
        {
            get { return string.Equals(_row.Role, "LIBRARIAN", StringComparison.OrdinalIgnoreCase); }
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

        // Every role may add a portrait, including the librarian. The picture is
        // stored on the account, so it follows the sign in on every dashboard.
        private bool CanEditPhoto
        {
            get { return true; }
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

        // ------------------------------------------------------- PROFILE PICTURE

        /// <summary>
        /// Builds the picture block that sits above the text fields: a round
        /// preview, the hint and the UPLOAD / REMOVE actions.
        /// </summary>
        private void BuildPhotoSection()
        {
            _photoHeading = new Label
            {
                AutoSize = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "PROFILE PICTURE",
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = CanEditPhoto
            };

            _photoPreview = new AvatarBadge
            {
                BackColor = Color.Transparent,
                FillTopColor = Color.FromArgb(34, 197, 94),
                FillBottomColor = Color.FromArgb(4, 90, 66),
                Initials = InitialsOf(_row.Name),
                InitialsColor = Color.White,
                RingColor = Color.FromArgb(45, 4, 120, 87),
                Size = new Size(92, 92),
                TabStop = false,
                Visible = CanEditPhoto
            };

            _photoHint = new Label
            {
                AutoSize = false,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "ADD A CLEAR PORTRAIT SO THE LIBRARY STAFF CAN RECOGNISE YOU. A SQUARE PHOTO WORKS BEST.",
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = CanEditPhoto
            };

            _uploadPhotoButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 10,
                Enabled = CanEditPhoto,
                FlatAppearance = { BorderSize = 0 },
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(21, 128, 61),
                NormalColor = Color.FromArgb(22, 163, 74),
                PressedColor = Color.FromArgb(20, 83, 45),
                Size = new Size(150, 38),
                Text = "UPLOAD PHOTO",
                UseVisualStyleBackColor = false,
                Visible = CanEditPhoto
            };
            _uploadPhotoButton.Click += UploadPhotoButton_Click;

            _removePhotoButton = new RoundedButton
            {
                BackColor = Color.Transparent,
                CornerRadius = 10,
                Enabled = false,
                FlatAppearance = { BorderSize = 0 },
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                HoverColor = Color.FromArgb(226, 232, 240),
                NormalColor = Color.FromArgb(241, 245, 249),
                PressedColor = Color.FromArgb(203, 213, 225),
                Size = new Size(104, 38),
                Text = "REMOVE",
                UseVisualStyleBackColor = false,
                Visible = CanEditPhoto
            };
            _removePhotoButton.Click += RemovePhotoButton_Click;

            _contentPanel.Controls.Add(_photoPreview);
            _contentPanel.Controls.Add(_photoHeading);
            _contentPanel.Controls.Add(_photoHint);
            _contentPanel.Controls.Add(_uploadPhotoButton);
            _contentPanel.Controls.Add(_removePhotoButton);
        }

        private static string InitialsOf(string name)
        {
            string[] parts = (name ?? string.Empty).Split(
                new[] { ' ', ',', '.', '-' },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "?";
            }

            if (parts.Length == 1)
            {
                return parts[0].Substring(0, 1).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private void LoadPhoto(byte[]? bytes)
        {
            _photoBytes = bytes != null && bytes.Length > 0 ? bytes : null;

            Image? next = oop_project.ProfilePicture.Load(_photoBytes);
            Image? previous = _photoImage;
            _photoImage = next;

            _photoPreview.Picture = next;
            previous?.Dispose();

            _removePhotoButton.Enabled = CanEditPhoto && _photoBytes != null;
        }

        private void UploadPhotoButton_Click(object? sender, EventArgs e)
        {
            using var chooser = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "PICTURE FILES|*.png;*.jpg;*.jpeg;*.bmp|ALL FILES|*.*",
                Multiselect = false,
                Title = "CHOOSE A PROFILE PICTURE"
            };

            if (chooser.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                using Image source = oop_project.ProfilePicture.LoadFile(chooser.FileName);
                byte[] portrait = oop_project.ProfilePicture.CreatePortrait(source);

                LoadPhoto(portrait);
                _photoChanged = true;
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is ExternalException ||
                ex is IOException ||
                ex is InvalidOperationException ||
                ex is OutOfMemoryException ||
                ex is UnauthorizedAccessException)
            {
                Authentication.ShowError(
                    "PROFILE PICTURE",
                    "THAT PICTURE COULD NOT BE USED.\nCHOOSE A JPG OR PNG PHOTO AND TRY AGAIN.");
            }
        }

        private void RemovePhotoButton_Click(object? sender, EventArgs e)
        {
            if (_photoBytes == null)
            {
                return;
            }

            LoadPhoto(null);
            _photoChanged = true;
        }

        private void LayoutPhotoSection(int top)
        {
            if (!CanEditPhoto)
            {
                return;
            }

            _photoHeading.SetBounds(LeftColumn, top, 600, 18);
            _photoPreview.SetBounds(LeftColumn, top + 24, 92, 92);
            _photoHint.SetBounds(LeftColumn + 112, top + 30, 600, 34);
            _uploadPhotoButton.SetBounds(LeftColumn + 112, top + 74, 150, 38);
            _removePhotoButton.SetBounds(LeftColumn + 274, top + 74, 104, 38);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            _photoImage?.Dispose();
            _photoImage = null;
        }

        // Places the visible fields in two columns (like the EDIT ACCOUNT pop up)
        // and returns the content height the chrome needs. The list is built from
        // the role instead of the Visible flag because a control inside a form
        // that is not shown yet reports Visible = false.
        private int LayoutFields()
        {
            int firstTop = CanEditPhoto ? FirstFieldTop + PhotoSectionHeight : FirstFieldTop;

            if (CanEditPhoto)
            {
                LayoutPhotoSection(FirstFieldTop);
            }

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

            // EVERY field is one cell of the same two column grid, including FULL
            // NAME. The fields use the same width so both columns line up exactly.
            // The librarian's own EDIT MY INFO keeps FULL NAME on a full width row
            // above the grid; the patron and staff forms stay equal.
            int row = 0;
            int column = 0;
            int firstCell = 0;

            if (IsLibrarian)
            {
                _nameField.Location = new Point(LeftColumn, firstTop);
                _nameField.Size = new Size(RightColumn + ColumnWidth - LeftColumn, FieldHeight);

                firstCell = 1;
                row = 1;
            }

            for (int index = firstCell; index < fields.Count; index++)
            {
                DialogField field = fields[index];
                field.Location = new Point(
                    column == 0 ? LeftColumn : RightColumn,
                    firstTop + (row * FieldStep));
                field.Size = new Size(ColumnWidth, FieldHeight);

                column++;
                if (column == 2)
                {
                    column = 0;
                    row++;
                }
            }

            int usedRows = row + (column > 0 ? 1 : 0);
            return firstTop + (usedRows * FieldStep) + 18;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            ClearErrors();

            // A pasted value skips CharacterCasing, so the uppercase fields are
            // normalised once more before they are written to MySQL.
            string name = _nameField.Value.Trim().ToUpperInvariant();
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
            Address = CanEditAddress ? _addressField.Value.Trim().ToUpperInvariant() : string.Empty;
            Office = CanEditOffice ? _officeField.Value.Trim().ToUpperInvariant() : string.Empty;

            ProfilePicture = _photoChanged && CanEditPhoto ? _photoBytes : null;
            ProfilePictureChanged = _photoChanged && CanEditPhoto;

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
