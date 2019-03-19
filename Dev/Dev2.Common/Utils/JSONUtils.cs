#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using System;
using System.Text;

namespace Dev2.Common.Utils
{
    public static class JSONUtils
    {
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
                    var indexTORemoveFrom = stringToScrub.Length - 1;
                    stringToScrub = stringToScrub.Remove(indexTORemoveFrom, 1);
                }
            }
            return stringToScrub;
        }

        static void Tabs(int pos, ref StringBuilder sb)
        {
            for (var i = 0; i < pos; i++)
            {
                sb.Append("\t");
            }
        }

        public static string Format(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            var cleanText = text.Replace(Environment.NewLine, String.Empty).Replace("\t", String.Empty);

            var offset = 0;
            var output = new StringBuilder();
            Func<string, int, char?> previousNotEmpty = (s, i) =>
            {
                return JSONUtils.previousNotEmpty(s, ref i);
            };
            Func<string, int, char?> nextNotEmpty = (s, i) =>
            {
                return JSONUtils.nextNotEmpty(s, ref i);
            };

            for (var i = 0; i < cleanText.Length; i++)
            {
                var chr = cleanText[i];

                if (chr.ToString() == "{")
                {
                    offset++;
                    output.Append(chr);
                    output.Append(Environment.NewLine);
                    Tabs(offset, ref output);
                }
                else if (chr.ToString() == "}")
                {
                    offset--;
                    output.Append(Environment.NewLine);
                    Tabs(offset, ref output);
                    output.Append(chr);
                }
                else if (chr.ToString() == ",")
                {
                    output.Append(chr);
                    output.Append(Environment.NewLine);
                    Tabs(offset, ref output);
                }
                else if (chr.ToString() == "[")
                {
                    output.Append(chr);

                    var next = nextNotEmpty(cleanText, i);

                    if (next != null && next.ToString() != "]")
                    {
                        offset++;
                        output.Append(Environment.NewLine);
                        Tabs(offset, ref output);
                    }
                }
                else if (chr.ToString() == "]")
                {
                    var prev = previousNotEmpty(cleanText, i);

                    if (prev != null && prev.ToString() != "[")
                    {
                        offset--;
                        output.Append(Environment.NewLine);
                        Tabs(offset, ref output);
                    }

                    output.Append(chr);
                }
                else
                {
                    output.Append(chr);
                }
            }

            return output.ToString().Trim();
        }

        private static char? previousNotEmpty(string s, ref int i)
        {
            if (string.IsNullOrEmpty(s) || i <= 0)
            {
                return null;
            }

            char? prev = null;

            while (i > 0 && prev == null)
            {
                prev = s[i - 1];
                if (prev.ToString() == " ")
                {
                    prev = null;
                }

                i--;
            }

            return prev;
        }

        private static char? nextNotEmpty(string s, ref int i)
        {
            if (String.IsNullOrEmpty(s) || i >= (s.Length - 1))
            {
                return null;
            }

            char? next = null;
            i++;

            while (i < (s.Length - 1) && next == null)
            {
                next = s[i++];
                if (next.ToString() == " ")
                {
                    next = null;
                }
            }

            return next;
        }
    }
}