using System;
using System.Activities;
using System.Collections.Concurrent;
using Dev2;

namespace Warewolf.ResourceManagement
{
    public  class ResourceActivityCache : IResourceActivityCache
    {
        readonly IActivityParser _activityParser;
        readonly ConcurrentDictionary<Guid,IDev2Activity> _cache;

        public ResourceActivityCache(IActivityParser activityParser, ConcurrentDictionary<Guid, IDev2Activity> cache)
        {
            _activityParser = activityParser;
            _cache = cache;
        }        

        public IDev2Activity Parse(Func<DynamicActivity> actFunc,Guid resourceIdGuid)
        {
            if(_cache.ContainsKey(resourceIdGuid))
            {
                return _cache[resourceIdGuid];
            }
            var dynamicActivity = actFunc();
            if (dynamicActivity != null)
            {
                IDev2Activity act = _activityParser.Parse(dynamicActivity);
                if (_cache.TryAdd(resourceIdGuid, act))
                {
                    return act;
                }
            }
            return null;
        }

        public void RemoveFromCache(Guid resourceID)
        {
            IDev2Activity act;
            _cache.TryRemove(resourceID, out act);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        public bool HasId(Guid resourceID)
        {
            return _cache.ContainsKey(resourceID);
        }
    }
}