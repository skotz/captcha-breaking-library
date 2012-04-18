using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Neural
{
    /// <summary>
    /// Contains activation functions for computing a neuron's activation
    /// </summary>
    static class ActivationFunctions
    {
        /// <summary>
        /// The sigmoid activation function. 
        /// See http://mathworld.wolfram.com/SigmoidFunction.html for an explanation. 
        /// </summary>
        static public double Sigmoid(double input)
        {
            return 1.0 / (1.0 + Math.Exp(-input));
        }

        /// <summary>
        /// The derivative of the sigmoid activation function (for back-propagation)
        /// </summary>
        static public double SigmoidDerivative(double input)
        {
            double value = Sigmoid(input);
            return value * (1 - value);
        }
    }
}
