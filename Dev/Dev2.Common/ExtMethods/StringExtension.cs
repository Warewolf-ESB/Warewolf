using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace Dev2.Common.ExtMethods
{
    /// <summary>
    /// Useful utilities
    /// </summary>
    public static class StringExtension
    {
        private static readonly Regex _isAlphaRegex = new Regex("^[a-zA-Z ]*$", RegexOptions.Compiled);
        private static readonly Regex _isAlphaNumericRegex = new Regex("^[0-9a-zA-Z]*$", RegexOptions.Compiled);
        private static readonly Regex _isEmailRegex = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _isBinary = new Regex("^[01]+$");
        public static Regex IsValidCategoryname = new Regex(@"[^a-zA-Z0-9._\s-]+$");
        public static Regex IsValidResourcename = new Regex(@"[^a-zA-Z0-9._\s-]+$");
        static readonly Regex _isHex1 = new Regex(@"\A\b[0-9a-fA-F]+\b\Z");
        static readonly Regex _isHex2 = new Regex(@"\A\b(0[xX])?[0-9a-fA-F]+\b\Z");

        public static string Escape(this string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string Unescape(this string payload)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(payload);
                return doc.InnerXml;
            }
            catch(Exception)
            {
                string xml = string.Format("<dummycake>{0}</dummycake>", payload);
                doc.LoadXml(xml);

            }
            if(doc.DocumentElement != null)
            {
                return doc.DocumentElement.InnerText;
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether the specified payload is alpha.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is alpha; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlpha(this string payload)
        {
            if(string.IsNullOrEmpty(payload))
            {
                return false;
            }

            bool result = _isAlphaRegex.IsMatch(payload);

            return result;
        }

        /// <summary>
        /// Determines whether [is whole number] [the specified payload].
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if [is whole number] [the specified payload]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWholeNumber(this string payload)
        {
            int value;
            return IsWholeNumber(payload, out value);
        }

        /// <summary>
        /// Determines whether [is whole number] [the specified payload].
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is whole number] [the specified payload]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWholeNumber(this string payload, out int value)
        {
            if(int.TryParse(payload, out value))
            {
                if(value >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is real number] [the specified payload].
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if [is real number] [the specified payload]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRealNumber(this string payload)
        {
            int value;
            return IsRealNumber(payload, out value);
        }

        /// <summary>
        /// Determines whether [is real number] [the specified payload].
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is real number] [the specified payload]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRealNumber(this string payload, out int value)
        {
            return int.TryParse(payload, out value);
        }

        /// <summary>
        /// Determines whether the specified payload is numeric.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(this string payload)
        {
            //2013.01.16, brendon.page, Converted this method to use decimal.tryparse instead of a regex expression.
            //                          This change was made because it is fatser and to add support for decimal points.
            //                          IsWholeNumber has been added for instances where a whole number is required.
            //                          IsAlphaNumeric has been updated to use IsWholeNumber.
            decimal value;
            return IsNumeric(payload, out value);
        }

        /// <summary>
        /// Determines whether the specified payload is numeric.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(this string payload, out decimal value)
        {
            return decimal.TryParse(payload, out value);
        }

        /// <summary>
        /// Determines whether [is alpha numeric] [the specified payload].
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if [is alpha numeric] [the specified payload]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlphaNumeric(this string payload)
        {
            return (!string.IsNullOrEmpty(payload) && (IsAlpha(payload) || IsNumeric(payload) || _isAlphaNumericRegex.IsMatch(payload)));
        }

        /// <summary>
        /// Determines whether the specified payload is email.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is email; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmail(this string payload)
        {
            if(string.IsNullOrEmpty(payload))
            {
                return false;
            }

            bool result = _isEmailRegex.IsMatch(payload);

            return result;
        }


        /// <summary>
        /// Determines whether the specified payload is binary.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is binary; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBinary(this string payload)
        {
            return _isBinary.IsMatch(payload);
        }

        /// <summary>
        /// Determines whether the specified payload is base64.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is base64; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBase64(this string payload)
        {
            bool result = false;
            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                Convert.FromBase64String(payload);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                result = true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                // if error is thrown we know it is not a valid base64 string
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified payload is hex.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is hex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsHex(this string payload)
        {

            bool result = (_isHex1.IsMatch(payload) || _isHex2.IsMatch(payload));

            if((payload.Length % 2) != 0)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Reverses the string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static string ReverseString(this string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// Determines whether the specified payload is a valid resource category name.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is a valid resource category name; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidCategoryName(this string payload)
        {
            return !IsValidCategoryname.IsMatch(payload);
        }

        /// <summary>
        /// Keyboard Accellerators are used in Windows to allow easy shortcuts to controls like Buttons and 
        /// MenuItems. These allow users to press the Alt key, and a shortcut key will be highlighted on the 
        /// control. If the user presses that key, that control will be activated.
        /// This method checks a string if it contains a keyboard accellerator. If it doesn't, it adds one to the
        /// beginning of the string. If there are two strings with the same accellerator, Windows handles it.
        /// The keyboard accellerator character for WPF is underscore (_). It will not be visible.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TryAddKeyboardAccellerator(this string input)
        {
            const string accellerator = "_";            // This is the default WPF accellerator symbol - used to be & in WinForms

            // If it already contains an accellerator, do nothing
            if(input.Contains(accellerator)) return input;

            return accellerator + input;
        }
    }
}
