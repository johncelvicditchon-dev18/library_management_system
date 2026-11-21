namespace library_management_system.forms
{
    partial class qr_code_scanner
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
            if (disposing && (components != null))
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
            backdrop = new oop_project.ScannerBackdrop();
            tcgcLogoBadge = new oop_project.TcgcLogoBadge();
            lrcLogoBadge = new oop_project.LrcLogoBadge();
            brandLabel = new Label();
            brandSubLabel = new Label();
            clockLabel = new Label();
            titleLabel = new Label();
            subtitleLabel = new Label();
            cameraArea = new Panel();
            cameraCard = new oop_project.RoundedCard();
            cameraView = new oop_project.ScannerCameraView();
            resultArea = new Panel();
            resultCard = new oop_project.ScannerResultCard();
            statusPill = new oop_project.ScannerStatusPill();
            backdrop.SuspendLayout();
            cameraArea.SuspendLayout();
            cameraCard.SuspendLayout();
            resultArea.SuspendLayout();
            SuspendLayout();
            // 
            // backdrop
            // 
            backdrop.Dock = DockStyle.Fill;
            backdrop.Location = new Point(0, 0);
            backdrop.Name = "backdrop";
            backdrop.Size = new Size(1280, 820);
            backdrop.TabIndex = 0;
            backdrop.Controls.Add(cameraArea);
            backdrop.Controls.Add(resultArea);
            backdrop.Controls.Add(statusPill);
            backdrop.Controls.Add(tcgcLogoBadge);
            backdrop.Controls.Add(lrcLogoBadge);
            backdrop.Controls.Add(brandLabel);
            backdrop.Controls.Add(brandSubLabel);
            backdrop.Controls.Add(clockLabel);
            backdrop.Controls.Add(titleLabel);
            backdrop.Controls.Add(subtitleLabel);
            // 
            // tcgcLogoBadge
            // 
            tcgcLogoBadge.BackColor = Color.Transparent;
            tcgcLogoBadge.Location = new Point(24, 30);
            tcgcLogoBadge.Margin = new Padding(0);
            tcgcLogoBadge.Name = "tcgcLogoBadge";
            tcgcLogoBadge.Size = new Size(44, 44);
            tcgcLogoBadge.TabIndex = 1;
            tcgcLogoBadge.Text = "tcgcLogoBadge";
            // 
            // lrcLogoBadge
            // 
            lrcLogoBadge.BackColor = Color.Transparent;
            lrcLogoBadge.Location = new Point(76, 30);
            lrcLogoBadge.Margin = new Padding(0);
            lrcLogoBadge.Name = "lrcLogoBadge";
            lrcLogoBadge.Size = new Size(44, 44);
            lrcLogoBadge.TabIndex = 2;
            lrcLogoBadge.Text = "lrcLogoBadge";
            // 
            // brandLabel
            // 
            brandLabel.AutoEllipsis = true;
            brandLabel.BackColor = Color.Transparent;
            brandLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            brandLabel.ForeColor = Color.FromArgb(15, 23, 42);
            brandLabel.Location = new Point(144, 38);
            brandLabel.Name = "brandLabel";
            brandLabel.Size = new Size(190, 20);
            brandLabel.TabIndex = 3;
            brandLabel.Text = "TCGC LIBRARY";
            brandLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // brandSubLabel
            // 
            brandSubLabel.AutoEllipsis = true;
            brandSubLabel.BackColor = Color.Transparent;
            brandSubLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            brandSubLabel.ForeColor = Color.FromArgb(4, 120, 87);
            brandSubLabel.Location = new Point(144, 59);
            brandSubLabel.Name = "brandSubLabel";
            brandSubLabel.Size = new Size(190, 17);
            brandSubLabel.TabIndex = 4;
            brandSubLabel.Text = "LEARNING RESOURCE CENTER";
            brandSubLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // clockLabel
            // 
            clockLabel.AutoEllipsis = true;
            clockLabel.BackColor = Color.Transparent;
            clockLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            clockLabel.ForeColor = Color.FromArgb(71, 85, 105);
            clockLabel.Location = new Point(1180, 40);
            clockLabel.Name = "clockLabel";
            clockLabel.Size = new Size(200, 24);
            clockLabel.TabIndex = 5;
            clockLabel.Text = "FRI, NOV 13  •  8:41 AM";
            clockLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // titleLabel
            // 
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            titleLabel.Location = new Point(340, 20);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(600, 42);
            titleLabel.TabIndex = 7;
            titleLabel.Text = "QR CODE SCANNER";
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // subtitleLabel
            // 
            subtitleLabel.AutoEllipsis = true;
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            subtitleLabel.ForeColor = Color.FromArgb(100, 116, 139);
            subtitleLabel.Location = new Point(340, 62);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(600, 20);
            subtitleLabel.TabIndex = 9;
            subtitleLabel.Text = "SCAN YOUR LIBRARY QR CODE TO RECORD YOUR VISIT.";
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cameraArea
            // 
            cameraArea.BackColor = Color.Transparent;
            cameraArea.Controls.Add(cameraCard);
            cameraArea.Location = new Point(326, 168);
            cameraArea.Margin = new Padding(0);
            cameraArea.Name = "cameraArea";
            cameraArea.Size = new Size(628, 628);
            cameraArea.TabIndex = 10;
            // 
            // cameraCard
            // 
            cameraCard.BackColor = Color.FromArgb(9, 15, 28);
            cameraCard.BorderColor = Color.FromArgb(30, 41, 59);
            cameraCard.Controls.Add(cameraView);
            cameraCard.CornerRadius = 24;
            cameraCard.Location = new Point(14, 14);
            cameraCard.Margin = new Padding(0);
            cameraCard.Name = "cameraCard";
            cameraCard.Size = new Size(600, 600);
            cameraCard.TabIndex = 0;
            // 
            // cameraView
            // 
            cameraView.BackColor = Color.FromArgb(9, 15, 28);
            cameraView.Dock = DockStyle.Fill;
            cameraView.Location = new Point(0, 0);
            cameraView.Name = "cameraView";
            cameraView.Size = new Size(600, 600);
            cameraView.TabIndex = 0;
            // 
            // resultArea
            // 
            resultArea.BackColor = Color.Transparent;
            resultArea.Controls.Add(resultCard);
            resultArea.Location = new Point(326, 814);
            resultArea.Margin = new Padding(0);
            resultArea.Name = "resultArea";
            resultArea.Size = new Size(628, 140);
            resultArea.TabIndex = 11;
            // 
            // resultCard
            // 
            resultCard.BackColor = Color.Transparent;
            resultCard.Location = new Point(14, 14);
            resultCard.Margin = new Padding(0);
            resultCard.Name = "resultCard";
            resultCard.Size = new Size(600, 112);
            resultCard.TabIndex = 0;
            // 
            // statusPill
            // 
            statusPill.BackColor = Color.Transparent;
            statusPill.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            statusPill.Location = new Point(340, 966);
            statusPill.Name = "statusPill";
            statusPill.Size = new Size(600, 44);
            statusPill.TabIndex = 12;
            statusPill.Text = "statusPill";
            // 
            // qr_code_scanner
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(247, 250, 248);
            ClientSize = new Size(1280, 820);
            Controls.Add(backdrop);
            MinimumSize = new Size(1000, 700);
            Name = "qr_code_scanner";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCGC - QR CODE SCANNER";
            backdrop.ResumeLayout(false);
            cameraArea.ResumeLayout(false);
            cameraCard.ResumeLayout(false);
            resultArea.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private oop_project.ScannerBackdrop backdrop;
        private oop_project.TcgcLogoBadge tcgcLogoBadge;
        private oop_project.LrcLogoBadge lrcLogoBadge;
        private Label brandLabel;
        private Label brandSubLabel;
        private Label clockLabel;
        private Label titleLabel;
        private Label subtitleLabel;
        private Panel cameraArea;
        private oop_project.RoundedCard cameraCard;
        private oop_project.ScannerCameraView cameraView;
        private Panel resultArea;
        private oop_project.ScannerResultCard resultCard;
        private oop_project.ScannerStatusPill statusPill;
    }
}
