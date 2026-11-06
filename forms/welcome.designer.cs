namespace oop_project
{
    partial class Welcome
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
            card = new RoundedCard();
            qr_scanner_button = new RoundedButton();
            evaluation_button = new RoundedButton();
            eyebrowChip = new PillBadge();
            tcgcLogoBadge = new TcgcLogoBadge();
            logoDivider = new Panel();
            lrcLogoBadge = new LrcLogoBadge();
            tcgcCaption = new Label();
            lrcCaption = new Label();
            headingLabel = new Label();
            subtitleLabel = new Label();
            start_button = new RoundedButton();
            footerDivider = new Panel();
            footerLabel = new Label();
            creditLabel = new Label();
            backdrop = new WelcomeBackdrop();
            card.SuspendLayout();
            backdrop.SuspendLayout();
            SuspendLayout();
            // 
            // card
            // 
            card.BackColor = Color.White;
            card.BorderColor = Color.FromArgb(238, 242, 247);
            card.Controls.Add(qr_scanner_button);
            card.Controls.Add(evaluation_button);
            card.Controls.Add(eyebrowChip);
            card.Controls.Add(tcgcLogoBadge);
            card.Controls.Add(logoDivider);
            card.Controls.Add(lrcLogoBadge);
            card.Controls.Add(tcgcCaption);
            card.Controls.Add(lrcCaption);
            card.Controls.Add(headingLabel);
            card.Controls.Add(subtitleLabel);
            card.Controls.Add(start_button);
            card.Controls.Add(footerDivider);
            card.Controls.Add(footerLabel);
            card.CornerRadius = 32;
            card.Location = new Point(310, 110);
            card.Name = "card";
            card.Size = new Size(780, 540);
            card.TabIndex = 0;
            // 
            // qr_scanner_button
            // 
            qr_scanner_button.BackColor = Color.Transparent;
            qr_scanner_button.CornerRadius = 1;
            qr_scanner_button.FlatAppearance.BorderSize = 0;
            qr_scanner_button.FlatStyle = FlatStyle.Flat;
            qr_scanner_button.Font = new Font("Segoe UI", 11F);
            qr_scanner_button.ForeColor = Color.White;
            qr_scanner_button.HoverColor = Color.FromArgb(4, 120, 87);
            qr_scanner_button.Location = new Point(260, 432);
            qr_scanner_button.Name = "qr_scanner_button";
            qr_scanner_button.NormalColor = Color.FromArgb(6, 95, 70);
            qr_scanner_button.PressedColor = Color.FromArgb(6, 78, 59);
            qr_scanner_button.Size = new Size(260, 35);
            qr_scanner_button.TabIndex = 12;
            qr_scanner_button.Text = "QR CODE SCANNER";
            qr_scanner_button.IconGlyph = "\uED14";
            qr_scanner_button.IconFallbackGlyph = "\uE8FE";
            qr_scanner_button.UseVisualStyleBackColor = false;
            qr_scanner_button.Click += roundedButton1_Click;
            // 
            // evaluation_button
            // 
            evaluation_button.BackColor = Color.Transparent;
            evaluation_button.CornerRadius = 1;
            evaluation_button.FlatAppearance.BorderSize = 0;
            evaluation_button.FlatStyle = FlatStyle.Flat;
            evaluation_button.Font = new Font("Segoe UI", 11F);
            evaluation_button.ForeColor = Color.White;
            evaluation_button.HoverColor = Color.FromArgb(21, 128, 61);
            evaluation_button.Location = new Point(388, 432);
            evaluation_button.Name = "evaluation_button";
            evaluation_button.NormalColor = Color.FromArgb(22, 163, 74);
            evaluation_button.PressedColor = Color.FromArgb(20, 83, 45);
            evaluation_button.Size = new Size(260, 35);
            evaluation_button.TabIndex = 13;
            evaluation_button.Text = "LIBRARY EVALUATION";
            evaluation_button.IconGlyph = "\uE73E";
            evaluation_button.UseVisualStyleBackColor = false;
            evaluation_button.Click += evaluation_button_Click;
            // 
            // eyebrowChip
            // 
            eyebrowChip.BackColor = Color.Transparent;
            eyebrowChip.BorderColor = Color.FromArgb(254, 215, 170);
            eyebrowChip.CornerRadius = 16;
            eyebrowChip.FillColor = Color.FromArgb(255, 247, 237);
            eyebrowChip.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            eyebrowChip.Location = new Point(220, 50);
            eyebrowChip.Name = "eyebrowChip";
            eyebrowChip.Size = new Size(326, 34);
            eyebrowChip.TabIndex = 0;
            eyebrowChip.TabStop = false;
            eyebrowChip.Text = "TCGC   ·   LEARNING RESOURCE CENTER";
            eyebrowChip.TextColor = Color.FromArgb(194, 65, 12);
            // 
            // tcgcLogoBadge
            // 
            tcgcLogoBadge.BackColor = Color.Transparent;
            tcgcLogoBadge.Location = new Point(260, 90);
            tcgcLogoBadge.Name = "tcgcLogoBadge";
            tcgcLogoBadge.Size = new Size(106, 106);
            tcgcLogoBadge.TabIndex = 1;
            tcgcLogoBadge.Text = "tcgcLogoBadge";
            // 
            // logoDivider
            // 
            logoDivider.BackColor = Color.FromArgb(226, 232, 240);
            logoDivider.Location = new Point(389, 119);
            logoDivider.Name = "logoDivider";
            logoDivider.Size = new Size(1, 48);
            logoDivider.TabIndex = 2;
            // 
            // lrcLogoBadge
            // 
            lrcLogoBadge.BackColor = Color.Transparent;
            lrcLogoBadge.Location = new Point(414, 90);
            lrcLogoBadge.LogoKind = LogoKind.Lrc;
            lrcLogoBadge.Name = "lrcLogoBadge";
            lrcLogoBadge.Size = new Size(106, 106);
            lrcLogoBadge.TabIndex = 3;
            lrcLogoBadge.Text = "lrcLogoBadge";
            // 
            // tcgcCaption
            // 
            tcgcCaption.BackColor = Color.Transparent;
            tcgcCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            tcgcCaption.ForeColor = Color.FromArgb(148, 163, 184);
            tcgcCaption.Location = new Point(260, 202);
            tcgcCaption.Name = "tcgcCaption";
            tcgcCaption.Size = new Size(106, 16);
            tcgcCaption.TabIndex = 4;
            tcgcCaption.Text = "TCGC";
            tcgcCaption.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lrcCaption
            // 
            lrcCaption.BackColor = Color.Transparent;
            lrcCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lrcCaption.ForeColor = Color.FromArgb(148, 163, 184);
            lrcCaption.Location = new Point(414, 202);
            lrcCaption.Name = "lrcCaption";
            lrcCaption.Size = new Size(106, 16);
            lrcCaption.TabIndex = 5;
            lrcCaption.Text = "LRC";
            lrcCaption.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // headingLabel
            // 
            headingLabel.BackColor = Color.Transparent;
            headingLabel.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            headingLabel.ForeColor = Color.FromArgb(15, 23, 42);
            headingLabel.Location = new Point(52, 256);
            headingLabel.Name = "headingLabel";
            headingLabel.Size = new Size(676, 54);
            headingLabel.TabIndex = 7;
            headingLabel.Text = "LIBRARY MANAGEMENT SYSTEM";
            headingLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // subtitleLabel
            // 
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.Font = new Font("Segoe UI", 11F);
            subtitleLabel.ForeColor = Color.FromArgb(71, 85, 105);
            subtitleLabel.Location = new Point(52, 310);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(676, 24);
            subtitleLabel.TabIndex = 8;
            subtitleLabel.Text = "Book records, borrowing, and visitor logs — organised in one secure place.";
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // start_button
            // 
            start_button.BackColor = Color.Transparent;
            start_button.CornerRadius = 15;
            start_button.FlatAppearance.BorderSize = 0;
            start_button.FlatStyle = FlatStyle.Flat;
            start_button.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            start_button.ForeColor = Color.White;
            start_button.HoverColor = Color.FromArgb(4, 120, 87);
            start_button.Location = new Point(230, 354);
            start_button.Name = "start_button";
            start_button.NormalColor = Color.FromArgb(6, 95, 70);
            start_button.PressedColor = Color.FromArgb(6, 78, 59);
            start_button.Size = new Size(320, 56);
            start_button.TabIndex = 9;
            start_button.Text = "PROCEED TO SIGN IN   →";
            start_button.UseVisualStyleBackColor = false;
            start_button.Click += start_button_Click;
            // 
            // footerDivider
            // 
            footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            footerDivider.Location = new Point(220, 470);
            footerDivider.Name = "footerDivider";
            footerDivider.Size = new Size(340, 1);
            footerDivider.TabIndex = 10;
            // 
            // footerLabel
            // 
            footerLabel.BackColor = Color.Transparent;
            footerLabel.Font = new Font("Segoe UI", 8.5F);
            footerLabel.ForeColor = Color.FromArgb(148, 163, 184);
            footerLabel.Location = new Point(52, 482);
            footerLabel.Name = "footerLabel";
            footerLabel.Size = new Size(676, 18);
            footerLabel.TabIndex = 11;
            footerLabel.Text = "Secure access for library staff, faculty, and students.";
            footerLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // creditLabel
            // 
            creditLabel.BackColor = Color.Transparent;
            creditLabel.Font = new Font("Segoe UI", 9F);
            creditLabel.ForeColor = Color.FromArgb(235, 246, 238, 228);
            creditLabel.Location = new Point(0, 712);
            creditLabel.Name = "creditLabel";
            creditLabel.Size = new Size(1400, 20);
            creditLabel.TabIndex = 1;
            creditLabel.Text = "© 2026 TANGUB CITY GLOBAL COLLEGE   ·   LEARNING RESOURCE CENTER";
            creditLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // backdrop
            // 
            backdrop.BackColor = Color.FromArgb(17, 32, 26);
            backdrop.CardBounds = new Rectangle(310, 110, 780, 540);
            backdrop.Controls.Add(creditLabel);
            backdrop.Controls.Add(card);
            backdrop.Dock = DockStyle.Fill;
            backdrop.Location = new Point(0, 0);
            backdrop.Name = "backdrop";
            backdrop.Size = new Size(1400, 760);
            backdrop.TabIndex = 0;
            // 
            // Welcome
            // 
            AcceptButton = start_button;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 32, 26);
            ClientSize = new Size(1400, 760);
            Controls.Add(backdrop);
            MinimumSize = new Size(980, 680);
            Name = "Welcome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCGC LIBRARY MANAGEMENT SYSTEM";
            card.ResumeLayout(false);
            card.PerformLayout();
            backdrop.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RoundedCard card;
        private PillBadge eyebrowChip;
        private TcgcLogoBadge tcgcLogoBadge;
        private Panel logoDivider;
        private LrcLogoBadge lrcLogoBadge;
        private Label tcgcCaption;
        private Label lrcCaption;
        private Label headingLabel;
        private RoundedButton start_button;
        private Panel footerDivider;
        private Label footerLabel;
        private Label creditLabel;
        private WelcomeBackdrop backdrop;
        private RoundedButton qr_scanner_button;
        private RoundedButton evaluation_button;
        private Label subtitleLabel;
    }
}
