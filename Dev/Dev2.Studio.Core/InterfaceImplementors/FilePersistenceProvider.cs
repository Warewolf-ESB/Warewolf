using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Dev2.Studio.Core {
    [Export(typeof(IFilePersistenceProvider))]
    public class FilePersistenceProvider : IFilePersistenceProvider {
        public void Write(string containerName, string data) {
            File.WriteAllText(containerName, data);
        }

        public void Delete(string containerName) {
            File.Delete(containerName);
        }

        public string Read(string containerName) {
            return File.ReadAllText(containerName);
        }

        public static string Serialize<T>(T obj)
        {
            var type = obj.GetType();

            var sb = new StringBuilder();
            using(var writer = new StringWriter(sb))
            {
                var s = new XmlSerializer(type);
                s.Serialize(writer, obj);
            }
            return sb.ToString();
        }

        public static T Deserialize<T>(string xml)
        {
            var type = typeof(T);

            using (var reader = new StringReader(xml))
            {
                var s = new XmlSerializer(type);
                return (T) s.Deserialize(reader);
            }
        }

        public static string SerializeBinary(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);

            ms.Seek(0, SeekOrigin.Begin);
            TextReader tr = new StreamReader(ms);
            return tr.ReadToEnd();
        }

        public static T DeserializeBinary<T>(string data)
        {
            MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(data));
            BinaryFormatter formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
        }
    }
}
