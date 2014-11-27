
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
            UITestControl dropPoint = VisualTreeWalker.GetChildByAutomationIdPath(Activity, "SmallViewContent", "DropPoint");
            var boundingRectangle = dropPoint.BoundingRectangle;
            ToolboxUIMap.DragControlToWorkflowDesigner(toolType, new Point(boundingRectangle.X + 10, boundingRectangle.Y + 10));
        }

        public UITestControl GetActivity()
        {

            UITestControl control = VisualTreeWalker.GetChildByAutomationIdPath(Activity, "SmallViewContent", "DropPoint");
            var uiTestControl = control.GetChildren()[0];
            return uiTestControl.FriendlyName == "Drop Activity Here" ? null : uiTestControl;
        }
    }
}
