using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static DsfWorkflowActivity CreateDsfWorkflowActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfWorkflowActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
