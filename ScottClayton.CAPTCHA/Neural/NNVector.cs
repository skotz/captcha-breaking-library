using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScottClayton.Neural
{
    /// <summary>
    /// An vector to feed through a neural network
    /// </summary>
    public class NNVector<T>
    {
        protected List<T> vector;

        /// <summary>
        /// The number of elements in this vector
        /// </summary>
        public int Size
        {
            get { return vector.Count; }
        }

        // Allow array-like access
        public T this[int index]
        {
            get { return vector[index]; }
            set { vector[index] = value; }
        }

        public NNVector(int size)
        {
            vector = new List<T>();
            for (int i = 0; i < size; i++)
            {
                vector.Add(default(T));
            }
        }

        protected NNVector()
        {
            vector = new List<T>();
        }

        /// <summary>
        /// Get a copy of this vector.
        /// </summary>
        public NNVector<T> Clone()
        {
            NNVector<T> copy = new NNVector<T>(Size);

            copy.vector = vector.Select(x => x).ToList();

            return copy;
        }

        /// <summary>
        /// Get the index of the largest element in this vector. 
        /// Great for a winner-takes-all method of reviewing output vectors.
        /// </summary>
        public int GetIndexOfLargestElement()
        {
            if (vector.Count > 0)
            {
                return vector.IndexOf(vector.Max());
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Set each element of the this vector to a specific value.
        /// </summary>
        /// <param name="value">The value to set each element to.</param>
        public void Fill(T value)
        {
            for (int i = 0; i < vector.Count; i++)
            {
                vector[i] = value;
            }
        }

        /// <summary>
        /// Concatenate the contents of one vector to the end of another.
        /// </summary>
        public static NNVector<T> Concatenate(NNVector<T> lhs, NNVector<T> rhs)
        {
            NNVector<T> both = new NNVector<T>();

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
