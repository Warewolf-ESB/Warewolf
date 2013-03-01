using System;
using System.Collections.Concurrent;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalog : IResourceCatalog
    {
        readonly ConcurrentDictionary<IResourceCatalogItemKey, IResourceCatalogItem> _items = new ConcurrentDictionary<IResourceCatalogItemKey, IResourceCatalogItem>();

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile ResourceCatalog _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ResourceCatalog Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new ResourceCatalog();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Initialization

        // Prevent instantiation
        ResourceCatalog()
        {
            //RepositoryPath = GlobalConstants.WorkspacePath;
            //Directory.CreateDirectory(RepositoryPath);
            //Get(ServerWorkspaceID, true);
        }

        #endregion

        #region Implementation of IResourceCatalog

        public void Load()
        {
        }

        #endregion
    }
}