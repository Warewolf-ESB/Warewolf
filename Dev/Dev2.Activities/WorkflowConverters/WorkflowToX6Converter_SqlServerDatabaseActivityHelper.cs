using System.Activities;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateSqlServerDatabaseActivity(DsfSqlServerDatabaseActivity sqlServerDatabaseActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = sqlServerDatabaseActivity.DisplayName ?? Constants.DISPLAYNAME_SQLSERVERDATABASE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            sqlServerDatabaseActivity.ToX6Json(cell);

            return cell;
        }
    }
}