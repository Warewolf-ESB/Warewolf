using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.X6;

using Dev2.WorkflowConverters;
namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static DsfConsumeRabbitMQActivity CreateDsfRabbitMQConsumeActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfConsumeRabbitMQActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}