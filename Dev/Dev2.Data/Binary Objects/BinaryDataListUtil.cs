using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

namespace Dev2.Data.Binary_Objects
{
    public class BinaryDataListUtil
    {

        public string SerializeDeferredItem(object item)
        {
            
            using(MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formater = new BinaryFormatter();
                formater.Serialize(ms, item);
                return Convert.ToBase64String(ms.ToArray());
            }

        }

        public T DeserializeDeferredItem<T>(string item)
        {
           
            using(MemoryStream ms = new MemoryStream(Convert.FromBase64String(item)))
            {
                BinaryFormatter formater = new BinaryFormatter();

                return (T)formater.Deserialize(ms);
            }

        }
    }
}
