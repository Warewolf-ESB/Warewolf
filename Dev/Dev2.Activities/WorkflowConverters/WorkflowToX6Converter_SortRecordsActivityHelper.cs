using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDsfSortRecordsActivity(DsfSortRecordsActivity sortRecordActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = sortRecordActivity.DisplayName ?? Constants.DISPLAYNAME_SORTACTIVITY,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            sortRecordActivity.ToX6Json(cell);

            return cell;
        }
    }
}