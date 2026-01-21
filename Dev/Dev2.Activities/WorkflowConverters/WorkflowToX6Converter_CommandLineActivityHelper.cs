using Dev2.Common.X6;
using Dev2.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateCommandLineActivity(DsfExecuteCommandLineActivity commandLineActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = commandLineActivity.DisplayName ?? Constants.DISPLAYNAME_COMMANDLINE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            commandLineActivity.ToX6Json(cell);

            return cell;
        }
    }
}
