
using Dev2.Studio.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Intellisense.Provider
{
    public static class ProviderExtensions
    {
        public static string FindTextToSearch(this IntellisenseProviderContext context)
        {
            string searchString = string.Empty;
            int foundMinimum = -1;
            int foundLength = 0;
            int maxStringLength = Math.Min(context.CaretPosition, context.InputText.Length);
            
            bool closedBraceFound = false;

            for(int i = maxStringLength - 1; i >= 0; i--)
            {
                char currentChar = context.InputText[i];

                if(currentChar == ')')
                {
                    closedBraceFound = true;
                }

                if(Char.IsWhiteSpace(currentChar))
                {
                    i = -1;
                }
                else
                {
                    if(currentChar == '[' && context.InputText[i - 1] == '[')
                    {
                        foundMinimum = i - 1;
                        foundLength = maxStringLength - foundMinimum;
                        i = -1;
                    }
                    else if(currentChar == ']')
                    {
                        i = -1;
                    }
                    else if(Char.IsSymbol(currentChar))
                    {
                        i = -1;
                    }
                    else if(currentChar == '(' && !closedBraceFound)
                    {
                        i = -1;
                    }
                    else if(currentChar == '(')
                    {
                        if(context.InputText.Length > i && context.InputText[i + 1] == ')')
                        {
                            i = -1;
                        }
                        else
                        {
                            foundMinimum = i;
                            foundLength = maxStringLength - i;
                        }
                    }
                    else
                    {
                        if(!Char.IsLetterOrDigit(currentChar))
                        {
                            if(currentChar == '(' ||
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

            if(foundMinimum != -1)
            {
                searchString = context.InputText.Substring(foundMinimum, foundLength);
            }

            var charArray = searchString.ToCharArray().ToList();

            if(!charArray.ToList().Any(c => Char.IsLetter(c) || c == '[' || c == '.' || c == ')'))
            {
                return string.Empty;
            }

            var indexOfOpenBrace = context.InputText.IndexOf('(');
            if(indexOfOpenBrace > 0 && context.InputText[indexOfOpenBrace - 1] == '[')
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

        public static IEnumerable<int> AllIndexesOf(this string inputString, string searchString)
        {
            if(string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(inputString))
            {
                yield return 0;
            }
            else
            {
                for(var index = 0; ; index += searchString.Length)
                {
                    index = inputString.IndexOf(searchString, index, StringComparison.Ordinal);
                    if(index == -1)
                        break;
                    yield return index;
                }
            }
        }

        public static Region RegionInPostion(this string inputText, int position)
        {
            var region = new Region();

            if(position > inputText.Length)
            {
                return new Region { Name = "" };
            }

            if(string.IsNullOrEmpty(inputText))
            {
                return region;
            }

            var rightSubstring = inputText.Substring(position, inputText.Length - position);
            char[] charArray = rightSubstring.ToCharArray();
            var openingBraces = 2;
            var closingBraces = 0;
            var index = position;
            var openBraceFound = false;

            foreach(var c in charArray)
            {
                if(c == '(')
                {
                    openBraceFound = true;
                }

                if(c == ')' && !openBraceFound)
                {
                    break;
                }

                if(Char.IsWhiteSpace(c))
                {
                    break;
                }

                index++;
                if(c == '[')
                {
                    openingBraces++;
                }

                if(c == ']')
                {
                    closingBraces++;
                }

                if(openingBraces == closingBraces)
                {
                    break;
                }
            }

            var endIndex = openingBraces - closingBraces == 0 ? index : position;

            var leftSubString = inputText.Substring(0, position);
            charArray = leftSubString.ToCharArray();
            openingBraces = 0;
            closingBraces = 2;
            index = position;

            foreach(var c in charArray.Reverse())
            {
                if(Char.IsWhiteSpace(c))
                {
                    break;
                }

                index--;
                if(c == '[')
                {
                    openingBraces++;
                }

                if(c == ']')
                {
                    closingBraces++;
                }

                if(openingBraces == closingBraces)
                {
                    break;
                }
            }

            var indexOfClosingBracket = leftSubString.LastIndexOf("]]", StringComparison.Ordinal) == -1 ? 0 : leftSubString.LastIndexOf("]]", StringComparison.Ordinal);
            var startIndex = openingBraces - closingBraces == 0 ? index : indexOfClosingBracket;

            region.StartIndex = startIndex;
            region.Name = inputText.Substring(startIndex, endIndex - startIndex);

            return region;
        }
    }

    public class Region
    {
        public int StartIndex { get; set; }
        public string Name { get; set; }
    }
}
