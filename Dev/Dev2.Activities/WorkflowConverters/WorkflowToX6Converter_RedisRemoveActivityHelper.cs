using System.Activities;
using Dev2.Activities.RedisRemove;
using Dev2.Common;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateRedisRemoveActivity(RedisRemoveActivity redisRemoveActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = redisRemoveActivity.DisplayName ?? Constants.DISPLAYNAME_REDISREMOVE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            redisRemoveActivity.ToX6Json(cell);

            return cell;
        }
    }
}