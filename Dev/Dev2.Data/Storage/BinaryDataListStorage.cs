using Dev2.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

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

        //private static object _lockLvl1Guard = new object();
        //private static object _lockLvl2Guard = new object();
        private static bool _backgroundWorkerInited = false;

        readonly String _uniqueIdentifierGuid;
        bool _disposed = false;
        int _key = Int32.MaxValue;
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

        string GetUniqueKey(int key)
        {
            if (_key != key)
            {
                _key = key;
                _uniqueKey = _key.ToString(CultureInfo.InvariantCulture) + _uniqueIndentifier;
            }
            return _uniqueKey;
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

                if (LevelTwoCache.Contains(uniqueKey))
                {
                    _level2Lock.EnterWriteLock();
                    LevelTwoCache.Remove(uniqueKey);
                    _level2Lock.ExitWriteLock();
                }

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
                _populatedKeys.MaxValue = idx;
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
                                                           key.Contains(_uniqueIndentifier));

            foreach (string key in keys)
            {
                IBinaryDataListRow row;
                _level1Lock.EnterWriteLock();
                LevelOneCache.TryRemove(key, out row);
                _level1Lock.ExitWriteLock();
            }
        }

        void RemoveFromLevelTwoCache()
        {
            IEnumerable<string> keys =
                LevelTwoCache.Select(pair => pair.Key)
                                .Where(
                                    key =>
                                    (key.Contains(_uniqueIdentifierGuid)) && !string.IsNullOrEmpty(key));

            foreach (var key in keys)
            {
                _level2Lock.EnterWriteLock();
                LevelTwoCache.Remove(key);
                _level2Lock.ExitWriteLock();
            }
        }

        void RemoveFromLevelThreeCache()
        {
            if (ItemsAddedToLevelThreeCache)
            {
                IEnumerable<string> keys =
                    LevelThreeCache.Keys.Where(
                        pair => (pair.Contains(_uniqueIndentifier)) && !string.IsNullOrEmpty(pair));

                foreach (string key in keys)
                {
                    _level3Lock.EnterWriteLock();
                    LevelThreeCache.Remove(key);
                    _level3Lock.ExitWriteLock();
                }
            }

            //LevelThreeCache.Dispose();
        }
    }

    internal class CacheItemPolicyDataList : CacheItemPolicy
    {
        public CacheItemPolicyDataList(CacheEntryRemovedCallback removedItemPolicy)
        {
            RemovedCallback += removedItemPolicy;
        }
    }

}