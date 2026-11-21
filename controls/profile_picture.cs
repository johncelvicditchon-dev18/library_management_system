using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace oop_project
{
    /// <summary>
    /// Turns the photo chosen in EDIT MY INFO into the small square JPEG stored on
    /// the account row, and back into the picture shown by the avatars. A missing
    /// or damaged picture never breaks the profile page.
    /// </summary>
    public static class ProfilePicture
    {
        /// <summary>Longest side of the stored portrait, in pixels.</summary>
        public const int MaxSide = 512;

        /// <summary>Largest photo the editor accepts, so a huge camera file cannot stall the UI.</summary>
        private const long MaxSourcePixels = 40000000;

        /// <summary>Reads the stored bytes. Returns null when there is no usable picture.</summary>
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

        /// <summary>Opens a picture file chosen by the user. Throws when the file is not an image.</summary>
        public static Image LoadFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Image source = Image.FromStream(stream, true, true))
            {
                return new Bitmap(source);
            }
        }

        /// <summary>
        /// Crops the middle square of the photo, shrinks it to at most 512 pixels
        /// and encodes it as a JPEG, so every portrait costs only a few dozen KB.
        /// </summary>
        public static byte[] CreatePortrait(Image source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if ((long)source.Width * source.Height > MaxSourcePixels)
            {
                throw new InvalidOperationException("THE PICTURE IS TOO LARGE. PLEASE CHOOSE A SMALLER PHOTO.");
            }

            using (Bitmap oriented = Orient(source))
            {
                int side = Math.Min(oriented.Width, oriented.Height);

                if (side <= 0)
                {
                    throw new InvalidOperationException("THE PICTURE HAS NO USABLE SIZE.");
                }

                int target = Math.Min(MaxSide, side);
                var crop = new Rectangle(
                    (oriented.Width - side) / 2,
                    (oriented.Height - side) / 2,
                    side,
                    side);

                using (var canvas = new Bitmap(target, target, PixelFormat.Format24bppRgb))
                {
                    using (Graphics graphics = Graphics.FromImage(canvas))
                    {
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.Clear(Color.White);
                        graphics.DrawImage(
                            oriented,
                            new Rectangle(0, 0, target, target),
                            crop,
                            GraphicsUnit.Pixel);
                    }

                    using (var output = new MemoryStream())
                    {
                        ImageCodecInfo? codec = FindJpegEncoder();
                        var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, 88L);

                        if (codec != null)
                        {
                            canvas.Save(output, codec, parameters);
                        }
                        else
                        {
                            canvas.Save(output, ImageFormat.Jpeg);
                        }

                        return output.ToArray();
                    }
                }
            }
        }

        private static ImageCodecInfo? FindJpegEncoder()
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>Applies the EXIF rotation of phone photos before the square crop.</summary>
        private static Bitmap Orient(Image source)
        {
            Bitmap copy = new Bitmap(source);

            try
            {
                int orientation = 1;

                foreach (PropertyItem item in source.PropertyItems)
                {
                    if (item.Id == 0x0112 && item.Value != null && item.Value.Length > 0)
                    {
                        orientation = item.Value[0];
                        break;
                    }
                }

                switch (orientation)
                {
                    case 2:
                        copy.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        copy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        copy.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case 5:
                        copy.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        copy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        copy.RotateFlip(RotateFlipType.Rotate90FlipY);
                        break;
                    case 8:
                        copy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
            }
            catch (ArgumentException)
            {
                // No EXIF block on this picture: the bitmap is already upright.
            }

            return copy;
        }
    }
}
