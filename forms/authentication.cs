using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using oop_project.Database;
using oop_project.Models;
namespace oop_project
{
    // ABSTRACTION + INHERITANCE: shared connection, dialog handling and field
    // validation for login and signup.
    // NOTE: this class is intentionally NOT abstract. The Windows Forms designer creates an
    // instance of the base type while rendering Login / Signup, so an abstract base would
    // raise "the designer must create an instance of type ... it is declared as abstract".
    public partial class authentication : Form
    {
        private readonly IDatabaseConnection _database = new MySqlDatabaseConnection();
        // WARNING STYLE: every invalid input uses the same red text and red border.
        protected static readonly Color ErrorColor = Color.FromArgb(220, 38, 38);
        // SUCCESS STYLE: a satisfied input uses the same green as the focused border.
        protected static readonly Color SuccessColor = Color.FromArgb(22, 163, 74);
        private static readonly Color IdleBorderColor = Color.FromArgb(229, 231, 235);

        /// <summary>
        /// Design time constructor. The Windows Forms designer creates an instance of
        /// the base type while rendering Login / Signup, so this class must be
        /// instantiable (not abstract) and must call InitializeComponent.
        /// </summary>
        public authentication()
        {
            InitializeComponent();
        }

        // POLYMORPHISM: each form supplies its own caption (Login and Signup override this).
        protected virtual string FormName
        {
            get { return "ACCOUNT"; }
        }
        protected MySqlConnection OpenConnection()
        {
            return _database.CreateConnection();
        }
        // ENCAPSULATION: every dialog is shown in UPPER CASE from one place.
        protected static void ShowMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            string text = message == null ? string.Empty : message.ToUpperInvariant();
            string title = caption == null ? string.Empty : caption.ToUpperInvariant();
            MessageBox.Show(text, title, buttons, icon);
        }
        protected void ShowError(string message)
        {
            ShowMessage(message, FormName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // ENCAPSULATION: one method decides how an invalid input looks.
        // Passing a null/empty message hides the warning and restores the normal border.
        protected static void SetFieldError(Label? messageLabel, FieldPanel? field, string? message)
        {
            bool hasError = !string.IsNullOrEmpty(message);
            if (messageLabel != null)
            {
                messageLabel.Text = hasError ? message! : string.Empty;
                messageLabel.ForeColor = ErrorColor;
                messageLabel.Visible = hasError;
            }
            if (field != null)
            {
                field.BorderColor = hasError ? ErrorColor : IdleBorderColor;
                field.Invalidate();
            }
        }

        // ENCAPSULATION: the matching counterpart of SetFieldError.
        // It keeps the message visible, but paints it green to confirm the input is correct.
        protected static void SetFieldSuccess(Label? messageLabel, FieldPanel? field, string message)
        {
            if (messageLabel != null)
            {
                messageLabel.Text = message;
                messageLabel.ForeColor = SuccessColor;
                messageLabel.Visible = true;
            }
            if (field != null)
            {
                field.BorderColor = SuccessColor;
                field.Invalidate();
            }
        }

        // LAYOUT: remembers where every control sits in the designer, once the form is loaded
        // (so any Windows DPI scaling has already been applied).
        private readonly Dictionary<Control, Point> _designLocations = new Dictionary<Control, Point>();
        private Size _designClientSize;
        private bool _designCaptured;

        // The design size is passed in because the window now opens maximised:
        // ClientSize is already the maximised size when this runs, so it must not
        // be used as the reference for centring the composition.
        protected void CaptureDesignLayout(Size designSize, params Control[] controls)
        {
            if (_designCaptured || controls == null)
            {
                return;
            }

            _designClientSize = designSize;
            _designLocations.Clear();

            foreach (Control control in controls)
            {
                if (control != null)
                {
                    _designLocations[control] = control.Location;
                }
            }

            _designCaptured = true;
        }

        // LAYOUT: when the window grows or is maximised, the existing gradient background
        // stretches to fill it and the whole composition stays centred - the fields, card and
        // hero never drift away from each other or get stuck in the top-left corner.
        protected void ApplyCenteredLayout(Control background, params Control[] controls)
        {
            if (!_designCaptured || background == null || controls == null)
            {
                return;
            }

            Size area = ClientSize;
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            int offsetX = (area.Width - _designClientSize.Width) / 2;
            int offsetY = (area.Height - _designClientSize.Height) / 2;

            background.Bounds = new Rectangle(
                background.Left,
                background.Top,
                Math.Max(1, area.Width - background.Left),
                Math.Max(1, area.Height - background.Top));

            foreach (Control control in controls)
            {
                if (control == null || !_designLocations.TryGetValue(control, out Point design))
                {
                    continue;
                }

                control.Location = new Point(design.X + offsetX, design.Y + offsetY);
            }
        }
    }
}
