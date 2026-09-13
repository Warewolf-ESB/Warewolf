using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.X6;

using Dev2.WorkflowConverters;
namespace Dev2.Activities.WF
{ 
    public partial class X6ToWorkflowConverter
    {
        private static PublishRabbitMQActivity CreateRabbitMQPublishActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new PublishRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfPublishRabbitMQActivity CreateDsfRabbitMQPublishActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfPublishRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}