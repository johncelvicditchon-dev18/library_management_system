using oop_project.forms;
using oop_project.Models;
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static oop_project.Authentication;
namespace oop_project
{
    public partial class Login : Form
    {
        private readonly Authentication.WindowLayout _layout = new Authentication.WindowLayout();

        public Login()
        {
            InitializeComponent();
            closeButton.Height = closeButton.Width;
            closeButton.BringToFront();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            signup_label.BringToFront();

            // HEADER TYPE: the sign-in card uses the same title style as the
            // sign-up card, so both authentication screens read as one product.
            label2.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label2.TextAlign = ContentAlignment.MiddleLeft;

            // ICONS: user inside the username box, lock inside the password box.
            usernameField.AttachIcon("\uE77B");
            passwordField.AttachIcon("\uE72E");

            this.ActiveControl = username_input;


            username_input.TextChanged += username_input_TextChanged;
            username_input.Leave += username_input_Leave;
            password_input.TextChanged += password_input_TextChanged;
            password_input.Leave += password_input_Leave;

            // SHOW / HIDE: the eye inside the password field reveals the typed text.
            password_toggle.Attach(password_input);

            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                TestConnection();
            }

            // FULL SCREEN: the sign in page opens maximised and F11 toggles a borderless view.
            ScreenMode.Apply(this);
        }

        // Keeps the card, the hero and the fields in the same composition when maximised.
        private Control[]? _layoutControls;

