namespace oop_project.forms
{
    partial class MyProfileDialog
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
            _nameField = new DialogField();
            _usernameField = new DialogField();
            _contactField = new DialogField();
            _addressField = new DialogField();
            _schoolIdField = new DialogField();
            _programField = new DialogField();
            _yearLevelField = new DialogField();
            _instituteField = new DialogField();
            _officeField = new DialogField();
            _footerDivider = new Panel();
            _titleLabel = new Label();
            _closeButton = new DialogCloseButton();
            _saveButton = new RoundedButton();
            _cancelButton = new RoundedButton();
            _contentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _contentPanel
            // 
            _contentPanel.BackColor = Color.White;
            _contentPanel.Controls.Add(_nameField);
            _contentPanel.Controls.Add(_usernameField);
            _contentPanel.Controls.Add(_contactField);
            _contentPanel.Controls.Add(_addressField);
            _contentPanel.Controls.Add(_schoolIdField);
            _contentPanel.Controls.Add(_programField);
            _contentPanel.Controls.Add(_yearLevelField);
            _contentPanel.Controls.Add(_instituteField);
            _contentPanel.Controls.Add(_officeField);
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(900, 400);
            _contentPanel.TabIndex = 3;
            // 
            // _nameField
            // 
            _nameField.BackColor = Color.White;
            _nameField.Caption = "FULL NAME";
            _nameField.Kind = DialogFieldKind.Text;
            _nameField.Location = new Point(34, 22);
            _nameField.Name = "_nameField";
            _nameField.Size = new Size(400, 88);
            _nameField.TabIndex = 0;
            _nameField.Value = "";
            // 
            // _usernameField
            // 
            _usernameField.BackColor = Color.White;
            _usernameField.Caption = "USERNAME";
            _usernameField.Kind = DialogFieldKind.Text;
            _usernameField.Location = new Point(468, 22);
            _usernameField.Name = "_usernameField";
            _usernameField.Size = new Size(400, 88);
            _usernameField.TabIndex = 1;
            _usernameField.Value = "";
            // 
            // _contactField
            // 
            _contactField.BackColor = Color.White;
            _contactField.Caption = "CONTACT NUMBER";
            _contactField.Kind = DialogFieldKind.Text;
            _contactField.Location = new Point(34, 112);
            _contactField.Name = "_contactField";
            _contactField.Size = new Size(400, 88);
            _contactField.TabIndex = 2;
            _contactField.Value = "";
            // 
            // _addressField
            // 
            _addressField.BackColor = Color.White;
            _addressField.Caption = "ADDRESS";
            _addressField.Kind = DialogFieldKind.Text;
            _addressField.Location = new Point(468, 112);
            _addressField.Name = "_addressField";
            _addressField.Size = new Size(400, 88);
            _addressField.TabIndex = 3;
            _addressField.Value = "";
            // 
            // _schoolIdField
            // 
            _schoolIdField.BackColor = Color.White;
            _schoolIdField.Caption = "SCHOOL ID";
            _schoolIdField.Kind = DialogFieldKind.Text;
            _schoolIdField.Location = new Point(34, 202);
            _schoolIdField.Name = "_schoolIdField";
            _schoolIdField.Size = new Size(400, 88);
            _schoolIdField.TabIndex = 4;
            _schoolIdField.Value = "";
            // 
            // _programField
            // 
            _programField.BackColor = Color.White;
            _programField.Caption = "PROGRAM";
            _programField.Kind = DialogFieldKind.Choice;
            _programField.Location = new Point(34, 292);
            _programField.Name = "_programField";
            _programField.Size = new Size(400, 88);
            _programField.TabIndex = 5;
            _programField.Value = "";
            // 
            // _yearLevelField
            // 
            _yearLevelField.BackColor = Color.White;
            _yearLevelField.Caption = "YEAR LEVEL";
            _yearLevelField.Kind = DialogFieldKind.Choice;
            _yearLevelField.Location = new Point(468, 292);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(400, 88);
            _yearLevelField.TabIndex = 6;
            _yearLevelField.Value = "";
            // 
            // _instituteField
            // 
            _instituteField.BackColor = Color.White;
            _instituteField.Caption = "INSTITUTE CODE";
            _instituteField.Kind = DialogFieldKind.Choice;
            _instituteField.Location = new Point(468, 202);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(400, 88);
            _instituteField.TabIndex = 7;
            _instituteField.Value = "";
            // 
            // _officeField
            // 
            _officeField.BackColor = Color.White;
            _officeField.Caption = "OFFICE";
            _officeField.Kind = DialogFieldKind.Text;
            _officeField.Location = new Point(468, 112);
            _officeField.Name = "_officeField";
            _officeField.Size = new Size(400, 88);
            _officeField.TabIndex = 8;
            _officeField.Value = "";
            _officeField.Visible = false;
            // 
            // _footerDivider
            // 
            _footerDivider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, 504);
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
            _titleLabel.Text = "MY PROFILE";
            _titleLabel.MouseDown += DragStart;
            _titleLabel.MouseMove += DragMove;
            _titleLabel.MouseUp += DragEnd;
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
            // _saveButton
            // 
            _saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _saveButton.BackColor = Color.Transparent;
            _saveButton.CornerRadius = 12;
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.FlatStyle = FlatStyle.Flat;
            _saveButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _saveButton.ForeColor = Color.White;
            _saveButton.HoverColor = Color.FromArgb(21, 128, 61);
            _saveButton.Location = new Point(700, 520);
            _saveButton.Name = "_saveButton";
            _saveButton.NormalColor = Color.FromArgb(22, 163, 74);
            _saveButton.PressedColor = Color.FromArgb(20, 83, 45);
            _saveButton.Size = new Size(168, 46);
            _saveButton.TabIndex = 5;
            _saveButton.Text = "SAVE CHANGES";
            _saveButton.UseVisualStyleBackColor = false;
            // 
            // _cancelButton
            // 
            _cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _cancelButton.BackColor = Color.Transparent;
            _cancelButton.CornerRadius = 12;
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _cancelButton.ForeColor = Color.FromArgb(51, 65, 85);
            _cancelButton.HoverColor = Color.FromArgb(226, 232, 240);
            _cancelButton.Location = new Point(568, 520);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.NormalColor = Color.FromArgb(241, 245, 249);
            _cancelButton.PressedColor = Color.FromArgb(203, 213, 225);
            _cancelButton.Size = new Size(120, 46);
            _cancelButton.TabIndex = 6;
            _cancelButton.Text = "CANCEL";
            _cancelButton.UseVisualStyleBackColor = false;
            // 
            // MyProfileDialog
            // 
            AcceptButton = _saveButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = _cancelButton;
            ClientSize = new Size(900, 582);
            Controls.Add(_contentPanel);
            Controls.Add(_footerDivider);
            Controls.Add(_titleLabel);
            Controls.Add(_closeButton);
            Controls.Add(_saveButton);
            Controls.Add(_cancelButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MyProfileDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "MY PROFILE";
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
        private DialogCloseButton _closeButton;
        private RoundedButton _saveButton;
        private RoundedButton _cancelButton;
        private DialogField _nameField;
        private DialogField _usernameField;
        private DialogField _contactField;
        private DialogField _addressField;
        private DialogField _schoolIdField;
        private DialogField _programField;
        private DialogField _yearLevelField;
        private DialogField _instituteField;
        private DialogField _officeField;
    }
}
