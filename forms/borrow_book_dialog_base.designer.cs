namespace oop_project.forms
{
    partial class BorrowBookDialogBase
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
            _loanCaption = new Label();
            _borrowerCaption = new Label();
            _dateField = DialogField.ForDate("DATE AND TIME OF BORROW");
            _bookField = DialogField.ForChoice("BOOK TITLE");
            _authorField = DialogField.ForReadOnlyText("AUTHOR");
            _loanInfoLabel = new Label();
            _borrowerField = DialogField.ForText("BORROWER NAME");
            _idNumberField = DialogField.ForText("ID NUMBER");
            _programField = DialogField.ForText("PROGRAM (OPTIONAL)");
            _yearLevelField = DialogField.ForChoice("YEAR LEVEL (OPTIONAL)");
            _instituteField = DialogField.ForText("INSTITUTE (OPTIONAL)");
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _loanCaption
            // 
            _loanCaption.AutoSize = false;
            _loanCaption.BackColor = Color.Transparent;
            _loanCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _loanCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _loanCaption.Location = new Point(34, 4);
            _loanCaption.Name = "_loanCaption";
            _loanCaption.Size = new Size(410, 18);
            _loanCaption.TabIndex = 0;
            _loanCaption.Text = "LOAN DETAILS (borrow_transactions_table)";
            // 
            // _borrowerCaption
            // 
            _borrowerCaption.AutoSize = false;
            _borrowerCaption.BackColor = Color.Transparent;
            _borrowerCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _borrowerCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _borrowerCaption.Location = new Point(470, 4);
            _borrowerCaption.Name = "_borrowerCaption";
            _borrowerCaption.Size = new Size(410, 18);
            _borrowerCaption.TabIndex = 1;
            _borrowerCaption.Text = "BORROWER DETAILS (borrowers_table)";
            // 
            // _dateField
            // 
            _dateField.DateBox.CustomFormat = "MMMM d, yyyy   h:mm tt";
            _dateField.DateBox.ShowUpDown = true;
            _dateField.Location = new Point(32, 30);
            _dateField.Name = "_dateField";
            _dateField.Size = new Size(400, 88);
            _dateField.TabIndex = 2;
            _dateField.Width = 400;
            // 
            // _bookField
            // 
            _bookField.ChoiceBox.DisplayMember = "DisplayText";
            _bookField.Location = new Point(32, 120);
            _bookField.Name = "_bookField";
            _bookField.Size = new Size(400, 88);
            _bookField.TabIndex = 3;
            _bookField.Width = 400;
            // 
            // _authorField
            // 
            _authorField.Location = new Point(32, 210);
            _authorField.Name = "_authorField";
            _authorField.Size = new Size(400, 88);
            _authorField.TabIndex = 4;
            _authorField.Width = 400;
            // 
            // _loanInfoLabel
            // 
            _loanInfoLabel.AutoSize = false;
            _loanInfoLabel.BackColor = Color.Transparent;
            _loanInfoLabel.Font = new Font("Segoe UI", 8.25F);
            _loanInfoLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _loanInfoLabel.Location = new Point(34, 306);
            _loanInfoLabel.Name = "_loanInfoLabel";
            _loanInfoLabel.Size = new Size(400, 36);
            _loanInfoLabel.TabIndex = 5;
            _loanInfoLabel.Text = "THE LOAN HAS NO DUE DATE.\r\nTHE DAYS BORROWED ARE COUNTED FROM THIS DATE.";
            // 
            // _borrowerField
            // 
            _borrowerField.Location = new Point(468, 30);
            _borrowerField.Name = "_borrowerField";
            _borrowerField.Size = new Size(400, 88);
            _borrowerField.TabIndex = 6;
            _borrowerField.Width = 400;
            // 
            // _idNumberField
            // 
            _idNumberField.Input.MaxLength = 50;
            _idNumberField.Location = new Point(468, 120);
            _idNumberField.Name = "_idNumberField";
            _idNumberField.Size = new Size(400, 88);
            _idNumberField.TabIndex = 7;
            _idNumberField.Width = 400;
            // 
            // _programField
            // 
            _programField.Input.MaxLength = 100;
            _programField.Location = new Point(468, 210);
            _programField.Name = "_programField";
            _programField.Size = new Size(400, 88);
            _programField.TabIndex = 8;
            _programField.Width = 400;
            // 
            // _yearLevelField
            // 
            _yearLevelField.ChoiceBox.Items.AddRange(YearLevels);
            _yearLevelField.Location = new Point(468, 300);
            _yearLevelField.Name = "_yearLevelField";
            _yearLevelField.Size = new Size(400, 88);
            _yearLevelField.TabIndex = 9;
            _yearLevelField.Width = 400;
            // 
            // _instituteField
            // 
            _instituteField.Input.MaxLength = 100;
            _instituteField.Location = new Point(468, 390);
            _instituteField.Name = "_instituteField";
            _instituteField.Size = new Size(400, 88);
            _instituteField.TabIndex = 10;
            _instituteField.Width = 400;
            // 
            // ContentPanel
            // 
            ContentPanel.Controls.Add(_loanCaption);
            ContentPanel.Controls.Add(_borrowerCaption);
            ContentPanel.Controls.Add(_loanInfoLabel);
            ContentPanel.Controls.Add(_dateField);
            ContentPanel.Controls.Add(_bookField);
            ContentPanel.Controls.Add(_authorField);
            ContentPanel.Controls.Add(_borrowerField);
            ContentPanel.Controls.Add(_idNumberField);
            ContentPanel.Controls.Add(_programField);
            ContentPanel.Controls.Add(_yearLevelField);
            ContentPanel.Controls.Add(_instituteField);
            ContentPanel.ResumeLayout(false);
            // 
            // _primaryButton
            // 
            PrimaryButton.Text = "BORROW BOOK";
            // 
            // BorrowBookDialogBase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "BorrowBookDialogBase";
            ResumeLayout(false);
        }

        #endregion

        private Label _loanCaption;
        private Label _borrowerCaption;
        private DialogField _dateField;
        private DialogField _bookField;
        private DialogField _authorField;
        private Label _loanInfoLabel;
        private DialogField _borrowerField;
        private DialogField _idNumberField;
        private DialogField _programField;
        private DialogField _yearLevelField;
        private DialogField _instituteField;
    }
}
