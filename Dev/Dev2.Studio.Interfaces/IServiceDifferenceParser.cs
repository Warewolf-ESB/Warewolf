using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceDifferenceParser
    {
        List<(Guid uniqueId, (ModelItem modelItem, Point point), (ModelItem modelItem, Point point), bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference);
    }
}