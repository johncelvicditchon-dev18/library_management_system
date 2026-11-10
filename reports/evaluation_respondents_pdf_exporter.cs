using System;
using System.Collections.Generic;
using oop_project.Models;

namespace oop_project.Reports
{
    /// <summary>
    /// The EVALUATION RESPONDENTS report: the people who answered one semester,
    /// with the respondent name, institute, program, role and student id. Faculty
    /// have no program or student id and staff have neither, so those cells are
    /// deliberately left blank instead of printing a placeholder.
    /// </summary>
    public static class EvaluationRespondentsPdfExporter
    {
        private static readonly ReportColumn[] Columns =
        {
            new ReportColumn { Header = "RESPONDENT NAME", Width = 240f, BoldBody = true },
            new ReportColumn { Header = "INSTITUTE", Width = 220f },
            new ReportColumn { Header = "PROGRAM", Width = 200f },
            new ReportColumn { Header = "ROLE", Width = 80f, Center = true },
            new ReportColumn { Header = "STUDENT ID", Width = 120f, Center = true }
        };

        /// <summary>
        /// Builds the PDF for the supplied respondents and saves it at
        /// <paramref name="path"/>. The semester title is printed under the heading.
        /// </summary>
        public static void Export(
            string periodTitle,
            IReadOnlyList<EvaluationRespondent> respondents,
            string path)
        {
            List<string[]> rows = new List<string[]>();

            if (respondents != null)
            {
                foreach (EvaluationRespondent person in respondents)
                {
                    if (person == null)
                    {
                        continue;
                    }

                    rows.Add(new[]
                    {
                        (person.Name ?? string.Empty).Trim().ToUpperInvariant(),
                        (person.Institute ?? string.Empty).Trim().ToUpperInvariant(),
                        (person.Program ?? string.Empty).Trim().ToUpperInvariant(),
                        (person.Role ?? string.Empty).Trim().ToUpperInvariant(),
                        (person.StudentId ?? string.Empty).Trim()
                    });
                }
            }

            ReportSection section = new ReportSection
            {
                Subtitle = (periodTitle ?? string.Empty).Trim(),
                Rows = rows
            };

            ReportPdfWriter.ExportSections(
                path,
                "EVALUATION RESPONDENTS",
                Columns,
                new[] { section },
                9f,
                true);
        }
    }
}
