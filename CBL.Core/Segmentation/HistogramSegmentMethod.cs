using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScottClayton.Image
{
    public class HistogramSegmentMethod : SegmentMethod
    {
        public int Tolerance { get; set; }

        public int NumChars { get; set; }

        public HistogramSegmentMethod(int tolerance)
            : this(tolerance, -1)
        {
        }

        public HistogramSegmentMethod(int tolerance, int numChars)
        {
            Tolerance = tolerance;
            NumChars = numChars;
        }

        public override SegmentMethod Clone()
        {
            return new HistogramSegmentMethod(Tolerance, NumChars);
        }

        public override List<Bitmap> Segment(Bitmap image)
        {
            List<Bitmap> images = new List<Bitmap>();
            int startIndex = -1;
            int sum = 0;

            // TODO: Rewrite without using the super-slow Bitmap.GetPixel()

            for (int x = 0; x < image.Width; x++)
            {
                int count = 0;
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y).Subtract(Color.White) > 10)
                    {
                        count++;
                    }
                }

                if (count <= Tolerance)
                {
                    if (startIndex >= 0)
                    {
                        // We just finished scanning over a block
                        Bitmap chunk = new Bitmap(x - startIndex, image.Height);
                        using (Graphics g = Graphics.FromImage(chunk))
                        {
                            g.DrawImage(image, new Rectangle(0, 0, x - startIndex, image.Height), new Rectangle(startIndex, 0, x - startIndex, image.Height), GraphicsUnit.Pixel);
                        }

                        sum += chunk.Width;

                        // Cut off the whitespace above and below the letter
                        images.Add(chunk.CropResize());
                        sum += chunk.Width;
                        
                        startIndex = -1;
                    }
                    // else - We are still looking for the beginning of a block
                }
                else
                {
                    if (startIndex < 0)
                    {
                        // We are looking for another block, and we found one
                        startIndex = x;
                    }
                }
            }

            if (NumChars > 0)
            {
                while (images.Count > NumChars)
                {
                    int index = 0;
                    int smallest = -1;
                    for (int i = 0; i < images.Count; i++)
                    {
                        int test = images[i].Width * images[i].Height;
                        if (smallest == -1 || test < smallest)
                        {
                            smallest = test;
                            index = i;
                        }
                    }
                    images.RemoveAt(index);
                }

                while (images.Count < NumChars)
                {
                    for (double i = 3.0; i > 0 && images.Count < NumChars; i -= 0.1)
                    {
                        //double i = 1.3;
                        for (int b = 0; b < images.Count; b++)
                        {
                            double acceptable = (((double)image.Width / (double)NumChars) * i);
                            if (images[b].Width > acceptable)
                            {
                                int ct = (int)Math.Round((double)images[b].Width / ((double)sum / (double)NumChars));
                                Bitmap[] splits = SplitInto(images[b], ct < 2 ? 2 : ct);
                                images.RemoveAt(b);

                                int xx = 0;
                                foreach (Bitmap b2 in splits)
                                {
                                    images.Insert(b + (xx++), b2.CropResize());
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return images;
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
    }
}
