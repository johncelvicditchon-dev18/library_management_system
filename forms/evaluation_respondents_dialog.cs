using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using oop_project.Models;
using oop_project.Reports;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up opened by the RESPONDENTS button in ANALYTICS. It lists the people who
    /// answered the selected semester: name, institute, program, role and student id.
    /// Faculty have no program or student id and staff have neither, so those cells
    /// stay blank instead of inventing a value.
    /// </summary>
    public sealed partial class EvaluationRespondentsDialog : Form
    {
        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        // Every row read for the semester. The search box filters this list, so
        // typing never touches MySQL again.
        private readonly List<EvaluationRespondent> _people = new List<EvaluationRespondent>();

        /// <summary>Design time constructor with no rows.</summary>
        public EvaluationRespondentsDialog()
            : this(string.Empty, new List<EvaluationRespondent>())
        {
        }

        public EvaluationRespondentsDialog(string periodTitle, List<EvaluationRespondent> respondents)
        {
            InitializeComponent();

            DoubleBuffered = true;
            DialogChrome.EnablePainting(this);

            periodLabel.Text = (periodTitle ?? string.Empty).Trim();
            _people.AddRange(respondents ?? new List<EvaluationRespondent>());

            // The white header can be dragged like every other pop up.
            DragControl(titleLabel);
            DragControl(periodLabel);
            MouseDown += (s, e) => _drag.Start(this, e);
            MouseMove += (s, e) => _drag.Move(this, e);
            MouseUp += (s, e) => _drag.End();

            closeButton.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            doneButton.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            exportButton.Click += (s, e) => ExportPdf();

            // Typing filters the five visible columns at once.
            searchBox.SearchTextChanged += (s, e) => ApplyFilter();

            ApplyFilter();

            Resize += (s, e) => DialogChrome.ApplyRoundedRegion(this);
            Shown += (s, e) =>
            {
                DialogChrome.ApplyRoundedRegion(this);

                // Creating the window can make the grid current again, so the
                // highlight is cleared once the list is really on screen.
                grid.CurrentCell = null;
                grid.ClearSelection();

                // Typing goes straight into the search bar.
                searchBox.FocusInput();
            };
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            DialogChrome.PaintCompactBackground(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void DragControl(Control control)
        {
            control.MouseDown += (s, e) => _drag.Start(this, e);
            control.MouseMove += (s, e) => _drag.Move(this, e);
            control.MouseUp += (s, e) => _drag.End();
        }

        /// <summary>
        /// Writes every respondent of the semester to a PDF that follows the Learning
        /// Resource Center form. Staff and faculty cells that have no value stay
        /// blank in the printed table.
        /// </summary>
        private void ExportPdf()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDF file (*.pdf)|*.pdf";
                dialog.FileName = "evaluation-respondents-" +
                                  DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";
                dialog.Title = "EXPORT RESPONDENTS AS PDF";
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    EvaluationRespondentsPdfExporter.Export(
                        periodLabel.Text, _people, dialog.FileName);

                    DialogChrome.ShowAlert(
                        "RESPONDENTS EXPORTED TO:\n" + dialog.FileName,
                        "PDF SAVED",
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    DialogChrome.ShowAlert(
                        "THE PDF COULD NOT BE SAVED.\n" + ex.Message,
                        "EXPORT FAILED",
                        MessageBoxIcon.Error);
                }
            }
        }

        private void FillRows(List<EvaluationRespondent> people)
        {
            grid.Rows.Clear();

            foreach (EvaluationRespondent person in people)
            {
                int index = grid.Rows.Add(
                    person.Name,
                    person.Institute,
                    person.Program,
                    person.Role,
                    person.StudentId);

                // A blank institute/program/student id is normal for faculty and
                // staff, so the row still alternates and selects as one record.
                grid.Rows[index].Tag = person;
            }

            // No row is "selected" until the librarian clicks one, so the list
            // opens without a highlighted record.
            grid.CurrentCell = null;
            grid.ClearSelection();
        }

        /// <summary>
        /// Filters the rows already on screen with the text in the search bar. The
        /// match checks every column shown from the database: name, institute,
        /// program, role and student id, so the librarian does not pick a field.
        /// </summary>
        private void ApplyFilter()
        {
            string term = searchBox.SearchText;
            List<EvaluationRespondent> matches = new List<EvaluationRespondent>();

            foreach (EvaluationRespondent person in _people)
            {
                if (Matches(person, term))
                {
                    matches.Add(person);
                }
            }

            FillRows(matches);

            int total = _people.Count;

            countLabel.Text = matches.Count == total
                ? total.ToString("N0") + (total == 1 ? " RESPONDENT" : " RESPONDENTS")
                : matches.Count.ToString("N0") + " OF " + total.ToString("N0") + " RESPONDENTS";

            emptyLabel.Visible = matches.Count == 0;

            if (matches.Count == 0)
            {
                emptyLabel.Text = total == 0 || term.Length == 0
                    ? "NO RESPONDENTS YET FOR THIS SEMESTER."
                    : "NO RESPONDENT MATCHES \"" + term.ToUpperInvariant() + "\".";

                emptyLabel.BringToFront();
            }
            else
            {
                emptyLabel.SendToBack();
            }
        }

        private static bool Matches(EvaluationRespondent person, string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return true;
            }

            return Contains(person.Name, term) ||
                   Contains(person.Institute, term) ||
                   Contains(person.Program, term) ||
                   Contains(person.Role, term) ||
                   Contains(person.StudentId, term);
        }

        private static bool Contains(string value, string term)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
