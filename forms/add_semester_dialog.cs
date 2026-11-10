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
    public sealed class AddSemesterDialog : Form
    {
        private static readonly Color Ink = Color.FromArgb(15, 23, 42);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Success = Color.FromArgb(4, 120, 87);

        private static readonly string[] SemesterNames =
        {
            "1ST SEMESTER",
            "2ND SEMESTER",
            "SUMMER"
        };

        private readonly DialogChrome.DragMover _drag = new DialogChrome.DragMover();

        private readonly DialogField _semesterField = DialogField.ForChoice("SEMESTER");
        private readonly DialogField _yearField = DialogField.ForText("SCHOOL YEAR (E.G. 2026-2027)");
        private readonly Label _preview = new Label();
        private readonly Label _note = new Label();
        private readonly DialogCloseButton _closeButton = new DialogCloseButton();
        private readonly RoundedButton _primaryButton = new RoundedButton();
        private readonly RoundedButton _secondaryButton = new RoundedButton();

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

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            KeyPreview = true;
            ClientSize = new Size(560, 436);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;
            Text = "ADD SEMESTER";

            BuildHeader();
            BuildFields();
            BuildFooter();

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

        private void BuildHeader()
        {
            Label accent = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Ink,
                Location = new Point(32, 26),
                Size = new Size(420, 30),
                Text = "ADD SEMESTER",
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label subtitle = new Label
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.75F),
                ForeColor = Muted,
                Location = new Point(34, 58),
                Size = new Size(470, 20),
                Text = "THE SEMESTER STUDENTS, FACULTY AND STAFF MAY EVALUATE.",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _closeButton.Location = new Point(ClientSize.Width - 32 - 34, 18);
            _closeButton.Size = new Size(34, 34);
            _closeButton.OnLightBackground = true;
            _closeButton.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(accent);
            Controls.Add(subtitle);
            Controls.Add(_closeButton);

            // The compact white header can be dragged like every other pop up.
            accent.MouseDown += (s, e) => _drag.Start(this, e);
            accent.MouseMove += (s, e) => _drag.Move(this, e);
            accent.MouseUp += (s, e) => _drag.End();
            MouseDown += (s, e) => _drag.Start(this, e);
            MouseMove += (s, e) => _drag.Move(this, e);
            MouseUp += (s, e) => _drag.End();
        }

        private void BuildFields()
        {
            _semesterField.Location = new Point(32, 96);
            _semesterField.Size = new Size(496, 88);

            _yearField.Location = new Point(32, 196);
            _yearField.Size = new Size(496, 88);

            _preview.AutoSize = false;
            _preview.BackColor = Color.Transparent;
            _preview.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            _preview.ForeColor = Success;
            _preview.Location = new Point(34, 292);
            _preview.Size = new Size(494, 22);
            _preview.TextAlign = ContentAlignment.MiddleLeft;

            _note.AutoSize = false;
            _note.BackColor = Color.Transparent;
            _note.Font = new Font("Segoe UI", 8F);
            _note.ForeColor = Muted;
            _note.Location = new Point(34, 318);
            _note.Size = new Size(494, 34);
            _note.Text = "ADDING A SEMESTER OPENS IT FOR EVALUATION RIGHT AWAY. ANY OTHER OPEN SEMESTER IS CLOSED FIRST.";
            _note.TextAlign = ContentAlignment.MiddleLeft;

            Controls.Add(_semesterField);
            Controls.Add(_yearField);
            Controls.Add(_preview);
            Controls.Add(_note);
        }

        private void BuildFooter()
        {
            Panel divider = new Panel
            {
                BackColor = Color.FromArgb(229, 231, 235),
                Location = new Point(0, 364),
                Size = new Size(ClientSize.Width, 1)
            };

            _secondaryButton.BackColor = Color.Transparent;
            _secondaryButton.CornerRadius = 11;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(140, 44);
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.Location = new Point(ClientSize.Width - 32 - 168 - 12 - 140, 376);
            _secondaryButton.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            _primaryButton.BackColor = Color.Transparent;
            _primaryButton.CornerRadius = 11;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 44);
            _primaryButton.Text = "OPEN SEMESTER";
            _primaryButton.UseVisualStyleBackColor = false;
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.Location = new Point(ClientSize.Width - 32 - 168, 376);
            _primaryButton.Click += (s, e) => Submit();

            AcceptButton = _primaryButton;

            Controls.Add(divider);
            Controls.Add(_secondaryButton);
            Controls.Add(_primaryButton);
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
