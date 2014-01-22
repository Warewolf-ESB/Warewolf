
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;

namespace Dev2.Data.Util
{
    // BUG 9519/9520 - 2013.05.29 - TWR - Created
    public static class Scrubber
    {
        // Compiled regex are always faster ;)
        private static readonly Regex XmlRegex = new Regex(string.Format("({0}).*?({1})", Regex.Escape("<?"), Regex.Escape("?>")));

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
            if(text.StartsWith("[{"))
            {
                text = "{UnnamedArrayData:" + text + "}";
            }
            return text;
        }

        #endregion

        #region ScrubXml

        static string ScrubXml(string text)
        {
            // Remove all instances of xml declaration:
            //
            //    <?xml version="1.0" encoding="utf-8"?>
            //
            // as this may be in weird places!
            // Regex should be: (\\Q<?\\E).*?(\\Q?>\\E) 
            //

            var result = XmlRegex.Replace(text, "");

            try
            {
                result = RemoveAllNamespaces(XElement.Parse(result)).ToString();
            }
            catch(Exception ex)
            {
                ServerLogger.LogError("Scrubber", ex);
            }

            return result;
        }

        #endregion

        #region RemoveAllNamespaces

        static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            var xElement = !xmlDocument.HasElements
                ? new XElement(xmlDocument.Name.LocalName) { Value = xmlDocument.Value }
                : new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(RemoveAllNamespaces));

            AddAttributes(xmlDocument, xElement);
            return xElement;
        }

        static void AddAttributes(XElement source, XContainer target)
        {
            foreach(var attribute in source.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration))
            {
                target.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
            }
        }

        #endregion

    }
}