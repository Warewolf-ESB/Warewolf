using System.Collections.Generic;
using Dev2.Data.Storage.Binary_Objects;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Binary_Objects.Structs
{
    /// <summary>
    /// Used to return federated keys ;)
    /// </summary>
    public class InternalFederatedTO
    {

        public FederatedStorageKey ParentKey;

        public FederatedStorageKey ChildKey;

        public IBinaryDataListEntry MasterEntry;

        /// <summary>
        /// Fetches the asynchronous list.
        /// </summary>
        /// <returns></returns>
        public IList<FederatedStorageKey> FetchAsList()
        {
            if (ParentKey != null)
            {
                return new List<FederatedStorageKey>() {ParentKey, ChildKey};
            }

            return new List<FederatedStorageKey>() { ChildKey };
        }

        /// <summary>
        /// Determines whether [has parent key].
        /// </summary>
        /// <returns></returns>
        public bool HasParentKey()
        {
            return ParentKey != null;
        }

    }
}
