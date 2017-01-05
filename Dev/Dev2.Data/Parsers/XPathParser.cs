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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Util;
using org.xml.sax;
using Saxon.Api;
using Warewolf.Resource.Errors;
// ReSharper disable LoopCanBeConvertedToQuery

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
            if (string.IsNullOrEmpty(xmlData))
                throw new ArgumentNullException(nameof(xmlData));
            if (string.IsNullOrEmpty(xPath))
                throw new ArgumentNullException(nameof(xPath));
            try
            {
                bool isFragment;
                var useXmlData = DataListUtil.AdjustForEncodingIssues(xmlData);
                var isXml = DataListUtil.IsXml(useXmlData, out isFragment);

                if (!isXml && !isFragment)
                {
                    throw new Exception("Input XML is not valid.");
                }
                List<string> stringList;
                XmlDocument document = new XmlDocument();
                document.LoadXml(useXmlData);
                var namespaces = new List<KeyValuePair<string, string>>();
                if (document.DocumentElement != null)
                {
                    var xmlAttributeCollection = document.DocumentElement.Attributes;

                    foreach (XmlAttribute attrib in xmlAttributeCollection)
                    {
                        if (attrib?.NodeType == XmlNodeType.Attribute)
                        {
                            if (attrib.Name.Contains("xmlns:"))
                            {
                                var nsAttrib = attrib.Name.Split(':');
                                var ns = nsAttrib[1];
                                namespaces.Add(new KeyValuePair<string, string>(ns, attrib.Value));
                            }
                        }
                    }
                }
                using (TextReader stringReader = new StringReader(useXmlData))
                {
                    Processor processor = new Processor(false);
                    var compiler = processor.NewXPathCompiler();
                    compiler.XPathLanguageVersion = "3.0";
                    foreach (var keyValuePair in namespaces)
                    {
                        compiler.DeclareNamespace(keyValuePair.Key, keyValuePair.Value);
                    }
                    var xPathExecutable = compiler.Compile(xPath);
                    var xPathSelector = xPathExecutable.Load();
                    var newDocumentBuilder = processor.NewDocumentBuilder();
                    newDocumentBuilder.BaseUri = new Uri("http://warewolf.io");
                    newDocumentBuilder.WhitespacePolicy = WhitespacePolicy.StripAll;                    
                    var xdmNode = newDocumentBuilder.Build(stringReader);
                    xPathSelector.ContextItem = xdmNode;
                    var xdmValue = xPathSelector.Evaluate();
                    var list = xdmValue.GetEnumerator();

                    stringList = BuildListFromXPathResult(list);                    
                }
                return stringList;
            }
            catch (Exception exception)
            {
                if (exception.GetType() == typeof(SAXException))
                {
                    throw new Exception(ErrorResource.XPathProvidedNotValid);
                }

                Dev2Logger.Error(exception);
                throw;
            }
        }

        private static List<string> BuildListFromXPathResult(IEnumerator list)
        {
            List<string> stringList = new List<string>();
            while (list.MoveNext())
            {
                var current = list.Current;
                var realElm = current as XdmNode;
                if(realElm != null)
                {
                    if(realElm.NodeKind == XmlNodeType.Attribute)
                    {
                        stringList.Add(realElm.StringValue);
                    }
                    else if(realElm.NodeKind == XmlNodeType.Element)

                    {
                        var xElement = XElement.Parse(current.ToString());
                        stringList.Add(xElement.ToString());
                    }
                    else
                    {
                        stringList.Add(realElm.ToString());
                    }
                }
                else
                {
                    stringList.Add(current.ToString());
                }
            }
            return stringList;
        }
    }
}
