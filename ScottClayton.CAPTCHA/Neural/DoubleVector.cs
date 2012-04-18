using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScottClayton.Neural
{
    /// <summary>
    /// A vector of doubles
    /// </summary>
    public class DoubleVector : NNVector<double>
    {
        public DoubleVector(int size)
            : base(size)
        {
        }

        private DoubleVector()
            : base()
        { 
        }

        /// <summary>
        /// Save this vector to a file.
        /// </summary>
        public void Save(BinaryWriter w)
        {
            w.Write(vector.Count);
            for (int i = 0; i < vector.Count; i++)
            {
                w.Write(vector[i]);
            }
        }

        /// <summary>
        /// Load a vector from a file
        /// </summary>
        public static DoubleVector Load(BinaryReader r)
        {
            int size = r.ReadInt32();
            DoubleVector v = new DoubleVector(size);

            v.vector = new List<double>();
            for (int i = 0; i < size; i++)
            {
                v.vector.Add(r.ReadDouble());
            }

            return v;
        }

        public DoubleVector Clone()
        {
            DoubleVector copy = new DoubleVector();
            foreach (double d in vector)
            {
                copy.vector.Add(d);
            }
            return copy;
        }

        /// <summary>
        /// Add one vector to another
        /// </summary>
        public static DoubleVector operator +(DoubleVector lhs, DoubleVector rhs)
        {
            if (lhs.Size != rhs.Size)
            {
                throw new VectorSizeMismatchException();
            }

            DoubleVector diff = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                diff[i] = lhs[i] + rhs[i];
            }

            return diff;
        }

        /// <summary>
        /// Add a double to every element of a vector
        /// </summary>
        public static DoubleVector operator +(DoubleVector lhs, double rhs)
        {
            DoubleVector diff = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                diff[i] = lhs[i] + rhs;
            }

            return diff;
        }

        /// <summary>
        /// Subtract one vector from another
        /// </summary>
        public static DoubleVector operator -(DoubleVector lhs, DoubleVector rhs)
        {
            if (lhs.Size != rhs.Size)
            {
                throw new VectorSizeMismatchException();
            }

            DoubleVector diff = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                diff[i] = lhs[i] - rhs[i];
            }

            return diff;
        }

        /// <summary>
        /// Subtract a value from every element of a vector
        /// </summary>
        public static DoubleVector operator -(DoubleVector lhs, double rhs)
        {
            DoubleVector diff = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                diff[i] = lhs[i] - rhs;
            }

            return diff;
        }

        /// <summary>
        /// Multiply one vector with another, element by element
        /// </summary>
        public static DoubleVector operator *(DoubleVector lhs, DoubleVector rhs)
        {
            if (lhs.Size != rhs.Size)
            {
                throw new VectorSizeMismatchException();
            }

            DoubleVector prod = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                prod[i] = lhs[i] * rhs[i];
            }

            return prod;
        }

        /// <summary>
        /// Multiply each item in a vector with a constant
        /// </summary>
        public static DoubleVector operator *(DoubleVector lhs, double constant)
        {
            DoubleVector prod = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                prod[i] = lhs[i] * constant;
            }

            return prod;
        }

        /// <summary>
        /// Divide each item in a vector with a constant
        /// </summary>
        public static DoubleVector operator /(DoubleVector lhs, double constant)
        {
            DoubleVector prod = new DoubleVector(lhs.Size);

            for (int i = 0; i < lhs.Size; i++)
            {
                prod[i] = lhs[i] / constant;
            }

            return prod;
        }

        /// <summary>
        /// Scale every value in this vector to between LOWER and UPPER
        /// </summary>
        /// <returns></returns>
        public DoubleVector Scale(double lower, double upper)
        {
            return (Scale() * (upper - lower)) + lower;
        }

        /// <summary>
        /// Scale every value in this vector to between 0.0 and 1.0
        /// </summary>
        /// <returns></returns>
        public DoubleVector Scale()
        {
            double s = vector.Min();
            double b = vector.Max();

            return (this - s) / (b - s);
        }

        public static DoubleVector Concatenate(DoubleVector lhs, DoubleVector rhs)
        {
            DoubleVector both = new DoubleVector();

            for (int i = 0; i < lhs.Size; i++)
            {
                both.vector.Add(lhs[i]);
            }

            for (int i = 0; i < rhs.Size; i++)
            {
                both.vector.Add(rhs[i]);
            }

            return both;
        }
    }
}
