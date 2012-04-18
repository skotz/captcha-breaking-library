using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.CAPTCHA;

namespace ScottClayton.Neural
{
    /// <summary>
    /// Data about how a neural network's training progress is going.
    /// </summary>
    public class OnTrainingProgressChangeEventArgs
    {
        /// <summary>
        /// The percentage complete.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// The mean square error achieved in this training session.
        /// </summary>
        public double Error { get; set; }

        /// <summary>
        /// Create a new neural network training arguments object.
        /// </summary>
        public OnTrainingProgressChangeEventArgs(int progress, double error)
        {
            Error = error;
            Progress = progress;
        }
    }

    /// <summary>
    /// Data about how a neural network's training went
    /// </summary>
    public class OnTrainingCompletedEventArgs
    {
        /// <summary>
        /// The mean square error achieved in this training session.
        /// </summary>
        public double Error { get; set; }

        public OnTrainingCompletedEventArgs(double error)
        {
            Error = error;
        }
    }

    /// <summary>
    /// Data about how a neural network's testing went
    /// </summary>
    public class OnTestingCompletedEventArgs
    {
        /// <summary>
        /// The mean square error achieved in this testing session.
        /// </summary>
        public double Error { get; set; }

        /// <summary>
        /// The percentage of paterns that were correctly solved
        /// </summary>
        public double PercentageCorrect { get; set; }

        public OnTestingCompletedEventArgs(double error, double percentCorrect)
        {
            Error = error;
            PercentageCorrect = percentCorrect;
        }
    }

    /// <summary>
    /// Contains the solution to an asynchronous CAPTCHA solve
    /// </summary>
    public class OnSolverCompletedEventArgs
    {
        /// <summary>
        /// The solution to the CAPTCHA
        /// </summary>
        public string Solution { get; private set; }

        public OnSolverCompletedEventArgs(string solution)
        {
            Solution = solution;
        }
    }

    /// <summary>
    /// Contains the result of creating a solution set
    /// </summary>
    public class OnSolverSetCreatedEventArgs
    {
        /// <summary>
        /// The set of solutions for training or testing
        /// </summary>
        public SolutionSet SolutionsSet { get; set; }

        public OnSolverSetCreatedEventArgs(SolutionSet set)
        {
            SolutionsSet = set;
        }
    }

    /// <summary>
    /// Contains the progress of creating a solver set.
    /// </summary>
    public class OnSolverProgressChangedEventArgs
    {
        /// <summary>
        /// The percentage complete the loading is. (Yoda?)
        /// </summary>
        public int PercentDone { get; set; }

        /// <summary>
        /// A very basic estimation of how much time there is left.
        /// </summary>
        public TimeSpan EstimatedTimeRemaining { get; set; }

        public OnSolverProgressChangedEventArgs(int progress, TimeSpan remaining)
        {
            PercentDone = progress;
            EstimatedTimeRemaining = remaining;
        }
    }
}
