using System;
using System.Collections.Generic;
using System.Globalization;
using oop_project.Models;

namespace oop_project.Reports
{
    /// <summary>
    /// The BORROWED BOOKS report: one row per borrow group (borrower + title + day)
    /// with the program, number of copies taken, the borrow date and the return
    /// date. A copy that is still out shows an em dash in the RETURNED column.
    /// </summary>
    public static class BorrowedBooksPdfExporter
    {
        private static readonly ReportColumn[] Columns =
        {
            new ReportColumn { Header = "NO.", Width = 26f, Center = true },
            new ReportColumn { Header = "BOOK TITLE", Width = 152f, BoldBody = true },
            new ReportColumn { Header = "BORROWER", Width = 126f, BoldBody = true },
            new ReportColumn { Header = "PROGRAM", Width = 160f },
            new ReportColumn { Header = "BORROWED COPY", Width = 88f, Center = true },
            new ReportColumn { Header = "BORROWED DATE", Width = 98f, Center = true },
            new ReportColumn { Header = "RETURNED", Width = 98f, Center = true }
        };

        /// <summary>Builds the PDF for the supplied groups and saves it at <paramref name="path"/>.</summary>
        public static void Export(IReadOnlyList<BorrowedBookGroup> groups, string path)
        {
            List<string[]> rows = new List<string[]>();
            int number = 1;

            if (groups != null)
            {
                foreach (BorrowedBookGroup group in groups)
                {
                    if (group == null)
                    {
                        continue;
                    }

                    rows.Add(new[]
                    {
                        number.ToString(CultureInfo.InvariantCulture),
                        (group.BookTitle ?? string.Empty).Trim().ToUpperInvariant(),
                        (group.BorrowerName ?? string.Empty).Trim().ToUpperInvariant(),
                        ProgramOf(group),
                        group.CopyCount.ToString(CultureInfo.InvariantCulture),
                        group.DateBorrowedText,
                        group.DateReturnedText
                    });

                    number++;
                }
            }

            ReportPdfWriter.Export(path, "BORROWED BOOKS", Columns, rows, 9f, true);
        }

        private static string ProgramOf(BorrowedBookGroup group)
        {
            BorrowRecord? record = group.Records.Count > 0 ? group.Records[0] : null;
            string program = record == null ? string.Empty : (record.BorrowerProgram ?? string.Empty).Trim();

            return program.ToUpperInvariant();
        }
    }
}
