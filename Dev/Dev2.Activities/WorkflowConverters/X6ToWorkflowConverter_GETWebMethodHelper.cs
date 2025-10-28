using Dev2.Activities;
using Dev2.Common.X6;
using System;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static WebGetActivity CreateWebGetActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new WebGetActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
