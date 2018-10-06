using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ScottClayton.Interpreter;

namespace CAPTCHA_Breaker_Compiled
{
    public partial class Form1 : Form
    {
        BackgroundWorker worker;

        public Bitmap StartImage { get; set; }

        public string Script { get; set; }

        public Form1(string[] args)
        {
            InitializeComponent();

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            CaptchaInterpreter.AllowGlobalDegugMessages();
            CaptchaInterpreter.OnGlobalBitmapMessage += new CaptchaInterpreter.BitmapMessageHandler(CaptchaInterpreter_OnGlobalBitmapMessage);

            if (args.Length > 0)
            {
                try
                {
                    Script = File.ReadAllText(args[0]);
                    Directory.SetCurrentDirectory(new FileInfo(args[0]).DirectoryName);

                    if (args[0].Contains("\\"))
                    {
                        textBox1.Text = args[0].Substring(args[0].LastIndexOf("\\") + 1);
                    }
                    else
                    {
                        textBox1.Text = "Script Loaded";
                    }
                }
                catch
                {
                    MessageBox.Show("Could not load script!", "CBL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void CaptchaInterpreter_OnGlobalBitmapMessage(Bitmap image)
        {
            try
            {
                pictureBox1.Image = CaptchaInterpreter.MergeVertical(StartImage, image);
            }
            catch (InvalidOperationException)
            {
                pictureBox1.Image = null;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                StartArgs go = (StartArgs)e.Argument;
                CaptchaInterpreter run = new CaptchaInterpreter(go.Program, go.Image);
                e.Result = run.Execute();
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox1.Text = (string)e.Result;

            try
            {
                if (copySolutionToClipboardToolStripMenuItem.Checked)
                {
                    Clipboard.SetText((string)e.Result);
                }
            }
            catch { }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    StartImage = new Bitmap(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
                    pictureBox1.Image = StartImage;
                    StartSolving();
                }
                else if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    StartImage = (Bitmap)e.Data.GetData(DataFormats.Bitmap);
                    pictureBox1.Image = StartImage;
                    StartSolving();
                }
            }
            catch
            {
                textBox1.Text = "Invalid Image";
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                Script = File.ReadAllText(openFileDialog1.FileName);
                Directory.SetCurrentDirectory(new FileInfo(openFileDialog1.FileName).DirectoryName);
            }
            catch
            {
                textBox1.Text = "Invalid Script";
            }
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                StartImage = new Bitmap(openFileDialog2.FileName);
                pictureBox1.Image = StartImage;

                StartSolving();
            }
            catch
            {
                textBox1.Text = "Unsupported Image Format";
            }
        }

        private void StartSolving()
        {
            if (!worker.IsBusy && Script.Length > 0)
            {
                textBox1.Text = "...solving...";
                worker.RunWorkerAsync(new StartArgs() { Program = Script, Image = (Bitmap)pictureBox1.Image.Clone() });
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created for CodeProject\nScott Clayton 2012\n\n" +
                "1) Load a CAPTCHA Breaking Scripting Language script\n" +
                "2) Load a CAPTCHA by dragging and dropping it on to the form\n" +
                "3) The CAPTCHA will be solved and the solution will be displayed", 
                "CAPTCHA Breaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void loadBreakerScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openCAPTCHAImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }
    }
}

public class StartArgs
{
    public Bitmap Image { get; set; }
    public string Program { get; set; }
}
