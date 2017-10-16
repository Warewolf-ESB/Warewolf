using System.Activities.Presentation.Model;
using System.Windows;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictNode
    {
        ModelItem CurrentActivity { get; set; }
        ModelItem CurrentFlowStep { get; set; }
        Point NodeLocation { get; set; }
        int TreeIndex { get; set; }
    }
}