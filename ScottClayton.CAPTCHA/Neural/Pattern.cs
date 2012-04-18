using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Neural
{
    public class Pattern
    {
        public DoubleVector Inputs { get; set; }
        public DoubleVector Outputs { get; set; }
        public double Error { get; set; }

        // SAVE AND LOAD
    }
}
