using Dev2.CodedUI.Tests;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    // ReSharper disable InconsistentNaming
    public partial class WorkflowDesignerUIMap : UIMapBase
    {
        public UITestControl ScrollViewer_GetVerticalScrollBar(UITestControl theTab)
        {
            GetScrollViewer(theTab);
            var scrollViewerChildren = GetScrollViewer(theTab).GetChildren();
            foreach(var scrollViewerChild in scrollViewerChildren)
            {
                if(scrollViewerChild.FriendlyName == "VerticalScrollBar")
                {
                    var getVericalScrollBarChildren = scrollViewerChild.GetChildren();
                    foreach(var scrollChild in getVericalScrollBarChildren)
                    {
                        if(scrollChild.FriendlyName == "thumb")
                        {
                            return scrollChild;
                        }
                    }
                }
            }
            return null;
        }

        public UITestControl ScrollViewer_GetHorizontalScrollBar(UITestControl theTab)
        {
            GetScrollViewer(theTab);
            var scrollViewerChildren = GetScrollViewer(theTab).GetChildren();
            foreach(var scrollViewerChild in scrollViewerChildren)
            {
                if(scrollViewerChild.FriendlyName == "HorizontalScrollBar")
                {
                    var getVericalScrollBarChildren = scrollViewerChild.GetChildren();
                    foreach(var scrollChild in getVericalScrollBarChildren)
                    {
                        if(scrollChild.FriendlyName == "thumb")
                        {
                            return scrollChild;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationId">The automation ID of the control you are looking for</param>
        /// <param name="waitAmt"></param>
        /// <returns>Returns the control as a UITestControl object</returns>
        public UITestControl FindControlByAutomationId(UITestControl theTab, string controlAutomationId, int waitAmt = 0)
        {
            // wait before the search ;)
            Playback.Wait(waitAmt);

            var workflowDesigner = GetFlowchartDesigner(theTab);
            foreach(UITestControl theControl in workflowDesigner.GetChildren())
            {
                string automationId = theControl.GetProperty("AutomationId").ToString();
                if(automationId.Contains(controlAutomationId) || theControl.FriendlyName.Contains(controlAutomationId))
                {
                    return theControl;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds controls on the Workflow Designer
        /// </summary>
        /// <param name="theTab"></param>
        /// <returns></returns>
        public UITestControlCollection GetAllControlsOnDesignSurface(UITestControl theTab)
        {
            var flowchartDesigner = GetFlowchartDesigner(theTab);
            return flowchartDesigner.GetChildren();
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
        /// <param name="verticalOffSet">How many pixels under the control</param>
        /// <param name="horizontalOffSet"></param>
        public Point GetPointUnderControl(UITestControl control, int verticalOffSet, int horizontalOffSet = 0)
        {
            Point returnPoint = new Point(control.BoundingRectangle.X + horizontalOffSet, control.BoundingRectangle.Y + verticalOffSet);

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

        public void CopyWorkflowXamlWithContextMenu(UITestControl theTab)
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
            foreach(UITestControl theControl in testFlowChildCollection)
            {
                if(theControl.FriendlyName == "Service Working Normaly")
                {
                    Point newPoint;
                    return theControl.TryGetClickablePoint(out newPoint);
                }
            }
            return false;
        }

        public void Adorner_ClickMapping(UITestControl theTab, string controlAutomationId)
        {
            UITestControl button = Adorner_GetButton(theTab, controlAutomationId, "OpenMappingsToggle");
            Mouse.Click(button, new Point(5, 5));
        }

        public void Adorner_ClickLargeView(UITestControl theTab)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, "LargeViewToggle");
            Mouse.Click(aControl, new Point(5, 5));
        }

        public void Adorner_ClickDoneButton(UITestControl theTab, string activityName)
        {
            UITestControl aControl = Adorner_GetDoneButton(theTab, activityName);
            var pt = new Point(5, 5);
            Mouse.Click(aControl, pt);
        }

        public void Adorner_ClickDoneButton(string activityName)
        {
            UITestControl aControl = Adorner_GetDoneButton(TabManagerUIMap.GetActiveTab(), activityName);
            var pt = new Point(5, 5);
            Mouse.Click(aControl, pt);
        }

        public UITestControl Adorner_GetDoneButton(UITestControl theTab, string activityName)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, activityName);
            foreach(var child in aControl.GetChildren())
            {
                if(child.GetProperty("AutomationId").ToString() == "DoneButton")
                {
                    return child;
                }
            }
            return null;
        }

        public bool Adorner_ClickFixErrors(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            if(testFlowChildCollection.Count > 0)
            {
                foreach(UITestControl theControl in testFlowChildCollection)
                {
                    if(theControl.GetProperty("AutomationID").ToString() == "SmallViewContent")
                    {
                        var smallViewControls = theControl.GetChildren();
                        foreach(var smallViewControl in smallViewControls)
                        {
                            if(smallViewControl.ControlType == ControlType.Button && smallViewControl.Height == 22)
                            {
                                Point newPoint = new Point(smallViewControl.Left + 10, smallViewControl.Top + 10);
                                Mouse.Click(newPoint);
                            }

                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public UITestControl Adorner_GetButton(UITestControl theTab, string controlAutomationId, string adornerFriendlyName)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach(UITestControl theControl in testFlowChildCollection)
            {
                if(theControl.FriendlyName == adornerFriendlyName)
                {
                    return theControl;
                }
            }
            return null;
        }

        public void DoubleClickOnActivity(UITestControl theTab, string controlAutomationId)
        {
            UITestControl activity = FindControlByAutomationId(theTab, controlAutomationId);
            Mouse.DoubleClick(new Point(activity.BoundingRectangle.X + 5, activity.BoundingRectangle.Y + 5));
        }

        public void OpenCloseLargeView(ToolType tool, UITestControl theTab)
        {
            DoubleClickOnActivity(theTab, tool.ToString());
        }

        public void OpenCloseLargeView(string tool, UITestControl theTab)
        {
            DoubleClickOnActivity(theTab, tool);
        }

        public void OpenCloseLargeView(UITestControl activity)
        {
            Mouse.DoubleClick(new Point(activity.BoundingRectangle.X + 5, activity.BoundingRectangle.Y + 5));
        }

        public int Adorner_CountInputMappings(UITestControl theTab, string controlAutomationId)
        {
            int rowCounter = 0;
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach(UITestControl theControl in testFlowChildCollection)
            {
                // inputMappings
                string automationId = theControl.GetProperty("AutomationID").ToString();
                if(automationId == "inputMappings")
                {
                    UITestControlCollection inputChildren = theControl.GetChildren();
                    foreach(UITestControl potentialRow in inputChildren)
                    {
                        if(potentialRow.ControlType.ToString() == "Row")
                        {
                            rowCounter++;
                        }
                    }
                }
            }
            return rowCounter;
        }

        public bool DoesActivityDataMappingContainText(UITestControl aControl, string text)
        {
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            foreach(UITestControl theControl in testFlowChildCollection)
            {
                // inputMappings
                string automationId = theControl.GetProperty("AutomationID").ToString();
                if(automationId == "inputMappings")
                {
                    UITestControlCollection inputChildren = theControl.GetChildren();
                    foreach(UITestControl potentialRow in inputChildren)
                    {
                        if(potentialRow.ControlType.ToString() == "Row")
                        {
                            var theRow = (potentialRow as WpfRow);
                            if(theRow != null)
                            {
                                foreach(var cell in theRow.Cells)
                                {
                                    if(cell is WpfEdit)
                                    {
                                        if((cell as WpfEdit).Text == text)
                                        {
                                            return true;
                                        }
                                    }
                                    else if((cell is WpfText))
                                    {
                                        if((cell as WpfText).DisplayText == text)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
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

            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <param name="controlAutomationId">A control from WorkflowDesignerUIMap.FindControlByAutomationID</param>
        public void OpenCloseLargeViewUsingContextMenu(UITestControl theTab, string controlAutomationId)
        {
            UITestControl theControl = FindControlByAutomationId(theTab, controlAutomationId);
            Point pointAtTopOfControl = new Point(theControl.BoundingRectangle.X + 5, theControl.BoundingRectangle.Y + 5);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, pointAtTopOfControl);

            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{UP}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");
            Playback.Wait(500);
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
            Playback.Wait(500);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait(value);
        }

        public WpfTable AssignControl_GetSmallViewTable(UITestControl theTab, string controlAutomationId, int row)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControl findContent = null;
            foreach(var child in assignControl.GetChildren())
            {
                if(child.FriendlyName == "SmallViewContent")
                {
                    findContent = child;
                    break;
                }
            }
            if(findContent != null)
            {
                UITestControl findTable = null;
                foreach(var child in findContent.GetChildren())
                {
                    if(child.FriendlyName == "SmallDataGrid")
                    {
                        findTable = child;
                        break;
                    }
                }
                if(findTable != null)
                {
                    WpfTable foundTable = (WpfTable)findTable;
                    return foundTable;
                }
            }
            throw new UITestControlNotFoundException("Cannot find multiassign small view content");
        }

        public WpfTable GetLargeViewTable(UITestControl theTab, string controlAutomationId)
        {
            UITestControl activityControl = FindControlByAutomationId(theTab, controlAutomationId);
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
                UITestControl findTable = null;
                var children = findContent.GetChildren();
                foreach(var child in children)
                {
                    if(child.FriendlyName == "LargeDataGrid")
                    {
                        findTable = child;
                        break;
                    }
                }
                if(findTable != null)
                {
                    WpfTable foundTable = (WpfTable)findTable;
                    return foundTable;
                }
            }
            throw new UITestControlNotFoundException("Cannot find specified control large view content");
        }

        public void AssignControl_ClickLeftTextboxInRow(UITestControl theTab, string controlAutomationId, int row)
        {
            WpfTable middleBox = AssignControl_GetSmallViewTable(theTab, controlAutomationId, row);
            // Get the textbox
            UITestControl leftTextboxInRow = middleBox.Rows[row].GetChildren()[2].GetChildren()[0];
            Point locationOfVariableTextbox = new Point(leftTextboxInRow.BoundingRectangle.X + 25,
                                                        leftTextboxInRow.BoundingRectangle.Y + 5);
            Mouse.Click(locationOfVariableTextbox);
        }

        public void AssignControl_LargeViewClickLeftTextboxInRow(UITestControl theTab, string controlAutomationId, int row)
        {
            WpfTable middleBox = GetLargeViewTable(theTab, controlAutomationId);
            // Get the textbox
            UITestControl leftTextboxInRow = middleBox.Rows[row].GetChildren()[2].GetChildren()[0];
            Point locationOfVariableTextbox = new Point(leftTextboxInRow.BoundingRectangle.X + 25,
                                                        leftTextboxInRow.BoundingRectangle.Y + 5);
            Mouse.Click(locationOfVariableTextbox);
        }

        public UITestControl AssignControl_GetLeftTextboxInRow(string controlAutomationId, int row)
        {
            var activeTab = TabManagerUIMap.GetActiveTab();
            Playback.Wait(2000);
            WpfTable middleBox = AssignControl_GetSmallViewTable(activeTab, controlAutomationId, row);
            // Get the textbox
            var getRow = middleBox.Rows[row];
            var getCell = getRow.GetChildren()[2];
            var getTextbox = getCell.GetChildren()[0];
            return getTextbox;
        }

        public void AssignControl_ClickScrollUp(UITestControl theTab, string controlAutomationId, int timesToClick)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControl middleBox = assignControl.GetChildren()[2];
            Point upArrow = new Point(middleBox.BoundingRectangle.X + middleBox.Width - 5, middleBox.BoundingRectangle.Y + 5);
            for(int j = 0; j < timesToClick; j++)
            {
                Mouse.Click(upArrow);
                Playback.Wait(250);
            }
        }

        public void AssignControl_ClickScrollDown(UITestControl theTab, string controlAutomationId, int timesToClick)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControl middleBox = assignControl.GetChildren()[2];
            Point upArrow = new Point(middleBox.BoundingRectangle.X + middleBox.Width - 5, middleBox.BoundingRectangle.Y + 40);
            for(int j = 0; j < timesToClick; j++)
            {
                Mouse.Click(upArrow);
                Playback.Wait(250);
            }
        }

        public bool AssignControl_LeftTextBoxInRowIsClickable(UITestControl theTab, string controlAutomationId, int row)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            WpfTable middleBox = (WpfTable)assignControl.GetChildren()[2];
            //UITestControl rowSearcher = new UITestControl(middleBox);
            Point p;
            if(middleBox.Rows[row].GetChildren()[2].GetChildren()[0].TryGetClickablePoint(out p))
            {
                return true;
            }
            return false;
        }

        public string AssignControl_GetVariableName(UITestControl theTab, string controlAutomationId, int itemInList)
        {
            var middleBox = AssignControl_GetSmallViewTable(theTab, controlAutomationId, itemInList);
            // Get the cell
            var getCell = middleBox.Rows[itemInList].GetChildren()[2];
            // Get the textbox
            var control = (WpfEdit)getCell.GetChildren()[0];
            return control.Text;
        }

        #region quick var input

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIWarewolfWindow : WpfWindow
        {

            public UIWarewolfWindow()
            {
                #region Search Criteria

                SearchProperties[UITestControl.PropertyNames.Name] = "Warewolf";
                SearchProperties.Add(new PropertyExpression(UITestControl.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                WindowTitles.Add("Warewolf");

                #endregion
            }

            #region Properties

            public UIFlowchartCustom4 UIFlowchartCustom
            {
                get
                {
                    if((mUIFlowchartCustom == null))
                    {
                        mUIFlowchartCustom = new UIFlowchartCustom4(this);
                    }
                    return mUIFlowchartCustom;
                }
            }

            #endregion

            #region Fields

            UIFlowchartCustom4 mUIFlowchartCustom;

            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIFlowchartCustom4 : WpfCustom
        {

            public UIFlowchartCustom4(UITestControl searchLimitContainer)
                :
                    base(searchLimitContainer)
            {
                #region Search Criteria

                SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.FlowchartDesigner";
                SearchProperties["AutomationId"] = "Unsaved 1(FlowchartDesigner)";
                WindowTitles.Add("Warewolf");

                #endregion
            }

            #region Properties

            public UIDsfMultiAssignActiviCustom UIDsfMultiAssignActiviCustom
            {
                get
                {
                    if((mUIDsfMultiAssignActiviCustom == null))
                    {
                        mUIDsfMultiAssignActiviCustom = new UIDsfMultiAssignActiviCustom(this);
                    }
                    return mUIDsfMultiAssignActiviCustom;
                }
            }

            #endregion

            #region Fields

            UIDsfMultiAssignActiviCustom mUIDsfMultiAssignActiviCustom;

            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIDsfMultiAssignActiviCustom : WpfCustom
        {

            public UIDsfMultiAssignActiviCustom(UITestControl searchLimitContainer)
                :
                    base(searchLimitContainer)
            {
                #region Search Criteria

                SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.MultiAssignDesigner";
                SearchProperties["AutomationId"] = "MultiAssignDesigner";
                WindowTitles.Add("Warewolf");

                #endregion
            }

            #region Properties

            public WpfToggleButton UIUI_Assign_QuickVariaToggleButton
            {
                get
                {
                    if((mUIUI_Assign_QuickVariaToggleButton == null))
                    {
                        mUIUI_Assign_QuickVariaToggleButton = new WpfToggleButton(this);

                        #region Search Criteria

                        mUIUI_Assign_QuickVariaToggleButton.SearchProperties[PropertyNames.AutomationId] = "[UI_Assign_QuickVariableAddBtn_AutoID]";
                        mUIUI_Assign_QuickVariaToggleButton.WindowTitles.Add("Warewolf");

                        #endregion
                    }
                    return mUIUI_Assign_QuickVariaToggleButton;
                }
            }

            #endregion

            #region Fields

            WpfToggleButton mUIUI_Assign_QuickVariaToggleButton;

            #endregion
        }

        #endregion

        public void AssignControl_ClickQuickVariableInputButton(UITestControl theTab, string controlAutomationId)
        {

            #region Variable Declarations

            WpfToggleButton uIUI_Assign_QuickVariaToggleButton = new UIWarewolfWindow().UIFlowchartCustom.UIDsfMultiAssignActiviCustom.UIUI_Assign_QuickVariaToggleButton;

            #endregion

            // Set to 'Pressed' state '[UI_Assign_QuickVariableAddBtn_AutoID]' toggle button
            uIUI_Assign_QuickVariaToggleButton.Pressed = true;
        }

        public void AssignControl_QuickVariableInputControl_EnterData(UITestControl theTab, string controlAutomationId, string splitOn, string prefix, string suffix, string variableList)
        {
            // Find the control
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            var qviControl = GetQVIControl(assignControlCollection);

            UITestControlCollection qviChildren = qviControl.GetChildren();

            UITestControlCollection textBoxes = new UITestControlCollection();
            foreach(UITestControl theControl in qviChildren)
            {
                if(theControl.ControlType == ControlType.Edit)
                {
                    textBoxes.Add(theControl);
                }
            }

            if(textBoxes.Count == 0)
            {
                Assert.Fail("Cant find QVI textboxes");
            }


            Mouse.Click(textBoxes[0], new Point(15, 5));
            Playback.Wait(250);
            SendKeys.SendWait(variableList.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}{TAB}");
            Playback.Wait(250);
            // And enter all the data
            SendKeys.SendWait(splitOn.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait(prefix.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait(suffix.Replace("(", "{(}").Replace(")", "{)}"));
        }

        public void AssignControl_QuickVariableInputControl_ClickAdd(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();

            UITestControl addBtn = new UITestControl();
            foreach(UITestControl controlChild in assignControlCollection)
            {
                if(controlChild.FriendlyName == "Add")
                {
                    addBtn = controlChild;
                }
            }
            Mouse.Click(addBtn, new Point(5, 5));
        }

        public void AssignControl_QuickVariableInputControl_ClickPreview(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = GetQVIControl(assignControlCollection);

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl previewBtn = new UITestControl();
            foreach(UITestControl quickVarInputChildren in qviChildren)
            {
                if(quickVarInputChildren.FriendlyName == "Preview")
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
            UITestControl qviControl = GetQVIControl(assignControlCollection);

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl addBtn = new UITestControl();
            foreach(UITestControl quickVarInputChildren in qviChildren)
            {
                if(quickVarInputChildren.FriendlyName == "Cancel")
                {
                    addBtn = quickVarInputChildren;
                }
            }

            Mouse.Click(addBtn, new Point(5, 5));

        }

        public string AssignControl_QuickVariableInputControl_GetPreviewData(UITestControl theTab, string controlAutomationId)
        {
            UITestControl assignControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection assignControlCollection = assignControl.GetChildren();
            UITestControl qviControl = GetQVIControl(assignControlCollection);

            UITestControlCollection qviChildren = qviControl.GetChildren();
            WpfText previewBox = (WpfText)qviChildren[qviChildren.Count - 1];
            return previewBox.DisplayText;
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
            Playback.Wait(500);
            SendKeys.SendWait(function.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(500);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait(result.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(500);
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
            theStudio.SearchProperties[UITestControl.PropertyNames.Name] = TestBase.GetStudioWindowName();
            theStudio.SearchProperties.Add(new PropertyExpression(UITestControl.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            theStudio.WindowTitles.Add(TestBase.GetStudioWindowName());
            theStudio.Find();

            UITestControl itelliList = new UITestControl(theStudio);
            itelliList.SearchProperties[WpfControl.PropertyNames.AutomationId] = "PART_ItemList";
            itelliList.Find();

            var itelliListItem = itelliList.GetChildren()[id];
            return itelliListItem;
            //PART_ItemList
        }

        /// <summary>
        /// Finds a control on the Workflow Designer
        /// </summary>
        /// <param name="theTab">A tab from TabManagerUIMap.FindTabByName</param>
        /// <returns>
        /// Returns the control as a UITestControl object
        /// </returns>
        public UITestControl GetFlowchartDesigner(UITestControl theTab)
        {
            var flowchartDesigner = VisualTreeWalker.GetChildByAutomationIdPath(theTab,
                "WorkSurfaceContextViewModel",
                "Uia.WorkflowDesignerView",
                "UserControl_1",
                "scrollViewer",
                "ActivityTypeDesigner",
                "WorkflowItemPresenter",
                "FlowchartDesigner");

            return flowchartDesigner;
        }

        /// <summary>
        /// Tool_s the get all text boxes.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <param name="toolAutomationId">The tool automation unique identifier.</param>
        /// <param name="toolDesignerTemplate">The tool designer template.</param>
        /// <returns></returns>
        public List<UITestControl> Tool_GetAllTextBoxes(UITestControl theTab, string toolAutomationId, string toolDesignerTemplate)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, toolAutomationId);
            UITestControlCollection uiTestControlCollection = aControl.GetChildren();

            foreach(UITestControl uiTestControl in uiTestControlCollection)
            {
                if(uiTestControl.ClassName == toolDesignerTemplate)
                {
                    UITestControlCollection testControlCollection = uiTestControl.GetChildren();
                    List<UITestControl> uiTestControls = testControlCollection.Where(c => c.ClassName == "Uia.TextBox").ToList();
                    return uiTestControls;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether [is control selected] [the specified workflow].
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns></returns>
        public bool IsControlSelected(UITestControl workflow)
        {
            var flowchartDesigner = (workflow as WpfControl);
            if(flowchartDesigner != null)
            {
                var itemStatus = flowchartDesigner.ItemStatus;
                var itemStatusVariables = itemStatus.Split(new[] { '=', ' ' });
                return itemStatusVariables[3] == "True";
            }
            throw new UITestControlNotFoundException("Cannot get flow chart designer as WpfControl");
        }

        /// <summary>
        /// Runs the workflow and wait until output step count attribute least.
        /// </summary>
        /// <param name="expectedStepCount">The expected step count.</param>
        /// <param name="timeout">The timeout.</param>
        /// <exception cref="Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException">Debug input dialog not shown within the given timeout</exception>
        public void RunWorkflowAndWaitUntilOutputStepCountAtLeast(int expectedStepCount, int timeout = 5000)
        {
            SendKeys.SendWait("{F5}");
            if(DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F6}");
            }
            else
            {
                throw new UITestControlNotFoundException("Debug input dialog not shown within the given timeout");
            }

            var steps = new UITestControlCollection();
            var count = 0;
            while(steps.Count < expectedStepCount && count <= timeout)
            {
                Playback.Wait(100);
                steps = OutputUIMap.GetOutputWindow();
                count++;
            }
        }

        /// <summary>
        /// Gets the qvi control.
        /// </summary>
        /// <param name="assignControlCollection">The assign control collection.</param>
        /// <returns></returns>
        static UITestControl GetQVIControl(UITestControlCollection assignControlCollection)
        {
            UITestControl qviControl = null;

            foreach(UITestControl theControl in assignControlCollection)
            {
                if(theControl.FriendlyName == "QuickVariableInputContent")
                {
                    qviControl = theControl;
                    break;
                }
            }
            if(qviControl == null)
            {
                Assert.Fail("Cant find quick variable input control");
            }
            return qviControl;
        }

        /// <summary>
        /// Gets the collapse help button.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <param name="activityAutomationID">The activity automation unique identifier.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public UITestControl GetCollapseHelpButton(UITestControl theTab, string activityAutomationID, int index = 0)
        {
            var activity = GetAllControlsOnDesignSurface(theTab)[index];
            return activity.GetChildren().FirstOrDefault(ui => ui.FriendlyName == "Close Help");
        }

        /// <summary>
        /// Gets the open help button.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <param name="activityAutomationID">The activity automation unique identifier.</param>
        /// <returns></returns>
        public UITestControl GetOpenHelpButton(UITestControl theTab, string activityAutomationID)
        {
            var activity = FindControlByAutomationId(theTab, activityAutomationID);
            return activity.GetChildren().FirstOrDefault(ui => ui.FriendlyName == "Open Help");
        }

        /// <summary>
        /// Determines whether [is activity icon visible] [the specified activity].
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        public bool IsActivityIconVisible(UITestControl activity)
        {
            const string ActivityDefaultGrey = "ffe9ecee";
            var pixelGrabber = new Bitmap(activity.CaptureImage());
            var thePixel = pixelGrabber.GetPixel(10, 10).Name;
            return thePixel != ActivityDefaultGrey;
        }

        /// <summary>
        /// Gets the scroll viewer.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public UITestControl GetScrollViewer(UITestControl theTab)
        {
            foreach(var control in theTab.GetChildren())
            {
                if(control.ClassName == "Uia.ContentPane")
                {
                    foreach(var contentPaneControl in control.GetChildren())
                    {
                        if(contentPaneControl.ClassName == "Uia.WorkflowDesignerView")
                        {
                            foreach(var workflowDesignerControl in contentPaneControl.GetChildren())
                            {
                                if(workflowDesignerControl.ClassName == "Uia.DesignerView")
                                {
                                    foreach(var designerViewControl in workflowDesignerControl.GetChildren())
                                    {
                                        if(designerViewControl.FriendlyName == "scrollViewer")
                                        {
                                            return designerViewControl;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scrolls the viewer_ click scroll down.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <param name="count">The count.</param>
        public void ScrollViewer_ClickScrollDown(UITestControl theTab, int count)
        {
            for(int i = 0; i < count; i++)
            {
                Mouse.Click(ScrollViewer_GetScrollDown(theTab));
            }
        }


        /// <summary>
        /// Scrolls the viewer_ get scroll down.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public UITestControl ScrollViewer_GetScrollDown(UITestControl theTab)
        {
            foreach(var control in GetScrollViewer(theTab).GetChildren())
            {
                if(control.FriendlyName == "VerticalScrollBar")
                {
                    foreach(var scrollControl in control.GetChildren())
                    {
                        if(scrollControl.FriendlyName == "repeatButton1")
                        {
                            return scrollControl;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scrolls the viewer_ get scroll up.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public UITestControl ScrollViewer_GetScrollUp(UITestControl theTab)
        {
            foreach(var control in GetScrollViewer(theTab).GetChildren())
            {
                if(control.FriendlyName == "VerticalScrollBar")
                {
                    foreach(var scrollControl in control.GetChildren())
                    {
                        if(scrollControl.FriendlyName == "repeatButton")
                        {
                            return scrollControl;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scrolls the viewer_ get scroll down.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public UITestControl ScrollViewer_GetScrollRight(UITestControl theTab)
        {
            foreach(var control in GetScrollViewer(theTab).GetChildren())
            {
                if(control.FriendlyName == "HorizontalScrollBar")
                {
                    foreach(var scrollControl in control.GetChildren())
                    {
                        if(scrollControl.FriendlyName == "repeatButton1")
                        {
                            return scrollControl;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scrolls the viewer_ get scroll up.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <returns></returns>
        public UITestControl ScrollViewer_GetScrollLeft(UITestControl theTab)
        {
            foreach(var control in GetScrollViewer(theTab).GetChildren())
            {
                if(control.FriendlyName == "HorizontalScrollBar")
                {
                    foreach(var scrollControl in control.GetChildren())
                    {
                        if(scrollControl.FriendlyName == "repeatButton")
                        {
                            return scrollControl;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Assigns the control_ is left text box highlighted red.
        /// </summary>
        /// <param name="leftTextBox">The left text box.</param>
        /// <returns></returns>
        public bool AssignControl_IsLeftTextBoxHighlightedRed(UITestControl leftTextBox)
        {
            var pixelGrabber = new Bitmap(leftTextBox.CaptureImage());
            return pixelGrabber.GetPixel(5, 0) == Color.Red;
        }

        public Point GetStartNodeBottomAutoConnectorPoint(UITestControl theTab)
        {
            var startNode = FindStartNode(theTab);
            //Note that 85 is the relative vertical distance in pixels from the top corner of the start node bounding rectangle
            // and 25 is the relative horizontal distance in pixels from that point
            return GetPointUnderControl(startNode, 85, 25);
        }

        /// <summary>
        /// Gets all connectors.
        /// </summary>
        /// <returns></returns>
        public List<UITestControl> GetAllConnectors()
        {
            UITestControlCollection uiTestControlCollection = GetFlowchartDesigner(TabManagerUIMap.GetActiveTab()).GetChildren();
            return uiTestControlCollection.Where(c => c.ClassName == "Uia.ConnectorWithoutStartDot").ToList();
        }

        /// <summary>
        /// Gets the help pane.
        /// </summary>
        /// <param name="theTab">The tab.</param>
        /// <param name="controlAutomationId">The control automation unique identifier.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public UITestControl GetHelpPane(UITestControl theTab, string controlAutomationId, int index = 0)
        {
            return GetAllControlsOnDesignSurface(theTab)[index];
        }

        /// <summary>
        /// Gets the SQL bulk insert children.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public IEnumerable<WpfControl> GetSqlBulkInsertChildren(UITestControl control)
        {
            var uiTestControls = control
                .GetChildren()
                .Select(i => i as WpfControl)
                .ToList();

            uiTestControls.AddRange(control
                .GetChildren()
                .SelectMany(c => c.GetChildren())
                .Select(i => i as WpfControl)
                .ToList());

            return uiTestControls;
        }

        /// <summary>
        /// Gets the SQL bulk insert large view first input textbox.
        /// </summary>
        /// <param name="sqlBulkInsertToolOnWorkflow">The SQL bulk insert tool configuration workflow.</param>
        /// <returns></returns>
        public UITestControl GetSqlBulkInsertLargeViewFirstInputTextbox(UITestControl sqlBulkInsertToolOnWorkflow)
        {
            return VisualTreeWalker.GetChildByAutomationIdPath(sqlBulkInsertToolOnWorkflow, "LargeViewContent", "LargeDataGrid", "Uia.DataGridRow", "Item: Dev2.TO.DataColumnMapping, Column Display In...", "UI__Row0_InputColumn_AutoID");
        }

        public void ScrollControlIntoView(UITestControl theTab, UITestControl theControl)
        {
            var workSurface = TabManagerUIMap.GetWorkSurface(theTab);
            if(theControl.BoundingRectangle.Y > workSurface.Height)
            {
                //might already be scrolled

                var scrollBar = WorkflowDesignerUIMap.ScrollViewer_GetVerticalScrollBar(theTab);
                WpfControl getTop = scrollBar as WpfControl;
                if(getTop != null && getTop.Top < 200)
                {
                    //Scoll bar is at the top, scroll down
                    Mouse.StartDragging(scrollBar);
                    Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollDown(theTab));
                }
            }
            else
            {
                //might already be scrolled
                var scrollBar = WorkflowDesignerUIMap.ScrollViewer_GetVerticalScrollBar(theTab);
                WpfControl getTop = scrollBar as WpfControl;
                if(getTop != null && getTop.Top > 200)
                {
                    //Scroll bar is at the bottom, scroll up
                    Mouse.StartDragging(scrollBar);
                    Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollUp(theTab));
                }
            }
            if(theControl.BoundingRectangle.X > workSurface.Width)
            {
                //might already be scrolled
                var scrollBar = WorkflowDesignerUIMap.ScrollViewer_GetHorizontalScrollBar(theTab);
                WpfControl getLeft = scrollBar as WpfControl;
                if(getLeft != null && getLeft.Left < 2084)
                {
                    //Scoll bar is at the left, scroll right
                    Mouse.StartDragging(scrollBar);
                    Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollRight(theTab));
                }
            }
            else
            {
                //might already be scrolled
                var scrollBar = WorkflowDesignerUIMap.ScrollViewer_GetVerticalScrollBar(theTab);
                WpfControl getLeft = scrollBar as WpfControl;
                if(getLeft != null && getLeft.Left > 2084)
                {
                    //Scroll bar is at the right, scroll left
                    Mouse.StartDragging(scrollBar);
                    Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollLeft(theTab));
                }
            }
        }

        public void DragControl(string controlAutomationID, Point point)
        {
            var theControl = FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), controlAutomationID);
            Mouse.StartDragging(theControl, new Point(10, 10));
            Mouse.StopDragging(point);
        }

        public bool TryCloseMappings(string controlAutomationID)
        {
            var button = WorkflowDesignerUIMap.Adorner_GetButton(TabManagerUIMap.GetActiveTab(), controlAutomationID, "Close Mapping");
            if(button != null)
            {
                WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);
                Mouse.Click(button);
                Playback.Wait(100);
                return true;
            }
            return false;
        }

        public string Adorner_GetHelpText(UITestControl resourceToGetHelpTextFor)
        {
            UITestControl parentControl = resourceToGetHelpTextFor.GetParent();
            List<UITestControl> uiTestControlCollection = parentControl.GetChildren().Where(c => c.ControlType == ControlType.Text).ToList();
            try
            {
                WpfText lbl = (WpfText)uiTestControlCollection[1];
                return lbl.DisplayText;
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}
