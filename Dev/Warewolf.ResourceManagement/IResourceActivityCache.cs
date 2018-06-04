using System;
using System.Activities;
using System.Collections.Concurrent;
using Dev2;

namespace Warewolf.ResourceManagement
{
    public interface IResourceActivityCache
    {
        IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid);

        IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid, bool failOnError);

        void RemoveFromCache(Guid resourceID);

        bool HasActivityInCache(Guid resourceIdGuid);

        IDev2Activity GetActivity(Guid resourceIdGuid);

        ConcurrentDictionary<Guid, IDev2Activity> Cache { get; }
    }
}