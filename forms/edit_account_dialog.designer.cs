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
            _accountCaption = new Label();
            _profileCaption = new Label();
            _profileNote = new Label();
            _nameField = new DialogField();
            _usernameField = new DialogField();
            _roleField = new DialogField();
            _statusField = new DialogField();
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
            _contentPanel.Controls.Add(_accountCaption);
            _contentPanel.Controls.Add(_profileCaption);
            _contentPanel.Controls.Add(_profileNote);
            _contentPanel.Controls.Add(_nameField);
            _contentPanel.Controls.Add(_usernameField);
            _contentPanel.Controls.Add(_roleField);
            _contentPanel.Controls.Add(_statusField);
            _contentPanel.Controls.Add(_schoolIdField);
            _contentPanel.Controls.Add(_programField);
            _contentPanel.Controls.Add(_yearLevelField);
            _contentPanel.Controls.Add(_instituteField);
            _contentPanel.Controls.Add(_contactField);
            _contentPanel.Controls.Add(_officeField);
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(900, 518);
            _contentPanel.TabIndex = 3;
            // 
            // _accountCaption
            // 
            _accountCaption.BackColor = Color.Transparent;
            _accountCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _accountCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _accountCaption.Location = new Point(34, 9);
            _accountCaption.Name = "_accountCaption";
            _accountCaption.Size = new Size(410, 18);
            _accountCaption.TabIndex = 0;
            _accountCaption.Text = "ACCOUNT ";
            // 
            // _profileCaption
            // 
            _profileCaption.BackColor = Color.Transparent;
            _profileCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _profileCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _profileCaption.Location = new Point(470, 9);
            _profileCaption.Name = "_profileCaption";
            _profileCaption.Size = new Size(410, 18);
            _profileCaption.TabIndex = 1;
            _profileCaption.Text = "PROFILE ";
            // 
            // _profileNote
            // 
            _profileNote.BackColor = Color.Transparent;
            _profileNote.Font = new Font("Segoe UI", 8.25F);
            _profileNote.ForeColor = Color.FromArgb(148, 163, 184);
            _profileNote.Location = new Point(470, 55);
            _profileNote.Name = "_profileNote";
            _profileNote.Size = new Size(400, 60);
            _profileNote.TabIndex = 12;
            _profileNote.Text = "THIS ACCOUNT HAS NO STUDENT, FACULTY, STAFF OR LIBRARIAN\r\nPROFILE ROW, SO ONLY THE ACCOUNT DETAILS CAN BE EDITED.";
            _profileNote.Visible = false;
            // 
            // _nameField
            // 
            _nameField.BackColor = Color.White;
            _nameField.Caption = "FULL NAME";
            _nameField.Kind = DialogFieldKind.Text;
            _nameField.Location = new Point(34, 45);
            _nameField.Name = "_nameField";
            _nameField.Size = new Size(400, 88);
            _nameField.TabIndex = 2;
            _nameField.Value = "";
            // 
            // _usernameField
            // 
            _usernameField.BackColor = Color.White;
            _usernameField.Caption = "USERNAME";
            _usernameField.Kind = DialogFieldKind.Text;
            _usernameField.Location = new Point(34, 135);
            _usernameField.Name = "_usernameField";
            _usernameField.Size = new Size(400, 88);
            _usernameField.TabIndex = 3;
            _usernameField.Value = "";
            // 
            // _roleField
            // 
            _roleField.BackColor = Color.White;
            _roleField.Caption = "ROLE";
            _roleField.Kind = DialogFieldKind.Choice;
            _roleField.Location = new Point(34, 225);
            _roleField.Name = "_roleField";
            _roleField.Size = new Size(400, 88);
            _roleField.TabIndex = 4;
            _roleField.Value = "";
            // 
            // _statusField
            // 
            _statusField.BackColor = Color.White;
            _statusField.Caption = "STATUS";
            _statusField.Kind = DialogFieldKind.Choice;
            _statusField.Location = new Point(34, 315);
            _statusField.Name = "_statusField";
            _statusField.Size = new Size(400, 88);
            _statusField.TabIndex = 5;
            _statusField.Value = "";
            // 
            // _schoolIdField
            // 
            _schoolIdField.BackColor = Color.White;
            _schoolIdField.Caption = "SCHOOL ID";
            _schoolIdField.Kind = DialogFieldKind.Text;
            _schoolIdField.Location = new Point(468, 45);
            _schoolIdField.Name = "_schoolIdField";
            _schoolIdField.Size = new Size(400, 88);
            _schoolIdField.TabIndex = 6;
            _schoolIdField.Value = "";
            // 
            // _programField
            // 
            _programField.BackColor = Color.White;
            _programField.Caption = "PROGRAM";
            _programField.Kind = DialogFieldKind.Text;
            _programField.Location = new Point(468, 135);
            _programField.Name = "_programField";
            _programField.Size = new Size(400, 88);
            _programField.TabIndex = 7;
            _programField.Value = "";
            // 
            // _yearLevelField
            // 
            _yearLevelField.BackColor = Color.White;
            _yearLevelField.Caption = "YEAR LEVEL";
            _yearLevelField.Kind = DialogFieldKind.Choice;
            _yearLevelField.Location = new Point(468, 225);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(400, 88);
            _yearLevelField.TabIndex = 8;
            _yearLevelField.Value = "";
            // 
            // _instituteField
            // 
            _instituteField.BackColor = Color.White;
            _instituteField.Caption = "INSTITUTE CODE";
            _instituteField.Kind = DialogFieldKind.Text;
            _instituteField.Location = new Point(468, 315);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(400, 88);
            _instituteField.TabIndex = 9;
            _instituteField.Value = "";
            // 
            // _contactField
            // 
            _contactField.BackColor = Color.White;
            _contactField.Caption = "CONTACT NUMBER";
            _contactField.Kind = DialogFieldKind.Text;
            _contactField.Location = new Point(468, 405);
            _contactField.Name = "_contactField";
            _contactField.Size = new Size(400, 88);
            _contactField.TabIndex = 10;
            _contactField.Value = "";
            // 
            // _officeField
            // 
            _officeField.BackColor = Color.White;
            _officeField.Caption = "OFFICE";
            _officeField.Kind = DialogFieldKind.Text;
            _officeField.Location = new Point(468, 30);
            _officeField.Name = "_officeField";
            _officeField.Size = new Size(400, 88);
            _officeField.TabIndex = 11;
            _officeField.Value = "";
            _officeField.Visible = false;
            // 
            // _footerDivider
            // 
            _footerDivider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, 622);
            _footerDivider.Name = "_footerDivider";
            _footerDivider.Size = new Size(900, 1);
            _footerDivider.TabIndex = 4;
            // 
            // _titleLabel
            // 
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.White;
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
            _subtitleLabel.ForeColor = Color.FromArgb(209, 250, 229);
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
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.Location = new Point(700, 638);
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
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.Location = new Point(568, 638);
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
            ClientSize = new Size(900, 700);
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
        private Label _accountCaption;
        private Label _profileCaption;
        private DialogField _nameField;
        private DialogField _usernameField;
        private DialogField _roleField;
        private DialogField _statusField;
        private DialogField _schoolIdField;
        private DialogField _programField;
        private DialogField _yearLevelField;
        private DialogField _instituteField;
        private DialogField _contactField;
        private DialogField _officeField;
        private Label _profileNote;
    }
}
