using System;
using System.Activities;
using System.Collections.Generic;

namespace Dev2
{
    public interface IActivityParser
    {
        IDev2Activity Parse(DynamicActivity dynamicActivity);
        IDev2Activity Parse(List<IDev2Activity> seenActivities, object flowChart);
        IEnumerable<IDev2Activity> ParseToLinkedFlatList(IDev2Activity topLevelActivity);
        IEnumerable<IDev2Activity> FlattenNextNodesInclusive(IDev2Activity firstOrDefault);
        IDev2Activity ParseWithCache(DynamicActivity dynamicActivity, Guid workspaceID, Guid resourceIdGuid);
        IDev2Activity GetActivityFromCache(Guid workspaceID, Guid resourceIdGuid);
        bool RemoveFromSerializedActivityCache(Guid workspaceID, Guid resourceIdGuid);
        void ClearSerializedActivityCache();
    }
}