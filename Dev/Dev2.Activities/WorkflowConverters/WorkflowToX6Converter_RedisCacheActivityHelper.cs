using Dev2.Activities.RedisCache;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using System.Activities;
using System.Collections.Generic;

namespace Dev2.Activities.WF {
    public partial class WorkflowToX6Converter
    {
        public Cell CreateRedisCacheActivity(RedisCacheActivity redisCacheActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = redisCacheActivity.DisplayName ?? Constants.DISPLAYNAME_REDISCACHE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            redisCacheActivity.ToX6Json(cell);

            return cell;
        }

        private string ProcessRedisCacheActivity(RedisCacheActivity redisCacheActivity, X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap, string previousNodeId)
        {
            {
                if (!activityNodeMap.TryGetValue(redisCacheActivity, out var redisCacheNodeId))
                {
                    redisCacheNodeId = CommonHelper.GenerateNodeId();
                    activityNodeMap[redisCacheActivity] = redisCacheNodeId;
                }

                var redisCacheNode = CreateRedisCacheActivity(redisCacheActivity, redisCacheNodeId);
                redisCacheNode.data.Add(Constants.PROPERTIES, ExtractActivityProperties(redisCacheActivity));

                graphData.Nodes.Add(redisCacheNode);
                graphData.ActivityNodeMap[redisCacheActivity] = redisCacheNode;

                if (!string.IsNullOrEmpty(previousNodeId))
                {
                    graphData.Edges.Add(CommonHelper.CreateEdge(previousNodeId, redisCacheNodeId));
                }

                ProcessRedisCacheNestedActivities(redisCacheActivity, redisCacheNodeId, graphData, activityNodeMap);

                return redisCacheNodeId;
            }
        }
         
        private void ProcessRedisCacheNestedActivities(RedisCacheActivity redisCacheActivity, string redisCacheNodeId,
            X6WorkflowLoadModel graphData, Dictionary<Activity, string> activityNodeMap)
        {

            var nestedActivity = redisCacheActivity.ActivityFunc?.Handler;
            if (nestedActivity == null) return;

            ProcessActivity(nestedActivity, graphData, activityNodeMap, string.Empty);

            if (!graphData.ActivityNodeMap.TryGetValue(nestedActivity, out Cell nestedNode)) return;

            var nestedNodeId = CommonHelper.GenerateNodeId();
            activityNodeMap[nestedActivity] = nestedNodeId;

            nestedNode.data[Constants.ISNESTED] = true;
            nestedNode.data[Constants.PARENTID] = redisCacheNodeId;
        }
    }
}