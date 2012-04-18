using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.Neural;
using System.IO;
using System.ComponentModel;
using ScottClayton.Utility;

namespace ScottClayton.CAPTCHA
{
    public class SimpleNeuralNetSolver : Solver
    {
        private int charsPerImage;
        private List<string> charsSet;
        private double learnRate;

        private SimpleNeuralNetwork sann;
        private BackgroundWorker trainerWorker;
        private BackgroundWorker testerWorker;

        public override List<string> CharacterSet { get { return charsSet; } protected set { charsSet = value; } }

        public SimpleNeuralNetSolver(string characterSet, int imageWidth, int imageHeight)
            : this(characterSet, imageWidth, imageWidth, (int)(imageWidth * imageWidth * 1.5), -1, 0.98)
        {
        }

        public SimpleNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int charactersPerImage)
            : this(characterSet, imageWidth, imageWidth, (int)(imageWidth * imageWidth * 1.5), charactersPerImage, 0.98)
        {
        }

        public SimpleNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int hiddenNeurons, int charactersPerImage)
            : this(characterSet, imageWidth, imageWidth, hiddenNeurons, charactersPerImage, 0.98)
        {
        }

        public SimpleNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int hiddenNeurons, int charactersPerImage, double learningRate)
        {
            // Split the set of characters into a list
            charsSet = characterSet.ToCharStringList();
            charsPerImage = charactersPerImage;
            learnRate = learningRate;

            ExpectedWidth = imageWidth;
            ExpectedHeight = imageHeight;

            sann = new SimpleNeuralNetwork(imageWidth * imageHeight, hiddenNeurons, characterSet.Length, learningRate);
            sann.OnTrainingProgressChange += new SimpleNeuralNetwork.TrainingProgressHandler(sann_OnTrainingProgressChange);

            trainerWorker = new BackgroundWorker();
            trainerWorker.DoWork += new DoWorkEventHandler(worker_DoWork);
            trainerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            trainerWorker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            trainerWorker.WorkerReportsProgress = true;

            testerWorker = new BackgroundWorker();
            testerWorker.DoWork += new DoWorkEventHandler(testerWorker_DoWork);
            testerWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(testerWorker_RunWorkerCompleted);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RaiseOnTrainingProgressChanged(new OnTrainingProgressChangeEventArgs(e.ProgressPercentage, (double)e.UserState));
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RaiseOnTrainingComplete((PatternResult)e.Result);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartTrainingArgs args = (StartTrainingArgs)e.Argument;
            e.Result = Train(args.Patterns, args.Iterations);
        }

        void sann_OnTrainingProgressChange(object sender, OnTrainingProgressChangeEventArgs e)
        {
            trainerWorker.ReportProgress(e.Progress, e.Error);
        }

        void testerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RaiseOnTrainingComplete((PatternResult)e.Result);
        }

        void testerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Test((List<Pattern>)e.Argument);
        }

        public override string Solve(List<Pattern> patterns)
        {
            // Run every pattern through the network and get a list of the outputs
            patterns.ForEach(p => p.Outputs = sann.FeedForward(p.Inputs.Scale(0.02, 0.98)));

            // Convert every output to a character in the solution
            return patterns.Select(p => GetOutputCharacter(p)).Aggregate((c,n) => c + n);
        }

        /// <summary>
        /// Use a winner-takes-all approach to determine what the most likely answer to a pattern is.
        /// </summary>
        /// <param name="p">The pattern to analyse</param>
        /// <returns></returns>
        private string GetOutputCharacter(Pattern p)
        {
            return GetOutputCharacter(p.Outputs);
        }

        /// <summary>
        /// Use a winner-takes-all approach to determine what the most likely answer to a pattern is.
        /// </summary>
        /// <param name="p">The pattern to analyse</param>
        /// <returns></returns>
        private string GetOutputCharacter(DoubleVector v)
        {
            // There is one output for each possible character the CAPTCHA can contain.
            // The index of the output with the highest value corresponds to a character in the character set.
            return charsSet[v.GetIndexOfLargestElement()];
        }

        public override PatternResult Train(List<Pattern> patterns, int iterations)
        {
            if (patterns.Count == 0)
            {
                return null;
            }

            DoubleVectorList inputs = new DoubleVectorList();
            DoubleVectorList outputs = new DoubleVectorList();

            foreach (Pattern p in patterns)
            {
                inputs.Add(p.Inputs.Scale(0.02, 0.98));
                outputs.Add(p.Outputs);
            }

            double error = sann.Train(inputs, outputs, iterations);
             
            return new PatternResult(error);
        }

        public override void TrainAsync(List<Pattern> patterns, int iterations)
        {
            if (!trainerWorker.IsBusy)
            {
                trainerWorker.RunWorkerAsync(new StartTrainingArgs() { Patterns = patterns, Iterations = iterations });
            }
        }

        public override PatternResult Test(List<Pattern> patterns)
        {
            if (patterns.Count == 0)
            {
                return null;
            }

            DoubleVector actual;
            double error = 0.0;
            double correct = 0;

            foreach (Pattern p in patterns)
            {
                actual = sann.FeedForward(p.Inputs.Scale(0.02, 0.98));
                error += sann.GetMeanSquareError(p.Outputs, actual);

                if (GetOutputCharacter(actual) == GetOutputCharacter(p.Outputs))
                {
                    correct++;
                }
            }

            return new PatternResult(error, (correct / patterns.Count) * 100.0);
        }

        public override void TestAsync(List<Pattern> patterns)
        {
            if (!testerWorker.IsBusy)
            {
                testerWorker.RunWorkerAsync(patterns);
            }
        }

        public override void Save(BinaryWriter w)
        {
            base.Save(w);
            w.Write(charsPerImage);
            w.Write(charsSet.Aggregate((c, n) => c + n));
            w.Write(learnRate);
            sann.Save(w);
        }

        public override void Load(BinaryReader r)
        {
            base.Load(r);
            charsPerImage = r.ReadInt32();
            charsSet = r.ReadString().ToCharArray().ToList().Select(c => c.ToString()).ToList();
            learnRate = r.ReadDouble();
            sann = SimpleNeuralNetwork.Load(r);
            sann.OnTrainingProgressChange += new SimpleNeuralNetwork.TrainingProgressHandler(sann_OnTrainingProgressChange);
        }
    }

    public struct StartTrainingArgs
    {
        public List<Pattern> Patterns { get; set; }
        public int Iterations { get; set; }
    }
}
