using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreatePathRenameActivity(DsfPathRename pathRenameActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = pathRenameActivity.DisplayName ?? Constants.DISPLAYNAME_PATHRENAME,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            pathRenameActivity.ToX6Json(cell);

            return cell;
        }
    }
}
