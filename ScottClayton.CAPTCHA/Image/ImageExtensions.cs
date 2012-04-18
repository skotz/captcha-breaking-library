using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using ScottClayton.Neural;

namespace ScottClayton.Image
{
    public static class ImageExtensions
    {
        public static Color Blend(this Color self, Color other)
        {
            return Color.FromArgb((self.R + other.R) / 2, (self.G + other.G) / 2, (self.B + other.B) / 2);
        }

        public static bool Equals2(this Color a, Color b)
        {
            return (a.R == b.R) && (a.G == b.G) && (a.B == b.B);
        }

        public static int Subtract(this Color c, Color other)
        {
            return Math.Max(Math.Abs(c.R - other.R), Math.Max(Math.Abs(c.G - other.G), Math.Abs(c.B - other.B)));
        }

        public static int Subtract(this byte[] c, byte[] o)
        {
            return Math.Max(Math.Abs(c[0] - o[0]), Math.Max(Math.Abs(c[1] - o[1]), Math.Abs(c[2] - o[2])));
        }

        public static int Subtract(this byte[] c, Color other)
        {
            return Math.Max(Math.Abs(c[2] - other.R), Math.Max(Math.Abs(c[1] - other.G), Math.Abs(c[0] - other.B)));
        }

        public static int SumDiff(this byte[] c, Color other)
        {
            return Math.Abs(c[2] - other.R) + Math.Abs(c[1] - other.G) + Math.Abs(c[0] - other.B);
        }

        public static int ColorDiff(this byte[] c, Color other)
        {
            int r = Math.Abs(c[2] - other.R);
            int g = Math.Abs(c[1] - other.G);
            int b = Math.Abs(c[0] - other.B);
            return (int)Math.Sqrt((r * r + g * g + b * b) / 3);
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this byte[] c, Color color)
        {
            // Yep. Very inefficient to create a Color here. TODO: Fix this.
            return GetEDeltaColorDifference(Color.FromArgb(c[2], c[1], c[0]), color);
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this Color color, byte r, byte g, byte b)
        {
            return GetEDeltaColorDifference(Color.FromArgb(r, g, b), color);
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this byte[] c, byte[] other)
        {
            // Yep. Very inefficient to create a Color here. TODO: Fix this.
            return GetEDeltaColorDifference(Color.FromArgb(c[2], c[1], c[0]), Color.FromArgb(other[2], other[1], other[0]));
        }

        /// <summary>
        /// A color difference algorithm to get the difference in visible color, and not just the integer difference in RGB values.
        /// NOTE: The JND (Just Noticible Difference) between two colors is about 2.3.
        /// </summary>
        public static double GetEDeltaColorDifference(this Color c, Color color)
        {
            LAB a = c.GetLAB();
            LAB b = color.GetLAB();

            return Math.Sqrt(Math.Pow(a.L - b.L, 2) + Math.Pow(a.a - b.a, 2) + Math.Pow(a.b - b.b, 2));
        }

        public static LAB GetLAB(this Color c)
        {
            return c.GetXYZ().GetLAB();
        }

        public static XYZ GetXYZ(this Color c)
        {
            // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

            double var_R = (c.R / 255.0);
            double var_G = (c.G / 255.0);
            double var_B = (c.B / 255.0);

            if (var_R > 0.04045) var_R = Math.Pow(((var_R + 0.055) / 1.055), 2.4);
            else var_R = var_R / 12.92;
            if (var_G > 0.04045) var_G = Math.Pow(((var_G + 0.055) / 1.055), 2.4);
            else var_G = var_G / 12.92;
            if (var_B > 0.04045) var_B = Math.Pow(((var_B + 0.055) / 1.055), 2.4);
            else var_B = var_B / 12.92;

            var_R = var_R * 100;
            var_G = var_G * 100;
            var_B = var_B * 100;

            XYZ xyz = new XYZ();
            xyz.X = var_R * 0.4124 + var_G * 0.3576 + var_B * 0.1805;
            xyz.Y = var_R * 0.2126 + var_G * 0.7152 + var_B * 0.0722;
            xyz.Z = var_R * 0.0193 + var_G * 0.1192 + var_B * 0.9505;

            return xyz;
        }

        public static LAB GetLAB(this XYZ c)
        {
            // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

            double var_X = c.X / 95.047;
            double var_Y = c.Y / 100.000;
            double var_Z = c.Z / 108.883;

            if (var_X > 0.008856) var_X = Math.Pow(var_X, (1.0 / 3));
            else var_X = (7.787 * var_X) + (16.0 / 116);
            if (var_Y > 0.008856) var_Y = Math.Pow(var_Y, (1.0 / 3));
            else var_Y = (7.787 * var_Y) + (16.0 / 116);
            if (var_Z > 0.008856) var_Z = Math.Pow(var_Z, (1.0 / 3));
            else var_Z = (7.787 * var_Z) + (16.0 / 116);

            LAB lab = new LAB();
            lab.L = (116 * var_Y) - 16;
            lab.a = 500 * (var_X - var_Y);
            lab.b = 200 * (var_Y - var_Z);

            return lab;
        }

        public static Bitmap Resize(this Bitmap image, int newWidth, int newHeight)
        {
            return ImageFunctions.ResizeBitmap(image, newWidth, newHeight);
        }

