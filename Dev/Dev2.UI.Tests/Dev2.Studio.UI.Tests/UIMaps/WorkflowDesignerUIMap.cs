namespace Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses
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
    using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Linq;
    
    public partial class WorkflowDesignerUIMap
    {
        /// <summary>
        /// Finds a control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationID">The automation ID of the control you are looking for</param>
        /// <returns>Returns the control as a UITestControl object</returns>
        public UITestControl FindControlByAutomationID(UITestControl theTab, string controlAutomationID)
        {
            // Unless the UI drastically changes (In which case most Automation tests will fail),
            // the order will remain constant

            // Cake names are used until they are replaced by the real names
            UITestControlCollection theCollection = new UITestControlCollection();
            try
            {
                theCollection = theTab.GetChildren();
            }
            catch
            {
                Assert.Fail("Error - Could not find '" + controlAutomationID + "' on the workflow designer!");
            }
            UITestControl splurtControl = theCollection[4];
            UITestControlCollection splurtChildChildren = splurtControl.GetChildren()[0].GetChildren();
            UITestControl cake2 = splurtChildChildren[0];
            UITestControlCollection cake2Children = cake2.GetChildren();
            UITestControl cake38 = cake2Children[3];
            UITestControlCollection cake38Children = cake38.GetChildren();
            // Cake38 -> ActivityTypeDesigner -> Cake53 -> FlowchartDesigner -> *Control Here*
            UITestControl cake53 = cake38Children[0].GetChildren()[0];
            UITestControlCollection cake53Children = cake53.GetChildren();
            UITestControl flowchartDesigner = cake53Children[0];
            UITestControlCollection flowchartDesignerChildren = flowchartDesigner.GetChildren();
            foreach (UITestControl theControl in flowchartDesignerChildren)
            {
                string automationID = theControl.GetProperty("AutomationId").ToString();
                if (automationID.Contains(controlAutomationID))
                {
                    return theControl;
                }
            }
            return null;
        }
        /// <summary>
        /// Finds the Start Node on a given tab
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <returns>Returns the Start Node as a UITestControl object</returns>
        public UITestControl FindStartNode(UITestControl theTab)
        {
            return FindControlByAutomationID(theTab, "Start");
        }

        /// <summary>
        /// Returns a point under a control
        /// </summary>
        /// <param name="control">A UITestControl from FindControlByAutomationID</param>
        /// <param name="pixels">How many pixels under the control</param>
        public Point GetPointUnderControl(UITestControl control, int pixels)
        {
            Point returnPoint = new Point(control.BoundingRectangle.X, control.BoundingRectangle.Y + pixels);

            return returnPoint;
        }

        /// <summary>
        /// Returns a point 200 pixels under the Start Node
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <returns></returns>
        public Point GetPointUnderStartNode(UITestControl theTab)
        {
            UITestControl startNode = FindStartNode(theTab);

            return GetPointUnderControl(startNode);
        }

        /// <summary>
        /// Returns a point 200 pixels under a control
        /// </summary>
        /// <param name="control">A UITestControl from FindControlByAutomationID</param>
        public Point GetPointUnderControl(UITestControl control)
        {
            return GetPointUnderControl(control, 200);
        }

        /// <summary>
        /// Clicks a control on the Workflow Designer
        /// </summary>
        /// <param name="theControl">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        public void ClickControl(UITestControl theControl)
        {
            Point p = new Point(theControl.BoundingRectangle.X + 25, theControl.BoundingRectangle.Y + 25);
            Mouse.Click(p);
        }

        /// <summary>
        /// Clicks a control on the Workflow Designer
        /// </summary>
        /// <param name="theControl">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        public void DoubleClickControlBar(UITestControl theControl)
        {
            Point p = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
            Mouse.DoubleClick(p);
        }

        /// <summary>
        /// Checks if a control exists on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationID">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        /// <returns></returns>
        public bool DoesControlExistOnWorkflowDesigner(UITestControl theTab, string controlAutomationID)
        {
            try
            {
                UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
                if (aControl != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public void CopyWorkflowXAML(UITestControl theTab)
        {
            UITestControl startButton = FindControlByAutomationID(theTab, "Start");
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(startButton.BoundingRectangle.X - 5, startButton.BoundingRectangle.Y - 5));
            Keyboard.SendKeys("c");
        }

        public void ClickExpandAll(UITestControl theTab)
        {
            UITestControlCollection theCollection = theTab.GetChildren();
            UITestControl splurtControl = theCollection[4];
            UITestControlCollection theControlCollection = splurtControl.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl expandAll = theControlCollection[1];
            Point p = expandAll.GetClickablePoint();
            Mouse.Click(expandAll, p);
        }

        public void ClickRestore(UITestControl theTab)
        {
            UITestControlCollection theCollection = theTab.GetChildren();
            UITestControl splurtControl = theCollection[4];
            UITestControlCollection theControlCollection = splurtControl.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl expandAll = theControlCollection[1];
            Point p = expandAll.GetClickablePoint();
            Mouse.Click(expandAll, p);
        }

        public void ClickCollapseAll(UITestControl theTab)
        {
            UITestControlCollection theCollection = theTab.GetChildren();
            UITestControl splurtControl = theCollection[4];
            UITestControlCollection theControlCollection = splurtControl.GetChildren()[0].GetChildren()[0].GetChildren();
            UITestControl collapseAll = theControlCollection[2];
            Point p = collapseAll.GetClickablePoint();
            Mouse.Click(collapseAll, p);
        }

        #region Adorners

        public bool IsAdornerVisible(UITestControl theTab, string controlAutomationID)
        {
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Service Working Normaly")
                {
                    Point newPoint = new Point();
                    if (theControl.TryGetClickablePoint(out newPoint))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public void Adorner_ClickMapping(UITestControl theTab, string controlAutomationID)
        {
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Open Mapping")
                {
                    Mouse.Click(theControl, new Point(5, 5));
                    break;
                }
            }
        }

        public void Adorner_ClickWizard(UITestControl theTab, string controlAutomationID)
        {
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Open Wizard")
                {
                    // Auto ID not set for some reason... ?
                    string automationID = theControl.GetProperty("AutomationID").ToString();
                    Mouse.Click(theControl, new Point(5, 5));
                    break;
                }
            }
        }

        public int Adorner_CountInputMappings(UITestControl theTab, string controlAutomationID)
        {
            int rowCounter = 0;
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                // inputMappings
                string automationID = theControl.GetProperty("AutomationID").ToString();
                if (automationID == "inputMappings")
                {
                    UITestControlCollection inputChildren = theControl.GetChildren();
                    foreach (UITestControl potentialRow in inputChildren)
                    {
                        if (potentialRow.ControlType.ToString() == "Row")
                        {
                            rowCounter++;
                        }
                    }
                }
            }
            return rowCounter;
        }

        #endregion Adorners

        public string GetWizardTitle(UITestControl theTab)
        {
            UITestControlCollection theCollection = theTab.GetChildren();
            UITestControl splurtControl = theCollection[4];
            UITestControl theControl = splurtControl.GetChildren()[0].GetChildren()[1].GetChildren()[0];
            return theControl.FriendlyName;
        }

        #region Assign Control

        public void AssignControl_ClickFirstTextbox(UITestControl theTab, string controlAutomationID)
        {
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            Point locationOfVariableTextbox = new Point(aControl.BoundingRectangle.Left + 50, aControl.BoundingRectangle.Top + 50);
            Mouse.Click(locationOfVariableTextbox);
        }

        public string AssignControl_GetVariableName(UITestControl theTab, string controlAutomationID, int itemInList)
        {
            UITestControl aControl = FindControlByAutomationID(theTab, controlAutomationID);
            List<WpfEdit> editList = new List<WpfEdit>();
            WpfEdit controlList = (WpfEdit)aControl.GetChildren()[2].GetChildren()[itemInList].GetChildren()[2].GetChildren()[0];
            return controlList.Text;
        }

        #endregion Assign Control

        // Intellisense Box
        public UITestControl GetIntellisenseItem(int id)
        {
            // Get the Studio
            WpfWindow theStudio = new WpfWindow();
            theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(TestBase.GetStudioWindowName());
            theStudio.Find();

            UITestControl itelliList = new UITestControl(theStudio);
            itelliList.SearchProperties[WpfTree.PropertyNames.AutomationId] = "PART_ItemList";
            itelliList.Find();

            UITestControl itelliListItem = itelliList.GetChildren()[id];
            return itelliListItem;
            //PART_ItemList
        }
    }
}
