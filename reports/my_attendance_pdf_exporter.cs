using System;
using System.Collections.Generic;
using System.Globalization;
using oop_project.Models;

namespace oop_project.Reports
{
    /// <summary>
    /// The MY ATTENDANCE report of the patron portal: the signed in student,
    /// faculty member or staff member's own library visits, with the date and time,
    /// the name, the program or office and the institute. Columns are fitted to the
    /// content so nothing wraps or is cut.
    /// </summary>
    public static class MyAttendancePdfExporter
    {
        private static readonly ReportColumn[] Columns =
        {
            new ReportColumn { Header = "DATE & TIME", Width = 150f, Center = true },
            new ReportColumn { Header = "NAME", Width = 250f, BoldBody = true },
            new ReportColumn { Header = "PROGRAM/OFFICE", Width = 200f },
            new ReportColumn { Header = "INSTITUTE", Width = 200f }
        };

        /// <summary>
        /// Builds the PDF for one patron and saves it at <paramref name="path"/>.
        /// The person's name and role are printed under the report heading.
        /// </summary>
        public static void Export(
            string personName,
            string role,
            IReadOnlyList<AttendanceRow> rows,
            string path)
        {
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

            foreach (AttendanceRow row in ordered)
            {
                data.Add(new[]
                {
                    row.StampText,
                    (row.Name ?? string.Empty).Trim().ToUpperInvariant(),
                    (row.ProgramOrOffice ?? string.Empty).Trim().ToUpperInvariant(),
                    (row.Institute ?? string.Empty).Trim().ToUpperInvariant()
                });
            }

            string subtitle = (personName ?? string.Empty).Trim().ToUpperInvariant();
            string roleText = (role ?? string.Empty).Trim().ToUpperInvariant();

            if (roleText.Length > 0)
            {
                subtitle = subtitle.Length > 0 ? subtitle + "  -  " + roleText : roleText;
            }

            ReportSection section = new ReportSection
            {
                Subtitle = subtitle,
                Rows = data
            };

            ReportPdfWriter.ExportSections(
                path,
                "LIBRARY ATTENDANCE",
                Columns,
                new[] { section },
                9f,
                true);
        }
    }
}
