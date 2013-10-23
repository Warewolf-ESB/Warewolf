using Dev2.Studio.UI.Tests.Utils;

namespace Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    public partial class ToolboxUIMap
    {
        UITestControl _toolTree;
        public ToolboxUIMap()
        {
            _toolTree = VisualTreeWalker.GetControl("UI_DocManager_AutoID", "UI_ToolboxPane_AutoID", "Uia.ToolboxUserControl", "PART_Tools");
        }

        public void clickToolboxItem(string automationID)
        {
            UITestControl theControl = FindControl(automationID);
            Point p = new Point(theControl.BoundingRectangle.X + 50, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(p);
        }

        /// <summary>
        /// Drags a control from the Toolbox to the Workflow
        /// </summary>
        /// <param name="theControl">The control to drag - UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("controlNameHere");</param>
        /// <param name="p">The point you wish to drop the control - Point p = WorkflowDesignerUIMap.GetPointUnderStartNode("someWorkflow"); is a good palce to start</param>
        public void DragControlToWorkflowDesigner(UITestControl theControl, Point p)
        {
            Mouse.StartDragging(theControl, MouseButtons.Left);
            Mouse.StopDragging(p);
        }

        /// <summary>
        /// Drags a control from the Toolbox to the Workflow
        /// </summary>
        /// <param name="controlId">The name of the control you to drag - Eg: Assign, Calculate, Etc</param>
        /// <param name="p">The point you wish to drop the control - Point p = WorkflowDesignerUIMap.GetPointUnderStartNode("someWorkflow"); is a good palce to start</param>
        public void DragControlToWorkflowDesigner(string controlId, Point p)
        {
            UITestControl theControl = FindToolboxItemByAutomationId(controlId);
            DragControlToWorkflowDesigner(theControl, p);
        }

        public UITestControl FindToolboxItemByAutomationId(string automationId)
        {
            UITestControl theControl = FindControl(automationId);
            return theControl;
        }

        public bool IsIconVisible(UITestControl tool)
        {
            const string White = "ffffffff";
            var pixelGrabber = new Bitmap(tool.CaptureImage());
            var result = false;

            //must find some color after 3 pixel grabs

            //first pass
            Mouse.Move(tool, new Point(24,9));
            var thePixel = pixelGrabber.GetPixel(24, 9).Name;
            result = thePixel != White;

            //second pass
            Mouse.Move(tool, new Point(25, 10));
            thePixel = pixelGrabber.GetPixel(25, 10).Name;
            result = result || thePixel != White;

            //third pass
            Mouse.Move(tool, new Point(26, 11));
            thePixel = pixelGrabber.GetPixel(24, 11).Name;
            result = result || thePixel != White;

            return result;
        }

        public UITestControl FindControl(string itemAutomationID)
        {
            var kids = _toolTree.GetChildren();

            foreach (var kid in kids)
            {
                // Now process to find the correct item ;)

                var innerKids = kid.GetChildren();

                foreach (var innerKid in innerKids)
                {
                    string autoID = innerKid.GetProperty("AutomationID").ToString();
                    if (autoID.Contains(itemAutomationID))
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
