using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.CAPTCHA;
using ScottClayton.Image;
using System.Drawing;
using ScottClayton.CAPTCHA.Utility;
using ScottClayton.Neural;
using System.IO;

namespace ScottClayton.Interpreter
{
    public class CaptchaInterpreter
    {
        public string ProgramCode { get; set; }

        public Bitmap ImageToBreak { get; set; }

        private List<string> PreconditionCode { get; set; }

        private CAPTCHABreaker captcha;

        private MODE mode = MODE.WARN;

        //private string recreationData;

        //private const string SPLITTER = ";~|!|~;";

        public delegate void BitmapMessageHandler(Bitmap image);
        public static event BitmapMessageHandler OnGlobalBitmapMessage;

        public delegate void ErrorNotificationHandler(string message, Exception ex);
        public static event ErrorNotificationHandler OnError;

        public delegate void InformationMessageHandler(string message);
        public static event InformationMessageHandler OnInfo;

        private bool fatalError = false;

        private Dictionary<string, Bitmap> subtractionImages;

        public CaptchaInterpreter(string program)
            : this(program, null)
        {
        }

        public CaptchaInterpreter(string program, Bitmap image)
        {
            ProgramCode = program;

            foreach (string term in CONST.LINE_TERMINATORS)
            {
                ProgramCode = ProgramCode.Replace(term, CONST.STATEMENT_TERMINATOR);
            }

            if (image != null)
            {
                ImageToBreak = image;
            }

            subtractionImages = new Dictionary<string, Bitmap>();

            captcha = new CAPTCHABreaker();
            captcha.OnBeforeSegmentation += new CAPTCHABreaker.BeforeSegmentHandler(captcha_OnBeforeSegmentation);

            GlobalMessage.OnGlobalBitmapMessage += new GlobalMessage.BitmapMessageHandler(GlobalMessage_OnGlobalBitmapMessage);
        }

        public static void AllowGlobalDegugMessages(bool allow = true)
        {
            GlobalMessage.ALLOW_MESSAGES = allow;
        }

        void GlobalMessage_OnGlobalBitmapMessage(List<Bitmap> images, string tag)
        {
            if (OnGlobalBitmapMessage != null)
            {
                OnGlobalBitmapMessage(images.MergeHorizontal());
            }
        }

