namespace oop_project
{
    partial class BookDescriptionDialogBase
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
            _heading = new Label();
            _coverFrame = new Panel();
            _cover = new PictureBox();
            _titleLabel = new Label();
            _authorLabel = new Label();
            _factsPanel = new TableLayoutPanel();
            _publishedCaption = new Label();
            _publishedValue = new Label();
            _copiesCaption = new Label();
            _copiesValue = new Label();
            _availableCaption = new Label();
            _availableValue = new Label();
            _dateCaption = new Label();
            _dateValue = new Label();
            _statusCaption = new Label();
            _statusValue = new Label();
            _descriptionLabel = new Label();
            _closeButton = new RoundedButton();
            _coverFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_cover).BeginInit();
            _factsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _heading
            // 
            _heading.AutoSize = true;
            _heading.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _heading.ForeColor = Color.FromArgb(4, 120, 87);
            _heading.Location = new Point(32, 24);
            _heading.Name = "_heading";
            _heading.TabIndex = 0;
            _heading.Text = "STUDENT PORTAL  •  BOOK DESCRIPTION";
            // 
            // _coverFrame
            // 
            _coverFrame.BackColor = Color.FromArgb(248, 250, 252);
            _coverFrame.Controls.Add(_cover);
            _coverFrame.Location = new Point(32, 58);
            _coverFrame.Name = "_coverFrame";
            _coverFrame.Size = new Size(210, 300);
            _coverFrame.TabIndex = 1;
            // 
            // _cover
            // 
            _cover.BackColor = Color.White;
            _cover.Dock = DockStyle.Fill;
            _cover.Name = "_cover";
            _cover.Padding = new Padding(8);
            _cover.SizeMode = PictureBoxSizeMode.Zoom;
            _cover.TabIndex = 0;
            // 
            // _titleLabel
            // 
            _titleLabel.AutoEllipsis = true;
            _titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            _titleLabel.Location = new Point(268, 58);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(456, 62);
            _titleLabel.TabIndex = 2;
            _titleLabel.Text = "SAMPLE BOOK TITLE";
            // 
            // _authorLabel
            // 
            _authorLabel.AutoEllipsis = true;
            _authorLabel.Font = new Font("Segoe UI", 10F);
            _authorLabel.ForeColor = Color.FromArgb(71, 85, 105);
            _authorLabel.Location = new Point(270, 124);
            _authorLabel.Name = "_authorLabel";
            _authorLabel.Size = new Size(454, 24);
            _authorLabel.TabIndex = 3;
            _authorLabel.Text = "BY SAMPLE AUTHOR";
            // 
            // _factsPanel
            // 
            _factsPanel.ColumnCount = 2;
            _factsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            _factsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _factsPanel.Controls.Add(_publishedCaption, 0, 0);
            _factsPanel.Controls.Add(_publishedValue, 1, 0);
            _factsPanel.Controls.Add(_copiesCaption, 0, 1);
            _factsPanel.Controls.Add(_copiesValue, 1, 1);
            _factsPanel.Controls.Add(_availableCaption, 0, 2);
            _factsPanel.Controls.Add(_availableValue, 1, 2);
            _factsPanel.Controls.Add(_dateCaption, 0, 3);
            _factsPanel.Controls.Add(_dateValue, 1, 3);
            _factsPanel.Controls.Add(_statusCaption, 0, 4);
            _factsPanel.Controls.Add(_statusValue, 1, 4);
            _factsPanel.Location = new Point(268, 162);
            _factsPanel.Name = "_factsPanel";
            _factsPanel.RowCount = 5;
            _factsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _factsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _factsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _factsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _factsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _factsPanel.Size = new Size(456, 148);
            _factsPanel.TabIndex = 4;
            // 
            // _publishedCaption
            // 
            _publishedCaption.Dock = DockStyle.Fill;
            _publishedCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _publishedCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _publishedCaption.Name = "_publishedCaption";
            _publishedCaption.TabIndex = 0;
            _publishedCaption.Text = "PUBLISHED";
            _publishedCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _publishedValue
            // 
            _publishedValue.AutoEllipsis = true;
            _publishedValue.Dock = DockStyle.Fill;
            _publishedValue.Font = new Font("Segoe UI", 9F);
            _publishedValue.ForeColor = Color.FromArgb(15, 23, 42);
            _publishedValue.Name = "_publishedValue";
            _publishedValue.TabIndex = 1;
            _publishedValue.Text = "—";
            _publishedValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _copiesCaption
            // 
            _copiesCaption.Dock = DockStyle.Fill;
            _copiesCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _copiesCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _copiesCaption.Name = "_copiesCaption";
            _copiesCaption.TabIndex = 2;
            _copiesCaption.Text = "COPIES OWNED";
            _copiesCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _copiesValue
            // 
            _copiesValue.AutoEllipsis = true;
            _copiesValue.Dock = DockStyle.Fill;
            _copiesValue.Font = new Font("Segoe UI", 9F);
            _copiesValue.ForeColor = Color.FromArgb(15, 23, 42);
            _copiesValue.Name = "_copiesValue";
            _copiesValue.TabIndex = 3;
            _copiesValue.Text = "—";
            _copiesValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _availableCaption
            // 
            _availableCaption.Dock = DockStyle.Fill;
            _availableCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _availableCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _availableCaption.Name = "_availableCaption";
            _availableCaption.TabIndex = 4;
            _availableCaption.Text = "COPIES AVAILABLE";
            _availableCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _availableValue
            // 
            _availableValue.AutoEllipsis = true;
            _availableValue.Dock = DockStyle.Fill;
            _availableValue.Font = new Font("Segoe UI", 9F);
            _availableValue.ForeColor = Color.FromArgb(15, 23, 42);
            _availableValue.Name = "_availableValue";
            _availableValue.TabIndex = 5;
            _availableValue.Text = "—";
            _availableValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _dateCaption
            // 
            _dateCaption.Dock = DockStyle.Fill;
            _dateCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _dateCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _dateCaption.Name = "_dateCaption";
            _dateCaption.TabIndex = 6;
            _dateCaption.Text = "DATE ADDED";
            _dateCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _dateValue
            // 
            _dateValue.AutoEllipsis = true;
            _dateValue.Dock = DockStyle.Fill;
            _dateValue.Font = new Font("Segoe UI", 9F);
            _dateValue.ForeColor = Color.FromArgb(15, 23, 42);
            _dateValue.Name = "_dateValue";
            _dateValue.TabIndex = 7;
            _dateValue.Text = "—";
            _dateValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statusCaption
            // 
            _statusCaption.Dock = DockStyle.Fill;
            _statusCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            _statusCaption.ForeColor = Color.FromArgb(100, 116, 139);
            _statusCaption.Name = "_statusCaption";
            _statusCaption.TabIndex = 8;
            _statusCaption.Text = "CALL STATUS";
            _statusCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statusValue
            // 
            _statusValue.AutoEllipsis = true;
            _statusValue.Dock = DockStyle.Fill;
            _statusValue.Font = new Font("Segoe UI", 9F);
            _statusValue.ForeColor = Color.FromArgb(15, 23, 42);
            _statusValue.Name = "_statusValue";
            _statusValue.TabIndex = 9;
            _statusValue.Text = "—";
            _statusValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _descriptionLabel
            // 
            _descriptionLabel.Font = new Font("Segoe UI", 9F);
            _descriptionLabel.ForeColor = Color.FromArgb(100, 116, 139);
            _descriptionLabel.Location = new Point(268, 322);
            _descriptionLabel.Name = "_descriptionLabel";
            _descriptionLabel.Size = new Size(456, 72);
            _descriptionLabel.TabIndex = 5;
            _descriptionLabel.Text = "THIS TITLE IS PART OF THE TCGC LEARNING RESOURCE CENTER COLLECTION.";
            // 
            // _closeButton
            // 
            _closeButton.CornerRadius = 10;
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.FlatStyle = FlatStyle.Flat;
            _closeButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _closeButton.ForeColor = Color.White;
            _closeButton.HoverColor = Color.FromArgb(21, 128, 61);
            _closeButton.Location = new Point(568, 408);
            _closeButton.Name = "_closeButton";
            _closeButton.NormalColor = Color.FromArgb(22, 163, 74);
            _closeButton.PressedColor = Color.FromArgb(20, 83, 45);
            _closeButton.Size = new Size(160, 40);
            _closeButton.TabIndex = 6;
            _closeButton.Text = "CLOSE";
            _closeButton.UseVisualStyleBackColor = false;
            // 
            // BookDescriptionDialogBase
            // 
            AcceptButton = _closeButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = _closeButton;
            ClientSize = new Size(760, 470);
            Controls.Add(_closeButton);
            Controls.Add(_descriptionLabel);
            Controls.Add(_factsPanel);
            Controls.Add(_authorLabel);
            Controls.Add(_titleLabel);
            Controls.Add(_coverFrame);
            Controls.Add(_heading);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BookDescriptionDialogBase";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "BOOK DESCRIPTION";
            _coverFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_cover).EndInit();
            _factsPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label _heading;
        private Panel _coverFrame;
        private PictureBox _cover;
        private Label _titleLabel;
        private Label _authorLabel;
        private TableLayoutPanel _factsPanel;
        private Label _publishedCaption;
        private Label _publishedValue;
        private Label _copiesCaption;
        private Label _copiesValue;
        private Label _availableCaption;
        private Label _availableValue;
        private Label _dateCaption;
        private Label _dateValue;
        private Label _statusCaption;
        private Label _statusValue;
        private Label _descriptionLabel;
        private RoundedButton _closeButton;
    }
}
