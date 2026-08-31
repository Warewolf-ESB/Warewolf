using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System;
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
        /// <summary>
        /// Embeds nested activities into their parent ForEach activities using the generic
        /// <c>data.isNested</c>/<c>data.parentId</c> mechanism that every other container
        /// (Sequence, Select-and-apply, Gate, Redis Cache, Suspend Execution, Manual Resumption)
        /// already uses, and that <c>body_schema</c>/<c>get_tool_schema</c> tell MCP callers to
        /// author. Before this, ForEach read only the legacy <c>isNestedInForEach</c>/
        /// <c>forEachParentId</c> keys, so a body authored per the published schema had its loop
        /// body silently dropped and the engine failed at execution with
        /// "Cannot execute a For Each with no content".
        /// </summary>
        /// <param name="allNodes">All nodes from the X6 graph</param>
        private void EmbedNestedActivitiesIntoForEachActivities(List<Cell> allNodes)
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

                if (parentActivity is not DsfForEachActivity forEach)
                    continue;

                // Order child cells by their index property, as Sequence does
                var childActivities = childCells
                    .OrderBy(c => TryGetIndex(c.data))
                    .Select(c => activityMap.TryGetValue(c.id, out var a) ? a : null)
                    .Where(a => a != null)
                    .ToList();

                if (childActivities.Count == 0)
                    continue;

                foreach (var childActivity in childActivities)
                {
                    if (!this.nestedActivites.Contains(childActivity))
                        this.nestedActivites.Add(childActivity);
                }

                // A ForEach runs exactly one handler, so a multi-step loop body is wrapped in a
                // Sequence rather than losing every step after the first.
                var body = childActivities.Count == 1
                    ? childActivities[0]
                    : WrapForEachBodyInSequence(childActivities);

                if (forEach.DataFunc == null)
                {
                    forEach.DataFunc = new ActivityFunc<string, bool>
                    {
                        DisplayName = Constants.DATAACTION,
                        Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                    };
                }

                forEach.DataFunc.Handler = body;
            }
        }

        /// <summary>
        /// Wraps a multi-activity ForEach body in a Sequence so all of its steps survive the
        /// conversion, keeping the order the caller authored.
        /// </summary>
        private Activity WrapForEachBodyInSequence(List<Activity> childActivities)
        {
            var sequence = new DsfSequenceActivity
            {
                DisplayName = Constants.DATAACTION,
                UniqueID = Guid.NewGuid().ToString()
            };

            foreach (var childActivity in childActivities)
            {
                sequence.Activities.Add(childActivity);
            }

            if (!this.nestedActivites.Contains(sequence))
                this.nestedActivites.Add(sequence);

            return sequence;
        }
    }
}
