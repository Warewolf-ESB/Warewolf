using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreatePublishRabbitMQActivity(PublishRabbitMQActivity rabbitMQPublishActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = rabbitMQPublishActivity.DisplayName ?? Constants.DISPLAYNAME_DELETERECORDS,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            rabbitMQPublishActivity.ToX6Json(cell);

            return cell;
        }

        public Cell CreateDsfPublishRabbitMQActivity(DsfPublishRabbitMQActivity rabbitMQPublishActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = rabbitMQPublishActivity.DisplayName ?? Constants.DISPLAYNAME_RABBITMQPUBLISH,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            rabbitMQPublishActivity.ToX6Json(cell);

            return cell;
        }
    }
}