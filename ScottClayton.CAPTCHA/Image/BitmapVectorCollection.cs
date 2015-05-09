using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ScottClayton.CAPTCHA.Image
{
    class BitmapVectorCollection
    {
        private List<BitmapVector> vectors;

        public int Count
        {
            get { return vectors.Count; }
        }

        public BitmapVectorCollection()
        {
            vectors = new List<BitmapVector>();
        }

        public void Add(BitmapVector v)
        {
            vectors.Add(v);
        }

        public void AddToGroup(BitmapVector v)
        {
            bool added = false;
            for (int i = 0; i < vectors.Count; i++)
            {
                if (vectors[i].Information == v.Information)
                {
                    vectors[i] += v;

                    added = true;
                    break;
                }
            }
            if (!added)
            {
                vectors.Add(v);
            }
        }

        public void Add(Bitmap image)
        {
            BitmapVector v = new BitmapVector(image, image.Width, image.Height);
            vectors.Add(v);
        }

        public void Add(List<BitmapVector> v)
        {
            vectors.AddRange(v);
        }

        public void Add(List<Bitmap> images)
        {
            foreach (Bitmap b in images)
                Add(b);
        }

        /// <summary>
        /// Adds a BitmapVector to the list of vectors created by resizing and averaging all input images into one. 
        /// The dimensions passed are for the output vector, and not the input images.
        /// </summary>
        /// <param name="images">The list of images to make a vector out of.</param>
        /// <param name="width">The width to make the vector.</param>
        /// <param name="height">The height to make the vector.</param>
        /// <param name="information">Any information that you would like to associate with this BitmapVector.</param>
        public void AddBitmapArrayAsOneVector(List<Bitmap> images, int width, int height, string information)
        {
            BitmapVector v = new BitmapVector(images[0], width, height);

            for (int i = 1; i < images.Count; i++)
            {
                v += new BitmapVector(images[i], width, height);
            }

            v /= images.Count;
            v.Information = information;

            Add(v);
        }

        public BitmapVector GetClosestMatch(Bitmap image)
        {
            BitmapVector v = new BitmapVector(image, vectors[0].Width, vectors[0].Height);
            return GetClosestMatch(v);
        }

        public BitmapVector GetClosestMatch(BitmapVector v)
        {
            if (vectors.Count == 0)
            {
                throw new CaptchaSolverException("There are no patterns loaded to which a sample pattern may be matched!");
            }

            int index = 0;
            double best = BitmapVector.RootMeanSquareDistance(vectors[0].Scale(), v.Scale());

            Parallel.For(1, vectors.Count, i =>
            {
                double test = BitmapVector.RootMeanSquareDistance(vectors[i].Scale(), v.Scale());

                if (test < best)
                {
                    best = test;
                    index = i;
                }
            });

            return vectors[index];
        }

        public BitmapVector GetClosestMatchNoScale(BitmapVector v)
        {
            if (vectors.Count == 0)
            {
                throw new CaptchaSolverException("There are no patterns loaded to which a sample pattern may be matched!");
            }

            int index = 0;
            double best = BitmapVector.RootMeanSquareDistance(vectors[0] / Count, v);

            Parallel.For(1, vectors.Count, i =>
            {
                double test = BitmapVector.RootMeanSquareDistance(vectors[i] / Count, v);

                if (test < best)
                {
                    best = test;
                    index = i;
                }
            });

            return vectors[index];
        }

        public void ExportAllVectorsAsBitmaps(string prefix)
        {
            int i = 0;
            foreach (BitmapVector v in vectors)
            {
                v.GetBitmap().Save(prefix + "_" + v.Information + "_" + (i++).ToString() + ".bmp");
            }
        }

        public List<Bitmap> GetAllVectorsAsBitmaps()
        {
            return vectors.Select(v => v.GetBitmap()).ToList();
        }

        #region Persistence Operations

        public void SaveToFile(BinaryWriter w)
        {
            w.Write(vectors.Count);

            foreach (BitmapVector v in vectors)
            {
                v.SaveToFile(w);
            }
        }

        public static BitmapVectorCollection LoadFromFile(BinaryReader r)
        {
            BitmapVectorCollection vc = new BitmapVectorCollection();
            int count = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                vc.Add(BitmapVector.LoadFromFile(r));
            }

            return vc;
        }

        #endregion
    }
}