        private Control[] LayoutControls()
        {
            return _layoutControls ??= new Control[]
            {
                roundedCard1,
                lrcLogoBadge1,
                brandLabel,
                taglineLabel,
                usernameLabel,
                usernameField,
                username_error,
                passwordLabel,
                passwordField,
                password_error,
                login_button
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _layout.Capture(new Size(1482, 853), LayoutControls());
            _layout.Apply(this, gradientPanel1, LayoutControls());

            _cardLayoutReady = true;
            ArrangeCard();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _layout.Apply(this, gradientPanel1, LayoutControls());

            if (_cardLayoutReady)
            {
                ArrangeCard();
            }
        }

        // CARD GRID: one 50px gutter on both sides, one header lock-up (seal,
        // title and close), evenly spaced fields and a centred footer. The card
        // is sized to its content so the spacing never depends on the window.
        private bool _cardLayoutReady;

        private void ArrangeCard()
        {
            if (roundedCard1 is null || gradientPanel1 is null ||
                ClientSize.Width < 100 || ClientSize.Height < 100)
            {
                return;
            }

            const int Pad = 50;
            const int HeaderTop = 26;
            const int LogoSize = 48;
            const int CloseSize = 36;
            const int FieldHeight = 46;
            const int ButtonHeight = 52;
            const int LabelGap = 28;
            const int FieldGap = 5;
            const int BlockGap = 32;

            int cardWidth = Math.Max(roundedCard1.Width, Pad * 2 + 200);
            int contentWidth = cardWidth - Pad * 2;

            // ---- Header: the seal and the close button share one centre line and
            // the close button ends exactly where the fields end.
            int titleLeft = Pad + LogoSize + 20;
            int closeLeft = Pad + contentWidth - CloseSize;

            logoBadge.SetBounds(Pad, HeaderTop, LogoSize, LogoSize);
            label2.SetBounds(titleLeft, HeaderTop, Math.Max(60, closeLeft - titleLeft - 16), LogoSize);
            closeButton.SetBounds(closeLeft, HeaderTop + (LogoSize - CloseSize) / 2, CloseSize, CloseSize);

            int dividerTop = HeaderTop + LogoSize + 20;
            headerLine.SetBounds(Pad, dividerTop, contentWidth, 1);

            // ---- Field stack: 32px between the divider and the first label, then
            // one 28px label-to-field step for each row. The warning line keeps
            // its slot even when hidden, so the card never jumps while typing.
            int usernameLabelTop = dividerTop + BlockGap;
            int usernameFieldTop = usernameLabelTop + LabelGap;
            int usernameErrorTop = usernameFieldTop + FieldHeight + FieldGap;
            int passwordLabelTop = usernameErrorTop + BlockGap;
            int passwordFieldTop = passwordLabelTop + LabelGap;
            int passwordErrorTop = passwordFieldTop + FieldHeight + FieldGap;
            int buttonTop = passwordErrorTop + BlockGap + 12;
            int footerTop = buttonTop + ButtonHeight + 20;
            int cardHeight = footerTop + 51;

            roundedCard1.Height = cardHeight;
            roundedCard1.Top = Math.Max(24, (ClientSize.Height - cardHeight) / 2);

            int left = roundedCard1.Left + Pad;
            int top = roundedCard1.Top;

            usernameLabel.Location = new Point(left, top + usernameLabelTop);
            usernameField.SetBounds(left, top + usernameFieldTop, contentWidth, FieldHeight);
            username_error.SetBounds(left, top + usernameErrorTop, contentWidth, 16);

            passwordLabel.Location = new Point(left, top + passwordLabelTop);
            passwordField.SetBounds(left, top + passwordFieldTop, contentWidth, FieldHeight);
            password_error.SetBounds(left, top + passwordErrorTop, contentWidth, 16);

            login_button.SetBounds(left, top + buttonTop, contentWidth, ButtonHeight);

            // ---- Footer: the prompt and the link are measured and centred as one
            // group, so the pair stays centred under the button at any font size.
            // Both labels are children of the card, so the position is card-relative.
            int footerWidth = label1.Width + 8 + signup_label.Width;
            int footerLeft = (cardWidth - footerWidth) / 2;

            label1.Location = new Point(footerLeft, footerTop);
            signup_label.Location = new Point(footerLeft + label1.Width + 8, footerTop);
        }

        private void username_input_TextChanged(object? sender, EventArgs e)
        {
            if (username_error.Visible) { ValidateUsername(); }
        }        private void username_input_Leave(object? sender, EventArgs e) { ValidateUsername(); }
        private void password_input_TextChanged(object? sender, EventArgs e)
        {
            if (password_error.Visible) { ValidatePassword(); }
        }
        private void password_input_Leave(object? sender, EventArgs e) { ValidatePassword(); }

        private bool ValidateUsername()
        {
            if (username_input.Text.Trim().Length == 0)
            {
                SetFieldError(username_error, usernameField, "Username is required");
                return false;
            }
            SetFieldError(username_error, usernameField, null);
            return true;
        }
        private bool ValidatePassword()
        {
            if (password_input.Text.Length == 0)
            {
                SetFieldError(password_error, passwordField, "Password is required");
                return false;
            }
            SetFieldError(password_error, passwordField, null);
            return true;
        }

        private void TestConnection()
        {
            using (MySqlConnection conn = OpenConnection())
            {
                try
                {
                    conn.Open();
                    EnsureContactNumberColumns(conn);
                    EnsureProfileColumnsRemoved(conn);
                }
                catch (MySqlException ex)
                {
                    ShowError("SIGN IN", "COULD NOT CONNECT TO MYSQL.\n" + ex.Message);
                }
            }
        }



        private void login_button_Click(object sender, EventArgs e)
        {
            string username = username_input.Text.Trim();
            string password = password_input.Text;

            bool usernameOk = ValidateUsername();
            bool passwordOk = ValidatePassword();
            if (!usernameOk || !passwordOk)
            {
                if (!usernameOk) { username_input.Focus(); }
                else { password_input.Focus(); }
                return;
            }

            using (MySqlConnection conn = OpenConnection())
            {
                try
                {
                    conn.Open();

                    const string query =
                        "SELECT a.account_id, a.`name`, a.`username`, a.`password`, " +
                        "       rt.`role_code` AS `role`, st.`status_code` AS `status` " +
                        "FROM account_table a " +
                        "LEFT JOIN account_role_table ar ON ar.`account_id` = a.`account_id` " +
                        "LEFT JOIN role_table rt ON rt.`role_id` = ar.`role_id` " +
                        "LEFT JOIN account_status_table st ON st.`status_id` = ar.`status_id` " +
                        "WHERE BINARY a.username = BINARY @username LIMIT 1;";

                    int accountID = 0;
                    string role = "STUDENT";
                    string status = "PENDING";
                    string accountUsername = username;
                    string fullName = username;
                    string storedPassword = string.Empty;
                    bool found = false;

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                found = true;
                                accountID = reader.GetInt32(reader.GetOrdinal("account_id"));
                                storedPassword = reader.IsDBNull(reader.GetOrdinal("password"))
                                    ? string.Empty
                                    : reader.GetString("password");
                                role = reader.IsDBNull(reader.GetOrdinal("role")) ? "STUDENT" : reader.GetString("role");
                                status = reader.IsDBNull(reader.GetOrdinal("status")) ? "PENDING" : reader.GetString("status");
                                accountUsername = reader.IsDBNull(reader.GetOrdinal("username")) ? username : reader.GetString("username");
                                fullName = reader.IsDBNull(reader.GetOrdinal("name")) ? username : reader.GetString("name");
                            }
                        }
                    }

                    // SECURITY: the typed password is checked against the PBKDF2
                    // hash, or against the plain text saved by an older build.
                    if (!found || !PasswordHasher.Verify(password, storedPassword))
                    {
                        ShowMessage("INCORRECT USERNAME OR PASSWORD.", "SIGN IN FAILED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (status != "ACTIVE")
                    {
                        string reason;

                        if (status == "REJECTED")
                        {
                            reason = "YOUR ACCOUNT WAS REJECTED BY THE LIBRARY STAFF.";
                        }
                        else if (status == "INACTIVE")
                        {
                            reason = "YOUR ACCOUNT HAS BEEN DEACTIVATED. PLEASE ASK THE LIBRARY STAFF TO REACTIVATE IT.";
                        }
                        else
                        {
                            reason = "YOUR ACCOUNT IS STILL PENDING APPROVAL BY THE LIBRARY STAFF.";
                        }

                        ShowMessage(reason, "ACCOUNT " + status, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // MIGRATION: a plain text row becomes a hash on the first
                    // correct sign in, so the stored copy never stays readable.
                    if (PasswordHasher.NeedsUpgrade(storedPassword))
                    {
                        PasswordHasher.UpgradeStoredPassword(accountID, password);
                    }

                    Person person = PersonFactory.Create(role);
                    person.Username = username;
                    person.FullName = fullName;
                    ShowMessage(person.GetWelcomeMessage(), "SIGNED IN", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Students, faculty and staff all use the single patron
                    // portal to browse the catalog and request books. Only the
                    // librarian (system admin) opens the management dashboard.
                    if (string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(role, "FACULTY", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(role, "STAFF", StringComparison.OrdinalIgnoreCase))
                    {
                        student_faculty_staff_dashboard patronForm = new student_faculty_staff_dashboard(
                            accountID, person.FullName, role, accountUsername, status);
                        patronForm.Show();
                    }
                    else
                    {
                        librarian_dashboard dashboardForm = new librarian_dashboard(
                            accountID, person.FullName, role, accountUsername, status);
                        dashboardForm.Show();
                    }

                    this.Hide();
                }
                catch (MySqlException ex)
                {
                    ShowError("SIGN IN", "COULD NOT SIGN IN BECAUSE OF A DATABASE ERROR.\n" + ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ShowError("SIGN IN", ex.Message);
                }
            }
        }
        private void signup_label_MouseLeave(object? sender, EventArgs e)
        {
            signup_label.ForeColor = Color.FromArgb(55, 65, 81);
            signup_label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            signup_label.Cursor = Cursors.Default;
        }
        private void signup_label_Click(object sender, EventArgs e)
        {
            Signup signupForm = new Signup();

            // Closing the sign-up form returns to the welcome page instead of
            // leaving this hidden sign-in form in the background.
            signupForm.FormClosed += (s, args) => Close();

            signupForm.Show();
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
