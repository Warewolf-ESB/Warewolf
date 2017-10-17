using Dev2;
using Dev2.Studio.Interfaces;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;

namespace Warewolf.MergeParser
{
    public class ConflictNode : IConflictNode
    {
        readonly IDev2Activity _activity;

        public IDev2Activity Activity => _activity;

        public ConflictNode(IDev2Activity activity)
        {
            _activity = activity;
        }

        public IEnumerable<IDev2Activity> GetNextNodes()
        {
            return _activity.NextNodes;
        }

        public Dictionary<string, IEnumerable<IDev2Activity>> GetChildrenNodes()
        {
            return _activity.GetChildrenNodes();
        }
        public ModelItem CurrentActivity { get; set; }
        public ModelItem CurrentFlowStep { get; set; }
        public bool HasConflict { get; set; }
        public Point NodeLocation { get; set; }
        public int TreeIndex { get; set; }
    }
}
