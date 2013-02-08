using System.Drawing;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Forms;
using System.Threading;

namespace Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses
{   
    public partial class WorkflowDesignerUIMap
    {
        /// <summary>
        /// Finds a control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationId">The automation ID of the control you are looking for</param>
        /// <returns>Returns the control as a UITestControl object</returns>
        public UITestControl FindControlByAutomationId(UITestControl theTab, string controlAutomationId)
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
                Assert.Fail("Error - Could not find '" + controlAutomationId + "' on the workflow designer!");
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
                string automationId = theControl.GetProperty("AutomationId").ToString();
                if (automationId.Contains(controlAutomationId))
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
            return FindControlByAutomationId(theTab, "Start");
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
        /// <param name="controlAutomationId">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        /// <returns></returns>
        public bool DoesControlExistOnWorkflowDesigner(UITestControl theTab, string controlAutomationId)
        {
            try
            {
                UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
                return aControl != null;
            }
            catch
            {
                return false;
            }
        }

        public void CopyWorkflowXaml(UITestControl theTab)
        {
            UITestControl startButton = FindControlByAutomationId(theTab, "Start");
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

        public bool IsAdornerVisible(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Service Working Normaly")
                {
                    Point newPoint = new Point();
                    return theControl.TryGetClickablePoint(out newPoint);
                }
            }
            return false;
        }

        public void Adorner_ClickMapping(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
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

        public void Adorner_ClickWizard(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Open Wizard")
                {
                    // Auto ID not set for some reason... ?
                    // string automationID = theControl.GetProperty("AutomationID").ToString();
                    Mouse.Click(theControl, new Point(5, 5));
                    break;
                }
            }
        }

        public int Adorner_CountInputMappings(UITestControl theTab, string controlAutomationId)
        {
            int rowCounter = 0;
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                // inputMappings
                string automationId = theControl.GetProperty("AutomationID").ToString();
                if (automationId == "inputMappings")
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationId">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        public void SetStartNode(UITestControl theTab, string controlAutomationId)
        {
            UITestControl theControl = FindControlByAutomationId(theTab, controlAutomationId);
            Point pointAtTopOfControl = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, pointAtTopOfControl);

            Thread.Sleep(500);
            SendKeys.SendWait("{UP}");
            Thread.Sleep(500);
            SendKeys.SendWait("{UP}");
            Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(500);
        }

        #region Assign Control

        /// <summary>
        /// Enter data into an existing Assign control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="assignControlTitle">The title of the Assign box on the workflow</param>
        /// <param name="variable">The value to input into the left textbox</param>
        /// <param name="value">The value to input into the right textbox</param>
        public void AssignControl_EnterData(UITestControl theTab, string assignControlTitle, string variable, string value)
        {
            AssignControl_ClickFirstTextbox(theTab, assignControlTitle);
            SendKeys.SendWait(variable);
            Thread.Sleep(500);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(500);
            SendKeys.SendWait(value);
        }

        public void AssignControl_ClickFirstTextbox(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            Point locationOfVariableTextbox = new Point(aControl.BoundingRectangle.Left + 50, aControl.BoundingRectangle.Top + 50);
            Mouse.Click(locationOfVariableTextbox);
        }

        public string AssignControl_GetVariableName(UITestControl theTab, string controlAutomationId, int itemInList)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            WpfEdit controlList = (WpfEdit)aControl.GetChildren()[2].GetChildren()[itemInList].GetChildren()[2].GetChildren()[0];
            return controlList.Text;
        }

        #endregion Assign Control

        #region Calculate Control

        /// <summary>
        /// Enter some data into a Calculate control
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="calculateControlTitle">The title of the Calculate control on the workflow</param>
        /// <param name="function">The value to input into the top (function / fx) textbox</param>
        /// <param name="result">The value to enter into the bottom (Result) textbox</param>
        public void CalculateControl_EnterData(UITestControl theTab, string calculateControlTitle, string function, string result)
        {
            UITestControl calculateControl = FindControlByAutomationId(theTab, calculateControlTitle);

            // Click
            Point controlPoint = new Point(calculateControl.BoundingRectangle.X + 100, calculateControl.BoundingRectangle.Y + 50);
            Mouse.Click(controlPoint);

            // Enter data
            Thread.Sleep(500);
            SendKeys.SendWait(function.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(500);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(500);
            SendKeys.SendWait(result.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(500);
        }

        #endregion Calculate Control

        #region DataSplit Control

        public void DataSplit_ClickFirstTextbox(UITestControl theTab, string dataSplitControlTitle)
        {
            UITestControl dataSplitControl = FindControlByAutomationId(theTab, dataSplitControlTitle);
            WpfEdit theTextBox = (WpfEdit)dataSplitControl.GetChildren()[3];
            Point textBox = new Point(theTextBox.BoundingRectangle.X + 5, theTextBox.BoundingRectangle.Y + 5);
            Mouse.Click(textBox);
        }

        public string DataSplit_GetTextFromStringToSplit(UITestControl theTab, string dataSplitControlTitle)
        {
            UITestControl dataSplitControl = FindControlByAutomationId(theTab, dataSplitControlTitle);
            WpfEdit theTextBox = (WpfEdit)dataSplitControl.GetChildren()[3];
            return theTextBox.Text;
        }

        #endregion

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
