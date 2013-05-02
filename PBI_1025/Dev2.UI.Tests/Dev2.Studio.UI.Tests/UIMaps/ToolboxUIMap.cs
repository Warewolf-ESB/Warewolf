namespace Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    public partial class ToolboxUIMap
    {
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
    }
}