        public static Bitmap ResizeKeepRatioAndCenter(this Bitmap image, int newWidth, int newHeight, Color bkg)
        {
            lock (image)
            {
                Bitmap result = new Bitmap(newWidth, newHeight);

                int w;
                int h;

                if ((double)newWidth / image.Width < (double)newHeight / image.Height)
                {
                    w = newWidth;
                    h = (int)(w * ((double)image.Height / image.Width));
                }
                else
                {
                    h = newHeight;
                    w = (int)(h * ((double)image.Width / image.Height));
                }

                using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                {
                    g.Clear(bkg);
                    g.DrawImage(image, newWidth / 2 - w / 2, newHeight / 2 - h / 2, w, h);
                }

                return result;
            }
        }

        /// <summary>
        /// Crop all of the excess whitespace off of an image.
        /// </summary>
        public static Bitmap CropResize(this Bitmap image)
        {
            int left = 0;
            int right = image.Width - 1;

            int top = 0;
            int bottom = image.Height - 1;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y).Subtract(Color.White) > 10)
                    {
                        left = x;
                        x = image.Width;
                        y = image.Height;
                    }
                }
            }

            for (int x = image.Width - 1; x >= 0; x--)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y).Subtract(Color.White) > 10)
                    {
                        right = x;
                        x = 0;
                        y = image.Height;
                    }
                }
            }

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y).Subtract(Color.White) > 10)
                    {
                        top = y;
                        x = image.Width;
                        y = image.Height;
                    }
                }
            }

            for (int y = image.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y).Subtract(Color.White) > 10)
                    {
                        bottom = y;
                        x = image.Width;
                        y = 0;
                    }
                }
            }

            Bitmap n = new Bitmap(right - left + 3, bottom - top + 3, image.PixelFormat);
            Graphics g = Graphics.FromImage(n);

            g.DrawImage(image, new Rectangle(0, 0, right - left + 3, bottom - top + 3), new Rectangle(left - 1, top - 1, right - left + 2, bottom - top + 2), GraphicsUnit.Pixel);

            return n;
        }

        public static Bitmap Crop(this Bitmap image, Rectangle area)
        {
            int left = area.X;
            int right = Math.Min(image.Width - 1, area.Right - 1);

            int top = area.Y;
            int bottom = Math.Min(image.Height - 1, area.Bottom - 1);

            Bitmap n = new Bitmap(right - left + 3, bottom - top + 3, image.PixelFormat);
            Graphics g = Graphics.FromImage(n);

            g.DrawImage(image, new Rectangle(0, 0, right - left + 3, bottom - top + 3), new Rectangle(left - 1, top - 1, right - left + 2, bottom - top + 2), GraphicsUnit.Pixel);

            return n;
        }

        public static Bitmap CloneFull(this Bitmap image)
        {
            Bitmap n = new Bitmap(image.Width, image.Height);
            Graphics g = Graphics.FromImage(n);
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            return n;
        }

        public static Bitmap MergeHorizontal(this List<Bitmap> images, int spacing = 5)
        {
            int width = images.Sum(b => b.Width + spacing) + spacing;
            int height = images.Max(b => b.Height) + spacing * 2;

            Bitmap merge = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(merge);

            int x = spacing;

            foreach (Bitmap b in images)
            {
                g.DrawImage(b, new Rectangle(x, height / 2 - b.Height / 2, b.Width, b.Height), new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
                x += b.Width + spacing;
            }

            return merge;
        }

        public static Bitmap MergeVertical(this List<Bitmap> images, int spacing = 5)
        {
            int width = images.Max(b => b.Width) + spacing * 2;
            int height = images.Sum(b => b.Height + spacing) + spacing;

            Bitmap merge = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(merge);

            int y = spacing;

            foreach (Bitmap b in images)
            {
                g.DrawImage(b, new Rectangle(width / 2 - b.Width / 2, y, b.Width, b.Height), new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
                y += b.Height + spacing;
            }

            return merge;
        }


        public static DoubleVector GetScaledVectorVerticalHistogram(this Bitmap image)
        {
            BitmapData bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int[] hist = new int[image.Width];
            for (int x = 0; x < image.Width; x++)
            {
                hist[x] = 0;
            }

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - image.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < image.Height; ++y)
                {
                    for (int x = 0; x < image.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        hist[x] += 255 * 255 * 255 - blue * green * red;

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            image.UnlockBits(bmData);

            DoubleVector v = new DoubleVector(image.Width);

            for (int i = 0; i < image.Width; i++)
            {
                v[i] = (double)hist[i];
            }

            //double largest = v[v.GetIndexOfLargestElement()];
            //for (int i = 0; i < image.Width; i++)
            //{
            //    v[i] /= largest;
            //}
            for (int i = 0; i < image.Width; i++)
            {
                v[i] /= image.Height;
            }

            return v;
        }

        public static DoubleVector GetScaledVectorHorizontalHistogram(this Bitmap image)
        {
            BitmapData bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int[] hist = new int[image.Height];
            for (int y = 0; y < image.Height; y++)
            {
                hist[y] = 0;
            }

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - image.Width * 3;

                byte red, green, blue;

                for (int x = 0; x < image.Width; ++x)
                {
                    for (int y = 0; y < image.Height; ++y)
                    {
                        int i = y * stride + x * 3;

                        blue = p[i + 0];
                        green = p[i + 1];
                        red = p[i + 2];

                        hist[y] += 255 * 255 * 255 - blue * green * red;
                    }
                }
            }

            image.UnlockBits(bmData);

            DoubleVector v = new DoubleVector(image.Height);

            for (int i = 0; i < image.Height; i++)
            {
                v[i] = (double)hist[i];
            }

            //double largest = v[v.GetIndexOfLargestElement()];
            //for (int i = 0; i < image.Height; i++)
            //{
            //    v[i] /= largest;
            //}
            for (int i = 0; i < image.Height; i++)
            {
                v[i] /= image.Width;
            }

            return v;
        }
    }
}
