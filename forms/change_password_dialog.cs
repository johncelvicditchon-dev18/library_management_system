using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Small modal used by the PROFILE tab of both dashboards. The signed in
    /// person confirms the current password, types the new one twice, and the
    /// account row is updated only when the current password is correct.
    /// </summary>
    public partial class ChangePasswordDialog : Form
    {
        private readonly int _accountId;
        private readonly AccountDirectory _accounts = new AccountDirectory();

        /// <summary>Design time constructor.</summary>
        public ChangePasswordDialog()
            : this(0, string.Empty, string.Empty)
        {
        }

        public ChangePasswordDialog(int accountId, string username, string role)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _accountId = accountId;

            string user = (username ?? string.Empty).Trim().ToUpperInvariant();
            string userRole = (role ?? string.Empty).Trim().ToUpperInvariant();
          
            // Each password field gets its own show / hide eye.
            currentToggle.Attach(currentInput);
            newToggle.Attach(newInput);
            confirmToggle.Attach(confirmInput);

            currentInput.TextChanged += delegate { ClearError(currentError, currentField); };
            newInput.TextChanged += delegate { ClearError(newError, newField); };
            confirmInput.TextChanged += delegate { ClearError(confirmError, confirmField); };

            currentInput.Enter += delegate { currentField.IsFocused = true; };
            currentInput.Leave += delegate { currentField.IsFocused = false; };
            newInput.Enter += delegate { newField.IsFocused = true; };
            newInput.Leave += delegate { newField.IsFocused = false; };
            confirmInput.Enter += delegate { confirmField.IsFocused = true; };
            confirmInput.Leave += delegate { confirmField.IsFocused = false; };

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            string current = currentInput.Text;
            string fresh = newInput.Text;
            string confirmation = confirmInput.Text;

            bool currentFilled = current.Length > 0;
            bool freshFilled = fresh.Length > 0;
            bool confirmationFilled = confirmation.Length > 0;
            bool matches = confirmationFilled && string.Equals(fresh, confirmation, StringComparison.Ordinal);

            SetError(currentError, currentField, currentFilled ? null : "ENTER YOUR CURRENT PASSWORD.");
            SetError(newError, newField, freshFilled ? null : "ENTER YOUR NEW PASSWORD.");
            SetError(confirmError, confirmField,
                !confirmationFilled ? "CONFIRM YOUR NEW PASSWORD." :
                matches ? null : "THE PASSWORDS DO NOT MATCH.");

            if (!currentFilled || !freshFilled || !matches)
            {
                return;
            }

            if (string.Equals(fresh, current, StringComparison.Ordinal))
            {
                SetError(newError, newField, "CHOOSE A PASSWORD DIFFERENT FROM THE CURRENT ONE.");
                newInput.Focus();
                newInput.SelectAll();
                return;
            }

            try
            {
                if (!_accounts.ChangePassword(_accountId, current, fresh))
                {
                    SetError(currentError, currentField, "THE CURRENT PASSWORD IS INCORRECT.");
                    currentInput.Focus();
                    currentInput.SelectAll();
                    return;
                }

                Authentication.ShowMessage(
                    "YOUR PASSWORD HAS BEEN CHANGED SUCCESSFULLY.",
                    "PASSWORD UPDATED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (MySqlException ex)
            {
                Authentication.ShowError("CHANGE PASSWORD", "THE PASSWORD COULD NOT BE UPDATED.\n" + ex.Message);
            }
            catch (ArgumentException ex)
            {
                Authentication.ShowError("CHANGE PASSWORD", ex.Message);
            }
        }

        private static void SetError(Label label, oop_project.FieldPanel field, string? message)
        {
            Authentication.SetFieldError(label, field, message);
        }

        private static void ClearError(Label label, oop_project.FieldPanel field)
        {
            Authentication.SetFieldError(label, field, null);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            currentInput.Focus();
        }
    }
}
