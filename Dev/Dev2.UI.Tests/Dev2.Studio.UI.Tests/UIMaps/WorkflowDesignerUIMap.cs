using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
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
            UITestControl designerWrapper = theCollection.FirstOrDefault(c => c.ControlType.Name == "Custom");

            if (designerWrapper != null)
            {
                UITestControlCollection designerWrapperChildren = designerWrapper.GetChildren();

                UITestControl designer = designerWrapperChildren.FirstOrDefault(c => c.ControlType.Name == "Custom");

                if (designer != null)
                {
                    UITestControlCollection designerChildren = designer.GetChildren();
                    var innerDesigner = designerChildren.LastOrDefault(c => c.ControlType.Name == "Custom");
                    if (innerDesigner != null)
                    {
                        UITestControlCollection innerDesignerChildren = innerDesigner.GetChildren();
                        //TODO : Find a cleaner way of getting the design surface
                        UITestControl cake2 = innerDesignerChildren[3];
                        UITestControlCollection splurtChildChildren = cake2.GetChildren();
                        cake2 = splurtChildChildren[0];
                        splurtChildChildren = cake2.GetChildren();
                        cake2 = splurtChildChildren[0];
                        splurtChildChildren = cake2.GetChildren();
                        cake2 = splurtChildChildren[0];
                        splurtChildChildren = cake2.GetChildren();
                        foreach (UITestControl theControl in splurtChildChildren)
                        {
                            string automationId = theControl.GetProperty("AutomationId").ToString();
                            if (automationId.Contains(controlAutomationId))
                            {
                                return theControl;
                            }
                        }
                    }
                }
            }

            //UITestControlCollection splurtChildChildren = splurtControl.GetChildren()[0].GetChildren();
            //UITestControl cake2 = splurtChildChildren[0];
            //UITestControlCollection cake2Children = cake2.GetChildren();
            //UITestControl cake38 = cake2Children[3];
            //UITestControlCollection cake38Children = cake38.GetChildren();
            //// Cake38 -> ActivityTypeDesigner -> Cake53 -> FlowchartDesigner -> *Control Here*
            //UITestControl cake53 = cake38Children[0].GetChildren()[0];
            //UITestControlCollection cake53Children = cake53.GetChildren();
            //UITestControl flowchartDesigner = cake53Children[0];
            //UITestControlCollection flowchartDesignerChildren = flowchartDesigner.GetChildren();
            //foreach (UITestControl theControl in flowchartDesignerChildren)
            //{
            //    string automationId = theControl.GetProperty("AutomationId").ToString();
            //    if (automationId.Contains(controlAutomationId))
            //    {
            //        return theControl;
            //    }
            //}
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
            UITestControl button = Adorner_GetButton(theTab, controlAutomationId, "OpenMappingsToggle");
            Mouse.Click(button, new Point(5, 5));
        }

        public void Adorner_ClickLargeView(UITestControl theTab)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, "LargeViewToggle");            
            Mouse.Click(aControl, new Point(5, 5));
        }

        public bool Adorner_ClickFixErrors(UITestControl theTab, string controlAutomationId)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, controlAutomationId);
            UITestControlCollection testFlowChildCollection = aControl.GetChildren();
            if(testFlowChildCollection.Count > 0)
            {
                foreach(UITestControl theControl in testFlowChildCollection)
                {
                    if (theControl.ControlType == ControlType.Button && theControl.Height == 22 && theControl.Width == 22)
                    {
                        Point newPoint = new Point();
                        if(theControl.TryGetClickablePoint(out newPoint))
                        {
                            Mouse.Click(theControl, newPoint);
                        }
                        else
                        {
                            return false;
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
            foreach (UITestControl theControl in testFlowChildCollection)
            {
                if (theControl.FriendlyName == adornerFriendlyName)
                {
                    return theControl;
                }
            }
            return null;
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
                Playback.Wait(250);
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
                Playback.Wait(250);
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

        #region quick var input

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIWarewolfWindow : WpfWindow
        {

            public UIWarewolfWindow()
            {
                #region Search Criteria
                this.SearchProperties[WpfWindow.PropertyNames.Name] = "Warewolf";
                this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public UIFlowchartCustom4 UIFlowchartCustom
            {
                get
                {
                    if ((this.mUIFlowchartCustom == null))
                    {
                        this.mUIFlowchartCustom = new UIFlowchartCustom4(this);
                    }
                    return this.mUIFlowchartCustom;
                }
            }
            #endregion

            #region Fields
            private UIFlowchartCustom4 mUIFlowchartCustom;
            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIFlowchartCustom4 : WpfCustom
        {

            public UIFlowchartCustom4(UITestControl searchLimitContainer) :
                base(searchLimitContainer)
            {
                #region Search Criteria
                this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.FlowchartDesigner";
                this.SearchProperties["AutomationId"] = "Unsaved 1(FlowchartDesigner)";
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public UIDsfMultiAssignActiviCustom UIDsfMultiAssignActiviCustom
            {
                get
                {
                    if ((this.mUIDsfMultiAssignActiviCustom == null))
                    {
                        this.mUIDsfMultiAssignActiviCustom = new UIDsfMultiAssignActiviCustom(this);
                    }
                    return this.mUIDsfMultiAssignActiviCustom;
                }
            }
            #endregion

            #region Fields
            private UIDsfMultiAssignActiviCustom mUIDsfMultiAssignActiviCustom;
            #endregion
        }

        [GeneratedCode("Coded UITest Builder", "11.0.60315.1")]
        public class UIDsfMultiAssignActiviCustom : WpfCustom
        {

            public UIDsfMultiAssignActiviCustom(UITestControl searchLimitContainer) :
                base(searchLimitContainer)
            {
                #region Search Criteria
                this.SearchProperties[UITestControl.PropertyNames.ClassName] = "Uia.DsfMultiAssignActivityDesigner";
                this.SearchProperties["AutomationId"] = "Assign(DsfMultiAssignActivityDesigner)";
                this.WindowTitles.Add("Warewolf");
                #endregion
            }

            #region Properties
            public WpfToggleButton UIUI_Assign_QuickVariaToggleButton
            {
                get
                {
                    if ((this.mUIUI_Assign_QuickVariaToggleButton == null))
                    {
                        this.mUIUI_Assign_QuickVariaToggleButton = new WpfToggleButton(this);
                        #region Search Criteria
                        this.mUIUI_Assign_QuickVariaToggleButton.SearchProperties[WpfToggleButton.PropertyNames.AutomationId] = "[UI_Assign_QuickVariableAddBtn_AutoID]";
                        this.mUIUI_Assign_QuickVariaToggleButton.WindowTitles.Add("Warewolf");
                        #endregion
                    }
                    return this.mUIUI_Assign_QuickVariaToggleButton;
                }
            }
            #endregion

            #region Fields
            private WpfToggleButton mUIUI_Assign_QuickVariaToggleButton;
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




            //WpfButton quickVarButton = GetQuickVariableInputButton(theTab, controlAutomationId);
            //Mouse.Move(new Point(uIUI_Assign_QuickVariaToggleButton.BoundingRectangle.X + 5, uIUI_Assign_QuickVariaToggleButton.BoundingRectangle.Y + 5));
            //Mouse.Click();
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
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait(variableList.Replace("(", "{(}").Replace(")", "{)}"));
            Playback.Wait(1000);
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

            UITestControlCollection qviChildren = qviControl.GetChildren();
            UITestControl addBtn = new UITestControl();
            foreach (UITestControl quickVarInputChildren in qviChildren)
            {
                if (quickVarInputChildren.FriendlyName == "Cancel")
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
            UITestControl cake2 = splurtChildChildren[3];
            UITestControlCollection cake2Children = cake2.GetChildren();
            UITestControl cake38 = cake2Children[3];
            UITestControlCollection cake38Children = cake38.GetChildren();
            // Cake38 -> ActivityTypeDesigner -> Cake53 -> FlowchartDesigner -> *Control Here*
            UITestControl cake53 = cake38Children[0].GetChildren()[0];
            UITestControlCollection cake53Children = cake53.GetChildren();
            UITestControl flowchartDesigner = cake53Children[0];
            return flowchartDesigner;
        }

        public bool DoesActivitDataMappingContainText(UITestControl dsfActivityControl, string searchText)
        {
            //DataMappings view must be expanded on the dsfActivityControl: this cannot be checked here
            var dsfActivityContents = dsfActivityControl.GetChildren();
            foreach (var x in from control in dsfActivityContents
                              where control.ControlType == ControlType.Table && control is WpfTable
                              select (control as WpfTable).GetChildren()
                                  into rows
                                  from row in rows
                                  where row.ControlType == ControlType.Row
                                  select row.GetChildren()
                                      into cells
                                      from cell in cells
                                      where cell.ControlType == ControlType.Cell
                                      where cell.GetChildren().Any(element => (element.ControlType == ControlType.Edit && element is WpfEdit && (element as WpfEdit).Text == searchText))
                                      select cell)
            {
                return true;
            }
            return false;
        }

        public List<UITestControl> Adorner_GetAllTextBoxes(UITestControl theTab)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, "AdornerScrollViewer");
            UITestControlCollection uiTestControlCollection = aControl.GetChildren();

            foreach(UITestControl uiTestControl in uiTestControlCollection)
            {
                if(uiTestControl.GetProperty("AutomationId").ToString() == "LargeViewContent")
                {
                    UITestControlCollection testControlCollection = uiTestControl.GetChildren();
                    List<UITestControl> uiTestControls = testControlCollection.Where(c => c.ClassName == "Uia.TextBox").ToList();
                    return uiTestControls;
                }
            }
            return null;                                 
        }

        public List<UITestControl> Tool_GetAllTextBoxes(UITestControl theTab,string toolAutomationId,string toolDesignerTemplate)
        {
            UITestControl aControl = FindControlByAutomationId(theTab, toolAutomationId);
            UITestControlCollection uiTestControlCollection = aControl.GetChildren();

            foreach (UITestControl uiTestControl in uiTestControlCollection)
            {
                if (uiTestControl.ClassName == toolDesignerTemplate)
                {
                    UITestControlCollection testControlCollection = uiTestControl.GetChildren();
                    List<UITestControl> uiTestControls = testControlCollection.Where(c => c.ClassName == "Uia.TextBox").ToList();
                    return uiTestControls;
                }
            }
            return null;  
        }
    }
}
