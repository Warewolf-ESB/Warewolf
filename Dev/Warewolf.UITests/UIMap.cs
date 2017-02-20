using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;

namespace Warewolf.UITests
{
    [Binding]
    public partial class UIMap
    {
        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        const int _lenientSearchTimeout = 30000;
        const int _lenientMaximumRetryCount = 3;
        const int _strictSearchTimeout = 3000;
        const int _strictMaximumRetryCount = 1;

        public void SetPlaybackSettings()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            Playback.PlaybackSettings.ContinueOnError = false;
#if DEBUG
            Playback.PlaybackSettings.ThinkTimeMultiplier = 2;
#else  
            Playback.PlaybackSettings.ThinkTimeMultiplier = 2;
#endif
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.SkipSetPropertyVerification = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
        }

        [Given("The Warewolf Studio is running")]
        public void AssertStudioIsRunning()
        {
            Assert.IsTrue(MainStudioWindow.Exists, "Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
            Keyboard.SendKeys(MainStudioWindow, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(MainStudioWindow, "^%{F4}");
#if !DEBUG
            var TimeBefore = System.DateTime.Now;
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Console.WriteLine("Waited " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms for the explorer spinner to disappear.");
#endif
        }
        
        public void TryPin_Unpinned_Pane_To_Default_Position()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MainStudioWindow.UnpinnedTab))
                {
                    Restore_Unpinned_Tab_Using_Context_Menu();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to Pin Unpinned Pane To Default Position before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Unpinned Pane to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public bool ControlExistsNow(UITestControl thisControl)
        {
            Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout;
            bool controlExists = false;
            controlExists = thisControl.TryFind();
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout;
            return controlExists;
        }

        [When(@"I Click Unpinned Workflow CollapseAll")]
        public void Click_Unpinned_Workflow_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [When(@"I Click Unpinned Workflow ExpandAll")]
        public void Click_Unpinned_Workflow_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        public void InitializeABlankWorkflow()
        {
            Click_NewWorkflow_RibbonButton();
        }

        [When(@"I Try Clear Toolbox Filter")]
        public void TryClearToolboxFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text != string.Empty)
            {
                Click_Clear_Toolbox_Filter_Clear_Button();
            }
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text == string.Empty, "Toolbox filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text + " is not empty after clicking clear filter button.");
        }

