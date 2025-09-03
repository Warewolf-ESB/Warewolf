using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        private string ProcessDsfSequenceActivity(DsfSequenceActivity sequenceActivity, X6WorkflowLoadModel graphData,
            Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            // The nodeId should already be generated and stored in activityNodeMap by ProcessActivity
            if (!activityNodeMap.TryGetValue(sequenceActivity, out var sequenceNodeId))
            {
                sequenceNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[sequenceActivity] = sequenceNodeId;
            }

            // Create the Sequence node - the ToX6Json method will handle embedding child activities
            var sequenceNode = CreateSequenceNode(sequenceActivity, sequenceNodeId);
            sequenceNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(sequenceActivity));

            graphData.Nodes.Add(sequenceNode);
            graphData.ActivityNodeMap[sequenceActivity] = sequenceNode;

            // Create edge from previous node to this Sequence node
            if (!string.IsNullOrEmpty(previousNodeId))
            {
                graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, sequenceNodeId));
            }

            // Process nested activities from DsfSequenceActivity.Activities
            ProcessSequenceNestedActivities(sequenceActivity, sequenceNodeId, graphData, activityNodeMap);

            return sequenceNodeId;
        }

        public Cell CreateSequenceNode(DsfSequenceActivity sequenceActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = sequenceActivity.DisplayName ?? Constants.DISPLAYNAME_SEQUENCE,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            sequenceActivity.ToX6Json(cell);

            return cell;
        }

        /// <summary>
        /// Processes nested activities within a Sequence activity and creates separate X6 nodes for them
        /// </summary>
        /// <param name="sequenceActivity">The Sequence activity containing nested activities</param>
        /// <param name="sequenceNodeId">The node ID of the parent Sequence activity</param>
        /// <param name="graphData">The X6 graph data to add nodes to</param>
        /// <param name="activityNodeMap">Map of activities to their node IDs</param>
        private void ProcessSequenceNestedActivities(DsfSequenceActivity sequenceActivity, string sequenceNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {
            if (sequenceActivity.Activities == null || sequenceActivity.Activities.Count == 0) { return; }

            var index = 0;
            foreach (var nestedActivity in sequenceActivity.Activities)
            {
                // Process Nested Activity
                ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

                // If node is not created, skip further processing
                if(!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) continue;

                // Generate a unique node ID for the nested activity
                var nestedNodeId = CommonHelper.GenerateNodeId();
                activityNodeMap[nestedActivity] = nestedNodeId;

                // Add nesting metadata to indicate this activity is nested within the Sequence
                nestedNode.data[Constants.ISNESTED] = true;
                nestedNode.data[Constants.PARENTID] = sequenceNodeId;
                nestedNode.data[Constants.SEQUENCE_NESTED_ACTIVITY_INDEX] = index;

                index++;

                // no need to process as its already been processed by ProcessActivity
                // Recursively process any further nested activities (e.g., if the nested activity is itself a container)
                //ProcessNestedActivityChildren(nestedActivity, graphData, activityNodeMap, sequenceNodeId);
            }
        }
    }
}
