using System.Activities;
using Dev2.Common.X6;
using Dev2.Activities;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateAdvancedRecordsetActivity(AdvancedRecordsetActivity advancedRecordsetActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = advancedRecordsetActivity.DisplayName ?? Constants.DISPLAYNAME_ADVANCEDRECORDSET,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            advancedRecordsetActivity.ToX6Json(cell);

            return cell;
        }
    }
}
