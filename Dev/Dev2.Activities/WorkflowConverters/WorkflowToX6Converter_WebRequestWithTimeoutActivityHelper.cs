using Dev2.Activities;
using Dev2.Common.X6;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateWebRequestWithTimeoutActivity(DsfWebGetRequestWithTimeoutActivity activity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = activity.DisplayName ?? Constants.DISPLAYNAME_WEBREQUEST,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            activity.ToX6Json(cell);

            return cell;
        }
    }
}
