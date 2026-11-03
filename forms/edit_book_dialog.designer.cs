namespace oop_project.forms
{
    partial class EditBookDialog
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
            _contentPanel = new Panel();
            _footerDivider = new Panel();
            _titleLabel = new Label();
            _subtitleLabel = new Label();
            _closeButton = new DialogCloseButton();
            _primaryButton = new RoundedButton();
            _secondaryButton = new RoundedButton();
            _titleField = new DialogField();
            _authorField = new DialogField();
            _yearField = new DialogField();
            _quantityField = new DialogField();
            _coverPicker = new BookCoverPicker();
            _hintLabel = new Label();
            _contentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _contentPanel
            // 
            _contentPanel.BackColor = Color.White;
            _contentPanel.Location = new Point(0, 104);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Size = new Size(640, 598);
            _contentPanel.TabIndex = 3;
            // 
            // _footerDivider
            // 
            _footerDivider.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _footerDivider.BackColor = Color.FromArgb(226, 232, 240);
            _footerDivider.Location = new Point(0, 702);
            _footerDivider.Name = "_footerDivider";
            _footerDivider.Size = new Size(640, 1);
            _footerDivider.TabIndex = 4;
            // 
            // _titleLabel
            // 
            _titleLabel.AutoSize = false;
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            _titleLabel.ForeColor = Color.White;
            _titleLabel.Location = new Point(32, 22);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(510, 32);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "EDIT BOOK";
            _titleLabel.MouseDown += DragStart;
            _titleLabel.MouseMove += DragMove;
            _titleLabel.MouseUp += DragEnd;
            // 
            // _subtitleLabel
            // 
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.BackColor = Color.Transparent;
            _subtitleLabel.Font = new Font("Segoe UI", 8.75F);
            _subtitleLabel.ForeColor = Color.FromArgb(209, 250, 229);
            _subtitleLabel.Location = new Point(34, 60);
            _subtitleLabel.Name = "_subtitleLabel";
            _subtitleLabel.Size = new Size(500, 22);
            _subtitleLabel.TabIndex = 1;
            _subtitleLabel.Text = "Change the row and its picture, then press UPDATE BOOK.";
            _subtitleLabel.MouseDown += DragStart;
            _subtitleLabel.MouseMove += DragMove;
            _subtitleLabel.MouseUp += DragEnd;
            // 
            // _closeButton
            // 
            _closeButton.Location = new Point(574, 20);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(34, 34);
            _closeButton.TabIndex = 2;
            _closeButton.Click += CloseButton_Click;
            // 
            // _primaryButton
            // 
            _primaryButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _primaryButton.CornerRadius = 12;
            _primaryButton.FlatAppearance.BorderSize = 0;
            _primaryButton.FlatStyle = FlatStyle.Flat;
            _primaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _primaryButton.ForeColor = Color.White;
            _primaryButton.HoverColor = Color.FromArgb(21, 128, 61);
            _primaryButton.Location = new Point(440, 718);
            _primaryButton.Name = "_primaryButton";
            _primaryButton.NormalColor = Color.FromArgb(22, 163, 74);
            _primaryButton.PressedColor = Color.FromArgb(20, 83, 45);
            _primaryButton.Size = new Size(168, 46);
            _primaryButton.TabIndex = 5;
            _primaryButton.Text = "UPDATE BOOK";
            _primaryButton.UseVisualStyleBackColor = false;
            // 
            // _secondaryButton
            // 
            _secondaryButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _secondaryButton.CornerRadius = 12;
            _secondaryButton.DialogResult = DialogResult.Cancel;
            _secondaryButton.FlatAppearance.BorderSize = 0;
            _secondaryButton.FlatStyle = FlatStyle.Flat;
            _secondaryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _secondaryButton.ForeColor = Color.FromArgb(51, 65, 85);
            _secondaryButton.HoverColor = Color.FromArgb(226, 232, 240);
            _secondaryButton.Location = new Point(308, 718);
            _secondaryButton.Name = "_secondaryButton";
            _secondaryButton.NormalColor = Color.FromArgb(241, 245, 249);
            _secondaryButton.PressedColor = Color.FromArgb(203, 213, 225);
            _secondaryButton.Size = new Size(120, 46);
            _secondaryButton.TabIndex = 6;
            _secondaryButton.Text = "CANCEL";
            _secondaryButton.UseVisualStyleBackColor = false;
            // 
            // _titleField
            // 
            _titleField.Caption = "BOOK TITLE";
            _titleField.Kind = DialogFieldKind.Text;
            _titleField.Location = new Point(32, 16);
            _titleField.Name = "_titleField";
            _titleField.Size = new Size(576, 88);
            _titleField.TabIndex = 0;
            _titleField.Width = 576;
            // 
            // _authorField
            // 
            _authorField.Caption = "AUTHOR";
            _authorField.Kind = DialogFieldKind.Text;
            _authorField.Location = new Point(32, 104);
            _authorField.Name = "_authorField";
            _authorField.Size = new Size(576, 88);
            _authorField.TabIndex = 1;
            _authorField.Width = 576;
            // 
            // _yearField
            // 
            _yearField.Caption = "YEAR PUBLISHED";
            _yearField.Kind = DialogFieldKind.Text;
            _yearField.Input.MaxLength = 4;
            _yearField.Location = new Point(32, 192);
            _yearField.Name = "_yearField";
            _yearField.Size = new Size(576, 88);
            _yearField.TabIndex = 2;
            _yearField.Width = 576;
            // 
            // _quantityField
            // 
            _quantityField.Caption = "QUANTITY (COPIES)";
            _quantityField.Kind = DialogFieldKind.Text;
            _quantityField.Input.MaxLength = 4;
            _quantityField.Location = new Point(32, 280);
            _quantityField.Name = "_quantityField";
            _quantityField.Size = new Size(576, 88);
            _quantityField.TabIndex = 3;
            _quantityField.Width = 576;
            // 
            // _coverPicker
            // 
            _coverPicker.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _coverPicker.Location = new Point(32, 372);
            _coverPicker.Name = "_coverPicker";
            _coverPicker.Size = new Size(576, 172);
            _coverPicker.TabIndex = 4;
            _coverPicker.Width = 576;
            // 
            // _hintLabel
            // 
            _hintLabel.AutoSize = false;
            _hintLabel.BackColor = Color.Transparent;
            _hintLabel.Font = new Font("Segoe UI", 8.25F);
            _hintLabel.ForeColor = Color.FromArgb(148, 163, 184);
            _hintLabel.Location = new Point(34, 552);
            _hintLabel.Name = "_hintLabel";
            _hintLabel.Size = new Size(576, 40);
            _hintLabel.TabIndex = 5;
            _hintLabel.Text = "BOOK ID #—     •     DATE ADDED: —\r\nTHE ID AND THE DATE ADDED NEVER CHANGE.";
            // 
            // _contentPanel
            // 
            _contentPanel.Controls.Add(_hintLabel);
            _contentPanel.Controls.Add(_coverPicker);
            _contentPanel.Controls.Add(_titleField);
            _contentPanel.Controls.Add(_authorField);
            _contentPanel.Controls.Add(_yearField);
            _contentPanel.Controls.Add(_quantityField);
            // 
            // EditBookDialog
            // 
            AcceptButton = _primaryButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = _secondaryButton;
            ClientSize = new Size(640, 780);
            Controls.Add(_contentPanel);
            Controls.Add(_footerDivider);
            Controls.Add(_titleLabel);
            Controls.Add(_subtitleLabel);
            Controls.Add(_closeButton);
            Controls.Add(_primaryButton);
            Controls.Add(_secondaryButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditBookDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "EDIT BOOK";
            MouseDown += DragStart;
            MouseMove += DragMove;
            MouseUp += DragEnd;
            _contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel _contentPanel;
        private Panel _footerDivider;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private DialogCloseButton _closeButton;
        private RoundedButton _primaryButton;
        private RoundedButton _secondaryButton;
        private DialogField _titleField;
        private DialogField _authorField;
        private DialogField _yearField;
        private DialogField _quantityField;
        private BookCoverPicker _coverPicker;
        private Label _hintLabel;
    }
}
