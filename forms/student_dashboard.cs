namespace oop_project.forms
{
    /// <summary>
    /// The student portal. It keeps the familiar name used by the login form, while
    /// the shared catalog / request / QR behaviour lives in <see cref="patron_dashboard"/>.
    /// </summary>
    public sealed partial class student_dashboard : patron_dashboard
    {
        public student_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
            : base(accountID, fullName, role, username, status)
        {
            InitializeComponent();
        }
    }
}
