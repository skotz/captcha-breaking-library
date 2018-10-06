using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ScottClayton.CAPTCHA;
using ScottClayton.Image;
using ScottClayton.Neural;
using ScottClayton.Utility;
using System.IO;
using System.ComponentModel;
using ScottClayton.CAPTCHA.Utility;
using System.Threading.Tasks;
using System.IO.Compression;
using AForge.Imaging.Filters;

namespace ScottClayton.CAPTCHA
{
    /// <summary>
    /// The master CAPTCHA breaking class.
    /// </summary>
    public class CAPTCHABreaker
    {
        public delegate void BeforeSegmentHandler(Segmenter s);

        /// <summary>
        /// YOU MUST SUBSCRIBE TO THIS EVENT.
        /// This event is raised with a Segmenter object that you must use to specify how the image will be preprocessed.
        /// </summary>
        public event BeforeSegmentHandler OnBeforeSegmentation;

        public delegate void TrainingProgressChangedHandler(object sender, OnTrainingProgressChangeEventArgs e);

        /// <summary>
        /// Event raised when training progress is made.
        /// </summary>
        public event TrainingProgressChangedHandler OnTrainingProgressChanged;

        public delegate void TrainingCompleteHandler(object sender, OnTrainingCompletedEventArgs e);

        /// <summary>
        /// Event raised when training completes.
        /// </summary>
        public event TrainingCompleteHandler OnTrainingComplete;

        public delegate void TestingCompleteHandler(object sender, OnTestingCompletedEventArgs e);

        /// <summary>
        /// Event raised when testing completes.
        /// </summary>
        public event TestingCompleteHandler OnTestingComplete;

        /// <summary>
        /// Event raised when a full test completes.
        /// </summary>
        public event TestingCompleteHandler OnFullTestingComplete;

        public delegate void SolverCompleteHandler(object sender, OnSolverCompletedEventArgs e);

        /// <summary>
        /// Event raised when a CAPTCHA has been solved in the background.
        /// </summary>
        public event SolverCompleteHandler OnSolvingComplete;

        public delegate void SolverSetCreatedHandler(object sender, OnSolverSetCreatedEventArgs e);

        /// <summary>
        /// Event raised when a solution set has been successfully created.
        /// </summary>
        public event SolverSetCreatedHandler OnSolverSetCreated;

        public delegate void SolverProgressChangedEventArgsHandler(object sender, OnSolverProgressChangedEventArgs e);

        /// <summary>
        /// Event raised when progress is made in creating a solution set in the background.
        /// </summary>
        public event SolverProgressChangedEventArgsHandler OnSolverSetProgressChanged;

        Segmenter segmenter;
        Solver solver;

        private BackgroundWorker asyncSolver;
        private BackgroundWorker asyncSetCreater;
        private BackgroundWorker asyncFullTest;

        private DateTime asyncSetCreaterStart;

        /// <summary>
        /// The sub-folder of a solution set folder that segmented patterns get stored in.
        /// </summary>
        private const string trainersFolder = "trainers";

        private ConsoleProgress trainingProgress;

        /// <summary>
        /// Create a new CAPTCHABreaker with a specified solver and segmenter.
        /// </summary>
        /// <param name="solveMethod">The method used for recognizing patterns that have been extracted from an image.</param>
        /// <param name="segmentMethod">The method to use for segmenting an image into patterns.</param>
        public CAPTCHABreaker(Solver solveMethod, SegmentMethod segmentMethod)
        {
            segmenter = new Segmenter();
            segmenter.SegmentationMethod = segmentMethod;

            solver = solveMethod;
            solver.OnTraininProgressChanged += new Action<object, OnTrainingProgressChangeEventArgs>(solver_OnTraininProgressChanged);
            solver.OnTrainingComplete += new Action<object, PatternResult>(solver_OnTrainingComplete);
            solver.OnTestingComplete += new Action<object, PatternResult>(solver_OnTestingComplete);

            asyncSolver = new BackgroundWorker();
            asyncSolver.DoWork += new DoWorkEventHandler(asyncSolver_DoWork);
            asyncSolver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(asyncSolver_RunWorkerCompleted);

            asyncSetCreater = new BackgroundWorker();
            asyncSetCreater.DoWork += new DoWorkEventHandler(asyncSetCreater_DoWork);
            asyncSetCreater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(asyncSetCreater_RunWorkerCompleted);
            asyncSetCreater.WorkerReportsProgress = true;
            asyncSetCreater.ProgressChanged += new ProgressChangedEventHandler(asyncSetCreater_ProgressChanged);

            asyncFullTest = new BackgroundWorker();
            asyncFullTest.DoWork += new DoWorkEventHandler(asyncFullTest_DoWork);
            asyncFullTest.RunWorkerCompleted += new RunWorkerCompletedEventHandler(asyncFullTest_RunWorkerCompleted);
        }

