using Dev2.Common.X6;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateWebDeleteActivity(DsfWebDeleteActivity webDeleteActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = webDeleteActivity.DisplayName ?? Constants.DISPLAYNAME_WEBDELETE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            webDeleteActivity.ToX6Json(cell);

            return cell;
        }
    }
}