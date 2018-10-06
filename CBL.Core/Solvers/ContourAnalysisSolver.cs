using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.Neural;
using System.IO;
using System.ComponentModel;
using ScottClayton.Utility;
using ScottClayton.CAPTCHA.Image;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using ScottClayton.CAPTCHA.Utility;

namespace ScottClayton.CAPTCHA
{
    public class ContourAnalysisSolver : Solver
    {
        private List<string> charsSet;

        private ImageProcessor solver;

        private BackgroundWorker trainerWorker;
        private BackgroundWorker testerWorker;

        public override List<string> CharacterSet { get { return charsSet; } protected set { charsSet = value; } }

        public ContourAnalysisSolver(string characterSet, int imageWidth, int imageHeight)
        {
            // Split the set of characters into a list
            charsSet = characterSet.ToCharStringList();

            ExpectedWidth = imageWidth;
            ExpectedHeight = imageHeight;

            solver = new ImageProcessor();
            solver.templates = new Templates();

            //solver.finder.maxRotateAngle = Math.PI / 2; // = 90 degrees
            solver.finder.maxACFDescriptorDeviation = 20;
            solver.finder.minACF = 0.95;
            solver.finder.minICF = 0.80;
            solver.adaptiveThresholdBlockSize = 50;
            solver.minContourArea = 15;
            solver.minContourLength = 10;
            solver.cannyThreshold = 50;
            solver.filterContoursBySize = false;
            solver.blur = true;
            solver.preprocess = false;
            solver.filterbyintersection = false;

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
            string solution = "";

            foreach (Pattern p in patterns)
            {
                solver.ProcessImage(GetCVImage(p));

                lock (solver.foundTemplates)
                {
                    if (solver.foundTemplates.Count > 0)
                    {
                        solver.foundTemplates.Sort((t1, t2) => -t1.template.rate.CompareTo(t2.template.rate));
                        foreach (FoundTemplateDesc found in solver.foundTemplates)
                        {
                            //DrawTemplate(found.template);

                            if (found.template.rect.Width * found.template.rect.Height < (double)p.Inputs.Size * 0.99)
                            {
                                solution += found.template.name;
                                break;

                                //solution += (f ? "(" : "") + found.template.name + (f ? ")" : "");
                            }
                        }
                    }
                    else
                    {
                        solution += "?";
                    }
                }
            }

            return solution;
        }

        private Image<Bgr, byte> GetCVImage(Pattern p)
        {
            // TODO: This convertes a bitmap to a pattern to a vector to a bitmap to another bitmap to a CV image. (Not good...)
            Bitmap bmp = ((BitmapVector)p.Inputs).Invert().GetBitmap();

            Bitmap withBorder = new Bitmap(ExpectedWidth + 8, ExpectedHeight + 8);
            Graphics g = Graphics.FromImage(withBorder);
            g.Clear(Color.White);
            g.DrawImage(bmp, new Rectangle(4, 4, ExpectedWidth, ExpectedHeight), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);

            //withBorder.Save("x_" + withBorder.GetHashCode() + ".bmp");

            return new Image<Bgr, byte>(withBorder);
        }

        private List<Template> GetTemplates(Pattern p)
        {
            List<Template> templates = new List<Template>();

            solver.ProcessImage(GetCVImage(p));
            solver.samples.Sort((t1, t2) => -t1.sourceArea.CompareTo(t2.sourceArea));

            foreach (Template t in solver.samples)
            {
                if (t.rect.Width * t.rect.Height >= (double)p.Inputs.Size * 0.99)
                {
                    continue;
                }

                Template temp = Clone(t);

                temp.name = GetOutputCharacter(p.Outputs);
                temp.preferredAngleNoMore90 = true;

                DrawTemplate(temp);

                templates.Add(temp);
            }

            return templates;
        }
        
        private void DrawTemplate(Template t)
        {
            try
            {
                Bitmap b = new Bitmap(300, 300);
                Graphics g = Graphics.FromImage(b);
                t.Draw(g, new Rectangle(0, 0, 299, 299));

                // Uncomment to save example images of the contours extracted
                //if (!Directory.Exists("cv"))
                //{
                //    Directory.CreateDirectory("cv");
                //}
                //if (!Directory.Exists("cv\\" + t.name))
                //{
                //    Directory.CreateDirectory("cv\\" + t.name);
                //}
                //string hash = b.GetHashCode().ToString("X");
                //b.Save("cv\\" + t.name + "\\!_" + t.name + "_" + t.rate + "_" + hash + ".bmp");
                
                if (GlobalMessage.ALLOW_MESSAGES)
                {
                    GlobalMessage.SendMessage(b);
                }
            }
            catch { }
        }
                
        public override PatternResult Train(List<Pattern> patterns, int iterations)
        {
            foreach (Pattern p in patterns)
            {
                solver.templates.AddRange(GetTemplates(p));
            }

            return new PatternResult();
        }

        private string GetOutputCharacter(DoubleVector v)
        {
            // There is one output for each possible character the CAPTCHA can contain.
            // The index of the output with the highest value corresponds to a character in the character set.
            return charsSet[v.GetIndexOfLargestElement()];
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
            double correct = 0;

            foreach (Pattern p in patterns)
            {
                solver.ProcessImage(GetCVImage(p));

                lock (solver.foundTemplates)
                {
                    if (solver.foundTemplates != null)
                    {
                        solver.foundTemplates.Sort((t1, t2) => -t1.template.rate.CompareTo(t2.template.rate));
                        foreach (FoundTemplateDesc ftd in solver.foundTemplates)
                        {
                            if (ftd != null)
                            {
                                if (GetOutputCharacter(p.Outputs) == ftd.template.name)
                                {
                                    correct++;
                                    break;
                                }
                            }
                            else
                            {
                                // TODO - ?
                            }
                        }
                    }
                }
            }

            return new PatternResult(0.0, (correct / patterns.Count) * 100.0);
        }

        public override void TestAsync(List<Pattern> patterns)
        {
            if (!testerWorker.IsBusy)
            {
                testerWorker.RunWorkerAsync(patterns);
            }
        }

        private Template Clone(Template t)
        {
            Template copy = null;
            byte[] template;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, t);
                template = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream(template))
            {
                copy = (Template)new BinaryFormatter().Deserialize(ms);
            }

            return copy;
        }

        public override void Save(BinaryWriter w)
        {
            base.Save(w);
            w.Write(charsSet.Aggregate((c, n) => c + n));

            byte[] allTemplates;
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms , solver.templates);
                allTemplates = ms.ToArray();
            }
            w.Write(allTemplates.Length);
            w.Write(allTemplates);
        }

        public override void Load(BinaryReader r)
        {
            base.Load(r);
            charsSet = r.ReadString().ToCharArray().ToList().Select(c => c.ToString()).ToList();

            int byteCount = r.ReadInt32();
            byte[] allTemplates = r.ReadBytes(byteCount);
            using (MemoryStream ms = new MemoryStream(allTemplates))
            {
                solver.templates = (Templates)new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}
