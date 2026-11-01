namespace oop_project.forms
{
    partial class PatronDashboardBase
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
        /// Builds the complete patron portal. The builder methods live next to the
        /// behaviour in patron_dashboard_base.cs, and this designer method invokes
        /// them so the Windows Forms designer renders the real layout.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            InitializeUi();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
