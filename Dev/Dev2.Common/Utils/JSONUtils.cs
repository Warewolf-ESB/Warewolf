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
using System.Linq;

namespace Dev2.Common.Utils
{
    public static class JsonUtils
    {
        public static string ScrubJson(string initialStringToScrub)
        {
            var stringToScrub = initialStringToScrub;
            if (!string.IsNullOrEmpty(stringToScrub))
            {
                if (stringToScrub.StartsWith("\"", StringComparison.Ordinal))
                {
                    stringToScrub = stringToScrub.Remove(0, 1);
                }

                if (stringToScrub.EndsWith("\"", StringComparison.Ordinal))
                {
                    var indexTORemoveFrom = stringToScrub.Length - 1;
                    stringToScrub = stringToScrub.Remove(indexTORemoveFrom, 1);
                }
            }
            return stringToScrub;
        }

        private const string Tab_String = "\t";

        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var cleanText = text.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);

            int indentation = 0;
            int quoteCount = 0;
            var result = from character in cleanText
                         let quotes = character == '"' ? quoteCount++ : quoteCount
                         let lineBreak = GetLineBreak(indentation, character, quotes)
                         let openChar = GetOpenChar(ref indentation, character)
                         let closeChar = GetCloseChar(ref indentation, character)
                         select lineBreak ?? (openChar.Length > 1 ? openChar : closeChar);

            return string.Concat(result).Trim();
        }

        private static string GetLineBreak(int indentation, char character, int quotes)
        {
            if (character == ',' && quotes % 2 == 0)
            {
                return character + Environment.NewLine + string.Concat(Enumerable.Repeat(Tab_String, indentation));
            }
            return null;
        }

        private static string GetOpenChar(ref int indentation, char character)
        {
            if (character == '{' || character == '[')
            {
                return character + Environment.NewLine + string.Concat(Enumerable.Repeat(Tab_String, ++indentation));
            }
            return character.ToString();
        }

        private static string GetCloseChar(ref int indentation, char character)
        {
            if (character == '}' || character == ']')
            {
                return Environment.NewLine + string.Concat(Enumerable.Repeat(Tab_String, --indentation)) + character;
            }
            return character.ToString();
        }
    }
}