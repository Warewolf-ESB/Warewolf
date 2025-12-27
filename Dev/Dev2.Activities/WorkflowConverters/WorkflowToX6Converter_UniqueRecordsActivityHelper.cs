using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDsfUniqueRecordsActivity(DsfUniqueActivity uniqueRecordActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = uniqueRecordActivity.DisplayName ?? Constants.DISPLAYNAME_UNIQUEACTIVITY,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            uniqueRecordActivity.ToX6Json(cell);

            return cell;
        }
    }
}