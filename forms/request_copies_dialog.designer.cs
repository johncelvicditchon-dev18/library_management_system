namespace oop_project.forms
{
    partial class RequestCopiesDialog
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
            accentBar = new AccentBar();
            badge = new PillBadge();
            headingLabel = new Label();
            bookTitleLabel = new Label();
            divider = new Panel();
            questionLabel = new Label();
            copiesField = new FieldPanel();
            copiesInput = new TextBox();
            copiesError = new Label();
            availableLabel = new Label();
            maxLabel = new Label();
            hintLabel = new Label();
            cancelButton = new RoundedButton();
            sendButton = new RoundedButton();
            copiesField.SuspendLayout();
            SuspendLayout();
            // 
            // accentBar
            // 
            accentBar.BackColor = Color.Transparent;
            accentBar.EndColor = Color.FromArgb(5, 150, 105);
            accentBar.Location = new Point(0, 0);
            accentBar.Margin = new Padding(3, 4, 3, 4);
            accentBar.Name = "accentBar";
            accentBar.Size = new Size(594, 8);
            accentBar.StartColor = Color.FromArgb(74, 222, 128);
            accentBar.TabIndex = 0;
            accentBar.TabStop = false;
            // 
            // badge
            // 
            badge.BackColor = Color.Transparent;
            badge.BorderColor = Color.FromArgb(187, 247, 208);
            badge.FillColor = Color.FromArgb(240, 253, 244);
            badge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            badge.Location = new Point(39, 35);
            badge.Margin = new Padding(3, 4, 3, 4);
            badge.Name = "badge";
            badge.Size = new Size(197, 44);
            badge.TabIndex = 1;
            badge.TabStop = false;
            badge.Text = "BORROW REQUEST";
            badge.TextColor = Color.FromArgb(21, 128, 61);
            // 
            // headingLabel
            // 
            headingLabel.BackColor = Color.Transparent;
            headingLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            headingLabel.ForeColor = Color.FromArgb(15, 23, 42);
            headingLabel.Location = new Point(37, 83);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(519, 53);
            headingLabel.TabIndex = 2;
            headingLabel.Text = "HOW MANY COPIES?";
            headingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // bookTitleLabel
            // 
            bookTitleLabel.AutoEllipsis = true;
            bookTitleLabel.BackColor = Color.Transparent;
            bookTitleLabel.Font = new Font("Segoe UI", 9.5F);
            bookTitleLabel.ForeColor = Color.FromArgb(71, 85, 105);
            bookTitleLabel.Location = new Point(39, 141);
            bookTitleLabel.Name = "bookTitleLabel";
            bookTitleLabel.Size = new Size(517, 29);
            bookTitleLabel.TabIndex = 3;
            bookTitleLabel.Text = "\"BOOK TITLE\"";
            // 
            // divider
            // 
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(39, 189);
            divider.Margin = new Padding(3, 4, 3, 4);
            divider.Name = "divider";
            divider.Size = new Size(517, 1);
            divider.TabIndex = 4;
            // 
            // questionLabel
            // 
            questionLabel.BackColor = Color.Transparent;
            questionLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            questionLabel.ForeColor = Color.FromArgb(100, 116, 139);
            questionLabel.Location = new Point(39, 208);
            questionLabel.Name = "questionLabel";
            questionLabel.Size = new Size(517, 27);
            questionLabel.TabIndex = 5;
            questionLabel.Text = "NUMBER OF COPIES";
            questionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // copiesField
            // 
            copiesField.BackColor = Color.White;
            copiesField.BorderColor = Color.FromArgb(226, 232, 240);
            copiesField.Controls.Add(copiesInput);
            copiesField.CornerRadius = 14;
            copiesField.FillColor = Color.FromArgb(248, 250, 252);
            copiesField.FocusColor = Color.FromArgb(22, 163, 74);
            copiesField.HasError = false;
            copiesField.IsFocused = false;
            copiesField.Location = new Point(39, 243);
            copiesField.Margin = new Padding(3, 4, 3, 4);
            copiesField.Name = "copiesField";
            copiesField.Size = new Size(517, 107);
            copiesField.TabIndex = 6;
            // 
            // copiesInput
            // 
            copiesInput.BackColor = Color.FromArgb(248, 250, 252);
            copiesInput.BorderStyle = BorderStyle.None;
            copiesInput.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            copiesInput.ForeColor = Color.FromArgb(6, 78, 59);
            copiesInput.Location = new Point(144, 27);
            copiesInput.Margin = new Padding(3, 4, 3, 4);
            copiesInput.MaxLength = 4;
            copiesInput.Name = "copiesInput";
            copiesInput.Size = new Size(229, 49);
            copiesInput.TabIndex = 0;
            copiesInput.Text = "1";
            copiesInput.TextAlign = HorizontalAlignment.Center;
            // 
            // copiesError
            // 
            copiesError.BackColor = Color.Transparent;
            copiesError.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            copiesError.ForeColor = Color.FromArgb(220, 38, 38);
            copiesError.Location = new Point(41, 355);
            copiesError.Name = "copiesError";
            copiesError.Size = new Size(512, 21);
            copiesError.TabIndex = 7;
            copiesError.TextAlign = ContentAlignment.MiddleLeft;
            copiesError.Visible = false;
            // 
            // availableLabel
            // 
            availableLabel.BackColor = Color.Transparent;
            availableLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            availableLabel.ForeColor = Color.FromArgb(4, 120, 87);
            availableLabel.Location = new Point(39, 384);
            availableLabel.Name = "availableLabel";
            availableLabel.Size = new Size(411, 27);
            availableLabel.TabIndex = 8;
            availableLabel.Text = "1 COPY IS AVAILABLE ON THE SHELF.";
            availableLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // maxLabel
            // 
            maxLabel.BackColor = Color.Transparent;
            maxLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            maxLabel.ForeColor = Color.FromArgb(148, 163, 184);
            maxLabel.Location = new Point(457, 384);
            maxLabel.Name = "maxLabel";
            maxLabel.Size = new Size(98, 27);
            maxLabel.TabIndex = 9;
            maxLabel.Text = "MAX 1";
            maxLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // hintLabel
            // 
            hintLabel.BackColor = Color.Transparent;
            hintLabel.Font = new Font("Segoe UI", 8.25F);
            hintLabel.ForeColor = Color.FromArgb(148, 163, 184);
            hintLabel.Location = new Point(39, 416);
            hintLabel.Name = "hintLabel";
            hintLabel.Size = new Size(517, 27);
            hintLabel.TabIndex = 10;
            hintLabel.Text = "THE LIBRARIAN WILL APPROVE OR REJECT THE REQUEST.";
            hintLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cancelButton
            // 
            cancelButton.BackColor = Color.Transparent;
            cancelButton.CornerRadius = 11;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cancelButton.ForeColor = Color.FromArgb(71, 85, 105);
            cancelButton.HoverColor = Color.FromArgb(226, 232, 240);
            cancelButton.Location = new Point(39, 477);
            cancelButton.Margin = new Padding(3, 4, 3, 4);
            cancelButton.Name = "cancelButton";
            cancelButton.NormalColor = Color.FromArgb(241, 245, 249);
            cancelButton.PressedColor = Color.FromArgb(203, 213, 225);
            cancelButton.Size = new Size(171, 59);
            cancelButton.TabIndex = 11;
            cancelButton.Text = "CANCEL";
            cancelButton.UseVisualStyleBackColor = false;
            // 
            // sendButton
            // 
            sendButton.BackColor = Color.Transparent;
            sendButton.CornerRadius = 11;
            sendButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            sendButton.FlatAppearance.BorderSize = 0;
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            sendButton.ForeColor = Color.White;
            sendButton.HoverColor = Color.FromArgb(21, 128, 61);
            sendButton.Location = new Point(237, 477);
            sendButton.Margin = new Padding(3, 4, 3, 4);
            sendButton.Name = "sendButton";
            sendButton.NormalColor = Color.FromArgb(22, 163, 74);
            sendButton.PressedColor = Color.FromArgb(20, 83, 45);
            sendButton.Size = new Size(318, 59);
            sendButton.TabIndex = 12;
            sendButton.Text = "SEND REQUEST";
            sendButton.UseVisualStyleBackColor = false;
            // 
            // RequestCopiesDialog
            // 
            AcceptButton = sendButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = cancelButton;
            ClientSize = new Size(594, 571);
            Controls.Add(accentBar);
            Controls.Add(badge);
            Controls.Add(headingLabel);
            Controls.Add(bookTitleLabel);
            Controls.Add(divider);
            Controls.Add(questionLabel);
            Controls.Add(copiesField);
            Controls.Add(copiesError);
            Controls.Add(availableLabel);
            Controls.Add(maxLabel);
            Controls.Add(hintLabel);
            Controls.Add(cancelButton);
            Controls.Add(sendButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RequestCopiesDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "REQUEST TO BORROW";
            copiesField.ResumeLayout(false);
            copiesField.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private oop_project.AccentBar accentBar;
        private oop_project.PillBadge badge;
        private System.Windows.Forms.Label headingLabel;
        private System.Windows.Forms.Label bookTitleLabel;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Label questionLabel;
        private oop_project.FieldPanel copiesField;
        private System.Windows.Forms.TextBox copiesInput;
        private System.Windows.Forms.Label copiesError;
        private System.Windows.Forms.Label availableLabel;
        private System.Windows.Forms.Label maxLabel;
        private System.Windows.Forms.Label hintLabel;
        private oop_project.RoundedButton cancelButton;
        private oop_project.RoundedButton sendButton;
    }
}
