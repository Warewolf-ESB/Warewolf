
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
using Dev2.Data.Storage.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Storage
{

    /// <summary>
    /// Wrapper to scale out the BinaryStorage class 
    /// Partitions across the mod of the key hash code for now ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Dev2DistributedCache<T> where T : AProtocolBuffer
    {

        // we always create a background container to work in ;)
        private readonly int _numOfSegments;
        readonly ConcurrentDictionary<int, Dev2BinaryStorage<T>> _scalableCache = new ConcurrentDictionary<int, Dev2BinaryStorage<T>>();

        public Dev2DistributedCache(int segmentSize, int numOfSegments)
        {

            _numOfSegments = numOfSegments;

            var sizeOf = segmentSize;

            for(int i = 0; i < _numOfSegments; i++)
            {
                _scalableCache[i] = new Dev2BinaryStorage<T>(Guid.NewGuid() + ".data", sizeOf);
            }

            // fire up the scrub region ;)
            CompactBuffer.Init(sizeOf);
        }

        #region Indexers

        public T this[StorageKey key]
        {
            get
            {
                int segID = GetSegmentKey(key);

                var storageContainer = _scalableCache[segID];

                return storageContainer[key.UniqueKey];
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value", "Cannot add null to dictionary");
                }

                int segID = GetSegmentKey(key);

                var storageContainer = _scalableCache[segID];

                storageContainer.Add(key.UniqueKey, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Useds the memory storage information mb.
        /// </summary>
        /// <returns></returns>
        public double UsedMemoryStorageInMB()
        {
            return _scalableCache.Values.Sum(container => (container.UsedMemoryMB() / (1024 * 1024)));
        }

        /// <summary>
        /// Capacities the memory storage information mb.
        /// </summary>
        /// <returns></returns>
        public double CapacityMemoryStorageInMB()
        {
            return _scalableCache.Values.Sum(container => (container.CapacityMemoryMB() / (1024 * 1024)));
        }

        /// <summary>
        /// Removes the specified key from storage
        /// </summary>
        /// <param name="theList">The list.</param>
        /// <returns></returns>
        public int RemoveAll(IEnumerable<Guid> theList)
        {
            return _scalableCache.Values.Sum(container => container.RemoveAll(theList));
        }

        /// <summary>
        /// Removes the specified key from storage
        /// </summary>
        /// <param name="key">The key.</param>
        public int RemoveAll(string key)
        {
            return _scalableCache.Values.Sum(container => container.RemoveAll(key));
        }

        public void CompactMemory(bool force = false)
        {
            foreach(var container in _scalableCache.Values)
            {
                container.CompactMemory(force);
            }
        }

        /// <summary>
        /// Disposes the configuration on exit.
        /// </summary>
        public void DisposeOnExit()
        {
            foreach(var tmp in _scalableCache.Values)
            {
                tmp.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private int GetSegmentKey(StorageKey sk)
        {
            return Math.Abs(sk.FragmentID % _numOfSegments);
        }

        #endregion

        public void Clear()
        {
            foreach(Dev2BinaryStorage<T> dev2BinaryStorage in _scalableCache.Values)
            {
                dev2BinaryStorage.Clear();
            }
        }
    }
}
