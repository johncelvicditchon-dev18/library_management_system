namespace oop_project.forms
{
    partial class AddSemesterDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _titleLabel = new Label();
            _subtitleLabel = new Label();
            _semesterField = new DialogField();
            _yearField = new DialogField();
            _preview = new Label();
            _note = new Label();
            _footerDivider = new Panel();
            _closeButton = new DialogCloseButton();
            _secondaryButton = new RoundedButton();
            _primaryButton = new RoundedButton();
            SuspendLayout();
            // 
            // _titleLabel
            // 
            _titleLabel.AutoSize = false;
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            _titleLabel.Location = new Point(32, 26);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(420, 30);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "ADD SEMESTER";
            _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _subtitleLabel
            // 
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.BackColor = Color.Transparent;
            _subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            _subtitleLabel.ForeColor = Color.FromArgb(100, 116, 139);
            _subtitleLabel.Location = new Point(34, 58);
            _subtitleLabel.Name = "_subtitleLabel";
            _subtitleLabel.Size = new Size(470, 20);
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = "THE SEMESTER STUDENTS, FACULTY AND STAFF MAY EVALUATE.";
            _subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _semesterField
            // 
            _semesterField.BackColor = Color.White;
            _semesterField.Caption = "SEMESTER";
            _semesterField.Kind = DialogFieldKind.Choice;
            _semesterField.Location = new Point(32, 96);
            _semesterField.Name = "_semesterField";
            _semesterField.Size = new Size(496, 88);
            _semesterField.TabIndex = 2;
            _semesterField.Value = "";
            // 
            // _yearField
            // 
            _yearField.BackColor = Color.White;
            _yearField.Caption = "SCHOOL YEAR (E.G. 2026-2027)";
            _yearField.Kind = DialogFieldKind.Text;
            _yearField.Location = new Point(32, 196);
            _yearField.Name = "_yearField";
            _yearField.Size = new Size(496, 88);
            _yearField.TabIndex = 3;
            _yearField.Value = "";
            // 
            // _preview
            // 
            _preview.AutoSize = false;
            _preview.BackColor = Color.Transparent;
            _preview.Font = new Font("Segoe UI", 9.25F, FontStyle.Bold);
            _preview.ForeColor = Color.FromArgb(4, 120, 87);
            _preview.Location = new Point(34, 292);
            _preview.Name = "_preview";
            _preview.Size = new Size(494, 22);
            _preview.TabIndex = 4;
            _preview.Text = "WILL OPEN:  1ST SEMESTER  2026-2027";
            _preview.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _note
            // 
            _note.AutoSize = false;
            _note.BackColor = Color.Transparent;
            _note.Font = new Font("Segoe UI", 8F);
            _note.ForeColor = Color.FromArgb(100, 116, 139);
            _note.Location = new Point(34, 318);
            _note.Name = "_note";
            _note.Size = new Size(494, 34);
            _note.TabIndex = 5;
            _note.Text = "ADDING A SEMESTER OPENS IT FOR EVALUATION RIGHT AWAY. ANY OTHER OPEN SEMESTER IS CLOSED FIRST.";
            _note.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _footerDivider
            // 
            _footerDivider.BackColor = Color.FromArgb(229, 231, 235);
            _footerDivider.Location = new Point(0, 364);
            _footerDivider.Name = "_footerDivider";
            _footerDivider.Size = new Size(560, 1);
            _footerDivider.TabIndex = 6;
            // 
            // _closeButton
            // 
            _closeButton.Location = new Point(494, 18);
            _closeButton.Name = "_closeButton";
            _closeButton.OnLightBackground = true;
            _closeButton.Size = new Size(34, 34);
            _closeButton.TabIndex = 7;
            // 
            // _secondaryButton
            // 
            _secondaryButton.BackColor = Color.Transparent;
            _secondaryButton.CornerRadius = 11;
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.Location = new Point(208, 376);
            _secondaryButton.Name = "_secondaryButton";
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(140, 44);
            _secondaryButton.TabIndex = 8;
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;
            // 
            // _primaryButton
            // 
            _primaryButton.BackColor = Color.Transparent;
            _primaryButton.CornerRadius = 11;
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.Location = new Point(360, 376);
            _primaryButton.Name = "_primaryButton";
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 44);
            _primaryButton.TabIndex = 9;
            _primaryButton.Text = "OPEN SEMESTER";
            _primaryButton.UseVisualStyleBackColor = false;
            // 
            // AddSemesterDialog
            // 
            AcceptButton = _primaryButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(560, 436);
            Controls.Add(_titleLabel);
            Controls.Add(_subtitleLabel);
            Controls.Add(_closeButton);
            Controls.Add(_semesterField);
            Controls.Add(_yearField);
            Controls.Add(_preview);
            Controls.Add(_note);
            Controls.Add(_footerDivider);
            Controls.Add(_secondaryButton);
            Controls.Add(_primaryButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Name = "AddSemesterDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ADD SEMESTER";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label _titleLabel;
        private Label _subtitleLabel;
        private DialogField _semesterField;
        private DialogField _yearField;
        private Label _preview;
        private Label _note;
        private Panel _footerDivider;
        private DialogCloseButton _closeButton;
        private RoundedButton _secondaryButton;
        private RoundedButton _primaryButton;
    }
}
