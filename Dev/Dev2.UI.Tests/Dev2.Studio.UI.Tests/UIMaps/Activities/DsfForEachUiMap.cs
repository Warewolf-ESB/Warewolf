using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfForEachUiMap : ToolsUiMapBase
    {
        public DsfForEachUiMap(bool createNewtab = true, bool dragAssignOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragAssignOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.ForEach);

            }
        }

        public void DragActivityOnDropPoint(ToolType toolType)
        {
            UITestControl dropPoint = VisualTreeWalker.GetChildByAutomationIDPath(Activity, "SmallViewContent", "DropPoint");
            var boundingRectangle = dropPoint.BoundingRectangle;
            ToolboxUIMap.DragControlToWorkflowDesigner(toolType, new Point(boundingRectangle.X + 10, boundingRectangle.Y + 10));
        }

        public UITestControl GetActivity()
        {
            
            UITestControl control = VisualTreeWalker.GetChildByAutomationIDPath(Activity, "SmallViewContent", "DropPoint");
            var uiTestControl = control.GetChildren()[0];
            return uiTestControl.FriendlyName == "Drop Activity Here" ? null : uiTestControl;
        }
    }
}