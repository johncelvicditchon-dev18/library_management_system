using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Sign in pop up shown before the LIBRARY RESOURCE CENTER EVALUATION FORM.
    /// The SEMESTER dropdown lists the semesters the librarian opened, so the
    /// respondent evaluates the term the library allows. Only an active STUDENT,
    /// FACULTY or STAFF account may continue, and only once per semester. The
    /// role, name and semester collected here are passed to
    /// <see cref="EvaluationForm"/> so the survey is stored against the right
    /// respondent and the right period.
    /// </summary>
    public partial class EvaluationLogin : Form
    {
        public EvaluationLogin()
        {
            InitializeComponent();
            DoubleBuffered = true;

            // The password is masked, and typing hides a previous inline warning.
            passwordField.Input.UseSystemPasswordChar = true;
            usernameField.ValueChanged += (s, e) => usernameField.ClearError();
            passwordField.ValueChanged += (s, e) => passwordField.ClearError();

            // The SEMESTER list is read from the semesters the librarian opened,
            // then the username field takes the focus.
            Shown += (s, e) =>
            {
                LoadSemesters();
                usernameField.FocusInput();
            };
        }

        // The semester the librarian has opened, and the way the "one evaluation
        // per semester" rule is checked before the survey is handed over.
        private readonly EvaluationDirectory _directory = new EvaluationDirectory();

        // The semesters listed in the dropdown, in the same order as its items.
        private readonly List<EvaluationPeriod> _semesterOptions = new List<EvaluationPeriod>();

        /// <summary>The account behind the successful sign in.</summary>
        public int AccountId { get; private set; }

        /// <summary>STUDENT, FACULTY or STAFF.</summary>
        public string RespondentRole { get; private set; } = string.Empty;

        /// <summary>The name on the account, used to prefill the evaluation form.</summary>
        public string RespondentName { get; private set; } = string.Empty;

        /// <summary>
        /// The student id from the profile table, filled for a STUDENT only. The
        /// account username is used when a student profile has no school id yet.
        /// FACULTY and STAFF have no student id, so the value stays empty.
        /// </summary>
        public string RespondentIdNumber { get; private set; } = string.Empty;

        /// <summary>The semester the librarian has open for evaluation.</summary>
        public int EvaluationPeriodId { get; private set; }

        /// <summary>"1ST SEMESTER 2025-2026", shown on the survey.</summary>
        public string EvaluationPeriodTitle { get; private set; } = string.Empty;

        /// <summary>
        /// Lists the semesters the librarian opened in the SEMESTER dropdown, so the
        /// respondent evaluates the term the library allows. When none is open the
        /// dropdown says so and SIGN IN stays disabled until one is opened.
        /// </summary>
        private void LoadSemesters()
        {
            _semesterOptions.Clear();
            semesterField.ChoiceBox.Items.Clear();
            semesterField.ClearError();

            try
            {
                _semesterOptions.AddRange(_directory.LoadOpenPeriods());
            }
            catch (MySqlException)
            {
                // The database error is reported when the credentials are checked,
                // so here the list only says it could not be read.
                semesterField.ChoiceBox.Items.Add("SEMESTER UNAVAILABLE");
                semesterField.ChoiceBox.SelectedIndex = 0;
                semesterField.ChoiceBox.Enabled = false;
                signInButton.Enabled = true;
                return;
            }

            foreach (EvaluationPeriod period in _semesterOptions)
            {
                semesterField.ChoiceBox.Items.Add(period.Title);
            }

            if (_semesterOptions.Count == 0)
            {
                semesterField.ChoiceBox.Items.Add("NO SEMESTER IS OPEN");
                semesterField.ChoiceBox.SelectedIndex = 0;
                semesterField.ChoiceBox.Enabled = false;
                semesterField.ShowError("The librarian has not opened a semester yet.");
                signInButton.Enabled = true;
                return;
            }

            semesterField.ChoiceBox.Enabled = true;
            semesterField.ChoiceBox.SelectedIndex = 0;
            signInButton.Enabled = true;
        }

        /// <summary>The semester picked in the dropdown, or null when none is listed.</summary>
        private EvaluationPeriod? SelectedSemester()
        {
            int index = semesterField == null ? -1 : semesterField.ChoiceBox.SelectedIndex;

            return index >= 0 && index < _semesterOptions.Count
                ? _semesterOptions[index]
                : null;
        }

        private void signInButton_Click_1(object? sender, EventArgs e)
        {
            usernameField.ClearError();
            passwordField.ClearError();

            string username = usernameField.Value.Trim();
            string password = passwordField.Value;
            bool valid = true;

            if (username.Length == 0)
            {
                usernameField.ShowError("Username is required.");
                valid = false;
            }

            if (password.Length == 0)
            {
                passwordField.ShowError("Password is required.");
                valid = false;
            }

            if (!valid)
            {
                if (username.Length == 0)
                {
                    usernameField.FocusInput();
                }
                else
                {
                    passwordField.FocusInput();
                }

                return;
            }

            signInButton.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string role;
                string status;
                string name;
                string accountUsername;
                string schoolId;
                int accountId;

                // The credentials are read first and the reader is closed before
                // the semester checks run, because a MySQL connection cannot run a
                // second command while a reader is still open on it.
                using (MySqlConnection conn = Authentication.OpenConnection())
                {
                    conn.Open();

                    const string query =
                        "SELECT a.account_id, a.`name`, a.`username`, ar.`role`, ar.`status`, " +
                        "       IFNULL(s.`school_id`, '') AS school_id " +
                        "FROM account_table a " +
                        "LEFT JOIN account_role_table ar ON ar.account_id = a.account_id " +
                        "LEFT JOIN student_info_table s ON s.account_id = a.account_id " +
                        "WHERE BINARY a.username = BINARY @username AND BINARY a.`password` = BINARY @password " +
                        "LIMIT 1;";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Authentication.ShowMessage(
                                    "INCORRECT USERNAME OR PASSWORD.",
                                    "SIGN IN FAILED",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                passwordField.FocusInput();
                                passwordField.Input.SelectAll();
                                return;
                            }

                            role = reader.IsDBNull(reader.GetOrdinal("role"))
                                ? "STUDENT"
                                : reader.GetString("role");
                            status = reader.IsDBNull(reader.GetOrdinal("status"))
                                ? "PENDING"
                                : reader.GetString("status");
                            name = reader.IsDBNull(reader.GetOrdinal("name"))
                                ? username
                                : reader.GetString("name");
                            accountUsername = reader.IsDBNull(reader.GetOrdinal("username"))
                                ? username
                                : reader.GetString("username");
                            schoolId = reader.IsDBNull(reader.GetOrdinal("school_id"))
                                ? string.Empty
                                : reader.GetString("school_id");
                            accountId = reader.GetInt32(reader.GetOrdinal("account_id"));
                        }
                    }
                }

                if (!string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    string reason;

                    if (string.Equals(status, "REJECTED", StringComparison.OrdinalIgnoreCase))
                    {
                        reason = "YOUR ACCOUNT WAS REJECTED BY THE LIBRARY STAFF.";
                    }
                    else if (string.Equals(status, "INACTIVE", StringComparison.OrdinalIgnoreCase))
                    {
                        reason = "YOUR ACCOUNT HAS BEEN DEACTIVATED. PLEASE ASK THE LIBRARY STAFF TO REACTIVATE IT.";
                    }
                    else
                    {
                        reason = "YOUR ACCOUNT IS STILL PENDING APPROVAL BY THE LIBRARY STAFF.";
                    }

                    Authentication.ShowMessage(
                        reason,
                        "ACCOUNT " + status.ToUpperInvariant(),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string normalizedRole = role.Trim().ToUpperInvariant();

                // An OFFICER account belongs to the staff group.
                if (normalizedRole == "OFFICER")
                {
                    normalizedRole = "STAFF";
                }

                if (normalizedRole != "STUDENT" &&
                    normalizedRole != "FACULTY" &&
                    normalizedRole != "STAFF")
                {
                    Authentication.ShowMessage(
                        "ONLY STUDENTS, FACULTY AND STAFF CAN ANSWER THE EVALUATION FORM.",
                        "NOT ALLOWED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // SEMESTER GATE: the survey is answered only for a semester the
                // librarian opened, and only once per account inside it. The term
                // picked in the dropdown is re-checked because the librarian may
                // have closed or replaced it while this window stayed open.
                EvaluationPeriod? selectedPeriod = SelectedSemester();

                if (selectedPeriod == null)
                {
                    // The librarian may have opened a semester after this window was
                    // already on screen, so the list is reread once before refusing.
                    LoadSemesters();
                    selectedPeriod = SelectedSemester();
                }

                if (selectedPeriod == null)
                {
                    Authentication.ShowMessage(
                        "THE EVALUATION FORM IS CURRENTLY CLOSED.\n\nPLEASE WAIT FOR THE LIBRARY TO OPEN IT FOR THE SEMESTER.",
                        "EVALUATION CLOSED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                EvaluationPeriod? openPeriod = _directory.GetOpenPeriod();

                if (openPeriod == null || openPeriod.PeriodId != selectedPeriod.PeriodId)
                {
                    Authentication.ShowMessage(
                        "THE SEMESTER YOU SELECTED IS NO LONGER OPEN.\n\n" +
                        "PLEASE CHOOSE THE SEMESTER THE LIBRARY HAS OPENED.",
                        "EVALUATION CLOSED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    LoadSemesters();
                    return;
                }

                if (_directory.HasResponded(accountId, openPeriod.PeriodId))
                {
                    Authentication.ShowMessage(
                        "YOU HAVE ALREADY SUBMITTED YOUR EVALUATION FOR\n" + openPeriod.Title + ".\n\n" +
                        "EACH RESPONDENT MAY ANSWER ONLY ONCE PER SEMESTER. THE FORM WILL BE OPEN AGAIN NEXT SEMESTER.",
                        "ALREADY SUBMITTED",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                AccountId = accountId;
                RespondentRole = normalizedRole;
                RespondentName = name.Trim();
                EvaluationPeriodId = openPeriod.PeriodId;
                EvaluationPeriodTitle = openPeriod.Title;

                // Only a STUDENT has a student id to show on the survey.
                RespondentIdNumber = normalizedRole == "STUDENT"
                    ? (string.IsNullOrWhiteSpace(schoolId) ? accountUsername.Trim() : schoolId.Trim())
                    : string.Empty;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (MySqlException ex)
            {
                Authentication.ShowError(
                    "EVALUATION SIGN IN",
                    "COULD NOT SIGN IN BECAUSE OF A DATABASE ERROR.\n" + ex.Message);
            }
            finally
            {
                signInButton.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