        /// <summary>
        /// Create a new CAPTCHABreaker without a segmenter or solver set yet.
        /// </summary>
        public CAPTCHABreaker()
        {
            segmenter = new Segmenter();

            asyncSolver = new BackgroundWorker();
            asyncSolver.DoWork += new DoWorkEventHandler(asyncSolver_DoWork);
            asyncSolver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(asyncSolver_RunWorkerCompleted);

            asyncSetCreater = new BackgroundWorker();
            asyncSetCreater.DoWork += new DoWorkEventHandler(asyncSetCreater_DoWork);
            asyncSetCreater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(asyncSetCreater_RunWorkerCompleted);
            asyncSetCreater.WorkerReportsProgress = true;
            asyncSetCreater.ProgressChanged += new ProgressChangedEventHandler(asyncSetCreater_ProgressChanged);
        }

        /// <summary>
        /// Specify the segmentation method that will be used to extract patterns from a CAPTCHA image.
        /// </summary>
        public void SetSegmentationMethod(SegmentMethod segmentMethod)
        {
            segmenter.SegmentationMethod = segmentMethod;
        }

        /// <summary>
        /// Specify the solver method that will be used to recognize segmented patterns.
        /// </summary>
        public void SetSolverMethod(Solver solveMethod)
        {
            solver = solveMethod;
            solver.OnTraininProgressChanged += new Action<object, OnTrainingProgressChangeEventArgs>(solver_OnTraininProgressChanged);
            solver.OnTrainingComplete += new Action<object, PatternResult>(solver_OnTrainingComplete);
            solver.OnTestingComplete += new Action<object, PatternResult>(solver_OnTestingComplete);
        }

        private List<Bitmap> Segment(Bitmap image)
        {
            if (OnBeforeSegmentation != null)
            {
                try
                {
                    // To make this thread safe, we need to create a copy
                    Segmenter segmenterCopy;
                    lock (segmenter)
                    {
                        segmenterCopy = segmenter.Clone();
                    }

                    // Send the image to all subscribers so that it can be manipulated to the point where it can be easily segmented
                    segmenterCopy.Image = image;
                    OnBeforeSegmentation(segmenterCopy);

                    // Segment the image using the preferred segmentation method
                    List<Bitmap> segments = segmenterCopy.SegmentationMethod.Segment(segmenterCopy.Image);

                    GlobalMessage.SendMessage(segments, "SEGMENTED");

                    // Resize each image
                    List<Bitmap> resized = segments.Select(s => s.ResizeKeepRatioAndCenter(solver.ExpectedWidth, solver.ExpectedHeight, Color.White)).ToList();

                    //// Blur patterns to aid recognition
                    //Parallel.ForEach(resized, s => new Blur().ApplyInPlace(s));

                    //resized.ForEach(b => b.Save("~" + b.GetHashCode() + ".bmp"));

                    GlobalMessage.SendMessage(resized, "RESIZED");

                    return resized;
                }
                catch (Exception ex)
                {
                    throw new SegmentationException("Error attempting to segment an image.", ex);
                }
            }
            else
            {
                throw new SegmentationEventNotSubscribedToException();
            }
        }

        /// <summary>
        /// This is what it's all about. Supply an CAPTCHA image from a scheme that this class has 
        /// been trained on and you'll get this CAPTCHA breaker's best guess at the answer.
        /// </summary>
        /// <param name="CAPTCHA">The image to solve.</param>
        /// <returns>Hopefully the solution.</returns>
        public string Solve(Bitmap CAPTCHA)
        {
            try
            {
                List<Bitmap> segments = Segment(CAPTCHA);
                return solver.Solve(GetLivePattern(segments));
            }
            catch (Exception ex)
            {
                // Checking Inner Exceptions is a must...
                throw new CaptchaSolverException("Could not solve the given CAPTCHA.", ex);
            }
        }

