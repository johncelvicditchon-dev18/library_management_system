namespace oop_project.forms
{
    partial class EvaluationForm
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
            headerPanel = new Panel();
            titleLabel = new Label();
            subtitleLabel = new Label();
            scrollPanel = new Panel();
            headerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(6, 78, 59);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1100, 92);
            headerPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.AutoEllipsis = true;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(32, 16);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(1030, 36);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "LIBRARY RESOURCE CENTER EVALUATION FORM";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // subtitleLabel
            // 
            subtitleLabel.AutoEllipsis = true;
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.Font = new Font("Segoe UI", 9F);
            subtitleLabel.ForeColor = Color.FromArgb(209, 250, 229);
            subtitleLabel.Location = new Point(34, 56);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new Size(1030, 22);
            subtitleLabel.TabIndex = 1;
            subtitleLabel.Text = "RATE EACH STATEMENT FROM 5 (EXCELLENT) TO 1 (POOR). THE FORM SCROLLS TO THE BOTTOM.";
            // 
            // scrollPanel
            // 
            scrollPanel.AutoScroll = true;
            scrollPanel.BackColor = Color.FromArgb(241, 245, 249);
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.Location = new Point(0, 92);
            scrollPanel.Name = "scrollPanel";
            scrollPanel.Padding = new Padding(24, 20, 24, 24);
            scrollPanel.Size = new Size(1100, 728);
            scrollPanel.TabIndex = 1;
            // 
            // EvaluationForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(241, 245, 249);
            ClientSize = new Size(1100, 820);
            Controls.Add(scrollPanel);
            Controls.Add(headerPanel);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(900, 620);
            Name = "EvaluationForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "LIBRARY RESOURCE CENTER EVALUATION FORM";
            headerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel headerPanel;
        private Label titleLabel;
        private Label subtitleLabel;
        private Panel scrollPanel;
    }
}
