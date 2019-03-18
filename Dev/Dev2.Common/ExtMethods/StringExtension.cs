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

using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Dev2.Common.ExtMethods
{
    public static class StringExtension
    {
        static readonly Regex IsAlphaRegex = new Regex("^[a-zA-Z ]*$", RegexOptions.Compiled);
        static readonly Regex IsAlphaNumericRegex = new Regex("^[0-9a-zA-Z]*$", RegexOptions.Compiled);

        static readonly Regex IsEmailRegex = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static readonly Regex IsBinaryField = new Regex("^[01]+$");
        static readonly Regex IsHex1 = new Regex(@"\A\b[0-9a-fA-F]+\b\Z");
        static readonly Regex IsHex2 = new Regex(@"\A\b(0[xX])?[0-9a-fA-F]+\b\Z");
        public static bool IsDate(this string payload)
        {
            var result = false;

            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var acceptedDateFormats = new List<string>
            {
                "yyyymmdd",
                "mmddyyyy",
                "yyyyddmm",
                "ddmmyyyy",
                "yyyy/mm/dd",
                "dd/mm/yyyy",
                "yyyy/dd/mm",
                "mm/dd/yyyy",
                "yyyy-mm-dd",
                "dd-mm-yyyy",
                "mm-dd-yyyy",
                "yyyy-dd-mm",
                @"dd\mm\yyyy",
                @"yyyy\mm\dd",
                @"yyyy\dd\mm",
                @"mm\dd\yyyy",
                "dd mm yyyy",
                "mm dd yyyy",
                "yyyy mm dd",
                "yyyy dd mm",
                "yyyy mm dd",
                "dd.mm.yyyy",
                "mm.dd.yyyy",
                "yyyy.mm.dd",
                "yyyy.dd.mm"
            };
            var d = new Dev2DateTimeParser();
            var count = 0;
            while (!result && count < acceptedDateFormats.Count)
            {
                result = d.TryParseDateTime(payload, acceptedDateFormats[count], out IDateTimeResultTO to, out string errorMsg);
                count++;
            }
            return result;
        }
        static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };

        public static bool IsXml(this string payload)
        {
            var result = false;
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            if (IsXml(payload, out bool isFragment))
            {
                result = true;
            }
            else
            {
                if (isFragment)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool IsJSON(this string payload)
        {
            var value = payload.TrimStart();
            value = value.TrimEnd();
            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {


                    JsonConvert.DeserializeObject(value);
                    return true;
                }


                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool IsXml(string data, out bool isFragment)
        {
            return IsXml(data, out isFragment, out bool isHtml) && !isFragment && !isHtml;
        }

        static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            var trimedData = data.Trim();
            var result = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

            isFragment = false;
            isHtml = false;

            if (result)
            {
                using (TextReader tr = new StringReader(trimedData))
                {
                    using (XmlReader reader = XmlReader.Create(tr, IsXmlReaderSettings))
                    {
                        try
                        {
                            reader.ReadToEnd(ref isFragment, ref isHtml, ref result);
                        }
                        catch (Exception)
                        {
                            tr.Close();
                            reader.Close();
                            isFragment = false;
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        static void ReadToEnd(this XmlReader reader, ref bool isFragment, ref bool isHtml, ref bool result)
        {
            long nodeCount = 0;
            while (reader.Read() && !isHtml && !isFragment && reader.NodeType != XmlNodeType.Document)
            {
                nodeCount++;

                if (reader.NodeType != XmlNodeType.CDATA)
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "html" && reader.Depth == 0)
                    {
                        isHtml = true;
                        result = false;
                    }

                    if (reader.NodeType == XmlNodeType.Element && nodeCount > 1 && reader.Depth == 0)
                    {
                        isFragment = true;
                    }
                }
            }
        }
        public static bool ContainsUnicodeCharacter(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            const int maxAnsiCode = 255;
            return input.Any(c => c > maxAnsiCode);
        }

        public static bool IsAlpha(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var result = IsAlphaRegex.IsMatch(payload);

            return result;
        }

        public static bool IsWholeNumber(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }
            return IsWholeNumber(payload, out int value);
        }

        public static bool IsWholeNumber(this string payload, out int value)
        {
            return int.TryParse(payload, out value) && value >= 0;
        }
        public static bool IsRealNumber(this string payload, out int value)
        {
            if (string.IsNullOrEmpty(payload))
            {
                value = 0;
                return false;
            }
            return int.TryParse(payload, out value);
        }

        public static bool IsNumeric(this string payload)
        {
            return IsNumeric(payload, out decimal value);
        }

        public static bool IsNumeric(this string payload, out decimal value)
        {
            if (string.IsNullOrEmpty(payload))
            {
                value = 0;
                return false;
            }

            var evalString = payload;

            if (payload[0] == '-')
            {
                evalString = payload.Substring(1, payload.Length - 1);
            }

            var current = CultureInfo.CurrentCulture.NumberFormat;
            if (evalString.Any(c => !char.IsDigit(c) && c != current.NumberDecimalSeparator[0]))
            {
                value = 0;
                return false;
            }

            return decimal.TryParse(payload, out value);
        }

        public static bool IsAlphaNumeric(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }
            return (IsAlpha(payload)  || IsAlphaNumericRegex.IsMatch(payload)) || IsNumeric(payload);
        }

        public static bool IsEmail(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var result = IsEmailRegex.IsMatch(payload);

            return result;
        }

        public static bool IsBinary(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }
            return IsBinaryField.IsMatch(payload);
        }

        public static bool IsBase64(this string payload)
        {
            var result = false;
            try
            {
                Convert.FromBase64String(payload);
                result = true;
            }
            catch (Exception)
            {
                // if error is thrown we know it is not a valid base64 string
            }

            return result;
        }

        public static bool IsHex(this string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }
            var result = IsHex1.IsMatch(payload) || IsHex2.IsMatch(payload);

            if (payload.Length % 2 != 0)
            {
                result = false;
            }
            return result;
        }

        public static string ReverseString(this string s)
        {
            var arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public static string RemoveWhiteSpace(this string value)
        {
            var cleanString = new StringBuilder(value.Trim()).Replace(" ", "");
            return cleanString.ToString();
        }

        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            var sb = new StringBuilder();
            foreach (char c in str)
            {
                if (!toExclude.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static bool SpaceCaseInsenstiveComparision(this string stringa, string stringb)
        {
            return stringa == null && stringb == null ||
                stringa != null &&
                stringa.ToLowerInvariant().ExceptChars(new[] { ' ', '\t', '\n', '\r' }).Equals(stringb.ToLowerInvariant().ExceptChars(new[] { ' ', '\t', '\n', '\r' }));
        }
    }
}