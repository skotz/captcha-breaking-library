using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using AForge.Imaging.Filters;
using ScottClayton.CAPTCHA.Utility;
using ScottClayton.CAPTCHA;
using Emgu.CV.Structure;
using Emgu.CV;

namespace ScottClayton.Image
{
    /// <summary>
    /// Provides methods for preprocessing an image and then segmenting it.
    /// </summary>
    public class Segmenter
    {
        /// <summary>
        /// The CAPTCHA that all preprocessing operations are applied on.
        /// You can access this image directly from the Segmentation Event if you wish to perform some custom processing.
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// The method used to break apart the CAPTCHA into individual letters after preprocessing.
        /// </summary>
        public SegmentMethod SegmentationMethod { get; set; }

        private static Random random = new Random();

        /// <summary>
        /// Get a new copy of this segmenter.
        /// </summary>
        public Segmenter Clone()
        {
            Segmenter clone = new Segmenter();
            clone.SegmentationMethod = SegmentationMethod.Clone();
            return clone;
        }

        /// <summary>
        /// Apply a mean shift filter (of sorts) to the image. 
        /// This will effectively flatten out color groups within a certain tolerance.
        /// </summary>
        /// <param name="iterations">The number of times to apply the filter.</param>
        /// <param name="radius">The pixel radius to gather a mean value from</param>
        /// <param name="tolerance">The tolerance a pixel must be within to be added to the average</param>
        public void MeanShiftFilter(int iterations, int radius = 3, double tolerance = 5.0, bool ignorebkg = true)
        {
            try
            {
                BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //Func<byte[], byte[], double> dist = (x, y) => Math.Sqrt(((x[0] - y[0]) * (x[0] - y[0]) + (x[0] - y[0]) * (x[0] - y[0]) + (x[0] - y[0]) * (x[0] - y[0])) / 3.0);
                Func<byte, byte, byte, byte, byte, byte, double> dist = (a, b, c, x, y, z) => (new byte[] { a, b, c }).GetEDeltaColorDifference(new byte[] { x, y, z });

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int stride = bmpData.Stride;

                    for (int iteration = 0; iteration < iterations; iteration++)
                    {
                        for (int x = 0; x < Image.Width - 1; x++)
                        {
                            for (int y = 0; y < Image.Height - 1; y++)
                            {
                                int indexCenter = y * stride + x * 4;

                                int avgR = 0;
                                int avgG = 0;
                                int avgB = 0;
                                int count = 0;

                                for (int dx = -radius; dx <= radius; dx++)
                                {
                                    for (int dy = -radius; dy <= radius; dy++)
                                    {
                                        int newx = (dx + x + Image.Width) % Image.Width;
                                        int newy = (dy + y + Image.Height) % Image.Height;

                                        int index = newy * stride + newx * 4;

                                        if (!ignorebkg || dist(p[index + 2], p[index + 1], p[index + 0], (byte)255, (byte)255, (byte)255) > tolerance)
                                        {
                                            if (dist(p[index + 2], p[index + 1], p[index + 0], p[indexCenter + 2], p[indexCenter + 1], p[indexCenter + 0]) < tolerance)
                                            {
                                                avgB += p[index];
                                                avgG += p[index + 1];
                                                avgR += p[index + 2];
                                                count++;
                                            }
                                        }
                                    }
                                }

                                // Divide by the number of pixels looked at
                                if (count > 0)
                                {
                                    avgR /= count;
                                    avgG /= count;
                                    avgB /= count;

                                    p[indexCenter] = (byte)avgB;
                                    p[indexCenter + 1] = (byte)avgG;
                                    p[indexCenter + 2] = (byte)avgR;
                                }
                            }
                        }
                    }
                }

                Image.UnlockBits(bmpData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error applying the Mean Shift Filter to the image", ex);
            }
        }

        /// <summary>
        /// Crop an image to a specified sub-rectangle.
        /// </summary>
        /// <param name="area">The rectangular area to crop the image down to.</param>
        public void Crop(Rectangle area)
        {
            try
            {
                Image = Image.Crop(area);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error cropping an image.", ex);
            }
        }

