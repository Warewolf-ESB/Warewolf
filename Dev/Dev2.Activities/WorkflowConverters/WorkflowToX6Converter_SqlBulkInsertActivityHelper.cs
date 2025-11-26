using System.Activities;
using Dev2.Common.X6;
using Dev2.Activities;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateSqlBulkInsertActivity(DsfSqlBulkInsertActivity sqlBulkInsertActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = sqlBulkInsertActivity.DisplayName ?? Constants.DISPLAYNAME_SQLBULKINSERT,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            sqlBulkInsertActivity.ToX6Json(cell);

            return cell;
        }
    }
}