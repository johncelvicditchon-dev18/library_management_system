using System.Collections.Generic;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// The BORROW BOOK pop up. The complete content is built by
    /// <see cref="BorrowBookDialogBase"/> because the Windows Forms designer
    /// creates an instance of a form's base class while rendering it.
    /// </summary>
    public sealed partial class BorrowBookDialog : BorrowBookDialogBase
    {
        public BorrowBookDialog()
            : base()
        {
            InitializeComponent();
        }

        public BorrowBookDialog(IReadOnlyList<Book> availableBooks)
            : base(availableBooks)
        {
            InitializeComponent();
        }
    }
}
