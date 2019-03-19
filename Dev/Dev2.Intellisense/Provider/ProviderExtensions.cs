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
using System.Linq;
using Dev2.Studio.Interfaces;

namespace Dev2.Intellisense.Provider
{
    public static class ProviderExtensions
    {
        public static string FindTextToSearch(this IntellisenseProviderContext context)
        {
            VerifyArgument.IsNotNull("context",context);
            var searchString = string.Empty;
            var foundMinimum = -1;
            var foundLength = 0;
            var inputText = context.InputText ?? string.Empty;
            var caretPosition = context.CaretPosition;

            var maxStringLength = Math.Min(caretPosition, inputText.Length);

            var closedBraceFound = false;
            var i = maxStringLength - 1;
            while (i >= 0)
            {
                var currentChar = inputText[i];
                if (currentChar == ')')
                {
                    closedBraceFound = true;
                }
                if(Char.IsWhiteSpace(currentChar))
                {
                    i = -1;
                }
                else
                {
                    ParseWarewolfLanguageSquareBraces(ref foundMinimum, ref foundLength, inputText, maxStringLength, closedBraceFound, ref i, currentChar);
                }
                i--;
            }

            if(foundMinimum != -1)
            {
                searchString = inputText.Substring(foundMinimum, foundLength);
            }

            var charArray = searchString.ToCharArray().ToList();

            if(!charArray.ToList().Any(c => Char.IsLetter(c) || c == '[' || c == '.' || c == ')'))
            {
                return string.Empty;
            }

            var indexOfOpenBrace = inputText.IndexOf('(');
            if(indexOfOpenBrace > 0 && inputText[indexOfOpenBrace - 1] == '[')
            {
                return string.Empty;
            }

            if(charArray.Count == 1)
            {
                if(!Char.IsLetterOrDigit(charArray[0]))
                {
                    return string.Empty;
                }

                if(charArray[0] == '(')
                {
                    return string.Empty;
                }
            }

            return searchString;
        }

        private static void ParseWarewolfLanguageSquareBraces(ref int foundMinimum, ref int foundLength, string inputText, int maxStringLength, bool closedBraceFound, ref int i, char currentChar)
        {
            if (currentChar == '[' && i > 0 && inputText[i - 1] == '[')
            {
                foundMinimum = i - 1;
                foundLength = maxStringLength - foundMinimum;
                i = -1;
            }
            else
            {
                if (currentChar == ']' || Char.IsSymbol(currentChar) || (currentChar == '(' && !closedBraceFound) || (currentChar == '(' && inputText.Length > i && i < inputText.Length && inputText[i + 1] == ')'))
                {
                    i = -1;
                }
                else
                {
                    ParseLastChar(ref foundMinimum, ref foundLength, ref i, maxStringLength, currentChar);
                }
            }
        }

        static void ParseLastChar(ref int foundMinimum, ref int foundLength, ref int i, int maxStringLength, char currentChar)
        {
            if (!Char.IsLetterOrDigit(currentChar))
            {
                if (currentChar == '(' ||
                    currentChar == ')' ||
                    currentChar == '[' ||
                    currentChar == ']' ||
                    currentChar == '_' ||
                    currentChar == '.')
                {
                    foundMinimum = i;
                    foundLength = maxStringLength - i;
                }
                else
                {
                    i = -1;
                }
            }
            else
            {
                foundMinimum = i;
                foundLength = maxStringLength - i;
            }
        }
    }
}
