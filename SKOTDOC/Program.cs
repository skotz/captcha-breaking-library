using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SKOTDOC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SKOTDOC v1.0 - Scott Clayton 2012");
            Console.WriteLine("For generating documentation in Wiki format for the CAPTCHA Breaking Language");

            // Get the list of documented functions
            List<Section> sections = new List<Section>();
            if (args.Length == 2)
            {
                try
                {
                    string[] file = File.ReadAllLines(args[0]);
                    int lineNum = 0;
                    bool inBlock = false;
                    bool inFunction = false;
                    Function current = null;

                    foreach (string line in file)
                    {
                        lineNum++;
                        try
                        {
                            if (line.Contains("#SKOTDOC."))
                            {
                                string doc = line.Substring(line.IndexOf("#SKOTDOC.") + 9);
                                string cmd = "";

                                try
                                {
                                    cmd = doc.Substring(0, doc.IndexOf(" "));
                                }
                                catch
                                {
                                    cmd = doc;
                                }

                                string data = "";
                                try
                                {
                                    data = doc.Substring(doc.IndexOf(" ") + 1);
                                }
                                catch { }

                                Console.WriteLine("LINE " + lineNum + ": " + doc);

                                switch (cmd.ToUpper())
                                {
                                    case "DEFINESECTION":
                                        sections.Add(new Section(data));
                                        break;
                                    case "BLOCKSTART":
                                        inBlock = true;
                                        current = new Function() { Name = data };
                                        break;
                                    case "BLOCKDESC":
                                        if (inBlock)
                                        {
                                            current.Description += data + " ";
                                        }
                                        break;
                                    case "BLOCKTYPE":
                                        if (inBlock)
                                        {
                                            if (sections.Any(s => s.Name == data))
                                            {
                                                for (int i = 0; i < sections.Count; i++)
                                                {
                                                    if (sections[i].Name == data)
                                                    {
                                                        sections[i].Functions.Add(current);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sections.Add(new Section() { Name = data });
                                                sections[sections.Count - 1].Functions.Add(current);
                                            }
                                        }
                                        break;
                                    case "BLOCKEND":
                                        inBlock = false;
                                        break;
                                    case "FUNCSTART":
                                        inFunction = true;
                                        if (inBlock)
                                        {
                                            current.Overloads.Add(new Overload() { Description = data });
                                        }
                                        break;
                                    case "FUNCDESC":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Description = data;
                                        }
                                        break;
                                    case "FUNCARG":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data));
                                        }
                                        break;
                                    case "FUNCEND":
                                        inFunction = false;
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("SKOTDOC ERROR AT LINE " + lineNum + ": Cannot understand SKOTDOC!\n" + ex);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("UNFORTUNATE ERROR: " + ex);
                }

                using (StreamWriter w = new StreamWriter(args[1], false))
                {
                    foreach (Section s in sections)
                    {
                        w.WriteLine(String.Format("=_{0}_ Section=", s.Name));
                        w.WriteLine("{0}", s.Description);
                        w.WriteLine();

                        // Sort the functions
                        s.Functions.Sort((a, b) => a.Name.CompareTo(b.Name));

                        foreach (Function f in s.Functions)
                        {
                            w.WriteLine(String.Format("=={0}==", "{{{" + f.Name + "}}}"));
                            w.WriteLine("*Description:* {0}", f.Description);

                            foreach (Overload o in f.Overloads)
                            {
                                w.Write(String.Format(" # {{{{{{{0}", f.Name));
                                if (o.Arguments.Count > 0)
                                {
                                    foreach (Argument a in o.Arguments)
                                    {
                                        w.Write(String.Format(", {0}", a.Name));
                                    }
                                }
                                w.WriteLine("}}}");
                                if (!String.IsNullOrEmpty(o.Description))
                                {
                                    w.WriteLine(String.Format("  * {0}", o.Description));
                                }
                                foreach (Argument a in o.Arguments)
                                {
                                    w.WriteLine(String.Format("  * _{0}_ = {1}", a.Name, a.Description));
                                }
                            }
                        }
                    }

                    w.WriteLine();
                    w.WriteLine("----");
                    w.WriteLine("Generated by SKOTDOC on " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString());
                }
            }
            else
            {
                Console.WriteLine("Oops! Call this program like this: SKOTDOC fileToDocument.cs fileToSaveDoc.wiki");
            }
        }
    }

    class Section
    {
        public List<Function> Functions { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Section()
        {
            Functions = new List<Function>();
        }

        public Section(string data)
        {
            Functions = new List<Function>();
            Name = data.Split(' ')[0];
            Description = data.Substring(data.IndexOf(" ") + 1);
        }
    }

    class Function
    {
        public List<Overload> Overloads { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Function()
        {
            Overloads = new List<Overload>();
        }
    }

    class Overload
    {
        public List<Argument> Arguments { get; set; }
        public string Description { get; set; }

        public Overload()
        {
            Arguments = new List<Argument>();
        }
    }

    class Argument
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Argument(string data)
        {
            Name = data.Split(' ')[0];
            Description = data.Substring(data.IndexOf(" ") + 1);
        }
    }
}