        /// <summary>
        /// This is what it's all about. Supply an CAPTCHA image from a scheme that this class has 
        /// been trained on and you'll get this CAPTCHA breaker's best guess at the answer.
        /// </summary>
        /// <param name="CAPTCHA">The image file to solve</param>
        /// <returns>Hopefully the solution.</returns>
        public string Solve(string CAPTCHA)
        {
            try
            {
                return Solve((Bitmap)Bitmap.FromFile(CAPTCHA));
            }
            catch (Exception ex)
            {
                throw new CaptchaSolverException("Could not solve the given CAPTCHA.", ex);
            }
        }

        /// <summary>
        /// This is what it's all about. Supply an CAPTCHA image from a scheme that this class has 
        /// been trained on and you'll get this CAPTCHA breaker's best guess at the answer returned in the OnSolvingComplete event.
        /// </summary>
        /// <param name="CAPTCHA">The image to solve.</param>
        public void SolveAsync(Bitmap CAPTCHA)
        {
            if (!asyncSolver.IsBusy)
            {
                asyncSolver.RunWorkerAsync(CAPTCHA);
            }
        }

        /// <summary>
        /// Train on a set of patterns.
        /// </summary>
        /// <param name="set">The set of patterns to train on.</param>
        /// <returns></returns>
        public PatternResult Train(SolutionSet set)
        {
            return Train(set, 1);
        }

        /// <summary>
        /// Train on a set of patterns a specified number of iterations.
        /// </summary>
        /// <param name="set">The set of patterns to train on.</param>
        /// <param name="iterations">The number of iterations to train.</param>
        /// <returns></returns>
        public PatternResult Train(SolutionSet set, int iterations)
        {
            return solver.Train(set.Patterns, iterations);
        }

        /// <summary>
        /// Train on a set of patterns a specified number of iterations.
        /// </summary>
        /// <param name="folder">The folder which contains the images to train on. Each image MUST be named with the solution to the CAPTCHA.</param>
        /// <param name="iterations">The number of iterations to train.</param>
        /// <returns></returns>
        public PatternResult TrainOnSet(string folder, int iterations)
        {
            SolutionSet set = LoadSolutionSet(folder);
            if (set.Patterns.Count == 0)
            {
                set = CreateSolutionSet(folder);
            }

            using (trainingProgress = new ConsoleProgress("Training "))
            {
                return Train(set, iterations);
            }
        }

        /// <summary>
        /// Train on a set of patterns and call back when done.
        /// </summary>
        /// <param name="set">The set of patterns to train on.</param>
        /// <returns></returns>
        public void TrainAsync(SolutionSet set)
        {
            TrainAsync(set, 1);
        }

        /// <summary>
        /// Train on a set of patterns and call back when done.
        /// </summary>
        /// <param name="set">The set of patterns to train on.</param>
        /// <param name="iterations">The number of iterations to train.</param>
        /// <returns></returns>
        public void TrainAsync(SolutionSet set, int iterations)
        {
            solver.TrainAsync(set.Patterns, iterations);
        }

        /// <summary>
        /// Test the system on an unseen set of CAPTCHAs to see how it performs.
        /// </summary>
        /// <param name="set">The set to test on.</param>
        /// <returns></returns>
        public PatternResult Test(SolutionSet set)
        {
            return solver.Test(set.Patterns);
        }

        /// <summary>
        /// Test the system on an unseen set of CAPTCHAs to see how it performs.
        /// </summary>
        /// <param name="folder">The folder which contains the images to test on. Each image MUST be named with the solution to the CAPTCHA.</param>
        /// <returns></returns>
        public PatternResult TestOnSet(string folder)
        {
            SolutionSet set = LoadSolutionSet(folder);
            if (set.Patterns.Count == 0)
            {
                set = CreateSolutionSet(folder);
            }

            return Test(set);
        }

