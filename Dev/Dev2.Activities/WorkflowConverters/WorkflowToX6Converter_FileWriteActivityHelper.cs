using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateFileWriteActivity(DsfFileWrite fileWriteActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = fileWriteActivity.DisplayName ?? Constants.DISPLAYNAME_FILEWRITE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            fileWriteActivity.ToX6Json(cell);

            return cell;
        }

        public Cell CreateFileWriteActivity(FileWriteActivity fileWriteActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = fileWriteActivity.DisplayName ?? Constants.DISPLAYNAME_FILEWRITE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            fileWriteActivity.ToX6Json(cell);

            return cell;
        }
    }
}
