namespace oop_project.forms
{
    /// <summary>
    /// Faculty use the same borrow workflow as students, but the profile labels
    /// show their program and institute instead of a student year level.
    /// </summary>
    public sealed partial class faculty_dashboard : patron_dashboard
    {
        public faculty_dashboard(int accountID, string fullName, string role, string username = "", string status = "")
            : base(accountID, fullName, role, username, status)
        {
            InitializeComponent();
        }
    }
}
