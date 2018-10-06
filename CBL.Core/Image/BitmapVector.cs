using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using ScottClayton.Neural;

namespace ScottClayton.CAPTCHA.Image
{
    class BitmapVector
    {
        private double[,] matrix;

        public string Information { get; set; }

        public double this[int x, int y]
        {
            get
            {
                return matrix[x, y];
            }
            set
            {
                matrix[x, y] = value;
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public BitmapVector()
        {
            Width = 0;
            Height = 0;
            matrix = new double[Width, Height];
            Information = "";
        }

        public BitmapVector(int width, int height)
        {
            Width = width;
            Height = height;
            matrix = new double[Width, Height];
            Fill(0.0);
            Information = "";
        }

        public BitmapVector(Bitmap image, int width, int height)
        {
            Width = width;
            Height = height;
            matrix = new double[Width, Height];

            Bitmap resized = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(resized);
            g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    matrix[x, y] = (double)resized.GetPixel(x, y).R / 255.0;
                }
            }

            Information = "";
        }

        public BitmapVector(Bitmap image, int width, int height, string information)
            : this(image, width, height)
        {
            Information = information;
        }

        public BitmapVector Subtract(BitmapVector other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] -= other[x, y];
                }
            }

            return sub;
        }

        public BitmapVector Subtract(double other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] -= other;
                }
            }

            return sub;
        }

        public BitmapVector Add(BitmapVector other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] += other[x, y];
                }
            }

            return sub;
        }

        public BitmapVector Add(double other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] += other;
                }
            }

            return sub;
        }

        public BitmapVector Divide(BitmapVector other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] /= other[x, y];
                }
            }

            return sub;
        }

        public BitmapVector Divide(double other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] /= other;
                }
            }

            return sub;
        }

        public BitmapVector Multiply(BitmapVector other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] *= other[x, y];
                }
            }

            return sub;
        }

        public BitmapVector Multiply(double other)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] *= other;
                }
            }

            return sub;
        }

        public BitmapVector Pow(double exp)
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] = Math.Pow(sub[x, y], exp);
                }
            }

            return sub;
        }

        /// <summary>
        /// Scales every value to between 0.0 and 1.0
        /// </summary>
        public BitmapVector Scale()
        {
            BitmapVector done = this.Clone();

            double s = done.Min();
            double b = done.Max();

            return (done - s) / (b - s);
        }

        public double Max()
        {
            double biggest = matrix[0, 0];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    biggest = Math.Max(biggest, matrix[x, y]);
                }
            }

            return biggest;
        }

        public double Min()
        {
            double smallest = matrix[0, 0];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    smallest = Math.Min(smallest, matrix[x, y]);
                }
            }

            return smallest;
        }

        public BitmapVector Abs()
        {
            BitmapVector sub = this.Clone();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sub[x, y] = Math.Abs(matrix[x, y]);
                }
            }

            return sub;
        }

        static public BitmapVector operator -(BitmapVector a, BitmapVector b)
        {
            return a.Subtract(b);
        }

        static public BitmapVector operator -(BitmapVector a, double b)
        {
            return a.Subtract(b);
        }

        static public BitmapVector operator +(BitmapVector a, BitmapVector b)
        {
            return a.Add(b);
        }

        static public BitmapVector operator +(BitmapVector a, double b)
        {
            return a.Add(b);
        }

        static public BitmapVector operator *(BitmapVector a, BitmapVector b)
        {
            return a.Multiply(b);
        }

        static public BitmapVector operator *(BitmapVector a, double b)
        {
            return a.Multiply(b);
        }

        static public BitmapVector operator /(BitmapVector a, BitmapVector b)
        {
            return a.Divide(b);
        }

        static public BitmapVector operator /(BitmapVector a, double b)
        {
            return a.Divide(b);
        }

        static public explicit operator BitmapVector(DoubleVector v)
        {
            // ONLY SQUARE DOUBLEVECTORS WILL WORK HERE

            int width = (int)Math.Sqrt(v.Size);
            BitmapVector b = new BitmapVector(width, width);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    b.matrix[x, y] = v[y + x * width];
                }
            }

            return b;
        }

        static public BitmapVector ConvertToLinearBitmapVector(DoubleVector v)
        {
            int width = v.Size;
            BitmapVector b = new BitmapVector(width, 1);

            for (int x = 0; x < width; x++)
            {
                b.matrix[x, 0] = v[x];
            }

            return b;
        }

        public Bitmap GetBitmap()
        {
            Bitmap b = new Bitmap(Width, Height);

            BitmapVector c = this.Clone();
            c = c.Scale();

            if (!c.matrix[0, 0].Equals(double.NaN))
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        try
                        {
                            b.SetPixel(x, y, Color.FromArgb((int)(c.matrix[x, y] * 255.0), (int)(c.matrix[x, y] * 255.0), (int)(c.matrix[x, y] * 255.0)));
                        }
                        catch (ArgumentException)
                        {
                            // TODO - c is usually filled with NaN if you get here
                        }
                    }
                }
            }
            else
            {
                // TODO - hmmm...
            }

            return b;
        }

        public double GetValue()
        {
            double value = 0.0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    value += Math.Abs(matrix[x, y]);
                }
            }

            return value;
        }

        public double GetAverageValue()
        {
            double value = 0.0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    value += Math.Abs(matrix[x, y]);
                }
            }

            return value / (Width * Height);
        }

        public static double RootMeanSquareDistance(BitmapVector left, BitmapVector right)
        {
            double value = 0.0;

            for (int x = 0; x < left.Width; x++)
            {
                for (int y = 0; y < left.Height; y++)
                {
                    value += Math.Pow(left.matrix[x, y] - right.matrix[x, y], 2.0);
                }
            }

            return Math.Sqrt(value / (left.Width * left.Height));
        }

        public BitmapVector Clone()
        {
            BitmapVector copy = new BitmapVector(Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    copy[x, y] = matrix[x, y];
                }
            }

            copy.Information = Information;

            return copy;
        }

        public void Fill(double value)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    matrix[x, y] = value;
                }
            }
        }

        public BitmapVector Invert()
        {
            BitmapVector copy = new BitmapVector(Width, Height);
            double max = this.Max();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    copy[x, y] = max - matrix[x, y];
                }
            }

            copy.Information = Information;

            return copy;
        }

        public BitmapVector Binarize()
        {
            BitmapVector copy = new BitmapVector(Width, Height);
            double max = this.Max();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    copy[x, y] = matrix[x, y] >= 0.5 ? 1.0 : 0.0;
                }
            }

            copy.Information = Information;

            return copy;
        }

        #region Persistence Operations

        public void SaveToFile(BinaryWriter w)
        {
            w.Write(Width);
            w.Write(Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    w.Write(matrix[x, y]);
                }
            }

            w.Write(Information);
        }

        public static BitmapVector LoadFromFile(BinaryReader r)
        {
            int width = r.ReadInt32();
            int height = r.ReadInt32();

            BitmapVector v = new BitmapVector(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    v.matrix[x, y] = r.ReadDouble();
                }
            }

            v.Information = r.ReadString();

            return v;
        }

        #endregion
    }

    static class BitmapVectorExtensions
    {
        public static BitmapVector Average(this List<BitmapVector> vectors)
        {
            BitmapVector avg = vectors[0].Clone();

            for (int i = 1; i < vectors.Count; i++)
            {
                avg += vectors[i];
            }

            return avg / vectors.Count;
        }
    }
}
