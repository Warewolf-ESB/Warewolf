
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
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Storage.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Storage
{
    [Serializable]
    public class BinaryDataListStorageLayer : IDisposable
    {
        [NonSerialized]
        private static Dev2DistributedCache<BinaryDataListRow> _levelZeroCache;

        bool _disposed;


        // New Row Based Junk
        public BinaryDataListStorageLayer()
        {
            if(_levelZeroCache == null)
            {
                _levelZeroCache = new Dev2DistributedCache<BinaryDataListRow>(StorageSettingManager.GetSegmentSize(), StorageSettingManager.GetSegmentCount());
            }
        }

        #region Properties

        public short ColumnSize { get; set; }

        #endregion

        #region Disposal
        /// <summary>
        /// Clears the cache from the BinaryDataListEntry level ;)
        /// </summary>
        /// <returns></returns>
        public int DisposeCache(string overrideKey = "")
        {
            if(string.IsNullOrEmpty(overrideKey))
            {
                throw new ArgumentNullException("overrideKey");
            }

            return _levelZeroCache.RemoveAll(overrideKey);

        }

        /// <summary>
        /// Removes the specified unique key.
        /// </summary>
        /// <param name="theList">The list.</param>
        /// <returns></returns>
        public static int RemoveAll(IEnumerable<Guid> theList)
        {
            return _levelZeroCache.RemoveAll(theList);
        }

        /// <summary>
        /// Removes the specified unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique key.</param>
        /// <returns></returns>
        public static int RemoveAll(string uniqueKey)
        {
            return _levelZeroCache.RemoveAll(uniqueKey);
        }

        public static void CompactMemory(bool force = false)
        {
            _levelZeroCache.CompactMemory(force);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BinaryDataListStorageLayer()
        {
            Dispose(false);
        }

        // ReSharper disable UnusedParameter.Local
        void Dispose(bool disposing)
        // ReSharper restore UnusedParameter.Local
        {
            if(_disposed)
            {
                return;
            }

            _disposed = true;
        }

        /// <summary>
        /// Disposes all static objects on exit ;)
        /// </summary>
        public static void DisposeOnExit()
        {
            if(_levelZeroCache != null)
            {
                _levelZeroCache.DisposeOnExit();
            }

        }

        #endregion

        #region Memory Management

        /// <summary>
        /// Gets the used memory information mb.
        /// </summary>
        /// <returns></returns>
        public static double GetUsedMemoryInMb()
        {
            if(_levelZeroCache != null)
            {
                return _levelZeroCache.UsedMemoryStorageInMB();
            }

            return 0.0;
        }

        /// <summary>
        /// Gets the capacity memory information mb.
        /// </summary>
        /// <returns></returns>
        public static double GetCapacityMemoryInMb()
        {
            if(_levelZeroCache != null)
            {
                return _levelZeroCache.CapacityMemoryStorageInMB();
            }

            return 0.0;
        }

        #endregion

        #region Setup / Teardown

        /// <summary>
        /// Bootstraps this instance.
        /// </summary>
        public static void Setup()
        {
            if(_levelZeroCache == null)
            {
                _levelZeroCache = new Dev2DistributedCache<BinaryDataListRow>(StorageSettingManager.GetSegmentSize(), StorageSettingManager.GetSegmentCount());
            }
        }

        public static void Teardown()
        {
            if(_levelZeroCache != null)
            {
                _levelZeroCache.DisposeOnExit();
            }
        }

        #endregion

        #region General

        public bool TryGetValue(StorageKey key, short missSize, out BinaryDataListRow value)
        {
            value = _levelZeroCache[key] ?? new BinaryDataListRow(missSize);

            return true;
        }

        public bool TrySetValue(StorageKey sk, short missSize, BinaryDataListRow value)
        {

            _levelZeroCache[sk] = value;

            return true;
        }



        public List<int> DistinctGetRows(StorageKey sk, IIndexIterator keys, List<int> colIdx)
        {

            List<IndexBasedBinaryDataListRow> rows = new List<IndexBasedBinaryDataListRow>();

            // avoid blank rows ;)
            while(keys.HasMore())
            {
                // fetch a fixed segment at a time ;)
                var idx = keys.FetchNextIndex();

                StorageKey tmpKey = new StorageKey(sk.UID, idx + sk.UniqueKey);

                var tmp = _levelZeroCache[tmpKey];

                if(tmp != null)
                {
                    rows.Add(new IndexBasedBinaryDataListRow { Row = tmp, Index = idx });
                }
                else
                {
                    throw new Exception(string.Format("Critical error. No value in storage for index {0}", idx));
                }
            }

            var indexBasedBinaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(colIdx);

            // fetch row indexes ;)
            IEnumerable<int> indexBasedBinaryDataListRows = rows.Distinct(indexBasedBinaryDataListRowEqualityComparer).Select(c => c.Index);

            return indexBasedBinaryDataListRows.ToList();

        }

        #endregion

        public static void Clear()
        {
            _levelZeroCache.Clear();
        }
    }
}
