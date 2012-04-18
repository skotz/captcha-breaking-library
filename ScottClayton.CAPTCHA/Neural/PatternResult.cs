using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Neural
{
    public class PatternResult
    {
        public double Error { get; set; }
        public double PercentageCorrect { get; set; }

        public PatternResult()
            : this(0, 0)
        {
        }

        public PatternResult(double error)
            : this(error, 0)
        {
        }

        public PatternResult(double error, double percentCorrect)
        {
            Error = error;
            PercentageCorrect = percentCorrect;
        }
    }
}
