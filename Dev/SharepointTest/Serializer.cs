using System.Configuration;
using System.Xml;
using Newtonsoft.Json;

namespace Warewolf.SharePoint
{
    public class Serializer
    {

        public Serialized JSONtoXML(string json)
        {

            Serialized serialized = new Serialized();
            XmlDocument doc = JsonConvert.DeserializeXmlNode(json);
            serialized.Data = doc.InnerXml;
            return serialized;

        }

        /// <summary>
        /// Serializes 
        /// </summary> 
        /// <param name="xml"></param>
        /// <returns></returns>
        public Serialized XMLtoJSON(string xml)
        {
            Serialized serialized = new Serialized();
            XmlNode node = new ConfigXmlDocument();
            node.InnerXml = xml;
            var doc = JsonConvert.SerializeXmlNode(node);

            serialized.Data = doc;
            return serialized;
            
        }

    }

    public class Serialized
    {
        public string Data { get; set; }

    }
}
