#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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