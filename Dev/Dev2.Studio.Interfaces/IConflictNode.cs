using System.Activities.Presentation.Model;
using System.Collections.Generic;
#if WINDOWS
using System.Windows;
#endif

namespace Dev2.Studio.Interfaces
{
    public interface IConflictNode
    {
        ModelItem CurrentFlowStep { get; set; }
#if WINDOWS
        Point NodeLocation { get; set; }
#endif
        int TreeIndex { get; set; }

        IDev2Activity Activity { get; }
        ModelItem CurrentActivity { get; }

        IEnumerable<IDev2Activity> GetNextNodes();
        IEnumerable<IDev2Activity> GetChildrenNodes();
    }
}