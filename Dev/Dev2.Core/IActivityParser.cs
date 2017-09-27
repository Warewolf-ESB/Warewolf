using System.Activities;
using System.Collections.Generic;

namespace Dev2
{
    public interface IActivityParser
    {
        IDev2Activity Parse(DynamicActivity dynamicActivity);
        IDev2Activity Parse(List<IDev2Activity> seenActivities, object flowChart);
        IEnumerable<IDev2Activity> ParseToLinkedFlatList(IDev2Activity topLevelActivity);
        IEnumerable<IDev2Activity> FlattenNextNodesExclusive(IDev2Activity firstOrDefault);
        IEnumerable<IDev2Activity> ParseFalseArmToFlatList(IDev2Activity firstOrDefault);
    }
}