using Dev2.Common.X6;
using Dev2.Activities;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateGateActivity(GateActivity gateActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = gateActivity.DisplayName ?? Constants.DISPLAYNAME_GATE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            gateActivity.ToX6Json(cell);

            return cell;
        }

        private string ProcessGateActivity(GateActivity gateActivity, X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            // The nodeId should already be generated and stored in activityNodeMap by ProcessActivity
            if (!activityNodeMap.TryGetValue(gateActivity, out var gateNodeId))
            {
                gateNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[gateActivity] = gateNodeId;
            }

            // Create the Gate node - the ToX6Json method will handle embedding child activities
            var gateNode = CreateGateActivity(gateActivity, gateNodeId);
            gateNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(gateActivity));

            graphData.Nodes.Add(gateNode);
            graphData.ActivityNodeMap[gateActivity] = gateNode;

            // Create edge from previous node to this Gate node
            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, gateNodeId));
            }

            // Process nested activities from GateActivity.DataFunc
            ProcessGateNestedActivities(gateActivity, gateNodeId, graphData, activityNodeMap);

            return gateNodeId;
        }

        /// <summary>
        /// Processes nested activities within a Gate activity and creates separate X6 nodes for them
        /// </summary>
        /// <param name="gateActivity">The Gate activity containing nested activities</param>
        /// <param name="gateNodeId">The node ID of the parent Gate activity</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        private void ProcessGateNestedActivities(GateActivity gateActivity, string gateNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {
            // Check if there's a nested activity in the DataFunc.Handler
            var nestedActivity = gateActivity.DataFunc?.Handler;
            if (nestedActivity == null) return;

            // Process Nested Activity
            ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

            // If node is not created, skip further processing
            if (!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) return;

            // Generate a unique node ID for the nested activity
            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            // Add nesting metadata to indicate this activity is nested within the Gate
            nestedNode.data[Constants.ISNESTED] = true;
            nestedNode.data[Constants.PARENTID] = gateNodeId;
        }
    }
}
