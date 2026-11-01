using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// The GENERATE REPORT pop up. The complete content is built by
    /// <see cref="LibraryReportDialogBase"/> because the Windows Forms designer
    /// creates an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class LibraryReportDialog : LibraryReportDialogBase
    {
        public LibraryReportDialog()
            : base()
        {
            InitializeComponent();
        }

        public LibraryReportDialog(LibraryCatalog catalog, string generatedBy)
            : base(catalog, generatedBy)
        {
            InitializeComponent();
        }
    }
}
