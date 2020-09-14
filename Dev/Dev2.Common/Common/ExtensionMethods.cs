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

using ChinhDo.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CodeDom;

namespace Dev2.Common.Common
{
    public static class ExtensionMethods
    {
        public static string GetAllMessages(this Exception exception)
        {
            var messages = exception.FromHierarchy(ex => ex.InnerException).Select(ex => ex.Message);
            return String.Join(Environment.NewLine, messages);
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (TSource current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem)
            where TSource : class => FromHierarchy(source, nextItem, s => s != null);
        
        public static StringBuilder CleanEncodingHeaderForXmlSave(this StringBuilder sb)
        {
            var removeStartIdx = sb.IndexOf("<?", 0, false);
            if (removeStartIdx >= 0)
            {
                var removeEndIdx = sb.IndexOf("?>", 0, false);
                var len = removeEndIdx - removeStartIdx + 2;
                var result = sb.Remove(removeStartIdx, len);

                return result;
            }

            return sb;
        }

        public static void WriteToFile(this StringBuilder sb, string fileName, Encoding encoding, IFileManager fileManager)
        {
            var length = sb.Length;
            var startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);
            var cleanStringBuilder = sb.CleanEncodingHeaderForXmlSave();

            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }
            fileManager.Snapshot(fileName);
            using (
                var fs = new FileStream(fileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite, 4096, true))
            {
                for (int i = 0; i < rounds; i++)
                {
                    var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                    if (len > cleanStringBuilder.Length - startIdx)
                    {
                        len = cleanStringBuilder.Length - startIdx;
                    }

                    var bytes = encoding.GetBytes(cleanStringBuilder.Substring(startIdx, len));
                    fs.Write(bytes, 0, bytes.Length);
                    startIdx += len;
                }
            }
        }

        public static XElement ToXElement(this StringBuilder sb)
        {
            try
            {
                using (Stream result = sb.EncodeStream(Encoding.UTF8))
                {
                    return XElement.Load(result);
                }
            }
            catch (Exception ex)
            {
                using (Stream result = sb.EncodeStream(Encoding.Unicode))
                {
                    return XElement.Load(result);
                }
            }
        }

        public static bool IsNullOrEmpty(this StringBuilder sb) => string.IsNullOrEmpty(sb?.ToString());

        public static StringBuilder ToStringBuilder(this string str) => new StringBuilder(str);

        public static Stream EncodeForXmlDocument(this StringBuilder sb)
        {
            try
            {
                var result = sb.EncodeStream(Encoding.UTF8);
                XElement.Load(result);
                result.Position = 0;
                return result;
            }
            catch (Exception ex)
            {
                var result = sb.EncodeStream(Encoding.Unicode);
                XElement.Load(result);
                result.Position = 0;
                return result;
            }
        }

        static Stream EncodeStream(this StringBuilder sb, Encoding encoding)
        {
            var length = sb.Length;
            var startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);
            var ms = new MemoryStream(length);
            for (int i = 0; i < rounds; i++)
            {
                var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                if (len > sb.Length - startIdx)
                {
                    len = sb.Length - startIdx;
                }

                var bytes = encoding.GetBytes(sb.Substring(startIdx, len));
                ms.Write(bytes, 0, bytes.Length);

                startIdx += len;
            }

            ms.Flush();
            ms.Position = 0;

