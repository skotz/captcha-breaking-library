using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ScottClayton.CAPTCHA.Utility
{
    public class ConsoleSpinner
    {
        private BackgroundWorker worker;
        private int step;
        private int sleepTime;
        private string startMessage;

        private static bool allowOut = true;
        public static bool AllowOutput { get { return allowOut; } set { allowOut = value; } }

        public ConsoleSpinner(string message)
            : this(message, 250)
        {
        }

        public ConsoleSpinner(string message, int refreshMiliseconds)
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.WorkerSupportsCancellation = true;

            sleepTime = refreshMiliseconds;
            startMessage = message;
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

        public void Start()
        {
            if (AllowOutput)
            {
                Console.Write(startMessage + "-");
            }
            step = 0;
            worker.RunWorkerAsync();
        }

        public void Stop()
        {
            worker.CancelAsync();

            if (AllowOutput)
            {
                Console.Write("\b");

                for (int i = 0; i < startMessage.Length; i++)
                {
                    Console.Write("\b");
                }
            }
        }
    }

}