        /// <summary>
        /// Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
        /// </summary>
        /// <param name="testFolder">The folder containing the imgaes to test on.</param>
        /// <param name="reportOutFile">The file to save the report to.</param>
        /// <param name="imageFilter">The filter (E.G., *.bmp) to find images.</param>
        public PatternResult FullTestOnFolder(string testFolder, string reportOutFile, string imageFilter = "*.bmp")
        {
            using (StreamWriter w = new StreamWriter(reportOutFile))
            {
                int correct = 0;
                w.WriteLine("CAPTCHA FULL TEST REPORT " + DateTime.Now.ToShortDateString());
                w.WriteLine("");

                string[] tests = Directory.GetFiles(testFolder, imageFilter, SearchOption.TopDirectoryOnly);

                Parallel.ForEach(tests, file =>
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        string solution = info.Name.Substring(0, info.Name.IndexOf("."));
                        string guess = Solve(file);

                        correct += solution == guess ? 1 : 0;

                        lock (w)
                        {
                            w.WriteLine(String.Format("[{0}] {1} - {2}", solution == guess ? "+" : "-", guess.PadLeft(15, ' '), file));
                        }
                    }
                    catch
                    {
                        // TODO - Don't toss the exception to nowhere land.
                        lock (w)
                        {
                            w.WriteLine(String.Format("[{0}] {1} - {2}", "!", "ERROR".PadLeft(15, ' '), file));
                        }
                    }
                });

                w.WriteLine("");
                w.WriteLine("TOTAL CORRECT: " + correct + " (" + correct + "/" + tests.Length + " = " + (((double)correct / tests.Length) * 100.0).ToString("0.00") + "%)");

