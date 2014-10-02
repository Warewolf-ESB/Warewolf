
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Data.Storage;

namespace Dev2.Data.Binary_Objects
{
    /// <summary>
    /// Used to register data list creation so we can ensure clean up ;)
    /// </summary>
    public static class DataListRegistar
    {
        private static readonly ConcurrentDictionary<int, IList<Guid>> RegistrationRoster = new ConcurrentDictionary<int, IList<Guid>>();

        private static readonly ConcurrentDictionary<int, int> ActivityThreadToParentThreadId = new ConcurrentDictionary<int, int>();

        private static readonly object Lock = new object();

        /// <summary>
        /// Registers the activity thread automatic parent unique identifier.
        /// </summary>
        /// <param name="parentId">The parent unique identifier.</param>
        /// <param name="childId">The child unique identifier.</param>
        public static void RegisterActivityThreadToParentId(int parentId, int childId)
        {
            if(parentId <= 0)
            {
                throw new Exception("ParentID is invalid [ " + parentId + " ]");
            }

            ActivityThreadToParentThreadId[childId] = parentId;
        }

        /// <summary>
        /// Registers the data list information scope.
        /// </summary>
        /// <param name="transactionScopeId">The transaction scope unique identifier.</param>
        /// <param name="dataListId">The data list unique identifier.</param>
        public static void RegisterDataListInScope(int transactionScopeId, Guid dataListId)
        {
            int keyId = transactionScopeId;

            lock(Lock)
            {
                // now we can correctly scope the data list creation ;)
                IList<Guid> theList;
                if(RegistrationRoster.TryGetValue(keyId, out theList))
                {
                    if(!theList.Contains(dataListId))
                    {
                        theList.Add(dataListId);
                    }
                }
                else
                {
                    Dev2Logger.Log.Debug("REGESTIRATION - Transactional scope ID = " + transactionScopeId);
                    // its new, add it ;)
                    RegistrationRoster[keyId] = new List<Guid> { dataListId };
                }
            }
        }

        /// <summary>
        /// Disposes the scope.
        /// </summary>
        /// <param name="transactionScopeId">The transaction scope unique identifier.</param>
        /// <param name="rootRequestId">The root request unique identifier.</param>
        /// <param name="doCompact">if set to <c>true</c> [document compact].</param>
        public static void DisposeScope(int transactionScopeId, Guid rootRequestId, bool doCompact = true)
        {
            Task.Run(() =>
            {

                Dev2Logger.Log.Debug("DISPOSING - Transactional scope ID = " + transactionScopeId);
                try
                {
                    lock(Lock)
                    {
                        IList<Guid> theList;
                        if(RegistrationRoster.TryGetValue(transactionScopeId, out theList))
                        {
                            theList.Remove(rootRequestId);
                            BinaryDataListStorageLayer.RemoveAll(theList);

                            // finally reset
                            IList<Guid> dummy;
                            RegistrationRoster.TryRemove(transactionScopeId, out dummy);

                            // now remove children ;)
                            var keyList = ActivityThreadToParentThreadId.Keys.ToList();
                            foreach(var key in keyList)
                            {
                                if(key == transactionScopeId)
                                {
                                    int dummyInt;
                                    ActivityThreadToParentThreadId.TryRemove(transactionScopeId, out dummyInt);
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Dev2Logger.Log.Error("DataListRegistar", e);
                }
                finally
                {
                    // now we need to pack memory to reclaim space ;)
                    if(doCompact)
                    {
                        // turned force off ;)
                        BinaryDataListStorageLayer.CompactMemory(true);
                    }
                }
            });
        }

        public static void ClearDataList()
        {
            BinaryDataListStorageLayer.Clear();
        }
    }
}
