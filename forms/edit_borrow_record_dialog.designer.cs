namespace oop_project.forms
{
    partial class EditBorrowRecordDialog
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
            _statusField = new DialogField();
            _borrowerField = new DialogField();
            _idNumberField = new DialogField();
            _programField = new DialogField();
            _yearLevelField = new DialogField();
            _instituteField = new DialogField();
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
            _contentPanel.Controls.Add(_statusField);
            _contentPanel.Controls.Add(_borrowerField);
            _contentPanel.Controls.Add(_idNumberField);
            _contentPanel.Controls.Add(_programField);
            _contentPanel.Controls.Add(_yearLevelField);
            _contentPanel.Controls.Add(_instituteField);
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(900, 518);
            _contentPanel.TabIndex = 3;
            // 
            // _loanCaption
            // 
            _loanCaption.BackColor = Color.Transparent;
            _loanCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _loanCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _loanCaption.Location = new Point(34, 10);
            _loanCaption.Name = "_loanCaption";
            _loanCaption.Size = new Size(410, 18);
            _loanCaption.TabIndex = 0;
            _loanCaption.Text = "LOAN DETAILS";
            // 
            // _borrowerCaption
            // 
            _borrowerCaption.BackColor = Color.Transparent;
            _borrowerCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _borrowerCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _borrowerCaption.Location = new Point(470, 10);
            _borrowerCaption.Name = "_borrowerCaption";
            _borrowerCaption.Size = new Size(410, 18);
            _borrowerCaption.TabIndex = 1;
            _borrowerCaption.Text = "BORROWER DETAILS";
            // 
            // _loanInfoLabel
            // 
            _loanInfoLabel.BackColor = Color.Transparent;
            _loanInfoLabel.Font = new Font("Segoe UI", 8.25F);
            _loanInfoLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _loanInfoLabel.Location = new Point(34, 306);
            _loanInfoLabel.Name = "_loanInfoLabel";
            _loanInfoLabel.Size = new Size(400, 40);
            _loanInfoLabel.TabIndex = 5;
            // 
            // _dateField
            // 
            _dateField.BackColor = Color.White;
            _dateField.Caption = "DATE AND TIME OF BORROW";
            _dateField.Kind = DialogFieldKind.Date;
            _dateField.Location = new Point(32, 44);
            _dateField.Name = "_dateField";
            _dateField.Size = new Size(400, 88);
            _dateField.TabIndex = 2;
            _dateField.Value = "November 02, 2026";
            // 
            // _bookField
            // 
            _bookField.BackColor = Color.White;
            _bookField.Caption = "BOOK TITLE";
            _bookField.Kind = DialogFieldKind.Choice;
            _bookField.Location = new Point(32, 134);
            _bookField.Name = "_bookField";
            _bookField.Size = new Size(400, 88);
            _bookField.TabIndex = 3;
            _bookField.Value = "";
            // 
            // _statusField
            // 
            _statusField.BackColor = Color.White;
            _statusField.Caption = "LOAN STATUS";
            _statusField.Kind = DialogFieldKind.Choice;
            _statusField.Location = new Point(32, 224);
            _statusField.Name = "_statusField";
            _statusField.Size = new Size(400, 88);
            _statusField.TabIndex = 4;
            _statusField.Value = "";
            // 
            // _borrowerField
            // 
            _borrowerField.BackColor = Color.White;
            _borrowerField.Caption = "BORROWER NAME";
            _borrowerField.Kind = DialogFieldKind.Text;
            _borrowerField.Location = new Point(468, 44);
            _borrowerField.Name = "_borrowerField";
            _borrowerField.Size = new Size(400, 88);
            _borrowerField.TabIndex = 6;
            _borrowerField.Value = "";
            // 
            // _idNumberField
            // 
            _idNumberField.BackColor = Color.White;
            _idNumberField.Caption = "ID NUMBER";
            _idNumberField.Kind = DialogFieldKind.Text;
            _idNumberField.Location = new Point(468, 134);
            _idNumberField.Name = "_idNumberField";
            _idNumberField.Size = new Size(400, 88);
            _idNumberField.TabIndex = 7;
            _idNumberField.Value = "";
            // 
            // _programField
            // 
            _programField.BackColor = Color.White;
            _programField.Caption = "PROGRAM (OPTIONAL)";
            _programField.Kind = DialogFieldKind.Text;
            _programField.Location = new Point(468, 224);
            _programField.Name = "_programField";
            _programField.Size = new Size(400, 88);
            _programField.TabIndex = 8;
            _programField.Value = "";
            // 
            // _yearLevelField
            // 
            _yearLevelField.BackColor = Color.White;
            _yearLevelField.Caption = "YEAR LEVEL (OPTIONAL)";
            _yearLevelField.Kind = DialogFieldKind.Choice;
            _yearLevelField.Location = new Point(468, 314);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(400, 88);
            _yearLevelField.TabIndex = 9;
            _yearLevelField.Value = "";
            // 
            // _instituteField
            // 
            _instituteField.BackColor = Color.White;
            _instituteField.Caption = "INSTITUTE (OPTIONAL)";
            _instituteField.Kind = DialogFieldKind.Text;
            _instituteField.Location = new Point(468, 404);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(400, 88);
            _instituteField.TabIndex = 10;
            _instituteField.Value = "";
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
            _titleLabel.Text = "EDIT BORROW RECORD";
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
            _subtitleLabel.Text = "Change the loan or the borrower details, then press UPDATE RECORD.";
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
            _primaryButton.Text = "UPDATE RECORD";
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
            // EditBorrowRecordDialog
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
            Name = "EditBorrowRecordDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "EDIT BORROW RECORD";
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
        private DialogField _statusField;
        private Label _loanInfoLabel;
        private DialogField _borrowerField;
        private DialogField _idNumberField;
        private DialogField _programField;
        private DialogField _yearLevelField;
        private DialogField _instituteField;
    }
}
