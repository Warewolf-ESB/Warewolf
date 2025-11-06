using System.Activities;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateMySqlDatabaseActivity(DsfMySqlDatabaseActivity mySqlDatabaseActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = mySqlDatabaseActivity.DisplayName ?? Constants.DISPLAYNAME_MYSQLDATABASE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            mySqlDatabaseActivity.ToX6Json(cell);

            return cell;
        }
    }
}
