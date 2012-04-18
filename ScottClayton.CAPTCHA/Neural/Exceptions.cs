using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Neural
{
    /// <summary>
    /// Exception raised when you try to perform an operation on multiple vectors of unequal size.
    /// </summary>
    class VectorSizeMismatchException : Exception
    {
        public VectorSizeMismatchException()
            : base("You tried to perform an operation on multiple vectors of unequal size.")
        {
        }

        public VectorSizeMismatchException(string message)
            : base(message)
        {
        }

        public VectorSizeMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
