using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDsfConsumeRabbitMQActivity(DsfConsumeRabbitMQActivity rabbitMQConsumeActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = rabbitMQConsumeActivity.DisplayName ?? Constants.DISPLAYNAME_DELETERECORDS,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            rabbitMQConsumeActivity.ToX6Json(cell);

            return cell;
        }
 
    }
}