using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class Dev2BinaryDataListStorage : IDisposable
    {

        //[NonSerialized]
        //static readonly NameValueCollection MemoryCacheConfiguration = new NameValueCollection { { "PhysicalMemoryLimitPercentage", GlobalConstants.DefaultDataListCacheSizeLvl2MemoryPercentage }, { "PollingInterval", GlobalConstants.DefaultDataListCacheSizeLvl2MemoryPollingInterval } };

        [NonSerialized]
        static readonly NameValueCollection MemoryCacheConfiguration = new NameValueCollection { { "CacheMemoryLimit", GlobalConstants.DefaultDataListCacheSizeLvl2MegaByteSize }, { "PollingInterval", GlobalConstants.DefaultDataListCacheSizeLvl2MemoryPollingInterval } };

        [NonSerialized]
        static readonly MemoryCache LevelTwoCache = new MemoryCache("DataList",MemoryCacheConfiguration);

        [NonSerialized]
        static readonly CacheItemPolicy CacheItemPolicy = new CacheItemPolicyDataList(RemovedCallback);

        readonly string _uniqueIndentifier;
        IndexList _populatedKeys;
        [NonSerialized]
        static bool ItemsAddedToLevelThreeCache;

        [NonSerialized]
        static readonly ConcurrentDictionary<string, IBinaryDataListRow> LevelOneCache = new ConcurrentDictionary<string, IBinaryDataListRow>(1, GlobalConstants.DefaultDataListCreateCacheSizeLvl1, StringComparer.Ordinal);

        [NonSerialized]
        static readonly Dev2PersistantDictionary<IBinaryDataListRow> LevelThreeCache = new Dev2PersistantDictionary<IBinaryDataListRow>(Path.GetTempFileName());

        int _key=Int32.MaxValue;
        string _uniqueKey;
        [NonSerialized]
        static readonly BackgroundWorker BackgroundWorker = new BackgroundWorker();
        [NonSerialized]
        readonly ManualResetEvent _keepRunning = new ManualResetEvent(true);
        private static object _lockDictionaryGuard = new object();
        readonly String _uniqueIdentifierGuid;


        // New Row Based Junk

        public Dev2BinaryDataListStorage(string uniqueIndex, Guid uniqueIdentifier)
        {
            if (!BackgroundWorker.IsBusy)
            {
                BackgroundWorker.WorkerSupportsCancellation = true;
                BackgroundWorker.WorkerReportsProgress = false;
                BackgroundWorker.DoWork += MoveItemsIntoMemoryCacheBackground;
                BackgroundWorker.RunWorkerAsync();
            }
//            if(BackgroundWorker.CancellationPending)
//            {
//                BackgroundWorker.RunWorkerAsync();
//            }

            _uniqueIdentifierGuid = uniqueIdentifier.ToString();
            _uniqueIndentifier = uniqueIndex + _uniqueIdentifierGuid;
            _populatedKeys = new IndexList();
        }

        void MoveItemsIntoMemoryCacheBackground(object sender, DoWorkEventArgs e)
        {
            while (_keepRunning.WaitOne())
            {
                MoveItemsIntoMemoryCache();
               // Thread.Sleep(150); // sleep for a little bit ;)
            }
        }

        static void MoveItemsIntoMemoryCache()
        {
            //if (BackgroundWorker.CancellationPending) return;
            if (LevelOneCache.Count >= GlobalConstants.DefaultDataListMaxCacheSizeLvl1)
            {
                var keys = LevelOneCache.Keys.ToList();
//                lock (_lockDictionaryGuard)
//                {
                    keys.ForEach(s =>
                    {
                        IBinaryDataListRow list;
                        LevelOneCache.TryRemove(s, out list);
                        LevelTwoCache.Set(s, list, CacheItemPolicy);
                    });
                //}
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
                TryGetValue(key, 100, out v); // TODO : Replace
                return v;
            }
            set
            {
                var uniqueKey = GetUniqueKey(key);
                AddToLevelOneCache(uniqueKey, value);
                _populatedKeys.SetMaxValue(key);
            }
        }

        public IIndexIterator Keys
        {
            get
            {
                return _populatedKeys.FetchIterator();
            }
            set
            {
                _populatedKeys.SetIterator(value);
            }
        }

        public List<IList<IBinaryDataListItem>> Values
        {
            get
            {
                return LevelTwoCache.Select(pair => (IList<IBinaryDataListItem>)pair.Value).ToList();
            }
        }

        public int Count
        {
            get
            {
                return _populatedKeys.Count();
            }
        }


        static void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            
            CacheEntryRemovedReason cacheEntryRemovedReason = arguments.RemovedReason;
            bool addEntryToRedis = cacheEntryRemovedReason == CacheEntryRemovedReason.Evicted;
            if(addEntryToRedis)
            {
                var cacheItem = arguments.CacheItem;
                var value = cacheItem.Value;
                var key = cacheItem.Key;
                LevelThreeCache.Add(key,(IBinaryDataListRow)value);
                ItemsAddedToLevelThreeCache = true;
            }
        }

        //public bool TryGetValue(int key, out IList<IBinaryDataListItem> value)
        public bool TryGetValue(int key, int missSize, out IBinaryDataListRow value)
        {
            bool r = false;
            string uniqueKey = GetUniqueKey(key);
            if(TryGetValueOutOfCaches(out value, uniqueKey))
            {
                r = true;
            }

            if(value == null)
            {
                value = new BinaryDataListRow(missSize); 
                IBinaryDataListRow items = value;
                AddToLevelOneCache(uniqueKey, items);
                r = true;
            }
            else
            {
                if(_populatedKeys.Contains(key))
                {
                    try
                    {
                        //IBinaryDataListRow items = value;
                        //AddToLevelOneCache(uniqueKey, items);

                    }
                    catch(Exception)
                    {
                        value = null;
                    }
                    if(value == null)
                    {
                        r = false;
                    }
                }
                else
                {
                    // Naughty, we need to push the key, not blank it?!?!
                    _populatedKeys.SetMaxValue(key);
                }
            }
            return r;
        }

        static void AddToLevelOneCache(string uniqueKey, IBinaryDataListRow row)
        {
            if(!row.IsEmpty)
            {
                LevelOneCache.AddOrUpdate(uniqueKey, row, (s, r) => row);
            }
        }

        bool TryGetValueOutOfCaches(out IBinaryDataListRow value, string uniqueKey)
        {
            if(TryGetValueFromLevelOneCache(uniqueKey, out value))
            {
                return true;
            }
            if(TryGetValueFromLevelTwoCache(uniqueKey, out value))
            {
                return true;
            }
            if(ItemsAddedToLevelThreeCache)
            {
                if(TryGetValueFromLevelThreeCache(uniqueKey, out value))
                {
                    return true;
                }
            }
            return false;
        }

        bool TryGetValueFromLevelThreeCache(string uniqueKey, out IBinaryDataListRow value)
        {
            value = LevelThreeCache[uniqueKey];
            return value != null;
        }

        bool TryGetValueFromLevelTwoCache(string uniqueKey, out IBinaryDataListRow value)
        {
            value = (IBinaryDataListRow)LevelTwoCache.Get(uniqueKey);
            return value != null;
        }

        bool TryGetValueFromLevelOneCache(string uniqueKey, out IBinaryDataListRow value)
        {
            return LevelOneCache.TryGetValue(uniqueKey, out value);
        }

        public void Add(int key, IBinaryDataListRow value)
        {
            this[key] = value;
        }

        public bool ContainsKey(int key)
        {
            return (_populatedKeys.Contains(key));
        }

        public bool Remove(int key)
        {
            string uniqueKey = GetUniqueKey(key);
            IBinaryDataListRow list;
            LevelOneCache.TryRemove(uniqueKey, out list);
            LevelTwoCache.Remove(uniqueKey);
            if (ItemsAddedToLevelThreeCache)
            {
                LevelThreeCache.Remove(uniqueKey);
            }
            _populatedKeys.AddGap(key);
            return true;
        }

        public void Clear()
        {

            RemoveFromLevelOneCache();
            RemoveFromLevelTwoCache();
            RemoveFromLevelThreeCache();
            _populatedKeys = new IndexList();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Dev2BinaryDataListStorage()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!disposing) return;
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
            if(LevelThreeCache != null)
            {
                RemoveFromLevelThreeCache();
            }
        }

        void DisposeMemoryCache()
        {
            if(LevelTwoCache != null)
            {
                RemoveFromLevelTwoCache();
            }
        }

        static void RemoveCachePolicyEvent()
        {
            if(CacheItemPolicy == null) return;
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
            List<string> keysToRemove = LevelOneCache.Keys
                                        .Where(key => 
                                          key.Contains(_uniqueIndentifier) || key.Contains(GlobalConstants.NullEntryNamespace))
                                        .ToList();
            if (keysToRemove.Count != 0)
            {
                IBinaryDataListRow row;
                keysToRemove.ForEach(keyToRemove => LevelOneCache.TryRemove(keyToRemove, out row));
            }
        }      
        void RemoveFromLevelTwoCache()
        {
            //pair => (pair.Key.Contains(_uniqueIdentifierGuid) || pair.Key.Contains(GlobalConstants.NullEntryNamespace)) ? pair.Key : null
            List<string> keysToRemove = LevelTwoCache.Select(pair => pair.Key).Where(key => key.Contains(_uniqueIdentifierGuid) || key.Contains(GlobalConstants.NullEntryNamespace)).ToList();
            if(keysToRemove.Count != 0)
            {
                keysToRemove.ForEach(keyToRemove =>
                {
                    if (!String.IsNullOrEmpty(keyToRemove))
                    {
                        LevelTwoCache.Remove(keyToRemove);
                    }
                });
            }
        }

        void RemoveFromLevelThreeCache()
        {
            if (ItemsAddedToLevelThreeCache)
            {
                var keysToRemove = LevelThreeCache.Keys.ToList().Where(pair => pair.Contains(_uniqueIndentifier) || pair.Contains(GlobalConstants.NullEntryNamespace)).ToList();

                if (keysToRemove.Count != 0)
                {

                    for (int i = keysToRemove.Count - 1; i >= 0; i--)
                    {
                        var value = keysToRemove[i];
                        if (!String.IsNullOrEmpty(value))
                        {
                            LevelThreeCache.Remove(value);
                        }
                    }
                }
            }
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