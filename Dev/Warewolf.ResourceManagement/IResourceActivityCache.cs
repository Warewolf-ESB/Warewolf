using System;
using System.Activities;
using Dev2;

namespace Warewolf.ResourceManagement
{
    public interface IResourceActivityCache
    {
        IDev2Activity Parse(DynamicActivity activity, Guid resourceIdGuid,bool failOnError=false);

        void RemoveFromCache(Guid resourceID);

        bool HasActivityInCache(Guid resourceIdGuid);

        IDev2Activity GetActivity(Guid resourceIdGuid);
    }
}