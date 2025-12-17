using Dev2.Activities.RedisRemove;
using Dev2.Common;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static RedisRemoveActivity CreateRedisRemoveActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new RedisRemoveActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}