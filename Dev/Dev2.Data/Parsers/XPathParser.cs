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


namespace Dev2.Data.Parsers
{
    public class XPathParser
    {
        public IEnumerable<string> ExecuteXPath(string xmlData, string xPath)
        {
            if (string.IsNullOrEmpty(xmlData))
            {
                throw new ArgumentNullException(nameof(xmlData));
            }

            if (string.IsNullOrEmpty(xPath))
            {
                throw new ArgumentNullException(nameof(xPath));
            }

            try
            {
                var useXmlData = DataListUtil.AdjustForEncodingIssues(xmlData);
                var isXml = DataListUtil.IsXml(useXmlData, out bool isFragment);

                if (!isXml && !isFragment)
                {
                    throw new Exception("Input XML is not valid.");
                }
                List<string> stringList;
                var document = new XmlDocument();
                document.LoadXml(useXmlData);
                var namespaces = new List<KeyValuePair<string, string>>();
                if (document.DocumentElement != null)
                {
                    namespaces = AddAttributesAsNamespaces(document, namespaces);
                }
                using (TextReader stringReader = new StringReader(useXmlData))
                {
                    var processor = new Processor(false);
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

                Dev2Logger.Error(exception, GlobalConstants.WarewolfError);
                throw;
            }
        }

        static List<KeyValuePair<string, string>> AddAttributesAsNamespaces(XmlDocument document, List<KeyValuePair<string, string>> namespaces)
        {
            var xmlAttributeCollection = document.DocumentElement.Attributes;
            foreach (XmlAttribute attrib in xmlAttributeCollection)
            {
                if (attrib?.NodeType == XmlNodeType.Attribute && attrib.Name.Contains("xmlns:"))
                {
                    var nsAttrib = attrib.Name.Split(':');
                    var ns = nsAttrib[1];
                    namespaces.Add(new KeyValuePair<string, string>(ns, attrib.Value));
                }
            }
            return namespaces;
        }

        static List<string> BuildListFromXPathResult(IEnumerator list)
        {
            var stringList = new List<string>();
            while (list.MoveNext())
            {
                var current = list.Current;
                if (current is XdmNode realElm)
                {
                    if (realElm.NodeKind == XmlNodeType.Attribute)
                    {
                        stringList.Add(realElm.StringValue);
                    }
                    else if (realElm.NodeKind == XmlNodeType.Element)

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
