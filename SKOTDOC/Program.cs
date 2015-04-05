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

            string verbatim = "";

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
                                    case "VERBATIM":
                                        verbatim += data + "\n\n";
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
                                    case "LITERAL":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data, ArgType.LITERAL));
                                        }
                                        break;
                                    case "FUNCARG":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data));
                                        }
                                        break;
                                    case "FUNCARGSTR":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data, ArgType.STRING));
                                        }
                                        break;
                                    case "FUNCARGINT":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data, ArgType.INT));
                                        }
                                        break;
                                    case "FUNCARGDBL":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data, ArgType.DOUBLE));
                                        }
                                        break;
                                    case "FUNCARGBOOL":
                                        if (inFunction)
                                        {
                                            current.Overloads[current.Overloads.Count - 1].Arguments.Add(new Argument(data, ArgType.BOOL));
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
                    string WikiPage = args[1];
                    if (WikiPage.Contains("\\"))
                    {
                        WikiPage = WikiPage.Substring(args[1].LastIndexOf('\\')).Substring(0, args[1].LastIndexOf('.'));
                    }
                    else
                    {
                        WikiPage = WikiPage.Substring(0, args[1].LastIndexOf('.'));
                    }

                    // Google Code wiki labels (removed 4/5/15 - migrated to github)
                    // w.WriteLine("#labels Featured,Phase-Implementation");

                    if (verbatim.Length > 0)
                    {
                        w.WriteLine();
                        w.WriteLine(verbatim);
                        w.WriteLine();
                    }

                    //w.WriteLine("<wiki:toc max_depth=\"2\" />");

                    foreach (Section s in sections)
                    {
                        w.WriteLine();
                        w.WriteLine("##" + s.Name);
                        w.WriteLine(s.Description);
                        w.WriteLine();
                        w.WriteLine("Method | Description");
                        w.WriteLine("------ | ------");
                        foreach (Function f in s.Functions)
                        {
                            w.WriteLine("[" + f.Name + "](#" + f.Name + ") | " + f.Description);
                        }
                        w.WriteLine();
                    }

                    foreach (Section s in sections)
                    {
                        w.WriteLine();
                        w.WriteLine("----");
                        w.WriteLine(String.Format("# *{0}* Section", s.Name));
                        w.WriteLine("{0}", s.Description);
                        w.WriteLine();

                        // Sort the functions
                        s.Functions.Sort((a, b) => a.Name.CompareTo(b.Name));

                        foreach (Function f in s.Functions)
                        {
                            w.WriteLine("----");
                            w.WriteLine("## {0}", f.Name);
                            w.WriteLine("{0}", f.Description);

                            int overload = 1;
                            foreach (Overload o in f.Overloads)
                            {
                                w.WriteLine();
                                w.WriteLine("### Overload " + overload++);
                                w.Write("    " + f.Name);
                                if (o.Arguments.Count > 0)
                                {
                                    foreach (Argument a in o.Arguments)
                                    {
                                        w.Write(String.Format(", {0}", a.Name));
                                    }
                                }
                                w.WriteLine();
                                bool both = !String.IsNullOrEmpty(o.Description) && o.Arguments.Count > 0;
                                if (!String.IsNullOrEmpty(o.Description))
                                {
                                    w.WriteLine();
                                    w.WriteLine("#### Description");
                                    w.WriteLine(o.Description);
                                    if (!both)
                                    {
                                        w.WriteLine();
                                    }
                                }
                                if (o.Arguments.Count > 0)
                                {
                                    w.WriteLine();
                                    if (!both)
                                    {
                                        w.WriteLine();
                                    }
                                    w.WriteLine("#### Parameters");
                                    w.WriteLine("Name | Type | Description");
                                    w.WriteLine("----- | ----- | -----");
                                    foreach (Argument a in o.Arguments)
                                    {
                                        if (a.Type == ArgType.LITERAL)
                                        {
                                            w.WriteLine(String.Format("*{0}* | *Literal Value* | {1}", a.Name, a.Description));
                                        }
                                        else if (a.Type == ArgType.DOUBLE)
                                        {
                                            w.WriteLine(String.Format("{0} | Decimal Value | {1}", a.Name, a.Description));
                                        }
                                        else if (a.Type == ArgType.INT)
                                        {
                                            w.WriteLine(String.Format("{0} | Whole Number | {1}", a.Name, a.Description));
                                        }
                                        else if (a.Type == ArgType.STRING)
                                        {
                                            w.WriteLine(String.Format("{0} | Quoted String | {1}", a.Name, a.Description));
                                        }
                                        else if (a.Type == ArgType.BOOL)
                                        {
                                            w.WriteLine(String.Format("{0} | Boolean (Y/N) | {1}", a.Name, a.Description));
                                        }
                                        else
                                        {
                                            w.WriteLine(String.Format("{0} | Parameter |  {1}", a.Name, a.Description));
                                        }
                                    }
                                    w.WriteLine();
                                }
                                w.WriteLine();
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
        public ArgType Type { get; set; }

        public Argument(string data, ArgType type = ArgType.NONE)
        {
            Name = data.Split(' ')[0];
            Description = data.Substring(data.IndexOf(" ") + 1);
            Type = type;
        }
    }

    public enum ArgType
    {
        BOOL,
        DOUBLE,
        INT,
        STRING,
        LITERAL,
        NONE
    }
}
