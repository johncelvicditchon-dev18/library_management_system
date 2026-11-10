namespace oop_project.forms
{
    partial class EvaluationRespondentsDialog
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
            components = new System.ComponentModel.Container();

            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle alternateStyle = new DataGridViewCellStyle();

            titleLabel = new Label();
            periodLabel = new Label();
            closeButton = new DialogCloseButton();
            searchBox = new SearchBox();
            gridFrame = new Panel();
            grid = new DataGridView();
            respondentNameColumn = new DataGridViewTextBoxColumn();
            instituteColumn = new DataGridViewTextBoxColumn();
            programColumn = new DataGridViewTextBoxColumn();
            roleColumn = new DataGridViewTextBoxColumn();
            studentIdColumn = new DataGridViewTextBoxColumn();
            emptyLabel = new Label();
            divider = new Panel();
            countLabel = new Label();
            exportButton = new RoundedButton();
            doneButton = new RoundedButton();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = false;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(15, 23, 42);
            titleLabel.Location = new Point(32, 26);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(700, 30);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "EVALUATION RESPONDENTS";
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // periodLabel
            // 
            periodLabel.AutoSize = false;
            periodLabel.BackColor = Color.Transparent;
            periodLabel.Font = new Font("Segoe UI", 8.75F);
            periodLabel.ForeColor = Color.FromArgb(100, 116, 139);
            periodLabel.Location = new Point(34, 58);
            periodLabel.Name = "periodLabel";
            periodLabel.Size = new Size(760, 20);
            periodLabel.TabIndex = 1;
            periodLabel.Text = "";
            periodLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // closeButton
            // 
            closeButton.Location = new Point(1054, 18);
            closeButton.Name = "closeButton";
            closeButton.OnLightBackground = true;
            closeButton.Size = new Size(34, 34);
            closeButton.TabIndex = 2;
            // 
            // searchBox
            // 
            searchBox.BackColor = Color.White;
            searchBox.Location = new Point(32, 86);
            searchBox.Name = "searchBox";
            searchBox.PlaceholderText = "SEARCH ANY COLUMN";
            searchBox.Size = new Size(380, 44);
            searchBox.TabIndex = 3;
            // 
            // gridFrame
            // 
            gridFrame.BackColor = Color.FromArgb(226, 232, 240);
            gridFrame.Controls.Add(emptyLabel);
            gridFrame.Controls.Add(grid);
            gridFrame.Location = new Point(32, 142);
            gridFrame.Name = "gridFrame";
            gridFrame.Padding = new Padding(1);
            gridFrame.Size = new Size(1056, 390);
            gridFrame.TabIndex = 4;
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = Color.FromArgb(6, 78, 59);
            headerStyle.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold);
            headerStyle.ForeColor = Color.White;
            headerStyle.Padding = new Padding(10, 0, 0, 0);
            headerStyle.SelectionBackColor = Color.FromArgb(6, 78, 59);
            headerStyle.SelectionForeColor = Color.White;
            headerStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersDefaultCellStyle = headerStyle;
            grid.ColumnHeadersHeight = 44;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Columns.AddRange(new DataGridViewColumn[] { respondentNameColumn, instituteColumn, programColumn, roleColumn, studentIdColumn });
            cellStyle.BackColor = Color.White;
            cellStyle.Font = new Font("Segoe UI", 9.25F);
            cellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            cellStyle.Padding = new Padding(10, 0, 0, 0);
            cellStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            cellStyle.SelectionForeColor = Color.FromArgb(6, 78, 59);
            grid.DefaultCellStyle = cellStyle;
            alternateStyle.BackColor = Color.FromArgb(248, 250, 252);
            alternateStyle.SelectionBackColor = Color.FromArgb(220, 252, 231);
            grid.AlternatingRowsDefaultCellStyle = alternateStyle;
            grid.Dock = DockStyle.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(241, 245, 249);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 36;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.ShowCellToolTips = true;
            grid.TabIndex = 0;
            grid.TabStop = false;
            // 
            // respondentNameColumn
            // 
            respondentNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            respondentNameColumn.FillWeight = 30F;
            respondentNameColumn.HeaderText = "RESPONDENT NAME";
            respondentNameColumn.MinimumWidth = 180;
            respondentNameColumn.Name = "respondentNameColumn";
            respondentNameColumn.ReadOnly = true;
            respondentNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // instituteColumn
            // 
            instituteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            instituteColumn.FillWeight = 28F;
            instituteColumn.HeaderText = "INSTITUTE";
            instituteColumn.MinimumWidth = 190;
            instituteColumn.Name = "instituteColumn";
            instituteColumn.ReadOnly = true;
            instituteColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // programColumn
            // 
            programColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            programColumn.FillWeight = 26F;
            programColumn.HeaderText = "PROGRAM";
            programColumn.MinimumWidth = 160;
            programColumn.Name = "programColumn";
            programColumn.ReadOnly = true;
            programColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // roleColumn
            // 
            roleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            roleColumn.FillWeight = 10F;
            roleColumn.HeaderText = "ROLE";
            roleColumn.MinimumWidth = 90;
            roleColumn.Name = "roleColumn";
            roleColumn.ReadOnly = true;
            roleColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // studentIdColumn
            // 
            studentIdColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            studentIdColumn.FillWeight = 14F;
            studentIdColumn.HeaderText = "STUDENT ID";
            studentIdColumn.MinimumWidth = 110;
            studentIdColumn.Name = "studentIdColumn";
            studentIdColumn.ReadOnly = true;
            studentIdColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // emptyLabel
            // 
            emptyLabel.AutoSize = false;
            emptyLabel.BackColor = Color.White;
            emptyLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            emptyLabel.ForeColor = Color.FromArgb(100, 116, 139);
            emptyLabel.Location = new Point(1, 45);
            emptyLabel.Name = "emptyLabel";
            emptyLabel.Size = new Size(1054, 344);
            emptyLabel.TabIndex = 1;
            emptyLabel.Text = "NO RESPONDENTS YET FOR THIS SEMESTER.";
            emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            emptyLabel.Visible = false;
            // 
            // divider
            // 
            divider.BackColor = Color.FromArgb(226, 232, 240);
            divider.Location = new Point(0, 548);
            divider.Name = "divider";
            divider.Size = new Size(1120, 1);
            divider.TabIndex = 5;
            // 
            // countLabel
            // 
            countLabel.AutoSize = false;
            countLabel.BackColor = Color.Transparent;
            countLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            countLabel.ForeColor = Color.FromArgb(100, 116, 139);
            countLabel.Location = new Point(34, 564);
            countLabel.Name = "countLabel";
            countLabel.Size = new Size(420, 22);
            countLabel.TabIndex = 6;
            countLabel.Text = "";
            countLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // exportButton
            // 
            exportButton.BackColor = Color.Transparent;
            exportButton.CornerRadius = 11;
            exportButton.FlatAppearance.BorderSize = 0;
            exportButton.FlatStyle = FlatStyle.Flat;
            exportButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            exportButton.ForeColor = Color.White;
            exportButton.HoverColor = Color.FromArgb(154, 52, 18);
            exportButton.Location = new Point(786, 562);
            exportButton.Name = "exportButton";
            exportButton.NormalColor = Color.FromArgb(194, 65, 12);
            exportButton.PressedColor = Color.FromArgb(120, 45, 15);
            exportButton.Size = new Size(150, 44);
            exportButton.TabIndex = 7;
            exportButton.Text = "EXPORT PDF";
            exportButton.UseVisualStyleBackColor = false;
            // 
            // doneButton
            // 
            doneButton.BackColor = Color.Transparent;
            doneButton.CornerRadius = 11;
            doneButton.FlatAppearance.BorderSize = 0;
            doneButton.FlatStyle = FlatStyle.Flat;
            doneButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            doneButton.ForeColor = Color.White;
            doneButton.HoverColor = Color.FromArgb(21, 128, 61);
            doneButton.Location = new Point(948, 562);
            doneButton.Name = "doneButton";
            doneButton.NormalColor = Color.FromArgb(22, 163, 74);
            doneButton.PressedColor = Color.FromArgb(20, 83, 45);
            doneButton.Size = new Size(140, 44);
            doneButton.TabIndex = 8;
            doneButton.Text = "CLOSE";
            doneButton.UseVisualStyleBackColor = false;
            // 
            // EvaluationRespondentsDialog
            // 
            AcceptButton = doneButton;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = doneButton;
            ClientSize = new Size(1120, 620);
            Controls.Add(titleLabel);
            Controls.Add(periodLabel);
            Controls.Add(closeButton);
            Controls.Add(searchBox);
            Controls.Add(gridFrame);
            Controls.Add(divider);
            Controls.Add(countLabel);
            Controls.Add(exportButton);
            Controls.Add(doneButton);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Name = "EvaluationRespondentsDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "EVALUATION RESPONDENTS";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label periodLabel;
        private DialogCloseButton closeButton;
        private SearchBox searchBox;
        private Panel gridFrame;
        private DataGridView grid;
        private DataGridViewTextBoxColumn respondentNameColumn;
        private DataGridViewTextBoxColumn instituteColumn;
        private DataGridViewTextBoxColumn programColumn;
        private DataGridViewTextBoxColumn roleColumn;
        private DataGridViewTextBoxColumn studentIdColumn;
        private Label emptyLabel;
        private Panel divider;
        private Label countLabel;
        private RoundedButton exportButton;
        private RoundedButton doneButton;
    }
}
