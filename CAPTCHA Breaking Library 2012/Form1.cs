using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScottClayton.CAPTCHA;
using ScottClayton.Image;
using ScottClayton.Neural;

namespace CaptchaBreakingLibrary
{
    public partial class Form1 : Form
    {
        CAPTCHABreaker captcha;
        CAPTCHABreaker cv;
        SolutionSet trainer;
        SolutionSet tester;

        public Form1()
        {
            InitializeComponent();

            captcha = new CAPTCHABreaker(new SimpleNeuralNetSolver("0123456789", 20, 20, 100, 8), new BlobSegmentMethod(15, 25, 8));
            captcha.OnBeforeSegmentation += new CAPTCHABreaker.BeforeSegmentHandler(captcha_OnBeforeSegmentation);
            captcha.OnTrainingComplete += new CAPTCHABreaker.TrainingCompleteHandler(captcha_OnTrainingComplete);
            captcha.OnTrainingProgressChanged += new CAPTCHABreaker.TrainingProgressChangedHandler(captcha_OnTrainingProgressChanged);
            captcha.OnSolvingComplete += new CAPTCHABreaker.SolverCompleteHandler(captcha_OnSolvingComplete);
            captcha.OnSolverSetCreated += new CAPTCHABreaker.SolverSetCreatedHandler(captcha_OnSolverSetCreated);
            captcha.OnSolverSetProgressChanged += new CAPTCHABreaker.SolverProgressChangedEventArgsHandler(captcha_OnSolverSetProgressChanged);

            cv = new CAPTCHABreaker(new ContourAnalysisSolver("0123456789", 50, 50), new BlobSegmentMethod(15, 25, 8));
            cv.OnBeforeSegmentation += new CAPTCHABreaker.BeforeSegmentHandler(captcha_OnBeforeSegmentation);
            cv.OnTrainingComplete += new CAPTCHABreaker.TrainingCompleteHandler(cv_OnTrainingComplete);
            cv.OnTrainingProgressChanged += new CAPTCHABreaker.TrainingProgressChangedHandler(cv_OnTrainingProgressChanged);
            cv.OnSolvingComplete += new CAPTCHABreaker.SolverCompleteHandler(cv_OnSolvingComplete);
        }
        
        void captcha_OnSolvingComplete(object sender, OnSolverCompletedEventArgs e)
        {
            richTextBox1.Text = "Solution: " + e.Solution;
        }
         
