using System.Xml;

namespace Dev2.Integration.Tests.MEF {
    public class DataListValueInjector {
        public string InjectDataListValue(string xmlString, string nameOfNode, string innerTextToInject) {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlString);
            xDoc.SelectSingleNode("//" + nameOfNode).InnerText = innerTextToInject;
            return xDoc.OuterXml.Replace("&gt;", ">").Replace("&lt;", "<");
        }
    }
}
