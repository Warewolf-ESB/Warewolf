using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static SuspendExecutionActivity CreateSuspendExecutionActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new SuspendExecutionActivity();
            activity.FromX6Json(node);
            return activity;
        }

        /// <summary>
        /// Embeds nested activities into their parent Suspend Execution activities' SaveDataFunc.Handler property
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        private void EmbedNestedActivitiesIntoSuspendExecutionActivities(List<Cell> allNodes)
        {
            if (allNodes == null || allNodes.Count == 0)
                return;

            var parentChildMap = CellOrganizer.BuildHierarchy(allNodes);

            foreach (var (parentCell, childCells) in parentChildMap)
            {
                if (childCells == null || childCells.Count == 0)
                    continue;

                if (!activityMap.TryGetValue(parentCell.id, out var parentActivity))
                    continue;

                if (parentActivity is not SuspendExecutionActivity suspendExecutionActivity)
                    continue;

                // Get the first child cell (SaveDataFunc should only have one handler)
                var childCell = childCells.First();

                if (activityMap.TryGetValue(childCell.id, out var childActivity))
                {
                    if (childActivity != null)
                    {
                        // Initialize SaveDataFunc if it doesn't exist
                        if (suspendExecutionActivity.SaveDataFunc == null)
                        {
                            suspendExecutionActivity.SaveDataFunc = new ActivityFunc<string, bool>
                            {
                                DisplayName = Constants.DATAACTION,
                                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                            };
                        }

                        // Set the nested activity as the handler
                        suspendExecutionActivity.SaveDataFunc.Handler = childActivity;
                    }

                    if (!this.nestedActivites.Contains(childActivity))
                        this.nestedActivites.Add(childActivity);
                }
            }
        }
    }
}
