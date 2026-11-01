namespace oop_project.forms
{
    partial class EditBookDialogBase
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
            _titleField = DialogField.ForText("BOOK TITLE");
            _authorField = DialogField.ForText("AUTHOR");
            _yearField = DialogField.ForText("YEAR PUBLISHED");
            _quantityField = DialogField.ForText("QUANTITY (COPIES)");
            _coverPicker = new BookCoverPicker();
            _hintLabel = new Label();
            ContentPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _titleField
            // 
            _titleField.Location = new Point(32, 16);
            _titleField.Name = "_titleField";
            _titleField.Size = new Size(576, 88);
            _titleField.TabIndex = 0;
            _titleField.Width = 576;
            // 
            // _authorField
            // 
            _authorField.Location = new Point(32, 104);
            _authorField.Name = "_authorField";
            _authorField.Size = new Size(576, 88);
            _authorField.TabIndex = 1;
            _authorField.Width = 576;
            // 
            // _yearField
            // 
            _yearField.Input.MaxLength = 4;
            _yearField.Location = new Point(32, 192);
            _yearField.Name = "_yearField";
            _yearField.Size = new Size(576, 88);
            _yearField.TabIndex = 2;
            _yearField.Width = 576;
            // 
            // _quantityField
            // 
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
            // ContentPanel
            // 
            ContentPanel.Controls.Add(_hintLabel);
            ContentPanel.Controls.Add(_coverPicker);
            ContentPanel.Controls.Add(_titleField);
            ContentPanel.Controls.Add(_authorField);
            ContentPanel.Controls.Add(_yearField);
            ContentPanel.Controls.Add(_quantityField);
            ContentPanel.ResumeLayout(false);
            // 
            // _primaryButton
            // 
            PrimaryButton.Text = "UPDATE BOOK";
            // 
            // EditBookDialogBase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "EditBookDialogBase";
            ResumeLayout(false);
        }

        #endregion

        private DialogField _titleField;
        private DialogField _authorField;
        private DialogField _yearField;
        private DialogField _quantityField;
        private BookCoverPicker _coverPicker;
        private Label _hintLabel;
    }
}
