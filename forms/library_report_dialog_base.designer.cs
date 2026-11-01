namespace oop_project.forms
{
    partial class LibraryReportDialogBase
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
            _titleCard = new RoundedCard();
            _titleValue = new Label();
            _titleCaption = new Label();
            _availableCard = new RoundedCard();
            _availableValue = new Label();
            _availableCaption = new Label();
            _borrowedCard = new RoundedCard();
            _borrowedValue = new Label();
            _borrowedCaption = new Label();
            _returnedCard = new RoundedCard();
            _returnedValue = new Label();
            _returnedCaption = new Label();
            _previewCaption = new Label();
            _previewCard = new RoundedCard();
            _previewTextBox = new TextBox();
            _titleCard.SuspendLayout();
            _availableCard.SuspendLayout();
            _borrowedCard.SuspendLayout();
            _returnedCard.SuspendLayout();
            _previewCard.SuspendLayout();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _titleCard
            // 
            _titleCard.BackColor = Color.FromArgb(248, 250, 252);
            _titleCard.BorderColor = Color.FromArgb(226, 232, 240);
            _titleCard.Controls.Add(_titleCaption);
            _titleCard.Controls.Add(_titleValue);
            _titleCard.CornerRadius = 14;
            _titleCard.Location = new Point(32, 14);
            _titleCard.Name = "_titleCard";
            _titleCard.Size = new Size(201, 74);
            _titleCard.TabIndex = 0;
            // 
            // _titleValue
            // 
            _titleValue.AutoSize = false;
            _titleValue.BackColor = Color.Transparent;
            _titleValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _titleValue.ForeColor = Color.FromArgb(6, 78, 59);
            _titleValue.Location = new Point(18, 8);
            _titleValue.Name = "_titleValue";
            _titleValue.Size = new Size(165, 34);
            _titleValue.TabIndex = 0;
            _titleValue.Text = "0";
            _titleValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _titleCaption
            // 
            _titleCaption.AutoSize = false;
            _titleCaption.BackColor = Color.Transparent;
            _titleCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _titleCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _titleCaption.Location = new Point(20, 44);
            _titleCaption.Name = "_titleCaption";
            _titleCaption.Size = new Size(161, 18);
            _titleCaption.TabIndex = 1;
            _titleCaption.Text = "TOTAL TITLES";
            _titleCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _availableCard
            // 
            _availableCard.BackColor = Color.FromArgb(248, 250, 252);
            _availableCard.BorderColor = Color.FromArgb(226, 232, 240);
            _availableCard.Controls.Add(_availableCaption);
            _availableCard.Controls.Add(_availableValue);
            _availableCard.CornerRadius = 14;
            _availableCard.Location = new Point(243, 14);
            _availableCard.Name = "_availableCard";
            _availableCard.Size = new Size(201, 74);
            _availableCard.TabIndex = 1;
            // 
            // _availableValue
            // 
            _availableValue.AutoSize = false;
            _availableValue.BackColor = Color.Transparent;
            _availableValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _availableValue.ForeColor = Color.FromArgb(22, 163, 74);
            _availableValue.Location = new Point(18, 8);
            _availableValue.Name = "_availableValue";
            _availableValue.Size = new Size(165, 34);
            _availableValue.TabIndex = 0;
            _availableValue.Text = "0";
            _availableValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _availableCaption
            // 
            _availableCaption.AutoSize = false;
            _availableCaption.BackColor = Color.Transparent;
            _availableCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _availableCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _availableCaption.Location = new Point(20, 44);
            _availableCaption.Name = "_availableCaption";
            _availableCaption.Size = new Size(161, 18);
            _availableCaption.TabIndex = 1;
            _availableCaption.Text = "AVAILABLE";
            _availableCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _borrowedCard
            // 
            _borrowedCard.BackColor = Color.FromArgb(248, 250, 252);
            _borrowedCard.BorderColor = Color.FromArgb(226, 232, 240);
            _borrowedCard.Controls.Add(_borrowedCaption);
            _borrowedCard.Controls.Add(_borrowedValue);
            _borrowedCard.CornerRadius = 14;
            _borrowedCard.Location = new Point(454, 14);
            _borrowedCard.Name = "_borrowedCard";
            _borrowedCard.Size = new Size(201, 74);
            _borrowedCard.TabIndex = 2;
            // 
            // _borrowedValue
            // 
            _borrowedValue.AutoSize = false;
            _borrowedValue.BackColor = Color.Transparent;
            _borrowedValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _borrowedValue.ForeColor = Color.FromArgb(194, 65, 12);
            _borrowedValue.Location = new Point(18, 8);
            _borrowedValue.Name = "_borrowedValue";
            _borrowedValue.Size = new Size(165, 34);
            _borrowedValue.TabIndex = 0;
            _borrowedValue.Text = "0";
            _borrowedValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _borrowedCaption
            // 
            _borrowedCaption.AutoSize = false;
            _borrowedCaption.BackColor = Color.Transparent;
            _borrowedCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _borrowedCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _borrowedCaption.Location = new Point(20, 44);
            _borrowedCaption.Name = "_borrowedCaption";
            _borrowedCaption.Size = new Size(161, 18);
            _borrowedCaption.TabIndex = 1;
            _borrowedCaption.Text = "BORROWED";
            _borrowedCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _returnedCard
            // 
            _returnedCard.BackColor = Color.FromArgb(248, 250, 252);
            _returnedCard.BorderColor = Color.FromArgb(226, 232, 240);
            _returnedCard.Controls.Add(_returnedCaption);
            _returnedCard.Controls.Add(_returnedValue);
            _returnedCard.CornerRadius = 14;
            _returnedCard.Location = new Point(665, 14);
            _returnedCard.Name = "_returnedCard";
            _returnedCard.Size = new Size(201, 74);
            _returnedCard.TabIndex = 3;
            // 
            // _returnedValue
            // 
            _returnedValue.AutoSize = false;
            _returnedValue.BackColor = Color.Transparent;
            _returnedValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _returnedValue.ForeColor = Color.FromArgb(100, 116, 139);
            _returnedValue.Location = new Point(18, 8);
            _returnedValue.Name = "_returnedValue";
            _returnedValue.Size = new Size(165, 34);
            _returnedValue.TabIndex = 0;
            _returnedValue.Text = "0";
            _returnedValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _returnedCaption
            // 
            _returnedCaption.AutoSize = false;
            _returnedCaption.BackColor = Color.Transparent;
            _returnedCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _returnedCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _returnedCaption.Location = new Point(20, 44);
            _returnedCaption.Name = "_returnedCaption";
            _returnedCaption.Size = new Size(161, 18);
            _returnedCaption.TabIndex = 1;
            _returnedCaption.Text = "RETURNED";
            _returnedCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _previewCaption
            // 
            _previewCaption.AutoSize = false;
            _previewCaption.BackColor = Color.Transparent;
            _previewCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            _previewCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _previewCaption.Location = new Point(34, 96);
            _previewCaption.Name = "_previewCaption";
            _previewCaption.Size = new Size(836, 18);
            _previewCaption.TabIndex = 4;
            _previewCaption.Text = "REPORT PREVIEW";
            // 
            // _previewCard
            // 
            _previewCard.BackColor = Color.FromArgb(15, 23, 42);
            _previewCard.BorderColor = Color.FromArgb(30, 41, 59);
            _previewCard.Controls.Add(_previewTextBox);
            _previewCard.CornerRadius = 14;
            _previewCard.Location = new Point(32, 118);
            _previewCard.Name = "_previewCard";
            _previewCard.Size = new Size(836, 340);
            _previewCard.TabIndex = 5;
            // 
            // _previewTextBox
            // 
            _previewTextBox.BackColor = Color.FromArgb(15, 23, 42);
            _previewTextBox.BorderStyle = BorderStyle.None;
            _previewTextBox.Font = new Font("Consolas", 8.75F);
            _previewTextBox.ForeColor = Color.FromArgb(209, 250, 229);
            _previewTextBox.Location = new Point(16, 14);
            _previewTextBox.Multiline = true;
            _previewTextBox.Name = "_previewTextBox";
            _previewTextBox.ReadOnly = true;
            _previewTextBox.ScrollBars = ScrollBars.Vertical;
            _previewTextBox.Size = new Size(804, 312);
            _previewTextBox.TabIndex = 0;
            _previewTextBox.TabStop = false;
            _previewTextBox.Text = "REPORT PREVIEW";
            _previewTextBox.WordWrap = true;
            // 
            // ContentPanel
            // 
            ContentPanel.Controls.Add(_titleCard);
            ContentPanel.Controls.Add(_availableCard);
            ContentPanel.Controls.Add(_borrowedCard);
            ContentPanel.Controls.Add(_returnedCard);
            ContentPanel.Controls.Add(_previewCaption);
            ContentPanel.Controls.Add(_previewCard);
            ContentPanel.ResumeLayout(false);
            // 
            // _primaryButton
            // 
            PrimaryButton.Text = "SAVE REPORT";
            // 
            // _secondaryButton
            // 
            SecondaryButton.Text = "CLOSE";
            // 
            // LibraryReportDialogBase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "LibraryReportDialogBase";
            _titleCard.ResumeLayout(false);
            _availableCard.ResumeLayout(false);
            _borrowedCard.ResumeLayout(false);
            _returnedCard.ResumeLayout(false);
            _previewCard.ResumeLayout(false);
            _previewCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RoundedCard _titleCard;
        private Label _titleValue;
        private Label _titleCaption;
        private RoundedCard _availableCard;
        private Label _availableValue;
        private Label _availableCaption;
        private RoundedCard _borrowedCard;
        private Label _borrowedValue;
        private Label _borrowedCaption;
        private RoundedCard _returnedCard;
        private Label _returnedValue;
        private Label _returnedCaption;
        private Label _previewCaption;
        private RoundedCard _previewCard;
        private TextBox _previewTextBox;
    }
}
