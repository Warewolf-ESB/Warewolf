// Decompiled with JetBrains decompiler
// Type: System.StringUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System
{
  internal static class StringUtility
  {
    public const char Space = ' ';
    public const char NewLine = '\n';
    public const string StrNewLine = "\n";
    public const int IntNewLine = 10;
    private const int CharStorageCapacity = 1024;
    private static readonly char[] CharStorage = new char[1024];
    public static readonly char[] Digits = StringUtility.BuildCharacterRange(48, 10);
    public static readonly char[] Hexadecimal = StringUtility.BuildCharacterRange(StringUtility.BuildCharacterRange(StringUtility.BuildCharacterRange(new char[22], 0, 48, 10), 10, 65, 6), 16, 97, 6);
    public static readonly char[] LowercaseLetters = StringUtility.BuildCharacterRange(97, 26);
    public static readonly char[] UppercaseLetters = StringUtility.BuildCharacterRange(65, 26);
    public static readonly char[] RegularCharEscapeCharacters = new char[13]
    {
      '\'',
      '\\',
      '0',
      'a',
      'b',
      'f',
      'n',
      'r',
      't',
      'v',
      'x',
      'u',
      'U'
    };
    public static readonly char[] RegularCharUnescapeCharacters = new char[10]
    {
      '\'',
      '\\',
      char.MinValue,
      '\a',
      '\b',
      '\f',
      '\n',
      '\r',
      '\t',
      '\v'
    };
    public static readonly char[] RegularStringEscapeCharacters = new char[13]
    {
      '"',
      '\\',
      '0',
      'a',
      'b',
      'f',
      'n',
      'r',
      't',
      'v',
      'x',
      'u',
      'U'
    };
    public static readonly char[] RegularStringUnescapeCharacters = new char[9]
    {
      '"',
      char.MinValue,
      '\a',
      '\b',
      '\f',
      '\n',
      '\r',
      '\t',
      '\v'
    };
    public static readonly char[] WhitespaceChars = new char[25]
    {
      '\t',
      '\n',
      '\v',
      '\f',
      '\r',
      ' ',
      '\u0085',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      ' ',
      '\u200B',
      '\u2028',
      '\u2029',
      '　',
      '\uFEFF'
    };
    public static readonly char[] ControlChars = new char[6]
    {
      char.MinValue,
      '\a',
      '\b',
      '\f',
      '\r',
      '\v'
    };
    public static readonly char[] CaseInsensitiveRange = StringUtility.BuildCharacterRange(StringUtility.BuildCharacterRange(new char[256], 0, 0, 256), 65, 97, 26);
    public static readonly BooleanArray WhitespaceMask = new BooleanArray(StringUtility.WhitespaceChars);
    public static readonly BooleanArray ControlMask = new BooleanArray(StringUtility.ControlChars);
    public static readonly BooleanArray WhitespaceOrControlMask = BooleanArray.Or(StringUtility.WhitespaceMask, StringUtility.ControlMask);
    public static readonly BooleanArray DigitMask = new BooleanArray(StringUtility.Digits, 256);
    public static readonly BooleanArray HexadecimalMask = new BooleanArray(StringUtility.Hexadecimal, 256);
    public static readonly BooleanArray LowercaseMask = new BooleanArray(StringUtility.LowercaseLetters, 256);
    public static readonly BooleanArray UppercaseMask = new BooleanArray(StringUtility.UppercaseLetters, 256);
    public static readonly BooleanArray LetterMask = BooleanArray.Or(StringUtility.LowercaseMask, StringUtility.UppercaseMask);
    public static readonly BooleanArray RegularCharEscapeMask = new BooleanArray(StringUtility.RegularCharEscapeCharacters, 256);
    public static readonly BooleanArray RegularStringEscapeMask = new BooleanArray(StringUtility.RegularStringEscapeCharacters, 256);

    public static char[] BuildCharacterRange(char[] result, int index, int start, int length)
    {
      for (int index1 = 0; index1 < length; ++index1)
        result[index1 + index] = (char) (start + index1);
      return result;
    }

    public static char[] BuildCharacterRange(int start, int length)
    {
      char[] chArray = new char[length];
      for (int index = 0; index < chArray.Length; ++index)
        chArray[index] = (char) (start + index);
      return chArray;
    }

    public static string BuildIdentifierPrefix(int index, bool excludeFinalLetter)
    {
      int num = 0;
      int[] numArray = new int[100];
      int index1 = 0;
      for (int index2 = 25; index2 < index; index2 += 26)
      {
        ++numArray[index1];
        while (numArray[index1] == 27)
        {
          numArray[index1++] = 1;
          ++numArray[index1];
        }
        if (index1 > num)
          num = index1;
        index1 = 0;
      }
      if (index >= 26)
        ++num;
      index -= index / 26 * 26;
      string str = excludeFinalLetter ? "" : ((char) (index + 65)).ToString();
      for (int index3 = 0; index3 < num; ++index3)
        str += ((char) (numArray[index3] + 64)).ToString();
      char[] charArray = str.ToCharArray();
			Array.Reverse(charArray);
      return new string(charArray);
    }

    public static bool AreDigits(this string self, int startIndex)
    {
      int length = self.Length;
      for (int index = startIndex; index < length; ++index)
      {
        if (!char.IsDigit(self, index))
          return false;
      }
      return true;
    }

    public static bool AreDigits(this string self, int startIndex, int count)
    {
      for (int index = 0; index < count; ++index)
      {
        if (!char.IsDigit(self, index + startIndex))
          return false;
      }
      return true;
    }

    public static unsafe bool Equals(
      string left,
      int leftIndex,
      string right,
      int rightIndex,
      int count)
    {
      if (leftIndex == rightIndex && (object) left == (object) right)
        return true;
      IntPtr num1;
      if (left == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = left)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num1;
      IntPtr num2;
      if (right == null)
      {
        num2 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr2 = right)
          num2 = (IntPtr) chPtr2;
      }
      char* chPtr3 = (char*) num2;
      char* chPtr4 = chPtr1 + leftIndex;
      char* chPtr5 = chPtr3 + rightIndex;
      for (int index = 0; index < count; ++index)
      {
        if ((int) chPtr4[index] != (int) chPtr5[index])
          return false;
      }
      return true;
    }

    public static string CapitalizeByWord(string value)
    {
      char[] charArray = value.ToCharArray();
      bool flag = true;
      for (int index = 0; index < charArray.Length; ++index)
      {
        if (char.IsWhiteSpace(charArray[index]))
          flag = true;
        else if (flag)
        {
          flag = false;
          charArray[index] = char.ToUpper(charArray[index]);
        }
      }
      return new string(charArray);
    }

    public static string ApplyMask(string input, BooleanArray mask) => StringUtility.ApplyMask(input, mask, false, out bool _);

    public static string ApplyMask(string input, BooleanArray mask, bool comparand) => StringUtility.ApplyMask(input, mask, comparand, out bool _);

    public static unsafe string ApplyMask(
      string input,
      BooleanArray mask,
      bool comparand,
      out bool changed)
    {
      changed = false;
      if (input == null)
        return input;
      int length1 = input.Length;
      if (length1 == 0)
        return input;
      string empty = string.Empty;
      int length2 = mask.Length;
      int length3 = 0;
      char[] charStorage = StringUtility.CharStorage;
      IntPtr num;
      if (input == null)
      {
        num = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = input)
          num = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num;
      for (int index1 = 0; index1 < length1; ++index1)
      {
        char index2 = chPtr1[index1];
        if ((int) index2 >= length2 && !comparand || mask[(int) index2] == comparand)
        {
          charStorage[length3++] = index2;
          if (length3 == 1024)
          {
            empty += new string(charStorage);
            length3 = 0;
          }
        }
        else
          changed = true;
      }
      if (length3 != 0)
        empty += new string(charStorage, 0, length3);
      return empty;
    }

    public static string GetUnescapedString(string escapedValue)
    {
      bool verbatim;
      bool regular;
      escapedValue = StringUtility.PrepareEscapedString(escapedValue, out verbatim, out regular);
      if (string.IsNullOrEmpty(escapedValue))
        return escapedValue;
      if (verbatim)
        return StringUtility.GetUnescapedString(escapedValue, true);
      if (regular)
        return StringUtility.GetUnescapedString(escapedValue, false);
      return (StringUtility.GetUnescapedString(escapedValue, false) ?? StringUtility.GetUnescapedString(escapedValue, true)) ?? throw new ArgumentException("Cannot determine the nature of the escaped string.");
    }

    public static string GetUnescapedVerbatimString(string escapedValue)
    {
      escapedValue = StringUtility.PrepareEscapedString(escapedValue, out bool _, out bool _);
      return string.IsNullOrEmpty(escapedValue) ? escapedValue : StringUtility.GetUnescapedString(escapedValue, true);
    }

    public static string GetUnescapedRegularString(string escapedValue)
    {
      escapedValue = StringUtility.PrepareEscapedString(escapedValue, out bool _, out bool _);
      return string.IsNullOrEmpty(escapedValue) ? escapedValue : StringUtility.GetUnescapedString(escapedValue, false);
    }

    private static string PrepareEscapedString(
      string escapedValue,
      out bool verbatim,
      out bool regular)
    {
      verbatim = regular = false;
      if (string.IsNullOrEmpty(escapedValue))
        return escapedValue;
      int length = escapedValue.Length;
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      if (length > 1)
      {
        flag1 = escapedValue[0] == '"';
        flag3 = !flag1 && escapedValue[0] == '@' && escapedValue[1] == '"';
        flag2 = length > (!flag3 ? 1 : 2) && escapedValue[length - 1] == '"';
      }
      if (verbatim = flag3 & flag2)
        escapedValue = length == 3 ? "" : escapedValue.Substring(2, length - 3);
      else if (regular = flag1 & flag2)
        escapedValue = length == 2 ? "" : escapedValue.Substring(1, length - 2);
      return escapedValue;
    }

    private static string GetUnescapedString(string escapedValue, bool verbatim)
    {
      int startIndex1 = 0;
      if (verbatim)
      {
        int length = escapedValue.Length;
        int startIndex2 = 0;
        string str = "";
        int num;
        while ((num = escapedValue.IndexOf("\"", startIndex1)) != -1)
        {
          if (num + 1 >= length)
            return (string) null;
          if (escapedValue[num + 1] != '"')
            return (string) null;
          if (num - startIndex2 > 0)
            str += escapedValue.Substring(startIndex2, num - startIndex2);
          str += "\"";
          startIndex1 = startIndex2 = num + 2;
          if (startIndex1 + 1 >= length)
            break;
        }
        if (startIndex2 < length)
          str += escapedValue.Substring(startIndex2, length - startIndex2);
        escapedValue = str;
      }
      else if (escapedValue.IndexOf('\\') != -1)
      {
        List<string> stringList = new List<string>();
        int length;
        for (; (length = escapedValue.IndexOf("\\\\")) != -1; escapedValue = escapedValue.Substring(length + 2))
        {
          if (length > 0)
            stringList.Add(escapedValue.Substring(0, length));
          else
            stringList.Add(string.Empty);
          stringList.Add((string) null);
          if (length + 2 == escapedValue.Length)
          {
            escapedValue = string.Empty;
            break;
          }
        }
        stringList.Add(escapedValue);
        escapedValue = "";
        for (int index1 = 0; index1 < stringList.Count; ++index1)
        {
          string escapedUnicode = stringList[index1];
          if (escapedUnicode == null)
            escapedValue += "\\";
          else if (escapedUnicode.Length > 0)
          {
            for (int index2 = escapedUnicode.IndexOf('\\'); index2 != -1; index2 = escapedUnicode.IndexOf('\\', index2))
            {
              if (index2 + 1 == escapedUnicode.Length)
                return (string) null;
              int lettersConsumed = 2;
              char ch;
              switch (escapedUnicode[index2 + 1])
              {
                case '"':
                  ch = '"';
                  break;
                case '0':
                  ch = char.MinValue;
                  break;
                case 'U':
                  ch = StringUtility.GetUnescapedUnicode(escapedUnicode, index2, out lettersConsumed);
                  break;
                case 'a':
                  ch = '\a';
                  break;
                case 'b':
                  ch = '\b';
                  break;
                case 'f':
                  ch = '\f';
                  break;
                case 'n':
                  ch = '\n';
                  break;
                case 'r':
                  ch = '\r';
                  break;
                case 't':
                  ch = '\t';
                  break;
                case 'u':
                  ch = StringUtility.GetUnescapedUnicode(escapedUnicode, index2, out lettersConsumed);
                  break;
                case 'v':
                  ch = '\v';
                  break;
                case 'x':
                  ch = StringUtility.GetUnescapedVariableUnicode(escapedUnicode, index2, out lettersConsumed);
                  break;
                default:
                  return (string) null;
              }
              if (lettersConsumed <= 0)
                return (string) null;
              escapedUnicode = (index2 > 0 ? escapedUnicode.Remove(index2) : string.Empty) + ch.ToString() + (index2 + lettersConsumed < escapedUnicode.Length ? escapedUnicode.Substring(index2 + lettersConsumed) : string.Empty);
            }
            escapedValue += escapedUnicode;
          }
        }
      }
      else if (escapedValue.IndexOfAny(StringUtility.RegularStringUnescapeCharacters) != -1)
        escapedValue = (string) null;
      return escapedValue;
    }

    public static bool GetUnescapedChar(string escapedValue, out char result)
    {
      result = char.MinValue;
      if (string.IsNullOrEmpty(escapedValue))
        return false;
      int length = escapedValue.Length;
      if (escapedValue[0] == '\'')
      {
        if (length < 3 || escapedValue[length - 1] != '\'')
          return false;
        escapedValue = escapedValue.Substring(1, length - 2);
        length -= 2;
      }
      if (escapedValue[0] == '\\')
      {
        if (length < 2)
          return false;
        int lettersConsumed = 2;
        char ch;
        switch (escapedValue[1])
        {
          case '\'':
            ch = '\'';
            break;
          case '0':
            ch = char.MinValue;
            break;
          case 'U':
            ch = StringUtility.GetUnescapedUnicode(escapedValue, 0, out lettersConsumed);
            break;
          case '\\':
            ch = '\\';
            break;
          case 'a':
            ch = '\a';
            break;
          case 'b':
            ch = '\b';
            break;
          case 'f':
            ch = '\f';
            break;
          case 'n':
            ch = '\n';
            break;
          case 'r':
            ch = '\r';
            break;
          case 't':
            ch = '\t';
            break;
          case 'u':
            ch = StringUtility.GetUnescapedUnicode(escapedValue, 0, out lettersConsumed);
            break;
          case 'v':
            ch = '\v';
            break;
          case 'x':
            ch = StringUtility.GetUnescapedVariableUnicode(escapedValue, 0, out lettersConsumed);
            break;
          default:
            return false;
        }
        if (lettersConsumed != length)
          return false;
        result = ch;
      }
      else
      {
        if (length != 1 || escapedValue.IndexOfAny(StringUtility.RegularCharUnescapeCharacters) != -1)
          return false;
        result = escapedValue[0];
      }
      return true;
    }

    public static bool GetUnescapedUnicode(string escapedValue, out char result)
    {
      if (string.IsNullOrEmpty(escapedValue))
      {
        result = char.MinValue;
        return false;
      }
      result = char.MinValue;
      int length = escapedValue.Length;
      int lettersConsumed = -1;
      if (length > 2)
      {
        if (escapedValue[0] == '\\')
        {
          switch (escapedValue[1])
          {
            case 'U':
              result = StringUtility.GetUnescapedUnicode(escapedValue, 0, out lettersConsumed);
              break;
            case 'u':
              result = StringUtility.GetUnescapedUnicode(escapedValue, 0, out lettersConsumed);
              break;
            case 'x':
              result = StringUtility.GetUnescapedVariableUnicode(escapedValue, 0, out lettersConsumed);
              break;
          }
        }
        else
        {
          switch (length)
          {
            case 4:
              result = StringUtility.GetUnescapedUnicode("\\u" + escapedValue, 0, out lettersConsumed);
              break;
            case 8:
              result = StringUtility.GetUnescapedUnicode("\\U" + escapedValue, 0, out lettersConsumed);
              break;
            default:
              if (length < 4)
              {
                result = StringUtility.GetUnescapedVariableUnicode("\\x" + escapedValue, 0, out lettersConsumed);
                break;
              }
              break;
          }
        }
      }
      else
        result = StringUtility.GetUnescapedVariableUnicode("\\x" + escapedValue, 0, out lettersConsumed);
      return lettersConsumed > 0;
    }

    public static char GetUnescapedVariableUnicode(
      string escapedUnicode,
      int position,
      out int lettersConsumed)
    {
      lettersConsumed = -1;
      int length = escapedUnicode.Length;
      if (position + 2 >= length || escapedUnicode[position] != '\\' || escapedUnicode[position + 1] != 'x')
        return char.MinValue;
      int num = Math.Min(length, position + 6);
      int startIndex = position + 2;
      position = startIndex;
      while (position < num && StringUtility.HexadecimalMask.Get(escapedUnicode[position]))
        ++position;
      uint result;
      if (position == startIndex || !uint.TryParse(escapedUnicode.Substring(startIndex, position - startIndex), NumberStyles.HexNumber, (IFormatProvider) null, out result))
        return char.MinValue;
      lettersConsumed = position - startIndex + 2;
      return (char) result;
    }

    public static char GetUnescapedUnicode(
      string escapedUnicode,
      int position,
      out int lettersConsumed)
    {
      lettersConsumed = -1;
      int length1 = escapedUnicode.Length;
      if (position + 5 >= length1 || escapedUnicode[position] != '\\')
        return char.MinValue;
      int length2 = escapedUnicode[position + 1] == 'u' ? 4 : (escapedUnicode[position + 1] == 'U' ? 8 : -1);
      if (length2 == -1)
        return char.MinValue;
      int num = Math.Min(length1, position + length2 + 2);
      int startIndex = position + 2;
      position = startIndex;
      while (position < num && StringUtility.HexadecimalMask.Get(escapedUnicode[position]))
        ++position;
      uint result;
      if (position - startIndex != length2 || !uint.TryParse(escapedUnicode.Substring(startIndex, length2), NumberStyles.HexNumber, (IFormatProvider) null, out result) || length2 == 8 && result > (uint) ushort.MaxValue)
        return char.MinValue;
      lettersConsumed = length2 + 2;
      return (char) result;
    }

    public static string GetUntilWhitespace(string input)
    {
      for (int index = 0; index < input.Length; ++index)
      {
        if (StringUtility.WhitespaceMask.Get((int) input[index], false))
          return index == 0 ? string.Empty : input.Substring(0, index);
      }
      return input;
    }

    public static int GetHighestIndexComponent(string[] formatStrings)
    {
      if (formatStrings == null || formatStrings.Length == 0)
        return -1;
      int highestIndexComponent1 = -1;
      for (int index = 0; index < formatStrings.Length; ++index)
      {
        int highestIndexComponent2;
        if ((highestIndexComponent2 = StringUtility.GetHighestIndexComponent(formatStrings[index])) > highestIndexComponent1)
          highestIndexComponent1 = highestIndexComponent2;
      }
      return highestIndexComponent1;
    }

    public static unsafe int GetHighestIndexComponent(string formatString)
    {
      if (formatString == null)
        return 0;
      int length = formatString.Length;
      if (length < 3)
        return 0;
      int highestIndexComponent = -1;
      IntPtr num1;
      if (formatString == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = formatString)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num1;
      char ch = *chPtr1;
      char* chPtr2 = chPtr1;
      for (int index = 1; index < length; ++index)
      {
        char letter = chPtr2[index];
        if (ch == '{')
        {
          if (index + 1 < length)
          {
            if (letter == '{')
            {
              ch = chPtr2[++index];
              continue;
            }
            if (StringUtility.DigitMask.Get(letter))
            {
              int startIndex = index;
              for (++index; index < length; ++index)
              {
                if (!StringUtility.DigitMask.Get(letter = chPtr2[index]))
                {
                  int num2;
                  if ((num2 = int.Parse(formatString.Substring(startIndex, index - startIndex))) > highestIndexComponent)
                  {
                    highestIndexComponent = num2;
                    break;
                  }
                  break;
                }
              }
            }
          }
          else
            break;
        }
        ch = letter;
      }
      return highestIndexComponent;
    }
  }
}
