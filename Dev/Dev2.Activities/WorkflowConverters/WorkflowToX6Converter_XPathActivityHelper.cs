using Dev2.Common.X6;
using System.Collections.Generic;
using Dev2.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateXPathActivity(DsfXPathActivity xpathActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = xpathActivity.DisplayName ?? Constants.DISPLAYNAME_XPATH,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            xpathActivity.ToX6Json(cell);

            return cell;
        }
    }
}
