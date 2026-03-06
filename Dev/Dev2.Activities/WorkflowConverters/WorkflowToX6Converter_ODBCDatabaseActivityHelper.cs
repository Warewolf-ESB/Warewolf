using Dev2.Common.X6;
using Dev2.WorkflowConverters;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateODBCDatabaseActivity(DsfODBCDatabaseActivity odbcDatabaseActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = odbcDatabaseActivity.DisplayName ?? Constants.DISPLAYNAME_ODBCDATABASE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            odbcDatabaseActivity.ToX6Json(cell);

            return cell;
        }
    }
}
