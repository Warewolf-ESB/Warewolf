using Dev2.Activities.Scripting;
using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreatePythonActivity(DsfPythonActivity pythonActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = pythonActivity.DisplayName ?? Constants.DISPLAYNAME_PYTHON,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            pythonActivity.ToX6Json(cell);

            return cell;
        }
    }
}
