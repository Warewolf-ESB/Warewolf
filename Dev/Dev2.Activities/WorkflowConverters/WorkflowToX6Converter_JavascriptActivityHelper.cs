using Dev2.Activities.Scripting;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateJavascriptActivity(DsfJavascriptActivity javascriptActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = javascriptActivity.DisplayName ?? Constants.DISPLAYNAME_JAVASCRIPT,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            javascriptActivity.ToX6Json(cell);

            return cell;
        }
    }
}
