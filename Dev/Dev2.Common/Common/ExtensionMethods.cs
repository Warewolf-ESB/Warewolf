/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Common.Common
{
    public static class ExtensionMethods
    {
        #region Exception Unrolling

        public static string GetAllMessages(this Exception exception)
        {
            IEnumerable<string> messages = exception.FromHierarchy(ex => ex.InnerException).Select(ex => ex.Message);
            return String.Join(Environment.NewLine, messages);
        }

        // a.k.a., linked list style enumerator
        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (TSource current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        #endregion Exception Unrolling

        #region StringBuilder Methods

        /// <summary>
        ///     Cleans the encoding header for XML save.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        public static StringBuilder CleanEncodingHeaderForXmlSave(this StringBuilder sb)
        {
            int removeStartIdx = sb.IndexOf("<?", 0, false);
            if (removeStartIdx >= 0)
            {
                int removeEndIdx = sb.IndexOf("?>", 0, false);
                int len = removeEndIdx - removeStartIdx + 2;
                StringBuilder result = sb.Remove(removeStartIdx, len);

                return result;
            }

            return sb;
        }

        /// <summary>
        ///     Writes the automatic file.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="fileManager"></param>
        public static void WriteToFile(this StringBuilder sb, string fileName, Encoding encoding, IFileManager fileManager)
        {
            int length = sb.Length;
            int startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

            // remove the darn header ;)
            sb = sb.CleanEncodingHeaderForXmlSave();
            if (!File.Exists(fileName))
            {
                using (File.Create(fileName))
                {
                    // Ensure it gets closed ;)
                }
            }
            fileManager.Snapshot(fileName);
            using (
                var fs = new FileStream(fileName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite, 4096, true))
            {
                for (int i = 0; i < rounds; i++)
                {
                    var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                    if (len > sb.Length - startIdx)
                    {
                        len = sb.Length - startIdx;
                    }

                    byte[] bytes = encoding.GetBytes(sb.Substring(startIdx, len));
                    fs.Write(bytes, 0, bytes.Length);
                    startIdx += len;
                }
            }
        }

        /// <summary>
        ///     Automatics the stream for XML load.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        public static XElement ToXElement(this StringBuilder sb)
        {
            try
            {
                // first try utf8, if that fails then unicode.
                // some test where kicking up issues with utf8 ;)

                using (Stream result = sb.EncodeStream(Encoding.UTF8))
                {
                    return XElement.Load(result);
                }
            }
            catch
            {
                using (Stream result = sb.EncodeStream(Encoding.Unicode))
                {
                    return XElement.Load(result);
                }
            }
        }

        public static bool IsNullOrEmpty(this StringBuilder sb)
        {
            return string.IsNullOrEmpty(sb?.ToString());
        }

        public static StringBuilder ToStringBuilder(this string str)
        {
            return new StringBuilder(str);
        }

        /// <summary>
        ///     Encodes for XML document.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        public static Stream EncodeForXmlDocument(this StringBuilder sb)
        {
            try
            {
                // first try utf8, if that fails then unicode.
                // some test where kicking up issues with utf8 ;)
                Stream result = sb.EncodeStream(Encoding.UTF8);
                XElement.Load(result);
                result.Position = 0;
                return result;
            }
            catch
            {
                Stream result = sb.EncodeStream(Encoding.Unicode);
                XElement.Load(result);
                result.Position = 0;
                return result;
            }
        }

        private static Stream EncodeStream(this StringBuilder sb, Encoding encoding)
        {
            int length = sb.Length;
            int startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);
            var ms = new MemoryStream(length);
            for (int i = 0; i < rounds; i++)
            {
                var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                if (len > sb.Length - startIdx)
                {
                    len = sb.Length - startIdx;
                }

                byte[] bytes = encoding.GetBytes(sb.Substring(startIdx, len));
                ms.Write(bytes, 0, bytes.Length);

                startIdx += len;
            }

            ms.Flush();
            ms.Position = 0;

            return ms;
        }

        public static string EscapeString(this string sb)
        {
            if (sb != null)
            {
                sb = sb.Replace("&", "&amp;");
                sb = sb.Replace("\"", "&quot;");
                sb = sb.Replace("'", "&apos;");
                sb = sb.Replace("<", "&lt;");
                sb = sb.Replace(">", "&gt;");
            }

            return sb;
        }

        /// <summary>
        ///     Unescapes the specified string builder
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        public static StringBuilder Unescape(this StringBuilder sb)
        {
            if (sb != null)
            {
                sb = sb.Replace("&quot;", "\"");
                sb = sb.Replace("&apos;", "'");
                sb = sb.Replace("&lt;", "<");
                sb = sb.Replace("&gt;", ">");
                sb = sb.Replace("&amp;", "&");
            }

            return sb;
        }

        /// <summary>
        ///     Unescapes the specified string builder
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        public static string UnescapeString(this string sb)
        {
            if (sb != null)
            {
                sb = sb.Replace("&quot;", "\"");
                sb = sb.Replace("&apos;", "'");
                sb = sb.Replace("&lt;", "<");
                sb = sb.Replace("&gt;", ">");
                sb = sb.Replace("&amp;", "&");
            }

            return sb;
        }

        /// <summary>
        ///     Determines whether [contains] [the specified string builder].
        /// </summary>
        /// <param name="sb">The string builder</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool Contains(this StringBuilder sb, string value)
        {
            return IndexOf(sb, value, 0, false) >= 0;
        }

        /// <summary>
        ///     Substrings the specified string builder.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="startIdx">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string Substring(this StringBuilder sb, int startIdx, int length)
        {
            return sb.ToString(startIdx, length);
        }

        /// <summary>
        ///     Lasts the index of.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static int LastIndexOf(this StringBuilder sb, string value, bool ignoreCase)
        {
            int result = -1;
            int startIndex = -1;
            while ((startIndex = IndexOf(sb, value, startIndex + 1, ignoreCase)) >= 0)
            {
                result = startIndex;
            }

            return result;
        }

        /// <summary>
        ///     Indexes the of.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="value">The value.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase)
        {
            if (value == null) return -1;
            int index;
            int length = value.Length;
            int maxSearchLength = sb.Length - length + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                    {
                        index = 1;
                        while (index < length && Char.ToLower(sb[i + index]) == Char.ToLower(value[index]))
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

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while (index < length && sb[i + index] == value[index])
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

        #endregion StringBuilder Methods

        /// <summary>
        ///     Extracts the XML attribute from unsafe XML.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="searchTagStart">The search tag start.</param>
        /// <param name="searchTagEnd">The search tag end.</param>
        /// <returns></returns>
        public static string ExtractXmlAttributeFromUnsafeXml(this StringBuilder sb, string searchTagStart,
            string searchTagEnd = "\"")
        {
            // 10 chars long
            int startIndex = sb.IndexOf(searchTagStart, 0, false);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            int tagLength = searchTagStart.Length;
            startIndex += tagLength;
            int endIdx = sb.IndexOf(searchTagEnd, startIndex, false);
            int length = endIdx - startIndex;

            return sb.Substring(startIndex, length);
        }

        // -- End StringBuilder Methods

        public static string AttributeSafe(this XElement elem, string name, bool returnsNull = false)
        {
            XAttribute attr = elem.Attribute(name);
            if (string.IsNullOrEmpty(attr?.Value))
            {
                return returnsNull ? null : string.Empty;
            }
            return attr.Value;
        }

        public static StringBuilder ElementSafeStringBuilder(this XElement elem, string name)
        {
            XElement child = elem.Element(name);
            return child == null ? new StringBuilder() : child.ToStringBuilder();
        }

        public static string ElementSafe(this XElement elem, string name)
        {
            XElement child = elem.Element(name);
            return child?.Value ?? string.Empty;
        }

        public static string ElementStringSafe(this XElement elem, string name)
        {
            XElement child = elem.Element(name);
            return child?.ToString() ?? string.Empty;
        }
        /// <summary>
        /// https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs,cfaa9abd8b214ecb
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return new ObservableCollection<T>();
            var col = new ObservableCollection<T>(enumerable);
            return col;
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            return GetByteArray(stream);
        }

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
    }
}