        /// <summary>
        /// Try to save the image to a file. If it fails then just move on and forget it ever happened.
        /// This is the way to go when you might have several threads trying to save to the same file, and you don't really need the image (for debugging).
        /// </summary>
        /// <param name="fileName">The location to attempt to save the image to.</param>
        public void TrySave(string fileName)
        {
            try
            {
                Image.Save(fileName);
            }
            catch (Cookie) { /* Yum! */ }
            catch { /* This never happened... */ }

            GlobalMessage.SendMessage(Image);
        }

        /// <summary>
        /// Resize the image
        /// </summary>
        /// <param name="newWidth">The new width</param>
        /// <param name="newHeight">The new height</param>
        public void Resize(int newWidth, int newHeight)
        {
            try
            {
                Image = Image.Resize(newWidth, newHeight);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error resizing an image.", ex);
            }
        }

        /// <summary>
        /// Fill a color into a region of an image within a certain tolerance. A random color will be used.
        /// </summary>
        /// <param name="origin">The point to start filling from</param>
        /// <param name="tolerance">The amount of difference naighboring pixels can have and still be considered part of the same group</param>
        public Color FloodFill(Point origin, int tolerance)
        {
            Color fill = Color.FromArgb(random.Next(20, 225), random.Next(20, 225), random.Next(20, 225));
            FloodFill(origin, tolerance, fill);
            return fill;
        }

        /// <summary>
        /// Flood fill from a given point in the image.
        /// </summary>
        private Color FloodFill(Point origin, int tolerance, ref bool[,] filledSquares)
        {
            Color fill = Color.FromArgb(random.Next(20, 225), random.Next(20, 225), random.Next(20, 225));
            FloodFill(origin, tolerance, fill, ref filledSquares);
            return fill;
        }

        /// <summary>
        /// Fill a color into a region of an image within a certain tolerance.
        /// </summary>
        /// <param name="origin">The point to start filling from</param>
        /// <param name="tolerance">The amount of difference naighboring pixels can have and still be considered part of the same group</param>
        /// <param name="fillColor">The color to fill with</param>
        public void FloodFill(Point origin, int tolerance, Color fillColor)
        {
            try
            {
                bool[,] filledSquares = new bool[Image.Width, Image.Height];
                FloodFill(origin, tolerance, fillColor, ref filledSquares);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error trying to flood fill from a point on an image.", ex);
            }
        }

        /// <summary>
        /// Flood fill from a given point in the image.
        /// </summary>
        private void FloodFill(Point origin, int tolerance, Color fillColor, ref bool[,] filledSquares)
        {
            Color initialColor = Image.GetPixel(origin.X, origin.Y);
            BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            unsafe
            {
                byte* scan0 = (byte*)(void*)Scan0;
                int stride = bmpData.Stride;

                bool[,] doneChecking = new bool[Image.Width, Image.Height];
                Queue<Point> nextPoints = new Queue<Point>();

                // Fill the initial pixel
                FloodFillPoint(scan0, stride, origin, Image.Width, Image.Height, initialColor, tolerance, fillColor, doneChecking, nextPoints, ref filledSquares);

                // Fill pixels in the queue until the queue is empty
                while (nextPoints.Count > 0)
                {
                    Point next = nextPoints.Dequeue();
                    FloodFillPoint(scan0, stride, next, Image.Width, Image.Height, initialColor, tolerance, fillColor, doneChecking, nextPoints, ref filledSquares);
                }
            }

            Image.UnlockBits(bmpData);
        }

