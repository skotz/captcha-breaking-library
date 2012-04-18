using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ScottClayton.CAPTCHA.Utility
{
    /// <summary>
    /// A console progress bar that can be put into a using statement.
    /// One period will be used for every 2 percentile marks.
    /// </summary>
    public class ConsoleProgress : IDisposable
    {
        private BackgroundWorker worker;
        private int step;
        private int sleepTime;
        private string startMessage;
        private int percentage;

        private static bool allowOut = true;
        public static bool AllowOutput { get { return allowOut; } set { allowOut = value; } }

        public ConsoleProgress(string message)
            : this(message, 250)
        {
        }

        public ConsoleProgress(string message, int refreshMiliseconds)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.WorkerSupportsCancellation = true;

            sleepTime = refreshMiliseconds;
            startMessage = message;

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
                    Console.Write("\b\b\b\b\b\b\b");
                    if (percentage % 2 == 0)
                    {
                        Console.Write(".");
                    }
                    Console.Write(" " + percentage.ToString().PadLeft(3, ' ') + "% -");
                }
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
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
                            Console.Write("\b-");
                            step++;
                            break;
                        case 1:
                            Console.Write("\b\\");
                            step++;
                            break;
                        case 2:
                            Console.Write("\b|");
                            step++;
                            break;
                        case 3:
                            Console.Write("\b/");
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
                Console.Write(startMessage + " 000% -");
            }
            step = 0;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Sometimes I don't know why, but I like implementing IDisposable.
        /// Just thought I'd throw that out there.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            worker.CancelAsync();

            if (AllowOutput)
            {
                Console.Write("\b\b\b\b\b\b\b 100% ");
                Console.WriteLine();
            }
        }
    }
}
