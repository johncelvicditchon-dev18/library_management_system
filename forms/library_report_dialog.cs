using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using oop_project.Models;

namespace oop_project.forms
{
    /// <summary>
    /// Pop up form opened by GENERATE REPORT. It previews the report, shows the
    /// headline numbers and lets the staff save the document as a text file.
    /// </summary>
    public sealed class LibraryReportDialog : LibraryDialog
    {
        private readonly string _reportText;

        public LibraryReportDialog(LibraryCatalog catalog, string generatedBy)
            : base("GENERATE REPORT", "Summary of the catalog, the loans and the returns.", 900, 660)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _reportText = catalog.BuildReportText(generatedBy, DateTime.Now);

            int width = ContentWidth;
            int cardWidth = (width - 30) / 4;

            ContentPanel.Controls.Add(CreateSummaryCard("TOTAL TITLES", catalog.TotalBooks, 32, Color.FromArgb(6, 78, 59)));
            ContentPanel.Controls.Add(CreateSummaryCard("AVAILABLE", catalog.AvailableBooks, 32 + cardWidth + 10, Color.FromArgb(22, 163, 74)));
            ContentPanel.Controls.Add(CreateSummaryCard("BORROWED", catalog.BorrowedBooks, 32 + (cardWidth + 10) * 2, Color.FromArgb(194, 65, 12)));
            ContentPanel.Controls.Add(CreateSummaryCard("RETURNED", catalog.ReturnedLoans, 32 + (cardWidth + 10) * 3, Color.FromArgb(100, 116, 139)));

            Label previewCaption = new Label();
            previewCaption.AutoSize = false;
            previewCaption.BackColor = Color.Transparent;
            previewCaption.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            previewCaption.ForeColor = Color.FromArgb(100, 116, 139);
            previewCaption.Location = new Point(34, 96);
            previewCaption.Size = new Size(width, 18);
            previewCaption.Text = "REPORT PREVIEW";

            RoundedCard previewCard = new RoundedCard();
            previewCard.BackColor = Color.FromArgb(15, 23, 42);
            previewCard.BorderColor = Color.FromArgb(30, 41, 59);
            previewCard.CornerRadius = 14;
            previewCard.Location = new Point(32, 118);
            previewCard.Size = new Size(width, 340);

            TextBox preview = new TextBox();
            preview.BackColor = Color.FromArgb(15, 23, 42);
            preview.BorderStyle = BorderStyle.None;
            preview.Font = new Font("Consolas", 8.75F);
            preview.ForeColor = Color.FromArgb(209, 250, 229);
            preview.Location = new Point(16, 14);
            preview.Multiline = true;
            preview.ReadOnly = true;
            preview.ScrollBars = ScrollBars.Vertical;
            preview.Size = new Size(width - 32, 312);
            preview.TabStop = false;
            preview.Text = _reportText;
            preview.WordWrap = true;
            preview.SelectionStart = 0;
            preview.SelectionLength = 0;

            previewCard.Controls.Add(preview);
            ContentPanel.Controls.Add(previewCaption);
            ContentPanel.Controls.Add(previewCard);

            PrimaryButton.Text = "SAVE REPORT";
            PrimaryButton.Click += SaveReport_Click;
            SecondaryButton.Text = "CLOSE";
        }

        private static RoundedCard CreateSummaryCard(string caption, int value, int x, Color valueColor)
        {
            const int CardWidth = 201;

            RoundedCard card = new RoundedCard();
            card.BackColor = Color.FromArgb(248, 250, 252);
            card.BorderColor = Color.FromArgb(226, 232, 240);
            card.CornerRadius = 14;
            card.Location = new Point(x, 14);
            card.Size = new Size(CardWidth, 74);

            Label valueLabel = new Label();
            valueLabel.AutoSize = false;
            valueLabel.BackColor = Color.Transparent;
            valueLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            valueLabel.ForeColor = valueColor;
            valueLabel.Location = new Point(18, 8);
            valueLabel.Size = new Size(CardWidth - 36, 34);
            valueLabel.Text = value.ToString();
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;

            Label captionLabel = new Label();
            captionLabel.AutoSize = false;
            captionLabel.BackColor = Color.Transparent;
            captionLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            captionLabel.ForeColor = Color.FromArgb(100, 116, 139);
            captionLabel.Location = new Point(20, 44);
            captionLabel.Size = new Size(CardWidth - 40, 18);
            captionLabel.Text = caption;
            captionLabel.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(captionLabel);
            card.Controls.Add(valueLabel);

            return card;
        }

        private void SaveReport_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "library-report-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                dialog.Title = "SAVE LIBRARY REPORT";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, _reportText);
                    ShowAlert("REPORT SAVED TO:\n" + dialog.FileName, "REPORT SAVED", MessageBoxIcon.Information);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    ShowAlert("THE REPORT COULD NOT BE SAVED.\n" + ex.Message, "REPORT NOT SAVED", MessageBoxIcon.Error);
                }
            }
        }
    }
}
