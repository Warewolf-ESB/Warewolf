using Dev2.Activities.DateAndTime;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDotNetDateTimeActivity(DsfDotNetDateTimeActivity dateTimeActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = dateTimeActivity.DisplayName ?? Constants.DISPLAYNAME_DOTNETDATETIME,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            dateTimeActivity.ToX6Json(cell);

            return cell;
        }

        public Cell CreateDateTimeActivity(DsfDateTimeActivity dateTimeActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = dateTimeActivity.DisplayName ?? Constants.DISPLAYNAME_DOTNETDATETIME,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            dateTimeActivity.ToX6Json(cell);

            return cell;
        }

    }
}
