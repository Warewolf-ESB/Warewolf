using System;
using System.Text.RegularExpressions;

namespace Dev2.Common.ExtMethods
{
    public static class StringExtension
    {
        private static Regex _isAlphaRegex = new Regex("^[a-zA-Z ]*$", RegexOptions.Compiled);
        //private static Regex _isNumericRegex = new Regex("^[0-9]*$", RegexOptions.Compiled);
        private static Regex _isAlphaNumericRegex = new Regex("^[0-9a-zA-Z]*$", RegexOptions.Compiled);
        private static Regex _isEmailRegex = new Regex(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsAlpha(this string payload)
        {
            bool result = false;
            result = _isAlphaRegex.IsMatch(payload);

            return result;
        }

        public static bool IsWholeNumber(this string payload)
        {
            int value;
            return IsWholeNumber(payload, out value);
        }

        public static bool IsWholeNumber(this string payload, out int value)
        {
            return int.TryParse(payload, out value);
        }

        public static bool IsNumeric(this string payload)
        {
            //2013.01.16, brendon.page, Converted this method to use decimal.tryparse instead of a regex expression.
            //                          This change was made because it is fatser and to add support for decimal points.
            //                          IsWholeNumber has been added for instances where a whole number is required.
            //                          IsAlphaNumeric has been updated to use IsWholeNumber.
            decimal value;
            return IsNumeric(payload, out value);
        }

        public static bool IsNumeric(this string payload, out decimal value)
        {
            return decimal.TryParse(payload, out value);
        }

        public static bool IsAlphaNumeric(this string payload)
        {
            return (IsAlpha(payload) || IsNumeric(payload) || _isAlphaNumericRegex.IsMatch(payload));

            //return (!IsAlpha(payload) && !IsWholeNumber(payload) && _isAlphaNumericRegex.IsMatch(payload));
        }

        public static bool IsEmail(this string payload)
        {
            bool result = false;
            result = _isEmailRegex.IsMatch(payload);

            return result;
        }

        public static string ReverseString(this string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }


    }
}
