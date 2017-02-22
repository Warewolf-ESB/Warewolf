using System;
using System.Collections.Generic;
using Dev2.DynamicServices;

namespace Dev2.Runtime.Hosting
{
    public class ServiceActionRepo
    {
        readonly Dictionary<Guid,DynamicService> _actionsCache = new Dictionary<Guid, DynamicService>();
        private static ServiceActionRepo _instance;

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

        public static ServiceActionRepo Instance => _instance ?? (_instance = new ServiceActionRepo());

        //Private constructor to ensure that Singleton is used
        private ServiceActionRepo() { }

        public void RemoveFromCache(Guid key)
        {
            if (_actionsCache.ContainsKey(key))
            {
                _actionsCache.Remove(key);
            }
        }
    }
}