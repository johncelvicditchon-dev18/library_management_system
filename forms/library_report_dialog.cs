using System;
using System.IO;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by GENERATE REPORT. It previews the report, shows the
    /// headline numbers and lets the staff save the document as a text file.
    /// </summary>
    public sealed partial class LibraryReportDialog : Form
    {
        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();
        private readonly string _reportText;

        /// <summary>Design time constructor with an empty catalog.</summary>
        public LibraryReportDialog()
            : this(new LibraryCatalog(), "SAMPLE STAFF")
        {
        }

        public LibraryReportDialog(LibraryCatalog catalog, string generatedBy)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();
            DialogChrome.ApplyParameters(this, "GENERATE REPORT",
                "Summary of the catalog, the loans and the returns.", 900, 660,
                _contentPanel, _footerDivider, _titleLabel, _subtitleLabel,
                _closeButton, _primaryButton, _secondaryButton);

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

            _primaryButton.Click += SaveReport_Click;
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
                    DialogChrome.ShowAlert("REPORT SAVED TO:\n" + dialog.FileName, "REPORT SAVED", MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    DialogChrome.ShowAlert("THE REPORT COULD NOT BE SAVED.\n" + ex.Message, "REPORT NOT SAVED", MessageBoxIcon.Error);
                }
            }
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DragStart(object? sender, MouseEventArgs e)
        {
            _drag.Start(this, e);
        }

        private void DragMove(object? sender, MouseEventArgs e)
        {
            _drag.Move(this, e);
        }

        private void DragEnd(object? sender, MouseEventArgs e)
        {
            _drag.End();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            DialogChrome.ApplyRoundedRegion(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DialogChrome.ApplyRoundedRegion(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                DialogChrome.ApplyDropShadow(parameters);
                return parameters;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            DialogChrome.PaintBackground(this, e);
        }
    }
}
