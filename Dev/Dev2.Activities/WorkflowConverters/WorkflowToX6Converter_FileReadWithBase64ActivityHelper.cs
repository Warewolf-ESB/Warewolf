using Dev2.Common.X6;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateFileReadWithBase64Activity(FileReadWithBase64 fileReadWithBase64Activity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = fileReadWithBase64Activity.DisplayName ?? Constants.DISPLAYNAME_FILEREAD,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            fileReadWithBase64Activity.ToX6Json(cell);

            return cell;
        }
    }
}
