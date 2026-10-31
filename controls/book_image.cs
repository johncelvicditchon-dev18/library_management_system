using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace oop_project
{
    /// <summary>
    /// Turns the bytes stored in books_table.book_image into the pictures the
    /// dashboards show. A missing or damaged cover never crashes a grid.
    /// </summary>
    public static class BookImage
    {
        public static Image? Load(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                using (Image source = Image.FromStream(stream, true, true))
                {
                    return new Bitmap(source);
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (ExternalException)
            {
                return null;
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        public static Image CreateThumbnail(byte[]? bytes, int width, int height, bool drawPlaceholder = false)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            Bitmap canvas = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(canvas))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.Clear(Color.White);

                using (Image? source = Load(bytes))
                {
                    if (source != null)
                    {
                        Rectangle target = Fit(source.Size, width, height);
                        graphics.DrawImage(source, target);
                        return canvas;
                    }
                }

                if (drawPlaceholder)
                {
                    DrawPlaceholder(graphics, width, height);
                }
            }

            return canvas;
        }

        private static Rectangle Fit(Size source, int width, int height)
        {
            double scale = Math.Min((double)width / source.Width, (double)height / source.Height);
            int targetWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
            int targetHeight = Math.Max(1, (int)Math.Round(source.Height * scale));

            return new Rectangle(
                (width - targetWidth) / 2,
                (height - targetHeight) / 2,
                targetWidth,
                targetHeight);
        }

        private static void DrawPlaceholder(Graphics graphics, int width, int height)
        {
            using (SolidBrush background = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                graphics.FillRectangle(background, 0, 0, width, height);
            }

            int bookWidth = Math.Max(18, width / 3);
            int bookHeight = Math.Max(24, height / 3);
            int left = (width - bookWidth) / 2;
            int top = (height - bookHeight) / 2;

            using (SolidBrush cover = new SolidBrush(Color.FromArgb(203, 213, 225)))
            using (Pen outline = new Pen(Color.FromArgb(148, 163, 184), Math.Max(1f, height / 60f)))
            {
                graphics.FillRectangle(cover, left, top, bookWidth, bookHeight);
                graphics.DrawRectangle(outline, left, top, bookWidth, bookHeight);
                graphics.DrawLine(outline, left + bookWidth / 4, top + 2, left + bookWidth / 4, top + bookHeight - 2);
            }
        }
    }
}
