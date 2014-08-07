using System.Windows.Forms;
using Dev2.Common.ExtMethods;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Utils;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class ToolboxUIMap : UIMapBase
    {
        UITestControl _toolTree;
        UITestControl _toolSearch;

        public ToolboxUIMap()
        {
            _toolTree = VisualTreeWalker.GetControlFromRoot( true, 0, "UI_ToolboxPane_AutoID", "UI_ToolboxControl_AutoID", "PART_Tools");
            _toolSearch = VisualTreeWalker.GetControlFromRoot( true, 0, "UI_ToolboxPane_AutoID", "UI_ToolboxControl_AutoID", "PART_SearchBox");
        }

        public void ClickToolboxItem(ToolType tool)
        {
            var theControl = FindControl(tool.GetDescription());
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(p);
        }

        /// <summary>
        /// Drags a control from the Toolbox to the Workflow
        /// </summary>
        /// <param name="toolName">The name of the control you to drag - Eg: Assign, Calculate, Etc</param>
        /// <param name="tabToDropOnto">The tab on which to drop the control</param>
        /// <param name="pointToDragTo">The point you wish to drop the control</param>
        /// <param name="getDroppedActivity">Get and return the dropped control</param>
        public UITestControl DragControlToWorkflowDesigner(ToolType tool, UITestControl tabToDropOnto, Point pointToDragTo = new Point(), bool getDroppedActivity = true)
        {
            UITestControl theControl = FindToolboxItemByAutomationId(tool);
            theControl.WaitForControlEnabled();
            if(pointToDragTo.X == 0 && pointToDragTo.Y == 0)
            {
                UITestControl theStartButton = WorkflowDesignerUIMap.FindStartNode(tabToDropOnto);
                pointToDragTo = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);
            }

            Mouse.StartDragging(theControl, MouseButtons.Left);
            Playback.Wait(20);
            Mouse.StopDragging(pointToDragTo);
            Playback.Wait(100);

            UITestControl resourceOnDesignSurface = null;
            if(getDroppedActivity)
            {
                resourceOnDesignSurface = WorkflowDesignerUIMap.FindControlByAutomationId(tabToDropOnto, tool.ToString());
                int counter = 0;
                while(resourceOnDesignSurface == null && counter < 5)
                {
                    Playback.Wait(1000);
                    resourceOnDesignSurface = WorkflowDesignerUIMap.FindControlByAutomationId(tabToDropOnto, tool.ToString());
                    Playback.Wait(500);
                    counter++;
                }
            }

            return resourceOnDesignSurface;
        }

        /// <summary>
        /// Drags a control from the Toolbox to the Workflow
        /// </summary>
        /// <param name="controlId">The name of the control you to drag - Eg: Assign, Calculate, Etc</param>
        /// <param name="p">The point you wish to drop the control - Point p = WorkflowDesignerUIMap.GetPointUnderStartNode("someWorkflow"); is a good palce to start</param>
        /// <param name="searchID">The search unique identifier.</param>
        public void DragControlToWorkflowDesigner(ToolType tool, Point p, string searchID = "")
        {
            UITestControl theControl = FindToolboxItemByAutomationId(tool, searchID);
            theControl.WaitForControlEnabled();
            Playback.Wait(100);
            Mouse.StartDragging(theControl, MouseButtons.Left);
            Mouse.StopDragging(p);
        }

        public UITestControl FindToolboxItemByAutomationId(ToolType tool, string properSearchTerm = "")
        {
            var theTerm = tool.GetToolboxName();
            if(!string.IsNullOrEmpty(properSearchTerm))
            {
                theTerm = properSearchTerm;
            }

            SearchForControl(theTerm);

            UITestControl theControl = FindControl(tool.GetDescription());
            return theControl;
        }

        /// <summary>
        /// Determines whether [is icon visible] [the specified tool].
        /// NOTE : Crap method. If icon has a lot of white in it it will fail ;)
        /// </summary>
        /// <param name="tool">The tool.</param>
        /// <returns></returns>
        public bool IsIconVisible(UITestControl tool)
        {
            const string White = "ffffffff";
            var pixelGrabber = new Bitmap(tool.CaptureImage());
            var result = false;

            //must find some color after 3 pixel grabs

            //first pass
            Mouse.Move(tool, new Point(24, 9));
            var thePixel = pixelGrabber.GetPixel(24, 9).Name;
            result = thePixel != White;

            //second pass
            Mouse.Move(tool, new Point(25, 10));
            thePixel = pixelGrabber.GetPixel(25, 10).Name;
            result = result || thePixel != White;

            //third pass
            Mouse.Move(tool, new Point(26, 21));
            thePixel = pixelGrabber.GetPixel(24, 11).Name;
            result = result || thePixel != White;

            return result;
        }

        /// <summary>
        /// Searches for control.
        /// </summary>
        public void ClearSearch()
        {
            Mouse.Click(_toolSearch, new Point(5, 5));
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("+{END}");
            SendKeys.SendWait("{DELETE}");

            Playback.Wait(100);
            SendKeys.SendWait("");

        }

        /// <summary>
        /// Searches for control.
        /// </summary>
        /// <param name="controlName">Name of the control.</param>
        public void SearchForControl(string controlName)
        {
            Mouse.Click(_toolSearch, new Point(5, 5));
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("+{END}");
            SendKeys.SendWait("{DELETE}");

            Playback.Wait(100);
            SendKeys.SendWait(controlName);

        }

        public UITestControl FindControl(string itemAutomationID)
        {
            var kids = _toolTree.GetChildren();

            foreach(var kid in kids)
            {
                // Now process to find the correct item ;)

                var innerKids = kid.GetChildren();

                foreach(var innerKid in innerKids)
                {
                    string autoID = innerKid.GetProperty("AutomationID").ToString();
                    if(autoID.Contains(itemAutomationID))
                    {
                        return innerKid;
                    }
                }
            }
            return null;
        }

        public UITestControlCollection GetAllTools()
        {
            UITestControlCollection result = new UITestControlCollection();
            foreach(var category in _toolTree.GetChildren())
            {
                var tools = category.GetChildren();
                if(tools.Count > 0)
                {
                    foreach(var tool in tools)
                    {
                        if(tool.ControlType == ControlType.TreeItem)
                        {
                            result.Add(tool);
                        }
                    }
                }
            }
            return result;
        }
    }
}
