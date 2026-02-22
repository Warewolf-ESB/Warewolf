using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfDotNetGatherSystemInformationActivity CreateDotNetGatherSystemInformationActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetGatherSystemInformationActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfGatherSystemInformationActivity CreateGatherSystemInformationActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfGatherSystemInformationActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
