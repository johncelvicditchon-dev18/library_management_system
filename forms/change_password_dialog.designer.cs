namespace oop_project.forms
{
    partial class ChangePasswordDialog
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
            divider = new Panel();
            currentCaption = new Label();
            currentField = new FieldPanel();
            currentInput = new TextBox();
            currentToggle = new PasswordToggle();
            currentError = new Label();
            newCaption = new Label();
            newField = new FieldPanel();
            newInput = new TextBox();
            newToggle = new PasswordToggle();
            newError = new Label();
            confirmCaption = new Label();
            confirmField = new FieldPanel();
            confirmInput = new TextBox();
            confirmToggle = new PasswordToggle();
            confirmError = new Label();
            cancelButton = new RoundedButton();
            saveButton = new RoundedButton();
            hintLabel = new Label();
            currentField.SuspendLayout();
            newField.SuspendLayout();
            confirmField.SuspendLayout();
            SuspendLayout();
            // 
            // accentBar
            // 
            accentBar.BackColor = Color.Transparent;
            accentBar.EndColor = Color.FromArgb(5, 150, 105);
            accentBar.Location = new Point(0, 0);
            accentBar.Margin = new Padding(3, 4, 3, 4);
            accentBar.Name = "accentBar";
            accentBar.Size = new Size(617, 8);
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
            badge.Location = new Point(41, 37);
            badge.Margin = new Padding(3, 4, 3, 4);
            badge.Name = "badge";
            badge.Size = new Size(181, 33);
            badge.TabIndex = 1;
            badge.TabStop = false;
            badge.Text = "ACCOUNT SECURITY";
            badge.TextColor = Color.FromArgb(21, 128, 61);
            // 
            // headingLabel
            // 
            headingLabel.BackColor = Color.Transparent;
            headingLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            headingLabel.ForeColor = Color.FromArgb(15, 23, 42);
            headingLabel.Location = new Point(39, 83);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(539, 53);
            headingLabel.TabIndex = 2;
            headingLabel.Text = "CHANGE PASSWORD";
            headingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // divider
            // 
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(41, 187);
            divider.Margin = new Padding(3, 4, 3, 4);
            divider.Name = "divider";
            divider.Size = new Size(535, 1);
            divider.TabIndex = 4;
            // 
            // currentCaption
            // 
            currentCaption.BackColor = Color.Transparent;
            currentCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            currentCaption.ForeColor = Color.FromArgb(100, 116, 139);
            currentCaption.Location = new Point(41, 208);
            currentCaption.Name = "currentCaption";
            currentCaption.Size = new Size(535, 24);
            currentCaption.TabIndex = 5;
            currentCaption.Text = "CURRENT PASSWORD";
            currentCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // currentField
            // 
            currentField.BackColor = Color.White;
            currentField.BorderColor = Color.FromArgb(229, 231, 235);
            currentField.Controls.Add(currentInput);
            currentField.Controls.Add(currentToggle);
            currentField.CornerRadius = 10;
            currentField.FillColor = Color.FromArgb(243, 244, 246);
            currentField.FocusColor = Color.FromArgb(22, 163, 74);
            currentField.HasError = false;
            currentField.IsFocused = false;
            currentField.Location = new Point(41, 235);
            currentField.Margin = new Padding(3, 4, 3, 4);
            currentField.Name = "currentField";
            currentField.Size = new Size(535, 69);
            currentField.TabIndex = 6;
            // 
            // currentInput
            // 
            currentInput.BackColor = Color.FromArgb(243, 244, 246);
            currentInput.BorderStyle = BorderStyle.None;
            currentInput.Font = new Font("Segoe UI", 11.5F);
            currentInput.Location = new Point(18, 17);
            currentInput.Margin = new Padding(3, 4, 3, 4);
            currentInput.Name = "currentInput";
            currentInput.Size = new Size(462, 26);
            currentInput.TabIndex = 0;
            currentInput.UseSystemPasswordChar = true;
            // 
            // currentToggle
            // 
            currentToggle.AccessibleName = "Show password";
            currentToggle.BackColor = Color.FromArgb(243, 244, 246);
            currentToggle.Location = new Point(489, 17);
            currentToggle.Margin = new Padding(3, 4, 3, 4);
            currentToggle.Name = "currentToggle";
            currentToggle.Size = new Size(30, 35);
            currentToggle.TabIndex = 1;
            currentToggle.TabStop = false;
            // 
            // currentError
            // 
            currentError.BackColor = Color.Transparent;
            currentError.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            currentError.ForeColor = Color.FromArgb(220, 38, 38);
            currentError.Location = new Point(43, 309);
            currentError.Name = "currentError";
            currentError.Size = new Size(530, 21);
            currentError.TabIndex = 7;
            currentError.TextAlign = ContentAlignment.MiddleLeft;
            currentError.Visible = false;
            // 
            // newCaption
            // 
            newCaption.BackColor = Color.Transparent;
            newCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            newCaption.ForeColor = Color.FromArgb(100, 116, 139);
            newCaption.Location = new Point(41, 349);
            newCaption.Name = "newCaption";
            newCaption.Size = new Size(535, 24);
            newCaption.TabIndex = 8;
            newCaption.Text = "NEW PASSWORD";
            newCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // newField
            // 
            newField.BackColor = Color.White;
            newField.BorderColor = Color.FromArgb(229, 231, 235);
            newField.Controls.Add(newInput);
            newField.Controls.Add(newToggle);
            newField.CornerRadius = 10;
            newField.FillColor = Color.FromArgb(243, 244, 246);
            newField.FocusColor = Color.FromArgb(22, 163, 74);
            newField.HasError = false;
            newField.IsFocused = false;
            newField.Location = new Point(41, 376);
            newField.Margin = new Padding(3, 4, 3, 4);
            newField.Name = "newField";
            newField.Size = new Size(535, 69);
            newField.TabIndex = 9;
            // 
            // newInput
            // 
            newInput.BackColor = Color.FromArgb(243, 244, 246);
            newInput.BorderStyle = BorderStyle.None;
            newInput.Font = new Font("Segoe UI", 11.5F);
            newInput.Location = new Point(18, 17);
            newInput.Margin = new Padding(3, 4, 3, 4);
            newInput.Name = "newInput";
            newInput.Size = new Size(462, 26);
            newInput.TabIndex = 0;
            newInput.UseSystemPasswordChar = true;
            // 
            // newToggle
            // 
            newToggle.AccessibleName = "Show password";
            newToggle.BackColor = Color.FromArgb(243, 244, 246);
            newToggle.Location = new Point(489, 17);
            newToggle.Margin = new Padding(3, 4, 3, 4);
            newToggle.Name = "newToggle";
            newToggle.Size = new Size(30, 35);
            newToggle.TabIndex = 1;
            newToggle.TabStop = false;
            // 
            // newError
            // 
            newError.BackColor = Color.Transparent;
            newError.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            newError.ForeColor = Color.FromArgb(220, 38, 38);
            newError.Location = new Point(43, 451);
            newError.Name = "newError";
            newError.Size = new Size(530, 21);
            newError.TabIndex = 10;
            newError.TextAlign = ContentAlignment.MiddleLeft;
            newError.Visible = false;
            // 
            // confirmCaption
            // 
            confirmCaption.BackColor = Color.Transparent;
            confirmCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            confirmCaption.ForeColor = Color.FromArgb(100, 116, 139);
            confirmCaption.Location = new Point(41, 485);
            confirmCaption.Name = "confirmCaption";
            confirmCaption.Size = new Size(535, 24);
            confirmCaption.TabIndex = 11;
            confirmCaption.Text = "CONFIRM NEW PASSWORD";
            confirmCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // confirmField
            // 
            confirmField.BackColor = Color.White;
            confirmField.BorderColor = Color.FromArgb(229, 231, 235);
            confirmField.Controls.Add(confirmInput);
            confirmField.Controls.Add(confirmToggle);
            confirmField.CornerRadius = 10;
            confirmField.FillColor = Color.FromArgb(243, 244, 246);
            confirmField.FocusColor = Color.FromArgb(22, 163, 74);
            confirmField.HasError = false;
            confirmField.IsFocused = false;
            confirmField.Location = new Point(41, 512);
            confirmField.Margin = new Padding(3, 4, 3, 4);
            confirmField.Name = "confirmField";
            confirmField.Size = new Size(535, 69);
            confirmField.TabIndex = 12;
            // 
            // confirmInput
            // 
            confirmInput.BackColor = Color.FromArgb(243, 244, 246);
            confirmInput.BorderStyle = BorderStyle.None;
            confirmInput.Font = new Font("Segoe UI", 11.5F);
            confirmInput.Location = new Point(18, 17);
            confirmInput.Margin = new Padding(3, 4, 3, 4);
            confirmInput.Name = "confirmInput";
            confirmInput.Size = new Size(462, 26);
            confirmInput.TabIndex = 0;
            confirmInput.UseSystemPasswordChar = true;
            // 
            // confirmToggle
            // 
            confirmToggle.AccessibleName = "Show password";
            confirmToggle.BackColor = Color.FromArgb(243, 244, 246);
            confirmToggle.Location = new Point(489, 17);
            confirmToggle.Margin = new Padding(3, 4, 3, 4);
            confirmToggle.Name = "confirmToggle";
            confirmToggle.Size = new Size(30, 35);
            confirmToggle.TabIndex = 1;
            confirmToggle.TabStop = false;
            // 
            // confirmError
            // 
            confirmError.BackColor = Color.Transparent;
            confirmError.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            confirmError.ForeColor = Color.FromArgb(220, 38, 38);
            confirmError.Location = new Point(43, 587);
            confirmError.Name = "confirmError";
            confirmError.Size = new Size(530, 21);
            confirmError.TabIndex = 13;
            confirmError.TextAlign = ContentAlignment.MiddleLeft;
            confirmError.Visible = false;
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
            cancelButton.Location = new Point(41, 672);
            cancelButton.Margin = new Padding(3, 4, 3, 4);
            cancelButton.Name = "cancelButton";
            cancelButton.NormalColor = Color.FromArgb(241, 245, 249);
            cancelButton.PressedColor = Color.FromArgb(203, 213, 225);
            cancelButton.Size = new Size(171, 61);
            cancelButton.TabIndex = 15;
            cancelButton.Text = "CANCEL";
            cancelButton.UseVisualStyleBackColor = false;
            // 
            // saveButton
            // 
            saveButton.BackColor = Color.Transparent;
            saveButton.CornerRadius = 11;
            saveButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            saveButton.ForeColor = Color.White;
            saveButton.HoverColor = Color.FromArgb(21, 128, 61);
            saveButton.Location = new Point(244, 672);
            saveButton.Margin = new Padding(3, 4, 3, 4);
            saveButton.Name = "saveButton";
            saveButton.NormalColor = Color.FromArgb(22, 163, 74);
            saveButton.PressedColor = Color.FromArgb(20, 83, 45);
            saveButton.Size = new Size(332, 61);
            saveButton.TabIndex = 16;
            saveButton.Text = "UPDATE PASSWORD";
            saveButton.UseVisualStyleBackColor = false;
            // 
            // hintLabel
            // 
            hintLabel.BackColor = Color.Transparent;
            hintLabel.Font = new Font("Segoe UI", 7F);
            hintLabel.ForeColor = Color.FromArgb(148, 163, 184);
            hintLabel.Location = new Point(41, 621);
            hintLabel.Name = "hintLabel";
            hintLabel.Size = new Size(535, 24);
            hintLabel.TabIndex = 14;
            hintLabel.Text = "FOR YOUR SECURITY, PLEASE USE YOUR NEW PASSWORD THE NEXT TIME YOU LOG IN.";
            hintLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ChangePasswordDialog
            // 
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = cancelButton;
            ClientSize = new Size(617, 768);
            Controls.Add(accentBar);
            Controls.Add(badge);
            Controls.Add(headingLabel);
            Controls.Add(divider);
            Controls.Add(currentCaption);
            Controls.Add(currentField);
            Controls.Add(currentError);
            Controls.Add(newCaption);
            Controls.Add(newField);
            Controls.Add(newError);
            Controls.Add(confirmCaption);
            Controls.Add(confirmField);
            Controls.Add(confirmError);
            Controls.Add(hintLabel);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePasswordDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "CHANGE PASSWORD";
            currentField.ResumeLayout(false);
            currentField.PerformLayout();
            newField.ResumeLayout(false);
            newField.PerformLayout();
            confirmField.ResumeLayout(false);
            confirmField.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private oop_project.AccentBar accentBar;
        private oop_project.PillBadge badge;
        private System.Windows.Forms.Label headingLabel;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Label currentCaption;
        private oop_project.FieldPanel currentField;
        private System.Windows.Forms.TextBox currentInput;
        private oop_project.PasswordToggle currentToggle;
        private System.Windows.Forms.Label currentError;
        private System.Windows.Forms.Label newCaption;
        private oop_project.FieldPanel newField;
        private System.Windows.Forms.TextBox newInput;
        private oop_project.PasswordToggle newToggle;
        private System.Windows.Forms.Label newError;
        private System.Windows.Forms.Label confirmCaption;
        private oop_project.FieldPanel confirmField;
        private System.Windows.Forms.TextBox confirmInput;
        private oop_project.PasswordToggle confirmToggle;
        private System.Windows.Forms.Label confirmError;
        private oop_project.RoundedButton cancelButton;
        private oop_project.RoundedButton saveButton;
        private Label hintLabel;
    }
}
