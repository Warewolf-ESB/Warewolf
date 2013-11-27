using System;

namespace Dev2.Data.Storage.Binary_Objects
{
    /// <summary>
    /// Used as the internal storage key ;)
    /// </summary>
    public class StorageKey
    {

        public Guid UID;

        public String UniqueKey;

        public int FragmentID;

        public StorageKey(Guid uid, String key)
        {
            UID = uid;

            UniqueKey = key;

            FragmentID = key.GetHashCode();

        }
    }
}
