using Dev2.Common;
using Dev2.Common.X6;
using Dev2.Activities;

using Dev2.WorkflowConverters;
namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static AdvancedRecordsetActivity CreateAdvancedRecordsetActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new AdvancedRecordsetActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
