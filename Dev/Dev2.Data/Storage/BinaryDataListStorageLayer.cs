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
        private static Dev2DistributedCache<BinaryDataListRow> LevelZeroCache;

        bool _disposed;


        // New Row Based Junk
        public BinaryDataListStorageLayer()
        {
            if(LevelZeroCache == null)
            {
                LevelZeroCache = new Dev2DistributedCache<BinaryDataListRow>(StorageSettingManager.GetSegmentCount(), StorageSettingManager.GetSegmentSize());
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

            return LevelZeroCache.RemoveAll(overrideKey);

        }

        /// <summary>
        /// Removes the specified unique key.
        /// </summary>
        /// <param name="theList">The list.</param>
        /// <returns></returns>
        public static int RemoveAll(IEnumerable<Guid> theList)
        {
            return LevelZeroCache.RemoveAll(theList);
        }

        /// <summary>
        /// Removes the specified unique key.
        /// </summary>
        /// <param name="uniqueKey">The unique key.</param>
        /// <returns></returns>
        public static int RemoveAll(string uniqueKey)
        {
            return LevelZeroCache.RemoveAll(uniqueKey);
        }

        public static void CompactMemory(bool force = false)
        {
            LevelZeroCache.CompactMemory(force);
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
            if(LevelZeroCache != null)
            {
                LevelZeroCache.DisposeOnExit();
            }

        }

        #endregion

        #region Memory Management

        /// <summary>
        /// Gets the used memory information mb.
        /// </summary>
        /// <returns></returns>
        public static double GetUsedMemoryInMB()
        {
            if(LevelZeroCache != null)
            {
                return LevelZeroCache.UsedMemoryStorageInMB();
            }

            return 0.0;
        }

        /// <summary>
        /// Gets the capacity memory information mb.
        /// </summary>
        /// <returns></returns>
        public static double GetCapacityMemoryInMB()
        {
            if(LevelZeroCache != null)
            {
                return LevelZeroCache.CapacityMemoryStorageInMB();
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
            if(LevelZeroCache == null)
            {
                LevelZeroCache = new Dev2DistributedCache<BinaryDataListRow>(StorageSettingManager.GetSegmentCount(), StorageSettingManager.GetSegmentSize());
            }
        }

        public static void Teardown()
        {
            if(LevelZeroCache != null)
            {
                LevelZeroCache.DisposeOnExit();
            }
        }

        #endregion

        #region General

        public bool TryGetValue(StorageKey key, short missSize, out BinaryDataListRow value)
        {
            value = LevelZeroCache[key] ?? new BinaryDataListRow(missSize);

            return true;
        }

        public bool TrySetValue(StorageKey sk, short missSize, BinaryDataListRow value)
        {

            LevelZeroCache[sk] = value;

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

                var tmp = LevelZeroCache[tmpKey];

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

    }

    public class BinaryDataListRowEqualityComparer : IEqualityComparer<IndexBasedBinaryDataListRow>
    {
        readonly List<int> _compareCols;

        #region Implementation of IEqualityComparer<in IndexBasedBinaryDataListRow>

        public BinaryDataListRowEqualityComparer(List<int> compareCols)
        {
            _compareCols = compareCols;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref>
        ///                                              <name>T</name>
        ///                                          </paramref>
        ///     to compare.</param>
        /// <param name="y">The second object of type <paramref>
        ///                                               <name>T</name>
        ///                                           </paramref>
        ///     to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(IndexBasedBinaryDataListRow x, IndexBasedBinaryDataListRow y)
        {
            var equal = false;
            foreach(var compareCol in _compareCols)
            {
                equal = x.Row.FetchValue(compareCol, -1) == y.Row.FetchValue(compareCol, -1);
            }
            return equal;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IndexBasedBinaryDataListRow obj)
        {
            return _compareCols.Sum(compareCol => obj.Row.FetchValue(compareCol, -1).GetHashCode());
        }

        #endregion
    }

    public struct IndexBasedBinaryDataListRow
    {
        public BinaryDataListRow Row { get; set; }
        public int Index { get; set; }


    }
}