        void captcha_OnBeforeSegmentation(Segmenter s)
        {
            //s.FloodFill(new Point(2, 2), 60, Color.White);

            //s.Binarize(20);
            //s.ColorFillBlobs(50, Color.White);
            //s.RemoveSmallBlobs(10, 3, 3, Color.White);
            //s.Image.Save("test.bmp");

            //s.Image = s.Image.Resize(s.Image.Width * 6, s.Image.Height * 3);
            s.Crop(new Rectangle(10, 10, 200 - 20, 50 - 20));
            s.Resize(1000, 120);

            s.ColorFillBlobs(30, Color.White, 30);

            s.TrySave("test1.bmp");

            //s.ErodeShapes(Color.White);
            //s.ErodeShapes(Color.White);

            //s.ErodeShapes(Color.White);
            //s.ErodeShapes(Color.White);

            s.BlackAndWhite();

            s.RemoveSmallBlobs(150, 15, 25, Color.White);

            s.ResizeRotateCut();

            s.ColorFillBlobs(1, Color.White, 1);

            s.RemoveSmallBlobs(150, 15, 25, Color.White);

            //s.ForEachPixel(c => Color.FromArgb((c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3, (c.R + c.G + c.B) / 3));

            s.TrySave("test2.bmp");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trainer = captcha.CreateSolutionSet("test2");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            trainer = captcha.LoadSolutionSet("test2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            captcha.TrainAsync(trainer, 1);
        }

        void captcha_OnTrainingProgressChanged(object sender, OnTrainingProgressChangeEventArgs e)
        {
            progressBar1.Value = e.Progress;
            richTextBox1.Text = "Error: " + e.Error;
        }

        void captcha_OnTrainingComplete(object sender, OnTrainingCompletedEventArgs e)
        {
            progressBar1.Value = 100;

            richTextBox1.Text = "Error: " + e.Error + "\r\n";
        }

        private void button16_Click(object sender, EventArgs e)
        {
            richTextBox1.Text += "\r\nSolving... Please Wait...\r\n";
            captcha.SolveAsync(Image.FromFile("solveme2.bmp") as Bitmap);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = captcha.Test(tester).PercentageCorrect.ToString("0.000") + "% Correct";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            captcha.CreateSolutionSetAsync("test4");
        }

        void captcha_OnSolverSetProgressChanged(object sender, OnSolverProgressChangedEventArgs e)
        {
            progressBar1.Value = e.PercentDone;
            label1.Text = e.EstimatedTimeRemaining.Hours.ToString("00") + ":" + e.EstimatedTimeRemaining.Minutes.ToString("00") + ":" + e.EstimatedTimeRemaining.Seconds.ToString("00") + " remaining...";
        }

        void captcha_OnSolverSetCreated(object sender, OnSolverSetCreatedEventArgs e)
        {
            tester = e.SolutionsSet;
            label1.Text = "Done";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tester = captcha.LoadSolutionSet("test4");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            captcha = new CAPTCHABreaker(new BitmapSubtractionSolver("0123456789", 20, 20), new BlobSegmentMethod(15, 25, 8));
            captcha.OnBeforeSegmentation += new CAPTCHABreaker.BeforeSegmentHandler(captcha_OnBeforeSegmentation);
            captcha.OnTrainingComplete += new CAPTCHABreaker.TrainingCompleteHandler(captcha_OnTrainingComplete);
            captcha.OnTrainingProgressChanged += new CAPTCHABreaker.TrainingProgressChangedHandler(captcha_OnTrainingProgressChanged);
            captcha.OnSolvingComplete += new CAPTCHABreaker.SolverCompleteHandler(captcha_OnSolvingComplete);
            captcha.OnSolverSetCreated += new CAPTCHABreaker.SolverSetCreatedHandler(captcha_OnSolverSetCreated);
            captcha.OnSolverSetProgressChanged += new CAPTCHABreaker.SolverProgressChangedEventArgsHandler(captcha_OnSolverSetProgressChanged);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HistogramSegmentMethod seg = new HistogramSegmentMethod(6);
            seg.Segment((Bitmap)Bitmap.FromFile("histotest.bmp")).ForEach(b => b.Save("xHISTO." + b.GetHashCode() + ".bmp"));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //cv.CreateSolutionSetAsync("testcv");
            cv.TrainAsync(cv.LoadSolutionSet("testcv2"));
        }

        private void button10_Click(object sender, EventArgs e)
        {
            PatternResult result = cv.Test(cv.LoadSolutionSet("testcv"));

            richTextBox1.Text = "Test Result: " + result.PercentageCorrect + "% Correct";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            cv.SolveAsync((Bitmap)Bitmap.FromFile("solveme2.bmp"));
        }

        void cv_OnSolvingComplete(object sender, OnSolverCompletedEventArgs e)
        {
            richTextBox1.Text = e.Solution;
        }

        void cv_OnTrainingProgressChanged(object sender, OnTrainingProgressChangeEventArgs e)
        {
            progressBar1.Value = e.Progress;
        }

        void cv_OnTrainingComplete(object sender, OnTrainingCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            richTextBox1.Text = "Done.";
        }

        private void button12_Click(object sender, EventArgs e)
        {
            captcha.SaveToFile("sann");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            captcha.LoadFromFile("sann");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            cv.SaveToFile("cv.db");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            cv.LoadFromFile("cv.db");
        }
    }
} 
