using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateReplaceActivity(DsfReplaceActivity replaceActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = replaceActivity.DisplayName ?? Constants.DISPLAYNAME_REPLACE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            replaceActivity.ToX6Json(cell);

            return cell;
        }
    }
}
