using System.Threading.Tasks;
using Dev2.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Dev2.Data.TO;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class BinaryDataListStorage : IDisposable
    {
        

        [NonSerialized]
        static readonly NameValueCollection MemoryCacheConfiguration = new NameValueCollection { { "CacheMemoryLimit", ConfigurationManager.AppSettings["DataListLvl2CacheCapacity"] }, { "PollingInterval", ConfigurationManager.AppSettings["DataListLvl2CachePollInterval"] } };

        [NonSerialized]
        static readonly MemoryCache LevelTwoCache = new MemoryCache("DataList", MemoryCacheConfiguration);

        [NonSerialized]
        static readonly CacheItemPolicy CacheItemPolicy = new CacheItemPolicyDataList(RemovedCallback);

        readonly string _uniqueIndentifier;
        IndexList _populatedKeys;

        [NonSerialized]
        static bool ItemsAddedToLevelThreeCache;

        [NonSerialized]
        static readonly ConcurrentDictionary<string, IBinaryDataListRow> LevelOneCache = new ConcurrentDictionary<string, IBinaryDataListRow>(GlobalConstants.DefaultConcurrentStorageAccsors, GlobalConstants.DefaultDataListCreateCacheSizeLvl1, StringComparer.Ordinal);

        [NonSerialized]
        static readonly Dev2PersistantDictionary<IBinaryDataListRow> LevelThreeCache = new Dev2PersistantDictionary<IBinaryDataListRow>(Guid.NewGuid().ToString());

        [NonSerialized]
        static readonly BackgroundWorker BackgroundWorker = new BackgroundWorker();

        [NonSerialized]
        readonly static ManualResetEvent _keepRunning = new ManualResetEvent(true);

        private static ReaderWriterLockSlim _level1Lock = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim _level2Lock = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim _level3Lock = new ReaderWriterLockSlim();

        private static bool _backgroundWorkerInited = false;

        readonly String _uniqueIdentifierGuid;
        bool _disposed = false;
        string _uniqueKey;

        public int ColumnSize { get; set; }

        // New Row Based Junk
        public BinaryDataListStorage(string uniqueIndex, Guid uniqueIdentifier)
        {
            if (!_backgroundWorkerInited)
            {
                _backgroundWorkerInited = true;
                if (!BackgroundWorker.IsBusy)
                {
                    BackgroundWorker.WorkerSupportsCancellation = true;
                    BackgroundWorker.WorkerReportsProgress = false;
                    BackgroundWorker.DoWork += MoveItemsIntoMemoryCacheBackground;
                    if (!BackgroundWorker.IsBusy) BackgroundWorker.RunWorkerAsync();
                }
            }

            _uniqueIdentifierGuid = uniqueIdentifier.ToString();
            _uniqueIndentifier = uniqueIndex + _uniqueIdentifierGuid;
            _populatedKeys = new IndexList(null, 1);
        }

        void MoveItemsIntoMemoryCacheBackground(object sender, DoWorkEventArgs e)
        {
            while (!_disposed)
            {
                _keepRunning.WaitOne();
                MoveItemsIntoMemoryCache();
                _keepRunning.Reset(); // reset the waitone event
            }
        }

        static void MoveItemsIntoMemoryCache()
        {
            if (LevelOneCache.Count >= GlobalConstants.DefaultDataListMaxCacheSizeLvl1)
            {
                _level1Lock.EnterWriteLock();
                foreach (string key in LevelOneCache.Keys)
                {
                    IBinaryDataListRow list;

                    // ensure it is in level 2 before it is removed from level 1 so we only need level1 lock ;)
                    if (LevelOneCache.TryGetValue(key, out list))
                    {
                        LevelTwoCache.Set(key, list, CacheItemPolicy);
                        LevelOneCache.TryRemove(key, out list);
                    }

                }
                _level1Lock.ExitWriteLock();
            }
        }

        int ExtractIndexFromKey(string key)
        {
            int pos = key.IndexOf("|", StringComparison.Ordinal);
            if (pos > 0)
            {
                string tmp = key.Substring(0, pos);
                int result;
                Int32.TryParse(tmp, out result);
                return result;
            }

            return -1;
        }

        string GetUniqueKey(int key)
        {
            return _uniqueKey = key + "|" + _uniqueIndentifier;
        }

        public IBinaryDataListRow this[int key]
        {
            get
            {
                IBinaryDataListRow v;
                TryGetValue(key, ColumnSize, out v);
                return v;
            }
            set
            {
                var uniqueKey = GetUniqueKey(key);

                AddToLevelOneCache(uniqueKey, value);
                // remove from level 2 and 3 if present

                _level2Lock.EnterWriteLock();
                if (LevelTwoCache.Contains(uniqueKey))
                {
                    LevelTwoCache.Remove(uniqueKey);
                }
                _level2Lock.ExitWriteLock();

                // only if there is data to clear....
                if (ItemsAddedToLevelThreeCache)
                {
                    _level3Lock.EnterWriteLock();
                    LevelThreeCache.Remove(uniqueKey);
                    _level3Lock.ExitWriteLock();
                }

                SetMaxValue(key);
                _populatedKeys.RemoveGap(key);
            }
        }
        
        // hack to work around Jurie's faulty logic below ;)
        public void ReInstateMaxValue(int idx)
        {
            _populatedKeys.MaxValue = idx;
        }

        // hack to work around Jurie's faulty logic below ;)
        public void ReInstateMinValue(int idx)
        {
            _populatedKeys.MinValue = idx;
        }
        
        public void SetMaxValue(int idx)
        {
            if (idx < _populatedKeys.MaxValue)
            {
                // remove gaps
                if (_populatedKeys.Gaps.Contains(idx))
                {
                    _populatedKeys.RemoveGap(idx);
                }
            }
            else if (idx >= _populatedKeys.MaxValue)
            {
                // set new max
                int dif = (idx - _populatedKeys.MaxValue);
                if (dif >= 1)
                {
                    // find the gaps ;)
                    for (int i = 0; i < dif; i++)
                    {
                        var val = _populatedKeys.MaxValue + i;
                        if (this[val] == null || this[val].IsEmpty)
                            _populatedKeys.AddGap(val);
                    }
                }
            }

            _populatedKeys.MaxValue = idx;
        }

        public void RemoveValueFromIndex(string key)
        {

            int idx = ExtractIndexFromKey(key);
            if (idx > -1 && idx <= _populatedKeys.MaxValue)
            {
                _populatedKeys.AddGap(idx); // Remove from indexes too :)    
            }
        }

        public IIndexIterator Keys
        {
            get
            {
                return _populatedKeys.FetchIterator();
            }
        }

        public int Count
        {
            get
            {
                return Keys.Count;
            }
        }

        static void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            _level3Lock.EnterWriteLock();
            CacheEntryRemovedReason cacheEntryRemovedReason = arguments.RemovedReason;
            bool addEntry = cacheEntryRemovedReason == CacheEntryRemovedReason.Evicted;
            if (addEntry)
            {
                var cacheItem = arguments.CacheItem;
                var value = cacheItem.Value;
                var key = cacheItem.Key;
                LevelThreeCache.Add(key, (IBinaryDataListRow)value);
                ItemsAddedToLevelThreeCache = true;
            }
            _level3Lock.ExitWriteLock();
        }

        public bool TryGetValue(int key, int missSize, out IBinaryDataListRow value)
        {
            bool r = false;
            string uniqueKey = GetUniqueKey(key);
            if (TryGetValueOutOfCaches(out value, uniqueKey))
            {
                r = true;
            }

            if (value == null)
            {
                value = new BinaryDataListRow(missSize);
                IBinaryDataListRow items = value;
                AddToLevelOneCache(uniqueKey, items);
                r = true;
            }
            else
            {
                if (!_populatedKeys.Contains(key))
                {
                    // Naughty, we need to push the key, not blank it?!?!
                    SetMaxValue(key);
                }
            }
            return r;
        }

        static void AddToLevelOneCache(string uniqueKey, IBinaryDataListRow row)
        {

            if (!row.IsEmpty)
            {
                _level1Lock.EnterWriteLock();
                LevelOneCache.AddOrUpdate(uniqueKey, row, (s, r) => row);
                _level1Lock.ExitWriteLock();
                _keepRunning.Set();
            }
            else
            {
                Remove(uniqueKey);
            }
        }

        bool TryGetValueOutOfCaches(out IBinaryDataListRow value, string uniqueKey)
        {
            if (TryGetValueFromLevelOneCache(uniqueKey, out value))
            {
                return true;
            }

            if (TryGetValueFromLevelTwoCache(uniqueKey, out value))
            {
                return true;
            }

            if (ItemsAddedToLevelThreeCache)
            {
                if (TryGetValueFromLevelThreeCache(uniqueKey, out value))
                {
                    return true;
                }
            }
            return false;
        }

        bool TryGetValueFromLevelThreeCache(string uniqueKey, out IBinaryDataListRow value)
        {
            _level3Lock.EnterReadLock();
            value = LevelThreeCache[uniqueKey];
            _level3Lock.ExitReadLock();
            return value != null;
        }

        bool TryGetValueFromLevelTwoCache(string uniqueKey, out IBinaryDataListRow value)
        {
            _level2Lock.EnterReadLock();

            if (LevelTwoCache.GetCount() > 0)
            {
                value = (IBinaryDataListRow) LevelTwoCache.Get(uniqueKey);
            }
            else
            {
                value = null;
            }

            _level2Lock.ExitReadLock();

            return value != null;
        }

        bool TryGetValueFromLevelOneCache(string uniqueKey, out IBinaryDataListRow value)
        {
            bool result;

            _level1Lock.EnterReadLock();
            result = LevelOneCache.TryGetValue(uniqueKey, out value);
            _level1Lock.ExitReadLock();

            return result;
        }

        public void Add(int key, IBinaryDataListRow value)
        {
            this[key] = value;
            _keepRunning.Set();
        }

        public bool ContainsKey(int key)
        {
            return (_populatedKeys.Contains(key));
        }

        public bool Remove(int key)
        {
            string uniqueKey = GetUniqueKey(key);
            Remove(uniqueKey);
            _populatedKeys.AddGap(key);
            return true;
        }

        public static bool Remove(string uniqueKey)
        {
            IBinaryDataListRow list;

            _level1Lock.EnterWriteLock();
            LevelOneCache.TryRemove(uniqueKey, out list);
            _level1Lock.ExitWriteLock();

            _level2Lock.EnterWriteLock();
            LevelTwoCache.Remove(uniqueKey);
            _level2Lock.ExitWriteLock();

            if (ItemsAddedToLevelThreeCache)
            {
                _level3Lock.EnterWriteLock();
                LevelThreeCache.Remove(uniqueKey);
                _level3Lock.ExitWriteLock();
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BinaryDataListStorage()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (!disposing)
            {
                return;
            }

            DisposeWaitHandle();
            DisposeBackgroundWorker();
            RemoveCachePolicyEvent();
            RemoveFromLevelOneCache();
            DisposeMemoryCache();
            DisposeLevelThreeCache();
        }

        void DisposeWaitHandle()
        {
            if (_keepRunning != null)
            {
                _keepRunning.Set();
            }
        }

        void DisposeLevelThreeCache()
        {
            if (LevelThreeCache != null)
            {
                RemoveFromLevelThreeCache();
            }

            // ensure we clean up file resources ;)
            if (LevelThreeCache != null)
            {
                LevelThreeCache.Dispose();
            }
        }

        void DisposeMemoryCache()
        {
            if (LevelTwoCache != null)
            {
                RemoveFromLevelTwoCache();
            }
        }

        static void RemoveCachePolicyEvent()
        {
            if (CacheItemPolicy == null) return;
            //CacheItemPolicy.RemovedCallback -= RemovedCallback;
        }

        void DisposeBackgroundWorker()
        {
            if (BackgroundWorker == null) return;
            if (BackgroundWorker.WorkerSupportsCancellation)
            {
                BackgroundWorker.CancelAsync();
            }
        }

        void RemoveFromLevelOneCache()
        {
            IEnumerable<string> keys = LevelOneCache.Keys
                                                    .Where(key =>
                                                           key.IndexOf(_uniqueIndentifier, StringComparison.Ordinal) > 0);

            _level1Lock.EnterWriteLock();

            foreach (string key in keys)
            {
                IBinaryDataListRow row;
                LevelOneCache.TryRemove(key, out row);
                RemoveValueFromIndex(key); // Remove from indexes too :)   
            }

            _level1Lock.ExitWriteLock();
        }

        void RemoveFromLevelTwoCache()
        {
            IEnumerable<string> keys =
                LevelTwoCache.Select(pair => pair.Key)
                                .Where(
                                    key =>
                                    (key.IndexOf(_uniqueIdentifierGuid, StringComparison.Ordinal)) >= 0 && !string.IsNullOrEmpty(key));

            _level2Lock.EnterWriteLock();

            foreach (var key in keys)
            {
                LevelTwoCache.Remove(key);
                RemoveValueFromIndex(key); // Remove from indexes too :) 
            }

            _level2Lock.ExitWriteLock();
        }

        void RemoveFromLevelThreeCache()
        {
            if (ItemsAddedToLevelThreeCache)
            {
                IEnumerable<string> keys =
                    LevelThreeCache.Keys.Where(
                        pair => (pair.IndexOf(_uniqueIndentifier, StringComparison.Ordinal)) >= 0 && !string.IsNullOrEmpty(pair));

                _level3Lock.EnterWriteLock();

                foreach (string key in keys)
                {
                    LevelThreeCache.Remove(key);
                    RemoveValueFromIndex(key); // Remove from indexes too :)   
                }

                _level3Lock.ExitWriteLock();
            }

            //LevelThreeCache.Dispose();
        }


        /// <summary>
        /// Gets the values.
        ///     
        /// TODO : WHAT ABOUT GAPS?! DO WE REALLY WANT BLANK VALUES FOR THESE?!
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public List<IBinaryDataListRow> GetValues(int startIndex, int endIndex)
        {
            var rows = new List<IBinaryDataListRow>();

            while(startIndex < endIndex)
            {
                IBinaryDataListRow value;
                string uniqueKey = GetUniqueKey(startIndex);
                if(TryGetValueOutOfCaches(out value, uniqueKey))
                {
                    rows.Add(value);
                    startIndex++;
                }
                else
                {
                    throw new Exception(string.Format("Critical error. No value in storage for index {0}", startIndex));                    
                }               
            }

            return rows;
        }

        public List<int> DistinctGetRows(IIndexIterator keys, List<int> colIdx)
        {

            List<IndexBasedBinaryDataListRow> rows = new List<IndexBasedBinaryDataListRow>();

            // avoid blank rows ;)
            while (keys.HasMore())
            {
                // fetch a fixed segment at a time ;)
                IBinaryDataListRow value;
                var idx = keys.FetchNextIndex();

                string uniqueKey = GetUniqueKey(idx);

                if (TryGetValueOutOfCaches(out value, uniqueKey))
                {
                    rows.Add(new IndexBasedBinaryDataListRow() { Row = value, Index = idx });
                }
                else
                {
                    throw new Exception(string.Format("Critical error. No value in storage for index {0}", idx));
                }
            }

            var indexBasedBinaryDataListRowEqualityComparer = new BinaryDataListRowEqualityComparer(colIdx);

            // fetch row indexes ;)
            IEnumerable<int> indexBasedBinaryDataListRows = rows.Distinct(indexBasedBinaryDataListRowEqualityComparer).Select(c=>c.Index);

            return indexBasedBinaryDataListRows.ToList();

        }
    }

    public class BinaryDataListRowEqualityComparer : IEqualityComparer<IndexBasedBinaryDataListRow>
    {
        readonly List<int> _compareCols;

        #region Implementation of IEqualityComparer<in IndexBasedBinaryDataListRow>

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        private BinaryDataListRowEqualityComparer()
        {
        }

        public BinaryDataListRowEqualityComparer(List<int> compareCols)
        {
            _compareCols = compareCols;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(IndexBasedBinaryDataListRow x, IndexBasedBinaryDataListRow y)
        {
            var equal = false;
            foreach(var compareCol in _compareCols)
            {
                equal = x.Row.FetchValue(compareCol) == y.Row.FetchValue(compareCol);
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
            var hashCode = 0;

            foreach(var compareCol in _compareCols)
            {
                hashCode+=obj.Row.FetchValue(compareCol).GetHashCode();
            }
            return hashCode;
        }

        #endregion
    }

    public struct IndexBasedBinaryDataListRow
    {
       public  IBinaryDataListRow Row { get; set; }
       public  int Index { get; set; }
        
    }

    internal class CacheItemPolicyDataList : CacheItemPolicy
    {
        public CacheItemPolicyDataList(CacheEntryRemovedCallback removedItemPolicy)
        {
            RemovedCallback += removedItemPolicy;
        }
    }
}