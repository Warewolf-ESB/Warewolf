using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

using Dev2.WorkflowConverters;
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
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfCaseConvertActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
