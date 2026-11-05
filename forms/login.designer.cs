namespace oop_project
{
    partial class Login
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
            logoBadge = new TcgcLogoBadge();
            headingLabel = new Label();
            usernameLabel = new Label();
            usernameField = new FieldPanel();
            username_input = new TextBox();
            passwordLabel = new Label();
            passwordField = new FieldPanel();
            password_input = new TextBox();
            password_toggle = new PasswordToggle();
            username_error = new Label();
            password_error = new Label();
            login_button = new RoundedButton();
            label1 = new Label();
            signup_label = new Label();
            gradientPanel1 = new GradientPanel();
            roundedCard1 = new RoundedCard();
            headerLine = new Panel();
            label2 = new Label();
            closeButton = new RoundedButton();
            lrcLogoBadge1 = new LrcLogoBadge();
            brandLabel = new Label();
            taglineLabel = new Label();
            usernameField.SuspendLayout();
            passwordField.SuspendLayout();
            gradientPanel1.SuspendLayout();
            roundedCard1.SuspendLayout();
            SuspendLayout();
            // 
            // logoBadge
            // 
            logoBadge.BackColor = Color.Transparent;
            logoBadge.Location = new Point(53, 22);
            logoBadge.Name = "logoBadge";
            logoBadge.Size = new Size(48, 48);
            logoBadge.TabIndex = 2;
            // 
            // headingLabel
            // 
            headingLabel.Location = new Point(0, 0);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(100, 23);
            headingLabel.TabIndex = 10;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            usernameLabel.ForeColor = Color.FromArgb(55, 65, 81);
            usernameLabel.Location = new Point(910, 361);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(98, 21);
            usernameLabel.TabIndex = 3;
            usernameLabel.Text = "USERNAME";
            // 
            // usernameField
            // 
            usernameField.BackColor = Color.White;
            usernameField.BorderColor = Color.FromArgb(229, 231, 235);
            usernameField.Controls.Add(username_input);
            usernameField.CornerRadius = 10;
            usernameField.FillColor = Color.FromArgb(243, 244, 246);
            usernameField.FocusColor = Color.FromArgb(22, 163, 74);
            usernameField.HasError = false;
            usernameField.IsFocused = false;
            usernameField.Location = new Point(910, 390);
            usernameField.Name = "usernameField";
            usernameField.Size = new Size(430, 46);
            usernameField.TabIndex = 4;
            // 
            // username_input
            // 
            username_input.BackColor = Color.FromArgb(243, 244, 246);
            username_input.BorderStyle = BorderStyle.None;
            username_input.Font = new Font("Segoe UI", 12F);
            username_input.Location = new Point(22, 10);
            username_input.Name = "username_input";
            username_input.Size = new Size(386, 27);
            username_input.TabIndex = 0;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            passwordLabel.ForeColor = Color.FromArgb(55, 65, 81);
            passwordLabel.Location = new Point(910, 468);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(98, 21);
            passwordLabel.TabIndex = 5;
            passwordLabel.Text = "PASSWORD";
            // 
            // passwordField
            // 
            passwordField.BackColor = Color.White;
            passwordField.BorderColor = Color.FromArgb(229, 231, 235);
            passwordField.Controls.Add(password_input);
            passwordField.Controls.Add(password_toggle);
            passwordField.CornerRadius = 10;
            passwordField.FillColor = Color.FromArgb(243, 244, 246);
            passwordField.FocusColor = Color.FromArgb(22, 163, 74);
            passwordField.HasError = false;
            passwordField.IsFocused = false;
            passwordField.Location = new Point(910, 496);
            passwordField.Name = "passwordField";
            passwordField.Size = new Size(430, 46);
            passwordField.TabIndex = 6;
            // 
            // password_input
            // 
            password_input.BackColor = Color.FromArgb(243, 244, 246);
            password_input.BorderStyle = BorderStyle.None;
            password_input.Font = new Font("Segoe UI", 12F);
            password_input.Location = new Point(22, 10);
            password_input.Name = "password_input";
            password_input.Size = new Size(360, 27);
            password_input.TabIndex = 0;
            password_input.UseSystemPasswordChar = true;
            // 
            // password_toggle
            // 
            password_toggle.AccessibleName = "Show password";
            password_toggle.BackColor = Color.FromArgb(243, 244, 246);
            password_toggle.Location = new Point(394, 10);
            password_toggle.Name = "password_toggle";
            password_toggle.Size = new Size(26, 26);
            password_toggle.TabIndex = 1;
            password_toggle.TabStop = false;
            // 
            // username_error
            // 
            username_error.BackColor = Color.Transparent;
            username_error.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            username_error.ForeColor = Color.FromArgb(220, 38, 38);
            username_error.Location = new Point(910, 441);
            username_error.Name = "username_error";
            username_error.Size = new Size(430, 16);
            username_error.TabIndex = 18;
            username_error.TextAlign = ContentAlignment.MiddleLeft;
            username_error.Visible = false;
            // 
            // password_error
            // 
            password_error.BackColor = Color.Transparent;
            password_error.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            password_error.ForeColor = Color.FromArgb(220, 38, 38);
            password_error.Location = new Point(910, 547);
            password_error.Name = "password_error";
            password_error.Size = new Size(430, 16);
            password_error.TabIndex = 19;
            password_error.TextAlign = ContentAlignment.MiddleLeft;
            password_error.Visible = false;
            // 
            // login_button
            // 
            login_button.BackColor = Color.Transparent;
            login_button.CornerRadius = 12;
            login_button.DisabledTextColor = Color.FromArgb(248, 250, 252);
            login_button.FlatAppearance.BorderSize = 0;
            login_button.FlatStyle = FlatStyle.Flat;
            login_button.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            login_button.ForeColor = Color.White;
            login_button.HoverColor = Color.FromArgb(21, 128, 61);
            login_button.Location = new Point(910, 581);
            login_button.Name = "login_button";
            login_button.NormalColor = Color.FromArgb(22, 163, 74);
            login_button.PressedColor = Color.FromArgb(20, 83, 45);
            login_button.Size = new Size(430, 52);
            login_button.TabIndex = 7;
            login_button.Text = "SIGN IN";
            login_button.UseVisualStyleBackColor = false;
            login_button.Click += login_button_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F);
            label1.ForeColor = Color.FromArgb(55, 65, 81);
            label1.Location = new Point(130, 430);
            label1.Name = "label1";
            label1.Size = new Size(191, 23);
            label1.TabIndex = 8;
            label1.Text = "Don't have an account?";
            // 
            // signup_label
            // 
            signup_label.AutoSize = true;
            signup_label.Cursor = Cursors.Hand;
            signup_label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            signup_label.ForeColor = Color.FromArgb(194, 65, 12);
            signup_label.Location = new Point(327, 430);
            signup_label.Name = "signup_label";
            signup_label.Size = new Size(74, 23);
            signup_label.TabIndex = 9;
            signup_label.Text = "Sign Up";
            signup_label.Click += signup_label_Click;
            // 
            // gradientPanel1
            // 
            gradientPanel1.BackColor = Color.FromArgb(6, 78, 59);
            gradientPanel1.Controls.Add(roundedCard1);
            gradientPanel1.Controls.Add(lrcLogoBadge1);
            gradientPanel1.Controls.Add(brandLabel);
            gradientPanel1.Controls.Add(taglineLabel);
            gradientPanel1.GradientBottom = Color.FromArgb(22, 163, 74);
            gradientPanel1.GradientTop = Color.FromArgb(6, 78, 59);
            gradientPanel1.Location = new Point(-3, 0);
            gradientPanel1.Name = "gradientPanel1";
            gradientPanel1.Size = new Size(1486, 856);
            gradientPanel1.TabIndex = 17;
            // 
            // roundedCard1
            // 
            roundedCard1.BackColor = Color.White;
            roundedCard1.BorderColor = Color.FromArgb(241, 245, 249);
            roundedCard1.Controls.Add(headerLine);
            roundedCard1.Controls.Add(label2);
            roundedCard1.Controls.Add(label1);
            roundedCard1.Controls.Add(signup_label);
            roundedCard1.Controls.Add(logoBadge);
            roundedCard1.Controls.Add(headingLabel);
            roundedCard1.Controls.Add(closeButton);
            roundedCard1.Location = new Point(867, 221);
            roundedCard1.Name = "roundedCard1";
            roundedCard1.Size = new Size(530, 473);
            roundedCard1.TabIndex = 3;
            // 
            // headerLine
            // 
            headerLine.BackColor = Color.FromArgb(229, 231, 235);
            headerLine.Location = new Point(53, 80);
            headerLine.Name = "headerLine";
            headerLine.Size = new Size(425, 1);
            headerLine.TabIndex = 49;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label2.ForeColor = Color.FromArgb(22, 163, 74);
            label2.Location = new Point(106, 22);
            label2.Name = "label2";
            label2.Size = new Size(174, 48);
            label2.TabIndex = 4;
            label2.Text = "SIGN IN";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // closeButton
            // 
            closeButton.BackColor = Color.Transparent;
            closeButton.CornerRadius = 12;
            closeButton.DisabledTextColor = Color.FromArgb(248, 250, 252);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            closeButton.ForeColor = Color.FromArgb(71, 85, 105);
            closeButton.HoverColor = Color.FromArgb(226, 232, 240);
            closeButton.Location = new Point(469, 18);
            closeButton.Name = "closeButton";
            closeButton.NormalColor = Color.FromArgb(241, 245, 249);
            closeButton.PressedColor = Color.FromArgb(203, 213, 225);
            closeButton.Size = new Size(36, 36);
            closeButton.TabIndex = 25;
            closeButton.Text = "✕";
            closeButton.UseVisualStyleBackColor = false;
            closeButton.Click += closeButton_Click;
            // 
            // lrcLogoBadge1
            // 
            lrcLogoBadge1.BackColor = Color.Transparent;
            lrcLogoBadge1.Location = new Point(0, 272);
            lrcLogoBadge1.LogoKind = LogoKind.Lrc;
            lrcLogoBadge1.Name = "lrcLogoBadge1";
            lrcLogoBadge1.Size = new Size(861, 410);
            lrcLogoBadge1.TabIndex = 2;
            lrcLogoBadge1.Text = "lrcLogoBadge1";
            // 
            // brandLabel
            // 
            brandLabel.BackColor = Color.Transparent;
            brandLabel.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            brandLabel.ForeColor = Color.White;
            brandLabel.Location = new Point(3, 199);
            brandLabel.Name = "brandLabel";
            brandLabel.Size = new Size(858, 24);
            brandLabel.TabIndex = 0;
            brandLabel.Text = "TCGC - LIBRARY MANAGEMENT SYSTEM";
            brandLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // taglineLabel
            // 
            taglineLabel.BackColor = Color.Transparent;
            taglineLabel.Font = new Font("Segoe UI", 9F);
            taglineLabel.ForeColor = Color.FromArgb(206, 240, 222);
            taglineLabel.Location = new Point(3, 225);
            taglineLabel.Name = "taglineLabel";
            taglineLabel.Size = new Size(858, 22);
            taglineLabel.TabIndex = 1;
            taglineLabel.Text = "Learning Resource Center";
            taglineLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Login
            // 
            AcceptButton = login_button;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1482, 853);
            Controls.Add(usernameField);
            Controls.Add(login_button);
            Controls.Add(passwordField);
            Controls.Add(passwordLabel);
            Controls.Add(usernameLabel);
            Controls.Add(username_error);
            Controls.Add(password_error);
            Controls.Add(gradientPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Login";
            Text = "TCGC LIBRARY MANAGEMENT SYSTEM - SIGN IN";
            usernameField.ResumeLayout(false);
            usernameField.PerformLayout();
            passwordField.ResumeLayout(false);
            passwordField.PerformLayout();
            gradientPanel1.ResumeLayout(false);
            roundedCard1.ResumeLayout(false);
            roundedCard1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TcgcLogoBadge logoBadge;
        private Label headingLabel;
        private Label usernameLabel;
        private FieldPanel usernameField;
        private Label passwordLabel;
        private FieldPanel passwordField;
        private RoundedButton login_button;
        private TextBox password_input;
        private PasswordToggle password_toggle;
        private TextBox username_input;
        private Label username_error;
        private Label password_error;
        private Label label1;
        private Label signup_label;
        private GradientPanel gradientPanel1;
        private LrcLogoBadge lrcLogoBadge1;
        private Label brandLabel;
        private Label taglineLabel;
        private RoundedCard roundedCard1;
        private RoundedButton closeButton;
        private Label label2;
        private Panel headerLine;
    }
}