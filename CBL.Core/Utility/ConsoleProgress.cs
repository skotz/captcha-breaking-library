using System;
using System.ComponentModel;

namespace ScottClayton.CAPTCHA.Utility
{
    /// <summary>
    /// A console progress bar that can be put into a using statement.
    /// One period will be used for every 4 percentile marks.
    /// </summary>
    public class ConsoleProgress : IDisposable
    {
        private BackgroundWorker worker;
        private int step;
        private int sleepTime;
        private string startMessage;
        private int percentage;
        private int writtenDots;

        private static bool allowOut = true;
        public static bool AllowOutput { get { return allowOut; } set { allowOut = value; } }

        private IProgressOutput WriteOutput;

        public ConsoleProgress(string message)
            : this(message, 250)
        {
        }

        public ConsoleProgress(string message, int refreshMiliseconds)
            : this(message, refreshMiliseconds, new ConsoleProgressOutput())
        {
        }

        public ConsoleProgress(string message, int refreshMiliseconds, IProgressOutput writeOutput)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.WorkerSupportsCancellation = true;

            sleepTime = refreshMiliseconds;
            startMessage = message;

            WriteOutput = writeOutput;
            writtenDots = 0;

            Start();
        }

        public void SetPercent(int percent)
        {
            // Could go up or down...
            if (percent != percentage)
            {
                percentage = percent;

                if (AllowOutput)
                {
                    WriteOutput.Backspace(7);

                    while (writtenDots < percentage / 4)
                    {
                        WriteOutput.Write(".");
                        writtenDots++;
                    }

                    WriteOutput.Write(" " + percentage.ToString().PadLeft(3, ' ') + "% -");
                }
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(sleepTime);

                if (worker.CancellationPending)
                {
                    return;
                }

                if (AllowOutput)
                {
                    switch (step)
                    {
                        case 0:
                            WriteOutput.Backspace(1);
                            WriteOutput.Write("-");
                            step++;
                            break;

                        case 1:
                            WriteOutput.Backspace(1);
                            WriteOutput.Write("\\");
                            step++;
                            break;

                        case 2:
                            WriteOutput.Backspace(1);
                            WriteOutput.Write("|");
                            step++;
                            break;

                        case 3:
                            WriteOutput.Backspace(1);
                            WriteOutput.Write("/");
                            step = 0;
                            break;
                    }
                }
            }
        }

        private void Start()
        {
            if (AllowOutput)
            {
                WriteOutput.Write(startMessage + "    0% -");
            }
            step = 0;
            worker.RunWorkerAsync();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            worker.CancelAsync();

            if (AllowOutput)
            {
                SetPercent(100);
                WriteOutput.Backspace(2);
            }
        }
    }
}