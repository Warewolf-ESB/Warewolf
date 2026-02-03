using Dev2.Common.X6;
using Dev2.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateGateActivity(GateActivity gateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = gateActivity.DisplayName ?? Constants.DISPLAYNAME_GATE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            gateActivity.ToX6Json(cell);

            return cell;
        }
    }
}
