using Dev2.Activities.SelectAndApply;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System;
using System.Activities.Statements;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfDotNetCalculateActivity CreateDotNetCalculateActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfDotNetCalculateActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private static DsfCalculateActivity CreateCalculateActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfCalculateActivity();
            activity.FromX6Json(node);
            return activity;
        }
    }
}
