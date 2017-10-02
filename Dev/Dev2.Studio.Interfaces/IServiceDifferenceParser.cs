using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceDifferenceParser 
    {
        List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference, bool loadDiffFromLoacalServer = true);
        Stack<ModelItem> GetAllNodes();
    }
}