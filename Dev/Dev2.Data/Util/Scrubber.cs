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
        static readonly Regex XmlRegex = new Regex(string.Format("({0}).*?({1})", Regex.Escape("<?"), Regex.Escape("?>")));

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
            switch (scrubType)
            {
                case ScrubType.Xml:
                    return ScrubXml(text);
                case ScrubType.JSon:
                    return ScrubJson(text);
                default:
                    break;
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
                Dev2Logger.Error("Scrubber", ex, GlobalConstants.WarewolfError);
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
