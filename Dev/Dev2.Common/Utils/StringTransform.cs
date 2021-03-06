﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        
        public static string TransformAllMatches(string initial, List<StringTransform> transforms)
        {
            var result = new StringBuilder(initial);
            foreach (StringTransform transform in transforms)
            {
                var regex = transform.SearchRegex;
                var groupNumbers = transform.GroupNumbers;
                var matches = regex.Matches(initial);
                if (matches.Count == 0)
                {
                    continue;
                }

                var encrypted = new StringBuilder();
                foreach (Match match in matches)
                {
                    result.Remove(match.Index, match.Length);
                    encrypted.Clear();
                    encrypted.Append(match.Value);
                    foreach (int groupNumber in groupNumbers)
                    {
                        var group = match.Groups[groupNumber];
                        var indexInMatch = group.Index - match.Index;
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