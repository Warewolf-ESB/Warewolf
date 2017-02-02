/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        private static readonly XmlReaderSettings IsXmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto, DtdProcessing = DtdProcessing.Ignore };
        private static readonly string[] StripTags = { "<XmlData>", "</XmlData>", "<Dev2ServiceInput>", "</Dev2ServiceInput>", "<sr>", "</sr>", "<ADL />" };
        private static readonly string[] NaughtyTags = { "<Dev2ResumeData>", "</Dev2ResumeData>",
                                                         "<Dev2XMLResult>", "</Dev2XMLResult>",
                                                         "<WebXMLConfiguration>", "</WebXMLConfiguration>",
                                                         "<ActivityInput>", "</ActivityInput>",
                                                         "<ADL>","</ADL>",
                                                         "<DL>","</DL>"
                                                       };
        const string AdlRoot = "ADL";
        /// <summary>
        /// Checks if the info contained in data is well formed XML
        /// </summary>
        public static bool IsXml(string data, out bool isFragment, out bool isHtml)
        {
            string trimedData = data.Trim();
            bool result = trimedData.StartsWith("<") && !trimedData.StartsWith("<![CDATA[");

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

        public static string ToCleanXml(this string payload)
        {
            string result = payload;
            string[] veryNaughtyTags = NaughtyTags;

            if (!string.IsNullOrEmpty(payload))
            {
                StripTags?.ToList()
                    .ForEach(tag =>
                    {
                        result = result.Replace(tag, "");
                    });

                if (veryNaughtyTags != null)
                {
                    result = CleanupNaughtyTags(veryNaughtyTags, result);
                }

                // we now need to remove non-valid chars from the stream

                int start = result.IndexOf("<", StringComparison.Ordinal);
                if (start >= 0)
                {
                    result = result.Substring(start);
                }

                if (result.Contains("<") && result.Contains(">"))
                {
                    bool isFragment;
                    bool isHtml;
                    var isXml = IsXml(result, out isFragment, out isHtml);
                    if (!(isXml && !isFragment && !isHtml))
                    {
                        // We need to replace DataList if present ;)
                        result = result.Replace("<DataList>", "").Replace("</DataList>", "");
                        result = result.Replace(string.Concat("<", AdlRoot, ">"), string.Empty).Replace(string.Concat("</", AdlRoot, ">"), "");
                        result = string.Concat("<", AdlRoot, ">", result, "</", AdlRoot, ">");
                    }
                }


            }

            return result;
        }

        /// <summary>
        /// Cleanups the naughty tags.
        /// </summary>
        /// <param name="toRemove">To remove.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        private static string CleanupNaughtyTags(string[] toRemove, string payload)
        {
            bool foundOpen = false;
            string result = payload;

            for (int i = 0; i < toRemove.Length; i++)
            {
                string myTag = toRemove[i];
                if (myTag.IndexOf("<", StringComparison.Ordinal) >= 0 && myTag.IndexOf("</", StringComparison.Ordinal) < 0)
                {
                    foundOpen = true;
                }
                else if (myTag.IndexOf("</", StringComparison.Ordinal) >= 0)
                {
                    // close tag
                    if (foundOpen)
                    {
                        // remove data between
                        int loc = i - 1;
                        if (loc >= 0)
                        {
                            int start = result.IndexOf(toRemove[loc], StringComparison.Ordinal);
                            int end = result.IndexOf(myTag, StringComparison.Ordinal);
                            if (start < end && start >= 0)
                            {
                                string canidate = result.Substring(start, end - start + myTag.Length);
                                string tmpResult = canidate.Replace(myTag, "").Replace(toRemove[loc], "");
                                if (tmpResult.IndexOf("</", StringComparison.Ordinal) >= 0 || tmpResult.IndexOf("/>", StringComparison.Ordinal) >= 0)
                                {
                                    // replace just the tags
                                    result = result.Replace(myTag, "").Replace(toRemove[loc], "");
                                }
                                else
                                {
                                    // replace any tag and it's contents as long as it is not XML in side
                                    result = result.Replace(canidate, "");
                                }
                            }
                        }
                    }
                    else
                    {
                        result = result.Replace(myTag, "");
                    }

                    foundOpen = false;
                }
            }

            return result.Trim();

        }
    }
}
