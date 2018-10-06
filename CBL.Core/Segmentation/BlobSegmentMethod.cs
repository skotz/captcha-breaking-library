using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using ScottClayton.CAPTCHA.Utility;

namespace ScottClayton.Image
{
    public class BlobSegmentMethod : SegmentMethod
    {
        private Bitmap bmp;
        private int minWidth;
        private int minHeight;
        private int numChars;

        ///// <summary>
        ///// The type of blob extraction to perform
        ///// </summary>
        //public BlobSegmentationType Setting { get; private set; }
        
        /// <summary>
        /// Sets up the segmenter to get all blobs with a height and width greater than the supplied dimensions
        /// </summary>
        /// <param name="minimumWidth">The minimum width of each blob</param>
        /// <param name="minimumHeight">The minimum height of each blob</param>
        public BlobSegmentMethod(int minimumWidth, int minimumHeight)
        {
            minWidth = minimumWidth;
            minHeight = minimumHeight;
            numChars = -1;
        }

        /// <summary>
        /// Sets up the segmenter to get the N largest blobs in the image
        /// </summary>
        /// <param name="minimumWidth">The minimum width of each blob</param>
        /// <param name="minimumHeight">The minimum height of each blob</param>
        /// <param name="numberOfBlobsToGet">The number of blobs to extract</param>
        public BlobSegmentMethod(int minimumWidth, int minimumHeight, int numberOfBlobsToGet)
        {
            minWidth = minimumWidth;
            minHeight = minimumHeight;
            numChars = numberOfBlobsToGet;
        }

        public override SegmentMethod Clone()
        {
            return new BlobSegmentMethod(minWidth, minHeight, numChars);
        }

