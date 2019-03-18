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
using System.IO;
using System.Linq;
using System.Xml;

namespace Dev2.Data.Util
{
    public static class XmlHelper
    {
        static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };
        static readonly string[] StripTags = { "<XmlData>", "</XmlData>", "<Dev2ServiceInput>", "</Dev2ServiceInput>", "<sr>", "</sr>", "<ADL />" };
        static readonly string[] NaughtyTags = { "<Dev2ResumeData>", "</Dev2ResumeData>",
                                                 "<Dev2XMLResult>", "</Dev2XMLResult>",
                                                 "<WebXMLConfiguration>", "</WebXMLConfiguration>",
                                                 "<ActivityInput>", "</ActivityInput>",
                                                 "<ADL>","</ADL>",
                                                 "<DL>","</DL>"
                                               };
        const string AdlRoot = "ADL";

        public static bool IsXml(string data, out bool isFragment, out bool isHtml)
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
                        TryProcessAllNodes(ref isFragment, ref isHtml, ref result, tr, reader);
                    }
                }
            }

            return result;
        }

        private static void TryProcessAllNodes(ref bool isFragment, ref bool isHtml, ref bool result, TextReader tr, XmlReader reader)
        {
            try
            {
                long nodeCount = 0;
                while (reader.Read() && !isHtml && !isFragment && reader.NodeType != XmlNodeType.Document)
                {
                    nodeCount++;
                    IsHtmlOrFragment(ref isFragment, ref isHtml, ref result, reader, nodeCount);
                }
            }
            catch (Exception)
            {
                tr.Close();
                reader.Close();
                isFragment = false;
                result = false;
            }
        }

        private static void IsHtmlOrFragment(ref bool isFragment, ref bool isHtml, ref bool result, XmlReader reader, long nodeCount)
        {
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

        public static string ToCleanXml(this string payload)
        {
            var result = payload;
            var veryNaughtyTags = NaughtyTags;

            if (!string.IsNullOrEmpty(payload))
            {
                StripTags?.ToList()
                    .ForEach(tag =>
                    {
                        result = result.Replace(tag, "");
                    });

                if (veryNaughtyTags != null)
                {
                    result = TryCleanupNaughtyTags(veryNaughtyTags, result);
                }
                var start = result.IndexOf("<", StringComparison.Ordinal);
                if (start >= 0)
                {
                    result = result.Substring(start);
                }

                if (result.Contains("<") && result.Contains(">"))
                {
                    var isXml = IsXml(result, out bool isFragment, out bool isHtml);
                    if (!(isXml && !isFragment && !isHtml))
                    {
                        result = result.Replace("<DataList>", "").Replace("</DataList>", "");
                        result = result.Replace(string.Concat("<", AdlRoot, ">"), string.Empty).Replace(string.Concat("</", AdlRoot, ">"), "");
                        result = string.Concat("<", AdlRoot, ">", result, "</", AdlRoot, ">");
                    }
                }

            }

            return result;
        }

        static string TryCleanupNaughtyTags(string[] toRemove, string payload)
        {
            var foundOpen = false;
            var result = payload;

            for (int i = 0; i < toRemove.Length; i++)
            {
                var myTag = toRemove[i];
                if (myTag.IndexOf("<", StringComparison.Ordinal) >= 0 && myTag.IndexOf("</", StringComparison.Ordinal) < 0)
                {
                    foundOpen = true;
                }
                else
                {
                    if (myTag.IndexOf("</", StringComparison.Ordinal) >= 0)
                    {
                        result = CloseOpenTag(toRemove, foundOpen, result, i, myTag);

                        foundOpen = false;
                    }
                }
            }
            return result.Trim();
        }

        private static string CloseOpenTag(string[] toRemove, bool foundOpen, string result, int i, string myTag)
        {
            var _result = result;
            if (foundOpen)
            {
                var loc = i - 1;
                if (loc >= 0)
                {
                    var start = result.IndexOf(toRemove[loc], StringComparison.Ordinal);
                    var end = result.IndexOf(myTag, StringComparison.Ordinal);
                    if (start < end && start >= 0)
                    {
                        var canidate = result.Substring(start, end - start + myTag.Length);
                        var tmpResult = canidate.Replace(myTag, "").Replace(toRemove[loc], "");
                        _result = tmpResult.IndexOf("</", StringComparison.Ordinal) >= 0 || tmpResult.IndexOf("/>", StringComparison.Ordinal) >= 0 ? result.Replace(myTag, "").Replace(toRemove[loc], "") : result.Replace(canidate, "");
                    }
                }
            }
            else
            {
                _result = result.Replace(myTag, "");
            }
            return _result;
        }
    }
}
