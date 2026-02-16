using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDotNetCalculateActivity(DsfDotNetCalculateActivity calculateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = calculateActivity.DisplayName ?? Constants.DISPLAYNAME_CALCULATE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            calculateActivity.ToX6Json(cell);

            return cell;
        }

        public Cell CreateCalculateActivity(DsfCalculateActivity calculateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = calculateActivity.DisplayName ?? Constants.DISPLAYNAME_CALCULATE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            calculateActivity.ToX6Json(cell);

            return cell;
        }
    }
}
