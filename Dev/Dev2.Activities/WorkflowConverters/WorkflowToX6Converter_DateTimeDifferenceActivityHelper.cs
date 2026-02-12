using Dev2.Activities.DateAndTime;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDateTimeDifferenceActivity(DsfDotNetDateTimeDifferenceActivity dateTimeDifferenceActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = dateTimeDifferenceActivity.DisplayName ?? Constants.DISPLAYNAME_DATETIMEDIFFERENCE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            dateTimeDifferenceActivity.ToX6Json(cell);

            return cell;
        }
    }
}
