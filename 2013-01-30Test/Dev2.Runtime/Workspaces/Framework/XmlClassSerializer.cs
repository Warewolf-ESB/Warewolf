using System.IO;
using System.Text;
using System.Xml.Serialization;

// ReSharper disable CheckNamespace
namespace Dev2.Serialization
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Serializes and deserializes objects into and from XML documents. 
    /// </summary>
    public static class XmlClassSerializer
    {
        /// <summary>
        /// Serializes the specified object to xml.
        /// </summary>
        /// <typeparam name="T">The type of the object to be serialized.</typeparam>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>The XML document.</returns>
        public static string Serialize<T>(T obj)
        {
            var type = obj.GetType();

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var s = new XmlSerializer(type);
                s.Serialize(writer, obj);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the specified XML document.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized.</typeparam>
        /// <param name="xml">The XML document to deserialize.</param>
        /// <returns>The object instance.</returns>
        public static T Deserialize<T>(string xml)
        {
            var type = typeof(T);

            using (var reader = new StringReader(xml))
            {
                var s = new XmlSerializer(type);
                return (T)s.Deserialize(reader);
            }
        }
    }
}
