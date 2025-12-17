using Dev2.Activities;
using Dev2.Common.X6;
using System.Collections.Generic;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateWebPostActivity(WebPostActivityNew webPostActivityNew, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = webPostActivityNew.DisplayName ?? Constants.DISPLAYNAME_WEBPOST,
                data = new Dictionary<string, object>()
            };

            _currentY += 150;

            webPostActivityNew.ToX6Json(cell);

            return cell;
        }
    }
}
