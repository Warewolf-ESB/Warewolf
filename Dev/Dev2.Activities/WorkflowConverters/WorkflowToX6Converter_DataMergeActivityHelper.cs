using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDataMergeActivity(DsfDataMergeActivity dataMergeActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = dataMergeActivity.DisplayName ?? Constants.DISPLAYNAME_DATAMERGE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            dataMergeActivity.ToX6Json(cell);

            return cell;
        }
    }
}
