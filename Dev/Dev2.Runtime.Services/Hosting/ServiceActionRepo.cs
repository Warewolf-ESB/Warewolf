using System;
using System.Collections.Generic;
using Dev2.DynamicServices;

namespace Dev2.Runtime.Hosting
{
    public class ServiceActionRepo
    {
        static readonly Lazy<ServiceActionRepo> _instance = new Lazy<ServiceActionRepo>(()=>
        {
            return new ServiceActionRepo();
        },System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        readonly Dictionary<Guid,DynamicService> _actionsCache;

        public void AddToCache(Guid key, DynamicService value)
        {
            if (_actionsCache.ContainsKey(key))
            {
                _actionsCache.Remove(key);
            }
            _actionsCache.Add(key,value);
        }

        public DynamicService ReadCache(Guid key)
        {
            if (_actionsCache.ContainsKey(key))
            {
                return _actionsCache[key];
            }
            return null;
        }

        public static ServiceActionRepo Instance => _instance.Value;

        //Private constructor to ensure that Singleton is used
        ServiceActionRepo()
        {
            _actionsCache = new Dictionary<Guid, DynamicService>();
        }

        public void RemoveFromCache(Guid key)
        {
            if (_actionsCache.ContainsKey(key))
            {
                _actionsCache.Remove(key);
            }
        }
    }
}