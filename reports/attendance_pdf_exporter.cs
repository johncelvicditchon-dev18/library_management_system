using System;
using System.Collections.Generic;
using System.Globalization;
using oop_project.Models;

namespace oop_project.Reports
{
    /// <summary>
    /// The LIBRARY ATTENDANCE report: every QR pass scanned at the library office
    /// entrance, with the date and time, the person, the role, the program or
    /// office and the institute. Columns are fitted to their content, so nobody's
    /// name is cut and nothing is wrapped.
    ///
    /// The log is split by year: the 2026 visits print under "LIBRARY ATTENDANCE
    /// 2026", then the 2027 visits start on a new page under "LIBRARY ATTENDANCE
    /// 2027", and so on.
    /// </summary>
    public static class AttendancePdfExporter
    {
        private static readonly ReportColumn[] Columns =
        {
            new ReportColumn { Header = "DATE & TIME", Width = 120f, Center = true },
            new ReportColumn { Header = "NAME", Width = 190f, BoldBody = true },
            new ReportColumn { Header = "ROLE", Width = 70f, Center = true },
            new ReportColumn { Header = "PROGRAM/OFFICE", Width = 180f },
            new ReportColumn { Header = "INSTITUTE", Width = 188f }
        };

        /// <summary>Builds the PDF for the supplied visits and saves it at <paramref name="path"/>.</summary>
        public static void Export(IReadOnlyList<AttendanceRow> rows, string path)
        {
            // The PDF lists the log from the earliest scan to the latest, even when
            // the grid is showing the newest visit first.
            List<AttendanceRow> ordered = new List<AttendanceRow>();

            if (rows != null)
            {
                foreach (AttendanceRow row in rows)
                {
                    if (row != null)
                    {
                        ordered.Add(row);
                    }
                }
            }

            ordered.Sort(delegate (AttendanceRow left, AttendanceRow right)
            {
                int byTime = left.ScanTime.CompareTo(right.ScanTime);

                return byTime != 0 ? byTime : left.AttendanceId.CompareTo(right.AttendanceId);
            });

            List<string[]> data = new List<string[]>();
            Dictionary<int, List<string[]>> byYear = new Dictionary<int, List<string[]>>();
            List<int> years = new List<int>();

            foreach (AttendanceRow row in ordered)
            {
                int year = row.ScanTime.Year;

                if (!byYear.TryGetValue(year, out List<string[]> yearRows))
                {
                    yearRows = new List<string[]>();
                    byYear.Add(year, yearRows);
                    years.Add(year);
                }

                yearRows.Add(new[]
                {
                    row.StampText,
                    (row.Name ?? string.Empty).Trim().ToUpperInvariant(),
                    (row.Role ?? string.Empty).Trim().ToUpperInvariant(),
                    (row.ProgramOrOffice ?? string.Empty).Trim().ToUpperInvariant(),
                    (row.Institute ?? string.Empty).Trim().ToUpperInvariant()
                });
            }

            years.Sort();

            List<ReportSection> sections = new List<ReportSection>();

            foreach (int year in years)
            {
                sections.Add(new ReportSection
                {
                    Subtitle = year.ToString(CultureInfo.InvariantCulture),
                    Rows = byYear[year]
                });
            }

            if (sections.Count == 0)
            {
                sections.Add(new ReportSection
                {
                    Subtitle = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture),
                    Rows = new List<string[]>()
                });
            }

            ReportPdfWriter.ExportSections(path, "LIBRARY ATTENDANCE", Columns, sections, 9f, true);
        }
    }
}
