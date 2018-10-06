using System.Collections.Generic;
using System.Linq;

namespace ScottClayton.Utility
{
    internal static class Extensions
    {
        /// <summary>
        /// Get a list of strings of each individual character of a string
        /// </summary>
        public static List<string> ToCharStringList(this string str)
        {
            return str.Select(c => c.ToString()).ToList();
        }
    }
}