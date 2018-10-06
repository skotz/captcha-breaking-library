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

        public Pattern()
        {

        }

        public Pattern(double[] inputs, double[] outputs)
        {
            Inputs = new DoubleVector(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                Inputs[i] = inputs[i];
            }

            Outputs = new DoubleVector(outputs.Length);
            for (int i = 0; i < outputs.Length; i++)
            {
                Outputs[i] = outputs[i];
            }
        }
    }
}
