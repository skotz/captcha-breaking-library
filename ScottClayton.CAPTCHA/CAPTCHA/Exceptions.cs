using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.CAPTCHA
{
    /// <summary>
    /// Exception raised when a database file for the CAPTCHA breaker could not be loaded or saved.
    /// </summary>
    public class BreakerDataBaseException : Exception
    {
        public BreakerDataBaseException()
            : base("Failed to save or load the CAPTCHA breaker database.")
        {
        }

        public BreakerDataBaseException(string message)
            : base(message)
        {
        }

        public BreakerDataBaseException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }

    /// <summary>
    /// Exception raised when an image could not be converted into a pattern for the breaker system.
    /// </summary>
    public class PatternGenerationException : Exception
    {
        public PatternGenerationException()
            : base("Failed to generate a pattern for a bitmap.")
        {
        }

        public PatternGenerationException(string message)
            : base(message)
        {
        }

        public PatternGenerationException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }

    /// <summary>
    /// Exception raised when a solution set could not be generated or loaded.
    /// </summary>
    public class SolutionSetException : Exception
    {
        public SolutionSetException()
            : base("Failed to create or load a solution set.")
        {
        }

        public SolutionSetException(string message)
            : base(message)
        {
        }

        public SolutionSetException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }

    /// <summary>
    /// Exception raised when an image could not be segmented.
    /// </summary>
    public class SegmentationException : Exception
    {
        public SegmentationException()
            : base("Failed to segment an image.")
        {
        }

        public SegmentationException(string message)
            : base(message)
        {
        }

        public SegmentationException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }

    /// <summary>
    /// Exception raised when an image could not be solved.
    /// </summary>
    public class CaptchaSolverException : Exception
    {
        public CaptchaSolverException()
            : base("Failed while trying to solve an image.")
        {
        }

        public CaptchaSolverException(string message)
            : base(message)
        {
        }

        public CaptchaSolverException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }
}
