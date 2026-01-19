using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static DsfConsumeRabbitMQActivity CreateDsfRabbitMQConsumeActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfConsumeRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}