        /// <summary>
        /// Using the flood fill algorithm, count the number of pixels that WOULD have been filled if we were actually filling.
        /// Great for counting the number of pixels in a blob.
        /// </summary>
        /// <param name="origin">The place to start counting from</param>
        /// <param name="tolerance">How far off a pixel can be and still be counted as part of the same group</param>
        /// <param name="cutoff">If you count this many pixels, then stop counting. NOTE: This will prevent a valid bounding box from being returned!</param>
        /// <returns></returns>
        private BlobCount FloodCount(Point origin, int tolerance, int cutoff = -1, bool[,] doneChecking = null)
        {
            Color initialColor = Image.GetPixel(origin.X, origin.Y);
            BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            int count = 0;
            Point upperLeft = origin;
            Point lowerRight = origin;

            unsafe
            {
                byte* scan0 = (byte*)(void*)Scan0;
                int stride = bmpData.Stride;

                if (doneChecking == null)
                {
                    doneChecking = new bool[Image.Width, Image.Height];
                }
                Queue<Point> nextPoints = new Queue<Point>();

                // Fill the initial pixel
                FloodFillPoint(scan0, stride, origin, Image.Width, Image.Height, initialColor, tolerance, Color.White, doneChecking, nextPoints, ref doneChecking, true);

                // Fill pixels in the queue until the queue is empty
                while (nextPoints.Count > 0)
                {
                    Point next = nextPoints.Dequeue();
                    if (FloodFillPoint(scan0, stride, next, Image.Width, Image.Height, initialColor, tolerance, Color.White, doneChecking, nextPoints, ref doneChecking, true))
                    {
                        upperLeft.X = Math.Min(upperLeft.X, next.X);
                        upperLeft.Y = Math.Min(upperLeft.Y, next.Y);
                        lowerRight.X = Math.Max(lowerRight.X, next.X);
                        lowerRight.Y = Math.Max(lowerRight.Y, next.Y);

                        count++;
                    }

                    if (cutoff > 0 && count > cutoff)
                    {
                        break;
                    }
                }
            }

            Image.UnlockBits(bmpData);

            return new BlobCount() { PixelCount = count, BlobBounds = new Rectangle(upperLeft.X, upperLeft.Y, lowerRight.X - upperLeft.X, lowerRight.Y - upperLeft.Y) };
        }

        /// <summary>
        /// Flood fill a certain color starting at a given point using a Breadth First Search (BFS).
        /// </summary>
        private unsafe bool FloodFillPoint(byte* p, int stride, Point origin, int imageW, int imageH, Color startColor, int tolerance,
            Color fillColor, bool[,] doneChecking, Queue<Point> nextPoints, ref bool[,] floodFilled, bool fakeFill = false)
        {
            int ind = origin.Y * stride + origin.X * 4; // TODO: make sure all index operations multiply by 4 and not 3!

            if (!doneChecking[origin.X, origin.Y] && GetDifference(startColor, p, ind) <= tolerance)
            {
                // Mark this pixel as checked
                doneChecking[origin.X, origin.Y] = true;

                // Fill the color in
                if (!fakeFill)
                {
                    p[ind + 0] = fillColor.B;
                    p[ind + 1] = fillColor.G;
                    p[ind + 2] = fillColor.R;

                    floodFilled[origin.X, origin.Y] = true;
                }

                // Queue up the neighboring 4 pixels
                nextPoints.Enqueue(new Point((origin.X + 1) % imageW, origin.Y));
                nextPoints.Enqueue(new Point((origin.X - 1 + imageW) % imageW, origin.Y));
                nextPoints.Enqueue(new Point(origin.X, (origin.Y + 1) % imageH));
                nextPoints.Enqueue(new Point(origin.X, (origin.Y - 1 + imageH) % imageH));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the difference between two pixels in an unsafe context
        /// </summary>
        private unsafe int GetDifference(Color c, byte* b, int index)
        {
            return (int)Math.Max(Math.Max(Math.Abs(b[index + 0] - c.B), Math.Abs(b[index + 1] - c.G)), Math.Abs(b[index + 2] - c.R));
        }

        /// <summary>
        /// Remove blobs (fill with the background color) from an image under certain constraints.
        /// </summary>
        /// <param name="minimumBlobSize">The smallest number of pixels a blob can be made of</param>
        /// <param name="minimumBlobWidth">The smallest width a blob can be</param>
        /// <param name="minimumBlobHeight">The smallest height a blob can be</param>
        /// <param name="backgroundColor">The color to fill small blobs with</param>
        public void RemoveSmallBlobs(int minimumBlobSize, int minimumBlobWidth, int minimumBlobHeight, Color backgroundColor)
        {
            RemoveSmallBlobs(minimumBlobSize, minimumBlobWidth, minimumBlobHeight, backgroundColor, 2);
        }

        /// <summary>
        /// Remove blobs (fill with the background color) from an image under certain constraints.
        /// </summary>
        /// <param name="minimumBlobSize">The smallest number of pixels a blob can be made of</param>
        /// <param name="minimumBlobWidth">The smallest width a blob can be</param>
        /// <param name="minimumBlobHeight">The smallest height a blob can be</param>
        /// <param name="backgroundColor">The color to fill small blobs with</param>
        /// <param name="colorTolerance">The RGB tolerance in color when flood filling</param>
        public void RemoveSmallBlobs(int minimumBlobSize, int minimumBlobWidth, int minimumBlobHeight, Color backgroundColor, int colorTolerance)
        {
            try
            {
                // This will prevent us from attempting to count a blob of N pixels N times (assuming N < minimumBlobSize, otherwise it would be filled)
                bool[,] done = new bool[Image.Width, Image.Height];

                for (int x = 0; x < Image.Width; x++)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        // Ignore the background
                        if (!done[x, y] && Image.GetPixel(x, y).Subtract(backgroundColor) >= colorTolerance)
                        {
                            // See how big of a blob there is here
                            BlobCount blob = FloodCount(new Point(x, y), colorTolerance, doneChecking: done);

                            // If it's small enough, fill it with the background color
                            if (blob.PixelCount < minimumBlobSize || blob.BlobBounds.Width < minimumBlobWidth || blob.BlobBounds.Height < minimumBlobHeight)
                            {
                                FloodFill(new Point(x, y), colorTolerance, backgroundColor);
                                // DEBUG: Color.FromArgb(Math.Min(255, blob.PixelCount), Math.Min(255, blob.PixelCount), Math.Min(255, blob.PixelCount)));
                            }
                        }
                    }
                }

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error trying to remove small blobs from the image.", ex);
            }
        }

