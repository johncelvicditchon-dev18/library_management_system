using System.Collections.Generic;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// The EDIT BORROW RECORD pop up. The complete content is built by
    /// <see cref="EditBorrowRecordDialogBase"/> because the Windows Forms designer
    /// creates an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class EditBorrowRecordDialog : EditBorrowRecordDialogBase
    {
        public EditBorrowRecordDialog()
            : base()
        {
            InitializeComponent();
        }

        public EditBorrowRecordDialog(BorrowRecord record, IReadOnlyList<Book> books)
            : base(record, books)
        {
            InitializeComponent();
        }
    }
}
