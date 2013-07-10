using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Wmhelp.XPath2;

namespace Dev2.Data.Parsers
{
    public class XPathParser
    {
//        public IEnumerable<string> ExecuteXPath(string xmlData, string xPath)
//        {
//            if(String.IsNullOrEmpty(xmlData)) throw new ArgumentNullException("xmlData");
//            if (String.IsNullOrEmpty(xPath)) throw new ArgumentNullException("xPath");
//            var processor = new Processor();
//            var newXPathCompiler = processor.NewXPathCompiler();
//            var xPathSelector = newXPathCompiler.Compile(xPath).Load();
//            xPathSelector.InputXmlResolver = null;
//            var stringReader = new StringReader(xmlData);
//            var reader2 = new XmlTextReader(stringReader) { XmlResolver = null };
//            var input = processor.NewDocumentBuilder().Build(reader2);
//            xPathSelector.ContextItem = input;
//            var xdmValue = xPathSelector.Evaluate();
//            var list = xdmValue.GetList().Cast<XdmItem>().Select(item => ((XdmNode)item).OuterXml).ToList();
//            return list;
//        }
        public IEnumerable<string> ExecuteXPath(string xmlData, string xPath)
        {
            if(String.IsNullOrEmpty(xmlData)) throw new ArgumentNullException("xmlData");
            if (String.IsNullOrEmpty(xPath)) throw new ArgumentNullException("xPath");
            try
            {
                bool isFragment;
                var isXml = DataListUtil.IsXml(xmlData,out isFragment);
                if(!isXml && !isFragment)
                {
                    throw new Exception("Input XML is not valid.");
                }
                var stringReader = new StringReader(xmlData);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                settings.DtdProcessing = DtdProcessing.Ignore;
                settings.ConformanceLevel =ConformanceLevel.Auto;
                XmlReader reader = XmlReader.Create(stringReader, settings);
                reader.Read();
                if(reader.NodeType == XmlNodeType.XmlDeclaration || reader.NodeType==XmlNodeType.Whitespace)
                {
                    reader.Skip();
                }
                if(xPath.StartsWith("/" + reader.Name) || xPath.StartsWith("//"+reader.Name))
                {
                    xPath = xPath.Replace("/" + reader.Name, "");
                }
                XNode xNode = XNode.ReadFrom(reader);
                IEnumerable<object> xdmValue = xNode.XPath2Select(xPath);
                var list = xdmValue.Select(element =>
                {
//                    if(!element.HasElements)
//                    {
//                        return element.Value.ToString();
//                    }
                    return element.ToString();
                }).ToList();
                return list;
            }
            catch(Exception exception)
            {
                if(exception.GetType() == typeof(XPath2Exception))
                {
                    throw new Exception("The XPath expression provided is not valid.");
                }
                ServerLogger.LogError(exception);
                throw;
            }
        }
    }
}