                return new PatternResult(tests.Length - correct, ((double)correct / tests.Length) * 100.0);
            }
        }

        /// <summary>
        /// Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
        /// </summary>
        /// <param name="testFolder">The folder containing the imgaes to test on.</param>
        /// <param name="reportOutFile">The file to save the report to.</param>
        /// <param name="imageFilter">The filter (E.G., *.bmp) to find images.</param>
        public void FullTestOnFolderAsync(string testFolder, string reportOutFile, string imageFilter = "*.bmp")
        {
            if (!asyncFullTest.IsBusy)
            {
                asyncFullTest.RunWorkerAsync(new FullTestStart() { Folder = testFolder, Filter = imageFilter, Report = reportOutFile });
            }
        }

        void asyncFullTest_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnFullTestingComplete != null)
            {
                PatternResult result = (PatternResult)e.Result;
                OnFullTestingComplete(this, new OnTestingCompletedEventArgs(result.Error, result.PercentageCorrect));
            }
        }

        void asyncFullTest_DoWork(object sender, DoWorkEventArgs e)
        {
            FullTestStart args = (FullTestStart)e.Argument;
            e.Result = FullTestOnFolder(args.Folder, args.Report, args.Filter);
        }

        /// <summary>
        /// Test the segmentation part of the system only, and output the results to a folder.
        /// </summary>
        /// <param name="imageLocation">The location of the image to segment.</param>
        /// <param name="folderOutputLocation">The folder to output the segments to.</param>
        public void TestSegmentation(string imageLocation, string folderOutputLocation)
        {
            try
            {
                if (!Directory.Exists(folderOutputLocation))
                {
                    Directory.CreateDirectory(folderOutputLocation);
                }

                List<Bitmap> segments = Segment((Bitmap)Bitmap.FromFile(imageLocation));
                int i = 0;
                foreach (Bitmap b in segments)
                {
                    GlobalMessage.SendMessage(b);
                    b.Save(folderOutputLocation + "\\" + (i++).ToString().PadLeft(3, '0') + ".bmp");
                }
            }
            catch (IOException ex)
            {
                throw new SegmentationException("Error attempting to segment an image. Does the image you specified exist? Is it a valid image?", ex);
            }
            catch (Exception ex)
            {
                throw new SegmentationException("Error attempting to segment an image.", ex);
            }
        }

        /// <summary>
        /// Create a solution set for training from an image and it's correct solution.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="solution"></param>
        /// <returns></returns>
        public SolutionSet CreateSolutionSet(Bitmap image, string solution)
        {
            List<Bitmap> segments = Segment(image);
            List<Pattern> patterns = GetPatternList(segments, solution.ToCharStringList());

            // This is the only way (aside from ugly reflection techniques) to create an instance of the solution set.
            return new SolutionSetCreator(patterns) as SolutionSet;
        }

        /// <summary>
        /// Load a pre-created set of solution patterns for either training or testing.
        /// </summary>
        /// <param name="folder">The folder that contains the images and the segmented patterns for training.</param>
        /// <returns></returns>
        public SolutionSet LoadSolutionSet(string folder)
        {
            try
            {
                List<Pattern> patterns = new List<Pattern>();
                List<string> solution = new List<string>();
                List<Bitmap> images = new List<Bitmap>();

                images.Add(new Bitmap(1, 1));
                solution.Add("!");

                string savedir = folder + "\\" + trainersFolder + "\\";

                if (Directory.Exists(savedir))
                {
                    foreach (string dir in Directory.GetDirectories(savedir))
                    {
                        // The solution to every image in a folder is the name of the folder itself
                        solution[0] = dir.Substring(dir.LastIndexOf("\\") + 1);

                        foreach (string file in Directory.GetFiles(dir, "*.bmp", SearchOption.TopDirectoryOnly))
                        {
                            images[0] = ((Bitmap)Bitmap.FromFile(file)).Resize(solver.ExpectedWidth, solver.ExpectedHeight);
                            patterns.AddRange(GetPatternList(images, solution));
                        }
                    }
                }

                // This is the only way (aside from ugly reflection techniques) to create an instance of the solution set.
                return new SolutionSetCreator(patterns) as SolutionSet;
            }
            catch (Exception ex)
            {
                throw new SolutionSetException("Error trying to load a solution set. Does the folder you specified exist and contain a folder of segmented trainers?", ex);
            }
        }

        /// <summary>
        /// Create a Solution Set from a folder of .BMP images where the name of the image is the solution to the CAPTCHA it contains.
        /// E.G., image "d829f4.bmp" would be a CAPTCHA with the solution "d829f4".
        /// </summary>
        /// <param name="folder">The folder containing the images to test on. The directory search for images is NOT recursive.</param>
        /// <returns></returns>
        public SolutionSet CreateSolutionSet(string folder)
        {
            try
            {
                List<Pattern> patterns = new List<Pattern>();

                string savedir = folder + "\\" + trainersFolder + "\\";
                string[] allFiles = Directory.GetFiles(folder, "*.bmp", SearchOption.TopDirectoryOnly);
                int completed = 0;

                // We can safely load each image in parallel
                Parallel.ForEach(allFiles, file =>
                {
                    // Assume the filename is the solution to the CAPTCHA
                    FileInfo info = new FileInfo(file);
                    string filename = info.Name.Substring(0, info.Name.IndexOf("."));

                    // Get list of solutions
                    List<string> solutions = filename.ToCharStringList();

                    // Segment the image and create patterns
                    List<Bitmap> segments = Segment((Bitmap)System.Drawing.Image.FromFile(file));
                    List<Pattern> temp = GetPatternList(segments, solutions);

                    // Save each segmented image to a folder for faster loading later
                    if (!Directory.Exists(savedir))
                    {
                        Directory.CreateDirectory(savedir);
                    }
                    for (int i = 0; i < Math.Min(segments.Count, solutions.Count); i++)
                    {
                        if (!Directory.Exists(savedir + solutions[i]))
                        {
                            Directory.CreateDirectory(savedir + solutions[i]);
                        }

                        segments[i].Save(savedir + solutions[i] + "\\" + filename + "_" + i + "_" + segments[i].GetHashCode().ToString("X") + ".bmp");
                    }

                    // Add them to the list of patterns
                    lock (patterns)
                    {
                        patterns.AddRange(temp);
                    }

                    // Report progress if we are running asynchronously
                    if (asyncSetCreater.IsBusy)
                    {
                        completed++;
                        double percent = ((double)completed / (double)allFiles.Length) * 100.0;
                        asyncSetCreater.ReportProgress((int)percent);
                    }
                });

                // This is the only way (aside from ugly reflection techniques) to create an instance of the solution set.
                return new SolutionSetCreator(patterns) as SolutionSet;
            }
            catch (Exception ex)
            {
                throw new SolutionSetException("Error trying create a solution set. Does the folder you specified exist and contain the expected images?", ex);
            }
        }

        /// <summary>
        /// Create a Solution Set from a folder of .BMP images where the name of the image is the solution to the CAPTCHA it contains.
        /// E.G., image "d829f4.bmp" would be a CAPTCHA with the solution "d829f4".
        /// </summary>
        /// <param name="folder">The folder containing the images to test on. The directory search for images is NOT recursive.</param>
        /// <returns></returns>
        public void CreateSolutionSetAsync(string folder)
        {
            if (!asyncSetCreater.IsBusy)
            {
                asyncSetCreater.RunWorkerAsync(folder);
            }
        }

        /// <summary>
        /// Disable all Console output from this library.
        /// </summary>
        public void SilenceConsoleOut()
        {
            ConsoleProgress.AllowOutput = false;
        }

        void asyncSetCreater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnSolverSetCreated != null)
            {
                OnSolverSetCreated(this, new OnSolverSetCreatedEventArgs(e.Result as SolutionSet));
            }
        }

        void asyncSetCreater_DoWork(object sender, DoWorkEventArgs e)
        {
            asyncSetCreaterStart = DateTime.Now;
            e.Result = CreateSolutionSet((string)e.Argument);
        }

        void asyncSolver_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnSolvingComplete != null)
            {
                OnSolvingComplete(this, new OnSolverCompletedEventArgs((string)e.Result));
            }
        }

        void asyncSolver_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Solve(e.Argument as Bitmap);
        }

        void solver_OnTestingComplete(object arg1, PatternResult arg2)
        {
            if (OnTestingComplete != null)
            {
                OnTestingComplete(this, new OnTestingCompletedEventArgs(arg2.Error, arg2.PercentageCorrect));
            }
        }

        void solver_OnTrainingComplete(object arg1, PatternResult arg2)
        {
            if (OnTrainingComplete != null)
            {
                OnTrainingComplete(this, new OnTrainingCompletedEventArgs(arg2.Error));
            }
        }

        void solver_OnTraininProgressChanged(object arg1, OnTrainingProgressChangeEventArgs arg2)
        {
            if (OnTrainingProgressChanged != null)
            {
                OnTrainingProgressChanged(this, new OnTrainingProgressChangeEventArgs(arg2.Progress, arg2.Error));
            }

            if (trainingProgress != null)
            {
                trainingProgress.SetPercent(arg2.Progress);
            }
        }

        void asyncSetCreater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (OnSolverSetProgressChanged != null && e.ProgressPercentage != 0)
            {
                // I have nothing against Windows loading bars...
                long taken = (DateTime.Now - asyncSetCreaterStart).Ticks;
                TimeSpan remaining = new TimeSpan((taken / e.ProgressPercentage) * (100 - e.ProgressPercentage));

                OnSolverSetProgressChanged(this, new OnSolverProgressChangedEventArgs(e.ProgressPercentage, remaining));
            }
        }

        private List<Pattern> GetPatternList(List<Bitmap> segments, List<string> solutions)
        {
            try
            {
                List<Pattern> list = new List<Pattern>();

                // If there are an unequal number of segments and solutions, then we probably segmented incorrectly.
                // We cannot tell the segmenter to segment to the same number of letters for an image we know the answer to, 
                // because then when we try to segment an image we don't have the solution to we won't know the answer.
                for (int i = 0; i < Math.Min(segments.Count, solutions.Count); i++)
                {
                    DoubleVector inputs = new DoubleVector(solver.ExpectedWidth * solver.ExpectedHeight);
                    DoubleVector outputs = new DoubleVector(solver.CharacterSet.Count);

                    // Create the input pattern
                    for (int x = 0; x < solver.ExpectedWidth; x++)
                    {
                        for (int y = 0; y < solver.ExpectedHeight; y++)
                        {
                            Color c = segments[i].GetPixel(x, y);
                            inputs[x * solver.ExpectedHeight + y] = 1.0 - (c.R * c.G * c.B) / (255.0 * 255.0 * 255.0);
                        }
                    }

                    // Create the output pattern. Each element corresponds to one character in the set of all possible characters.
                    // Use numbers close to 0 and 1, not the actual numbers to help in training.
                    outputs.Fill(0.01);
                    outputs[solver.CharacterSet.IndexOf(solutions[i])] = 0.99;

                    list.Add(new Pattern() { Inputs = inputs, Outputs = outputs });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new PatternGenerationException("Error trying to create a list of patterns from a list of bitmaps and their corresponding solutions.", ex);
            }
        }

        /// <summary>
        /// Get a set of patterns from a list of bitmaps without the solution encoded in them.
        /// This is used primarily for generating the input to the system for a live, unseen CAPTCHA.
        /// </summary>
        private List<Pattern> GetLivePattern(List<Bitmap> segments)
        {
            try
            {
                List<Pattern> list = new List<Pattern>();

                for (int i = 0; i < segments.Count; i++)
                {
                    DoubleVector inputs = new DoubleVector(solver.ExpectedWidth * solver.ExpectedHeight);

                    // Create the input pattern
                    for (int x = 0; x < solver.ExpectedWidth; x++)
                    {
                        for (int y = 0; y < solver.ExpectedHeight; y++)
                        {
                            Color c = segments[i].GetPixel(x, y);
                            inputs[x * solver.ExpectedHeight + y] = 1.0 - (c.R * c.G * c.B) / (255.0 * 255.0 * 255.0);
                        }
                    }

                    list.Add(new Pattern() { Inputs = inputs });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new PatternGenerationException("Error trying to create a list of patterns from a list of bitmaps.", ex);
            }
        }

        /// <summary>
        /// Save this breaker and all the trained data to a database file.
        /// </summary>
        /// <param name="filename">The file to save as</param>
        public void SaveToFile(string filename)
        {
            SaveToFile(filename, "SKOT");
        }

        /// <summary>
        /// Save this breaker and all the trained data to a database file.
        /// </summary>
        /// <param name="filename">The file to save as</param>
        /// <param name="metadata">Extra information to save to the file along with the database</param>
        public void SaveToFile(string filename, string metadata)
        {
            try
            {
                // Backup old file
                if (File.Exists(filename))
                {
                    if (File.Exists(filename + "~bak"))
                    {
                        File.Delete(filename + "~bak");
                    }
                    File.Move(filename, filename + "~bak");
                }

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                using (GZipStream gz = new GZipStream(fs, CompressionMode.Compress))
                using (BinaryWriter w = new BinaryWriter(gz))
                {
                    w.Write(metadata);
                    solver.Save(w);
                }
            }
            catch (Exception ex)
            {
                throw new BreakerDataBaseException("Error trying to save the CAPTCHA Breaker DB to a file.", ex);
            }
        }

        /// <summary>
        /// Get just the metadata part of the saved DB file.
        /// </summary>
        /// <param name="filename">The file containing the saved database</param>
        /// <returns></returns>
        public string LoadMetadataFromFile(string filename)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                using (BinaryReader r = new BinaryReader(gz))
                {
                    return r.ReadString();
                }
            }
            catch (Exception ex)
            {
                throw new BreakerDataBaseException("Error trying to get the metadata from a CAPTCHA Breaker DB file. The file may be corrupted.", ex);
            }
        }

        /// <summary>
        /// Load this captcha breaker from a DB file.
        /// </summary>
        /// <param name="filename">The file containing the saved contents of this breaker.</param>
        public void LoadFromFile(string filename)
        {
            string metadata;
            LoadFromFile(filename, out metadata);
        }

        /// <summary>
        /// Load this captcha breaker from a base64 encoded string.
        /// </summary>
        public void LoadFromBase64(string base64)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                using (BinaryReader r = new BinaryReader(gz))
                {
                    string metadata = r.ReadString();
                    solver.Load(r);
                }
            }
            catch (Exception ex)
            {
                throw new BreakerDataBaseException("Could not load the DB file for the CAPTCHA breaker from a base64 encoded string. Did you change something in the setup portion of the breaker script since the last load?", ex);
            }
        }

        /// <summary>
        /// Load this captcha breaker from a DB file.
        /// </summary>
        /// <param name="filename">The file containing the saved contents of this breaker.</param>
        /// <param name="metadata">Returns the meta-data associated with this DB</param>
        public void LoadFromFile(string filename, out string metadata)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                using (BinaryReader r = new BinaryReader(gz))
                {
                    metadata = r.ReadString();
                    solver.Load(r);
                }
            }
            catch (IOException ex)
            {
                throw new BreakerDataBaseException("Could not load the DB file for the CAPTCHA breaker. Does the file exist? Did you change something in the setup portion of the breaker script since the last load?", ex);
            }
            catch (Exception ex)
            {
                throw new BreakerDataBaseException("Could not load the DB file for the CAPTCHA breaker. Did you change something in the setup portion of the breaker script since the last load?", ex);
            }
        }
    }
}

public struct FullTestStart
{
    public string Folder { get; set; }
    public string Filter { get; set; }
    public string Report { get; set; }
}