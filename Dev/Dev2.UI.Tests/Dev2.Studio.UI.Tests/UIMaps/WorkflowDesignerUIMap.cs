using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;

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
            UITestControl splurtControl = theCollection[6];
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

        public void Adorner_ClickHelp(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == "Help")
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
            AssignControl_ClickLeftTextboxInRow(theTab, assignControlTitle, 0);
            SendKeys.SendWait(variable);
            Thread.Sleep(500);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(500);
            SendKeys.SendWait(value);
        }

        public void AssignControl_ClickLeftTextboxInRow(UITestControl theTab, string controlAutomationId, int row)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            WpfTable middleBox = (WpfTable)assignControl.GetChildren()[2];
            // Get the textbox
            UITestControl leftTextboxInRow = middleBox.Rows[row].GetChildren()[2].GetChildren()[0];
            Point locationOfVariableTextbox = new Point(leftTextboxInRow.BoundingRectangle.X + 25, leftTextboxInRow.BoundingRectangle.Y + 5);
            Mouse.Click(locationOfVariableTextbox);
        }

        public void AssignControl_ClickScrollUp(UITestControl theTab, string controlAutomationId, int timesToClick)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControl middleBox = assignControl.GetChildren()[2];
            Point upArrow = new Point(middleBox.BoundingRectangle.X + middleBox.Width - 5, middleBox.BoundingRectangle.Y + 5);
            for (int j = 0; j < timesToClick; j++)
            {
                Mouse.Click(upArrow);
                Thread.Sleep(250);
            }
        }

        public void AssignControl_ClickScrollDown(UITestControl theTab, string controlAutomationId, int timesToClick)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControl middleBox = assignControl.GetChildren()[2];
            Point upArrow = new Point(middleBox.BoundingRectangle.X + middleBox.Width - 5, middleBox.BoundingRectangle.Y + 40);
            for (int j = 0; j < timesToClick; j++)
            {
                Mouse.Click(upArrow);
                Thread.Sleep(250);
            }
        }

        public bool AssignControl_LeftTextBoxInRowIsClickable(UITestControl theTab, string controlAutomationId, int row)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            WpfTable middleBox = (WpfTable)assignControl.GetChildren()[2];
            //UITestControl rowSearcher = new UITestControl(middleBox);
            Point p = new Point();
            if (middleBox.Rows[row].GetChildren()[2].GetChildren()[0].TryGetClickablePoint(out p))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string AssignControl_GetVariableName(UITestControl theTab, string controlAutomationId, int itemInList)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            WpfEdit controlList = (WpfEdit)aControl.GetChildren()[2].GetChildren()[itemInList].GetChildren()[2].GetChildren()[0];
            return controlList.Text;
        }

        public void AssignControl_ClickQuickVariableInputButton(UITestControl theTab, string controlAutomationId)
        {
            WpfButton quickVarButton = GetQuickVariableInputButton(theTab, controlAutomationId);
            Mouse.Move(new Point(quickVarButton.BoundingRectangle.X + 5, quickVarButton.BoundingRectangle.Y + 5));
            Mouse.Click();
        }

        public void AssignControl_QuickVariableInputControl_EnterData(UITestControl theTab, string controlAutomationId, string splitOn, string prefix, string suffix, string variableList)
        {
            // Find the control
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = new UITestControl();
            foreach (UITestControl theControl in assignControlCollection)
            {
                if (theControl.FriendlyName == "quickVariableInputControl")
                {
                    qviControl = theControl;
                    break;
                }
            }

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl splitTxt = new UITestControl();
            foreach (UITestControl theControl in qviChildren)
            {
                if (theControl.FriendlyName == "SplitTokenTxt")
                {
                    splitTxt = theControl;
                    break;
                }
            }

            Mouse.Click(splitTxt, new Point(15, 5));
            Thread.Sleep(250);
            // And enter all the data
            SendKeys.SendWait(splitOn.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait(prefix.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait(suffix.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(250);
            SendKeys.SendWait("{TAB}");
            Thread.Sleep(250);
            SendKeys.SendWait(variableList.Replace("(", "{(}").Replace(")", "{)}"));
            Thread.Sleep(1000);
        }

        public void AssignControl_QuickVariableInputControl_ClickAdd(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = new UITestControl();
            foreach (UITestControl theControl in assignControlCollection)
            {
                if (theControl.FriendlyName == "quickVariableInputControl")
                {
                    qviControl = theControl;
                    break;
                }
            }

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl addBtn = new UITestControl();
            foreach (UITestControl quickVarInputChildren in qviChildren)
            {
                if (quickVarInputChildren.FriendlyName == "Add")
                {
                    addBtn = quickVarInputChildren;
                }
            }
            Mouse.Click(addBtn, new Point(5, 5));
        }


        public void AssignControl_QuickVariableInputControl_ClickPreview(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = new UITestControl();
            foreach (UITestControl theControl in assignControlCollection)
            {
                if (theControl.FriendlyName == "quickVariableInputControl")
                {
                    qviControl = theControl;
                    break;
                }
            }

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl previewBtn = new UITestControl();
            foreach (UITestControl quickVarInputChildren in qviChildren)
            {
                if (quickVarInputChildren.FriendlyName == "Preview")
                {
                    previewBtn = quickVarInputChildren;
                }
            }
            Mouse.Click(previewBtn, new Point(5, 5));
        }

        public void AssignControl_QuickVariableInputControl_ClickCancel(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = new UITestControl();
            foreach (UITestControl theControl in assignControlCollection)
            {
                if (theControl.FriendlyName == "quickVariableInputControl")
                {
                    qviControl = theControl;
                    break;
                }
            }

            UITestControl cancelBtn = new UITestControl();

            foreach (UITestControl qviChildren in qviControl.GetChildren())
            {
                if (qviChildren.FriendlyName == "Cancel")
                {
                    cancelBtn = qviChildren;
                }
            }
            Mouse.Click(cancelBtn, new Point(5, 5));
        }

        public string AssignControl_QuickVariableInputControl_GetPreviewData(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = new UITestControl();
            foreach (UITestControl theControl in assignControlCollection)
            {
                if (theControl.FriendlyName == "quickVariableInputControl")
                {
                    qviControl = theControl;
                    //break;
                }
            }
            UITestControlCollection qviChildren = qviControl.GetChildren();
            WpfEdit previewBox = (WpfEdit)qviChildren[qviChildren.Count - 1];
            return previewBox.Text;
        }
        #endregion Assign Control

        #region BaseConvert Control

        public int BaseConvert_GetDDLHeight(UITestControl theTab, string baseConvertControlTitle)
        {
            UITestControl baseConvertControl = FindControlByAutomationId(theTab, baseConvertControlTitle);
            WpfComboBox theComboBox = (WpfComboBox)baseConvertControl.GetChildren()[2].GetChildren()[0].GetChildren()[3].GetChildren()[0];
            return theComboBox.Height;
        }

        #endregion BaseConvert Control

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

        #region FindRecords Control

        public int FindRecords_GetDDLHeight(UITestControl theTab, string findRecordsControlTitle)
        {
            UITestControl findRecordsControl = FindControlByAutomationId(theTab, findRecordsControlTitle);
            WpfComboBox theComboBox = (WpfComboBox)findRecordsControl.GetChildren()[5];
            return theComboBox.Height;
        }

        #endregion BaseConvert Control

        #region Sort Control

        public int Sort_GetDDLHeight(UITestControl theTab, string sortControlTitle)
        {
            UITestControl sortControl = FindControlByAutomationId(theTab, sortControlTitle);

            // The specific layout for the Text combo box
            WpfComboBox theComboBox = (WpfComboBox)sortControl.GetChildren()[5];

            return theComboBox.Height;
        }

        #endregion Sort Control

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
        
        /// <summary>
        /// Finds a control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationId">The automation ID of the control you are looking for</param>
        /// <returns>Returns the control as a UITestControl object</returns>
        public UITestControl GetFlowchartDesigner(UITestControl theTab)
        {
            // Unless the UI drastically changes (In which case most Automation tests will fail),
            // the order will remain constant

            // Cake names are used until they are replaced by the real names
            var theCollection = new UITestControlCollection();
            try
            {
                theCollection = theTab.GetChildren();
            }
            catch
            {
                Assert.Fail("Error - Could not find '" + theTab.Name + "' on the workflow designer!");
            }
            UITestControl splurtControl = theCollection[6];
            UITestControlCollection splurtChildChildren = splurtControl.GetChildren()[0].GetChildren();
            UITestControl cake2 = splurtChildChildren[0];
            UITestControlCollection cake2Children = cake2.GetChildren();
            UITestControl cake38 = cake2Children[3];
            UITestControlCollection cake38Children = cake38.GetChildren();
            // Cake38 -> ActivityTypeDesigner -> Cake53 -> FlowchartDesigner -> *Control Here*
            UITestControl cake53 = cake38Children[0].GetChildren()[0];
            UITestControlCollection cake53Children = cake53.GetChildren();
            UITestControl flowchartDesigner = cake53Children[0];
            return flowchartDesigner;
        }
    }        
}