        /// <summary>
        /// Fill each unique blob in an image with a random color.
        /// A group of adjacent pixels is considered a single blob when they are all similar to each other in the L*a*b* color space below a given threshold.
        /// In the L*a*b* color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye."
        /// </summary>
        /// <param name="tolerance">The Delta E difference between two (L*a*b*) colors to allow when filling a blob.</param>
        /// <param name="background">The color of the background</param>
        /// <param name="backgroundTolerance">The Delta E difference between a pixel (L*a*b*) and the background to allow when filling.</param>
        public void ColorFillBlobs(double tolerance, Color background, double backgroundTolerance)
        {
            try
            {
                byte[,][] colors2 = new byte[Image.Width, Image.Height][];

                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                            // Store in BGR order
                            colors2[x, y] = new byte[] { p[0], p[1], p[2] };
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                int similarNeighborPixels;
                int pixelRadius = 1;

                for (int x = pixelRadius; x < Image.Width - pixelRadius; x++)
                {
                    for (int y = pixelRadius; y < Image.Height - pixelRadius; y++)
                    {
                        if (!alreadyFilled[x, y])
                        {
                            if (colors2[x, y].GetEDeltaColorDifference(background) > backgroundTolerance)
                            {
                                similarNeighborPixels = 0;
                                for (int xv = -pixelRadius; xv <= pixelRadius; xv++)
                                {
                                    for (int yv = -pixelRadius; yv <= pixelRadius; yv++)
                                    {
                                        if (yv != 0 || xv != 0)
                                        {
                                            if (colors2[x, y].GetEDeltaColorDifference(colors2[x + xv, y + yv]) < tolerance)
                                            {
                                                similarNeighborPixels++;
                                            }
                                        }
                                    }
                                }

                                if (similarNeighborPixels >= ((pixelRadius * 2 + 1) * (pixelRadius * 2 + 1)) - 1)
                                {
                                    FloodFill(new Point(x, y), (int)tolerance, ref alreadyFilled);
                                }
                            }
                            else
                            {
                                Image.SetPixel(x, y, background);
                            }
                        }
                    }
                }

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error trying to fill blobs in an image with random colors.", ex);
            }
        }

        /// <summary>
        /// Convert every pixel value below a threshold to black and every pixel above to white.
        /// </summary>
        /// <param name="threshold">The threshold to divide by. [Range: 0 to 255]</param>
        public void Binarize(int threshold)
        {
            if (threshold < 0 || threshold > 255)
            {
                throw new ImageProcessingException("The threshold to Binarize must be between 0 and 255!");
            }

            try
            {
                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                            if ((p[0] * p[1] * p[2]) / (255 * 255) < threshold && (x != 0 && y != 0 && x != Image.Width - 1 && y != Image.Height - 1))
                            {
                                p[0] = 0;
                                p[1] = 0;
                                p[2] = 0;
                            }
                            else
                            {
                                p[0] = 255;
                                p[1] = 255;
                                p[2] = 255;
                            }
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error binarizing an image.", ex);
            }
        }

