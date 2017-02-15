/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// ReSharper disable InconsistentNaming

using System;
using System.Text;

namespace Dev2.Common.Utils
{
    public static class JSONUtils
    {
        /// <summary>
        ///     Scrubs the JSON.
        /// </summary>
        /// <param name="stringToScrub">The string to scrub.</param>
        /// <returns></returns>
        public static string ScrubJSON(string stringToScrub)
        {
            if (!String.IsNullOrEmpty(stringToScrub))
            {
                if (stringToScrub.StartsWith("\""))
                {
                    stringToScrub = stringToScrub.Remove(0, 1);
                }

                if (stringToScrub.EndsWith("\""))
                {
                    int indexTORemoveFrom = stringToScrub.Length - 1;
                    stringToScrub = stringToScrub.Remove(indexTORemoveFrom, 1);
                }
            }
            return stringToScrub;
        }

        public static string Format(string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;
            text = text.Replace(Environment.NewLine, String.Empty).Replace("\t", String.Empty);

            var offset = 0;
            var output = new StringBuilder();
            Action<StringBuilder, int> tabs = (sb, pos) => { for (var i = 0; i < pos; i++) { sb.Append("\t"); } };
            Func<string, int, char?> previousNotEmpty = (s, i) =>
            {
                if (string.IsNullOrEmpty(s) || i <= 0) return null;

                char? prev = null;

                while (i > 0 && prev == null)
                {
                    prev = s[i - 1];
                    if (prev.ToString() == " ") prev = null;
                    i--;
                }

                return prev;
            };
            Func<string, int, char?> nextNotEmpty = (s, i) =>
            {
                if (String.IsNullOrEmpty(s) || i >= (s.Length - 1)) return null;

                char? next = null;
                i++;

                while (i < (s.Length - 1) && next == null)
                {
                    next = s[i++];
                    if (next.ToString() == " ") next = null;
                }

                return next;
            };

            for (var i = 0; i < text.Length; i++)
            {
                var chr = text[i];

                if (chr.ToString() == "{")
                {
                    offset++;
                    output.Append(chr);
                    output.Append(Environment.NewLine);
                    tabs(output, offset);
                }
                else if (chr.ToString() == "}")
                {
                    offset--;
                    output.Append(Environment.NewLine);
                    tabs(output, offset);
                    output.Append(chr);
                }
                else if (chr.ToString() == ",")
                {
                    output.Append(chr);
                    output.Append(Environment.NewLine);
                    tabs(output, offset);
                }
                else if (chr.ToString() == "[")
                {
                    output.Append(chr);

                    var next = nextNotEmpty(text, i);

                    if (next != null && next.ToString() != "]")
                    {
                        offset++;
                        output.Append(Environment.NewLine);
                        tabs(output, offset);
                    }
                }
                else if (chr.ToString() == "]")
                {
                    var prev = previousNotEmpty(text, i);

                    if (prev != null && prev.ToString() != "[")
                    {
                        offset--;
                        output.Append(Environment.NewLine);
                        tabs(output, offset);
                    }

                    output.Append(chr);
                }
                else
                    output.Append(chr);
            }

            return output.ToString().Trim();
        }
    }
}