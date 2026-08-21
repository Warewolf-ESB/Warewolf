using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Creates Workflow from X6 Json data (nodes, edges and common data) 
    /// </summary>
    public partial class X6ToWorkflowConverter
    {
        private static DsfSequenceActivity CreateSequenceActivity(Cell node)
        {
            // Try both camelCase and lowercase variations for compatibility
            var hasDisplayName = node.data.TryGetString("displayName", out var displayName) ||
                                 node.data.TryGetString(Constants.DISPLAYNAME, out displayName);

            if (!hasDisplayName || string.IsNullOrWhiteSpace(displayName))
                return null;

            var activity = new DsfSequenceActivity();
            activity.FromX6Json(node);
            return activity;
        }

        private void EmbedNestedActivitiesIntoSequenceActivities(List<Cell> allNodes)
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

                if (parentActivity is not DsfSequenceActivity sequence)
                    continue;

                // Order child cells by their index property
                var orderedChildren = childCells
                    .OrderBy(c => TryGetIndex(c.data))
                    .ToList();

                foreach (var childCell in orderedChildren)
                {
                    if (activityMap.TryGetValue(childCell.id, out var childActivity))
                    {
                        sequence.Activities.Add(childActivity);
                        if (!this.nestedActivites.Contains(childActivity))
                            this.nestedActivites.Add(childActivity);
                    }
                }
            }
        }

        private static int TryGetIndex(Dictionary<string, object> data)
        {
            if (data.TryGetInt(Constants.SEQUENCE_NESTED_ACTIVITY_INDEX, out var index)) return index;
            return int.MaxValue;
        }
    }
}
