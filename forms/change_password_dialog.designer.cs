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
            this.accentBar = new oop_project.AccentBar();
            this.badge = new oop_project.PillBadge();
            this.headingLabel = new System.Windows.Forms.Label();
            this.accountLabel = new System.Windows.Forms.Label();
            this.divider = new System.Windows.Forms.Panel();
            this.currentCaption = new System.Windows.Forms.Label();
            this.currentField = new oop_project.FieldPanel();
            this.currentInput = new System.Windows.Forms.TextBox();
            this.currentToggle = new oop_project.PasswordToggle();
            this.currentError = new System.Windows.Forms.Label();
            this.newCaption = new System.Windows.Forms.Label();
            this.newField = new oop_project.FieldPanel();
            this.newInput = new System.Windows.Forms.TextBox();
            this.newToggle = new oop_project.PasswordToggle();
            this.newError = new System.Windows.Forms.Label();
            this.confirmCaption = new System.Windows.Forms.Label();
            this.confirmField = new oop_project.FieldPanel();
            this.confirmInput = new System.Windows.Forms.TextBox();
            this.confirmToggle = new oop_project.PasswordToggle();
            this.confirmError = new System.Windows.Forms.Label();
            this.hintLabel = new System.Windows.Forms.Label();
            this.cancelButton = new oop_project.RoundedButton();
            this.saveButton = new oop_project.RoundedButton();
            this.currentField.SuspendLayout();
            this.newField.SuspendLayout();
            this.confirmField.SuspendLayout();
            this.SuspendLayout();
            // 
            // accentBar
            // 
            this.accentBar.BackColor = System.Drawing.Color.Transparent;
            this.accentBar.EndColor = System.Drawing.Color.FromArgb(5, 150, 105);
            this.accentBar.Location = new System.Drawing.Point(0, 0);
            this.accentBar.Name = "accentBar";
            this.accentBar.Size = new System.Drawing.Size(540, 6);
            this.accentBar.StartColor = System.Drawing.Color.FromArgb(74, 222, 128);
            this.accentBar.TabIndex = 0;
            this.accentBar.TabStop = false;
            // 
            // badge
            // 
            this.badge.AutoSize = true;
            this.badge.BackColor = System.Drawing.Color.Transparent;
            this.badge.BorderColor = System.Drawing.Color.FromArgb(187, 247, 208);
            this.badge.FillColor = System.Drawing.Color.FromArgb(240, 253, 244);
            this.badge.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.badge.Location = new System.Drawing.Point(36, 28);
            this.badge.Name = "badge";
            this.badge.Size = new System.Drawing.Size(134, 27);
            this.badge.TabIndex = 1;
            this.badge.TabStop = false;
            this.badge.Text = "ACCOUNT SECURITY";
            this.badge.TextColor = System.Drawing.Color.FromArgb(21, 128, 61);
            // 
            // headingLabel
            // 
            this.headingLabel.BackColor = System.Drawing.Color.Transparent;
            this.headingLabel.Font = new System.Drawing.Font("Segoe UI", 19F, System.Drawing.FontStyle.Bold);
            this.headingLabel.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.headingLabel.Location = new System.Drawing.Point(34, 62);
            this.headingLabel.Name = "headingLabel";
            this.headingLabel.Size = new System.Drawing.Size(472, 40);
            this.headingLabel.TabIndex = 2;
            this.headingLabel.Text = "CHANGE PASSWORD";
            this.headingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // accountLabel
            // 
            this.accountLabel.AutoEllipsis = true;
            this.accountLabel.BackColor = System.Drawing.Color.Transparent;
            this.accountLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.accountLabel.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.accountLabel.Location = new System.Drawing.Point(36, 104);
            this.accountLabel.Name = "accountLabel";
            this.accountLabel.Size = new System.Drawing.Size(468, 22);
            this.accountLabel.TabIndex = 3;
            this.accountLabel.Text = "SIGNED IN AS  @ACCOUNT";
            // 
            // divider
            // 
            this.divider.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.divider.Location = new System.Drawing.Point(36, 140);
            this.divider.Name = "divider";
            this.divider.Size = new System.Drawing.Size(468, 1);
            this.divider.TabIndex = 4;
            // 
            // currentCaption
            // 
            this.currentCaption.BackColor = System.Drawing.Color.Transparent;
            this.currentCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.currentCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.currentCaption.Location = new System.Drawing.Point(36, 156);
            this.currentCaption.Name = "currentCaption";
            this.currentCaption.Size = new System.Drawing.Size(468, 18);
            this.currentCaption.TabIndex = 5;
            this.currentCaption.Text = "CURRENT PASSWORD";
            this.currentCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // currentField
            // 
            this.currentField.BackColor = System.Drawing.Color.White;
            this.currentField.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.currentField.Controls.Add(this.currentInput);
            this.currentField.Controls.Add(this.currentToggle);
            this.currentField.CornerRadius = 10;
            this.currentField.FillColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.currentField.FocusColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.currentField.IsFocused = false;
            this.currentField.Location = new System.Drawing.Point(36, 176);
            this.currentField.Name = "currentField";
            this.currentField.Size = new System.Drawing.Size(468, 52);
            this.currentField.TabIndex = 6;
            // 
            // currentInput
            // 
            this.currentInput.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.currentInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.currentInput.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.currentInput.Location = new System.Drawing.Point(16, 13);
            this.currentInput.Name = "currentInput";
            this.currentInput.Size = new System.Drawing.Size(404, 26);
            this.currentInput.TabIndex = 0;
            this.currentInput.UseSystemPasswordChar = true;
            // 
            // currentToggle
            // 
            this.currentToggle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.currentToggle.Location = new System.Drawing.Point(428, 13);
            this.currentToggle.Name = "currentToggle";
            this.currentToggle.Size = new System.Drawing.Size(26, 26);
            this.currentToggle.TabIndex = 1;
            this.currentToggle.TabStop = false;
            // 
            // currentError
            // 
            this.currentError.BackColor = System.Drawing.Color.Transparent;
            this.currentError.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.currentError.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.currentError.Location = new System.Drawing.Point(38, 232);
            this.currentError.Name = "currentError";
            this.currentError.Size = new System.Drawing.Size(464, 16);
            this.currentError.TabIndex = 7;
            this.currentError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.currentError.Visible = false;
            // 
            // newCaption
            // 
            this.newCaption.BackColor = System.Drawing.Color.Transparent;
            this.newCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.newCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.newCaption.Location = new System.Drawing.Point(36, 262);
            this.newCaption.Name = "newCaption";
            this.newCaption.Size = new System.Drawing.Size(468, 18);
            this.newCaption.TabIndex = 8;
            this.newCaption.Text = "NEW PASSWORD";
            this.newCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // newField
            // 
            this.newField.BackColor = System.Drawing.Color.White;
            this.newField.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.newField.Controls.Add(this.newInput);
            this.newField.Controls.Add(this.newToggle);
            this.newField.CornerRadius = 10;
            this.newField.FillColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.newField.FocusColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.newField.IsFocused = false;
            this.newField.Location = new System.Drawing.Point(36, 282);
            this.newField.Name = "newField";
            this.newField.Size = new System.Drawing.Size(468, 52);
            this.newField.TabIndex = 9;
            // 
            // newInput
            // 
            this.newInput.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.newInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.newInput.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.newInput.Location = new System.Drawing.Point(16, 13);
            this.newInput.Name = "newInput";
            this.newInput.Size = new System.Drawing.Size(404, 26);
            this.newInput.TabIndex = 0;
            this.newInput.UseSystemPasswordChar = true;
            // 
            // newToggle
            // 
            this.newToggle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.newToggle.Location = new System.Drawing.Point(428, 13);
            this.newToggle.Name = "newToggle";
            this.newToggle.Size = new System.Drawing.Size(26, 26);
            this.newToggle.TabIndex = 1;
            this.newToggle.TabStop = false;
            // 
            // newError
            // 
            this.newError.BackColor = System.Drawing.Color.Transparent;
            this.newError.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.newError.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.newError.Location = new System.Drawing.Point(38, 338);
            this.newError.Name = "newError";
            this.newError.Size = new System.Drawing.Size(464, 16);
            this.newError.TabIndex = 10;
            this.newError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.newError.Visible = false;
            // 
            // confirmCaption
            // 
            this.confirmCaption.BackColor = System.Drawing.Color.Transparent;
            this.confirmCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.confirmCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.confirmCaption.Location = new System.Drawing.Point(36, 364);
            this.confirmCaption.Name = "confirmCaption";
            this.confirmCaption.Size = new System.Drawing.Size(468, 18);
            this.confirmCaption.TabIndex = 11;
            this.confirmCaption.Text = "CONFIRM NEW PASSWORD";
            this.confirmCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // confirmField
            // 
            this.confirmField.BackColor = System.Drawing.Color.White;
            this.confirmField.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.confirmField.Controls.Add(this.confirmInput);
            this.confirmField.Controls.Add(this.confirmToggle);
            this.confirmField.CornerRadius = 10;
            this.confirmField.FillColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.confirmField.FocusColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.confirmField.IsFocused = false;
            this.confirmField.Location = new System.Drawing.Point(36, 384);
            this.confirmField.Name = "confirmField";
            this.confirmField.Size = new System.Drawing.Size(468, 52);
            this.confirmField.TabIndex = 12;
            // 
            // confirmInput
            // 
            this.confirmInput.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.confirmInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.confirmInput.Font = new System.Drawing.Font("Segoe UI", 11.5F);
            this.confirmInput.Location = new System.Drawing.Point(16, 13);
            this.confirmInput.Name = "confirmInput";
            this.confirmInput.Size = new System.Drawing.Size(404, 26);
            this.confirmInput.TabIndex = 0;
            this.confirmInput.UseSystemPasswordChar = true;
            // 
            // confirmToggle
            // 
            this.confirmToggle.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.confirmToggle.Location = new System.Drawing.Point(428, 13);
            this.confirmToggle.Name = "confirmToggle";
            this.confirmToggle.Size = new System.Drawing.Size(26, 26);
            this.confirmToggle.TabIndex = 1;
            this.confirmToggle.TabStop = false;
            // 
            // confirmError
            // 
            this.confirmError.BackColor = System.Drawing.Color.Transparent;
            this.confirmError.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.confirmError.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.confirmError.Location = new System.Drawing.Point(38, 440);
            this.confirmError.Name = "confirmError";
            this.confirmError.Size = new System.Drawing.Size(464, 16);
            this.confirmError.TabIndex = 13;
            this.confirmError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.confirmError.Visible = false;
            // 
            // hintLabel
            // 
            this.hintLabel.BackColor = System.Drawing.Color.Transparent;
            this.hintLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.hintLabel.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.hintLabel.Location = new System.Drawing.Point(36, 466);
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Size = new System.Drawing.Size(468, 18);
            this.hintLabel.TabIndex = 14;
            this.hintLabel.Text = "ANY PASSWORD LENGTH IS ACCEPTED. THE CHANGE IS SAVED IMMEDIATELY.";
            this.hintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.CornerRadius = 11;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.cancelButton.HoverColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.cancelButton.Location = new System.Drawing.Point(36, 504);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.NormalColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.cancelButton.PressedColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.cancelButton.Size = new System.Drawing.Size(150, 46);
            this.cancelButton.TabIndex = 15;
            this.cancelButton.Text = "CANCEL";
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.Transparent;
            this.saveButton.CornerRadius = 11;
            this.saveButton.FlatAppearance.BorderSize = 0;
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.saveButton.ForeColor = System.Drawing.Color.White;
            this.saveButton.HoverColor = System.Drawing.Color.FromArgb(21, 128, 61);
            this.saveButton.Location = new System.Drawing.Point(314, 504);
            this.saveButton.Name = "saveButton";
            this.saveButton.NormalColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.saveButton.PressedColor = System.Drawing.Color.FromArgb(20, 83, 45);
            this.saveButton.Size = new System.Drawing.Size(190, 46);
            this.saveButton.TabIndex = 16;
            this.saveButton.Text = "UPDATE PASSWORD";
            this.saveButton.UseVisualStyleBackColor = false;
            // 
            // ChangePasswordDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(540, 576);
            this.Controls.Add(this.accentBar);
            this.Controls.Add(this.badge);
            this.Controls.Add(this.headingLabel);
            this.Controls.Add(this.accountLabel);
            this.Controls.Add(this.divider);
            this.Controls.Add(this.currentCaption);
            this.Controls.Add(this.currentField);
            this.Controls.Add(this.currentError);
            this.Controls.Add(this.newCaption);
            this.Controls.Add(this.newField);
            this.Controls.Add(this.newError);
            this.Controls.Add(this.confirmCaption);
            this.Controls.Add(this.confirmField);
            this.Controls.Add(this.confirmError);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangePasswordDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CHANGE PASSWORD";
            this.currentField.ResumeLayout(false);
            this.currentField.PerformLayout();
            this.newField.ResumeLayout(false);
            this.newField.PerformLayout();
            this.confirmField.ResumeLayout(false);
            this.confirmField.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private oop_project.AccentBar accentBar;
        private oop_project.PillBadge badge;
        private System.Windows.Forms.Label headingLabel;
        private System.Windows.Forms.Label accountLabel;
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
        private System.Windows.Forms.Label hintLabel;
        private oop_project.RoundedButton cancelButton;
        private oop_project.RoundedButton saveButton;
    }
}
