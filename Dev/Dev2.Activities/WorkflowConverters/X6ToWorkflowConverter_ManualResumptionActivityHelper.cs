using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.WF
{
    public partial class X6ToWorkflowConverter
    {
        private static ManualResumptionActivity CreateManualResumptionActivity(Cell node)
        {
            var hasDisplayName = node.data.TryGetString(Constants.DISPLAYNAME, out var displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new ManualResumptionActivity();
            activity.FromX6Json(node);
            return activity;
        }


        private void EmbedNestedActivitiesIntoManualResumptionActivities(List<Cell> allNodes)
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

                if (parentActivity is not ManualResumptionActivity manualResumptionActivity)
                    continue;

                var childCell = childCells.First();

                if (activityMap.TryGetValue(childCell.id, out var childActivity))
                {

                    if (childActivity != null)
                    {
                        // Initialize ApplyActivityFunc if it doesn't exist
                        if (manualResumptionActivity.OverrideDataFunc == null)
                        {
                            manualResumptionActivity.OverrideDataFunc = new ActivityFunc<string, bool>
                            {
                                DisplayName = Constants.DATAACTION,
                                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}"),
                                Handler = new DsfSequenceActivity(),
                            };
                        }

                        manualResumptionActivity.OverrideDataFunc.Handler = childActivity;
                    }

                    if (!this.nestedActivites.Contains(childActivity))
                        this.nestedActivites.Add(childActivity);
                }
            }
        }
    }
}