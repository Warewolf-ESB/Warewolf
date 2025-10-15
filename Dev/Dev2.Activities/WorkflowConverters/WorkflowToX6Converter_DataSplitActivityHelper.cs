using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDataSplitActivity(DsfDataSplitActivity dataSplitActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = dataSplitActivity.DisplayName ?? Constants.DISPLAYNAME_DATASPLIT,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            dataSplitActivity.ToX6Json(cell);

            return cell;
        }
    }
}
