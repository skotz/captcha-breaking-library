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
                //// COMPILE
                //else if (args[0].ToUpper() == "-C")
                //{
                //    if (File.Exists(args[1]))
                //    {
                //        string program = File.ReadAllText(args[1]);

                //        if (program.ToUpper().Contains("%IMAGE%"))
                //        {
                //            byte[] code = CompileScript(program);


                //        }
                //        else
                //        {
                //            Console.WriteLine("Compiled CAPTCHA Breaker scripts must contain a \"Solve, %IMAGE%\" command.");
                //            Console.ReadKey();
                //        }
                //    }
                //    else
                //    {
                //        Console.WriteLine("The file you specified does not exist!");
                //        Console.ReadKey();
                //    }
                //}
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
                Console.WriteLine("You must specify a captcha breaking script to run!");
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
