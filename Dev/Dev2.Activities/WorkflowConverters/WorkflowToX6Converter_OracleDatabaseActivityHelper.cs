using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateOracleDatabaseActivity(DsfOracleDatabaseActivity oracleDatabaseActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = oracleDatabaseActivity.DisplayName ?? Constants.DISPLAYNAME_ORACLESQLDATABASE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            oracleDatabaseActivity.ToX6Json(cell);

            return cell;
        }
    }
}