using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// The EDIT BOOK pop up. The complete content is built by
    /// <see cref="EditBookDialogBase"/> because the Windows Forms designer creates
    /// an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class EditBookDialog : EditBookDialogBase
    {
        public EditBookDialog()
            : base()
        {
            InitializeComponent();
        }

        public EditBookDialog(Book book)
            : base(book)
        {
            InitializeComponent();
        }
    }
}
