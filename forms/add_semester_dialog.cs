using System;
using System.Drawing;
using System.Windows.Forms;

namespace oop_project.forms
{
    /// <summary>
    /// Small pop up used by the ANALYTICS tab so the librarian adds the semester
    /// that students, faculty and staff may evaluate, for example
    /// "1ST SEMESTER 2026-2027". It only collects and validates the term; the
    /// dashboard opens it in evaluation_period_table through
    /// <see cref="Models.EvaluationDirectory.OpenPeriod"/>.
    /// </summary>
    public sealed partial class AddSemesterDialog : Form
    {
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Success = Color.FromArgb(4, 120, 87);

        private static readonly string[] SemesterNames =
        {
            "1ST SEMESTER",
            "2ND SEMESTER",
            "SUMMER"
        };

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        /// <summary>"1ST SEMESTER", "2ND SEMESTER" or "SUMMER".</summary>
        public string Semester { get; private set; } = string.Empty;

        /// <summary>"2026-2027".</summary>
        public string SchoolYear { get; private set; } = string.Empty;

        public AddSemesterDialog()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Builds the pop up with the term the dashboard suggests for today, so the
        /// librarian normally just presses ADD &amp; OPEN.
        /// </summary>
        public AddSemesterDialog(string suggestedSemester, string suggestedSchoolYear)
        {
            DialogChrome.EnablePainting(this);
            InitializeComponent();

            // RUNTIME DATA: the semester list and the suggested term are filled
            // after the designer has created the controls, so the Design View
            // never tries to reach the database or the clock.
            foreach (string name in SemesterNames)
            {
                _semesterField.ChoiceBox.Items.Add(name);
            }

            int suggestedIndex = Array.FindIndex(
                SemesterNames,
                name => string.Equals(name, suggestedSemester, StringComparison.OrdinalIgnoreCase));

            _semesterField.ChoiceBox.SelectedIndex = suggestedIndex >= 0 ? suggestedIndex : 0;
            _yearField.Value = string.IsNullOrWhiteSpace(suggestedSchoolYear)
                ? DefaultSchoolYear()
                : suggestedSchoolYear.Trim();

            _semesterField.ValueChanged += (s, e) => UpdatePreview();
            _yearField.ValueChanged += (s, e) => UpdatePreview();

            UpdatePreview();

            Resize += (s, e) => DialogChrome.ApplyRoundedRegion(this);
            Shown += (s, e) =>
            {
                DialogChrome.ApplyRoundedRegion(this);
                _semesterField.FocusInput();
            };

            _closeButton.Click += (s, e) => CancelAndClose();
            _secondaryButton.Click += (s, e) => CancelAndClose();
            _primaryButton.Click += (s, e) => Submit();

            // The compact white header can be dragged like every other pop up.
            _titleLabel.MouseDown += (s, e) => _drag.Start(this, e);
            _titleLabel.MouseMove += (s, e) => _drag.Move(this, e);
            _titleLabel.MouseUp += (s, e) => _drag.End();
            MouseDown += (s, e) => _drag.Start(this, e);
            MouseMove += (s, e) => _drag.Move(this, e);
            MouseUp += (s, e) => _drag.End();
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
                CancelAndClose();
            }
        }

        private void CancelAndClose()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void UpdatePreview()
        {
            string semester = _semesterField.Value;

            if (TryNormalizeSchoolYear(_yearField.Value, out string year))
            {
                _preview.Text = "WILL OPEN:  " + semester + "  " + year;
                _preview.ForeColor = Success;
            }
            else
            {
                _preview.Text = "WILL OPEN:  " + semester + "  " + _yearField.Value.Trim();
                _preview.ForeColor = Muted;
            }
        }

        private void Submit()
        {
            _semesterField.ClearError();
            _yearField.ClearError();

            string semester = _semesterField.Value.Trim();

            if (semester.Length == 0)
            {
                _semesterField.ShowError("Choose the semester to open.");
                _semesterField.FocusInput();
                return;
            }

            if (!TryNormalizeSchoolYear(_yearField.Value, out string year))
            {
                _yearField.ShowError("Type the school year as 2026-2027.");
                _yearField.FocusInput();
                return;
            }

            Semester = semester.ToUpperInvariant();
            SchoolYear = year;

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Accepts "2026-2027", "AY 2026-2027", "2026 - 2027", "2026/2027" and the
        /// en dash the word processor inserts, then returns the canonical form.
        /// </summary>
        private static bool TryNormalizeSchoolYear(string? input, out string normalized)
        {
            normalized = string.Empty;

            string value = (input ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Replace("AY", string.Empty)
                .Replace('\u2013', '-')
                .Replace('\u2014', '-')
                .Replace('/', '-')
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty);

            string[] parts = value.Split('-');

            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
            {
                return false;
            }

            if (start < 1900 || start > 2999 || end != start + 1)
            {
                return false;
            }

            normalized = start + "-" + end;
            return true;
        }

        private static string DefaultSchoolYear()
        {
            DateTime today = DateTime.Now;
            int start = today.Month >= 6 ? today.Year : today.Year - 1;

            return start + "-" + (start + 1);
        }
    }
}
