using System;
using Microsoft.Win32;

namespace Gui.Utility
{
    // Used to modify the registry
    public static class RegistryKeyExtensionMethods
    {
        public static RegistryKey GetOrCreateSubKey(this RegistryKey registryKey, string parentKeyLocation,
            string key, bool writable)
        {
            string keyLocation = string.Format(@"{0}\{1}", parentKeyLocation, key);

            RegistryKey foundRegistryKey = registryKey.OpenSubKey(keyLocation, writable);

            return foundRegistryKey ?? registryKey.CreateSubKey(parentKeyLocation, key);
        }

        public static RegistryKey CreateSubKey(this RegistryKey registryKey, string parentKeyLocation, string key)
        {
            RegistryKey parentKey = registryKey.OpenSubKey(parentKeyLocation, true); //must be writable == true
            if(parentKey == null) { throw new NullReferenceException(string.Format("Missing parent key: {0}", parentKeyLocation)); }

            RegistryKey createdKey = parentKey.CreateSubKey(key);
            if(createdKey == null) { throw new Exception(string.Format("Key not created: {0}", key)); }

            return createdKey;
        }
    }
}
