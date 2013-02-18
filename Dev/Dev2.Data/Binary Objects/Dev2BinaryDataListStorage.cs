using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class Dev2BinaryDataListStorage : IDisposable
    {
        [NonSerialized]
        static readonly NameValueCollection MemoryCacheConfiguration = new NameValueCollection { { "CacheMemoryLimitMegabytes", GlobalConstants.DefaultDataListCacheSizeLvlMegaByteSize }, { "PhysicalMemoryLimitPercentage", GlobalConstants.DefaultDataListCacheSizeLvlMemoryPercentage }, { "PollingInterval", GlobalConstants.DefaultDataListCacheSizeLvlMemoryPollingInterval } };

        [NonSerialized]
        static readonly MemoryCache MemoryCache = new MemoryCache("DataList",MemoryCacheConfiguration);

        [NonSerialized]
        static readonly CacheItemPolicy CacheItemPolicy = new CacheItemPolicyDataList(RemovedCallback);

        readonly string _uniqueIndentifier;
        IndexList _populatedKeys;
        [NonSerialized]
        static bool ItemsAddedToRedis;

        [NonSerialized]
        static readonly ConcurrentDictionary<string, IList<IBinaryDataListItem>> LevelOneCache = new ConcurrentDictionary<string, IList<IBinaryDataListItem>>(1,GlobalConstants.DefaultDataListCacheSizeLvl1,StringComparer.Ordinal);

        int _key=Int32.MaxValue;
        string _uniqueKey;
        [NonSerialized]
        static readonly BackgroundWorker BackgroundWorker = new BackgroundWorker();
        [NonSerialized]
        readonly ManualResetEvent _keepRunning = new ManualResetEvent(true);

        String _uniqueIdentifierGuid;

        public Dev2BinaryDataListStorage(string uniqueIndex, Guid uniqueIdentifier)
        {
            if(!BackgroundWorker.IsBusy)
            {
                BackgroundWorker.WorkerSupportsCancellation = true;
                BackgroundWorker.WorkerReportsProgress = false;
                BackgroundWorker.DoWork += MoveItemsIntoMemoryCacheBackground;
                BackgroundWorker.RunWorkerAsync();
            }
            _uniqueIdentifierGuid = uniqueIdentifier.ToString();
            _uniqueIndentifier = uniqueIndex + _uniqueIdentifierGuid;
            _populatedKeys = new IndexList();
        }

        void MoveItemsIntoMemoryCacheBackground(object sender, DoWorkEventArgs e)
        {
            while(_keepRunning.WaitOne())
            {
                MoveItemsIntoMemoryCache();
            }
        }

        static void MoveItemsIntoMemoryCache()
        {
            if(BackgroundWorker.CancellationPending) return;
            if(LevelOneCache.Count >= GlobalConstants.DefaultDataListMaxCacheSizeLvl1)
            {
                    var keys = LevelOneCache.Keys.ToList();
                    keys.ForEach(s =>
                    {
                        IList<IBinaryDataListItem> list;
                        LevelOneCache.TryRemove(s, out list);
                        MemoryCache.Set(s, list, CacheItemPolicy);
                    });
            }
        }


        string GetUniqueKey(int key)
        {
            if(_key != key)
            {
                _key = key;
                _uniqueKey=_key.ToString(CultureInfo.InvariantCulture) + _uniqueIndentifier;
            }
            return _uniqueKey;
        }

        public IList<IBinaryDataListItem> this[int key]
        {
            get
            {
                IList<IBinaryDataListItem> v;
                TryGetValue(key, out v);
                return v;
            }
            set
            {
                LevelOneCache.AddOrUpdate(GetUniqueKey(key),value,(s,list)=>value);
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
                return MemoryCache.Select(pair => (IList<IBinaryDataListItem>)pair.Value).ToList();
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
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var cacheItem = arguments.CacheItem;
                    var value = cacheItem.Value;
                    var key = cacheItem.Key;
                    if(Dev2RedisClient.RedisClient != null)
                    {
                        Dev2RedisClient.RedisClient.Set(key, value);
                        ItemsAddedToRedis = true;
                    }
                    
                });
            }
        }

        public bool TryGetValue(int key, out IList<IBinaryDataListItem> value)
        {
            bool r = false;
            string uniqueKey = GetUniqueKey(key);
            if(LevelOneCache.TryGetValue(uniqueKey, out value))
            {
                r = true;
            }
            else
            {
                value = (IList<IBinaryDataListItem>)MemoryCache.Get(uniqueKey);
            }
            if(value!=null)
            {
                r = true;
            }
            else
            {
                if(ItemsAddedToRedis)
                {
                    if(Dev2RedisClient.RedisClient != null)
                    {
                        value = Dev2RedisClient.RedisClient.Get<List<IBinaryDataListItem>>(uniqueKey);
                    }
                }
                if (value == null)
                {
                    value = new List<IBinaryDataListItem>();
                    LevelOneCache.AddOrUpdate(uniqueKey, value, (s, list) => list);
                }
                else
                {
                    r = true;
                    if(_populatedKeys.Contains(key))
                    {
                        try
                        {
                            LevelOneCache.AddOrUpdate(uniqueKey, value, (s, list) => list);
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
                        r = false;
                        value = null;
                    }
                }
            }
            return r;
        }

        public void Add(int key, IList<IBinaryDataListItem> value)
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
            IList<IBinaryDataListItem> list;
            LevelOneCache.TryRemove(uniqueKey, out list);
            list = null;
            MemoryCache.Remove(uniqueKey);
            if(Dev2RedisClient.RedisClient != null)
            {
                Dev2RedisClient.RedisClient.Remove(uniqueKey);
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
            DisposeRedis();
        }

        void DisposeWaitHandle()
        {
            _keepRunning.Reset();
        }

        void DisposeRedis()
        {
            if(Dev2RedisClient.RedisClient == null)return;
            RemoveFromLevelThreeCache();
        }

        void DisposeMemoryCache()
        {
            if(MemoryCache == null) return;
            RemoveFromLevelTwoCache();
        }

        static void RemoveCachePolicyEvent()
        {
            if(CacheItemPolicy == null) return;
            CacheItemPolicy.RemovedCallback -= RemovedCallback;
        }

        void DisposeBackgroundWorker()
        {
            if(BackgroundWorker==null) return;
            if(BackgroundWorker.WorkerSupportsCancellation)
            {
                BackgroundWorker.CancelAsync();
            }
        }

        void RemoveFromLevelOneCache()
        {
            List<string> keysToRemove = LevelOneCache.Keys
                                        .Where(key => 
                                          key.Contains(_uniqueIndentifier))
                                        .ToList();
            if(keysToRemove.Count != 0)
            {
                IList<IBinaryDataListItem> list;
                keysToRemove.ForEach(keyToRemove => LevelOneCache.TryRemove(keyToRemove, out list));
            }
        }      
        void RemoveFromLevelTwoCache()
        {
            List<string> keysToRemove = MemoryCache.Select(pair => pair.Key.Contains(_uniqueIdentifierGuid) ? pair.Key : null).ToList();
            if(keysToRemove.Count != 0)
            {
                keysToRemove.ForEach(keyToRemove =>
                {
                    if(!String.IsNullOrEmpty(keyToRemove))
                    {
                        MemoryCache.Remove(keyToRemove);
                    }
                });
            }
        }

        void RemoveFromLevelThreeCache()
        {
            if(Dev2RedisClient.RedisClient != null)
            {
                List<string> keysToRemove = Dev2RedisClient.RedisClient.GetAllKeys().Where(pair => pair.Contains(_uniqueIndentifier)).ToList();
                Dev2RedisClient.RedisClient.RemoveAll(keysToRemove);
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