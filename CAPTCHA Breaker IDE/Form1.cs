using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScottClayton.CAPTCHA;
using ScottClayton.Interpreter;
using System.IO;

namespace CAPTCHA_Breaker_IDE
{
    public partial class Form1 : Form
    {
        Bitmap image;
        BackgroundWorker worker;

        private string loadedFile = "";

        public Form1()
        {
            InitializeComponent();

            image = null;

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            CaptchaInterpreter.AllowGlobalDegugMessages();
            CaptchaInterpreter.OnGlobalBitmapMessage += new CaptchaInterpreter.BitmapMessageHandler(CaptchaInterpreter_OnGlobalBitmapMessage);
            CaptchaInterpreter.OnError += new CaptchaInterpreter.ErrorNotificationHandler(CaptchaInterpreter_OnError);
            CaptchaInterpreter.OnInfo += new CaptchaInterpreter.InformationMessageHandler(CaptchaInterpreter_OnInfo);
        }

        void CaptchaInterpreter_OnInfo(string message)
        {
            Out("SCRIPT -- " + message);
        }

        void CaptchaInterpreter_OnError(string message, Exception ex)
        {
            Error(message);
        }

        void CaptchaInterpreter_OnGlobalBitmapMessage(Bitmap image)
        {
            try
            {
                pictureSegments.Image = image;
            }
            catch (InvalidOperationException)
            {
                pictureSegments.Image = null;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartArgs go = (StartArgs)e.Argument;
            CaptchaInterpreter run = new CaptchaInterpreter(go.Program, go.Image);
            e.Result = run.Execute();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStatus.Text = "Done!";
            textBox1.Text = (string)e.Result;
            Out("Done!");
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult d = openFileDialog1.ShowDialog();
            if (d == DialogResult.OK)
            {
                image = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
                pictureCAPTCHA.Image = image;
            }
        }

        private void TestProgram()
        {
            if (image != null)
            {
                try
                {
                    lblStatus.Text = "Working...";
                    worker.RunWorkerAsync(new StartArgs() { Program = richTextBox1.Text, Image = (Bitmap)image.Clone() });
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    lblStatus.Text = ex.Message;
                    //MessageBox.Show(ex.Message, "CAPTCHA Breaker IDE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Error("No sample image has been loaded yet!");
            }
        }

        private void executeScriptToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            Out("Executing Script...");
            TestProgram();
        }

        private void Error(string message)
        {
            Out("ERROR: " + message);
        }

        private void Out(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Out), new object[] { message });
            }
            else
            {
                richTextBox2.Text = DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0') +
                    ":" + DateTime.Now.Second.ToString().PadLeft(2, '0') + " -- " + message + "\r\n" + richTextBox2.Text;
            }
        }

        private void loadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                richTextBox1.Text = File.ReadAllText(openFileDialog2.FileName);
                loadedFile = openFileDialog2.FileName;
            }
            catch (Exception ex)
            {
                Error("Could not open CAPTCHA Breaker Script!");
                lblStatus.Text = ex.Message;
            }
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedFile.Length > 0)
            {
                try
                {
                    File.WriteAllLines(loadedFile, richTextBox1.Lines);
                    Out("Script saved!");
                }
                catch (Exception ex)
                {
                    Error("Could not save CAPTCHA Breaker Script!");
                    lblStatus.Text = ex.Message;
                }
            }
            else
            {
                saveFileDialog1.ShowDialog();
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                File.WriteAllLines(saveFileDialog1.FileName, richTextBox1.Lines);
                loadedFile = saveFileDialog1.FileName;
                Out("Script saved!");
            }
            catch (Exception ex)
            {
                Error("Could not save CAPTCHA Breaker Script!");
                lblStatus.Text = ex.Message;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created for CodeProject\nScott Clayton 2012", "CAPTCHA Breaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

public class StartArgs
{
    public Bitmap Image { get; set; }
    public string Program { get; set; }
}