using System;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Storage.ProtocolBuffers
{
    /// <summary>
    /// Used to track aliases when shaping ;)
    /// </summary>
    [Serializable]
    public class BinaryDataListAlias
    {
        public Guid MasterKeyID;

        public bool IsCOW;

        public string MasterKey;

        public string ChildKey;

        public string MasterNamespace;

        public string MasterColumn;

        public IBinaryDataListEntry MasterEntry;

        
    }
}
