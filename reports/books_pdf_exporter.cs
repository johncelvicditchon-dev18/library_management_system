using System;
using System.Collections.Generic;
using System.Globalization;
using oop_project.Models;

namespace oop_project.Reports
{
    /// <summary>
    /// The BOOKS report of the Learning Resource Center form: one numbered row per
    /// catalog title with its author, year and number of copies.
    /// </summary>
    public static class BooksPdfExporter
    {
        private static readonly ReportColumn[] Columns =
        {
            new ReportColumn { Header = "NO.", Width = 31f, Center = true },
            new ReportColumn { Header = "BOOKS TITLE", Width = 382f, BoldBody = true },
            new ReportColumn { Header = "AUTHORS", Width = 196f, BoldBody = true },
            new ReportColumn { Header = "YEAR", Width = 67f, Center = true },
            new ReportColumn { Header = "COPY", Width = 72f, Center = true }
        };

        /// <summary>Builds the PDF for the supplied books and saves it at <paramref name="path"/>.</summary>
        public static void Export(IReadOnlyList<Book> books, string path)
        {
            List<string[]> rows = new List<string[]>();
            int number = 1;

            if (books != null)
            {
                foreach (Book book in books)
                {
                    if (book == null)
                    {
                        continue;
                    }

                    rows.Add(new[]
                    {
                        number.ToString(CultureInfo.InvariantCulture),
                        (book.Title ?? string.Empty).Trim().ToUpperInvariant(),
                        (book.Author ?? string.Empty).Trim().ToUpperInvariant(),
                        book.Year > 0 ? book.Year.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        book.Quantity > 0 ? book.Quantity.ToString(CultureInfo.InvariantCulture) : string.Empty
                    });

                    number++;
                }
            }

            ReportPdfWriter.Export(path, "BOOKS", Columns, rows, 10f);
        }
    }
}
