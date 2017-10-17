using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictNode
    {
        ModelItem CurrentActivity { get; set; }
        ModelItem CurrentFlowStep { get; set; }
        Point NodeLocation { get; set; }
        int TreeIndex { get; set; }

        IDev2Activity Activity { get; }
        IEnumerable<IDev2Activity> GetNextNodes();
        Dictionary<string, IEnumerable<IDev2Activity>> GetChildrenNodes();
    }
}