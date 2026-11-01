using oop_project.Models;

namespace oop_project
{
    /// <summary>
    /// The BOOK DESCRIPTION pop up. The complete content is built by
    /// <see cref="BookDescriptionDialogBase"/> because the Windows Forms designer
    /// creates an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class BookDescriptionDialog : BookDescriptionDialogBase
    {
        public BookDescriptionDialog()
            : base()
        {
            InitializeComponent();
        }

        public BookDescriptionDialog(Book book, int available, string portalName)
            : base(book, available, portalName)
        {
            InitializeComponent();
        }
    }
}
