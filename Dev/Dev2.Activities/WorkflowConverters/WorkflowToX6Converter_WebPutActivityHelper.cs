using Dev2.Activities;
using Dev2.Common.X6;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateWebPutActivity(WebPutActivity webPutActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = webPutActivity.DisplayName ?? Constants.DISPLAYNAME_WEBPUT,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            webPutActivity.ToX6Json(cell);

            return cell;
        }
    }
}