        /// <summary>
        /// Segment this image by extracting all blobs. Each blob must be a different color to be correctly segmented.
        /// </summary>
        /// <param name="image">The image to extract blobs from</param>
        /// <returns></returns>
        public override List<Bitmap> Segment(Bitmap image)
        { 
            // Before: 707 ms
            // After:  61 ms

            this.bmp = CropMinimum(image);

            List<BLOB> bitmaps = new List<BLOB>();
            byte[] c;
            int sum = 0;
            int close = 5;

            ConsoleProgress prog = new ConsoleProgress("Segmenting ");
            
            // Create a separate bitmap for each distinct color in the image
            while (!((c = GetNextNonWhitePixel(close)).Subtract(Color.White) < close))
            {
                Bitmap copy = new Bitmap(bmp.Width, bmp.Height);
                int pixelCount = 0;

                BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                BitmapData bmData2 = copy.LockBits(new Rectangle(0, 0, copy.Width, copy.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;
                IntPtr Scan02 = bmData2.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    byte* p2 = (byte*)(void*)Scan02;
                    int nOffset = stride - bmp.Width * 3;

                    int red, green, blue;
                    int difference;

                    for (int y = 0; y < bmp.Height; ++y)
                    {
                        for (int x = 0; x < bmp.Width; ++x)
                        {
                            blue = p[0] - c[0];
                            green = p[1] - c[1];
                            red = p[2] - c[2];

                            if (blue < 0) blue = -blue;
                            if (green < 0) green = -green;
                            if (red < 0) red = -red;

                            difference = blue > green ? blue : green;
                            difference = difference > red ? difference : red;

                            if (difference < close)
                            {
                                p[0] = p[1] = p[2] = 255;
                                p2[0] = p2[1] = p2[2] = 0;
                                pixelCount++;
                            }
                            else
                            {
                                p2[0] = p2[1] = p2[2] = 255;
                            }

                            p += 3;
                            p2 += 3;
                        }

                        p += nOffset;
                        p2 += nOffset;
                    }
                }

                bmp.UnlockBits(bmData);
                copy.UnlockBits(bmData2);

                copy = CropMinimum(copy);
                if (copy.Width >= minWidth && copy.Height >= minHeight)
                {
                    bitmaps.Add(new BLOB() { Image = copy, PixelCount = pixelCount });
                    sum += copy.Width;
                }
            }

            if (bitmaps.Count == 0)
            {
                // The image will fail anyway, might as well return something that won't create an infinite loop
                return new List<Bitmap>() { new Bitmap(10, 10) };
            }

            // If we know that there are N characters in the image and we don't have all of them, then do some math 
            // to see if we can logically split larger blobs into smaller blobs using a histogram or delete small blobs if there are too many.
            if (numChars != -1)
            {
                int smallest; // bitmaps[0].Image.Width * bitmaps[0].Image.Height;
                int index = 0;
                int dim;
                while (bitmaps.Count > numChars)
                {
                    smallest = bitmaps[0].PixelCount ; // .Image.Width * bitmaps[0].Image.Height;
                    index = 0;

                    for (int b = 0; b < bitmaps.Count; b++)
                    {
                        dim = bitmaps[b].PixelCount; // .Image.Width * bitmaps[b].Image.Height;

                        if (dim < smallest)
                        {
                            smallest = dim;
                            index = b;
                        }
                    }

                    bitmaps.RemoveAt(index);
                }

                while (bitmaps.Count < numChars)
                {
                    for (double i = 3.0; i > 0 && bitmaps.Count < numChars; i -= 0.1)
                    {
                        //double i = 1.3;
                        for (int b = 0; b < bitmaps.Count; b++)
                        {
                            double acceptable = (((double)bmp.Width / (double)numChars) * i);
                            if (bitmaps[b].Image.Width > acceptable)
                            {
                                int ct = (int)Math.Round((double)bitmaps[b].Image.Width / ((double)sum / (double)numChars));
                                Bitmap[] splits = SplitInto(bitmaps[b].Image, ct < 2 ? 2 : ct);
                                bitmaps.RemoveAt(b);

                                int xx = 0;
                                foreach (Bitmap b2 in splits)
                                {
                                    bitmaps.Insert(b + (xx++), new BLOB() { Image = CropMinimum(b2) });
                                }
                                break;
                            }
                        }
                    }
                }
            }

            prog.Stop();

            return bitmaps.Select(b => b.Image).ToList();
        }

        /// <summary>
        /// Split an image into N smaller images using a histogram to find the most likely split point.
        /// </summary>
        private Bitmap[] SplitInto(Bitmap b, int count)
        {
            Bitmap[] done = new Bitmap[count];
            Graphics[] g = new Graphics[count];
            int w = b.Width / count;
            int h = b.Height;

            int lrSearch = 10;

            List<int> histogram = new List<int>();
            for (int x = 0; x < b.Width; x++)
            {
                int nums = 0;
                for (int y = 0; y < b.Height; y++)
                {
                    if (b.GetPixel(x, y).Subtract(Color.White) > 5)
                    {
                        nums++;
                    }
                }
                histogram.Add(nums);
            }

            for (int i = 0; i < count; i++)
            {
                int adjust = 0;
                int best = b.Height;
                if (i * w > lrSearch && i * w < b.Width - lrSearch - 1)
                {
                    for (int x = -lrSearch; x <= lrSearch; x++)
                    {
                        if (histogram[i * w + x] < best)
                        {
                            best = histogram[i * w + x];
                            adjust = x;
                        }
                    }
                }

                int newRight = i * w + w;
                int best2 = b.Height;
                if (i * w + w > lrSearch && i * w + w < b.Width - lrSearch - 1)
                {
                    for (int x = -lrSearch; x <= lrSearch; x++)
                    {
                        if (histogram[i * w + w + x] < best2)
                        {
                            best2 = histogram[i * w + w + x];
                            newRight = i * w + w + x;
                        }
                    }
                }

                done[i] = new Bitmap(newRight - (i * w + adjust) < 1 ? 1 : newRight - (i * w + adjust), h);
                g[i] = Graphics.FromImage(done[i]);

                g[i].DrawImage(b, new Rectangle(0, 0, newRight - (i * w + adjust), h), new Rectangle(i * w + adjust, 0, newRight - (i * w + adjust), h), GraphicsUnit.Pixel);
            }

            return done;
        }

        /// <summary>
        /// Cut the unneeded white space off from around an image.
        /// </summary>
        private Bitmap CropMinimum(Bitmap bmp)
        {
            // Before Conversion: 100ms Average
            // After Conversion:  13ms  Average

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int[] horizontal = new int[bmp.Height];
            int[] vertical = new int[bmp.Width];
            for (int x = 0; x < bmp.Width; x++)
            {
                vertical[x] = 0;
            }

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - bmp.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < bmp.Height; ++y)
                {
                    int count = 0;
                    for (int x = 0; x < bmp.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        if (!(p[0] > 245 && p[1] > 245 && p[2] > 245))
                        {
                            // Far enough away from white to count it
                            vertical[x]++;
                            count++;
                        }

                        p += 3;
                    }
                    horizontal[y] = count;

                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmData);

            int left = -1;
            int right = -1;
            int top = -1;
            int bottom = -1;

            for (int x = 0; x < bmp.Width; x++)
            {
                if (vertical[x] != 0)
                {
                    left = x;
                    break;
                }
            }

            for (int x = bmp.Width - 1; x >= 0; x--)
            {
                if (vertical[x] != 0)
                {
                    right = x;
                    break;
                }
            }

            for (int y = 0; y < bmp.Height; y++)
            {
                if (horizontal[y] != 0)
                {
                    top = y;
                    break;
                }
            }

            for (int y = bmp.Height - 1; y >= 0; y--)
            {
                if (horizontal[y] != 0)
                {
                    bottom = y;
                    break;
                }
            }

            Rectangle crop = new Rectangle(left - 1, top - 1, right - left + 2, bottom - top + 2);

            Bitmap cropped = new Bitmap(crop.Width, crop.Height);
            Graphics g = Graphics.FromImage(cropped);

            g.DrawImage(bmp, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);

            return cropped;
        }

        /// <summary>
        /// Get the color of the next pixel in the image that is not white.
        /// </summary>
        private byte[] GetNextNonWhitePixel(int close)
        {
            // Before: 63.0 ms
            // After:  5.2 ms

            BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int ind;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte red, green, blue;

                for (int x = 0; x < bmp.Width; ++x)
                {
                    for (int y = 0; y < bmp.Height; ++y)
                    {
                        ind = y * stride + x * 3;

                        blue = p[ind];
                        green = p[ind + 1];
                        red = p[ind + 2];

                        // If the color we found was not within CLOSE distance to the color white, then return it
                        if (!(p[ind] > 255 - close && p[ind + 1] > 255 - close && p[ind + 2] > 255 - close))
                        {
                            bmp.UnlockBits(bmData);
                            return new byte[] { blue, green, red };
                        }
                    }
                }
            }

            bmp.UnlockBits(bmData);
            return new byte[] { 255, 255, 255 };
        }
    }

    public class BLOB
    {
        public Bitmap Image { get; set; }
        public int PixelCount { get; set; }
    }

    //public enum BlobSegmentationType
    //{
    //    /// <summary>
    //    /// Segment out all blobs with a pixel count greater than N
    //    /// </summary>
    //    PIXEL_COUNT,

    //    /// <summary>
    //    /// Segment out the larnest N blobs from the image
    //    /// </summary>
    //    N_LARGEST_BLOBS,

    //    /// <summary>
    //    /// Segment out letters with a width and height greater than supplied dimensions
    //    /// </summary>
    //    MIN_DIMENSIONS
    //}
}
