using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateManualResumptionActivity(ManualResumptionActivity manualResumptionActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = manualResumptionActivity.DisplayName ?? Constants.MANUALRESUMPTION_DISPLAYNAME,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            manualResumptionActivity.ToX6Json(cell);

            return cell;
        }
        private string ProcessManualResumptionActivity(ManualResumptionActivity manualResumptionActivity, X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            {
                if (!activityNodeMap.TryGetValue(manualResumptionActivity, out var manualResumptionNodeId))
                {
                    manualResumptionNodeId = CommonHelper.GenerateNodeId();
                    activityNodeMap[manualResumptionActivity] = manualResumptionNodeId;
                }

                var redisCacheNode = CreateManualResumptionActivity(manualResumptionActivity, manualResumptionNodeId);
                redisCacheNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(manualResumptionActivity));

                graphData.Nodes.Add(redisCacheNode);
                graphData.ActivityNodeMap[manualResumptionActivity] = redisCacheNode;

                if (!string.IsNullOrEmpty(previousNodeId))
                {
                    graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, manualResumptionNodeId));
                }

                ProcessManualResumptionNestedActivities(manualResumptionActivity, manualResumptionNodeId, graphData, activityNodeMap);

                return manualResumptionNodeId;
            }
        }

        private void ProcessManualResumptionNestedActivities(ManualResumptionActivity manualResumptionActivity, string manualResumptionNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {

            var nestedActivity = manualResumptionActivity.OverrideDataFunc?.Handler;
            if (nestedActivity == null) return;

            ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

            if (!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) return;

            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            nestedNode.data[Constants.ISNESTED] = true;
            nestedNode.data[Constants.PARENTID] = manualResumptionNodeId;
        }
    }
}