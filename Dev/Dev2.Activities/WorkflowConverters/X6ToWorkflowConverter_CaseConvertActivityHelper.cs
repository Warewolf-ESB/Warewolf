using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Helper for converting X6 JSON to DsfCaseConvertActivity
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfCaseConvertActivity CreateCaseConvertActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfCaseConvertActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
