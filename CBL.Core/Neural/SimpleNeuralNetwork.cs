using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScottClayton.Neural
{
    /// <summary>
    /// Implements a simple, fully-connected, artificial neural network.
    /// </summary>
    class SimpleNeuralNetwork
    {
        private int inputsCount;
        private int hiddenCount;
        private int outputCount;

        private double[,] weights_IH;
        private double[,] weights_HO;

        private double[] neurons_H;
        private DoubleVector neurons_O;

        private double[] error_O;
        private double[] error_H;

        private Random random;

        private double learningRate;

        // For a gradual fine-tuning learning descent
        // x = (.98)x will bring an initial x = .5 to x = .33 after 20 iterations [.5*(.98)^20 = .334]
        static private double _learningRateDescent = 0.95;

        // For notifying interrested subscribers of the networks training progress
        public delegate void TrainingProgressHandler(object sender, OnTrainingProgressChangeEventArgs e);
        public event TrainingProgressHandler OnTrainingProgressChange;

        /// <summary>
        /// The speed at which the network adjusts its weights.
        /// </summary>
        public double LearningRate
        {
            get { return learningRate; }
            set { learningRate = value; }
        }

        /// <summary>
        /// The number of inputs this network accepts.
        /// </summary>
        public int InputNeurons
        {
            get { return inputsCount; }
        }

        /// <summary>
        /// The number of hidden neurons in this network.
        /// </summary>
        public int HiddenNeurons
        {
            get { return hiddenCount; }
        }

        /// <summary>
        /// The number of outputs in this networks.
        /// </summary>
        public int OutputNeurons
        {
            get { return outputCount; }
        }

        /// <summary>
        /// Create a new neural network.
        /// </summary>
        /// <param name="inputs">The number of inputs that this network will accept.</param>
        /// <param name="hiddenNeurons">The number of hidden neurons in this network.</param>
        /// <param name="outputNeurons">The number of outputs to expect from this network.</param>
        /// <param name="learningRate">The rate to train this network.</param>
        public SimpleNeuralNetwork(int inputs, int hiddenNeurons, int outputNeurons, double learningRate)
        {
            random = new Random();

            inputsCount = inputs;
            hiddenCount = hiddenNeurons;
            outputCount = outputNeurons;

            // The weights
            weights_IH = new double[inputsCount, hiddenCount];
            weights_HO = new double[hiddenCount, outputCount];
            InitializeWeightsRandomly();

            // The neuron activations
            neurons_H = new double[hiddenCount];
            neurons_O = new DoubleVector(outputCount);

            // The error arrays
            error_H = new double[hiddenCount];
            error_O = new double[outputCount];

            this.learningRate = learningRate;
        }

        private SimpleNeuralNetwork()
        {
        }

        /// <summary>
        /// Set the network weights to random values.
        /// </summary>
        public void InitializeWeightsRandomly()
        {
            // Initialize weights between the input layer and the hidden layer
            for (int i = 0; i < inputsCount; i++)
            {
                for (int h = 0; h < hiddenCount; h++)
                {
                    weights_IH[i, h] = Rand();
                }
            }

            // Initialize weights between the hidden layer and the output layer
            for (int h = 0; h < hiddenCount; h++)
            {
                for (int o = 0; o < outputCount; o++)
                {
                    weights_HO[h, o] = Rand();
                }
            }
        }

        /// <summary>
        /// Run a vector of inputs through this neural network and get the output vector
        /// </summary>
        /// <param name="inputs">The vector of inputs to push through</param>
        public DoubleVector FeedForward(DoubleVector inputs)
        {
            double sum;

            for (int h = 0; h < hiddenCount; h++)
            {
                sum = 0.0;

                // Find the sum of all inputs multiplied by their respective weights
                for (int i = 0; i < inputsCount; i++)
                {
                    sum += inputs[i] * weights_IH[i, h];
                }

                // Put the sum through an activation function to find the activation of this neuron
                neurons_H[h] = ActivationFunctions.Sigmoid(sum);
            }

            // Calculate the activations of the output neurons
            for (int o = 0; o < outputCount; o++)
            {
                sum = 0.0;

                // Find the sum of all inputs multiplied by their respective weights
                for (int h = 0; h < hiddenCount; h++)
                {
                    sum += neurons_H[h] * weights_HO[h, o];
                }

                // Put the sum through an activation function to find the activation of this neuron
                neurons_O[o] = ActivationFunctions.Sigmoid(sum);
            }

            return neurons_O.Clone();
        }

        /// <summary>
        /// Train the network on a single input vector. Returns the sum of the mean square error of the sets.
        /// </summary>
        /// <param name="inputs">The input vectors to train on</param>
        /// <param name="targets">The target output vectors</param>
        /// <param name="iterations">The number of iterations to train on these inputs</param>
        public double Train(DoubleVectorList inputs, DoubleVectorList targets, int iterations)
        {
            // Make sure all input vectors have a matching output vector
            if (inputs.Count != targets.Count)
            {
                throw new Exception("You must provide a desired output vector for each input vector!");
            }

            double error = 0.0;
            int progress;

            Random r = new Random();

            for (int i = 0; i < iterations; i++)
            {
                // The error for this iteration
                error = 0.0;

                // Create a list of indexes and mix it up so that we don't train on the sets sequentially
                List<int> indices = new List<int>();
                for (int n = 0; n < inputs.Count; n++)
                {
                    indices.Insert(r.Next(n), n);
                }

                // Train for one iteration on this set
                for (int s = 0; s < inputs.Count; s++)
                {
                    error += Train(inputs[indices[s]], targets[indices[s]], 1);

                    // Report Progress
                    progress = (int)(((double)i * 100 / (double)iterations) + (((double)s * 100) / (double)inputs.Count) / (double)iterations);
                    if (OnTrainingProgressChange != null)
                    {
                        OnTrainingProgressChange(this, new OnTrainingProgressChangeEventArgs(progress, (error / (double)s) * (double)inputs.Count));
                    }
                }

                // Lower the learning rate
                learningRate *= _learningRateDescent;
            }

            // Return error of most recent iteration
            return error;
        }

        /// <summary>
        /// Train the network on a single input vector. Returns the mean square error of the set.
        /// </summary>
        /// <param name="inputs">The input vector to train on</param>
        /// <param name="targets">The target output vector</param>
        /// <param name="iterations">The number of iterations to train on these inputs</param>
        public double Train(DoubleVector inputs, DoubleVector targets, int iterations)
        {
            // Make sure the vectors are the correct sizes
            if (inputs.Size != inputsCount)
            {
                throw new Exception("An invalid number of inputs were fed into this neural network.");
            }
            if (targets.Size != outputCount)
            {
                throw new Exception("An invalid number of outputs are being expected from this neural network.");
            }

            for (int loop = 0; loop < iterations; loop++)
            {
                // The feed-formard pass
                DoubleVector actuals = FeedForward(inputs);

                // Calculate the error for the output layer
                for (int o = 0; o < outputCount; o++)
                {
                    error_O[o] = (targets[o] - actuals[o]) * ActivationFunctions.SigmoidDerivative(actuals[o]);
                }

                // Calculate the error for the hidden layer
                for (int h = 0; h < hiddenCount; h++)
                {
                    error_H[h] = 0.0;

                    for (int o = 0; o < outputCount; o++)
                    {
                        error_H[h] += error_O[o] * weights_HO[h, o];
                    }

                    error_H[h] *= ActivationFunctions.SigmoidDerivative(neurons_H[h]);
                }

                // Update the weights for the output layer
                for (int o = 0; o < outputCount; o++)
                {
                    for (int h = 0; h < hiddenCount; h++)
                    {
                        weights_HO[h, o] += learningRate * error_O[o] * neurons_H[h];
                    }
                }

                // Updat the weights for the hidden layer
                for (int h = 0; h < hiddenCount; h++)
                {
                    for (int i = 0; i < inputsCount; i++)
                    {
                        weights_IH[i, h] += learningRate * error_H[h] * inputs[i];
                    }
                }
            }

            // Return the mean square error
            return GetMeanSquareError(targets, FeedForward(inputs));
        }

        /// <summary>
        /// Get the mean square error between two output vectors
        /// See http://en.wikipedia.org/wiki/Mean_squared_error for an explanation of this equasion.
        /// </summary>
        public double GetMeanSquareError(DoubleVector target, DoubleVector actual)
        {
            // Make sure the vectors are the correct sizes
            if (target.Size != actual.Size)
            {
                throw new Exception("The two vectors must be the same size to get the error!");
            }

            double error = 0.0;

            // Get the MEAN (average)
            DoubleVector sdiff = target - actual;

            // Make it SQUARE
            sdiff *= sdiff;

            // Add up the ERRORS
            for (int i = 0; i < sdiff.Size; i++)
            {
                error += sdiff[i];
            }

            // Return the MEAN SQUARE ERROR
            return error / (double)(sdiff.Size + 1);
        }

        /// <summary>
        /// Returns a random double-precision number between -1.0 and 1.0;
        /// </summary>
        private double Rand()
        {
            return random.NextDouble() * 2.0 - 1.0;
        }

        #region Persistence Operations

        /// <summary>
        /// Save this vector to a file.
        /// </summary>
        public void Save(BinaryWriter w)
        {
            w.Write(inputsCount);
            w.Write(hiddenCount);
            w.Write(outputCount);

            for (int i = 0; i < inputsCount; i++)
            {
                for (int h = 0; h < hiddenCount; h++)
                {
                    w.Write(weights_IH[i, h]);
                }
            }

            for (int h = 0; h < hiddenCount; h++)
            {
                for (int o = 0; o < outputCount; o++)
                {
                    w.Write(weights_HO[h, o]);
                }
            }

            w.Write(neurons_H.Length);
            for (int h = 0; h < hiddenCount; h++)
            {
                w.Write(neurons_H[h]);
            }

            neurons_O.Save(w);

            w.Write(error_H.Length);
            for (int h = 0; h < error_H.Length; h++)
            {
                w.Write(error_H[h]);
            }

            w.Write(error_O.Length);
            for (int h = 0; h < error_O.Length; h++)
            {
                w.Write(error_O[h]);
            }

            w.Write(learningRate);
        }

        /// <summary>
        /// Load a vector from a file
        /// </summary>
        public static SimpleNeuralNetwork Load(BinaryReader r)
        {
            SimpleNeuralNetwork snn = new SimpleNeuralNetwork();

            snn.inputsCount = r.ReadInt32();
            snn.hiddenCount = r.ReadInt32();
            snn.outputCount = r.ReadInt32();

            snn.weights_IH = new double[snn.inputsCount, snn.hiddenCount];
            snn.weights_HO = new double[snn.hiddenCount, snn.outputCount];

            for (int i = 0; i < snn.inputsCount; i++)
            {
                for (int h = 0; h < snn.hiddenCount; h++)
                {
                    snn.weights_IH[i, h] = r.ReadDouble();
                }
            }

            for (int h = 0; h < snn.hiddenCount; h++)
            {
                for (int o = 0; o < snn.outputCount; o++)
                {
                    snn.weights_HO[h, o] = r.ReadDouble();
                }
            }

            snn.neurons_H = new double[r.ReadInt32()];
            for (int h = 0; h < snn.hiddenCount; h++)
            {
                snn.neurons_H[h] = r.ReadDouble();
            }

            snn.neurons_O = DoubleVector.Load(r);

            snn.error_H = new double[r.ReadInt32()];
            for (int h = 0; h < snn.error_H.Length; h++)
            {
                snn.error_H[h] = r.ReadDouble();
            }

            snn.error_O = new double[r.ReadInt32()];
            for (int h = 0; h < snn.error_O.Length; h++)
            {
                snn.error_O[h] = r.ReadDouble();
            }

            snn.learningRate = r.ReadDouble();

            return snn;
        }

        #endregion
    }
}
