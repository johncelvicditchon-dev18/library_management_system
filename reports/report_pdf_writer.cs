using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace oop_project.Reports
{
    /// <summary>One column of a printed report table.</summary>
    internal sealed class ReportColumn
    {
        public string Header { get; set; } = string.Empty;
        public float Width { get; set; }
        public bool Center { get; set; }
        public bool BoldBody { get; set; }
    }

    /// <summary>
    /// One group of report rows printed under its own subtitle, for example one
    /// year of attendance. Each section starts on a new page.
    /// </summary>
    internal sealed class ReportSection
    {
        public string Subtitle { get; set; } = string.Empty;
        public IReadOnlyList<string[]> Rows { get; set; } = new List<string[]>();
    }

    /// <summary>
    /// Shared writer for the Learning Resource Center PDF reports. It draws the two
    /// logos, the "LEARNING RESOURCE CENTER | TCGC LIBRARY" heading, a bordered
    /// table with the columns it is given, and the Director's approval block.
    ///
    /// The PDF is written directly, so the export needs no third party library.
    /// Only the standard Times fonts are used and the logos are embedded as images.
    /// </summary>
    internal static class ReportPdfWriter
    {
        public const float TableWidth = PageWidth - (Margin * 2f);

        private const float PageWidth = 841.89f;
        private const float PageHeight = 595.28f;
        private const float Margin = 47f;

        private const float HeaderTop = 122f;
        private const float HeaderHeight = 20f;
        private const float RowHeight = 18f;
        private const float NextPageTop = 42f;
        private const float BottomLimit = PageHeight - 74f;

        private const float LogoSize = 42f;
        private const float LogoGap = 10f;

        private const string CenterName = "JOSEPH D. ONIOT, RL, MLIS, PhD";
        private const string CenterTitle = "Director, Learning Resource Center";
        private const string ReportHeading = "LEARNING RESOURCE CENTER | TCGC LIBRARY";

        /// <summary>
        /// Writes the report. Every row must have one string per column; an empty
        /// string simply leaves that cell blank.
        /// </summary>
        public static void Export(
            string path,
            string heading,
            IReadOnlyList<ReportColumn> columns,
            IReadOnlyList<string[]> rows,
            float bodyFontSize,
            bool fitToContent = false)
        {
            List<ReportSection> sections = new List<ReportSection>
            {
                new ReportSection { Rows = rows ?? new List<string[]>() }
            };

            ExportSections(path, heading, columns, sections, bodyFontSize, fitToContent);
        }

        /// <summary>
        /// Writes a report made of several sections, each starting on a new page with
        /// its own subtitle under the heading. Used by LIBRARY ATTENDANCE, where every
        /// year in the log gets its own page or pages.
        /// </summary>
        public static void ExportSections(
            string path,
            string heading,
            IReadOnlyList<ReportColumn> columns,
            IReadOnlyList<ReportSection> sections,
            float bodyFontSize,
            bool fitToContent = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A file path is required.", nameof(path));
            }

            if (columns == null || columns.Count == 0)
            {
                throw new ArgumentException("The report needs at least one column.", nameof(columns));
            }

            if (sections == null || sections.Count == 0)
            {
                sections = new List<ReportSection>
                {
                    new ReportSection { Rows = new List<string[]>() }
                };
            }

            // Column widths are chosen from every section, so the table keeps the
            // same shape on the 2026 page and the 2027 page.
            List<string[]> allRows = new List<string[]>();

            foreach (ReportSection section in sections)
            {
                if (section == null || section.Rows == null)
                {
                    continue;
                }

                foreach (string[] row in section.Rows)
                {
                    allRows.Add(row);
                }
            }

            IReadOnlyList<ReportColumn> effectiveColumns = columns;
            float effectiveBodyFontSize = bodyFontSize;

            if (fitToContent)
            {
                effectiveColumns = FitColumnsToContent(columns, allRows, ref effectiveBodyFontSize);
            }

            float headerFontSize = fitToContent
                ? effectiveBodyFontSize
                : FitHeaderFont(effectiveColumns);

            byte[] document = BuildDocument(
                heading, effectiveColumns, sections, headerFontSize, effectiveBodyFontSize);

            File.WriteAllBytes(path, document);
        }

        /// <summary>
        /// Gives every column the width its longest cell (or heading) actually needs,
        /// so a full name, program or title is printed without an ellipsis. If the
        /// content is wider than the page, the body font shrinks step by step until
        /// the whole table fits.
        /// </summary>
        private static IReadOnlyList<ReportColumn> FitColumnsToContent(
            IReadOnlyList<ReportColumn> columns,
            IReadOnlyList<string[]> rows,
            ref float bodyFontSize)
        {
            float size = bodyFontSize;
            float[] widths = new float[columns.Count];

            while (true)
            {
                float total = 0f;

                for (int index = 0; index < columns.Count; index++)
                {
                    float needed = Measure(columns[index].Header, true, size);

                    if (rows != null)
                    {
                        foreach (string[] cells in rows)
                        {
                            if (cells == null || index >= cells.Length)
                            {
                                continue;
                            }

                            string text = cells[index] ?? string.Empty;

                            if (text.Length > 0)
                            {
                                needed = Math.Max(
                                    needed, Measure(text, columns[index].BoldBody, size));
                            }
                        }
                    }

                    widths[index] = needed + (columns[index].Center ? 12f : 16f);
                    total += widths[index];
                }

                if (total <= TableWidth || size <= 6.5f)
                {
                    break;
                }

                size -= 0.25f;
            }

            // Spread the leftover space so the table still reaches both margins.
            float scale = TableWidth / Total(widths);
            List<ReportColumn> effective = new List<ReportColumn>(columns.Count);

            for (int index = 0; index < columns.Count; index++)
            {
                effective.Add(new ReportColumn
                {
                    Header = columns[index].Header,
                    Width = widths[index] * scale,
                    Center = columns[index].Center,
                    BoldBody = columns[index].BoldBody
                });
            }

            bodyFontSize = size;
            return effective;
        }

        private static float Total(float[] values)
        {
            float total = 0f;

            foreach (float value in values)
            {
                total += value;
            }

            return total <= 0f ? 1f : total;
        }

        private static float FitHeaderFont(IReadOnlyList<ReportColumn> columns)
        {
            // Start from the comfortable size and shrink until every heading fits
            // its own column, which keeps "BORROWED COPY" readable on a seven
            // column report without wrapping the header row.
            float size = 10f;

            while (size > 7.25f)
            {
                bool fits = true;

                foreach (ReportColumn column in columns)
                {
                    if (Measure(column.Header, true, size) > column.Width - 10f)
                    {
                        fits = false;
                        break;
                    }
                }

                if (fits)
                {
                    break;
                }

                size -= 0.25f;
            }

            return size;
        }

        // ------------------------------------------------------------------ DOCUMENT

        private static byte[] BuildDocument(
            string heading,
            IReadOnlyList<ReportColumn> columns,
            IReadOnlyList<ReportSection> sections,
            float headerFontSize,
            float bodyFontSize)
        {
            PdfObjects objects = new PdfObjects();
            int catalogId = objects.Add(Ascii("<< /Type /Catalog /Pages 2 0 R >>"));
            int pagesId = objects.Add(Ascii("<< >>"));

            int fontRegular = objects.Add(Ascii(
                "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman /Encoding /WinAnsiEncoding >>"));
            int fontBold = objects.Add(Ascii(
                "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Bold /Encoding /WinAnsiEncoding >>"));
            int fontItalic = objects.Add(Ascii(
                "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Italic /Encoding /WinAnsiEncoding >>"));

            int lrcLogo = 0;
            int tcgcLogo = 0;

            Bitmap? lrc = LoadLogo("oop_project.logos.lrc_logo.png");
            Bitmap? tcgc = LoadLogo("oop_project.logos.tcgc_logo.png");

            if (lrc != null)
            {
                using (lrc)
                {
                    lrcLogo = AddImage(objects, lrc);
                }
            }

            if (tcgc != null)
            {
                using (tcgc)
                {
                    tcgcLogo = AddImage(objects, tcgc);
                }
            }

            List<StringBuilder> pages = BuildPages(heading, columns, sections, headerFontSize, bodyFontSize);

            StringBuilder resources = new StringBuilder();
            resources.Append("/Font << /F1 ").Append(fontRegular).Append(" 0 R /F2 ")
                .Append(fontBold).Append(" 0 R /F3 ").Append(fontItalic).Append(" 0 R >>");

            if (lrcLogo > 0 || tcgcLogo > 0)
            {
                resources.Append(" /XObject <<");

                if (lrcLogo > 0)
                {
                    resources.Append(" /Im1 ").Append(lrcLogo).Append(" 0 R");
                }

                if (tcgcLogo > 0)
                {
                    resources.Append(" /Im2 ").Append(tcgcLogo).Append(" 0 R");
                }

                resources.Append(" >>");
            }

            List<int> pageIds = new List<int>();

            foreach (StringBuilder content in pages)
            {
                int contentId = objects.Add(StreamObject(content.ToString()));
                int pageId = objects.Add(Ascii(
                    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + Number(PageWidth) + " " + Number(PageHeight) + "] " +
                    "/Resources << " + resources + " >> /Contents " + contentId + " 0 R >>"));

                pageIds.Add(pageId);
            }

            StringBuilder kids = new StringBuilder();
            foreach (int pageId in pageIds)
            {
                kids.Append(pageId).Append(" 0 R ");
            }

            objects.Set(pagesId, Ascii(
                "<< /Type /Pages /Kids [" + kids.ToString().Trim() + "] /Count " + pages.Count + " >>"));

            return objects.Build(catalogId);
        }

        private static List<StringBuilder> BuildPages(
            string heading,
            IReadOnlyList<ReportColumn> columns,
            IReadOnlyList<ReportSection> sections,
            float headerFontSize,
            float bodyFontSize)
        {
            List<StringBuilder> pages = new List<StringBuilder>();
            StringBuilder page = new StringBuilder();
            float y = HeaderTop;

            foreach (ReportSection section in sections)
            {
                if (section == null)
                {
                    continue;
                }

                // Every section starts on a fresh page with its own subtitle
                // (for example the attendance year) under the report heading.
                page = new StringBuilder();
                pages.Add(page);

                y = DrawLetterhead(page, heading, section.Subtitle);
                DrawTableHeader(page, columns, y, headerFontSize);
                y += HeaderHeight;

                int number = 1;

                if (section.Rows != null)
                {
                    foreach (string[] cells in section.Rows)
                    {
                        if (y + RowHeight > BottomLimit)
                        {
                            page = new StringBuilder();
                            pages.Add(page);
                            y = NextPageTop;
                            DrawTableHeader(page, columns, y, headerFontSize);
                            y += HeaderHeight;
                        }

                        DrawRow(page, columns, cells, number, y, bodyFontSize);
                        y += RowHeight;
                        number++;
                    }
                }
            }

            // Defensive: a report with no sections still gets one page.
            if (pages.Count == 0)
            {
                page = new StringBuilder();
                pages.Add(page);
                y = DrawLetterhead(page, heading, string.Empty);
                DrawTableHeader(page, columns, y, headerFontSize);
                y += HeaderHeight;
            }

            // The approval block belongs to the last page. When the table reaches the
            // bottom, the block moves to a fresh page instead of being clipped.
            float signatureTop = y + 50f;

            if (signatureTop + 62f > PageHeight - 24f)
            {
                page = new StringBuilder();
                pages.Add(page);
                signatureTop = 90f;
            }

            DrawSignature(page, signatureTop);

            return pages;
        }

        private static float DrawLetterhead(StringBuilder page, string heading, string subtitle)
        {
            float center = PageWidth / 2f;
            float logoTop = 26f;

            bool hasLrc = LogoExists("oop_project.logos.lrc_logo.png");
            bool hasTcgc = LogoExists("oop_project.logos.tcgc_logo.png");

            if (hasLrc && hasTcgc)
            {
                float total = (LogoSize * 2f) + LogoGap;
                DrawImage(page, "Im1", center - (total / 2f), logoTop, LogoSize, LogoSize);
                DrawImage(page, "Im2", center - (total / 2f) + LogoSize + LogoGap, logoTop, LogoSize, LogoSize);
            }
            else if (hasLrc)
            {
                DrawImage(page, "Im1", center - (LogoSize / 2f), logoTop, LogoSize, LogoSize);
            }
            else if (hasTcgc)
            {
                DrawImage(page, "Im2", center - (LogoSize / 2f), logoTop, LogoSize, LogoSize);
            }

            DrawCentered(page, ReportHeading, 82f, 11f, true);
            DrawCentered(page, heading, 100f, 11f, true);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                DrawCentered(page, subtitle.Trim(), 118f, 10.5f, true);
                return 138f;
            }

            return HeaderTop;
        }

        private static void DrawTableHeader(
            StringBuilder page,
            IReadOnlyList<ReportColumn> columns,
            float top,
            float fontSize)
        {
            DrawRowFrame(page, columns, top, HeaderHeight);

            for (int index = 0; index < columns.Count; index++)
            {
                DrawCenteredCell(page, columns, columns[index].Header, index, top, fontSize, true);
            }
        }

        private static void DrawRow(
            StringBuilder page,
            IReadOnlyList<ReportColumn> columns,
            string[] cells,
            int number,
            float top,
            float bodyFontSize)
        {
            DrawRowFrame(page, columns, top, RowHeight);

            for (int index = 0; index < columns.Count; index++)
            {
                string text = cells != null && index < cells.Length ? cells[index] ?? string.Empty : string.Empty;

                if (index == 0 && text.Length == 0)
                {
                    text = number.ToString(CultureInfo.InvariantCulture);
                }

                if (text.Length == 0)
                {
                    continue;
                }

                if (columns[index].Center)
                {
                    DrawCenteredCell(page, columns, text, index, top, bodyFontSize, columns[index].BoldBody);
                }
                else
                {
                    DrawLeftCell(page, columns, text, index, top, bodyFontSize, columns[index].BoldBody);
                }
            }
        }

        private static void DrawRowFrame(
            StringBuilder page,
            IReadOnlyList<ReportColumn> columns,
            float top,
            float height)
        {
            page.Append("0.75 w 0 0 0 RG\n");
            page.Append(Number(Margin)).Append(' ').Append(Number(Y(top + height))).Append(' ')
                .Append(Number(TableWidth)).Append(' ').Append(Number(height)).Append(" re S\n");

            float x = Margin;

            for (int index = 0; index < columns.Count - 1; index++)
            {
                x += columns[index].Width;
                page.Append(Number(x)).Append(' ').Append(Number(Y(top))).Append(" m ")
                    .Append(Number(x)).Append(' ').Append(Number(Y(top + height))).Append(" l S\n");
            }
        }

        private static void DrawLeftCell(
            StringBuilder page,
            IReadOnlyList<ReportColumn> columns,
            string text,
            int column,
            float top,
            float size,
            bool bold)
        {
            float available = columns[column].Width - 14f;
            string fitted = FitText(text, bold, size, available);

            DrawText(page, fitted, ColumnLeft(columns, column) + 7f, RowBaseline(top, size), size, bold, false, 0f, false);
        }

        private static void DrawCenteredCell(
            StringBuilder page,
            IReadOnlyList<ReportColumn> columns,
            string text,
            int column,
            float top,
            float size,
            bool bold)
        {
            float available = columns[column].Width - 10f;
            string fitted = FitText(text, bold, size, available);

            DrawText(
                page, fitted, ColumnLeft(columns, column), RowBaseline(top, size), size, bold, false,
                columns[column].Width, true);
        }

        private static void DrawSignature(StringBuilder page, float approvedTop)
        {
            DrawText(page, "Approved by:", Margin, Y(approvedTop), 9.5f, false, true, 0f, false);
            DrawText(page, CenterName, Margin, Y(approvedTop + 42f), 11f, true, false, 0f, false);
            DrawText(page, CenterTitle, Margin, Y(approvedTop + 59f), 10f, false, false, 0f, false);
        }

        private static float RowBaseline(float top, float size)
        {
            return Y(top + (RowHeight / 2f) + (size * 0.34f));
        }

        private static float ColumnLeft(IReadOnlyList<ReportColumn> columns, int column)
        {
            float left = Margin;

            for (int index = 0; index < column; index++)
            {
                left += columns[index].Width;
            }

            return left;
        }

        // ------------------------------------------------------------------- DRAWING

        private static void DrawCentered(
            StringBuilder page, string text, float top, float size, bool bold)
        {
            DrawText(page, text, 0f, Y(top), size, bold, false, PageWidth, true);
        }

        private static void DrawText(
            StringBuilder page,
            string text,
            float x,
            float y,
            float size,
            bool bold,
            bool italic,
            float width,
            bool centered)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            float textWidth = Measure(text, bold, size);
            float left = centered ? x + Math.Max(0f, (width - textWidth) / 2f) : x;

            page.Append("BT\n");
            page.Append(italic ? "/F3" : bold ? "/F2" : "/F1").Append(' ')
                .Append(Number(size)).Append(" Tf\n");
            page.Append(Number(left)).Append(' ').Append(Number(y)).Append(" Td\n");
            page.Append('(').Append(Escape(text)).Append(") Tj\nET\n");
        }

        private static void DrawImage(
            StringBuilder page, string name, float left, float top, float width, float height)
        {
            page.Append("q\n");
            page.Append(Number(width)).Append(" 0 0 ").Append(Number(height)).Append(' ')
                .Append(Number(left)).Append(' ').Append(Number(Y(top + height))).Append(" cm\n");
            page.Append('/').Append(name).Append(" Do\nQ\n");
        }

        private static float Y(float top)
        {
            return PageHeight - top;
        }

        // ------------------------------------------------------------- TEXT METRICS

        private static string FitText(string text, bool bold, float size, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || Measure(text, bold, size) <= maxWidth)
            {
                return text;
            }

            const string Suffix = "...";

            for (int length = text.Length - 1; length > 0; length--)
            {
                string candidate = text.Substring(0, length).TrimEnd() + Suffix;

                if (Measure(candidate, bold, size) <= maxWidth)
                {
                    return candidate;
                }
            }

            return Suffix;
        }

        private static float Measure(string text, bool bold, float size)
        {
            int[] table = bold ? BoldWidths : RegularWidths;
            float total = 0f;

            foreach (char character in text)
            {
                if (character == '\u2014')
                {
                    total += 1000f;
                    continue;
                }

                if (character == '\u2013')
                {
                    total += 500f;
                    continue;
                }

                int code = character;

                if (code < 32 || code > 126)
                {
                    total += 556f;
                    continue;
                }

                total += table[code - 32];
            }

            return total * size / 1000f;
        }

        private static readonly int[] RegularWidths =
        {
            250, 333, 408, 500, 500, 833, 778, 180, 333, 333, 500, 564, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 278, 278, 564, 564, 564, 444,
            921, 722, 667, 667, 722, 611, 556, 722, 722, 333, 389, 722, 611, 889, 722, 722,
            556, 722, 667, 556, 611, 722, 722, 944, 722, 722, 611, 333, 278, 333, 469, 500,
            333, 444, 500, 444, 500, 444, 333, 500, 500, 278, 278, 500, 278, 778, 500, 500,
            500, 500, 333, 389, 278, 500, 500, 722, 500, 500, 444, 480, 200, 480, 541
        };

        private static readonly int[] BoldWidths =
        {
            250, 333, 555, 500, 500, 1000, 833, 278, 333, 333, 500, 570, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 333, 333, 570, 570, 570, 500,
            930, 722, 667, 722, 722, 667, 611, 778, 778, 389, 500, 778, 667, 944, 722, 778,
            611, 778, 722, 556, 667, 722, 722, 1000, 722, 722, 667, 333, 278, 333, 581, 500,
            333, 500, 556, 444, 556, 444, 333, 500, 556, 278, 333, 556, 278, 833, 556, 500,
            556, 556, 444, 389, 333, 556, 500, 722, 500, 500, 444, 394, 220, 394, 520
        };

        // -------------------------------------------------------------------- IMAGES

        private static readonly Dictionary<string, bool> LogoCache = new Dictionary<string, bool>();

        private static bool LogoExists(string name)
        {
            if (LogoCache.TryGetValue(name, out bool exists))
            {
                return exists;
            }

            using (Stream? stream = typeof(ReportPdfWriter).Assembly.GetManifestResourceStream(name))
            {
                exists = stream != null;
            }

            LogoCache[name] = exists;
            return exists;
        }

        private static Bitmap? LoadLogo(string name)
        {
            try
            {
                using (Stream? stream = typeof(ReportPdfWriter).Assembly.GetManifestResourceStream(name))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    using (Image image = Image.FromStream(stream))
                    {
                        return new Bitmap(image);
                    }
                }
            }
            catch (Exception)
            {
                // A missing or unreadable logo must never stop the export.
                return null;
            }
        }

        private static int AddImage(PdfObjects objects, Bitmap source)
        {
            const int Maximum = 200;
            int width = source.Width;
            int height = source.Height;

            if (width > Maximum || height > Maximum)
            {
                float scale = Math.Min(Maximum / (float)width, Maximum / (float)height);
                width = Math.Max(1, (int)Math.Round(width * scale));
                height = Math.Max(1, (int)Math.Round(height * scale));
            }

            byte[] rgb = new byte[width * height * 3];

            using (Bitmap canvas = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                using (Graphics graphics = Graphics.FromImage(canvas))
                {
                    graphics.Clear(Color.White);
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(source, new Rectangle(0, 0, width, height));
                }

                BitmapData data = canvas.LockBits(
                    new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                try
                {
                    int stride = data.Stride;
                    byte[] buffer = new byte[stride * height];
                    Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                    for (int row = 0; row < height; row++)
                    {
                        Buffer.BlockCopy(
                            buffer, row * stride, rgb, row * width * 3, width * 3);
                    }
                }
                finally
                {
                    canvas.UnlockBits(data);
                }
            }

            // GDI+ stores 24bpp pixels as B, G, R while the PDF image stream is
            // declared DeviceRGB, so the blue and red channels are swapped here.
            // Without this the LRC shield's orange turns blue in the exported PDF.
            for (int index = 0; index < rgb.Length; index += 3)
            {
                byte blue = rgb[index];
                rgb[index] = rgb[index + 2];
                rgb[index + 2] = blue;
            }

            byte[] compressed = ZlibCompress(rgb);
            string dictionary =
                "<< /Type /XObject /Subtype /Image /Width " + width + " /Height " + height +
                " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /FlateDecode /Length " +
                compressed.Length + " >>";

            return objects.Add(StreamBytes(dictionary, compressed));
        }

        // --------------------------------------------------------------------- PDF IO

        private sealed class PdfObjects
        {
            private readonly List<byte[]> _items = new List<byte[]>();

            public int Add(byte[] body)
            {
                _items.Add(body);
                return _items.Count;
            }

            public void Set(int id, byte[] body)
            {
                _items[id - 1] = body;
            }

            public byte[] Build(int rootId)
            {
                using (MemoryStream output = new MemoryStream())
                {
                    WriteAscii(output, "%PDF-1.4\n");
                    output.WriteByte((byte)'%');
                    output.WriteByte(0xE2);
                    output.WriteByte(0xE3);
                    output.WriteByte(0xCF);
                    output.WriteByte(0xD3);
                    output.WriteByte((byte)'\n');

                    long[] offsets = new long[_items.Count + 1];

                    for (int index = 0; index < _items.Count; index++)
                    {
                        offsets[index + 1] = output.Position;
                        WriteAscii(output, (index + 1).ToString(CultureInfo.InvariantCulture) + " 0 obj\n");
                        output.Write(_items[index], 0, _items[index].Length);
                        WriteAscii(output, "\nendobj\n");
                    }

                    long startXref = output.Position;
                    WriteAscii(
                        output,
                        "xref\n0 " + (_items.Count + 1).ToString(CultureInfo.InvariantCulture) + "\n0000000000 65535 f \n");

                    for (int index = 1; index <= _items.Count; index++)
                    {
                        WriteAscii(
                            output,
                            offsets[index].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
                    }

                    WriteAscii(
                        output,
                        "trailer\n<< /Size " + (_items.Count + 1).ToString(CultureInfo.InvariantCulture) +
                        " /Root " + rootId.ToString(CultureInfo.InvariantCulture) + " 0 R >>\nstartxref\n" +
                        startXref.ToString(CultureInfo.InvariantCulture) + "\n%%EOF");

                    return output.ToArray();
                }
            }
        }

        private static byte[] StreamObject(string content)
        {
            byte[] bytes = Encoding.Latin1.GetBytes(content);

            return StreamBytes(
                "<< /Length " + bytes.Length.ToString(CultureInfo.InvariantCulture) + " >>", bytes);
        }

        private static byte[] StreamBytes(string dictionary, byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                WriteAscii(stream, dictionary + "\nstream\n");
                stream.Write(data, 0, data.Length);
                WriteAscii(stream, "\nendstream");

                return stream.ToArray();
            }
        }

        private static byte[] Ascii(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private static void WriteAscii(Stream stream, string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                output.WriteByte(0x78);
                output.WriteByte(0x9C);

                using (DeflateStream deflate = new DeflateStream(
                    output, CompressionLevel.Optimal, true))
                {
                    deflate.Write(data, 0, data.Length);
                }

                uint a = 1;
                uint b = 0;

                foreach (byte value in data)
                {
                    a = (a + value) % 65521;
                    b = (b + a) % 65521;
                }

                uint adler = (b << 16) | a;
                output.WriteByte((byte)(adler >> 24));
                output.WriteByte((byte)(adler >> 16));
                output.WriteByte((byte)(adler >> 8));
                output.WriteByte((byte)adler);

                return output.ToArray();
            }
        }

        private static string Escape(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length + 8);

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '(':
                        builder.Append("\\(");
                        break;
                    case ')':
                        builder.Append("\\)");
                        break;
                    case '\u2014':
                        builder.Append('\u0097'); // WinAnsi em dash
                        break;
                    case '\u2013':
                        builder.Append('\u0096'); // WinAnsi en dash
                        break;
                    case '\u2018':
                        builder.Append('\u0091');
                        break;
                    case '\u2019':
                        builder.Append('\u0092');
                        break;
                    case '\u201c':
                        builder.Append('\u0093');
                        break;
                    case '\u201d':
                        builder.Append('\u0094');
                        break;
                    case '\u2022':
                        builder.Append('\u0095');
                        break;
                    case '\u2026':
                        builder.Append('\u0085');
                        break;
                    default:
                        builder.Append(character >= 32 && character <= 255 ? character : '?');
                        break;
                }
            }

            return builder.ToString();
        }

        private static string Number(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
