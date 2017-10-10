using System;
using System.Activities;
using Dev2;

namespace Warewolf.ResourceManagement
{
    public interface IResourceActivityCache
    {
        void AddToCache(Guid resourceID, DynamicActivity activity);

        IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid);

        IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid, bool failOnError);

        void RemoveFromCache(Guid resourceID);

        bool HasActivityInCache(Guid resourceIdGuid);

        IDev2Activity GetActivity(Guid resourceIdGuid);
    }
}