using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScottClayton.Neural
{
    /// <summary>
    /// A list of NNVectors with an added size check and the ability to save the list.
    /// </summary>
    class DoubleVectorList : List<DoubleVector>
    {
        /// <summary>
        /// Add a vector to the list
        /// </summary>
        /// <param name="item">The vector to add</param>
        /// <exception cref="VectorSizeMismatchException"></exception>
        public new void Add(DoubleVector item)
        {
            // If entering another item into the list, make sure that it has the same size as the rest of the vectors
            if (this.Count > 0 && this[0].Size != item.Size)
            {
                throw new VectorSizeMismatchException();
            }

            base.Add(item);
        }

        public void Save(BinaryWriter w)
        {
            w.Write(this.Count);
            foreach (DoubleVector v in this)
            {
                v.Save(w);
            }
        }

        public static DoubleVectorList Load(BinaryReader r)
        {
            DoubleVectorList nnvl = new DoubleVectorList();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                nnvl.Add(DoubleVector.Load(r));
            }
            return nnvl;
        }
    }
}
