using Dev2.Common;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

using Dev2.WorkflowConverters;
namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfDotNetAggregateCalculateActivity CreateDotNetAggregateCalculateActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetAggregateCalculateActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfAggregateCalculateActivity CreateAggregateCalculateActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfAggregateCalculateActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
