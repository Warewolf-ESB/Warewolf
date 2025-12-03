using Dev2.Activities.RedisCache;
using Dev2.Common;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static RedisCacheActivity CreateRedisCacheActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new RedisCacheActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}