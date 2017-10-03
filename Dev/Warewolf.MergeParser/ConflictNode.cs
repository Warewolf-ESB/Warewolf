using Dev2.Studio.Interfaces;
using System.Activities.Presentation.Model;
using System.Windows;

namespace Warewolf.MergeParser
{
    public class ConflictNode : IConflictNode
    {
        public ModelItem CurrentActivity { get; set; }
        public ModelItem CurrentFlowStep { get; set; }
        public bool HasConflict { get; set; }
        public Point NodeLocation { get; set; }
    }
}
