using System;
using System.Linq;
using Tu.Extensions;

namespace Tu.Rules
{
    public class Validator : IValidator
    {
        readonly IRegexUtilities _regexUtilities;

        public Validator(IRegexUtilities regexUtilities)
        {
            if(regexUtilities == null)
            {
                throw new ArgumentNullException("regexUtilities");
            }
            _regexUtilities = regexUtilities;
        }

        public bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public bool IsLengthEqual(string value, int length)
        {
            return (value == null ? 0 : value.Length) == length;
        }

        public bool IsLengthLessThanOrEqualTo(string value, int length)
        {
            return (value == null ? 0 : value.Length) <= length;
        }

        public bool IsLengthGreaterThan(string value, int length)
        {
            return (value == null ? 0 : value.Length) > length;
        }

        public bool IsNumeric(string value)
        {
            long result;
            return long.TryParse(value, out result);
        }

        public bool IsUpper(string value)
        {
            return value.Where(char.IsLetter).All(char.IsUpper);
        }

        public bool StartsWith(string value, string prefix)
        {
            return prefix != null && (value ?? string.Empty).StartsWith(prefix);
        }

        public bool IsCountOfLessThanOrEqualTo(string value, char c, int maxCount)
        {
            return Count(value, c) <= maxCount;
        }

        public bool ContainsSpecialChars(string value)
        {
            return !value.All(c => char.IsLetterOrDigit(c) || char.IsSeparator(c));
        }

        public bool IsValidEmailAddress(string value)
        {
            return _regexUtilities.IsValidEmail(value);
        }

        static int Count(string value, char c)
        {
            return value == null ? 0 : value.Count(v => v == c);
        }
    }
}