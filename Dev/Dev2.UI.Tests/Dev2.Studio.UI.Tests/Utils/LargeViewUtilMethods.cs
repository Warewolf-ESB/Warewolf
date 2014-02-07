using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.Utils
{
    public class LargeViewUtilMethods
    {
        public void LargeViewTextboxesEnterTestData(string toolName, UITestControl theTab)
        {
            //Find the start point
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow               
            ToolboxUIMap.DragControlToWorkflowDesigner(toolName, workflowPoint1);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, toolName,
                                                                           "Open Large View");

            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            // Add the data!


            List<UITestControl> listOfTextboxes = GetAllTextBoxesFromLargeView(toolName, theTab);

            int counter = 0;
            foreach(var textbox in listOfTextboxes)
            {
                WpfEdit tb = textbox as WpfEdit;
                if(tb != null && !tb.IsPassword)
                {
                    tb.SetFocus();
                    SendKeys.SendWait("[[theVar" + counter.ToString(CultureInfo.InvariantCulture) + "]]");
                }

                counter++;
            }
        }

        public void EnterDataIntoPasswordBoxes(List<UITestControl> allTextboxes)
        {
            List<UITestControl> passwordboxes = allTextboxes.Where(c => (c as WpfEdit).IsPassword).ToList();

            int passCounter = 0;

            foreach(var passwordbox in passwordboxes)
            {
                passwordbox.SetFocus();
                SendKeys.SendWait("pass" + passCounter);
                passCounter++;
            }
        }

        public List<UITestControl> GetAllTextBoxesFromLargeView(string toolName, UITestControl theTab)
        {
            List<UITestControl> results = new List<UITestControl>();

            UITestControl activityControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, toolName);
            UITestControl findContent = null;
            foreach(var child in activityControl.GetChildren())
            {
                if(child.FriendlyName == "LargeViewContent")
                {
                    findContent = child;
                    break;
                }
            }
            if(findContent != null)
            {
                var children = findContent.GetChildren();
                results = children.Where(c => c.ControlType.Name == "Edit").ToList();
            }

            return results;
        }

        public void ClickDoneButton(UITestControl theTab, string toolName)
        {
            UITestControl assignControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, toolName);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();

            UITestControl addBtn = new UITestControl();
            foreach(UITestControl controlChild in assignControlCollection)
            {
                if(controlChild.FriendlyName == "Done")
                {
                    addBtn = controlChild;
                }
            }
            Mouse.Click(addBtn, new Point(5, 5));
        }

        #region Dock Manager UI Map

        public DocManagerUIMap DockManagerUIMap
        {
            get
            {
                if(_dockManagerUIMap == null)
                {
                    _dockManagerUIMap = new DocManagerUIMap();
                }
                return _dockManagerUIMap;
            }
        }

        private DocManagerUIMap _dockManagerUIMap;

        #endregion Dock Manager UI Map

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if(_toolboxUIMap == null)
                    _toolboxUIMap = new ToolboxUIMap();
                return _toolboxUIMap;
            }

        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion Toolbox UI Map

        #region Workflow Designer UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if(_workflowDesignerUIMap == null)
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion Workflow Designer UI Map

    }
}
