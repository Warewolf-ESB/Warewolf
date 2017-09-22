using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;

namespace Dev2.Studio.Interfaces
{
    public interface IServiceDifferenceParser
    {
      

        (ModelItem current, ModelItem difference, List<KeyValuePair<Guid, bool>> differenceStore) GetDifferences(IContextualResourceModel current, IContextualResourceModel difference);
    }
}