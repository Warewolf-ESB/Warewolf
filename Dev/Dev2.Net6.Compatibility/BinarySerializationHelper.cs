using KGySoft.Serialization.Binary;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.Net6.Compatibility
{
    public class BinarySerializationHelper
    {
        
        public System.Collections.Concurrent.ConcurrentDictionary<string, Guid> DeserializeFile(string binarySerializedFile)
        {
            var localDictionary = new System.Collections.Concurrent.ConcurrentDictionary<string, Guid>();

            try
            {
                using (var streamSerializedInNetFramework = new FileStream(binarySerializedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // deserializing a type, whose fields used to have "m_" prefix, which have been removed
                    var surrogate = new CustomSerializerSurrogateSelector();
                    surrogate.Deserializing += (sender, args) =>
                    {
                        if (args.Object is System.Collections.Concurrent.ConcurrentDictionary<string, Guid>)
                        {
                            if (args.SerializationInfo != null)
                            {
                                var entry = args.SerializationInfo.ToEnumerable().FirstOrDefault(c => c.Name == "m_serializationArray");
                                if (entry.Name == "m_serializationArray")
                                {
                                    localDictionary = new System.Collections.Concurrent.ConcurrentDictionary<string, Guid>();
                                    var keyValuePairs = (KeyValuePair<string, Guid>[])entry.Value;

                                    foreach (var pair in keyValuePairs)
                                    {
                                        localDictionary.TryAdd(pair.Key, pair.Value);
                                    }

                                    args.Handled = true;
                                    return;
                                }
                            }
                        }
                        return;
                    };

                    var formatter = new BinaryFormatter // or a BinarySerializationFormatter
                    {
                        SurrogateSelector = surrogate, // to remap field names as specified above
                        Binder = new WeakAssemblySerializationBinder() // if assembly version changed, too
                    };

                    var retValue = formatter.Deserialize(streamSerializedInNetFramework);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return localDictionary;
        }
    }
}
