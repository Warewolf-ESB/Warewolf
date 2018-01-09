using System;
using System.Activities;
using System.Collections.Concurrent;
using Dev2;
using Dev2.Common;

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

        public IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid) => Parse(activity, resourceIdGuid, false);

        public IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid, bool failOnException)
        {
            if(HasActivityInCache(resourceIdGuid))
            {
                var dev2Activity = GetActivity(resourceIdGuid);
                if (dev2Activity != null)
                {
                    return dev2Activity;
                }
            }
            var dynamicActivity = activity;
            if (dynamicActivity != null)
            {
                try
                {
                    var act = _activityParser.Parse(dynamicActivity);
                    if (_cache.TryAdd(resourceIdGuid, act))
                    {
                        return act;
                    }
                    _cache.AddOrUpdate(resourceIdGuid, act, (guid, dev2Activity) =>
                    {
                        _cache[resourceIdGuid] = act;
                        return act;
                    });
                    return act;
                }                    
                catch(Exception err) //errors caught inside                    
                {
                    Dev2Logger.Error(err, "Warewolf Error");
                    if(failOnException)
                    {
                        throw;
                    }
                }   
            }
            return null;
        }

        public IDev2Activity GetActivity(Guid resourceIdGuid)
        {
            return _cache[resourceIdGuid];
        }

        public bool HasActivityInCache(Guid resourceIdGuid)
        {
            return _cache.ContainsKey(resourceIdGuid);
        }

        public void RemoveFromCache(Guid resourceID)
        {
            _cache.TryRemove(resourceID, out IDev2Activity act);
        }
    }
}