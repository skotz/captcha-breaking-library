using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ScottClayton.Interpreter;
using System.IO.Compression;
using System.Drawing;

namespace CAPTCHA_Language_Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                // SCRIPT RUN
                if (args[0].ToUpper() == "-S")
                {
                    if (File.Exists(args[1]))
                    {
                        string program = File.ReadAllText(args[1]);
                        Directory.SetCurrentDirectory(new FileInfo(args[1]).DirectoryName);
                        CaptchaInterpreter run = new CaptchaInterpreter(program);
                        run.Execute();
                    }
                    else
                    {
                        Console.WriteLine("The file you specified does not exist!");
                        Console.ReadKey();
                    }
                }
            }
            else if (args.Length == 3)
            {
                // SCRIPT RUN - Training Mode
                if (args[0].ToUpper() == "-S" && args[2].ToUpper() == "-T")
                {
                    if (File.Exists(args[1]))
                    {
                        string program = File.ReadAllText(args[1]);
                        Directory.SetCurrentDirectory(new FileInfo(args[1]).DirectoryName);
                        CaptchaInterpreter run = new CaptchaInterpreter(program, true);
                        run.Execute();
                    }
                    else
                    {
                        Console.WriteLine("The file you specified does not exist!");
                        try
                        {
                            Console.Read();
                        }
                        catch
                        {
                            Console.ReadKey();
                        }
                    }
                }
            }
            else if (args.Length == 4)
            {
                // SCRIPT RUN - WITH IMAGE
                if (args[0].ToUpper() == "-S" && args[2].ToUpper() == "-I")
                {
                    if (File.Exists(args[1]))
                    {
                        if (File.Exists(args[3]))
                        {
                            string program = File.ReadAllText(args[1]);
                            Directory.SetCurrentDirectory(new FileInfo(args[1]).DirectoryName);
                            CaptchaInterpreter run = new CaptchaInterpreter(program, new Bitmap(args[3]));
                            run.Execute();
                        }
                        else
                        {
                            Console.WriteLine("The image you specified does not exist!");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Console.WriteLine("The file you specified does not exist!");
                        try
                        {
                            Console.Read();
                        }
                        catch
                        {
                            Console.ReadKey();
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("CAPTCHA Breaking Scripting Language Interpreter");
                Console.WriteLine("https" + "://github.com/skotz/captcha-breaking-library");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("    Execute a script:");
                Console.WriteLine("        cbli.exe -s <scriptFile.captcha>");
                Console.WriteLine("    Execute a script in training mode:");
                Console.WriteLine("        cbli.exe -s <scriptFile.captcha> -t");
                Console.WriteLine("    Execute a script and pass an image to solve:");
                Console.WriteLine("        cbli.exe -s <scriptFile.captcha> -i <imageToSolve.bmp>");
                Console.WriteLine();
                Console.ReadKey();
            }
        }

        public static byte[] CompileScript(string program)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gz))
                {
                    sw.Write(program);
                }
                return ms.ToArray();
            }
        }
    }
}
