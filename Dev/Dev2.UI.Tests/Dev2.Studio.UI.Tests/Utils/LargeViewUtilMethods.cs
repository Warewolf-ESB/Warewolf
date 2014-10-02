
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests.Utils
{
    public class LargeViewUtilMethods
    {
        public void LargeViewTextboxesEnterTestData(ToolType tool, UITestControl theTab)
        {
            //Find the start point
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow               
            ToolboxUIMap.DragControlToWorkflowDesigner(tool, workflowPoint1);

            WorkflowDesignerUIMap.OpenCloseLargeView(tool, theTab);

            // Add the data!


            List<UITestControl> listOfTextboxes = GetAllTextBoxesFromLargeView(tool.ToString(), theTab);

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
            List<UITestControl> passwordboxes = allTextboxes.Where(c =>
                {
                    var wpfEdit = c as WpfEdit;
                    return wpfEdit != null && wpfEdit.IsPassword;
                }).ToList();

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

// ReSharper disable InconsistentNaming
        public DocManagerUIMap DockManagerUIMap
// ReSharper restore InconsistentNaming
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

// ReSharper disable InconsistentNaming
        private DocManagerUIMap _dockManagerUIMap;
// ReSharper restore InconsistentNaming

        #endregion Dock Manager UI Map

        #region Toolbox UI Map

// ReSharper disable InconsistentNaming
        public ToolboxUIMap ToolboxUIMap
// ReSharper restore InconsistentNaming
        {
            get
            {
                if(_toolboxUIMap == null)
                    _toolboxUIMap = new ToolboxUIMap();
                return _toolboxUIMap;
            }

        }

// ReSharper disable InconsistentNaming
        private ToolboxUIMap _toolboxUIMap;
// ReSharper restore InconsistentNaming

        #endregion Toolbox UI Map

        #region Workflow Designer UI Map

// ReSharper disable InconsistentNaming
        public WorkflowDesignerUIMap WorkflowDesignerUIMap
// ReSharper restore InconsistentNaming
        {
            get
            {
                if(_workflowDesignerUIMap == null)
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                return _workflowDesignerUIMap;
            }
        }

// ReSharper disable InconsistentNaming
        private WorkflowDesignerUIMap _workflowDesignerUIMap;
// ReSharper restore InconsistentNaming

        #endregion Workflow Designer UI Map

    }
}
