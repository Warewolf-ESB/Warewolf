using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateFolderReadActivity(DsfFolderReadActivity folderReadActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = folderReadActivity.DisplayName ?? Constants.DISPLAYNAME_FOLDERREAD,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            folderReadActivity.ToX6Json(cell);

            return cell;
        }
    }
}