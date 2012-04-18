using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Interpreter
{
    static class ConversionExtensions
    {
        public static double ToDouble(this string value)
        {
            double d;
            double.TryParse(value, out d);
            return d;
        }

        public static int ToInt(this string value)
        {
            int i;
            int.TryParse(value, out i);
            return i;
        }

        public static string GetArg(this List<string> args, int index, string replacement = "")
        {
            if (args.Count > index)
            {
                return args[index];
            }
            else
            {
                return replacement;
            }
        }

        public static string GetQuotedArg(this List<string> args, int index, string replacement = "")
        {
            if (args.Count > index && args[index].Length >= 2)
            {
                return args[index].Substring(1, args[index].Length - 2);
            }
            else
            {
                return replacement;
            }
        }
    }
}
