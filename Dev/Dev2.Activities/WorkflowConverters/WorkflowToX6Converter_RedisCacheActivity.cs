using System.Activities;
using Dev2.Activities.RedisCache;
using Dev2.Common;
using Dev2.Common.X6;

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
    }
}