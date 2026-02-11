using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateSuspendExecutionActivity(SuspendExecutionActivity suspendExecutionActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = suspendExecutionActivity.DisplayName ?? Constants.DISPLAYNAME_SUSPENDEXECUTION,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            suspendExecutionActivity.ToX6Json(cell);

            return cell;
        }

        private string ProcessSuspendExecutionActivity(SuspendExecutionActivity suspendExecutionActivity, X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            // The nodeId should already be generated and stored in activityNodeMap by ProcessActivity
            if (!activityNodeMap.TryGetValue(suspendExecutionActivity, out var suspendExecutionNodeId))
            {
                suspendExecutionNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[suspendExecutionActivity] = suspendExecutionNodeId;
            }

            // Create the Suspend Execution node - the ToX6Json method will handle embedding child activities
            var suspendExecutionNode = CreateSuspendExecutionActivity(suspendExecutionActivity, suspendExecutionNodeId);
            suspendExecutionNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(suspendExecutionActivity));

            graphData.Nodes.Add(suspendExecutionNode);
            graphData.ActivityNodeMap[suspendExecutionActivity] = suspendExecutionNode;

            // Create edge from previous node to this Suspend Execution node
            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, suspendExecutionNodeId));
            }

            // Process nested activities from SuspendExecutionActivity.SaveDataFunc
            ProcessSuspendExecutionNestedActivities(suspendExecutionActivity, suspendExecutionNodeId, graphData, activityNodeMap);

            return suspendExecutionNodeId;
        }

        /// <summary>
        /// Processes nested activities within a Suspend Execution activity and creates separate X6 nodes for them
        /// </summary>
        /// <param name="suspendExecutionActivity">The Suspend Execution activity containing nested activities</param>
        /// <param name="suspendExecutionNodeId">The node ID of the parent activity</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        private void ProcessSuspendExecutionNestedActivities(SuspendExecutionActivity suspendExecutionActivity, string suspendExecutionNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {
            // Check if there's a nested activity in the SaveDataFunc.Handler
            var nestedActivity = suspendExecutionActivity.SaveDataFunc?.Handler;
            if (nestedActivity == null) return;

            // Process Nested Activity
            ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

            // If node is not created, skip further processing
            if (!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) return;

            // Generate a unique node ID for the nested activity
            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            // Add nesting metadata to indicate this activity is nested within the Suspend Execution
            nestedNode.data[Constants.ISNESTED] = true;
            nestedNode.data[Constants.PARENTID] = suspendExecutionNodeId;
        }
    }
}
