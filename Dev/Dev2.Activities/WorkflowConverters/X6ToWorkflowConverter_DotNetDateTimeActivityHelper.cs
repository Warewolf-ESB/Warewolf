using Dev2.Activities.DateAndTime;
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
        private static DsfDotNetDateTimeActivity CreateDotNetDateTimeActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetDateTimeActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfDateTimeActivity CreateDateTimeActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDateTimeActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
