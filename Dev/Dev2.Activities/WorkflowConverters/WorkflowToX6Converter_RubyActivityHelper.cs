using Dev2.Activities.Scripting;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateRubyActivity(DsfRubyActivity rubyActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = rubyActivity.DisplayName ?? Constants.DISPLAYNAME_RUBY,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            rubyActivity.ToX6Json(cell);

            return cell;
        }
    }
}
