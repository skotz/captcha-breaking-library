using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Utility
{
    static class Extensions
    {
        /// <summary>
        /// Get a list of strings of each individual character of a string
        /// </summary>
        public static List<string> ToCharStringList(this string str)
        {
            return str.ToCharArray().Select(c => c.ToString()).ToList();
        }
    }
}
