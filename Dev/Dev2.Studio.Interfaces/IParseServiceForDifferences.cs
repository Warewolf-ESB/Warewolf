using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;

namespace Dev2.Studio.Interfaces
{
    public interface IParseServiceForDifferences
    {
      

        List<(Guid uniqueId, ModelItem current, ModelItem difference, bool conflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference);
    }
}