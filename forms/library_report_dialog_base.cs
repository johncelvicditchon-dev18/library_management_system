using System;
using System.IO;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by GENERATE REPORT. It previews the report, shows the
    /// headline numbers and lets the staff save the document as a text file.
    ///
    /// The complete control tree is serialized in the designer file so the
    /// Windows Forms designer renders the real dialog instead of a blank form.
    /// </summary>
    public partial class LibraryReportDialogBase : LibraryDialog
    {
        private readonly string _reportText;

        /// <summary>Design time constructor with an empty catalog.</summary>
        public LibraryReportDialogBase()
            : this(new LibraryCatalog(), "SAMPLE STAFF")
        {
        }

        public LibraryReportDialogBase(LibraryCatalog catalog, string generatedBy)
            : base("GENERATE REPORT", "Summary of the catalog, the loans and the returns.", 900, 660)
        {
            InitializeComponent();

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _reportText = catalog.BuildReportText(generatedBy, DateTime.Now);

            _titleValue.Text = catalog.TotalBooks.ToString();
            _availableValue.Text = catalog.AvailableBooks.ToString();
            _borrowedValue.Text = catalog.BorrowedBooks.ToString();
            _returnedValue.Text = catalog.ReturnedLoans.ToString();
            _previewTextBox.Text = _reportText;
            _previewTextBox.SelectionStart = 0;
            _previewTextBox.SelectionLength = 0;

            PrimaryButton.Click += SaveReport_Click;
        }

        private void SaveReport_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "library-report-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                dialog.Title = "SAVE LIBRARY REPORT";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, _reportText);
                    ShowAlert("REPORT SAVED TO:\n" + dialog.FileName, "REPORT SAVED", MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    ShowAlert("THE REPORT COULD NOT BE SAVED.\n" + ex.Message, "REPORT NOT SAVED", MessageBoxIcon.Error);
                }
            }
        }
    }
}
