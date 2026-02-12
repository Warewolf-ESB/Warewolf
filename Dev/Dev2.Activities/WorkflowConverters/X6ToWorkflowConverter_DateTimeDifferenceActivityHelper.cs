using Dev2.Activities.DateAndTime;
using Dev2.Common;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfDateTimeDifferenceActivity CreateDateTimeDifferenceActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDateTimeDifferenceActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfDotNetDateTimeDifferenceActivity CreateDotNetDateTimeDifferenceActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetDateTimeDifferenceActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
