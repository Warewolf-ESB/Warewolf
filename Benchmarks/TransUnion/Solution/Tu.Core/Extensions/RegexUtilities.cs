using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Tu.Extensions
{
    public class RegexUtilities : IRegexUtilities
    {
        bool _invalid;

        public bool IsValidEmail(string strIn)
        {
            _invalid = false;
            if(String.IsNullOrEmpty(strIn))
            {
                return false;
            }

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch(RegexMatchTimeoutException)
            {
                return false;
            }

            if(_invalid)
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch(RegexMatchTimeoutException)
            {
                return false;
            }
        }

        string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            var idn = new IdnMapping();

            var domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch(ArgumentException)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
