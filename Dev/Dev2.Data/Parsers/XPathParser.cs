
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Util;
using Wmhelp.XPath2;

namespace Dev2.Data.Parsers
{
    /// <summary>
    /// XPath Parser
    /// </summary>
    public class XPathParser
    {
        /// <summary>
        /// Executes the X path.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <param name="xPath">The x path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// xmlData
        /// or
        /// xPath
        /// </exception>
        /// <exception cref="System.Exception">
        /// Input XML is not valid.
        /// or
        /// The XPath expression provided is not valid.
        /// </exception>
        public IEnumerable<string> ExecuteXPath(string xmlData, string xPath)
        {
            if(String.IsNullOrEmpty(xmlData)) throw new ArgumentNullException("xmlData");
            if (String.IsNullOrEmpty(xPath)) throw new ArgumentNullException("xPath");
            try
            {
                bool isFragment;
                var useXmlData = DataListUtil.AdjustForEncodingIssues(xmlData);
                var isXml = DataListUtil.IsXml(useXmlData, out isFragment);
                
                if(!isXml && !isFragment)
                {
                    throw new Exception("Input XML is not valid.");
                }

                using(TextReader stringReader = new StringReader(useXmlData))
                {
                    XmlReaderSettings settings = new XmlReaderSettings
                    {
                        IgnoreWhitespace = true,
                        DtdProcessing = DtdProcessing.Ignore,
                        ConformanceLevel = ConformanceLevel.Auto
                    };

                    using (XmlTextReader xtr = new XmlTextReader(stringReader))
                    {
                        xtr.Namespaces = false;

                        using (XmlReader reader = XmlReader.Create(xtr, settings))
                        {
                            reader.Read();

                            if (reader.NodeType == XmlNodeType.XmlDeclaration || reader.NodeType == XmlNodeType.Whitespace)
                            {
                                reader.Skip();
                                // handle DocumentType nodes
                                if (reader.NodeType == XmlNodeType.DocumentType)
                                {
                                    reader.Skip();
                                }
                                // skip white space ;)
                                while(reader.Value.IndexOf("\n", StringComparison.Ordinal) >= 0)
                                {
                                    reader.Skip();
                                }
                            }

                            if (xPath.StartsWith("/" + reader.Name) || xPath.StartsWith("//" + reader.Name))
                            {
                                xPath = xPath.Replace("/" + reader.Name, "");
                            }
                            else if(xPath.StartsWith(reader.Name))
                            {
                                xPath = xPath.Replace(reader.Name, "/");
                            }

                            XNode xNode = XNode.ReadFrom(reader);
                            IEnumerable<object> xdmValue = xNode.XPath2Select(xPath);
                            var list = xdmValue.Select(element =>
                                {
                                    var realElm = element as XObject;
                                if(realElm != null && realElm.NodeType == XmlNodeType.Attribute)
                                    {
                                        var xAttribute = realElm as XAttribute;
                                    if(xAttribute != null)
                                        {
                                            return xAttribute.Value;
                                        }
                                    }

                                    return element.ToString();

                                }).ToList();
                            return list;
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                if(exception.GetType() == typeof(XPath2Exception))
                {
                    throw new Exception("The XPath expression provided is not valid.");
                }

                Dev2Logger.Log.Error(exception);
                throw;
            }
        }
    }
}
