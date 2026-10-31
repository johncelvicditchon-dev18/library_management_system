using oop_project.forms;
using oop_project.Models;
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace oop_project
{
    public partial class Login : authentication
    {
        protected override string FormName
        {
            get { return "SIGN IN"; }
        }
        public Login()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            signup_label.BringToFront();
            this.ActiveControl = username_input;


            username_input.TextChanged += username_input_TextChanged;
            username_input.Leave += username_input_Leave;
            password_input.TextChanged += password_input_TextChanged;
            password_input.Leave += password_input_Leave;

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
            CaptureDesignLayout(new Size(1482, 853), LayoutControls());
            ApplyCenteredLayout(gradientPanel1, LayoutControls());
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyCenteredLayout(gradientPanel1, LayoutControls());
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
                    EnsureTextDateFormat(conn);
                }
                catch (MySqlException ex)
                {
                    ShowError("COULD NOT CONNECT TO MYSQL.\n" + ex.Message);
                }
            }
        }
        private static void EnsureTextDateFormat(MySqlConnection conn)
        {
            const string checkQuery = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'account_role_table' AND COLUMN_NAME = 'date_created';";
            using (MySqlCommand check = new MySqlCommand(checkQuery, conn))
            {
                object? dataType = check.ExecuteScalar();
                if (dataType == null || dataType == DBNull.Value) { return; }
                if (Convert.ToString(dataType) == "varchar") { return; }
            }
            using (MySqlCommand alter = new MySqlCommand("ALTER TABLE account_role_table MODIFY date_created VARCHAR(30) NOT NULL;", conn))
            {
                alter.ExecuteNonQuery();
            }
            using (MySqlCommand reformat = new MySqlCommand("UPDATE account_role_table SET date_created = UPPER(DATE_FORMAT(STR_TO_DATE(date_created, '%Y-%m-%d %H:%i:%s'), '%M %e, %Y')) WHERE date_created REGEXP '^[0-9]{4}-[0-9]{2}-[0-9]{2}';", conn))
            {
                reformat.ExecuteNonQuery();
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

                    string query = "SELECT a.account_id, a.`name`, a.`username`, ar.`role`, ar.`status` FROM account_table a LEFT JOIN account_role_table ar ON ar.account_id = a.account_id WHERE BINARY a.username = BINARY @username AND  BINARY a.`password` = BINARY @password LIMIT 1;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ShowMessage("INCORRECT USERNAME OR PASSWORD.", "SIGN IN FAILED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            string role = reader.IsDBNull(reader.GetOrdinal("role")) ? "STUDENT" : reader.GetString("role");
                            string status = reader.IsDBNull(reader.GetOrdinal("status")) ? "PENDING" : reader.GetString("status");

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

                            Person person = PersonFactory.Create(role);
                            person.Username = username;
                            string accountRole = role;
                            string accountUsername = reader.IsDBNull(reader.GetOrdinal("username")) ? username : reader.GetString("username");
                            person.FullName = reader.IsDBNull(reader.GetOrdinal("name")) ? username : reader.GetString("name");
                            int accountID = reader.GetInt32(reader.GetOrdinal("account_id"));
                            ShowMessage(person.GetWelcomeMessage(), "SIGNED IN", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // A student gets the student portal (catalog + requests + QR).
                            // The librarian, staff and faculty keep the management dashboard.
                            if (string.Equals(role, "STUDENT", StringComparison.OrdinalIgnoreCase))
                            {
                                student_dashboard studentForm = new student_dashboard(
                                    accountID, person.FullName, role, accountUsername, status);
                                studentForm.Show();
                            }
                            else
                            {
                                admin_dashboard dashboardForm = new admin_dashboard(
                                    accountID, person.FullName, role, accountUsername, status);
                                dashboardForm.Show();
                            }

                            this.Hide();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    ShowError("COULD NOT SIGN IN BECAUSE OF A DATABASE ERROR.\n" + ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ShowError(ex.Message);
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
            signupForm.Show();
            this.Hide();
        }
    }
}
