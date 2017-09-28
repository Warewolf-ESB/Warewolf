using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceDifferenceParser
    {
        List<(Guid uniqueId, ModelItem current, ModelItem difference, bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference);
        Point GetPointForTool(FlowNode flowNode);
    }
}