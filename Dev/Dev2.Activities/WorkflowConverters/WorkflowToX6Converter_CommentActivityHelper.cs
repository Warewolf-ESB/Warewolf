using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateCommentActivity(DsfCommentActivity commentActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = commentActivity.DisplayName ?? Constants.DISPLAYNAME_COMMENT,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            commentActivity.ToX6Json(cell);

            return cell;
        }
    }
}
