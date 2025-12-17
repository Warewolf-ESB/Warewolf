using Dev2.Common.X6;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreatePostgreSQLDatabaseActivity(DsfPostgreSqlActivity postgresqlDatabaseActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = postgresqlDatabaseActivity.DisplayName ?? Constants.DISPLAYNAME_POSTGRESQLDATABASE,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            postgresqlDatabaseActivity.ToX6Json(cell);

            return cell;
        }
    }
}