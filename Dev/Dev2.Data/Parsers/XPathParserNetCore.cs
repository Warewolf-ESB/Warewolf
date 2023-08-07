#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Warewolf.Resource.Errors;
namespace Dev2.Data.Parsers
{
    public class XPathParserNetCore
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

                // Create a namespace manager if needed
                var namespaceManager = new XmlNamespaceManager(document.NameTable);
                foreach (var keyValuePair in namespaces)
                {
                    namespaceManager.AddNamespace(keyValuePair.Key, keyValuePair.Value);
                }

                // Create the XmlNavigator from the XmlDocument
                var navigator = document.CreateNavigator();
                object expressionValue;

                var xpathExpression = navigator.Compile(xPath);
                xpathExpression.SetContext(namespaceManager);
                expressionValue = navigator.Evaluate(xpathExpression);
                stringList = BuildListFromXPathResultGeneric(expressionValue);
                return stringList;
            }
            catch (Exception exception)
            {
                if (exception.GetType() == typeof(XPathException))
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

        static List<string> BuildListFromXPathResultGeneric(object list)
        {
            if (list is XPathNodeIterator nodeIterator)
                return BuildListFromXPathResult(nodeIterator);

            string value;

            if (list is Boolean)
                value = list.ToString().ToLowerInvariant();
            else
                value = list.ToString();

            return new List<string>() { value };

        }

        static List<string> BuildListFromXPathResult(XPathNodeIterator list)
        {
            var stringList = new List<string>();
            while (list.MoveNext())
            {
                var current = list.Current;
                if (current != null && current.IsNode)
                {
                    var realElm = current;
                    if (realElm.NodeType == XPathNodeType.Attribute)
                    {
                        stringList.Add(realElm.Value);
                    }
                    else if (realElm.NodeType == XPathNodeType.Element)
                    {
                        var xElement = XElement.Parse(current.OuterXml);
                        stringList.Add(xElement.ToString());
                    }
                    else
                    {
                        stringList.Add(realElm.OuterXml);
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
