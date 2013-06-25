using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Windows;

namespace Dev2.Utilities
{
    public static class FlowNodeActivityDropUtils
    {
        #region Fields

        private static ViewStateService DropViewStateService;
        private static Point DropPoint;

        #endregion

        public static bool RegisterFlowNodeDrop(ViewStateService viewStateService, Point dropPoint)
        {
            if (viewStateService != null)
            {
                DropViewStateService = viewStateService;
                DropPoint = dropPoint;
                return true;
            }
            return false;
        }

        public static bool SetDropPointAndDeregisterFlowNode(ModelItem modelItem)
        {
            if (DropViewStateService != null && modelItem != null)
            {
                DropViewStateService.StoreViewState(modelItem, "ShapeLocation", DropPoint);

                DropViewStateService = null;
                return true;
            }
            return false;
        }
    }
}
