#if !NET9_0_OR_GREATER
using KGySoft.Serialization.Binary;
using System.Linq.Expressions;

#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
using System.Runtime.Serialization.Formatters.Binary;
#pragma warning restore SYSLIB0011
#endif

namespace Dev2.Net6.Compatibility
{
    /// <summary>
    /// BACKWARD COMPATIBILITY ONLY: This class uses BinaryFormatter to deserialize files
    /// that were serialized in .NET Framework. BinaryFormatter is deprecated in .NET 8 and
    /// fully removed starting with .NET 9, so the real deserialization path below is only
    /// compiled for TargetFrameworks up to net8.0 (see #if !NET9_0_OR_GREATER).
    /// DO NOT use this class for new serialization - use DataContractSerializer instead.
    /// This class should only be used as a fallback to read legacy data.
    /// </summary>
    public class BinarySerializationHelper
    {

        /// <summary>
        /// Deserializes a file that was previously serialized using BinaryFormatter in .NET Framework.
        /// WARNING: This method uses the obsolete BinaryFormatter and should only be used for
        /// backward compatibility to read legacy files.
        /// </summary>
        /// <remarks>
        /// On net9.0+ (where BinaryFormatter no longer exists) this always returns null. Callers
        /// already treat a null/failed result as "no legacy data available" and fall back to
        /// creating a fresh empty map, so this degrades gracefully. Environments that still hold
        /// files in this legacy format must be run on a net8.0 (or earlier) build at least once
        /// before upgrading to net9.0+ — that run rewrites the data via the modern MessagePack
        /// format (see WorkspaceRepository.WriteUserMap), after which this legacy path is no
        /// longer needed for that environment.
        /// </remarks>
        public System.Collections.Concurrent.ConcurrentDictionary<string, Guid> DeserializeFile(string binarySerializedFile)
        {
#if NET9_0_OR_GREATER
            return null;
#else
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

                    // Using BinaryFormatter for backward compatibility only
                    // This is required to read files serialized in .NET Framework
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
                    var formatter = new BinaryFormatter // or a BinarySerializationFormatter
                    {
                        SurrogateSelector = surrogate, // to remap field names as specified above
                        Binder = new WeakAssemblySerializationBinder() // if assembly version changed, too
                    };

                    var retValue = formatter.Deserialize(streamSerializedInNetFramework);
#pragma warning restore SYSLIB0011
                }
            }
            catch (Exception)
            {
                return null;
            }
            return localDictionary;
#endif
        }
    }
}
