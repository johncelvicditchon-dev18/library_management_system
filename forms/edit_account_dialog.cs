using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// The EDIT ACCOUNT pop up. The complete content is built by
    /// <see cref="EditAccountDialogBase"/> because the Windows Forms designer
    /// creates an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class EditAccountDialog : EditAccountDialogBase
    {
        public EditAccountDialog()
            : base()
        {
            InitializeComponent();
        }

        public EditAccountDialog(AccountRow row)
            : base(row)
        {
            InitializeComponent();
        }
    }
}
