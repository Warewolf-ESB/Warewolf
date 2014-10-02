
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace System
{
    internal static class StringUtility
    {
        #region Constants
        public const char Space = ' ';
        public const char NewLine = '\n';
        public const string StrNewLine = "\n";
        public const int IntNewLine = NewLine;
        private const int CharStorageCapacity = 1024;
        #endregion

        #region Readonly Fields
        private static readonly char[] CharStorage = new char[CharStorageCapacity];

        public static readonly char[] Digits = BuildCharacterRange(48, 10);
        public static readonly char[] Hexadecimal = BuildCharacterRange(BuildCharacterRange(BuildCharacterRange(new char[22], 0, 48, 10), 10, 65, 6), 16, 97, 6);
        public static readonly char[] LowercaseLetters = BuildCharacterRange(97, 26);
        public static readonly char[] UppercaseLetters = BuildCharacterRange(65, 26);
        public static readonly char[] RegularCharEscapeCharacters = new char[] { '\'', '\\', '0', 'a', 'b', 'f', 'n', 'r', 't', 'v', 'x', 'u', 'U' };
        public static readonly char[] RegularCharUnescapeCharacters = new char[] { '\'', '\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v' };
        public static readonly char[] RegularStringEscapeCharacters = new char[] { '"', '\\', '0', 'a', 'b', 'f', 'n', 'r', 't', 'v', 'x', 'u', 'U' };
        public static readonly char[] RegularStringUnescapeCharacters = new char[] { '"', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v' };
        public static readonly char[] WhitespaceChars = new char[] { '\t', '\n', '\v', '\f', '\r', ' ', '\x0085', '\x00a0', '?', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '?', '?', '?', '?', '?', '\u2028', '\u2029', ' ', '?' };
        public static readonly char[] ControlChars = new char[] { '\0', '\a', '\b', '\f', '\r', '\v' };
        public static readonly char[] CaseInsensitiveRange = BuildCharacterRange(BuildCharacterRange(new char[256], 0, 0, 256), 65, 97, 26);
        public static readonly BooleanArray WhitespaceMask = new BooleanArray(WhitespaceChars);
        public static readonly BooleanArray ControlMask = new BooleanArray(ControlChars);
        public static readonly BooleanArray WhitespaceOrControlMask = BooleanArray.Or(WhitespaceMask, ControlMask);
        public static readonly BooleanArray DigitMask = new BooleanArray(Digits, 256);
        public static readonly BooleanArray HexadecimalMask = new BooleanArray(Hexadecimal, 256);
        public static readonly BooleanArray LowercaseMask = new BooleanArray(LowercaseLetters, 256);
        public static readonly BooleanArray UppercaseMask = new BooleanArray(UppercaseLetters, 256);
        public static readonly BooleanArray LetterMask = BooleanArray.Or(LowercaseMask, UppercaseMask);
        public static readonly BooleanArray RegularCharEscapeMask = new BooleanArray(RegularCharEscapeCharacters, 256);
        public static readonly BooleanArray RegularStringEscapeMask = new BooleanArray(RegularStringEscapeCharacters, 256);
        #endregion

        #region Build Handling
        public static char[] BuildCharacterRange(char[] result, int index, int start, int length)
        {
            for (int i = 0; i < length; i++) result[i + index] = (char)(start + i);
            return result;
        }

        public static char[] BuildCharacterRange(int start, int length)
        {
            char[] result = new char[length];
            for (int i = 0; i < result.Length; i++) result[i] = (char)(start + i);
            return result;
        }

        public static string BuildIdentifierPrefix(int index, bool excludeFinalLetter)
        {
            string result = null;
            
            {
                int i;
                int max = 0;
                int[] indices = new int[100];
                int wheel = 0;

                for (i = 25; i < index; i += 26)
                {
                    indices[wheel]++;

                    while (indices[wheel] == 27)
                    {
                        indices[wheel++] = 1;
                        indices[wheel]++;
                    }

                    if (wheel > max) max = wheel;
                    wheel = 0;
                }

                if (index >= 26)
                    max++;
                index = index - ((index / 26) * 26);

                result = excludeFinalLetter ? "" : ((char)(index + 65)).ToString();
                for (i = 0; i < max; i++) result += ((char)(indices[i] + 64)).ToString();


                char[] temp = result.ToCharArray();
                Array.Reverse(temp);
                result = new string(temp);
            }

            return result;
        }
        #endregion

        #region Extensions
        public static bool AreDigits(this string self, int startIndex)
        {
            int length = self.Length;

            for (int i = startIndex; i < length; i++)
                if (!Char.IsDigit(self, i))
                    return false;

            return true;
        }

        public static bool AreDigits(this string self, int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
                if (!Char.IsDigit(self, i + startIndex))
                    return false;

            return true;
        }
        #endregion

        #region Equality Handling
        public static unsafe bool Equals(string left, int leftIndex, string right, int rightIndex, int count)
        {
            if (leftIndex == rightIndex)
                if (String.ReferenceEquals(left, right))
                    return true;

            fixed (char* leftPointer = left, rightPointer = right)
            {
                char* leftSource = leftPointer + leftIndex, rightSource = rightPointer + rightIndex;

                for (int i = 0; i < count; i++)
                    if (leftSource[i] != rightSource[i])
                        return false;
            }

            return true;
        }
        #endregion

        #region Capitalization Handling
        public static string CapitalizeByWord(string value)
        {
            char[] letters = value.ToCharArray();
            bool lastWS = true;

            for (int i = 0; i < letters.Length; i++)
            {
                bool ws = Char.IsWhiteSpace(letters[i]);

                if (ws) lastWS = true;
                else if (lastWS)
                {
                    lastWS = false;
                    letters[i] = Char.ToUpper(letters[i]);
                }
            }

            return new string(letters);
        }
        #endregion

        #region Apply(...)
        public static string ApplyMask(string input, BooleanArray mask)
        {
            bool changed;
            return ApplyMask(input, mask, false, out changed);
        }

        public static string ApplyMask(string input, BooleanArray mask, bool comparand)
        {
            bool changed;
            return ApplyMask(input, mask, comparand, out changed);
        }

        public static unsafe string ApplyMask(string input, BooleanArray mask, bool comparand, out bool changed)
        {
            changed = false;
            if (input == null) return input;
            int length = input.Length;
            if (length == 0) return input;
            string output = String.Empty;
            int count = mask.Length;
            int index = 0;
            char[] storage = CharStorage;

            fixed (char* pointer = input)
            {
                char current = Char.MinValue;

                for (int i = 0; i < length; i++)
                {
                    current = pointer[i];

                    if ((current >= count && !comparand) || (mask[current] == comparand))
                    {
                        storage[index++] = current;

                        if (index == CharStorageCapacity)
                        {
                            output += new string(storage);
                            index = 0;
                        }
                    }
                    else changed = true;
                }
            }

            if (index != 0) output += new string(storage, 0, index);
            return output;
        }
        #endregion

        #region Get(...)
        public static string GetUnescapedString(string escapedValue)
        {
            bool verbatim, regular;
            escapedValue = PrepareEscapedString(escapedValue, out verbatim, out regular);
            if (String.IsNullOrEmpty(escapedValue)) return escapedValue;

            if (verbatim) return GetUnescapedString(escapedValue, true);
            if (regular) return GetUnescapedString(escapedValue, false);

            string possible = GetUnescapedString(escapedValue, false);
            if (possible == null) possible = GetUnescapedString(escapedValue, true);
            if (possible == null) throw new ArgumentException("Cannot determine the nature of the escaped string.");
            return possible;
        }

        public static string GetUnescapedVerbatimString(string escapedValue)
        {
            bool verbatim, regular;
            escapedValue = PrepareEscapedString(escapedValue, out verbatim, out regular);
            if (String.IsNullOrEmpty(escapedValue)) return escapedValue;
            return GetUnescapedString(escapedValue, true);
        }

        public static string GetUnescapedRegularString(string escapedValue)
        {
            bool verbatim, regular;
            escapedValue = PrepareEscapedString(escapedValue, out verbatim, out regular);
            if (String.IsNullOrEmpty(escapedValue)) return escapedValue;
            return GetUnescapedString(escapedValue, false);
        }

        private static string PrepareEscapedString(string escapedValue, out bool verbatim, out bool regular)
        {
            verbatim = regular = false;
            if (String.IsNullOrEmpty(escapedValue)) return escapedValue;
            int length = escapedValue.Length;

            bool startsWithDQ = false, endsWithDQ = false, startsWithATDQ = false;

            if (length > 1)
            {
                startsWithDQ = escapedValue[0] == '"';
                startsWithATDQ = !startsWithDQ && (escapedValue[0] == '@' && escapedValue[1] == '"');
                endsWithDQ = (length > (startsWithATDQ ? 2 : 1)) && escapedValue[length - 1] == '"';
            }

            if (verbatim = startsWithATDQ && endsWithDQ) escapedValue = length == 3 ? "" : escapedValue.Substring(2, length - 3);
            else if (regular = startsWithDQ && endsWithDQ) escapedValue = length == 2 ? "" : escapedValue.Substring(1, length - 2);

            return escapedValue;
        }

        private static string GetUnescapedString(string escapedValue, bool verbatim)
        {
            int index = 0;

            if (verbatim)
            {
                int length = escapedValue.Length;
                int previous = 0;
                string result = "";

                while ((index = escapedValue.IndexOf("\"", index)) != -1)
                {
                    if (index + 1 >= length) return null;
                    if (escapedValue[index + 1] != '"') return null;
                    if (index - previous > 0) result += escapedValue.Substring(previous, index - previous);
                    result += "\"";
                    index = previous = index + 2;
                    if (index + 1 >= length) break;
                }

                if (previous < length) result += escapedValue.Substring(previous, length - previous);
                escapedValue = result;
            }
            else if (escapedValue.IndexOf('\\') != -1)
            {
                List<string> splits = new List<string>();

                while ((index = escapedValue.IndexOf("\\\\")) != -1)
                {
                    if (index > 0) splits.Add(escapedValue.Substring(0, index));
                    else splits.Add(String.Empty);
                    splits.Add(null);

                    if (index + 2 == escapedValue.Length)
                    {
                        escapedValue = String.Empty;
                        break;
                    }
                    else escapedValue = escapedValue.Substring(index + 2);
                }

                splits.Add(escapedValue);
                escapedValue = "";

                for (int s = 0; s < splits.Count; s++)
                {
                    string part = splits[s];

                    if (part == null) escapedValue += "\\";
                    else if (part.Length > 0)
                    {
                        for (int i = part.IndexOf('\\'); i != -1; i = part.IndexOf('\\', i))
                        {
                            if (i + 1 == part.Length) return null;
                            int indicatorLength = 2;
                            char indicator = part[i + 1];
                            char replacement;

                            switch (indicator)
                            {
                                case '"': replacement = '"'; break;
                                case '0': replacement = '\0'; break;
                                case 'a': replacement = '\a'; break;
                                case 'b': replacement = '\b'; break;
                                case 'f': replacement = '\f'; break;
                                case 'n': replacement = '\n'; break;
                                case 'r': replacement = '\r'; break;
                                case 't': replacement = '\t'; break;
                                case 'v': replacement = '\v'; break;
                                case 'x': replacement = GetUnescapedVariableUnicode(part, i, out indicatorLength); break;
                                case 'u': replacement = GetUnescapedUnicode(part, i, out indicatorLength); break;
                                case 'U': replacement = GetUnescapedUnicode(part, i, out indicatorLength); break;
                                default: return null;
                            }

                            if (indicatorLength <= 0) return null;

                            part = (i > 0 ? part.Remove(i) : String.Empty) + replacement + (i + indicatorLength < part.Length ? part.Substring(i + indicatorLength) : String.Empty);
                        }

                        escapedValue += part;
                    }
                }

            }
            else
            {
                if ((index = escapedValue.IndexOfAny(RegularStringUnescapeCharacters)) != -1) escapedValue = null;
            }

            return escapedValue;
        }

        public static bool GetUnescapedChar(string escapedValue, out char result)
        {
            result = Char.MinValue;
            if (String.IsNullOrEmpty(escapedValue)) return false;
            int length = escapedValue.Length;

            if (escapedValue[0] == '\'')
            {
                if (length < 3) return false;
                if (escapedValue[length - 1] != '\'') return false;
                escapedValue = escapedValue.Substring(1, length - 2);
                length -= 2;
            }

            if (escapedValue[0] == '\\')
            {
                if (length < 2) return false;
                int indicatorLength = 2;
                char indicator = escapedValue[1];
                char replacement;

                switch (indicator)
                {
                    case '\\': replacement = '\\'; break;
                    case '\'': replacement = '\''; break;
                    case '0': replacement = '\0'; break;
                    case 'a': replacement = '\a'; break;
                    case 'b': replacement = '\b'; break;
                    case 'f': replacement = '\f'; break;
                    case 'n': replacement = '\n'; break;
                    case 'r': replacement = '\r'; break;
                    case 't': replacement = '\t'; break;
                    case 'v': replacement = '\v'; break;
                    case 'x': replacement = GetUnescapedVariableUnicode(escapedValue, 0, out indicatorLength); break;
                    case 'u': replacement = GetUnescapedUnicode(escapedValue, 0, out indicatorLength); break;
                    case 'U': replacement = GetUnescapedUnicode(escapedValue, 0, out indicatorLength); break;
                    default: return false;
                }

                if (indicatorLength != length) return false;
                result = replacement;
            }
            else
            {
                if (length != 1) return false;
                if (escapedValue.IndexOfAny(RegularCharUnescapeCharacters) != -1) return false;
                result = escapedValue[0];
            }

            return true;
        }

        public static bool GetUnescapedUnicode(string escapedValue, out char result)
        {
            if (String.IsNullOrEmpty(escapedValue))
            {
                result = Char.MinValue;
                return false;
            }

            result = Char.MinValue;
            int length = escapedValue.Length;
            int consumed = -1;

            if (length > 2)
            {
                if (escapedValue[0] == '\\')
                {
                    switch (escapedValue[1])
                    {
                        case 'x': result = GetUnescapedVariableUnicode(escapedValue, 0, out consumed); break;
                        case 'u': result = GetUnescapedUnicode(escapedValue, 0, out consumed); break;
                        case 'U': result = GetUnescapedUnicode(escapedValue, 0, out consumed); break;
                    }
                }
                else
                {

                    if (length == 4) result = GetUnescapedUnicode("\\u" + escapedValue, 0, out consumed);
                    else if (length == 8) result = GetUnescapedUnicode("\\U" + escapedValue, 0, out consumed);
                    else if (length < 4) result = GetUnescapedVariableUnicode("\\x" + escapedValue, 0, out consumed);
                }
            }
            else result = GetUnescapedVariableUnicode("\\x" + escapedValue, 0, out consumed);

            if (consumed <= 0) return false;
            return true;
        }

        public static char GetUnescapedVariableUnicode(string escapedUnicode, int position, out int lettersConsumed)
        {
            lettersConsumed = -1;
            int totalLength = escapedUnicode.Length;

            if (position + 2 >= totalLength) return Char.MinValue;
            if (escapedUnicode[position] != '\\' || escapedUnicode[position + 1] != 'x') return Char.MinValue;
            totalLength = Math.Min(totalLength, position + 6);
            int start = position + 2;

            for (position = start; position < totalLength; position++)
                if (!HexadecimalMask.Get(escapedUnicode[position]))
                    break;

            if (position == start) return Char.MinValue;
            uint charValue;
            if (!UInt32.TryParse(escapedUnicode.Substring(start, position - start), NumberStyles.HexNumber, null, out charValue)) return Char.MinValue;
            lettersConsumed = (position - start) + 2;
            return (char)charValue;
        }

        public static char GetUnescapedUnicode(string escapedUnicode, int position, out int lettersConsumed)
        {
            lettersConsumed = -1;
            int totalLength = escapedUnicode.Length;

            if (position + 5 >= totalLength) return Char.MinValue;
            if (escapedUnicode[position] != '\\') return Char.MinValue;
            int required = escapedUnicode[position + 1] == 'u' ? 4 : (escapedUnicode[position + 1] == 'U' ? 8 : -1);
            if (required == -1) return Char.MinValue;

            totalLength = Math.Min(totalLength, position + required + 2);
            int start = position + 2;

            for (position = start; position < totalLength; position++)
                if (!HexadecimalMask.Get(escapedUnicode[position]))
                    break;

            if (position - start != required) return Char.MinValue;
            uint charValue;
            if (!UInt32.TryParse(escapedUnicode.Substring(start, required), NumberStyles.HexNumber, null, out charValue)) return Char.MinValue;
            if (required == 8 && charValue > 0xFFFF) return Char.MinValue;
            lettersConsumed = required + 2;
            return (char)charValue;
        }

        public static string GetUntilWhitespace(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (WhitespaceMask.Get(input[i], false))
                {
                    if (i == 0) return String.Empty;
                    return input.Substring(0, i);
                }

            return input;
        }

        public static int GetHighestIndexComponent(string[] formatStrings)
        {
            if (formatStrings == null || formatStrings.Length == 0) return -1;
            int count = -1, value = 0;

            for (int i = 0; i < formatStrings.Length; i++)
                if ((value = GetHighestIndexComponent(formatStrings[i])) > count)
                    count = value;

            return count;
        }

        public static unsafe int GetHighestIndexComponent(string formatString)
        {
            if (formatString == null) return 0;
            int length = formatString.Length;
            if (length < 3) return 0;
            int count = -1, value = 0;

            fixed (char* pointer = formatString)
            {
                char current = pointer[0], next = Char.MinValue;
                char* source = pointer;

                for (int i = 1; i < length; ++i)
                {
                    next = source[i];

                    if (current == '{') 
                    {
                        if (i + 1 >= length) break;

                        if (next == '{')
                        {
                            current = source[++i];
                            continue;
                        }
                        else if (DigitMask.Get(next))
                        {
                            int start = i;

                            for (++i; i < length; ++i)
                                if (!DigitMask.Get(next = source[i]))
                                {
                                    if ((value = Int32.Parse(formatString.Substring(start, i - start))) > count) count = value;
                                    break;
                                }
                        }
                    }

                    current = next;
                }
            }

            return count;
        }
        #endregion
    }
}
