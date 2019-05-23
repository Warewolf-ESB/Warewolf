#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        public ConcurrentDictionary<Guid, IDev2Activity> Cache => _cache;

        public ResourceActivityCache(IActivityParser activityParser, ConcurrentDictionary<Guid, IDev2Activity> cache)
        {
            _activityParser = activityParser ?? throw new ArgumentNullException(nameof(activityParser));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid) => Parse(activity, resourceIdGuid, false);

        public IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid, bool failOnError)
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
                    if (Cache.TryAdd(resourceIdGuid, act))
                    {
                        return act;
                    }
                    Cache.AddOrUpdate(resourceIdGuid, act, (guid, dev2Activity) =>
                    {
                        Cache[resourceIdGuid] = act;
                        return act;
                    });
                    return act;
                }                    
                catch(Exception err) //errors caught inside                    
                {
                    Dev2Logger.Error(err, "Warewolf Error");
                    if(failOnError)
                    {
                        throw;
                    }
                }   
            }
            return null;
        }

        public IDev2Activity GetActivity(Guid resourceIdGuid) => Cache[resourceIdGuid];

        public bool HasActivityInCache(Guid resourceIdGuid) => Cache.ContainsKey(resourceIdGuid);

        public void RemoveFromCache(Guid resourceID)
        {
            Cache.TryRemove(resourceID, out IDev2Activity act);
        }
    }
}