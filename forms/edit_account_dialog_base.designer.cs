namespace oop_project.forms
{
    partial class EditAccountDialogBase
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
            _accountCaption = new Label();
            _profileCaption = new Label();
            _nameField = DialogField.ForText("FULL NAME");
            _usernameField = DialogField.ForText("USERNAME");
            _roleField = DialogField.ForChoice("ROLE");
            _statusField = DialogField.ForChoice("STATUS");
            _schoolIdField = DialogField.ForText("SCHOOL ID");
            _programField = DialogField.ForText("PROGRAM");
            _yearLevelField = DialogField.ForChoice("YEAR LEVEL");
            _instituteField = DialogField.ForText("INSTITUTE CODE");
            _contactField = DialogField.ForText("CONTACT NUMBER");
            _officeField = DialogField.ForText("OFFICE");
            _profileNote = new Label();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _accountCaption
            // 
            _accountCaption.AutoSize = false;
            _accountCaption.BackColor = Color.Transparent;
            _accountCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _accountCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _accountCaption.Location = new Point(34, 4);
            _accountCaption.Name = "_accountCaption";
            _accountCaption.Size = new Size(410, 18);
            _accountCaption.TabIndex = 0;
            _accountCaption.Text = "ACCOUNT (account_table + account_role_table)";
            // 
            // _profileCaption
            // 
            _profileCaption.AutoSize = false;
            _profileCaption.BackColor = Color.Transparent;
            _profileCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _profileCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _profileCaption.Location = new Point(470, 4);
            _profileCaption.Name = "_profileCaption";
            _profileCaption.Size = new Size(410, 18);
            _profileCaption.TabIndex = 1;
            _profileCaption.Text = "PROFILE (student_info_table / faculty_info_table / staff_info_table)";
            // 
            // _nameField
            // 
            _nameField.Input.MaxLength = 250;
            _nameField.Location = new Point(32, 30);
            _nameField.Name = "_nameField";
            _nameField.Size = new Size(400, 88);
            _nameField.TabIndex = 2;
            _nameField.Width = 400;
            // 
            // _usernameField
            // 
            _usernameField.Input.MaxLength = 250;
            _usernameField.Location = new Point(32, 120);
            _usernameField.Name = "_usernameField";
            _usernameField.Size = new Size(400, 88);
            _usernameField.TabIndex = 3;
            _usernameField.Width = 400;
            // 
            // _roleField
            // 
            _roleField.ChoiceBox.Items.AddRange(Roles);
            _roleField.Location = new Point(32, 210);
            _roleField.Name = "_roleField";
            _roleField.Size = new Size(400, 88);
            _roleField.TabIndex = 4;
            _roleField.Width = 400;
            // 
            // _statusField
            // 
            _statusField.ChoiceBox.Items.AddRange(Statuses);
            _statusField.Location = new Point(32, 300);
            _statusField.Name = "_statusField";
            _statusField.Size = new Size(400, 88);
            _statusField.TabIndex = 5;
            _statusField.Width = 400;
            // 
            // _schoolIdField
            // 
            _schoolIdField.Input.MaxLength = 30;
            _schoolIdField.Location = new Point(468, 30);
            _schoolIdField.Name = "_schoolIdField";
            _schoolIdField.Size = new Size(400, 88);
            _schoolIdField.TabIndex = 6;
            _schoolIdField.Width = 400;
            // 
            // _programField
            // 
            _programField.Input.MaxLength = 120;
            _programField.Location = new Point(468, 120);
            _programField.Name = "_programField";
            _programField.Size = new Size(400, 88);
            _programField.TabIndex = 7;
            _programField.Width = 400;
            // 
            // _yearLevelField
            // 
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            _yearLevelField.Location = new Point(468, 210);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(400, 88);
            _yearLevelField.TabIndex = 8;
            _yearLevelField.Width = 400;
            // 
            // _instituteField
            // 
            _instituteField.Input.MaxLength = 10;
            _instituteField.Location = new Point(468, 300);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(400, 88);
            _instituteField.TabIndex = 9;
            _instituteField.Width = 400;
            // 
            // _contactField
            // 
            _contactField.Input.MaxLength = 20;
            _contactField.Location = new Point(468, 390);
            _contactField.Name = "_contactField";
            _contactField.Size = new Size(400, 88);
            _contactField.TabIndex = 10;
            _contactField.Width = 400;
            // 
            // _officeField
            // 
            _officeField.Input.MaxLength = 120;
            _officeField.Location = new Point(468, 30);
            _officeField.Name = "_officeField";
            _officeField.Size = new Size(400, 88);
            _officeField.TabIndex = 11;
            _officeField.Visible = false;
            _officeField.Width = 400;
            // 
            // _profileNote
            // 
            _profileNote.AutoSize = false;
            _profileNote.BackColor = Color.Transparent;
            _profileNote.Font = new Font("Segoe UI", 8.25F);
            _profileNote.ForeColor = Color.FromArgb(148, 163, 184);
            _profileNote.Location = new Point(470, 40);
            _profileNote.Name = "_profileNote";
            _profileNote.Size = new Size(400, 60);
            _profileNote.TabIndex = 12;
            _profileNote.Text = "THIS ACCOUNT HAS NO STUDENT, FACULTY, STAFF OR LIBRARIAN\r\nPROFILE ROW, SO ONLY THE ACCOUNT DETAILS CAN BE EDITED.";
            _profileNote.Visible = false;
            // 
            // ContentPanel
            // 
            ContentPanel.Controls.Add(_accountCaption);
            ContentPanel.Controls.Add(_profileCaption);
            ContentPanel.Controls.Add(_nameField);
            ContentPanel.Controls.Add(_usernameField);
            ContentPanel.Controls.Add(_roleField);
            ContentPanel.Controls.Add(_statusField);
            ContentPanel.Controls.Add(_schoolIdField);
            ContentPanel.Controls.Add(_programField);
            ContentPanel.Controls.Add(_yearLevelField);
            ContentPanel.Controls.Add(_instituteField);
            ContentPanel.Controls.Add(_contactField);
            ContentPanel.Controls.Add(_officeField);
            ContentPanel.Controls.Add(_profileNote);
            ContentPanel.ResumeLayout(false);
            // 
            // _primaryButton
            // 
            PrimaryButton.Text = "UPDATE ACCOUNT";
            // 
            // EditAccountDialogBase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "EditAccountDialogBase";
            ResumeLayout(false);
        }

        #endregion

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
