namespace oop_project.forms
{
    partial class BorrowBookDialog
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
            _loanCaption = new Label();
            _borrowerCaption = new Label();
            _loanInfoLabel = new Label();
            _dateField = new DialogField();
            _bookField = new DialogField();
            _authorField = new DialogField();
            _copiesField = new DialogField();
            _roleField = new DialogField();
            _contactNumberField = new DialogField();
            _lastNameField = new DialogField();
            _firstNameField = new DialogField();
            _middleNameField = new DialogField();
            _idNumberField = new DialogField();
            _officeField = new DialogField();
            _instituteField = new DialogField();
            _programField = new DialogField();
            _yearLevelField = new DialogField();
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
            _contentPanel.Controls.Add(_loanCaption);
            _contentPanel.Controls.Add(_borrowerCaption);
            _contentPanel.Controls.Add(_loanInfoLabel);
            _contentPanel.Controls.Add(_dateField);
            _contentPanel.Controls.Add(_bookField);
            _contentPanel.Controls.Add(_authorField);
            _contentPanel.Controls.Add(_copiesField);
            _contentPanel.Controls.Add(_roleField);
            _contentPanel.Controls.Add(_contactNumberField);
            _contentPanel.Controls.Add(_lastNameField);
            _contentPanel.Controls.Add(_firstNameField);
            _contentPanel.Controls.Add(_middleNameField);
            _contentPanel.Controls.Add(_idNumberField);
            _contentPanel.Controls.Add(_officeField);
            _contentPanel.Controls.Add(_instituteField);
            _contentPanel.Controls.Add(_programField);
            _contentPanel.Controls.Add(_yearLevelField);
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(1240, 518);
            _contentPanel.TabIndex = 3;
            // 
            // _loanCaption
            // 
            _loanCaption.BackColor = Color.Transparent;
            _loanCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _loanCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _loanCaption.Location = new Point(34, 11);
            _loanCaption.Name = "_loanCaption";
            _loanCaption.Size = new Size(570, 18);
            _loanCaption.TabIndex = 0;
            _loanCaption.Text = "LOAN DETAILS ";
            // 
            // _borrowerCaption
            // 
            _borrowerCaption.BackColor = Color.Transparent;
            _borrowerCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _borrowerCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _borrowerCaption.Location = new Point(638, 11);
            _borrowerCaption.Name = "_borrowerCaption";
            _borrowerCaption.Size = new Size(570, 18);
            _borrowerCaption.TabIndex = 1;
            _borrowerCaption.Text = "BORROWER DETAILS";
            // 
            // _loanInfoLabel
            // 
            _loanInfoLabel.BackColor = Color.Transparent;
            _loanInfoLabel.Font = new Font("Segoe UI", 8.25F);
            _loanInfoLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _loanInfoLabel.Location = new Point(34, 412);
            _loanInfoLabel.Name = "_loanInfoLabel";
            _loanInfoLabel.Size = new Size(570, 36);
            _loanInfoLabel.TabIndex = 5;
            _loanInfoLabel.Text = "THE LOAN HAS NO DUE DATE.\r\nTHE DAYS BORROWED ARE COUNTED FROM THIS DATE.";
            // 
            // _dateField
            // 
            _dateField.BackColor = Color.White;
            _dateField.Caption = "DATE AND TIME OF BORROW";
            _dateField.Kind = DialogFieldKind.Date;
            _dateField.Location = new Point(32, 44);
            _dateField.Name = "_dateField";
            _dateField.Size = new Size(570, 88);
            _dateField.TabIndex = 2;
            _dateField.Value = "November 01, 2026";
            // 
            // _bookField
            // 
            _bookField.BackColor = Color.White;
            _bookField.Caption = "BOOK TITLE";
            _bookField.Kind = DialogFieldKind.Choice;
            _bookField.Location = new Point(32, 134);
            _bookField.Name = "_bookField";
            _bookField.Size = new Size(570, 88);
            _bookField.TabIndex = 3;
            _bookField.Value = "";
            // 
            // _authorField
            // 
            _authorField.BackColor = Color.White;
            _authorField.Caption = "AUTHOR";
            _authorField.Kind = DialogFieldKind.ReadOnly;
            _authorField.Location = new Point(32, 224);
            _authorField.Name = "_authorField";
            _authorField.Size = new Size(570, 88);
            _authorField.TabIndex = 4;
            _authorField.Value = "";
            // 
            // _copiesField
            // 
            _copiesField.BackColor = Color.White;
            _copiesField.Caption = "COPIES TO BORROW";
            _copiesField.Kind = DialogFieldKind.Text;
            _copiesField.Location = new Point(32, 314);
            _copiesField.Name = "_copiesField";
            _copiesField.Size = new Size(570, 88);
            _copiesField.TabIndex = 5;
            _copiesField.Value = "1";
            // 
            // _roleField
            // 
            _roleField.BackColor = Color.White;
            _roleField.Caption = "ROLE";
            _roleField.Kind = DialogFieldKind.Choice;
            _roleField.Location = new Point(638, 44);
            _roleField.Name = "_roleField";
            _roleField.Size = new Size(275, 88);
            _roleField.TabIndex = 6;
            _roleField.Value = "";
            // 
            // _contactNumberField
            // 
            _contactNumberField.BackColor = Color.White;
            _contactNumberField.Caption = "CONTACT NUMBER";
            _contactNumberField.Kind = DialogFieldKind.Text;
            _contactNumberField.Location = new Point(933, 44);
            _contactNumberField.Name = "_contactNumberField";
            _contactNumberField.Size = new Size(275, 88);
            _contactNumberField.TabIndex = 7;
            _contactNumberField.Value = "";
            // 
            // _lastNameField
            // 
            _lastNameField.BackColor = Color.White;
            _lastNameField.Caption = "LAST NAME";
            _lastNameField.Kind = DialogFieldKind.Text;
            _lastNameField.Location = new Point(638, 134);
            _lastNameField.Name = "_lastNameField";
            _lastNameField.Size = new Size(275, 88);
            _lastNameField.TabIndex = 8;
            _lastNameField.Value = "";
            // 
            // _firstNameField
            // 
            _firstNameField.BackColor = Color.White;
            _firstNameField.Caption = "FIRST NAME";
            _firstNameField.Kind = DialogFieldKind.Text;
            _firstNameField.Location = new Point(933, 134);
            _firstNameField.Name = "_firstNameField";
            _firstNameField.Size = new Size(275, 88);
            _firstNameField.TabIndex = 9;
            _firstNameField.Value = "";
            // 
            // _middleNameField
            // 
            _middleNameField.BackColor = Color.White;
            _middleNameField.Caption = "MIDDLE NAME (OPTIONAL)";
            _middleNameField.Kind = DialogFieldKind.Text;
            _middleNameField.Location = new Point(638, 224);
            _middleNameField.Name = "_middleNameField";
            _middleNameField.Size = new Size(275, 88);
            _middleNameField.TabIndex = 10;
            _middleNameField.Value = "";
            // 
            // _idNumberField
            // 
            _idNumberField.BackColor = Color.White;
            _idNumberField.Caption = "ID NUMBER";
            _idNumberField.Kind = DialogFieldKind.Text;
            _idNumberField.Location = new Point(933, 224);
            _idNumberField.Name = "_idNumberField";
            _idNumberField.Size = new Size(275, 88);
            _idNumberField.TabIndex = 11;
            _idNumberField.Value = "";
            // 
            // _officeField
            // 
            _officeField.BackColor = Color.White;
            _officeField.Caption = "OFFICE";
            _officeField.Kind = DialogFieldKind.Text;
            _officeField.Location = new Point(933, 404);
            _officeField.Name = "_officeField";
            _officeField.Size = new Size(275, 88);
            _officeField.TabIndex = 16;
            _officeField.Value = "";
            // 
            // _instituteField
            // 
            _instituteField.BackColor = Color.White;
            _instituteField.Caption = "INSTITUTE";
            _instituteField.Kind = DialogFieldKind.Choice;
            _instituteField.Location = new Point(638, 314);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(275, 88);
            _instituteField.TabIndex = 13;
            _instituteField.Value = "";
            // 
            // _programField
            // 
            _programField.BackColor = Color.White;
            _programField.Caption = "PROGRAM";
            _programField.Kind = DialogFieldKind.Choice;
            _programField.Location = new Point(933, 314);
            _programField.Name = "_programField";
            _programField.Size = new Size(275, 88);
            _programField.TabIndex = 14;
            _programField.Value = "";
            // 
            // _yearLevelField
            // 
            _yearLevelField.BackColor = Color.White;
            _yearLevelField.Caption = "YEAR LEVEL";
            _yearLevelField.Kind = DialogFieldKind.Choice;
            _yearLevelField.Location = new Point(638, 404);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(275, 88);
            _yearLevelField.TabIndex = 15;
            _yearLevelField.Value = "";
            // 
            // _footerDivider
            // 
            _footerDivider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, 622);
            _footerDivider.Name = "_footerDivider";
            _footerDivider.Size = new Size(1240, 1);
            _footerDivider.TabIndex = 4;
            // 
            // _titleLabel
            // 
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            _titleLabel.Location = new Point(32, 22);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(1110, 32);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "BORROW A BOOK";
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
            _subtitleLabel.Size = new Size(1100, 22);
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = "Record the loan and the borrower details in one step.";
            _subtitleLabel.MouseDown += DragStart;
            _subtitleLabel.MouseMove += DragMove;
            _subtitleLabel.MouseUp += DragEnd;
            // 
            // _closeButton
            // 
            _closeButton.BackColor = Color.Transparent;
            _closeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _closeButton.Location = new Point(1174, 20);
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
            _primaryButton.Location = new Point(1040, 638);
            _primaryButton.Name = "_primaryButton";
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 46);
            _primaryButton.TabIndex = 5;
            _primaryButton.Text = "BORROW BOOK";
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
            _secondaryButton.Location = new Point(908, 638);
            _secondaryButton.Name = "_secondaryButton";
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(120, 46);
            _secondaryButton.TabIndex = 6;
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;
            // 
            // BorrowBookDialog
            // 
            AcceptButton = _primaryButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = _secondaryButton;
            ClientSize = new Size(1240, 700);
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
            Name = "BorrowBookDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "BORROW A BOOK";
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
        private Label _loanCaption;
        private Label _borrowerCaption;
        private DialogField _dateField;
        private DialogField _bookField;
        private DialogField _authorField;
        private Label _loanInfoLabel;
        private DialogField _copiesField;
        private DialogField _roleField;
        private DialogField _contactNumberField;
        private DialogField _lastNameField;
        private DialogField _firstNameField;
        private DialogField _middleNameField;
        private DialogField _idNumberField;
        private DialogField _officeField;
        private DialogField _instituteField;
        private DialogField _programField;
        private DialogField _yearLevelField;
    }
}