        /// <summary>
        /// White out all pixels that are not a color (any shade of grey)
        /// </summary>
        /// <param name="distance">The threshold distance from a color that a pixel can be and be considered not a color.</param>
        public void RemoveNonColor(int distance)
        {
            try
            {
                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                Func<byte, byte, byte, bool> IsColor = (r, g, b) => Math.Sqrt((r - g) * (r - g) + (g - b) * (g - b) + (b - r) * (b - r)) > distance;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                            if (!IsColor(p[0], p[1], p[2]))
                            {
                                p[0] = 255;
                                p[1] = 255;
                                p[2] = 255;
                            }
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error binarizing an image.", ex);
            }
        }

        private Color GetMostCommonColor(double tolerance, double bkgTol)
        {
            List<ColorCount> colors = new List<ColorCount>();
            ColorCount background = new ColorCount() { R = 255, G = 255, B = 255 };

            BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - Image.Width * 3;

                for (int y = 0; y < Image.Height; ++y)
                {
                    for (int x = 0; x < Image.Width; ++x)
                    {
                        ColorCount temp = new ColorCount() { R = p[2], G = p[1], B = p[0] };

                        if (background.GetLABDist(temp) > bkgTol)
                        {
                            int bestindex = -1;
                            double best = Double.MaxValue;
                            for (int i = 0; i < colors.Count; i++)
                            {
                                double test = colors[i].GetLABDist(temp);
                                if (test < best && test < tolerance)
                                {
                                    best = test;
                                    bestindex = i;
                                }
                            }
                            if (bestindex != -1 && colors.Count > 0)
                            {
                                colors[bestindex].FoundOne();
                            }
                            else
                            {
                                colors.Add(temp);
                            }
                        }

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            Image.UnlockBits(bmData);

            colors.Sort((c, n) => -c.Count.CompareTo(n.Count));
            return colors.FirstOrDefault().Color;
        }

        public void KeepOnlyMostCommonColor(double tolerance)
        {
            try
            {
                Color mostCommon = GetMostCommonColor(tolerance, tolerance * 1.5);

                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                            if (mostCommon.GetEDeltaColorDifference(p[2], p[1], p[0]) > tolerance)
                            {
                                p[2] = 255;
                                p[1] = 255;
                                p[0] = 255;
                            }
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error keeping the most common color of an image.", ex); 
            }
        }

        /// <summary>
        /// Invert the image
        /// </summary>
        public void Invert()
        {
            try
            {
                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                bool[,] alreadyFilled = new bool[Image.Width, Image.Height];

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - Image.Width * 3;

                    for (int y = 0; y < Image.Height; ++y)
                    {
                        for (int x = 0; x < Image.Width; ++x)
                        {
                                p[0] = (byte)(255 - p[0]);
                                p[1] = (byte)(255 - p[1]);
                                p[2] = (byte)(255 - p[2]);

                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                Image.UnlockBits(bmData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error inverting an image.", ex);
            }
        }

        /// <summary>
        /// Convert the image to black and white, where anything not white turns black (even the color #FEFEFE).
        /// If you need to choose the threshold yourself, then use Binarize().
        /// </summary>
        public void BlackAndWhite()
        {
            Binarize(254);
        }

        /// <summary>
        /// Perform a Convolution that will create a form of outlining.
        /// </summary>
        public void Outline()
        {
            ConvolutionFilter(0, 1, 0, 1, -4, 1, 0, 1, 0);
        }

        /// <summary>
        /// Erode all of the edges of the blobs in an image by one pixel. The image must have a solid background by this point.
        /// </summary>
        /// <param name="background">The background color of the image.</param>
        public void ErodeShapes(Color background)
        {
            try
            {
                bool[,] adjacent = new bool[Image.Width, Image.Height];

                BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int stride = bmpData.Stride;
                    int[] i;

                    for (int x = 1; x < Image.Width - 1; x++)
                    {
                        for (int y = 1; y < Image.Height - 1; y++)
                        {
                            // Don't need to blank out an already blanked out square
                            int index = y * stride + x * 4;
                            if (p[index + 0] != background.B || p[index + 1] != background.G || p[index + 2] != background.R)
                            {
                                // Get the indexes of 4 adjacent pixels (I add zero for clarity and change-ability)
                                i = new int[] { (y + 1) * stride + (x + 0) * 4, (y - 1) * stride + (x + 0) * 4, (y + 0) * stride + (x + 1) * 4, (y + 0) * stride + (x - 1) * 4 };

                                for (int z = 0; z < 4 && !adjacent[x, y]; z++)
                                {
                                    // See if the pixel is the background color
                                    if (p[i[z] + 0] == background.B && p[i[z] + 1] == background.G && p[i[z] + 2] == background.R)
                                    {
                                        // This pixel is close to a background pixel, so mark it for deletion
                                        adjacent[x, y] = true;
                                    }
                                }
                            }
                        }
                    }

                    // Now go through and color all the adjacent pixels with the background color
                    for (int x = 1; x < Image.Width - 1; x++)
                    {
                        for (int y = 1; y < Image.Height - 1; y++)
                        {
                            if (adjacent[x, y])
                            {
                                int index = y * stride + x * 4;
                                p[index + 0] = background.B;
                                p[index + 1] = background.G;
                                p[index + 2] = background.R;
                            }
                        }
                    }
                }

                Image.UnlockBits(bmpData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error eroding the edges of an image.", ex);
            }
        }

        public void EdgePreservingSmooth()
        {
            BilateralSmoothing filter = new BilateralSmoothing();
            filter.KernelSize = 7;
            filter.SpatialFactor = 10;
            filter.ColorFactor = 60;
            filter.ColorPower = 0.5;
            filter.ApplyInPlace(Image);
        }

        public void Median()
        {
            new Median().ApplyInPlace(Image);
        }

        public void GrowShapes(Color background)
        {
            try
            {
                bool[,] adjacent = new bool[Image.Width, Image.Height];

                BitmapData bmpData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int stride = bmpData.Stride;
                    int[] i;

                    for (int x = 1; x < Image.Width - 1; x++)
                    {
                        for (int y = 1; y < Image.Height - 1; y++)
                        {
                            int index = y * stride + x * 4;
                            //if (background.GetEDeltaColorDifference(p[index + 0], p[index + 1], p[index + 2]) < 3.0)
                            //{
                                // Get the indexes of 4 adjacent pixels (I add zero for clarity and change-ability)
                                i = new int[] { (y + 1) * stride + (x + 0) * 4, (y - 1) * stride + (x + 0) * 4, (y + 0) * stride + (x + 1) * 4, (y + 0) * stride + (x - 1) * 4,
                                (y + 1) * stride + (x + 1) * 4, (y - 1) * stride + (x + 1) * 4, (y + 1) * stride + (x + 1) * 4, (y + 1) * stride + (x - 1) * 4};

                                for (int z = 0; z < 8 && !adjacent[x, y]; z++)
                                {
                                    if (p[i[z] + 0] != background.B || p[i[z] + 1] != background.G || p[i[z] + 2] != background.R)
                                    {
                                        // This pixel is close to a colored pixel, so mark it for filling
                                        adjacent[x, y] = true;
                                    }
                                }
                            //}
                        }
                    }

                    // Now go through and color all the adjacent pixels
                    for (int x = 1; x < Image.Width - 1; x++)
                    {
                        for (int y = 1; y < Image.Height - 1; y++)
                        {
                            if (adjacent[x, y])
                            {
                                int index = y * stride + x * 4;
                                p[index + 0] = 0;
                                p[index + 1] = 0;
                                p[index + 2] = 0;
                            }
                        }
                    }
                }

                Image.UnlockBits(bmpData);

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error eroding the edges of an image.", ex);
            }
        }

        /// <summary>
        /// Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).
        /// </summary>
        public void ResizeRotateCut()
        {
            ResizeRotateCut(false);
        }

        /// <summary>
        /// Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).
        /// </summary>
        /// <param name="drawHistogramBarsBecauseTheyreCool"> 
        /// Saying TRUE here makes the image unusable, but you can save the image to a file and it looks cool with all the pretty histogram lines on it...
        /// </param>
        public void ResizeRotateCut(bool drawHistogramBarsBecauseTheyreCool)
        {
            try
            {
                Bitmap copy = new Bitmap(Image.Width, Image.Height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(copy);
                g.DrawImage(Image, 0, 0, Image.Width, Image.Height);

                Bitmap rot;
                int best = copy.Height;
                double ang = 0.0;

                copy = copy.CropResize();

                for (double d = -45.0; d <= 45.0; d += 5.0)
                {
                    RotateBicubic r = new RotateBicubic(d);
                    r.FillColor = Color.White;
                    r.KeepSize = false;
                    rot = r.Apply(copy);

                    int hist = GetHistogram(rot, 7);

                    if (hist < best)
                    {
                        best = hist;
                        ang = d;
                    }

                    //Pen p1 = new Pen(new SolidBrush(Color.FromArgb(128, Color.Red)));
                    //Pen p2 = new Pen(new SolidBrush(Color.FromArgb(128, Color.LimeGreen)));
                    //Bitmap rot2 = new Bitmap(600, 600);
                    //using (Graphics g2 = Graphics.FromImage(rot2))
                    //{
                    //    g2.Clear(Color.White);
                    //    g2.DrawImage(rot, rot2.Width / 2 - rot.Width / 2, rot2.Height - rot.Height);
                    //    for (int x = 0; x < rot2.Width; x++)
                    //    {
                    //        int count = 0;
                    //        for (int y = 0; y < rot2.Height; y++)
                    //        {
                    //            if (rot2.GetPixel(x, y).Subtract(Color.White) > 10)
                    //            {
                    //                count++;
                    //            }
                    //        }
                    //        if (count > rot2.Height / 15)
                    //        {
                    //            double percent = (double)count / rot2.Height;
                    //            g2.DrawLine(p1, x, (int)((rot2.Height - 1) * (1.0 - percent)), x, rot2.Height - 1);
                    //        }
                    //        else
                    //        {
                    //            g2.DrawLine(p2, x, 0, x, rot2.Height - 1);
                    //        }
                    //    }
                    //    g2.DrawString(ang.ToString("0.00") + " degrees", new Font("Tahoma", 8.25f), Brushes.Black, new PointF(1.0f, 1.0f));
                    //}
                    //rot2.Save("zhist " + (90 - (45 + d)).ToString("00") + ".png");

                    GlobalMessage.SendMessage(rot);
                }

                RotateBicubic r2 = new RotateBicubic(ang);
                r2.FillColor = Color.White;
                r2.KeepSize = false;
                rot = r2.Apply(copy);

                GetHistogram(rot, 7);
                rot = rot.CropResize();

                if (drawHistogramBarsBecauseTheyreCool)
                {
                    Pen p1 = new Pen(new SolidBrush(Color.FromArgb(128, Color.Red)));
                    Pen p2 = new Pen(new SolidBrush(Color.FromArgb(128, Color.LimeGreen)));

                    using (Graphics g2 = Graphics.FromImage(rot))
                    {
                        for (int x = 0; x < rot.Width; x++)
                        {
                            int count = 0;
                            for (int y = 0; y < rot.Height; y++)
                            {
                                if (rot.GetPixel(x, y).Subtract(Color.White) > 10)
                                {
                                    count++;
                                }
                            }

                            if (count > rot.Height / 15)
                            {
                                double percent = (double)count / rot.Height;
                                g2.DrawLine(p1, x, (int)((rot.Height - 1) * (1.0 - percent)), x, rot.Height - 1);
                            }
                            else
                            {
                                g2.DrawLine(p2, x, 0, x, rot.Height - 1);
                            }
                        }

                        g2.DrawString(ang.ToString("0.00") + " degrees", new Font("Tahoma", 8.25f), Brushes.Black, new PointF(1.0f, 1.0f));
                    }
                }

                Image = rot;

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error rotating an image to find the best vertical histogram.", ex);
            }
        }

        /// <summary>
        /// Subtract a static image from the CAPTCHA.
        /// Some weak CAPTCHAs have a static textured background that can simply be subtracted from the image in this way.
        /// </summary>
        /// <param name="image"></param>
        public void Subtract(Bitmap image)
        {
            if (image.Width != Image.Width || image.Height != Image.Height)
            {
                throw new ImageProcessingException("The image you are subtracting from the CAPTCHA must be the same size as the CAPTCHA!");
            }

            try
            {
                Color c, i;
                for (int x = 0; x < Image.Width; x++)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        c = Image.GetPixel(x, y);
                        i = image.GetPixel(x, y);
                        Image.SetPixel(x, y, Color.FromArgb(Math.Abs(c.R - i.R), Math.Abs(c.G - i.G), Math.Abs(c.B - i.B)));
                    }
                }

                GlobalMessage.SendMessage(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error subtracting one image from another.", ex);
            }
        }

        private int GetHistogram(Bitmap img, int number, bool draw = false)
        {
            // Speed with GetPixel: 144ms Average
            // Speed with unsafe:  17ms  Average 

            BitmapData bmData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int[] hist = new int[img.Width];
            for (int x = 0; x < img.Width; x++)
            {
                hist[x] = 0;
            }

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - img.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < img.Height; ++y)
                {
                    for (int x = 0; x < img.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        if (!(p[0] > 245 && p[1] > 245 && p[2] > 245))
                        {
                            // Far enough away from white to count it
                            hist[x]++;
                        }

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            img.UnlockBits(bmData);

            int[] arr2 = new int[img.Width];
            for (int x = 0; x < img.Width; ++x)
            {
                if (hist[x] != 0 || (x > 0 && (hist[x] == 0 && hist[x - 1] != 0)))
                {
                    arr2[x] = hist[x];
                }
                else
                {
                    arr2[x] = img.Height;
                }
            }

            Array.Sort(arr2);

            return arr2[number - 1];
        }

        /// <summary>
        /// Perform a specified operation on each pixel in the image
        /// </summary>
        /// <param name="eachPixel">The function to perform on each pixel</param>
        public void ForEachPixel(Func<Color, Color> eachPixel)
        {
            try
            {
                BitmapData bmData = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    for (int x = 1; x < Image.Width - 1; x++)
                    {
                        for (int y = 1; y < Image.Height - 1; y++)
                        {
                            int index = y * stride + x * 4;

                            Color temp = Color.FromArgb(p[index + 2], p[index + 1], p[index]);
                            temp = eachPixel.Invoke(temp);

                            p[index + 2] = temp.R;
                            p[index + 1] = temp.G;
                            p[index + 0] = temp.B;
                        }
                    }
                }

                Image.UnlockBits(bmData);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error processing an image with a specified per-pixel delegate.", ex);
            }
        }

        /// <summary>
        /// Apply a convolution filter to the image.
        /// </summary>
        public void ConvolutionFilter(params int[] kernel)
        {
            if (kernel.Length == 9)
            {
                ConvolutionFilter(new int[,] { { kernel[0], kernel[1], kernel[2] },
                                               { kernel[3], kernel[4], kernel[5] },
                                               { kernel[6], kernel[7], kernel[8] } });
            }
            else
            {
                throw new ImageProcessingException("You must specify 9 values for the Convolution Filter!");
            }
        }

        /// <summary>
        /// Apply a convolution filter to the image.
        /// </summary>
        /// <param name="m">The 3x3 matrix containing the convolution to use.</param>
        private void ConvolutionFilter(int[,] kernel)
        {
            try
            {
                new Convolution(kernel).ApplyInPlace(Image);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingException("Error attempting to apply a convolution filter to an image!", ex);
            }
        }
    }

    /// <summary>
    /// Represents the result of analysing a blob in an image
    /// </summary>
    public struct BlobCount
    {
        /// <summary>
        /// Number of pixels in this blob
        /// </summary>
        public int PixelCount { get; set; }

        /// <summary>
        /// The bounding box of the blob found
        /// </summary>
        public Rectangle BlobBounds { get; set; }
    }

    public class ColorCount
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        private byte[] array { get { return new byte[] { R, G, B }; } }
        public int Count { get; set; }
        public Color Color { get { return Color.FromArgb(R, G, B); } }

        public double GetLABDist(ColorCount other)
        {
            return array.GetEDeltaColorDifference(other.array);
        }

        public void FoundOne()
        {
            Count++;
        }

        public new string ToString()
        {
            return Count.ToString();
        }
    }
}
