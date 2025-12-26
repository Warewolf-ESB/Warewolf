using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateRecordsetLengthActivity(DsfRecordsetNullhandlerLengthActivity recordsetLengthActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = recordsetLengthActivity.DisplayName ?? Constants.DISPLAYNAME_LENGTH,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            recordsetLengthActivity.ToX6Json(cell);

            return cell;
        }
    }
}
