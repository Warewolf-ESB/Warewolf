
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public class DsfSequenceUiMap : ToolsUiMapBase
    {
        public DsfSequenceUiMap(bool createNewtab = true, bool dragAssignOntoNewTab = true)
            : base(createNewtab, 1500)
        {
            if(dragAssignOntoNewTab)
            {
                DragToolOntoDesigner(ToolType.Sequence);

            }
        }

        public void DragActivityOnDropPoint(ToolType toolType)
        {

            UITestControl dropPoint = VisualTreeWalker.GetChildByAutomationIdPath(Activity, "SmallViewContent", "DropPoint");
            var boundingRectangle = dropPoint.BoundingRectangle;
            ToolboxUIMap.DragControlToWorkflowDesigner(toolType, new Point(boundingRectangle.X + 10, boundingRectangle.Y + 10));
        }

        public void DragActivityOnLargeViewDropPoint(ToolType toolType)
        {

            UITestControl dropPoint = VisualTreeWalker.GetChildByAutomationIdPath(Activity, "LargeViewContent", "ActivitiesPresenter");
            var boundingRectangle = dropPoint.BoundingRectangle;
            ToolboxUIMap.DragControlToWorkflowDesigner(toolType, new Point(boundingRectangle.X + 10, boundingRectangle.Y + 10));
        }

        public List<string> GetActivityList()
        {
            List<string> activityNames = new List<string>();

            WpfTable table = VisualTreeWalker.GetChildByAutomationIdPath(Activity, "SmallViewContent", "InitialFocusElement") as WpfTable;
            if(table != null)
            {
                var uiTestControlCollection = table.Rows;
                foreach(var control in uiTestControlCollection)
                {
                    activityNames.Add(control.Name);
                }
            }
            return activityNames;
        }
    }
}
