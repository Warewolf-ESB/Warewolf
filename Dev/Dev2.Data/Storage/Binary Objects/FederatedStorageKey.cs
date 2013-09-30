using System.Collections.Generic;

namespace Dev2.Data.Storage.Binary_Objects
{
    /// <summary>
    /// Used to fetch and set federated entry values ;)
    /// </summary>
    public class FederatedStorageKey
    {

        public StorageKey TheKey;

        public ICollection<string> ImpactedColumns;
    }
}
