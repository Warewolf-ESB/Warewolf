using System;
using System.Collections.Generic;

namespace Tu.Extensions
{
    public static class StringUtils
    {
        public static List<string> ToLines(this string s)
        {

            return string.IsNullOrEmpty(s) ? new List<string>() : new List<string>(s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
        }

        public static List<string> ToFields(this string s, string separator)
        {
            return string.IsNullOrEmpty(s) ? new List<string>() : new List<string>(s.Split(new[] { separator }, StringSplitOptions.None));
        }

        public static string ToStringSafe(this object obj)
        {
            return obj == null || Convert.IsDBNull(obj) ? string.Empty : obj.ToString();
        }
    }
}
