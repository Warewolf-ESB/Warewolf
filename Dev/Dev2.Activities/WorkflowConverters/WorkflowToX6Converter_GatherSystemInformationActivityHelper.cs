using Dev2.Activities;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateDotNetGatherSystemInformationActivity(DsfDotNetGatherSystemInformationActivity gatherSystemInfoActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = gatherSystemInfoActivity.DisplayName ?? Constants.DISPLAYNAME_GATHERSYSTEMINFORMATION,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            gatherSystemInfoActivity.ToX6Json(cell);

            return cell;
        }

        public Cell CreateGatherSystemInformationActivity(DsfGatherSystemInformationActivity gatherSystemInfoActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = gatherSystemInfoActivity.DisplayName ?? Constants.DISPLAYNAME_GATHERSYSTEMINFORMATION,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            gatherSystemInfoActivity.ToX6Json(cell);

            return cell;
        }
    }
}
