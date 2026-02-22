using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDotNetAggregateCalculateActivity(DsfDotNetAggregateCalculateActivity aggregateCalculateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = aggregateCalculateActivity.DisplayName ?? Constants.DISPLAYNAME_AGGREGATECALCULATE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            aggregateCalculateActivity.ToX6Json(cell);

            return cell;
        }


        public Cell CreateAggregateCalculateActivity(DsfAggregateCalculateActivity aggregateCalculateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = aggregateCalculateActivity.DisplayName ?? Constants.DISPLAYNAME_AGGREGATECALCULATE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            aggregateCalculateActivity.ToX6Json(cell);

            return cell;
        }
    }
}
