using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Common.Utils
{
    public class StringTransform
    {
        public Regex SearchRegex { private get; set; }
        public int[] GroupNumbers { private get; set; }
        public Func<string, string> TransformFunction { private get; set; }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        public static string TransformAllMatches(string initial, List<StringTransform> transforms)
        {
            StringBuilder result = new StringBuilder(initial);
            foreach (StringTransform transform in transforms)
            {
                Regex regex = transform.SearchRegex;
                int[] groupNumbers = transform.GroupNumbers;
                MatchCollection matches = regex.Matches(initial);
                if (matches.Count == 0) continue;
                StringBuilder encrypted = new StringBuilder();
                foreach (Match match in matches)
                {
                    result.Remove(match.Index, match.Length);
                    encrypted.Clear();
                    encrypted.Append(match.Value);
                    foreach (int groupNumber in groupNumbers)
                    {
                        Group group = match.Groups[groupNumber];
                        int indexInMatch = group.Index - match.Index;
                        encrypted.Remove(indexInMatch, group.Length);
                        encrypted.Insert(indexInMatch, transform.TransformFunction(group.Value));
                    }
                    result.Insert(match.Index, encrypted.ToString());
                }
            }
            return result.ToString();
        }
    }
}
