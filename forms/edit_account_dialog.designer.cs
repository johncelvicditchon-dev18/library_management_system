namespace oop_project.forms
{
    partial class EditAccountDialog
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
            _contentPanel = new Panel();
            _profileNote = new Label();
            _nameField = new DialogField();
            _usernameField = new DialogField();
            _schoolIdField = new DialogField();
            _programField = new DialogField();
            _yearLevelField = new DialogField();
            _instituteField = new DialogField();
            _contactField = new DialogField();
            _officeField = new DialogField();
            _footerDivider = new Panel();
            _titleLabel = new Label();
            _subtitleLabel = new Label();
            _closeButton = new DialogCloseButton();
            _primaryButton = new RoundedButton();
            _secondaryButton = new RoundedButton();
            _contentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _contentPanel
            // 
            _contentPanel.BackColor = Color.White;
            _contentPanel.Controls.Add(_profileNote);
            _contentPanel.Controls.Add(_nameField);
            _contentPanel.Controls.Add(_usernameField);
            _contentPanel.Controls.Add(_schoolIdField);
            _contentPanel.Controls.Add(_programField);
            _contentPanel.Controls.Add(_yearLevelField);
            _contentPanel.Controls.Add(_instituteField);
            _contentPanel.Controls.Add(_contactField);
            _contentPanel.Controls.Add(_officeField);
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(900, 448);
            _contentPanel.TabIndex = 3;
            // 
            // _profileNote
            // 
            _profileNote.BackColor = Color.Transparent;
            _profileNote.Font = new Font("Segoe UI", 8.25F);
            _profileNote.ForeColor = Color.FromArgb(148, 163, 184);
            _profileNote.Location = new Point(36, 232);
            _profileNote.Name = "_profileNote";
            _profileNote.Size = new Size(448, 60);
            _profileNote.TabIndex = 9;
            _profileNote.Text = "THIS ACCOUNT HAS NO STUDENT, FACULTY OR STAFF PROFILE ROW,\r\nSO ONLY THE NAME AND USERNAME CAN BE EDITED.";
            _profileNote.Visible = false;
            // 
            // _nameField
            // 
            _nameField.BackColor = Color.White;
            _nameField.Caption = "FULL NAME";
            _nameField.Kind = DialogFieldKind.Text;
            _nameField.Location = new Point(32, 30);
            _nameField.Name = "_nameField";
            _nameField.Size = new Size(406, 88);
            _nameField.TabIndex = 0;
            _nameField.Value = "";
            // 
            // _usernameField
            // 
            _usernameField.BackColor = Color.White;
            _usernameField.Caption = "USERNAME";
            _usernameField.Kind = DialogFieldKind.Text;
            _usernameField.Location = new Point(32, 130);
            _usernameField.Name = "_usernameField";
            _usernameField.Size = new Size(406, 88);
            _usernameField.TabIndex = 2;
            _usernameField.Value = "";
            // 
            // _schoolIdField
            // 
            _schoolIdField.BackColor = Color.White;
            _schoolIdField.Caption = "SCHOOL ID";
            _schoolIdField.Kind = DialogFieldKind.Text;
            _schoolIdField.Location = new Point(462, 30);
            _schoolIdField.Name = "_schoolIdField";
            _schoolIdField.Size = new Size(406, 88);
            _schoolIdField.TabIndex = 1;
            _schoolIdField.Value = "";
            // 
            // _programField
            // 
            _programField.BackColor = Color.White;
            _programField.Caption = "PROGRAM";
            _programField.Kind = DialogFieldKind.Choice;
            _programField.Location = new Point(462, 230);
            _programField.Name = "_programField";
            _programField.Size = new Size(406, 88);
            _programField.TabIndex = 5;
            _programField.Value = "";
            // 
            // _yearLevelField
            // 
            _yearLevelField.BackColor = Color.White;
            _yearLevelField.Caption = "YEAR LEVEL";
            _yearLevelField.Kind = DialogFieldKind.Choice;
            _yearLevelField.Location = new Point(32, 230);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(406, 88);
            _yearLevelField.TabIndex = 4;
            _yearLevelField.Value = "";
            // 
            // _instituteField
            // 
            _instituteField.BackColor = Color.White;
            _instituteField.Caption = "INSTITUTE";
            _instituteField.Kind = DialogFieldKind.Choice;
            _instituteField.Location = new Point(462, 130);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(406, 88);
            _instituteField.TabIndex = 3;
            _instituteField.Value = "";
            // 
            // _contactField
            // 
            _contactField.BackColor = Color.White;
            _contactField.Caption = "CONTACT NUMBER";
            _contactField.Kind = DialogFieldKind.Text;
            _contactField.Location = new Point(32, 330);
            _contactField.Name = "_contactField";
            _contactField.Size = new Size(836, 88);
            _contactField.TabIndex = 6;
            _contactField.Value = "";
            // 
            // _officeField
            // 
            _officeField.BackColor = Color.White;
            _officeField.Caption = "OFFICE";
            _officeField.Kind = DialogFieldKind.Text;
            _officeField.Location = new Point(36, 230);
            _officeField.Name = "_officeField";
            _officeField.Size = new Size(448, 88);
            _officeField.TabIndex = 7;
            _officeField.Value = "";
            _officeField.Visible = false;
            // 
            // _footerDivider
            // 
            _footerDivider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, 552);
            _footerDivider.Name = "_footerDivider";
            _footerDivider.Size = new Size(900, 1);
            _footerDivider.TabIndex = 4;
            // 
            // _titleLabel
            // 
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            _titleLabel.Location = new Point(32, 22);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(770, 32);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "EDIT ACCOUNT";
            _titleLabel.MouseDown += DragStart;
            _titleLabel.MouseMove += DragMove;
            _titleLabel.MouseUp += DragEnd;
            // 
            // _subtitleLabel
            // 
            _subtitleLabel.BackColor = Color.Transparent;
            _subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            _subtitleLabel.ForeColor = Color.FromArgb(100, 116, 139);
            _subtitleLabel.Location = new Point(34, 60);
            _subtitleLabel.Name = "_subtitleLabel";
            _subtitleLabel.Size = new Size(760, 22);
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = "Change the account or its profile, then press UPDATE ACCOUNT.";
            _subtitleLabel.MouseDown += DragStart;
            _subtitleLabel.MouseMove += DragMove;
            _subtitleLabel.MouseUp += DragEnd;
            // 
            // _closeButton
            // 
            _closeButton.BackColor = Color.Transparent;
            _closeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _closeButton.Location = new Point(834, 20);
            _closeButton.Name = "_closeButton";
            _closeButton.OnLightBackground = true;
            _closeButton.Size = new Size(34, 34);
            _closeButton.TabIndex = 2;
            _closeButton.TabStop = false;
            _closeButton.Click += CloseButton_Click;
            // 
            // _primaryButton
            // 
            _primaryButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _primaryButton.BackColor = Color.Transparent;
            _primaryButton.CornerRadius = 12;
            _primaryButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.IconFallbackGlyph = "";
            _primaryButton.IconGlyph = "";
            _primaryButton.IconSize = 16F;
            _primaryButton.Location = new Point(700, 568);
            _primaryButton.Name = "_primaryButton";
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 46);
            _primaryButton.TabIndex = 5;
            _primaryButton.Text = "UPDATE ACCOUNT";
            _primaryButton.UseVisualStyleBackColor = false;
            // 
            // _secondaryButton
            // 
            _secondaryButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _secondaryButton.BackColor = Color.Transparent;
            _secondaryButton.CornerRadius = 12;
            _secondaryButton.DialogResult = DialogResult.Cancel;
            _secondaryButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.IconFallbackGlyph = "";
            _secondaryButton.IconGlyph = "";
            _secondaryButton.IconSize = 16F;
            _secondaryButton.Location = new Point(568, 568);
            _secondaryButton.Name = "_secondaryButton";
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(120, 46);
            _secondaryButton.TabIndex = 6;
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;
            // 
            // EditAccountDialog
            // 
            AcceptButton = _primaryButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = _secondaryButton;
            ClientSize = new Size(900, 630);
            Controls.Add(_contentPanel);
            Controls.Add(_footerDivider);
            Controls.Add(_titleLabel);
            Controls.Add(_subtitleLabel);
            Controls.Add(_closeButton);
            Controls.Add(_primaryButton);
            Controls.Add(_secondaryButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditAccountDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "EDIT ACCOUNT";
            MouseDown += DragStart;
            MouseMove += DragMove;
            MouseUp += DragEnd;
            _contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel _contentPanel;
        private Panel _footerDivider;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private DialogCloseButton _closeButton;
        private RoundedButton _primaryButton;
        private RoundedButton _secondaryButton;
        private DialogField _nameField;
        private DialogField _usernameField;
        private DialogField _schoolIdField;
        private DialogField _programField;
        private DialogField _yearLevelField;
        private DialogField _instituteField;
        private DialogField _contactField;
        private DialogField _officeField;
        private Label _profileNote;
    }
}