        public void Close_And_Lock_Side_Menu_Bar()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.LockMenuButton);
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer);
            Mouse.Click(MainStudioWindow.SideMenuBar.LockMenuButton);
        }

        public void WaitForControlVisible(UITestControl control, int searchTimeout = 60000)
        {
            control.WaitForControlCondition((uicontrol) =>
            {
                var point = new Point();
                return control.TryGetClickablePoint(out point);
            }, searchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }

        public void WaitForControlEnabled(UITestControl control, int searchTimeout = 60000)
        {
            control.WaitForControlCondition((uicontrol) =>
            {
                return control.Enabled;
            }, searchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }

        public void WaitForControlNotVisible(UITestControl control, int searchTimeout = 60000)
        {
            control.WaitForControlCondition((uicontrol) =>
            {
                var point = new Point();
                return !uicontrol.TryGetClickablePoint(out point);
            }, searchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }

        public void WaitForSpinner(UITestControl control, int searchTimeout = 30000)
        {
            WaitForControlNotVisible(control, searchTimeout);
        }

        [When(@"I Filter the ToolBox with ""(.*)""")]
        public void Filter_ToolBox(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = FilterText;
        }

        [When(@"I Enter Dice Roll Values")]
        public void WhenIEnterDiceRollValues()
        {
            Enter_Dice_Roll_Values();
        }

        [When(@"I Save With Ribbon Button and Dialog As ""(.*)"" and Append Unique Guid")]
        public void WhenISaveWithRibbonButtonAndDialogAsAndAppendUniqueGuid(string p0)
        {
            Save_With_Ribbon_Button_And_Dialog(p0 + Guid.NewGuid().ToString().Substring(0, 8));
        }

        [Given(@"I Save With Ribbon Button And Dialog As ""(.*)""")]
        [When(@"I Save With Ribbon Button And Dialog As ""(.*)""")]
        [Then(@"I Save With Ribbon Button And Dialog As ""(.*)""")]
        public void Save_With_Ribbon_Button_And_Dialog(string Name)
        {
            Click_Save_RibbonButton();
            DialogsUIMap.Enter_Service_Name_Into_Save_Dialog(Name);
            DialogsUIMap.Click_SaveDialog_Save_Button();
        }

        public void Enter_Text_Into_Debug_Input_Row1_Value_Textbox(string text)
        {
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            }
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debug input data row1 textbox text is not equal to \'" + text + "\' after typing that in.");
        }
        
        public void CreateAndSave_Dice_Workflow(string WorkflowName)
        {
            ExplorerUIMap.Select_NewWorkFlowService_From_ContextMenu();
            ToolsUIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            Enter_Dice_Roll_Values();
            Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            Click_Close_Workflow_Tab_Button();
        }

        [Given("I Click Save Ribbon Button Without Expecting a Dialog")]
        [When("I Click Save Ribbon Button Without Expecting a Dialog")]
        [Given("I Click Save Ribbon Button Without Expecting a Dialog")]
        [Then("I Click Save Ribbon Button Without Expecting a Dialog")]
        public void Click_Save_Ribbon_Button_Without_Expecting_A_Dialog()
        {
            Click_Save_Ribbon_Button_With_No_Save_Dialog(2000);
        }

        [Given(@"I Click Save Ribbon Button With No Save Dialog")]
        [When(@"I Click Save Ribbon Button With No Save Dialog")]
        [Then(@"I Click Save Ribbon Button With No Save Dialog")]
        public void Click_Save_Ribbon_Button_With_No_Save_Dialog(int WaitForSave = 2000)
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Exists, "Save ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
        }

        public void Enter_Text_Into_Debug_Input_Row1_Value_Textbox_With_Special_Test_For_Textbox_Height(string text)
        {
            var varValue = MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText;

            var heightBeforeEnterClick = varValue.Height;
            varValue.Text = text;
            Keyboard.SendKeys(varValue, "{Enter}", ModifierKeys.None);
            Assert.IsTrue(varValue.Height > heightBeforeEnterClick, "Debug input dialog does not resize after adding second line.");

            Keyboard.SendKeys(varValue, "{Back}", ModifierKeys.None);
            Assert.AreEqual(heightBeforeEnterClick, varValue.Height, "Debug input dialog value textbox does not resize after deleting second line.");
        }

        public void Press_F5_To_Debug()
        {
            Keyboard.SendKeys(MainStudioWindow, "{F5}", ModifierKeys.None);
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input window does not exist after pressing F5.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.DebugF6Button.Exists, "Debug button in Debug Input window does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.CancelButton.Exists, "Cancel Debug Input Window button does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Exists, "Remember Checkbox does not exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "View in Browser button does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Exists, "Input Data Window does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists, "Xml tab does not Exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.Exists, "Assert Json tab does not exist in the debug input window.");
        }

        [Given(@"I Click New Web Source Test Connection Button")]
        [When(@"I Click New Web Source Test Connection Button")]
        [Then(@"I Click New Web Source Test Connection Button")]
        public void Click_NewWebSource_TestConnectionButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton, new Point(52, 14));
        }

        [Given(@"I Click Close Clean Workflow Tab")]
        [When(@"I Click Close Clean Workflow Tab")]
        [Then(@"I Click Close Clean Workflow Tab")]
        public void ThenIClickCloseCleanWorkflowTab()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton);
        }

        [Given(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        [When(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        [Then(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        public void Click_View_Tests_In_Explorer_Context_Menu(string ServiceName)
        {
            ExplorerUIMap.Filter_Explorer(ServiceName);
            ExplorerUIMap.Show_ExplorerFirstItemTests_With_ExplorerContextMenu();
        }

        [Given(@"I Click View Tests In Explorer Context Menu for Sub Item ""(.*)""")]
        [When(@"I Click View Tests In Explorer Context Menu for Sub Item ""(.*)""")]
        [Then(@"I Click View Tests In Explorer Context Menu for Sub Item ""(.*)""")]
        public void Click_View_Tests_In_Explorer_Context_Menu_For_Sub_Item(string ServiceName)
        {
            ExplorerUIMap.Filter_Explorer(ServiceName);
            ExplorerUIMap.Show_ExplorerFirstSubItemTests_With_ExplorerContextMenu();
        }

        public void Delete_Assign_With_Context_Menu_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on unpinned design surface after deleting with context menu.");
        }

        public void Debug_Workflow_With_Ribbon_Button()
        {
            Click_Debug_RibbonButton();
            Click_DebugInput_Debug_Button();
            WaitForSpinner(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        public void Debug_Unpinned_Workflow_With_F6()
        {
            Press_F6();
            WaitForSpinner(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        [Given(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1 on unpinned tab")]
        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1 on unpinned tab")]
        [Then(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1 on unpinned tab")]
        public void Enter_Variable_And_Value_Into_Assign_On_Unpinned_Tab(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2_On_Unpinned_tab();
                    Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2 on unpinned tab.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab();
                    Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1 on unpinned tab.");
                    break;
            }
        }

        [When(@"I Enter variable text as ""(.*)"" into assign row 1 on unpinned tab")]
        public void Enter_Variable_Into_Assign_Row1_On_Unpinned_Tab(string VariableText)
        {
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.Exists);
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = VariableText;
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1 on unpinned tab.");
        }

        [When(@"I Enter variable text as ""(.*)"" into assign row 2 on unpinned tab")]
        public void Enter_Variable_Into_Assign_Row2_On_Unpinned_Tab(string VariableText)
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = VariableText;
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2 on unpinned tab.");
        }

        public void Pin_Unpinned_Pane_To_Default_Position()
        {
            Mouse.StartDragging(MainStudioWindow.UnpinnedTab, new Point(5, 5));
            Mouse.StopDragging(MainStudioWindow.UnpinnedTab);
        }
        public void Unpin_Tab_With_Drag(UITestControl Tab)
        {
            Mouse.StartDragging(Tab);
            Mouse.StopDragging(0, 21);
            Playback.Wait(2000);
        }

        public void Check_Debug_Input_Dialog_Remember_Inputs_Checkbox()
        {
            MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked = true;
        }

        [Given(@"I Click User Button On Sharepoint Source")]
        [When(@"I Click User Button On Sharepoint Source")]
        [Then(@"I Click User Button On Sharepoint Source")]
        public void Click_UserButton_On_SharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Selected = true;
        }

        [Given(@"I Click Windows Button On Sharepoint Source")]
        [When(@"I Click Windows Button On Sharepoint Source")]
        [Then(@"I Click Windows Button On Sharepoint Source")]
        public void Click_WindowsButton_On_SharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Selected = true;
        }

        public void Enter_Dice_Roll_Values()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.FromComboBox.FromTextEdit.Exists, "From textbox does not exist");
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.FromComboBox.FromTextEdit.Text = "1";
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.ToComboBox.ToTextEdit.Exists, "To textbox does not exist");
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.ToComboBox.ToTextEdit.Text = "6";
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.ResultComboBox.TextEdit.Text = "[[out]]";
        }

        public void Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(2, 10));
            Mouse.StopDragging(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 126));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "MultiAssign does not exist on unpinned tab design surface after dragging from toolbox.");
        }

        public void Toggle_Between_Studio_and_Unpinned_Tab()
        {
            Keyboard.SendKeys(MainStudioWindow, "{ALT}{TAB}");
            Point point;
            Assert.IsFalse(MainStudioWindow.UnpinnedTab.TryGetClickablePoint(out point), "Unpinned pane still visible after Alt+TAB");
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [Given(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        [When(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        [Then(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.AreEqual("SomeVariable", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text does not equal somevariable after using that variable on a unpinned tab.");
        }

        [Given(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        [Then(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [Given(@"I Click Cancel DebugInput Window")]
        [When(@"I Click Cancel DebugInput Window")]
        [Then(@"I Click Cancel DebugInput Window")]
        public void Click_Cancel_DebugInput_Window()
        {
            Mouse.Click(MainStudioWindow.DebugInputDialog.CancelButton, new Point(26, 13));
        }

        [Given(@"I Click Clear Toolbox Filter Clear Button")]
        [When(@"I Click Clear Toolbox Filter Clear Button")]
        [Then(@"I Click Clear Toolbox Filter Clear Button")]
        public void Click_Clear_Toolbox_Filter_Clear_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.ClearFilterButton, new Point(8, 7));
        }

        public void DoubleClick_Toolbox()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(2, 10));
        }

        public void SingleClick_Toolbox()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(2, 10));
        }

        [Given(@"I Click Close Sharepoint Server Source Tab")]
        [When(@"I Click Close Sharepoint Server Source Tab")]
        [Then(@"I Click Close Sharepoint Server Source Tab")]
        public void WhenIClickCloseSharepointServerSourceWizardTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.CloseButton);
        }

        [Given(@"I Click Close WCFService Source Tab Button")]
        [When(@"I Click Close WCFService Source Tab Button")]
        [Then(@"I Click Close WCFService Source Tab Button")]

        public void Click_Close_WCFServiceSource_TabButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.CloseTabButton);
        }

        [Given(@"I Click Close OAuthSource Source Tab Button")]
        [When(@"I Click Close OAuthSource Source Tab Button")]
        [Then(@"I Click Close OAuthSource Source Tab Button")]
        public void Click_OAuthSource_CloseTabButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.CloseTabButton);
        }

        [Given(@"I Click Close Dependecy Tab")]
        [When(@"I Click Close Dependecy Tab")]
        [Then(@"I Click Close Dependecy Tab")]
        public void Click_Close_Dependecy_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.CloseButton, new Point(13, 10));
        }

        [Given(@"I Click Close Deploy Tab Button")]
        [When(@"I Click Close Deploy Tab Button")]
        [Then(@"I Click Close Deploy Tab Button")]
        public void Click_Close_Deploy_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton.Exists, "DeployTab close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton, new Point(16, 6));
        }

        [Given(@"I Click Close FullScreen")]
        [When(@"I Click Close FullScreen")]
        [Then(@"I Click Close FullScreen")]
        public void Click_Close_FullScreen()
        {
            Mouse.Click(MainStudioWindow.ExitFullScreenF11Text.ExitFullScreenF11Hyperlink, new Point(64, 5));
        }

        [Given(@"I Click Close Server Source Wizard Tab Button")]
        [When(@"I Click Close Server Source Wizard Tab Button")]
        [Then(@"I Click Close Server Source Wizard Tab Button")]
        public void Click_Close_Server_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.CloseButton, new Point(5, 5));
        }

        [Given(@"I Click Close Settings Tab Button")]
        [When(@"I Click Close Settings Tab Button")]
        [Then(@"I Click Close Settings Tab Button")]
        public void Click_Close_Settings_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton, new Point(16, 6));
        }

        [Given(@"I Click Close SharepointSource Tab Button")]
        [When(@"I Click Close SharepointSource Tab Button")]
        [Then(@"I Click Close SharepointSource Tab Button")]
        public void Click_Close_SharepointSource_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.CloseButton, new Point(13, 7));
        }

        [Given(@"I Click Close Studio TopRibbon Button")]
        [When(@"I Click Close Studio TopRibbon Button")]
        [Then(@"I Click Close Studio TopRibbon Button")]
        public void Click_Close_Studio_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.CloseStudioButton, new Point(23, 1));
        }

        [Given(@"I Click Close Tab Context Menu Button")]
        [When(@"I Click Close Tab Context Menu Button")]
        [Then(@"I Click Close Tab Context Menu Button")]
        public void Click_Close_Tab_Context_Menu_Button()
        {
            Mouse.Click(MainStudioWindow.TabContextMenu.Close, new Point(27, 13));
        }

        [Given(@"I Click Close Web Source Wizard Tab Button")]
        [When(@"I Click Close Web Source Wizard Tab Button")]
        [Then(@"I Click Close Web Source Wizard Tab Button")]
        public void Click_Close_Web_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.CloseButton, new Point(9, 6));
        }

        [Given(@"I Click Close Workflow Tab Button")]
        [When(@"I Click Close Workflow Tab Button")]
        [Then(@"I Click Close Workflow Tab Button")]
        public void Click_Close_Workflow_Tab_Button()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton, new Point(5, 5));
        }

        [Given(@"I Click ConfigureSetting From Menu")]
        [When(@"I Click ConfigureSetting From Menu")]
        [Then(@"I Click ConfigureSetting From Menu")]
        public void Click_ConfigureSetting_From_Menu()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 13));
            MainStudioWindow.DockManager.SplitPaneMiddle.DrawHighlight();
        }

        [Given(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        [When(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        [Then(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        public void Click_Debug_Output_Assign_Cell_For_Unpinned_Workflow_Tab()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
        }

        [Given(@"I Click DebugInput Cancel Button")]
        [When(@"I Click DebugInput Cancel Button")]
        [Then(@"I Click DebugInput Cancel Button")]
        public void Click_DebugInput_Cancel_Button()
        {
            Mouse.Click(MainStudioWindow.DebugInputDialog.CancelButton, new Point(34, 10));
        }

        [Given(@"I Click DebugInput Debug Button")]
        [When(@"I Click DebugInput Debug Button")]
        [Then(@"I Click DebugInput Debug Button")]
        public void Click_DebugInput_Debug_Button()
        {
            Mouse.Click(MainStudioWindow.DebugInputDialog.DebugF6Button, new Point(34, 10));
        }

        [Given(@"I Click DebugInput ViewInBrowser Button")]
        [When(@"I Click DebugInput ViewInBrowser Button")]
        [Then(@"I Click DebugInput ViewInBrowser Button")]
        public void Click_DebugInput_ViewInBrowser_Button()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "ViewInBrowserF7Button is not enabled after clicking RunDebug from Menu.");
            Mouse.Click(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button, new Point(82, 14));
        }

        [Given(@"I Click First Recordset Input Checkbox")]
        [When(@"I Click First Recordset Input Checkbox")]
        [Then(@"I Click First Recordset Input Checkbox")]
        public void Click_First_Recordset_Input_Checkbox()
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [Given(@"I Click FullScreen TopRibbon Button")]
        [When(@"I Click FullScreen TopRibbon Button")]
        [Then(@"I Click FullScreen TopRibbon Button")]
        public void Click_FullScreen_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeRestoreStudioButton, new Point(12, 9));
        }

        [Given("I Click New Workflow Ribbon Button")]
        [When("I Click New Workflow Ribbon Button")]
        [Then("I Click New Workflow Ribbon Button")]
        public void Click_NewWorkflow_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.NewWorkflowButton, new Point(6, 6));
            WaitForControlVisible(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after clicking new workflow ribbon button.");
        }

        [Given(@"I Click Save Ribbon Button")]
        [When(@"I Click Save Ribbon Button")]
        [Then(@"I Click Save Ribbon Button")]
        public void Click_Save_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton);
        }

        [Given(@"I Click Deploy Ribbon Button")]
        [When(@"I Click Deploy Ribbon Button")]
        [Then(@"I Click Deploy Ribbon Button")]
        public void Click_Deploy_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.DeployButton, new Point(16, 11));
            Playback.Wait(2000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "Deploy tab does not exist after clicking deploy ribbon button.");
        }

        [Given(@"I Click Scheduler Ribbon Button")]
        [When(@"I Click Scheduler Ribbon Button")]
        [Then(@"I Click Scheduler Ribbon Button")]
        public void Click_Scheduler_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SchedulerButton, new Point(4, 12));            
        }

        [Given(@"I Click Debug Ribbon Button")]
        [When(@"I Click Debug Ribbon Button")]
        [Then(@"I Click Debug Ribbon Button")]
        public void Click_Debug_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.RunAndDebugButton, new Point(13, 14));
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input Dialog does not exist after clicking debug ribbon button.");
        }

        [Given(@"I Click Settings Ribbon Button")]
        [When(@"I Click Settings Ribbon Button")]
        [Then(@"I Click Settings Ribbon Button")]
        public void Click_Settings_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 2));
            MainStudioWindow.DockManager.SplitPaneMiddle.DrawHighlight();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.Exists, "Settings tab does not exist after clicking settings ribbon button.");
        }

        [Given(@"I Click Knowledge Ribbon Button")]
        [When(@"I Click Knowledge Ribbon Button")]
        [Then(@"I Click Knowledge Ribbon Button")]
        public void Click_Knowledge_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.KnowledgeBaseButton, new Point(4, 8));
        }

        [Given(@"I Click Lock Ribbon Button")]
        [When(@"I Click Lock Ribbon Button")]
        [Then(@"I Click Lock Ribbon Button")]
        public void Click_Lock_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.LockStudioButton, new Point(14, 5));
        }

        [When(@"I Click Unlock Ribbon Button")]
        public void Click_Unlock_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.LockMenuButton, new Point(8, 6));
        }

        [When(@"I Click Unlock TopRibbon Button")]
        public void Click_Unlock_TopRibbonButton()
        {
            Mouse.Click(MainStudioWindow.LockStudioButton, new Point(10, 12));
        }

        [Given(@"I Click Maximize Restore TopRibbon Button")]
        [When(@"I Click Maximize Restore TopRibbon Button")]
        [Then(@"I Click Maximize Restore TopRibbon Button")]
        public void Click_MaximizeRestore_TopRibbonButton()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(9, 11));
        }

        [Given(@"I Click Maximize TopRibbon Button")]
        [When(@"I Click Maximize TopRibbon Button")]
        [Then(@"I Click Maximize TopRibbon Button")]
        public void Click_Maximize_TopRibbonButton()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(14, 14));
        }

        [Given(@"I Click Minimize TopRibbon Button")]
        [When(@"I Click Minimize TopRibbon Button")]
        [Then(@"I Click Minimize TopRibbon Button")]
        public void Click_Minimize_TopRibbonButton()
        {
            Mouse.Click(MainStudioWindow.MinimizeStudioButton, new Point(6, 14));
        }

        [Given(@"I Click Nested Workflow Name")]
        [When(@"I Click Nested Workflow Name")]
        [Then(@"I Click Nested Workflow Name")]
        public void Click_Nested_Workflow_Name()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.UIHelloWorldTreeItem1.UIHelloWorldButton, new Point(37, 10));
        }

        [Given(@"I Click New Workflow Tab")]
        [When(@"I Click New Workflow Tab")]
        [Then(@"I Click New Workflow Tab")]
        public void Click_New_Workflow_Tab()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, new Point(63, 18));
        }

        [Given(@"I Click NewVersion button")]
        [When(@"I Click NewVersion button")]
        [Then(@"I Click NewVersion button")]
        public void Click_NewVersion_button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.NewVersionButton.Enabled, "New version available button is disabled");
            Mouse.Click(MainStudioWindow.SideMenuBar.NewVersionButton, new Point(17, 9));
        }

        [Given(@"I Click Output OnRecordset InVariableList")]
        [When(@"I Click Output OnRecordset InVariableList")]
        [Then(@"I Click Output OnRecordset InVariableList")]
        public void Click_Output_OnRecordset_InVariableList()
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Output OnVariable InVariableList")]
        [When(@"I Click Output OnVariable InVariableList")]
        [Then(@"I Click Output OnVariable InVariableList")]
        public void Click_Output_OnVariable_InVariableList()
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Pin Toggle DebugOutput")]
        [When(@"I Click Pin Toggle DebugOutput")]
        [Then(@"I Click Pin Toggle DebugOutput")]
        public void Click_Pin_Toggle_DebugOutput()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputUnpinBtn, new Point(11, 10));
        }

        [Given(@"I Click Pin Toggle Documentor")]
        [When(@"I Click Pin Toggle Documentor")]
        [Then(@"I Click Pin Toggle Documentor")]
        public void Click_Pin_Toggle_Documentor()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Help.DocumentorUnpinBtn, new Point(2, 11));
        }

        [Given(@"I Click Pin Toggle Toolbox")]
        [When(@"I Click Pin Toggle Toolbox")]
        [Then(@"I Click Pin Toggle Toolbox")]
        public void Click_Pin_Toggle_Toolbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolUnpinBtn, new Point(10, 8));
        }

        [Given(@"I Click Pin Toggle VariableList")]
        [When(@"I Click Pin Toggle VariableList")]
        [Then(@"I Click Pin Toggle VariableList")]
        public void Click_Pin_Toggle_VariableList()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.VariableUnpinBtn, new Point(10, 14));
        }

        [Given(@"I Click Position Button")]
        [When(@"I Click Position Button")]
        [Then(@"I Click Position Button")]
        public void Click_Position_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.FilesMenu.PositionButton, new Point(8, 7));
        }

        [Given(@"I Click Remove Unused Variables")]
        [When(@"I Click Remove Unused Variables")]
        [Then(@"I Click Remove Unused Variables")]
        public void Click_Remove_Unused_Variables()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused, new Point(30, 4));
        }

        #region SourceWizards.uitest

        [When(@"I Click Sharepoint Server Source TestConnection")]
        public void Click_Sharepoint_Server_Source_TestConnection()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton, new Point(58, 16));
        }

        [Given(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        [When(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        [Then(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress)
        {
            CreateRemoteServerSource(ServerSourceName, ServerAddress, false);
        }

        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress, bool PublicAuth = false)
        {
            Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext
                .NewServerSource.AddressComboBox.AddressEditBox.Text = ServerAddress;
            if (ServerAddress == "tst-ci-")
            {
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.TSTCIREMOTE.Exists, "TSTCIREMOTE does not exist in server source wizard drop down list after starting by typing tst-ci-.");
                ToolsUIMap.Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            }
            if (PublicAuth)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
            }
            Click_Server_Source_Wizard_Test_Connection_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.ErrorText.Spinner);
            Save_With_Ribbon_Button_And_Dialog(ServerSourceName);
            Click_Close_Server_Source_Wizard_Tab_Button();
        }

        public void Enter_TextIntoOAuthKey_On_OAuthSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.OAuthKeyTextBox.Text = "test";
        }

        public void Click_OAuthSource_AuthoriseButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.AuthoriseButton);
        }

        public void Select_AssemblyFile_From_COMPluginDataTree(string filter)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Text = filter;
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Nodes[1]);
        }

        [Given(@"I Click Close COMPlugin Source Tab Button")]
        [When(@"I Click Close COMPlugin Source Tab Button")]
        [Then(@"I Click Close COMPlugin Source Tab Button")]
        public void Click_COMPluginSource_CloseTabButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.CloseTabButton);
        }

        public void Enter_Text_Into_Exchange_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Text = "https://outlook.office365.com/EWS/Exchange.asmx";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.UserNameTextBox.Text = "Nkosinathi.Sangweni@TheUnlimited.co.za";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.PasswordTextBox.Text = "Password123";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Text = "dev2warewolf@gmail.com";
        }

        [When(@"I Click ExchangeSource TestConnection Button")]
        public void Click_ExchangeSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.Spinner);
        }

        [Given(@"I Click Close Exchange Source Tab Button")]
        [When(@"I Click Close Exchange Source Tab Button")]
        [Then(@"I Click Close RabExchangebitMQ Source Tab Button")]
        public void Click_ExchangeSource_CloseTabButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.CloseButton);
        }

        [Given(@"I Click EmailSource TestConnection Button")]
        [When(@"I Click EmailSource TestConnection Button")]
        [Then(@"I Click EmailSource TestConnection Button")]
        public void Click_EmailSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.Spinner);
        }

        [Given(@"I Click Close EmailSource Tab")]
        [When(@"I Click Close EmailSource Tab")]
        [Then(@"I Click Close EmailSource Tab")]
        public void Click_Close_EmailSource_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.CloseButton, new Point(13, 10));
        }

        public void Enter_Text_Into_EmailSource_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.HostTextBoxEdit.Text = "localhost";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.UserNameTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PasswordTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PortTextBoxEdit.Text = "2";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.FromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.Text = "dev2warewolf@gmail.com";
        }

        public void Edit_Timeout_On_EmailSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text = "2000";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.FromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.Text = "dev2warewolf@gmail.com";
        }

        public void Edit_Timeout_On_ExchangeSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text = "2000";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Text = "dev2warewolf@gmail.com";
        }

        public void Enter_Text_On_RabbitMQSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.HostTextBoxEdit.Text = "rsaklfsvrgendev";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PortTextBoxEdit.Text = "5672";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.UserNameTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PasswordTextBoxEdit.Text = "test";
        }

        public void Click_RabbitMQSource_TestConnectionButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton);
        }

        [Given(@"I RabbitMqAsserts")]
        [When(@"I RabbitMqAsserts")]
        [Then(@"I RabbitMqAsserts")]
        public void RabbitMqAsserts()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Exists, "VirtualHoast textbox does not exist after opening RabbitMq Source tab");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PasswordTextBoxEdit.Exists, "Password textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.UserNameTextBoxEdit.Exists, "Username textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.HostTextBoxEdit.Exists, "Host textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PortTextBoxEdit.Exists, "Port textbox does not exist after opening RabbitMq Source");
        }

        [Given(@"I Click Close RabbitMQ Source Tab Button")]
        [When(@"I Click Close RabbitMQ Source Tab Button")]
        [Then(@"I Click Close RabbitMQ Source Tab Button")]
        public void Click_Close_RabbitMQSource_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.CloseTabButton, new Point(13, 4));
        }

        [When(@"I Click Server Source Wizard Address Protocol Dropdown")]
        public void Click_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
        }

        [When(@"I Click Server Source Wizard Test Connection Button")]
        public void Click_Server_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton, new Point(51, 8));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.Spinner);
        }

        public void Click_ConfigFileDirectoryButton_On_DotnetPluginSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton);
            Assert.IsTrue(DialogsUIMap.SelectFilesWindow.Exists, "Select Files Window did not open after clicking Assembly Directory Button");
        }

        [Given(@"I Click Close DotNetPlugin Source Tab")]
        [When(@"I Click Close DotNetPlugin Source Tab")]
        [Then(@"I Click Close DotNetPlugin Source Tab")]
        public void Click_Close_DotNetPlugin_Source_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.CloseButton, new Point(13, 4));
        }

        [When(@"I Type ""(.*)"" into Plugin Source Wizard Assembly Textbox")]
        public void Type_dll_into_Plugin_Source_Wizard_Assembly_Textbox(string text)
        {
            if (!File.Exists(text))
            {
                text = text.Replace("Framework64", "Framework");
                if (!File.Exists(text))
                {
                    throw new Exception("No suitable DLL could be found for this test to use.");
                }
            }
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text = text;
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after DLL has been selected in plugin source wizard.");
        }

        [Then(@"I Enter Text Into Database Server Tab")]
        [Given(@"I Enter Text Into Database Server Tab")]
        [Then(@"I Enter Text Into Database Server Tab")]
        public void Enter_Text_Into_DatabaseServer_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "RSAKLFSVRGENDEV";
        }

        [When(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        [Given(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        [Then(@"I Enter RunAsUser(Root) Username And Password on Database source")]
        public void IEnterRunAsUserRootOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "root";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "admin";
        }

        [When(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        [Given(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        [Then(@"I Enter RunAsUser(PostGres) Username And Password on Database source")]
        public void IEnterRunAsUserPostGresOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "postgres";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [When(@"I Select mysql From DB Source Wizard Database Combobox")]
        [Given(@"I Select mysql From DB Source Wizard Database Combobox")]
        [Then(@"I Select mysql From DB Source Wizard Database Combobox")]
        public void Select_mysql_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsmysqlDB);
            Assert.AreEqual("mysql", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIMysqlText.DisplayText);
        }

        [When(@"I Select postgres From DB Source Wizard Database Combobox")]
        [Given(@"I Select postgres From DB Source Wizard Database Combobox")]
        [Then(@"I Select postgres From DB Source Wizard Database Combobox")]
        public void Select_postgres_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAspostgresDB);
            Assert.AreEqual("postgres", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIPostgresText.DisplayText);
        }

        [When(@"I Select HR From DB Source Wizard Database Combobox")]
        [Given(@"I Select HR From DB Source Wizard Database Combobox")]
        [Then(@"I Select HR From DB Source Wizard Database Combobox")]
        public void Select_HR_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsHRDB);
            Assert.AreEqual("HR", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIHRText.DisplayText);
        }

        [When(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        [Given(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        [Then(@"I Select ExcelFiles From DB Source Wizard Database Combobox")]
        public void Select_ExcelFiles_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsExcelFilesDB);
            Assert.AreEqual("Excel Files", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIExcelFilesText.DisplayText);
        }

        [When(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        [Given(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        [Then(@"I Select MSAccess From DB Source Wizard Database Combobox")]
        public void Select_MSAccess_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsMSAccessDB);
            Assert.AreEqual("MS Access Database", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.MSAccessDatabaseText.DisplayText);
        }

        [When(@"I Select TestDB From DB Source Wizard Database Combobox")]
        [Given(@"I Select TestDB From DB Source Wizard Database Combobox")]
        [Then(@"I Select TestDB From DB Source Wizard Database Combobox")]
        public void Select_TestDB_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsTestDB);
            Assert.AreEqual("TestDB", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.TestDBText.DisplayText);
        }

        [When(@"I Select test From DB Source Wizard Database Combobox")]
        [Given(@"I Select test From DB Source Wizard Database Combobox")]
        [Then(@"I Select test From DB Source Wizard Database Combobox")]
        public void Select_test_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAstest);
            Assert.AreEqual("test", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.testText.DisplayText);
        }

        [When(@"I Select master From DB Source Wizard Database Combobox")]
        [Given(@"I Select master From DB Source Wizard Database Combobox")]
        [Then(@"I Select master From DB Source Wizard Database Combobox")]
        public void Select_master_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsmaster);
            Assert.AreEqual("master", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.masterText.DisplayText);
        }

        [When(@"I Select Dev2TestingDB From DB Source Wizard Database Combobox")]
        public void Select_Dev2TestingDB_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox);
            Mouse.Click(MainStudioWindow.Dev2TestingDBCustom);
            Assert.AreEqual("Dev2TestingDB", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.UIDev2TestingDBText.DisplayText);
        }

        [When(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        public void Type_rsaklfsvrgen_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "rsaklfsvrgen";
        }

        [Given(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        [Then(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        public void Assert_RSAKLFSVRGENDEV_appears_as_an_option_in_the_DB_source_wizard_server_combobox()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV.Exists, "RSAKLFSVRGENDEV does not exist as an option in DB source wizard server combobox.");
        }

        [When(@"I Type RSAKLFSVRGENDEV into DB Source Wizard Server Textbox")]
        public void Type_RSAKLFSVRGENDEV_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "RSAKLFSVRGENDEV";
        }

        [When(@"I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist")]
        public void Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV, new Point(97, 17));
            Assert.AreEqual("RSAKLFSVRGENDEV", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text, "RSAKLFSVRGENDEV is not selected as the server in the DB source wizard.");
        }

        [Given(@"I Click Close DB Source Wizard Tab Button")]
        [When(@"I Click Close DB Source Wizard Tab Button")]
        [Then(@"I Click Close DB Source Wizard Tab Button")]
        public void Click_Close_DB_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.CloseButton, new Point(13, 4));
        }

        [Given(@"I Click UserButton On Database Source")]
        [When(@"I Click UserButton On Database Source")]
        [Then(@"I Click UserButton On Database Source")]
        public void Click_UserButton_On_DatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        [Given(@"I Click WindowsButton On Database Source")]
        [When(@"I Click WindowsButton On Database Source")]
        [Then(@"I Click WindowsButton On Database Source")]
        public void Click_WindowsButton_On_DatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.WindowsRadioButton);
        }

        [Given(@"I Enter TestUser Username And Password on Database source")]
        [When(@"I Enter TestUser Username And Password on Database source")]
        [Then(@"I Enter TestUser Username And Password on Database source")]
        public void IEnterRunAsUserTestUserOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "testuser";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [Given(@"I Click DB Source Wizard Test Connection Button")]
        [When(@"I Click DB Source Wizard Test Connection Button")]
        [Then(@"I Click DB Source Wizard Test Connection Button")]
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseComboxBox.Exists, "Database Combobox is not visible.");
        }


        [Given(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [When(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [Then(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        public void Assert_The_DB_Source_Wizard_Test_Succeeded_Image_Is_Visible()
        {
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceTab.WorkSurfaceContext.ConnectionPassedImage.TryGetClickablePoint(out point), "New DB source wizard test succeeded image is not visible after testing with RSAKLFSVRGENDEV and waiting for the spinner.");
        }

        #endregion

        [Given(@"I Press F6")]
        [When(@"I Press F6")]
        [Then(@"I Press F6")]
        public void Press_F6()
        {
            Keyboard.SendKeys(MainStudioWindow, "{F6}", ModifierKeys.None);
        }

        [Given(@"I Press F6 On Unpinned Tab")]
        [When(@"I Press F6 On Unpinned Tab")]
        [Given(@"I Press F6 On Unpinned Tab")]
        public void Press_F6_On_UnPinnedTab()
        {
            Keyboard.SendKeys(MainStudioWindow.UnpinnedTab, "{F6}", ModifierKeys.None);
        }

        [Given(@"I PressF11 EnterFullScreen")]
        [When(@"I PressF11 EnterFullScreen")]
        [Then(@"I PressF11 EnterFullScreen")]
        public void PressF11_EnterFullScreen()
        {
            Keyboard.SendKeys(MainStudioWindow, "{F11}", ModifierKeys.None);
        }

        [Given(@"I Select Rename From Explorer Context Menu")]
        [When(@"I Select Rename From Explorer Context Menu")]
        [Then(@"I Select Rename From Explorer Context Menu")]
        public void Select_Rename_From_Explorer_ContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Rename);
        }

        [Given(@"I Restore Unpinned Tab Using Context Menu")]
        [When(@"I Restore Unpinned Tab Using Context Menu")]
        [Then(@"I Restore Unpinned Tab Using Context Menu")]
        public void Restore_Unpinned_Tab_Using_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab, MouseButtons.Right, ModifierKeys.None, new Point(14, 12));
            MainStudioWindow.UnpinnedTabContextMenu.TabbedDocument.Checked = true;
        }

        [Given(@"I Right Click Help Tab")]
        [When(@"I Right Click Help Tab")]
        [Then(@"I Right Click Help Tab")]
        public void Right_Click_Help_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.HelpTab, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
        }

        [When(@"I Select Copy FromContextMenu")]
        public void Select_Copy_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Copy, new Point(27, 18));
        }

        [When(@"I Select CopyAsImage FromContextMenu")]
        public void Select_CopyAsImage_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.CopyasImage, new Point(62, 22));
        }

        [When(@"I Select Cut FromContextMenu")]
        public void Select_Cut_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Cut, new Point(53, 16));
        }

        [When(@"I Select DatabaseAndTable From BulkInsert Tool")]
        public void Select_DatabaseAndTable_From_BulkInsert_Tool()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox);
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox.TestingDB);
        }

        [When(@"I Select DeleteRow FromContextMenu")]
        public void Select_DeleteRow_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRow, new Point(74, 9));
        }

        [When(@"I Select http From Server Source Wizard Address Protocol Dropdown")]
        public void Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsHttp, new Point(31, 12));
            Assert.AreEqual("http", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.HttpSelectedItemText.DisplayText, "Server source wizard address protocol is not equal to http.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Exists, "Server source wizard address textbox does not exist");
        }

        [When(@"I Select InsertRow FromContextMenu")]
        public void Select_InsertRow_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.InsertRow, new Point(66, 19));
        }

        [When(@"I Select MSSQLSERVER From DB Source Wizard Address Protocol Dropdown")]
        public void Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_Dropdown()
        {
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsMicrosoftSQLServer.MicrosoftSQLServerText.Exists, "Microsoft SQL Server does not exist as an option in new DB source wizard type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsMicrosoftSQLServer.MicrosoftSQLServerText, new Point(118, 6));
        }

        [Given(@"I Select Open FromExplorerContextMenu")]
        [When(@"I Select Open FromExplorerContextMenu")]
        [Then(@"I Select Open FromExplorerContextMenu")]
        public void Select_Open_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Open);
        }
        
        [When(@"I Select Paste FromContextMenu")]
        public void Select_Paste_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Paste, new Point(52, 16));
        }

        public void Select_Server_Authentication_Public()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
        }

        [When(@"I Select SaveAsImage FromContextMenu")]
        public void Select_SaveAsImage_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.SaveasImage, new Point(38, 15));
        }

        [When(@"I Select SetAsStartNode FromContextMenu")]
        public void Select_SetAsStartNode_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.SetasStartNode, new Point(67, 16));
        }

        [When(@"I Select ShowLargeView FromContextMenu")]
        public void Select_ShowLargeView_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.ShowLargeView, new Point(43, 15));
        }

        [Then(@"I Click Create Test From Debug")]
        [Given(@"I Click Create Test From Debug")]
        [When(@"I Click Create Test From Debug")]
        public void Click_Create_Test_From_Debug()
        {
            int CreateTestButtonEnabledTimeout = 60000;
            WaitForControlEnabled(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton, CreateTestButtonEnabledTimeout);
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Enabled, "Debug Output New Test button not enabled after waiting for " + CreateTestButtonEnabledTimeout + "ms.");
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton, new Point(5, 5));
        }

        public void Save_Button_IsEnabled()
        {
            MainStudioWindow.SideMenuBar.SaveButton.EnsureClickable();
        }

        [Then(@"Hello World Workflow Tab Is Open")]
        [Given(@"Hello World Workflow Tab Is Open")]
        public void Hello_World_Workflow_Tab_Is_Open()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.UIHelloWorldText.Exists, "Hello World workflow tab does not exist.");
        }

        [When(@"I Expand Debug Output Recordset")]
        public void Expand_Debug_Output_Recordset()
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.Expanded = true;
        }

        [Then(@"The GetCountries Recordset Is Visible in Debug Output")]
        public void ThenTheDebugOutputShowsGetCountriesRecordset()
        {
            Assert.AreEqual("[[dbo_GetCountries(204).CountryID]]", ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetName.DisplayText, "Wrong recordset name in debug output for new DB connector.");
            Assert.AreEqual("155", ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetValue.DisplayText, "Wrong recordset value in debug output for new DB connector.");
        }

        public void Open_Deploy_Using_Shortcut()
        {
            Keyboard.SendKeys("D", (ModifierKeys.Control));
        }

        [Given(@"Unit Tests Url Exists")]
        [When(@"Unit Tests Url Exists")]
        [Then(@"Unit Tests Url Exists")]
        public void UnitTestUrlExists()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnitTestsUrlWorkflowUrlText.UnitTestsUrlWorkflowUrlHyperlink.Exists, "UnitTestsUrlWorkflowUrl does not exist");
        }

        [Given(@"Resource Did not Open")]
        [When(@"Resource Did not Open")]
        [Then(@"Resource Did not Open")]
        public void ResourceDidNotOpen()
        {
            WaitForControlVisible(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsFalse(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("Hello World"));
        }

        [Given(@"I Filter Variable List ""(.*)""")]
        [When(@"I Filter Variable List ""(.*)""")]
        [Then(@"I Filter Variable List ""(.*)""")]
        public void Filter_VariableList(string text)
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = text;
        }

        [Given(@"I Click Clear Variable List Filter")]
        [When(@"I Click Clear Variable List Filter")]
        [Then(@"I Click Clear Variable List Filter")]
        public void Click_Clear_Variable_List_Filter()
        {
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton);
        }

        public void Set_Input_Output_Variables()
        {
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
            ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.OutputCheckbox.Checked = true;
        }

        [Given(@"I drag a ""(.*)"" tool")]
        [When(@"I drag a ""(.*)"" tool")]
        [Then(@"I drag a ""(.*)"" tool")]
        public void WhenIDragATool(string tool)
        {
            ToolsUIMap.Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface();
        }

        public void Click_AssemblyDirectoryButton_On_DotnetPluginSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyDirectoryButton);
        }

        [Given(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        [When(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        [Then(@"I change Server Authentication From Deploy And Validate Changes From Explorer")]
        public void ChangeServerAuthenticationFromDeployAndValidateChangesFromExplorer()
        {
            var windowsRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton;
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;

            if (publicRadioButton.Selected)
            {
                windowsRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                ExplorerUIMap.Select_RemoteConnectionIntegration_From_Explorer();
                ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
                Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Selected, "Windows Radio Button not selected.");
            }
            else
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                ExplorerUIMap.Select_RemoteConnectionIntegration_From_Explorer();
                ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
                Playback.Wait(1000);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
                Click_Deploy_Ribbon_Button();
                DeployUIMap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                DeployUIMap.Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Selected, "Public Radio Button not selected.");
            }
        }

        [Given(@"I set AuthenticationType to Public")]
        [When(@"I set AuthenticationType to Public")]
        [Then(@"I set AuthenticationType to Public")]
        public void ChangeServerAuthenticationTypeToPublic()
        {
            ExplorerUIMap.Click_Explorer_RemoteServer_Edit_Button();
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;
            if (!publicRadioButton.Selected)
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Click_Close_Server_Source_Wizard_Tab_Button();
            }
            else
            {
                Click_Close_Server_Source_Wizard_Tab_Button();
            }
        }
        
        public void Click_UserButton_On_ServerSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        public void Enter_TextIntoAddress_On_ServerSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Text = "RSAKLFSVRGENDEV";
        }

        public void Enter_RunAsUser_On_ServerSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UsernameTextBox.Text = "IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "I73573r0";
        }

        public void Click_UserButton_On_WebServiceSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton);
        }

        public void Click_AnonymousButton_On_WebServiceSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton);
        }

        public void Enter_TextIntoAddress_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://RSAKLFSVRTFSBLD:9810";
        }

        public void Enter_RunAsUser_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserNameTextBox.Text = "IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.PasswordTextBox.Text = "I73573r0";
        }

        public void Enter_DefaultQuery_On_WebServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Text = "";
        }

        public void Enter_TextIntoAddress_In_SharepointServiceSourceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Text = "http://rsaklfsvrsharep";
        }

        public void Enter_TextIntoAddress_On_WCFServiceTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.WCFEndpointURLEdit.Text = "test";
        }

        public void Click_WCFServiceSource_TestConnectionButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.TestConnectionButton);
        }

        public void CreateAttachmentsForTest(string filepath)
        {
            var fileStream = File.Create(filepath);
            fileStream.Close();
        }

        public void CreateFolderForAttachments(string folderName)
        {
            Directory.CreateDirectory(folderName);
        }

        public void RemoveTestFiles(string filePath1, string filePath2, string folderName)
        {
            if (File.Exists(filePath1))
            {
                File.Delete(filePath1);
                File.Delete(filePath2);
                Directory.Delete(folderName);
                Assert.IsFalse(Directory.Exists(folderName));
            }
        }

        public void Change_Dll_And_Save(string newDll)
        {
            AssemblyComboBox assembly = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox;            
            assembly.TextEdit.Text = newDll;
            Keyboard.SendKeys(assembly.TextEdit, "S", (ModifierKeys.Control));
        }
    }
}
