using System.Activities;
using Dev2.Common;
using Dev2.Common.X6;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateFindRecordsMultipleCriteriaActivity(DsfFindRecordsMultipleCriteriaActivity findRecordsActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = findRecordsActivity.DisplayName ?? Constants.DISPLAYNAME_FINDRECORDS,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            findRecordsActivity.ToX6Json(cell);

            return cell;
        }
    }
}