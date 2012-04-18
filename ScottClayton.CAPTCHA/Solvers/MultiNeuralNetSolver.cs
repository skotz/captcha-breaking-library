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
    public class MultiNeuralNetSolver : Solver
    {
        private int charsPerImage;
        private List<string> charsSet;
        private double learnRate;

        private List<SpecialNeuralNet> sann;
        private BackgroundWorker trainerWorker;
        private BackgroundWorker testerWorker;

        public override List<string> CharacterSet { get { return charsSet; } protected set { charsSet = value; } }

        public MultiNeuralNetSolver(string characterSet, int imageWidth, int imageHeight)
            : this(characterSet, imageWidth, imageWidth, (int)(imageWidth * imageWidth * 1.5), -1, 0.98)
        {
        }

        public MultiNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int charactersPerImage)
            : this(characterSet, imageWidth, imageWidth, (int)(imageWidth * imageWidth * 1.5), charactersPerImage, 0.98)
        {
        }

        public MultiNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int hiddenNeurons, int charactersPerImage)
            : this(characterSet, imageWidth, imageWidth, hiddenNeurons, charactersPerImage, 0.98)
        {
        }

        public MultiNeuralNetSolver(string characterSet, int imageWidth, int imageHeight, int hiddenNeurons, int charactersPerImage, double learningRate)
        {
            // Split the set of characters into a list
            charsSet = characterSet.ToCharStringList();
            charsPerImage = charactersPerImage;
            learnRate = learningRate;

            ExpectedWidth = imageWidth;
            ExpectedHeight = imageHeight;

            sann = new List<SpecialNeuralNet>();
            for (int i = 0; i < charsSet.Count; i++)
            {
                // Each neural net specialized in only one letter (one output that indicates likliness of a pattern being a letter)
                sann.Add(new SpecialNeuralNet()
                {
                    NeuralNet = new SimpleNeuralNetwork(imageWidth * imageHeight, hiddenNeurons, 1, learningRate),
                    Solution = charsSet[i]
                });
                sann[sann.Count - 1].NeuralNet.OnTrainingProgressChange += new SimpleNeuralNetwork.TrainingProgressHandler(sann_OnTrainingProgressChange);
            }

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

        private string SolveOneCharacter(Pattern p)
        {
            // Run the pattern through each specialized neural net
            sann.ForEach(nn => nn.LastOutput = nn.NeuralNet.FeedForward(p.Inputs.Scale())[0]);

            // Sort the networks based on how likely their solution is the solution to the given pattern
            sann.Sort((a, b) => -a.LastOutput.CompareTo(b.LastOutput));

            // Get the solution of the highest rated network for the given pattern
            return sann[0].Solution;
        }

        public override string Solve(List<Pattern> patterns)
        {
            // Convert every output to a character in the solution
            return patterns.Select(p => SolveOneCharacter(p)).Aggregate((c, n) => c + n);
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
            double error = 0.0;

            if (patterns.Count == 0)
            {
                return null;
            }

            foreach (string c in charsSet)
            {
                DoubleVectorList inputs = new DoubleVectorList();
                DoubleVectorList outputs = new DoubleVectorList();

                foreach (Pattern p in patterns)
                {
                    // Add every item. The solution is either "yes this is a letter X" or "no this is not a letter X"
                    inputs.Add(p.Inputs.Scale());
                    if (GetOutputCharacter(p.Outputs) == c)
                    {
                        DoubleVector v = new DoubleVector(1);
                        v[0] = 0.99;
                        outputs.Add(v);
                    }
                    else
                    {
                        DoubleVector v = new DoubleVector(1);
                        v[0] = 0.01;
                        outputs.Add(v);
                    }
                }

                // Train only if we have patters for this letter in our character set
                if (inputs.Count > 0)
                {
                    // Get the specialized neural network for this specific letter
                    error += sann.Where(nn => nn.Solution == c).ToList()[0].NeuralNet.Train(inputs, outputs, iterations);
                }
            }

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

            double error = 0.0; // TODO: Need to calculate error
            double correct = 0;

            foreach (Pattern p in patterns)
            {
                if (SolveOneCharacter(p) == GetOutputCharacter(p.Outputs))
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

            w.Write(sann.Count);
            for (int i = 0; i < sann.Count; i++)
            {
                sann[i].NeuralNet.Save(w);
                w.Write(sann[i].Solution);
                w.Write(sann[i].LastOutput);
            }
        }

        public override void Load(BinaryReader r)
        {
            base.Load(r);
            charsPerImage = r.ReadInt32();
            charsSet = r.ReadString().ToCharArray().ToList().Select(c => c.ToString()).ToList();
            learnRate = r.ReadDouble();

            int count = r.ReadInt32();
            sann = new List<SpecialNeuralNet>();
            for (int i = 0; i < count; i++)
            {
                sann.Add(new SpecialNeuralNet());
                sann[i].NeuralNet = SimpleNeuralNetwork.Load(r);
                sann[i].NeuralNet.OnTrainingProgressChange += new SimpleNeuralNetwork.TrainingProgressHandler(sann_OnTrainingProgressChange);
                sann[i].Solution = r.ReadString();
                sann[i].LastOutput = r.ReadDouble();
            }
        }
    }

    class SpecialNeuralNet
    {
        public SimpleNeuralNetwork NeuralNet { get; set; }
        public string Solution { get; set; }
        public double LastOutput { get; set; }
    }
}
