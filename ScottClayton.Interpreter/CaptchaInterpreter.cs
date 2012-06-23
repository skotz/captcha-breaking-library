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

//
// Note: Anything prefixed with the SKOTDOC command is for the automatic documentation generator for the Wiki page on Google Docs.
//
// #SKOTDOC.DEFINESECTION Setup These are the functions that are placed towards the top of CBL scripts. They set up the methods that will be used to segment and untimately solve the CAPTCHA images.
// #SKOTDOC.DEFINESECTION Preprocess These are the functions that are placed between the {{{DEFINEPRECONDITIONS}}} and {{{ENDPRECONDITIONS}}} commands. They are run for each image in the set to precondition the image before trying to segment out individual letters.
// #SKOTDOC.DEFINESECTION Working These are the functions that are used towards the end of CBL scripts. Most of the functions in this group are used temporarily while developing, testing, or measuring the effectiveness of the script.
//
// #SKOTDOC.VERBATIM =CBL Syntax Documentation= 
// #SKOTDOC.VERBATIM Here is the basic documentation for each CBL (CAPTCHA Breaking Language) command currently available within the scripting language (there is more you can access when using the .NET library directly). 
// #SKOTDOC.VERBATIM The plan is to have a programming guide complete with examples and a program structure guide in the near future, but that is dependent on outside forces at the moment. 
//

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
                            // #SKOTDOC.BLOCKSTART ENDPRECONDITIONS
                            // #SKOTDOC.BLOCKTYPE Setup
                            // #SKOTDOC.BLOCKDESC End the preconditioning loop block.
                            // #SKOTDOC.FUNCSTART
                            // #SKOTDOC.FUNCDESC End the preconditioning loop block.
                            case "ENDPRECONDITIONS":
                                gatheringPreconditions = false;
                                Out("Preconditions loaded");
                                break;
                            // #SKOTDOC.FUNCEND
                            // #SKOTDOC.BLOCKEND

                            default:
                                PreconditionCode.Add(command);
                                break;
                        }
                    }
                    else
                    {
                        switch (args.GetArg(0))
                        {
                            // #SKOTDOC.BLOCKSTART SETMODE
                            // #SKOTDOC.BLOCKTYPE Setup
                            // #SKOTDOC.BLOCKDESC Set the level of debugger output to the screen when the script is run in a console.
                            case "SETMODE":
                                switch (args.GetArg(1, "WARN").ToUpper())
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Set the level of debugger output to the screen when the script is run in a console.
                                    // #SKOTDOC.LITERAL WARN Only output error or warning messages.
                                    case "WARN":
                                        mode = MODE.WARN;
                                        Out("Mode set to WARN");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Set the level of debugger output to the screen when the script is run in a console.
                                    // #SKOTDOC.LITERAL QUIET Do not print any information to the screen unless something fatal happened.
                                    case "QUIET":
                                        mode = MODE.QUIET;
                                        Out("Mode set to QUIET");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Set the level of debugger output to the screen when the script is run in a console.
                                    // #SKOTDOC.LITERAL ALL Output all messages including errors, warnings, and normal informational messages. 
                                    case "ALL":
                                        mode = MODE.ALL;
                                        Out("Mode set to ALL");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Unknown mode \"" + args.GetArg(1, "?") + "\" in call to SETMODE");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART SETUPSOLVER
                            // #SKOTDOC.BLOCKTYPE Setup
                            // #SKOTDOC.BLOCKDESC Set up the solver which is responsible for determining what letter an individual picture represents.
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
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a fully connected, backpropagation neural network.
                                            // #SKOTDOC.LITERAL SNN Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            case 3:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a fully connected, backpropagation neural network.
                                            // #SKOTDOC.LITERAL SNN Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            case 4:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, chars));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a fully connected, backpropagation neural network.
                                            // #SKOTDOC.LITERAL SNN Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG HiddenNeurons The number of neurons to put in the middle (hidden) layer of the neural network.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            case 5:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, hidden, chars));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a fully connected, backpropagation neural network.
                                            // #SKOTDOC.LITERAL SNN Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG HiddenNeurons The number of neurons to put in the middle (hidden) layer of the neural network.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            // #SKOTDOC.FUNCARG LearnRate The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly.
                                            case 6:
                                                captcha.SetSolverMethod(new SimpleNeuralNetSolver(charset, width, height, hidden, chars, learn));
                                                Out("Simple Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "MNN":
                                        switch (args.Count - 2)
                                        {
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.LITERAL MNN Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            case 3:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.LITERAL MNN Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            case 4:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, chars));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.LITERAL MNN Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG HiddenNeurons The number of neurons to put in the middle (hidden) layer of the neural network.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            case 5:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, hidden, chars));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.LITERAL MNN Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG HiddenNeurons The number of neurons to put in the middle (hidden) layer of the neural network.
                                            // #SKOTDOC.FUNCARG Characters The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
                                            // #SKOTDOC.FUNCARG LearnRate The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly.
                                            case 6:
                                                captcha.SetSolverMethod(new MultiNeuralNetSolver(charset, width, height, hidden, chars, learn));
                                                Out("Multi Neural Network Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "BVS":
                                        switch (args.Count - 2)
                                        {
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
                                            // #SKOTDOC.LITERAL BVS Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            case 3:
                                                captcha.SetSolverMethod(new BitmapSubtractionSolver(charset, width, height));
                                                Out("Bitmap Vector Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
                                            // #SKOTDOC.LITERAL BVS Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            // #SKOTDOC.FUNCARG MergePatterns Boolean value 'Y' or 'N' - Whether or not to group all patterns with the same solution. If you do not, then a separate pattern will be created for every input (not recommended usually) and it will take a lot of time and resources.
                                            case 4:
                                                captcha.SetSolverMethod(new BitmapSubtractionSolver(charset, width, height, merge));
                                                Out("Bitmap Vector Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "HS":
                                        switch (args.Count - 2)
                                        {
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use a histogram solver that compares the histograms of patterns to samples.
                                            // #SKOTDOC.LITERAL HS Set up the solver to use a histogram solver that compares the histograms of patterns to samples.
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            case 3:
                                                captcha.SetSolverMethod(new HistogramSolver(charset, width, height));
                                                Out("Histogram Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            default:
                                                Error("Wrong number of arguments in call to SETUPSOLVER");
                                                break;
                                        }
                                        break;

                                    case "CV":
                                        switch (args.Count - 2)
                                        {
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Set up the solver to use contour vector analysis. Contour analysis has the advantage on being invariant to scale, rotation, and translation which makes it ideal for some (but not all) situations.
                                            // #SKOTDOC.LITERAL CV Set up the solver to use contour vector analysis.
                                            // #SKOTDOC.FUNCARG CharacterSet A string containing all possible characters that could be used in the CAPTCHA system.
                                            // #SKOTDOC.FUNCARG Width The width that will be used for each input image.
                                            // #SKOTDOC.FUNCARG Height The height that will be used for each input image.
                                            case 3:
                                                captcha.SetSolverMethod(new ContourAnalysisSolver(charset, width, height));
                                                Out("Contour Analysis Solver Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

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
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART DEFINEPRECONDITIONS
                            // #SKOTDOC.BLOCKTYPE Setup
                            // #SKOTDOC.BLOCKDESC Start the preconditioning loop block ("loop" because it's run for each image being processed).
                            // #SKOTDOC.FUNCSTART
                            // #SKOTDOC.FUNCDESC Start the preconditioning loop block.
                            case "DEFINEPRECONDITIONS":
                                gatheringPreconditions = true;
                                break;
                            // #SKOTDOC.FUNCEND
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART SETUPSEGMENTER
                            // #SKOTDOC.BLOCKTYPE Setup
                            // #SKOTDOC.BLOCKDESC Set up the segmenter which is responsible for extracting individual letters from an image after preprocessing.
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
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.
                                            // #SKOTDOC.LITERAL BLOB Use the blob segmenter to extract individual symbols.
                                            // #SKOTDOC.FUNCARG MinWidth The minimum width a blob must be to be considered a blob worthy of extraction.
                                            // #SKOTDOC.FUNCARG MinHeight The minimum height a blob must be to be considered a blob worthy of extraction.
                                            case 2:
                                                captcha.SetSegmentationMethod(new BlobSegmentMethod(segwidth, segheight));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.
                                            // #SKOTDOC.LITERAL BLOB Use the blob segmenter to extract individual symbols.
                                            // #SKOTDOC.FUNCARG MinWidth The minimum width a blob must be to be considered a blob worthy of extraction.
                                            // #SKOTDOC.FUNCARG MinHeight The minimum height a blob must be to be considered a blob worthy of extraction.
                                            // #SKOTDOC.FUNCARG NumBlobs The fixed number of blobs to extract from the image. If fewer than this number are found, then the largest blobs will be split up until there are this many blobs. If there are too many, then the smallest will be ignored.
                                            case 3:
                                                captcha.SetSegmentationMethod(new BlobSegmentMethod(segwidth, segheight, segblobs));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            default:
                                                Error("Wrong number of arguments in call to SETUPSEGMENTER");
                                                break;
                                        }
                                        break;

                                    case "HIST":
                                        switch (args.Count - 2)
                                        {
                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Use histograms to determine where the best place in the image is to slice between letters.
                                            // #SKOTDOC.LITERAL HIST Use histograms to divide up the image.
                                            // #SKOTDOC.FUNCARG Tolerance Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point.
                                            case 1:
                                                captcha.SetSegmentationMethod(new HistogramSegmentMethod(tolerance: segwidth));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

                                            // #SKOTDOC.FUNCSTART
                                            // #SKOTDOC.FUNCDESC Use histograms to determine where the best place in the image is to slice between letters.
                                            // #SKOTDOC.LITERAL HIST Use histograms to divide up the image.
                                            // #SKOTDOC.FUNCARG Tolerance Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point.
                                            // #SKOTDOC.FUNCARG NumberOfChars The number of characters you expect to have extracted from the image. If there are more than this, then the least likely matches will be discarded. If there are fewer than this, then the largest ones will be subdivided.
                                            case 2:
                                                captcha.SetSegmentationMethod(new HistogramSegmentMethod(tolerance: segwidth, numChars: segheight));
                                                Out("Segmenter Setup Complete");
                                                break;
                                            // #SKOTDOC.FUNCEND

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
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART TESTSEGMENT
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder.
                            case "TESTSEGMENT":
                                string tsimage = args.GetQuotedArg(1);
                                string tsfolder = args.GetQuotedArg(2);

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder.
                                    // #SKOTDOC.FUNCARG ImageLocation The location of the image to test the segmentation on.
                                    // #SKOTDOC.FUNCARG OutputFolder The folder to output the segmented test symbols to.
                                    case 2:
                                        captcha.TestSegmentation(tsimage, tsfolder);
                                        Out("Test segmentation of " + args.GetArg(1) + " complete");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to TESTSEGMENT");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART TRAIN
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Train the solver on the patterns acquired or loaded.
                            case "TRAIN":
                                string tfolder = args.GetQuotedArg(1);
                                int titerations = args.GetArg(2).ToInt();

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Start training on a folder of patterns that have already been segmented and labeled for training.
                                    // #SKOTDOC.FUNCARG Folder The folder that contains the generated testing set of labeled patterns.
                                    case 1:
                                        PatternResult tresult1 = captcha.TrainOnSet(tfolder, 1);
                                        Out("Training Complete", true);
                                        Out("Error Approximation: " + tresult1.Error.ToString());
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Start training on a folder of patterns that have already been segmented and labeled for training.
                                    // #SKOTDOC.FUNCARG Folder The folder that contains the generated testing set of labeled patterns.
                                    // #SKOTDOC.FUNCARG Iterations Complete this many iterations of training on the given training set.
                                    case 2:
                                        PatternResult tresult2 = captcha.TrainOnSet(tfolder, titerations);
                                        Out("Training Complete", true);
                                        Out("Error Approximation: " + tresult2.Error.ToString());
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to TRAIN");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART TEST
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Test the solver's ability to produce correct predictions on the patterns acquired or loaded. (Use patterns that were not used in training or you will get skewed results.)
                            case "TEST":
                                string testfolder = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Test the solver's ability to produce correct predictions on the patterns acquired or loaded.
                                    // #SKOTDOC.FUNCARG Folder The folder that contains the set of labeled patterns to test on. (Use patterns that were not used in training or you will get skewed results.)
                                    case 1:
                                        PatternResult tresult3 = captcha.TestOnSet(testfolder);
                                        Out("Testing Complete. Percent Correct: " + tresult3.PercentageCorrect.ToString("0.00") + "%", true);
                                        Append("x-test.txt", tresult3.PercentageCorrect.ToString("0.00") + "%");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to TEST");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART FULLTEST
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
                            case "FULLTEST":
                                string fullfolder = args.GetQuotedArg(1);
                                string fullreport = args.GetQuotedArg(2);
                                string fullext = args.GetQuotedArg(3);

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
                                    // #SKOTDOC.FUNCARG Folder The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct.
                                    // #SKOTDOC.FUNCARG ReportFile The file to save the report to.
                                    case 2:
                                        PatternResult tresult4 = captcha.FullTestOnFolder(fullfolder, fullreport);
                                        Out("FULL Testing Complete. Percent Correct: " + tresult4.PercentageCorrect.ToString("0.00") + "%", true);
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
                                    // #SKOTDOC.FUNCARG Folder The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct.
                                    // #SKOTDOC.FUNCARG ReportFile The file to save the report to.
                                    // #SKOTDOC.FUNCARG ImageFilter The filter (e.g., *.bmp) to use to find images.
                                    case 3:
                                        PatternResult tresult5 = captcha.FullTestOnFolder(fullfolder, fullreport, fullext);
                                        Out("FULL Testing Complete. Percent Correct: " + tresult5.PercentageCorrect.ToString("0.00") + "%", true);
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to FULLTEST");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART SOLVE
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Solve a given image using the logic developed and trained for in the CBL script and output the solution.
                            case "SOLVE":
                                string imagelocation = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    case 1:
                                        // #SKOTDOC.FUNCSTART
                                        // #SKOTDOC.FUNCDESC Solve a CAPTCHA using the logic developed in the current CBL script.
                                        // #SKOTDOC.FUNCARG ImageLocation The image file to load and solve.
                                        // #SKOTDOC.FUNCEND

                                        // #SKOTDOC.FUNCSTART
                                        // #SKOTDOC.FUNCDESC Solve a CAPTCHA using the logic developed in the current CBL script.
                                        // #SKOTDOC.LITERAL %IMAGE% This placeholder will be replaced with the first command line value when run from the command line or, if being run from the CBL-GUI script runner app, will be replaced with the image that was dragged and dropped or loaded by the GUI.
                                        // #SKOTDOC.FUNCEND

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
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART SAVE
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Save the DataBase of trained patterns to a file so that it can be loaded later. The idea is to distribute the database file with your finished script (the finished script shouldn't do any training, only efficient solving).
                            case "SAVE":
                                string saveLoc = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Save the DataBase of trained patterns to the default "captcha.db" file.
                                    case 0:
                                        captcha.SaveToFile("captcha.db");
                                        Out("CAPTCHA Breaking Solution Saved");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Save the DataBase of trained patterns to a given file.
                                    // #SKOTDOC.FUNCARG Location The file name to save the pattern database to.
                                    case 1:
                                        captcha.SaveToFile(saveLoc);
                                        Out("CAPTCHA Breaking Solution Saved");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to SAVE");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART LOAD
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Load a pattern database. The database you load needs to have been saved under the same setup conditions as the script is being loaded under.
                            case "LOAD":
                                string dbLoc = args.GetQuotedArg(1);

                                switch (args.Count - 1)
                                {
                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Load a pattern database from the default "captcha.db" file.
                                    case 0:
                                        //List<string> recreate = captcha.LoadMetadataFromFile(dbLoc).Split(new string[] { SPLITTER }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                        captcha.LoadFromFile("captcha.db");
                                        Out("CAPTCHA Breaking Solution Loaded");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    // #SKOTDOC.FUNCSTART
                                    // #SKOTDOC.FUNCDESC Load a pattern database from a specified file.
                                    // #SKOTDOC.FUNCARG Location The name of the pattern database file to load.
                                    case 1:
                                        captcha.LoadFromFile(dbLoc);
                                        Out("CAPTCHA Breaking Solution Loaded");
                                        break;
                                    // #SKOTDOC.FUNCEND

                                    default:
                                        Error("Wrong number of arguments in call to LOAD");
                                        break;
                                }
                                break;
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART SAY
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Print out a line of debug text to the console.
                            case "SAY":
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Print out a line of debug text to the console.
                                // #SKOTDOC.FUNCARG Text The text to print.
                                string say = args.GetQuotedArg(1);
                                Out(say, true);
                                break;
                                // #SKOTDOC.FUNCEND
                            // #SKOTDOC.BLOCKEND

                            // #SKOTDOC.BLOCKSTART WAIT
                            // #SKOTDOC.BLOCKTYPE Working
                            // #SKOTDOC.BLOCKDESC Wait for the user to press a key.
                            case "WAIT":
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Wait for the user to press a key.
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
                                // #SKOTDOC.FUNCEND
                            // #SKOTDOC.BLOCKEND

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
                        // #SKOTDOC.BLOCKSTART RESIZE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Resize the image to a specified width and height.
                        case "RESIZE":
                            int rswidth = args.GetArg(1).ToInt();
                            int rsheight = args.GetArg(2).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Resize each image to a specified width and height.
                                case 2:
                                    // #SKOTDOC.FUNCARG Width The width to resize image to.
                                    // #SKOTDOC.FUNCARG Height The height to resize image to.
                                    s.Resize(rswidth, rsheight);
                                    Out("Image Resized to " + rswidth + "x" + rsheight);
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to RESIZE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART ERODE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Erodes the edges of blobs within an image.
                        case "ERODE":
                            int etimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.
                                case 0:
                                    s.ErodeShapes(Color.White);
                                    Out("Image Edges Eroded");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.
                                case 1:
                                    // #SKOTDOC.FUNCARG BackgroundColor The color of the background that surrounds the individual blobs in the image.
                                    for (int i = 0; i < etimes; i++)
                                    {
                                        s.ErodeShapes(Color.White);
                                    }
                                    Out("Image Edges Eroded");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to ERODE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART GROW
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Grow the size of all blobs in the image by one pixel.
                        case "GROW":
                            int gtimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.
                                case 0:
                                    s.GrowShapes(Color.White);
                                    Out("Image Edges Grown");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.
                                case 1:
                                    // #SKOTDOC.FUNCARG BackgroundColor The color of the background that surrounds the individual blobs in the image.
                                    for (int i = 0; i < gtimes; i++)
                                    {
                                        s.GrowShapes(Color.White);
                                    }
                                    Out("Image Edges Grown");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to GROW");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART OUTLINE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Performs a convolutional filter on the image that outlines edges.
                        case "OUTLINE":
                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Outline all edges in the image using a convolutional filter.
                                case 0:
                                    s.Outline();
                                    Out("Image Outline Filter Applied");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to OUTLINE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART SUBTRACT
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Perform a pixel-by-pixel subtraction of a given image from the working image and set each pixel value as the difference between the two.
                        case "SUBTRACT":
                            string img = args.GetQuotedArg(1);

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Subtract one image from another.
                                case 1:
                                    // #SKOTDOC.FUNCARG ImageLocation The absolute or relative location to the image to subtract from the working image.
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
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to SUBTRACT");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART MEDIAN
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Perform a convolutional median filter on the image.
                        case "MEDIAN":
                            int mtimes = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Perform a convolutional median filter on the image one time.
                                case 0:
                                    s.Median();
                                    Out("Median Filter Applied");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Perform a convolutional median filter on the image several times.
                                case 1:
                                    // #SKOTDOC.FUNCARG NumTimes The number of times to apply the Median filter to the image.
                                    for (int i = 0; i < mtimes; i++)
                                    {
                                        s.Median();
                                    }
                                    Out("Median Filter Applied");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to MEDIAN");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART INVERT
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Invert the colors in the image.
                        case "INVERT":
                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Invert the colors in the image.
                                case 0:
                                    s.Invert();
                                    Out("Image Inverted");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to INVERT");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART CROP
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Crop the image.
                        case "CROP":
                            int cx = args.GetArg(1).ToInt();
                            int cy = args.GetArg(2).ToInt();
                            int cw = args.GetArg(3).ToInt();
                            int ch = args.GetArg(4).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Crop the image to a given rectangle.
                                case 4:
                                    // #SKOTDOC.FUNCARG X The left side of the rectangle.
                                    // #SKOTDOC.FUNCARG Y The top of the rectangle.
                                    // #SKOTDOC.FUNCARG Width The width of the rectangle.
                                    // #SKOTDOC.FUNCARG Height The height of the rectangle.
                                    s.Crop(new Rectangle(cx, cy, cw, ch));
                                    Out("Image Cropped");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to CROP");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART BILATERALSMOOTH
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Performs a bilateral smoothing (edge preserving smoothing) and noise reduction filter on an image.
                        case "BILATERALSMOOTH":
                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Perfrom an edge preserving smoothing algorithm.
                                case 0:
                                    s.EdgePreservingSmooth();
                                    Out("Image Smoothed");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to BILATERALSMOOTH");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART COLORFILLBLOBS
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Fill each unique blob in an image with a random color.
                        // #SKOTDOC.BLOCKDESC A group of adjacent pixels is considered a single blob when they are all similar to each other in the L`*`a`*`b`*` color space below a given threshold.
                        // #SKOTDOC.BLOCKDESC In the L`*`a`*`b`*` color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye."
                        case "COLORFILLBLOBS":
                            double cfbtolerance = args.GetArg(1).ToDouble();
                            double cfbbkgtol = args.GetArg(2).ToDouble();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Fill all blobs within a 1.0 distance in the L`*`a`*`b`*` colorspace with a random color.
                                case 0:
                                    s.ColorFillBlobs(1.0, Color.White, 1.0);
                                    Out("Blobs within 1.00 tolerance (L*a*b* color space) filled");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Fill all blobs within a given distance in the L`*`a`*`b`*` colorspace with a random color.
                                case 2:
                                    // #SKOTDOC.FUNCARG ColorTolerance The maximum Delta E difference between two (L`*`a`*`b`*`) colors to allow when filling a blob. I.E., the colors have to be at most this close together to be considered to be in the same blob.
                                    // #SKOTDOC.FUNCARG BackgroundTolerance The maximum Delta E difference between a pixel (L`*`a`*`b`*`) and the background to allow when filling.
                                    s.ColorFillBlobs(cfbtolerance, Color.White, cfbbkgtol);
                                    Out("Blobs within " + cfbtolerance.ToString("0.00") + " tolerance (L*a*b* color space) filled");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to COLORFILLBLOBS");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART REMOVESMALLBLOBS
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Remove blobs (by filling them with the background color) from an image that are too small.
                        case "REMOVESMALLBLOBS":
                            int rsbcount = args.GetArg(1).ToInt();
                            int rsbwidth = args.GetArg(2).ToInt();
                            int rsbheight = args.GetArg(3).ToInt();
                            int rsbtolerance = args.GetArg(4).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Remove blobs from an image that are too small by either pixel count or X and Y dimensions.
                                case 3:
                                    // #SKOTDOC.FUNCARG MinPixelCount The smallest number of pixels a blob can be made of.
                                    // #SKOTDOC.FUNCARG MinWidth The smallest width a blob can be.
                                    // #SKOTDOC.FUNCARG MinHeight The smallest height a blob can be.
                                    s.RemoveSmallBlobs(rsbcount, rsbwidth, rsbheight, Color.White);
                                    Out("Small blobs removed from image");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Fill all blobs within a given distance in the L`*`a`*`b`*` colorspace with a random color.
                                case 4:
                                    // #SKOTDOC.FUNCARG MinPixelCount The smallest number of pixels a blob can be made of.
                                    // #SKOTDOC.FUNCARG MinWidth The smallest width a blob can be.
                                    // #SKOTDOC.FUNCARG MinHeight The smallest height a blob can be.
                                    // #SKOTDOC.FUNCARG ColorTolerance The RGB tolerance in color when flood filling
                                    s.RemoveSmallBlobs(rsbcount, rsbwidth, rsbheight, Color.White, rsbtolerance);
                                    Out("Small blobs removed from image");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to REMOVESMALLBLOBS");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART BLACKANDWHITE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Convert the image to black and white, where anything not white turns black (even the color #FEFEFE).
                        // #SKOTDOC.BLOCKDESC If you need to choose the threshold yourself, then see BINARIZE.
                        case "BLACKANDWHITE":
                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Flatten an image to black and white.
                                case 0:
                                    s.BlackAndWhite();
                                    Out("Image binarized to black and white");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to BLACKANDWHITE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART BINARIZE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Convert the image to black and white, where anything above a certain threshold is turned white.
                        case "BINARIZE":
                            int thresh = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Convert the image to black and white, where anything above a given threshold is turned white.
                                case 1:
                                    // #SKOTDOC.FUNCARG Threshold A threshold value between 0 and 255 that determines what colors turn black and which turn white.
                                    s.Binarize(thresh);
                                    Out("Image binarized to black and white");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to BINARIZE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART REMOVENONCOLOR
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC White out all pixels that are not a color (any shade of grey). (Useful when a CAPTCHA only colors the letters and not the background.)
                        case "REMOVENONCOLOR":
                            int dist = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Remove all grayscale colors from the image leaving only colors.
                                case 0:
                                    s.RemoveNonColor(3);
                                    Out("All grayscale colors removed from image");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Remove all colors withing a certain threshold of a shade of gray from the image leaving only colors.
                                case 1:
                                    // #SKOTDOC.FUNCARG Distance The threshold value which determines how close a color has to be to gray to be removed.
                                    s.RemoveNonColor(dist);
                                    Out("All grayscale colors removed from image");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to REMOVENONCOLOR");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART KEEPONLYMAINCOLOR
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Finds the color that occurrs most often in the image and removes all other colors that are not the most common color. 
                        // #SKOTDOC.BLOCKDESC This is great if the main CAPTCHA text is all one color and that text always represents the most common color in the image (in which case this function single-handedly segments the letters from the background).
                        case "KEEPONLYMAINCOLOR":
                            int th = args.GetArg(1).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Find the color that occurrs most often in the image within a certain threshold and remove all other colors that are not withing a given threshold from that color.
                                case 1:
                                    // #SKOTDOC.FUNCARG Threshold The threshold value which determines how close a color has to be to be kept.
                                    s.KeepOnlyMostCommonColor(th);
                                    Out("All but the most common color removed");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to KEEPONLYMAINCOLOR");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART SAVESAMPLE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Save a sample of the working image for debugging purposes. This is helpful when writing a script, as you can see every step along the way if you wish.
                        case "SAVESAMPLE":
                            string filename = args.GetQuotedArg(1);

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Save a sample of the working image.
                                case 1:
                                    // #SKOTDOC.FUNCARG FileLocation The name and location of where to save the image to.
                                    s.TrySave(filename);
                                    Out("Image saved as " + args.GetArg(1));
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to SAVESAMPLE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART MEANSHIFT
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Apply a mean shift filter to the image. This will effectively flatten out color groups within a certain tolerance.
                        case "MEANSHIFT":
                            int msiterations = args.GetArg(1).ToInt();
                            int msradius = args.GetArg(2).ToInt();
                            double mstolerance = args.GetArg(3).ToDouble();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Apply a 1 iteration mean shift filter with a radius of 1 and a tolerance of 1.
                                case 0:
                                    s.MeanShiftFilter(1, 1, 1);
                                    Out("Mean Shift Filter (" + msiterations + " iterations)");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Apply a mean shift filter a given number of times with a given radius and a given tolerance.
                                case 3:
                                    // #SKOTDOC.FUNCARG Iterations The number of times to repeat the filter on the image.
                                    // #SKOTDOC.FUNCARG Radius The radius of the filter.
                                    // #SKOTDOC.FUNCARG Tolerance The tolerance that determines how close in color pixels have to be if they are to be considered in the same group.
                                    s.MeanShiftFilter(msiterations, msradius, mstolerance);
                                    Out("Mean Shift Filter (" + msiterations + " iterations)");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to MEANSHIFT");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART FILLWHITE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Fill a color into a region of an image.
                        case "FILLWHITE":
                            int fwx = args.GetArg(1).ToInt();
                            int fwy = args.GetArg(2).ToInt();

                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Fill the background color into a region of an image.
                                case 2:
                                    // #SKOTDOC.FUNCARG X The X location of the region to start filling from.
                                    // #SKOTDOC.FUNCARG Y The Y location of the region to start filling from.
                                    s.FloodFill(new Point(fwx, fwy), 1, Color.White);
                                    Out("Point flood filled with white");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to MEANSHIFT");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART CONVOLUTE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Perform a convolutional filter on the image.
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
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Perform a convolutional filter on the image with a 3x3 kernel.
                                case 9:
                                    // #SKOTDOC.FUNCARG A1 The upper-left value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG A2 The upper-middle value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG A3 The upper-right value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG B1 The middle-left value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG B2 The center value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG B3 The middle-right value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG C1 The lower-left value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG C2 The lower-middle value of the 3x3 kernel.
                                    // #SKOTDOC.FUNCARG C3 The lower-right value of the 3x3 kernel.
                                    s.ConvolutionFilter(c_a, c_b, c_c, c_d, c_e, c_f, c_g, c_h, c_i);
                                    Out("Convolution filter applied to image.");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to CONVOLUTE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART HISTOGRAMROTATE
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Rotate an image using trial and error until a best angle is found (measured by a vertical histogram). 
                        // #SKOTDOC.BLOCKDESC Use this when an image has slanted letters and you want them to be right side up.
                        case "HISTOGRAMROTATE":
                            switch (args.Count - 1)
                            {
                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Rotate an image using trial and error until a best angle is found (measured by a vertical histogram). 
                                case 0:
                                    s.ResizeRotateCut();
                                    Out("Image rotated until best histogram was found");
                                    break;
                                // #SKOTDOC.FUNCEND

                                // #SKOTDOC.FUNCSTART
                                // #SKOTDOC.FUNCDESC Rotate an image using trial and error until a best angle is found (measured by a vertical histogram). 
                                case 1:
                                    // #SKOTDOC.LITERAL TRUE Overlay the resulting image with a completely useless (albeit cool to look at) histogram graph.
                                    s.ResizeRotateCut(true);
                                    Out("Image rotated until best histogram was found (HISTOGRAM DEBUG OVERLAY APPLIED)");
                                    break;
                                // #SKOTDOC.FUNCEND

                                default:
                                    Error("Wrong number of arguments in call to SAVESAMPLE");
                                    break;
                            }
                            break;
                        // #SKOTDOC.BLOCKEND

                        // #SKOTDOC.BLOCKSTART WAIT
                        // #SKOTDOC.BLOCKTYPE Preprocess
                        // #SKOTDOC.BLOCKDESC Wait for a key press from the user to continue.
                        case "WAIT":
                            // #SKOTDOC.FUNCSTART
                            // #SKOTDOC.FUNCDESC Wait for a key press from the user to continue.
                            // #SKOTDOC.FUNCEND
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
                        // #SKOTDOC.BLOCKEND

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
