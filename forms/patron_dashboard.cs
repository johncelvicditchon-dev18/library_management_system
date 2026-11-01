namespace oop_project.forms
{
    /// <summary>
    /// The patron portal used by students and faculty. The complete layout is
    /// built by <see cref="PatronDashboardBase"/> because the Windows Forms
    /// designer creates an instance of a form's base class while rendering it.
    /// This file keeps the designer association (patron_dashboard.designer.cs)
    /// and the public constructors used by the application.
    /// </summary>
    public partial class patron_dashboard : PatronDashboardBase
    {
        public patron_dashboard()
            : base()
        {
            InitializeComponent();
            ApplyDashboardTitle();
        }

        public patron_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
            : base(accountID, fullName, role, username, status)
        {
            InitializeComponent();
            ApplyDashboardTitle();
            StartRuntime();
        }
    }
}
