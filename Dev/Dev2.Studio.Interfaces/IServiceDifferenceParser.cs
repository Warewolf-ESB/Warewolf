using System;
using System.Activities.Presentation.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceDifferenceParser
    {
        List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference, bool loadDiffFromLoacalServer = true);
        ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)> GetAllNodes();
        bool NodeHasConflict(string uniqueId);
    }
}