        public string Execute()
        {
            string solution = "";
            List<string> statements = ProgramCode.Split(new string[] { CONST.STATEMENT_TERMINATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();

            PreconditionCode = new List<string>();
            bool gatheringPreconditions = false;

            foreach (string command in statements)
            {
                if (fatalError)
                {
                    break;
                }

                try
                {
                    // Lines that start with comment characters are completely ignored
                    if (command.Trim().StartsWith(CONST.COMMENT_CHARACTER))
                    {
                        continue;
                    }

                    // Get the list of line arguments
                    List<string> args = command.Split(new string[] { CONST.ARGUMENT_SEPERATER }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                    args[0] = args[0].ToUpper();

                    if (gatheringPreconditions)
                    {
                        switch (args.GetArg(0))
                        {
                            case "ENDPRECONDITIONS":
                                gatheringPreconditions = false;
                                Out("Preconditions loaded");
                                break;

                            default:
                                PreconditionCode.Add(command);
                                break;
                        }
                    }
                    else
                    {
                        switch (args.GetArg(0))
                        {
                            case "SETMODE":
                                switch (args.GetArg(1, "WARN").ToUpper())
                                {
                                    case "WARN":
                                        mode = MODE.WARN;
                                        Out("Mode set to WARN");
                                        break;
                                    case "QUIET":
                                        mode = MODE.QUIET;
                                        Out("Mode set to QUIET");
                                        break;
                                    case "ALL":
                                        mode = MODE.ALL;
                                        Out("Mode set to ALL");
                                        break;
                                    default:
                                        Error("Unknown mode \"" + args.GetArg(1, "?") + "\" in call to SETMODE");
                                        break;
                                }
                                break;

                            case "SETUPSOLVER":
                                string type = args.GetArg(1).ToUpper();
                                string charset = args.GetQuotedArg(2);
                                int width = args.GetArg(3).ToInt();
                                int height = args.GetArg(4).ToInt();
                                int chars = args.GetArg(5).ToInt();
                                bool merge = args.GetArg(5).ToString().ToUpper().Equals("Y");
                                int hidden = args.GetArg(6).ToInt();
                                double learn = args.GetArg(7).ToDouble();

                                switch (type)
                                {
                                    case "SNN":
                                        switch (args.Count - 2)
                                        {
                                            case 3:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            case 4:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, chars));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            case 5:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, hidden, chars));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            case 6:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, hidden, chars, learn));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "MNN":
                                        switch (args.Count - 2)
                                        {
                                            case 3:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            case 4:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, chars));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            case 5:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, hidden, chars));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            case 6:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, hidden, chars, learn));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "BVS":
                                        switch (args.Count - 2)
                                        {
                                            case 3:
                                                captcha.SetSolverMethod(new BitmapSubtractionSolver(charset, width, height));
                                                Out("Bitmap Vector Solver Setup Complete");
                                                break;
                                            case 4:
                                                captcha.SetSolverMethod(new BitmapSubtractionSolver(charset, width, height, merge));
                                                Out("Bitmap Vector Solver Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "HS":
                                        switch (args.Count - 2)
                                        {
                                            case 3:
                                                captcha.SetSolverMethod(new HistogramSolver(charset, width, height));
                                                Out("Histogram Solver Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "CV":
                                        switch (args.Count - 2)
                                        {
                                            case 3:
                                                captcha.SetSolverMethod(new ContourAnalysisSolver(charset, width, height));
                                                Out("Contour Analysis Solver Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    default:
                                        Error("Wrong type of SOLVER in call to SETUPSOLVER - Must be SNN, MNN, BVS, or CV");
                                        break;
                                }
                                break;

                            case "DEFINEPRECONDITIONS":
                                gatheringPreconditions = true;
                                break;

                            case "SETUPSEGMENTER":
                                string xtype = args.GetArg(1).ToUpper();
                                int segwidth = args.GetArg(2).ToInt();
                                int segheight = args.GetArg(3).ToInt();
                                int segblobs = args.GetArg(4).ToInt();

                                switch (xtype)
                                {
                                    case "BLOB":
                                        switch (args.Count - 2)
                                        {
                                            case 2:
                                                captcha.SetSegmentationMethod(new BlobSegmentMethod(segwidth, segheight));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            case 3:
                                                captcha.SetSegmentationMethod(new BlobSegmentMethod(segwidth, segheight, segblobs));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSEGMENTER");
                                                break;
                                        }
                                        break;

                                    case "HIST":
                                        switch (args.Count - 2)
                                        {
                                            case 1:
                                                captcha.SetSegmentationMethod(new HistogramSegmentMethod(tolerance: segwidth));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            case 2:
                                                captcha.SetSegmentationMethod(new HistogramSegmentMethod(tolerance: segwidth, numChars: segheight));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            default:
                                                Error("Wrong number of arguments in call to SETUPSEGMENTER");
                                                break;
                                        }
                                        break;

                                    default:
                                        Error("Wrong type of SOLVER in call to SETUPSEGMENTER - Must be BLOB or HIST");
                                        break;
                                }
                                break;

                            case "TESTSEGMENT":
                                string tsimage = args.GetQuotedArg(1);
                                string tsfolder = args.GetQuotedArg(2);

                                switch (args.Count - 1)
                                {
                                    case 2:
                                        captcha.TestSegmentation(tsimage, tsfolder);
                                        Out("Test segmentation of " + args.GetArg(1) + " complete");
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to TESTSEGMENT");
                                        break;
                                }
                                break;

                            case "TRAIN":
                                string tfolder = args.GetQuotedArg(1);
                                int titerations = args.GetArg(2).ToInt();

                                switch (args.Count - 1)
                                {
                                    case 1:
                                        PatternResult tresult1 = captcha.TrainOnSet(tfolder, 1);
                                        Out("Training Complete", true);
                                        Out("Error Approximation: " + tresult1.Error.ToString());
                                        break;
                                    case 2:
                                        PatternResult tresult2 = captcha.TrainOnSet(tfolder, titerations);
                                        Out("Training Complete", true);
                                        Out("Error Approximation: " + tresult2.Error.ToString());
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to TRAIN");
                                        break;
                                }
                                break;

                            case "TEST":
                                string testfolder = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    case 1:
                                        PatternResult tresult3 = captcha.TestOnSet(testfolder);
                                        Out("Testing Complete. Percent Correct: " + tresult3.PercentageCorrect.ToString("0.00") + "%", true);
                                        Append("x-test.txt", tresult3.PercentageCorrect.ToString("0.00") + "%");
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to TEST");
                                        break;
                                }
                                break;

                            case "FULLTEST":
                                string fullfolder = args.GetQuotedArg(1);
                                string fullreport = args.GetQuotedArg(2);
                                string fullext = args.GetQuotedArg(3);

                                switch (args.Count - 1)
                                {
                                    case 2:
                                        PatternResult tresult4 = captcha.FullTestOnFolder(fullfolder, fullreport);
                                        Out("FULL Testing Complete. Percent Correct: " + tresult4.PercentageCorrect.ToString("0.00") + "%", true);
                                        break;
                                    case 3:
                                        PatternResult tresult5 = captcha.FullTestOnFolder(fullfolder, fullreport, fullext);
                                        Out("FULL Testing Complete. Percent Correct: " + tresult5.PercentageCorrect.ToString("0.00") + "%", true);
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to FULLTEST");
                                        break;
                                }
                                break;

                            case "SOLVE":
                                string imagelocation = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    case 1:
                                        if (args.GetArg(1).ToUpper() == "%IMAGE%")
                                        {
                                            if (ImageToBreak != null)
                                            {
                                                solution = captcha.Solve(ImageToBreak);
                                                Out("CAPTCHA Solution: " + solution, true);
                                            }
                                            else
                                            {
                                                Error("Image could not be found!");
                                            }
                                        }
                                        else
                                        {
                                            solution = captcha.Solve(imagelocation);
                                            Out("CAPTCHA Solution: " + solution, true);
                                        }
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to SOLVE");
                                        break;
                                }
                                break;

                            case "SAVE":
                                string saveLoc = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    case 0:
                                        captcha.SaveToFile("captcha.db");
                                        Out("CAPTCHA Breaking Solution Saved");
                                        break;
                                    case 1:
                                        captcha.SaveToFile(saveLoc);
                                        Out("CAPTCHA Breaking Solution Saved");
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to SAVE");
                                        break;
                                }
                                break;

                            case "LOAD":
                                string dbLoc = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    case 0:
                                        //List<string> recreate = captcha.LoadMetadataFromFile(dbLoc).Split(new string[] { SPLITTER }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                        captcha.LoadFromFile("captcha.db");
                                        Out("CAPTCHA Breaking Solution Loaded");
                                        break;
                                    case 1:
                                        captcha.LoadFromFile(dbLoc);
                                        Out("CAPTCHA Breaking Solution Loaded");
                                        break;
                                    default:
                                        Error("Wrong number of arguments in call to LOAD");
                                        break;
                                }
                                break;

                            case "SAY":
                                string say = args.GetQuotedArg(1);
                                Out(say, true);
                                break;

                            case "WAIT":
                                Console.WriteLine("Press a key to continue.");
                                try
                                {
                                    Console.ReadKey();
                                }
                                catch
                                {
                                    Console.Read();
                                }
                                break;

                            default:
                                Error("Unknown command \"" + args.GetArg(0) + "\"");
                                break;
                        }
                    }
                }
                catch (BreakerDataBaseException ex)
                {
                    // Yes, display the actual error message straight to the user...
                    Error(ex.ToString());
                }
                catch (PatternGenerationException ex)
                {
                    Error(ex.ToString());
                }
                catch (SolutionSetException ex)
                {
                    Error(ex.ToString());
                }
                catch (SegmentationException ex)
                {
                    Error(ex.ToString());
                }
                catch (CaptchaSolverException ex)
                {
                    Error(ex.ToString());
                }
                catch (Exception ex)
                {
                    // All of the above exceptions should produce meaningful error messages, but we aren't sure about that here.
                    try
                    {
                        File.WriteAllText("ERROR.txt", ex.ToString());
                        Error("Error performing a breaker operation! Details have been saved to ERROR.txt.");
                        Error(ex.ToString());
                    }
                    catch
                    {
                        Error(ex.ToString());
                    }

                    Console.WriteLine("Press a key...");
                    try
                    {
                        Console.ReadKey();
                    }
                    catch
                    {
                        Console.Read();
                    }
                }
            }

            return solution;
        }

        void captcha_OnBeforeSegmentation(Segmenter s)
        {
            foreach (string command in PreconditionCode)
            {
                if (fatalError)
                {
                    break;
                }

                try
                {
                    // Lines that start with comment characters are completely ignored
                    if (command.Trim().StartsWith(CONST.COMMENT_CHARACTER))
                    {
                        continue;
                    }

                    // Get the list of line arguments
                    List<string> args = command.Trim().Split(new string[] { CONST.ARGUMENT_SEPERATER }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                    args[0] = args[0].ToUpper();

                    switch (args.GetArg(0))
                    {
                        case "RESIZE":
                            int rswidth = args.GetArg(1).ToInt();
                            int rsheight = args.GetArg(2).ToInt();

                            switch (args.Count - 1)
                            {
                                case 2:
                                    s.Resize(rswidth, rsheight);
                                    Out("Image Resized to " + rswidth + "x" + rsheight);
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to RESIZE");
                                    break;
                            }
                            break;

                        case "ERODE":
                            int etimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.ErodeShapes(Color.White);
                                    Out("Image Edges Eroded");
                                    break;
                                case 1:
                                    for (int i = 0; i < etimes; i++)
                                    {
                                        s.ErodeShapes(Color.White);
                                    }
                                    Out("Image Edges Eroded");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to ERODE");
                                    break;
                            }
                            break;

                        case "GROW":
                            int gtimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.GrowShapes(Color.White);
                                    Out("Image Edges Grown");
                                    break;
                                case 1:
                                    for (int i = 0; i < gtimes; i++)
                                    {
                                        s.GrowShapes(Color.White);
                                    }
                                    Out("Image Edges Grown");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to GROW");
                                    break;
                            }
                            break;

                        case "OUTLINE":
                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.Outline();
                                    Out("Image Outline Filter Applied");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to OUTLINE");
                                    break;
                            }
                            break;

                        case "SUBTRACT":
                            string img = args.GetQuotedArg(1);

                            switch (args.Count - 1)
                            {
                                case 1:
                                    Bitmap imageToSub = null;
                                    lock (subtractionImages)
                                    {
                                        if (subtractionImages.ContainsKey(img.ToUpper()))
                                        {
                                            imageToSub = subtractionImages[img.ToUpper()].CloneFull();
                                        }
                                        else
                                        {
                                            // Save it so that we don't have to keep loading it
                                            imageToSub = (Bitmap)Bitmap.FromFile(img);
                                            subtractionImages.Add(img.ToUpper(), imageToSub.CloneFull());
                                        }
                                    }
                                    s.Subtract(imageToSub);
                                    Out("Image Subtracted");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to SUBTRACT");
                                    break;
                            }
                            break;

                        case "MEDIAN":
                            int mtimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.Median();
                                    Out("Median Filter Applied");
                                    break;
                                case 1:
                                    for (int i = 0; i < mtimes; i++)
                                    {
                                        s.Median();
                                    }
                                    Out("Median Filter Applied");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to MEDIAN");
                                    break;
                            }
                            break;

                        case "INVERT":
                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.Invert();
                                    Out("Image Inverted");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to INVERT");
                                    break;
                            }
                            break;

                        case "CROP":
                            int cx = args.GetArg(1).ToInt();
                            int cy = args.GetArg(2).ToInt();
                            int cw = args.GetArg(3).ToInt();
                            int ch = args.GetArg(4).ToInt();

                            switch (args.Count - 1)
                            {
                                case 4:
                                    s.Crop(new Rectangle(cx, cy, cw, ch));
                                    Out("Image Cropped");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to CROP");
                                    break;
                            }
                            break;

                        case "BILATERALSMOOTH":
                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.EdgePreservingSmooth();
                                    Out("Image Smoothed");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to BILATERALSMOOTH");
                                    break;
                            }
                            break;

                        case "COLORFILLBLOBS":
                            double cfbtolerance = args.GetArg(1).ToDouble();
                            double cfbbkgtol = args.GetArg(2).ToDouble();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.ColorFillBlobs(1.0, Color.White, 1.0);
                                    Out("Blobs within 1.00 tolerance (L*a*b* color space) filled");
                                    break;
                                case 2:
                                    s.ColorFillBlobs(cfbtolerance, Color.White, cfbbkgtol);
                                    Out("Blobs within " + cfbtolerance.ToString("0.00") + " tolerance (L*a*b* color space) filled");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to COLORFILLBLOBS");
                                    break;
                            }
                            break;

                        case "REMOVESMALLBLOBS":
                            int rsbcount = args.GetArg(1).ToInt();
                            int rsbwidth = args.GetArg(2).ToInt();
                            int rsbheight = args.GetArg(3).ToInt();
                            int rsbtolerance = args.GetArg(4).ToInt();

                            switch (args.Count - 1)
                            {
                                case 3:
                                    s.RemoveSmallBlobs(rsbcount, rsbwidth, rsbheight, Color.White);
                                    Out("Small blobs removed from image");
                                    break;
                                case 4:
                                    s.RemoveSmallBlobs(rsbcount, rsbwidth, rsbheight, Color.White, rsbtolerance);
                                    Out("Small blobs removed from image");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to REMOVESMALLBLOBS");
                                    break;
                            }
                            break;

                        case "BLACKANDWHITE":
                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.BlackAndWhite();
                                    Out("Image binarized to black and white");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to BLACKANDWHITE");
                                    break;
                            }
                            break;

                        case "BINARIZE":
                            int thresh = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 1:
                                    s.Binarize(thresh);
                                    Out("Image binarized to black and white");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to BINARIZE");
                                    break;
                            }
                            break;

                        case "REMOVENONCOLOR":
                            int dist = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.RemoveNonColor(3);
                                    Out("All grayscale colors removed from image");
                                    break;
                                case 1:
                                    s.RemoveNonColor(dist);
                                    Out("All grayscale colors removed from image");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to REMOVENONCOLOR");
                                    break;
                            }
                            break;

                        case "KEEPONLYMAINCOLOR":
                            int th = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                case 1:
                                    s.KeepOnlyMostCommonColor(th);
                                    Out("All but the most common color removed");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to KEEPONLYMAINCOLOR");
                                    break;
                            }
                            break;

                        case "SAVESAMPLE":
                            string filename = args.GetQuotedArg(1);

                            switch (args.Count - 1)
                            {
                                case 1:
                                    s.TrySave(filename);
                                    Out("Image saved as " + args.GetArg(1));
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to SAVESAMPLE");
                                    break;
                            }
                            break;

                        case "MEANSHIFT":
                            int msiterations = args.GetArg(1).ToInt();
                            int msradius = args.GetArg(2).ToInt();
                            double mstolerance = args.GetArg(3).ToDouble();

                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.MeanShiftFilter(1, 1, 1);
                                    Out("Mean Shift Filter (" + msiterations + " iterations)");
                                    break;
                                case 3:
                                    s.MeanShiftFilter(msiterations, msradius, mstolerance);
                                    Out("Mean Shift Filter (" + msiterations + " iterations)");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to MEANSHIFT");
                                    break;
                            }
                            break;

                        case "FILLWHITE":
                            int fwx = args.GetArg(1).ToInt();
                            int fwy = args.GetArg(2).ToInt();

                            switch (args.Count - 1)
                            {
                                case 2:
                                    s.FloodFill(new Point(fwx, fwy), 1, Color.White);
                                    Out("Point flood filled with white");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to MEANSHIFT");
                                    break;
                            }
                            break;

                        case "CONVOLUTE":
                            int c_a = args.GetArg(1).ToInt();
                            int c_b = args.GetArg(2).ToInt();
                            int c_c = args.GetArg(3).ToInt();
                            int c_d = args.GetArg(4).ToInt();
                            int c_e = args.GetArg(5).ToInt();
                            int c_f = args.GetArg(6).ToInt();
                            int c_g = args.GetArg(7).ToInt();
                            int c_h = args.GetArg(8).ToInt();
                            int c_i = args.GetArg(9).ToInt();

                            switch (args.Count - 1)
                            {
                                case 9:
                                    s.ConvolutionFilter(c_a, c_b, c_c, c_d, c_e, c_f, c_g, c_h, c_i);
                                    Out("Convolution filter applied to image.");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to CONVOLUTE");
                                    break;
                            }
                            break;

                        case "HISTOGRAMROTATE":
                            switch (args.Count - 1)
                            {
                                case 0:
                                    s.ResizeRotateCut();
                                    Out("Image rotated until best histogram was found");
                                    break;
                                case 1:
                                    s.ResizeRotateCut(true);
                                    Out("Image rotated until best histogram was found (HISTOGRAM DEBUG OVERLAY APPLIED)");
                                    break;
                                default:
                                    Error("Wrong number of arguments in call to SAVESAMPLE");
                                    break;
                            }
                            break;

                        case "WAIT":
                            Console.WriteLine("Press a key to continue.");
                            try
                            {
                                Console.ReadKey();
                            }
                            catch
                            {
                                Console.Read();
                            }
                            break;

                        default:
                            Error("Unknown precondition command \"" + args.GetArg(0) + "\"");
                            break;
                    }
                }
                catch (ImageProcessingException ex)
                {
                    Error(ex.ToString());
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.WriteAllText("ERROR.txt", ex.ToString());
                        Error("Error performing an image operation! Details have been saved to ERROR.txt.");
                        Error(ex.ToString());
                    }
                    catch
                    {
                        Error(ex.ToString());
                    }
                }
            }
        }

        private void Error(string message)
        {
            if (OnError != null)
            {
                OnError(message, null);
            }

            fatalError = true;
            Console.WriteLine("ERROR: " + message); 
            
            try
            {
                Console.ReadKey();
            }
            catch
            {
                Console.Read();
            }
        }

        private void Warn(string message)
        {
            switch (mode)
            {
                case MODE.WARN:
                    Console.WriteLine("WARN: " + message);
                    if (OnInfo != null)
                    {
                        OnInfo("WARN: " + message);
                    }
                    break;
                case MODE.ALL:
                    Console.WriteLine("WARN: " + message);
                    if (OnInfo != null)
                    {
                        OnInfo("WARN: " + message);
                    }
                    break;
            }
        }

        private void Out(string message, bool force = false)
        {
            if (mode == MODE.ALL || force)
            {
                if (OnInfo != null)
                {
                    OnInfo(message);
                }

                Console.WriteLine(message);
            }
        }

        public static Bitmap MergeVertical(Bitmap a, Bitmap b)
        {
            return new List<Bitmap>() { a, b }.MergeVertical();
        }

        public static void Append(string file, string msg)
        {
            using (StreamWriter w = new StreamWriter(file, true))
            {
                w.WriteLine(msg);
            }
        }
    }

    enum MODE
    {
        QUIET,
        WARN,
        ALL
    }

    class CONST
    {
        public static string[] LINE_TERMINATORS = new string[] { "\r", "\n" };
        public const string STATEMENT_TERMINATOR = "{SPLIT}";
        public const string COMMENT_CHARACTER = "*";
        public const string ARGUMENT_SEPERATER = ",";
    }
}
