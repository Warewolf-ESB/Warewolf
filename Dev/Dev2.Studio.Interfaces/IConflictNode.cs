using System.Collections.Generic;
#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation.Model;
using System.Windows;
#endif

namespace Dev2.Studio.Interfaces
{
    public interface IConflictNode
    {
#if WINDOWS || NETFRAMEWORK
        ModelItem CurrentFlowStep { get; set; }
        Point NodeLocation { get; set; }
#endif
        int TreeIndex { get; set; }

        IDev2Activity Activity { get; }
#if WINDOWS || NETFRAMEWORK
        ModelItem CurrentActivity { get; }
#endif

        IEnumerable<IDev2Activity> GetNextNodes();
        IEnumerable<IDev2Activity> GetChildrenNodes();
    }
}