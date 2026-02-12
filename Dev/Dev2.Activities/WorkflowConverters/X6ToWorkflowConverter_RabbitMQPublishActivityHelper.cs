using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{ 
    public partial class X6ToWorkflowConverter
    {
        private static PublishRabbitMQActivity CreateRabbitMQPublishActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new PublishRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfPublishRabbitMQActivity CreateDsfRabbitMQPublishActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfPublishRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}