using System;
using System.Collections.Generic;
using System.Threading;
using Dev2.DynamicServices;

namespace Dev2.Runtime.Hosting
{
    public class ServiceActionRepo
    {
        private static bool _isAddingToCache = false;
        static readonly Lazy<ServiceActionRepo> _instance = new Lazy<ServiceActionRepo>(()=>
        {
            return new ServiceActionRepo();
        },System.Threading.LazyThreadSafetyMode.PublicationOnly);
        readonly Dictionary<Guid,DynamicService> _actionsCache;

        public void AddToCache(Guid key, DynamicService value)
        {
            while(_isAddingToCache)
            {
                Thread.Sleep(100);
            }

            try
            {
                _isAddingToCache = true;
                if (_actionsCache.ContainsKey(key))
                {
                    _actionsCache.Remove(key);
                }
                _actionsCache.Add(key,value);
            }
            finally
            {
                _isAddingToCache = false;
            }
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