using System;
using System.Activities;
using Dev2;

namespace Warewolf.ResourceManagement
{
    public interface IResourceActivityCache
    {
        IDev2Activity Parse(Func<DynamicActivity> actFunc,Guid resourceIdGuid);

        bool HasId(Guid resourceID);

        void RemoveFromCache(Guid resourceID);

        void ClearCache();
    }
}