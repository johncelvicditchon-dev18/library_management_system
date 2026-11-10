namespace oop_project.forms
{
    partial class EvaluationLogin
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
            subtitleLabel = new Label();
            divider = new Panel();
            semesterField = new DialogField();
            usernameField = new DialogField();
            passwordField = new DialogField();
            signInButton = new RoundedButton();
            cancelButton = new RoundedButton();
            SuspendLayout();
            // 
            // accentBar
            // 
            accentBar.BackColor = Color.Transparent;
            accentBar.EndColor = Color.FromArgb(5, 150, 105);
            accentBar.Location = new Point(0, 0);
            accentBar.Margin = new Padding(3, 4, 3, 4);
            accentBar.Name = "accentBar";
            accentBar.Size = new Size(560, 8);
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
            badge.Location = new Point(40, 32);
            badge.Margin = new Padding(3, 4, 3, 4);
            badge.Name = "badge";
            badge.Size = new Size(191, 33);
            badge.TabIndex = 1;
            badge.TabStop = false;
            badge.Text = "LIBRARY EVALUATION";
            badge.TextColor = Color.FromArgb(21, 128, 61);
            // 
            // headingLabel
            // 
            headingLabel.BackColor = Color.Transparent;
            headingLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            headingLabel.ForeColor = Color.FromArgb(15, 23, 42);
            headingLabel.Location = new Point(37, 78);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(486, 50);
            headingLabel.TabIndex = 2;
            headingLabel.Text = "SIGN IN TO EVALUATE";
            headingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // subtitleLabel
            // 
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            subtitleLabel.ForeColor = Color.FromArgb(100, 116, 139);
            subtitleLabel.Location = new Point(39, 134);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(482, 22);
            subtitleLabel.TabIndex = 3;
            subtitleLabel.Text = "PICK THE OPEN SEMESTER, THEN SIGN IN WITH YOUR ACCOUNT.";
            // 
            // divider
            // 
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(40, 172);
            divider.Margin = new Padding(3, 4, 3, 4);
            divider.Name = "divider";
            divider.Size = new Size(480, 1);
            divider.TabIndex = 4;
            // 
            // semesterField
            // 
            semesterField.BackColor = Color.White;
            semesterField.Caption = "SEMESTER TO EVALUATE";
            semesterField.Kind = DialogFieldKind.Choice;
            semesterField.Location = new Point(40, 190);
            semesterField.Name = "semesterField";
            semesterField.Size = new Size(480, 88);
            semesterField.TabIndex = 5;
            semesterField.Value = "";
            // 
            // usernameField
            // 
            usernameField.BackColor = Color.White;
            usernameField.Caption = "USERNAME";
            usernameField.Kind = DialogFieldKind.Text;
            usernameField.Location = new Point(40, 284);
            usernameField.Name = "usernameField";
            usernameField.Size = new Size(480, 88);
            usernameField.TabIndex = 6;
            usernameField.Value = "";
            // 
            // passwordField
            // 
            passwordField.BackColor = Color.White;
            passwordField.Caption = "PASSWORD";
            passwordField.Kind = DialogFieldKind.Text;
            passwordField.Location = new Point(40, 378);
            passwordField.Name = "passwordField";
            passwordField.Size = new Size(480, 88);
            passwordField.TabIndex = 7;
            passwordField.Value = "";
            // 
            // signInButton
            // 
            signInButton.BackColor = Color.Transparent;
            signInButton.CornerRadius = 11;
            signInButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            signInButton.FlatAppearance.BorderSize = 0;
            signInButton.FlatStyle = FlatStyle.Flat;
            signInButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            signInButton.ForeColor = Color.White;
            signInButton.HoverColor = Color.FromArgb(21, 128, 61);
            signInButton.IconFallbackGlyph = "";
            signInButton.IconGlyph = "";
            signInButton.IconSize = 16F;
            signInButton.Location = new Point(226, 494);
            signInButton.Name = "signInButton";
            signInButton.NormalColor = Color.FromArgb(22, 163, 74);
            signInButton.PressedColor = Color.FromArgb(20, 83, 45);
            signInButton.Size = new Size(294, 50);
            signInButton.TabIndex = 9;
            signInButton.Text = "SIGN IN";
            signInButton.UseVisualStyleBackColor = false;
            signInButton.Click += signInButton_Click_1;
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
            cancelButton.IconFallbackGlyph = "";
            cancelButton.IconGlyph = "";
            cancelButton.IconSize = 16F;
            cancelButton.Location = new Point(40, 494);
            cancelButton.Name = "cancelButton";
            cancelButton.NormalColor = Color.FromArgb(241, 245, 249);
            cancelButton.PressedColor = Color.FromArgb(203, 213, 225);
            cancelButton.Size = new Size(180, 50);
            cancelButton.TabIndex = 8;
            cancelButton.Text = "CANCEL";
            cancelButton.UseVisualStyleBackColor = false;
            // 
            // EvaluationLogin
            // 
            AcceptButton = signInButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = cancelButton;
            ClientSize = new Size(560, 574);
            Controls.Add(accentBar);
            Controls.Add(badge);
            Controls.Add(headingLabel);
            Controls.Add(subtitleLabel);
            Controls.Add(divider);
            Controls.Add(semesterField);
            Controls.Add(usernameField);
            Controls.Add(passwordField);
            Controls.Add(cancelButton);
            Controls.Add(signInButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EvaluationLogin";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "LIBRARY EVALUATION SIGN IN";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private AccentBar accentBar;
        private PillBadge badge;
        private Label headingLabel;
        private Label subtitleLabel;
        private Panel divider;
        private DialogField semesterField;
        private DialogField usernameField;
        private DialogField passwordField;
        private RoundedButton signInButton;
        private RoundedButton cancelButton;
    }
}
