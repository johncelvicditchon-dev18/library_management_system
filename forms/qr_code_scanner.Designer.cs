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
            rootLayout = new TableLayoutPanel();
            headerLayout = new TableLayoutPanel();
            logoRow = new TableLayoutPanel();
            tcgcLogoBadge = new oop_project.TcgcLogoBadge();
            lrcLogoBadge = new oop_project.LrcLogoBadge();
            titleLabel = new Label();
            cameraArea = new Panel();
            cameraCard = new oop_project.RoundedCard();
            cameraPreview = new PictureBox();
            cameraPlaceholder = new Label();
            nameArea = new Panel();
            nameCard = new oop_project.RoundedCard();
            nameLabel = new Label();
            statusLabel = new Label();
            rootLayout.SuspendLayout();
            headerLayout.SuspendLayout();
            logoRow.SuspendLayout();
            cameraArea.SuspendLayout();
            cameraCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cameraPreview).BeginInit();
            nameArea.SuspendLayout();
            nameCard.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.BackColor = Color.White;
            rootLayout.ColumnCount = 3;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 600F));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            rootLayout.Controls.Add(headerLayout, 1, 1);
            rootLayout.Controls.Add(cameraArea, 1, 2);
            rootLayout.Controls.Add(nameArea, 1, 3);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(0, 0);
            rootLayout.Name = "rootLayout";
            rootLayout.Padding = new Padding(24, 24, 24, 26);
            rootLayout.RowCount = 5;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 600F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rootLayout.Size = new Size(1180, 780);
            rootLayout.TabIndex = 0;
            // 
          
            // headerLayout
            // 
            headerLayout.BackColor = Color.White;
            headerLayout.ColumnCount = 1;
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.Controls.Add(logoRow, 0, 0);
            headerLayout.Controls.Add(titleLabel, 0, 1);
            headerLayout.Dock = DockStyle.Fill;
            headerLayout.Location = new Point(369, 27);
            headerLayout.Margin = new Padding(0);
            headerLayout.Name = "headerLayout";
            headerLayout.RowCount = 2;
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            headerLayout.Size = new Size(490, 170);
            headerLayout.TabIndex = 0;
            // 
            // logoRow
            // 
            logoRow.BackColor = Color.White;
            logoRow.ColumnCount = 5;
            logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
            logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            logoRow.Controls.Add(tcgcLogoBadge, 1, 0);
            logoRow.Controls.Add(lrcLogoBadge, 3, 0);
            logoRow.Dock = DockStyle.Fill;
            logoRow.Location = new Point(0, 0);
            logoRow.Margin = new Padding(0, 10, 0, 0);
            logoRow.Name = "logoRow";
            logoRow.RowCount = 1;
            logoRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            logoRow.Size = new Size(490, 76);
            logoRow.TabIndex = 0;
            // 
            // tcgcLogoBadge
            // 
            tcgcLogoBadge.BackColor = Color.Transparent;
            tcgcLogoBadge.Dock = DockStyle.Fill;
            tcgcLogoBadge.Location = new Point(181, 0);
            tcgcLogoBadge.Margin = new Padding(0);
            tcgcLogoBadge.Name = "tcgcLogoBadge";
            tcgcLogoBadge.Size = new Size(64, 64);
            tcgcLogoBadge.TabIndex = 0;
            tcgcLogoBadge.Text = "tcgcLogoBadge";
            // 
            // lrcLogoBadge
            // 
            lrcLogoBadge.BackColor = Color.Transparent;
            lrcLogoBadge.Dock = DockStyle.Fill;
            lrcLogoBadge.Location = new Point(277, 0);
            lrcLogoBadge.LogoKind = oop_project.LogoKind.Lrc;
            lrcLogoBadge.Margin = new Padding(0);
            lrcLogoBadge.Name = "lrcLogoBadge";
            lrcLogoBadge.Size = new Size(64, 64);
            lrcLogoBadge.TabIndex = 1;
            lrcLogoBadge.Text = "lrcLogoBadge";
            // 
            // titleLabel
            // 
            titleLabel.BackColor = Color.White;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(17, 24, 39);
            titleLabel.Location = new Point(0, 86);
            titleLabel.Margin = new Padding(0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(490, 84);
            titleLabel.TabIndex = 1;
            titleLabel.Text = "QR CODE SCANNER";
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cameraArea
            // 
            cameraArea.BackColor = Color.White;
            cameraArea.Controls.Add(cameraCard);
            cameraArea.Dock = DockStyle.Fill;
            cameraArea.Location = new Point(369, 197);
            cameraArea.Margin = new Padding(0, 8, 0, 18);
            cameraArea.Name = "cameraArea";
            cameraArea.Padding = new Padding(12, 12, 20, 20);
            cameraArea.Size = new Size(600, 600);
            cameraArea.TabIndex = 1;    
            // 
            // cameraCard
            // 
            cameraCard.BackColor = Color.White;
            cameraCard.BorderColor = Color.FromArgb(241, 245, 249);
            cameraCard.Controls.Add(cameraPreview);
            cameraCard.Controls.Add(cameraPlaceholder);
            cameraCard.CornerRadius = 18;
            cameraCard.Dock = DockStyle.Fill;
            cameraCard.Location = new Point(12, 12);
            cameraCard.Name = "cameraCard";
            cameraCard.Size = new Size(458, 262);
            cameraCard.TabIndex = 0;
            // 
            // cameraPreview
            // 
            cameraPreview.BackColor = Color.White;
            cameraPreview.Dock = DockStyle.Fill;
            cameraPreview.Location = new Point(0, 0);
            cameraPreview.Name = "cameraPreview";
            cameraPreview.Size = new Size(600, 600);
            cameraPreview.SizeMode = PictureBoxSizeMode.Zoom;
            cameraPreview.TabIndex = 0;
            cameraPreview.TabStop = false;
        
            // 
            // cameraPlaceholder
            // 
            cameraPlaceholder.BackColor = Color.Transparent;
            cameraPlaceholder.Dock = DockStyle.Fill;
            cameraPlaceholder.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            cameraPlaceholder.ForeColor = Color.FromArgb(209, 213, 219);
            cameraPlaceholder.Location = new Point(0, 0);
            cameraPlaceholder.Name = "cameraPlaceholder";
            cameraPlaceholder.Size = new Size(458, 262);
            cameraPlaceholder.TabIndex = 1;
            cameraPlaceholder.Text = "CAMERA HERE!";
            cameraPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // nameArea
            // 
            nameArea.BackColor = Color.White;
            nameArea.Controls.Add(nameCard);
            nameArea.Dock = DockStyle.Fill;
            nameArea.Location = new Point(369, 509);
            nameArea.Margin = new Padding(0);
            nameArea.Name = "nameArea";
            nameArea.Padding = new Padding(12, 10, 20, 18);
            nameArea.Size = new Size(490, 150);
            nameArea.TabIndex = 2;
            // 
            // nameCard
            // 
            nameCard.BackColor = Color.White;
            nameCard.BorderColor = Color.FromArgb(241, 245, 249);
            nameCard.Controls.Add(nameLabel);
            nameCard.Controls.Add(statusLabel);
            nameCard.CornerRadius = 16;
            nameCard.Dock = DockStyle.Fill;
            nameCard.Location = new Point(12, 10);
            nameCard.Name = "nameCard";
            nameCard.Size = new Size(458, 122);
            nameCard.TabIndex = 0;
            // 
            // nameLabel
            // 
            nameLabel.AutoEllipsis = true;
            nameLabel.BackColor = Color.White;
            nameLabel.Dock = DockStyle.Fill;
            nameLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            nameLabel.ForeColor = Color.FromArgb(5, 150, 105);
            nameLabel.Location = new Point(0, 0);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(458, 96);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "WAITING FOR QR CODE...";
            nameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // statusLabel
            // 
            statusLabel.AutoEllipsis = true;
            statusLabel.BackColor = Color.White;
            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            statusLabel.ForeColor = Color.FromArgb(148, 163, 184);
            statusLabel.Location = new Point(0, 96);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(458, 26);
            statusLabel.TabIndex = 1;
            statusLabel.Text = "CAMERA STARTING...";
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // qr_code_scanner
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1180, 780);
            Controls.Add(rootLayout);
            MinimumSize = new Size(900, 760);
            Name = "qr_code_scanner";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCGC - QR CODE SCANNER";
            rootLayout.ResumeLayout(false);
            headerLayout.ResumeLayout(false);
            logoRow.ResumeLayout(false);
            cameraArea.ResumeLayout(false);
            cameraCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cameraPreview).EndInit();
            nameArea.ResumeLayout(false);
            nameCard.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel rootLayout;
        private TableLayoutPanel headerLayout;
        private TableLayoutPanel logoRow;
        private oop_project.TcgcLogoBadge tcgcLogoBadge;
        private oop_project.LrcLogoBadge lrcLogoBadge;
        private Label titleLabel;
        private Panel cameraArea;
        private oop_project.RoundedCard cameraCard;
        private PictureBox cameraPreview;
        private Label cameraPlaceholder;
        private Panel nameArea;
        private oop_project.RoundedCard nameCard;
        private Label nameLabel;
        private Label statusLabel;
    }
}
