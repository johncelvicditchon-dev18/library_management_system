using System;
using System.Globalization;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Modal used when a patron presses REQUEST TO BORROW. It asks one question
    /// only - how many copies of the title the patron wants - and returns the
    /// chosen number through <see cref="Copies"/>.
    ///
    /// The patron types the number directly into the field; the dialog accepts
    /// digits only and checks that the value is between 1 and the number of
    /// copies that are actually on the shelf.
    /// </summary>
    public partial class RequestCopiesDialog : Form
    {
        // CS_DROPSHADOW gives the window the soft shadow Windows uses for menus.
        private const int DropShadowClass = 0x00020000;

        private int _copies = 1;
        private int _maximum = 1;

        /// <summary>Design time constructor with a sample title.</summary>
        public RequestCopiesDialog()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WireInput();
        }

        public RequestCopiesDialog(string bookTitle, int available)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _maximum = Math.Max(1, available);

            bookTitleLabel.Text = "\"" + (bookTitle ?? string.Empty).Trim().ToUpperInvariant() + "\"";
            availableLabel.Text = _maximum.ToString(CultureInfo.InvariantCulture) +
                                  (_maximum == 1
                                      ? " COPY IS AVAILABLE ON THE SHELF."
                                      : " COPIES ARE AVAILABLE ON THE SHELF.");
            maxLabel.Text = "MAX " + _maximum.ToString(CultureInfo.InvariantCulture);

            // Allow a slightly longer number than the maximum so a mistyped value
            // is reported back to the patron instead of being silently cut off.
            copiesInput.MaxLength = Math.Max(3, _maximum.ToString(CultureInfo.InvariantCulture).Length + 1);

            WireInput();
        }

        /// <summary>The number of copies the patron chose.</summary>
        public int Copies
        {
            get { return _copies; }
        }

        private void WireInput()
        {
            copiesInput.KeyPress += CopiesInput_KeyPress;
            copiesInput.TextChanged += delegate { ClearError(); };
            copiesInput.Enter += delegate { copiesField.IsFocused = true; };
            copiesInput.Leave += delegate { copiesField.IsFocused = false; };
            sendButton.Click += SendButton_Click;

            // Enter sends from anywhere on the dialog.
            KeyPreview = true;
            copiesInput.Text = "1";
        }

        private void CopiesInput_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // The field holds a number, so only digits can be typed into it.
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SendButton_Click(object? sender, EventArgs e)
        {
            int value;
            string text = copiesInput.Text.Trim();

            if (text.Length == 0)
            {
                ShowError("Enter the number of copies.");
                return;
            }

            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) || value < 1)
            {
                ShowError("Enter a number from 1 to " + _maximum.ToString(CultureInfo.InvariantCulture) + ".");
                return;
            }

            if (value > _maximum)
            {
                ShowError(_maximum == 1
                    ? "Only 1 copy is available."
                    : "Only " + _maximum.ToString(CultureInfo.InvariantCulture) + " copies are available.");
                return;
            }

            ClearError();
            _copies = value;
            DialogResult = DialogResult.OK;
        }

        private void ShowError(string message)
        {
            oop_project.Authentication.SetFieldError(copiesError, copiesField, message);
            copiesInput.Focus();
            copiesInput.SelectAll();
        }

        private void ClearError()
        {
            oop_project.Authentication.SetFieldError(copiesError, copiesField, null);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // The patron can start typing the number straight away.
            copiesInput.Focus();
            copiesInput.SelectAll();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= DropShadowClass;
                return cp;
            }
        }
    }
}
