using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateZipActivity(DsfZip zipActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = zipActivity.DisplayName ?? Constants.DISPLAYNAME_ZIP,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            zipActivity.ToX6Json(cell);

            return cell;
        }
    }
}
