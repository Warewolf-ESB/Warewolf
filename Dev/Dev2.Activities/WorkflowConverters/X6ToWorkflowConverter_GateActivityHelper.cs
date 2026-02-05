using Dev2.Common.X6;
using Dev2.Activities;
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
        private static GateActivity CreateGateActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetValue(Constants.DISPLAYNAME, out var displayObject);

            if (!hasDisplayName || displayObject is not string displayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new GateActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private void EmbedNestedActivitiesIntoGateActivities(List<Cell> allNodes)
        {
            if (allNodes == null || allNodes.Count == 0)
                return;

            var parentChildMap = CellOrganizer.BuildHierarchy(allNodes);

            foreach (var kvp in parentChildMap)
            {
                var parentCell = kvp.Key;
                var childCells = kvp.Value;

                if (childCells == null || childCells.Count == 0)
                    continue;

                if (!activityMap.TryGetValue(parentCell.id, out var parentActivity))
                    continue;

                if (parentActivity is not GateActivity gateActivity)
                    continue;

                // Get the first child cell (Gate should only have one nested activity)
                var childCell = childCells.First();

                if (activityMap.TryGetValue(childCell.id, out var childActivity))
                {
                    if (childActivity != null)
                    {
                        // Initialize DataFunc if it doesn't exist
                        if (gateActivity.DataFunc == null)
                        {
                            gateActivity.DataFunc = new ActivityFunc<string, bool>
                            {
                                DisplayName = Constants.DATAACTION,
                                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                            };
                        }

                        // Set the nested activity as the handler
                        gateActivity.DataFunc.Handler = childActivity;
                    }

                    if (!this.nestedActivites.Contains(childActivity))
                        this.nestedActivites.Add(childActivity);
                }
            }
        }
    }
}
