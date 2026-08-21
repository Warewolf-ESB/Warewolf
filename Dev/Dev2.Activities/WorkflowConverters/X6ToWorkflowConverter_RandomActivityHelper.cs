using Dev2.Common.X6;

using Dev2.WorkflowConverters;
namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data)
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfRandomActivity CreateRandomActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfRandomActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
