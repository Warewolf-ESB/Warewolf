using Dev2.Common.X6;
using Dev2.Activities.Exchange;

namespace Dev2.Activities.WF
{
    public partial class WorkflowToX6Converter
    {
        public Cell CreateExchangeEmailActivity(DsfExchangeEmailNewActivity exchangeEmailActivity, string nodeId)
        {
            var cell = new Cell
            {
                id = nodeId,
                position = new Position(_currentX, _currentY),
                label = exchangeEmailActivity.DisplayName ?? Constants.DISPLAYNAME_EXCHANGEEMAIL,
                data = new System.Collections.Generic.Dictionary<string, object>()
            };

            _currentY += 150;

            // Let the activity serialize its own specific properties
            exchangeEmailActivity.ToX6Json(cell);

            return cell;
        }
    }
}