            return ms;
        }

        public static string EscapeString(this string sb)
        {
            string escapedString = null;
            if (sb != null)
            {
                escapedString = sb.Replace("&", "&amp;");
                escapedString = escapedString.Replace("\"", "&quot;");
                escapedString = escapedString.Replace("'", "&apos;");
                escapedString = escapedString.Replace("<", "&lt;");
                escapedString = escapedString.Replace(">", "&gt;");
            }

            return escapedString;
        }

        public static StringBuilder Unescape(this StringBuilder sb)
        {
            StringBuilder unescapedString = null;
            if (sb != null)
            {
                unescapedString = sb.Replace("&quot;", "\"");
                unescapedString = unescapedString.Replace("&apos;", "'");
                unescapedString = unescapedString.Replace("&lt;", "<");
                unescapedString = unescapedString.Replace("&gt;", ">");
                unescapedString = unescapedString.Replace("&amp;", "&");
            }
            return unescapedString;
        }

        public static string UnescapeString(this string sb)
        {
            string unescapedString = null;
            if (sb != null)
            {
                unescapedString = sb.Replace("&quot;", "\"");
                unescapedString = unescapedString.Replace("&apos;", "'");
                unescapedString = unescapedString.Replace("&lt;", "<");
                unescapedString = unescapedString.Replace("&gt;", ">");
                unescapedString = unescapedString.Replace("&amp;", "&");
            }
            return unescapedString;
        }

        public static bool Contains(this StringBuilder sb, string value) => IndexOf(sb, value, 0, false) >= 0;

        public static string Substring(this StringBuilder sb, int startIdx, int length)
        {
            return sb.ToString(startIdx, length);
        }

        public static int LastIndexOf(this StringBuilder sb, string value, bool ignoreCase)
        {
            var result = -1;
            var startIndex = -1;
            while ((startIndex = IndexOf(sb, value, startIndex + 1, ignoreCase)) >= 0)
            {
                result = startIndex;
            }
            return result;
        }

        public static int LastIndexOf(this StringBuilder sb, string value, int startIdx, bool ignoreCase)
        {
            var result = -1;
            var startIndex = IndexOf(sb, value, 0, ignoreCase);
            while (startIdx - startIndex >= 0 && startIndex != -1)
            {
                result = startIndex;
                startIndex = IndexOf(sb, value, startIndex + 1, ignoreCase);
            }
            return result;
        }

        public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase) => IndexOf(sb, value, null, startIndex, ignoreCase);

        public static int IndexOf(this StringBuilder sb, string value, string escapeChar, int startIndex, bool ignoreCase)
        {
            if (value == null)
            {
                return -1;
            }

            var length = value.Length;
            var maxSearchLength = sb.Length - length + 1;

            if (ignoreCase)
            {
                return GetIndexOfCharacter(sb, value, startIndex, maxSearchLength, length);
            }

            return GetIndexOfCharacter(sb, value, escapeChar, startIndex, maxSearchLength, length);

        }

        private static int GetIndexOfCharacter(this StringBuilder sb, string value, int startIndex, int maxSearchLength, int length)
        {
            var index = 1;

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                index = sb.IndexOf(value, index, length, i);
                var IsSbCharAsValue = Char.ToLower(sb[i]) == Char.ToLower(value[0]);

                if (IsSbCharAsValue && index == length)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int GetIndexOfCharacter(this StringBuilder sb, string value, string escapeChar, int startIndex, int maxSearchLength, int length)
        {
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    var index = 1;
                    while (index < length && sb[i + index] == value[index] || SkipDueToEscapeChar(sb, startIndex, i + index - 1, escapeChar, value))
                    {
                        ++index;
                    }

                    if (index == length)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        static int IndexOf(this StringBuilder sb, string value, int index, int length, int startingAt)
        {
            if (Char.ToLower(sb[startingAt]) == Char.ToLower(value[0]))
            {
                index = 1;
                while (index < length && Char.ToLower(sb[startingAt + index]) == Char.ToLower(value[index]))
                {
                    ++index;
                }
            }

            return index;
        }

        static bool SkipDueToEscapeChar(StringBuilder word, int startIdx, int candidatePos, string escapeChar, string searchValue)
        {
            if (!String.IsNullOrEmpty(escapeChar))
            {
                var charToRemove = escapeChar.Length == 1 ? 2 : 1;
                var checkValue = escapeChar + searchValue;
                var check = word.Substring(startIdx, word.Length - candidatePos + checkValue.Length - charToRemove);
                return check.Contains(checkValue) && check.EndsWith(checkValue);
            }
            return false;
        }

        /// <summary>
        ///     Turns xml into string builder
        /// </summary>
        /// <param name="elm">The elm.</param>
        /// <returns></returns>
        public static StringBuilder ToStringBuilder(this XElement elm)
        {
            var result = new StringBuilder();
            using (var sw = new StringWriter(result))
            {
                elm.Save(sw, SaveOptions.DisableFormatting);
            }

            return result.CleanEncodingHeaderForXmlSave();
        }

        public static bool IsEqual(this StringBuilder sb, StringBuilder that)
        {
            if (that != null && sb != null && sb.Length == that.Length)
            {
                // length check passes, check content ;)
                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] != that[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (that != null && sb != null)
                {
                    var compareOrdinal = string.CompareOrdinal(sb.ToString(), that.ToString());
                    if (compareOrdinal == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }

        public static string ExtractXmlAttributeFromUnsafeXml(this StringBuilder sb, string searchTagStart) => sb.ExtractXmlAttributeFromUnsafeXml(searchTagStart, "\"");

        public static string ExtractXmlAttributeFromUnsafeXml(this StringBuilder sb, string searchTagStart,
            string searchTagEnd)
        {
            var startIndex = sb.IndexOf(searchTagStart, 0, false);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            var tagLength = searchTagStart.Length;
            startIndex += tagLength;
            var endIdx = sb.IndexOf(searchTagEnd, startIndex, false);
            var length = endIdx - startIndex;

            return sb.Substring(startIndex, length);
        }

        public static string AttributeSafe(this XElement elem, string name) => elem.AttributeSafe(name, false);

        public static string AttributeSafe(this XElement elem, string name, bool returnsNull)
        {
            var attr = elem.Attribute(name);
            if (string.IsNullOrEmpty(attr?.Value))
            {
                return returnsNull ? null : string.Empty;
            }
            return attr.Value;
        }

        public static StringBuilder ElementSafeStringBuilder(this XElement elem, string name)
        {
            var child = elem.Element(name);
            return child == null ? new StringBuilder() : child.ToStringBuilder();
        }

        public static string ElementSafe(this XElement elem, string name)
        {
            var child = elem.Element(name);
            return child?.Value ?? string.Empty;
        }

        public static string ElementStringSafe(this XElement elem, string name)
        {
            var child = elem.Element(name);
            return child?.ToString() ?? string.Empty;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new ObservableCollection<T>();
            }

            var col = new ObservableCollection<T>(enumerable);
            return col;
        }

        public static bool IsValidXml(this string content)
        {
            if (string.IsNullOrEmpty(content) || !content.TrimStart().StartsWith("<"))
            {
                return false;
            }

            try
            {

                XDocument.Parse(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidJson(this string strInput)
        {
            var trimmedInput = strInput.Trim();
            if (trimmedInput.StartsWith("{") && trimmedInput.EndsWith("}") || trimmedInput.StartsWith("[") && trimmedInput.EndsWith("]", StringComparison.CurrentCulture))
            {
                try
                {
                    JToken.Parse(trimmedInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            return false;
        }

        public static byte[] ToByteArray(this Stream stream) => GetByteArray(stream);

        public static byte[] GetByteArray(Stream stream)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string ToBase64String(this byte[] bytes, Base64FormattingOptions options = Base64FormattingOptions.None) => Convert.ToBase64String(bytes, options);

        public static byte[] Base64StringToByteArray(this string base64String) => GetBase64StringToByteArray(base64String);

        private static byte[] GetBase64StringToByteArray(string base64String)
        {
            if (base64String.IsBase64String(out byte[] bytes))
            {
                return bytes;
            }
            return GetBytesFromFailedBase64String(base64String);
        }

        public static string ReadToString(this byte[] bytes) => GetReadToEnd(bytes);

        private static string GetReadToEnd(byte[] bytes)
        {
            var text = string.Empty;
            using (var stream = new StreamReader(new MemoryStream(bytes)))
            {
                text = stream.ReadToEnd();
            }
            return text;
        }

        public static string ReadToEnd(this Stream stream) => GetReadToEnd(stream);

        private static string GetReadToEnd(Stream stream)
        {
            var stringStream = string.Empty;
            using (var tempStream = new StreamReader(stream))
            {
                stringStream = tempStream.ReadToEnd();
            };
            return stringStream;
        }

        public static bool IsBase64Stream(this Stream stream, out byte[] bytes) => GetIsBase64Stream(stream, out bytes);

        private static bool GetIsBase64Stream(Stream stream, out byte[] bytes)
        {
            return IsBase64String(stream.ReadToEnd(), out bytes);
        }

        public static bool IsBase64String(this string base64String, out byte[] bytes) => GetIsBase64String(base64String, out bytes);

        private static bool GetIsBase64String(string base64String, out byte[] bytes)
        {
            if (base64String.Replace(" ", "").Length % 4 != 0)
            {
                bytes = GetBytesFromFailedBase64String(base64String);
                return false;
            }
            try
            {
                bytes = GetBytesFromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {
                bytes = GetBytesFromFailedBase64String(base64String);
                return false;
            }
        }

        private static byte[] GetBytesFromFailedBase64String(string base64String) => Encoding.ASCII.GetBytes(base64String);

        private static byte[] GetBytesFromBase64String(string base64String) => Convert.FromBase64String(base64String);
    }
}