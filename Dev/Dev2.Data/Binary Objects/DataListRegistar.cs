using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dev2.Common;
using Dev2.Data.Storage;
using Dev2.DataList.Contract;

namespace Dev2.Data.Binary_Objects
{
    /// <summary>
    /// Used to register data list creation so we can ensure clean up ;)
    /// </summary>
    public static class DataListRegistar
    {
        private static ConcurrentDictionary<int, IList<Guid>> _registrationRoster = new ConcurrentDictionary<int, IList<Guid>>();

        private static ConcurrentDictionary<int, int> _activityThreadToParentThreadID = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Registers the activity thread automatic parent unique identifier.
        /// </summary>
        /// <param name="parentId">The parent unique identifier.</param>
        /// <param name="childID">The child unique identifier.</param>
        public static void RegisterActivityThreadToParentId(int parentId, int childID)
        {
            if (parentId <= 0)
            {
                throw new Exception("ParentID is invalid [ " + parentId + " ]");
            }

            _activityThreadToParentThreadID[childID] = parentId;
        }

        /// <summary>
        /// Registers the data list information scope.
        /// </summary>
        /// <param name="transactionScopeID">The transaction scope unique identifier.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        public static void RegisterDataListInScope(int transactionScopeID, Guid dataListID)
        {
            IList<Guid> theList;

            int keyID = transactionScopeID;

            // we need to check for a child to parent mapping first ;)
            int parentID;
            if (_activityThreadToParentThreadID.TryGetValue(transactionScopeID, out parentID))
            {
                keyID = parentID;
            }

            // now we can correctly scope the data list creation ;)
            if(_registrationRoster.TryGetValue(keyID, out theList))
            {
                if (!theList.Contains(dataListID))
                {
                    theList.Add(dataListID);    
                }

            }
            else
            {
                // its new, add it ;)
                _registrationRoster[keyID] = new List<Guid> { dataListID };
            }
        }

        /// <summary>
        /// Disposes the scope.
        /// </summary>
        /// <param name="transactionScopeID">The transaction scope unique identifier.</param>
        /// <param name="rootRequestID">The root request unique identifier.</param>
        public static void DisposeScope(int transactionScopeID, Guid rootRequestID)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            IList<Guid> theList;

            if(_registrationRoster.TryGetValue(transactionScopeID, out theList))
            {
                foreach (var id in theList)
                {
                    // force its eviction ;)
                    if (id != rootRequestID)
                    {
                        if (!compiler.ForceDeleteDataListByID(id))
                        {
                            // did not find it in the cache, we need to manually remove it ;)
                            BinaryDataListStorageLayer.RemoveAll(id.ToString());
                        }
                    }
                }

                // finally reset
                IList<Guid> dummy;
                _registrationRoster.TryRemove(transactionScopeID, out dummy);

                // now remove children ;)
                foreach(var key in _activityThreadToParentThreadID.Keys)
                {
                    if (key == transactionScopeID)
                    {
                        int dummyInt;
                        _activityThreadToParentThreadID.TryRemove(transactionScopeID, out dummyInt);
                    }
                }
            }
        }

    }
}
