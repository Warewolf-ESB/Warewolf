using Dev2.Activities.SelectAndApply;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateSelectAndApplyActivity(DsfSelectAndApplyActivity selectAndApplyActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = selectAndApplyActivity.DisplayName ?? Constants.DISPLAYNAME_SELECTANDAPPLY,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            selectAndApplyActivity.ToX6Json(cell);

            return cell;
        }

        private string ProcessDsfSelectAndApplyActivity(DsfSelectAndApplyActivity selectAndApplyActivity, X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {

            {
                // The nodeId should already be generated and stored in activityNodeMap by ProcessActivity
                if (!activityNodeMap.TryGetValue(selectAndApplyActivity, out var selectAndApplyNodeId))
                {
                    selectAndApplyNodeId = CommonHelper.GenerateNodeId();
                    activityNodeMap[selectAndApplyActivity] = selectAndApplyNodeId;
                }

                // Create the Select and apply node - the ToX6Json method will handle embedding child activities
                var selectAndApplyNode = CreateSelectAndApplyActivity(selectAndApplyActivity, selectAndApplyNodeId);
                selectAndApplyNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(selectAndApplyActivity));

                graphData.Nodes.Add(selectAndApplyNode);
                graphData.ActivityNodeMap[selectAndApplyActivity] = selectAndApplyNode;

                // Create edge from previous node to this Select and apply node
                if (!string.IsNullOrEmpty(previousNodeId))
                {
                    graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, selectAndApplyNodeId));
                }

                // Process nested activities from DsfSelectAndApplyActivity.ApplyActivityFunc
                ProcessSelectAndApplyNestedActivities(selectAndApplyActivity, selectAndApplyNodeId, graphData, activityNodeMap);

                return selectAndApplyNodeId;
            }
        }

        /// <summary>
        /// Processes nested activities within a Select and apply activity and creates separate X6 nodes for them
        /// </summary>
        /// <param name="selectAndApplyActivity">The Select and apply activity containing nested activities</param>
        /// <param name="selectAndApplyNodeId">The node ID of the parent activity</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        private void ProcessSelectAndApplyNestedActivities(DsfSelectAndApplyActivity selectAndApplyActivity, string selectAndApplyNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {

            // Check if there's a nested activity in the DataFunc.Handler
            var nestedActivity = selectAndApplyActivity.ApplyActivityFunc?.Handler;
            if (nestedActivity == null) return;

            // Process Nested Activity
            ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

            // If node is not created, skip further processing
            if (!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) return;

            // Generate a unique node ID for the nested activity
            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            // Add nesting metadata to indicate this activity is nested within the Select and apply
            nestedNode.data[Constants.ISNESTED] = true;
            nestedNode.data[Constants.PARENTID] = selectAndApplyNodeId;
        }
    }
}
