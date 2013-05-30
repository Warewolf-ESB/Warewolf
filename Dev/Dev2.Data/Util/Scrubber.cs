
using System.Text.RegularExpressions;

namespace Dev2.Data.Util
{
    // BUG 9519/9520 - 2013.05.29 - TWR - Created
    public static class Scrubber
    {
        #region Scrub

        public static string Scrub(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            text = text.Trim();
            var scrubType = text.StartsWith("<") ? ScrubType.Xml : ScrubType.JSon;
            return Scrub(text, scrubType);
        }

        public static string Scrub(string text, ScrubType scrubType)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
            switch(scrubType)
            {
                case ScrubType.Xml:
                    return ScrubXml(text);
                case ScrubType.JSon:
                    return ScrubJson(text);
            }
            return text;
        }

        #endregion

        #region ScrubJson

        static string ScrubJson(string text)
        {
            return text;
        }

        #endregion

        #region ScrubXml

        static string ScrubXml(string text)
        {
            // HACK: Remove strings that cause issues with the data mapper

            var regex = new Regex("\\sxmlns[^\"]+\"[^\"]+\""); // e.g. xmlns="http://www.webservice.net"
            text = regex.Replace(text, "");

            regex = new Regex("(<\\?).*(\\?>)"); // e.g. <?xml version="1.0" encoding="utf-8"?>
            text = regex.Replace(text, "");

            return text;
        }

        #endregion

    }
}