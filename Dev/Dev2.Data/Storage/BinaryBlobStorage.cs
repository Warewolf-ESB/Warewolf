using Dev2.Data.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;
using Dev2.Data.Storge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// The api for storing binary data ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryBlobStorage : IDev2PersistedDictionaryStorage, IDisposable
    {

        readonly Dev2PersistantDictionary<byte[]> BlobCache;
        readonly ProtocolBufferFactory pbf = new ProtocolBufferFactory();

        private bool disposing;

        public string IndexFilePath
        {
            get { return BlobCache.IndexFilePath; }
        }

        public string DataFilePath
        {
            get { return BlobCache.DataFilePath; }
        }

        public BinaryBlobStorage()
        {
            BlobCache = new Dev2PersistantDictionary<byte[]>(Guid.NewGuid().ToString());
        }

        public BinaryBlobStorage(string filePath, string idxPath)
        {
            BlobCache = new Dev2PersistantDictionary<byte[]>(filePath, idxPath);
        }

        public T FetchObject<T>(string key)
        {
            byte[] data = BlobCache[key];

            IDev2ProtocolBuffer obj = pbf.FindMatch(typeof(T).ToString());

            obj.ToObject(data);

            return (T)obj;
        }

        public void PushObject(string key, IDev2ProtocolBuffer obj)
        {
            if (obj != null)
            {
                BlobCache[key] = obj.ToByteArray();
            }
        }


        public void SyncTo(NetworkContext sendTo)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            if (!disposing)
            {
                disposing = true;
                BlobCache.Dispose();
            }
        }
    }
}
