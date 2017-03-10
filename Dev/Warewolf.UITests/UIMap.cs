using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;

namespace Warewolf.UITests
{
    [Binding]
    public partial class UIMap
    {
        const int _lenientSearchTimeout = 30000;
        const int _lenientMaximumRetryCount = 3;
        const int _strictSearchTimeout = 3000;
        const int _strictMaximumRetryCount = 1;

        public static void SetPlaybackSettings()
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
            Mouse.MouseMoveSpeed = 2500;
            Mouse.MouseDragSpeed = 2500;

            Playback.PlaybackError -= OnPlaybackError;
            Playback.PlaybackError += OnPlaybackError;
        }

        private static void OnPlaybackError(object sender, PlaybackErrorEventArgs e)
        {
            var errorType = e.Error.GetType().ToString();
            string messageText;
            object exceptionSource;
            switch (errorType)
            {
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotAvailableException":
                    messageText = errorType + "\n" + e.Error.Message;
                    exceptionSource = (e.Error as UITestControlNotAvailableException).ExceptionSource;
                    if (exceptionSource is UITestControl)
                    {
                        Console.WriteLine(messageText + "\n" + (exceptionSource as UITestControl).FriendlyName);
                    }
                    e.Result = PlaybackErrorOptions.Retry;
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.FailedToPerformActionOnBlockedControlException":
                    messageText = errorType + "\n" + e.Error.Message;
                    exceptionSource = (e.Error as FailedToPerformActionOnBlockedControlException).ExceptionSource;
                    if (exceptionSource is UITestControl)
                    {
                        Console.WriteLine(messageText + "\n" + (exceptionSource as UITestControl).FriendlyName);
                    }
                    e.Result = PlaybackErrorOptions.Retry;
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException":
                    messageText = errorType + "\n" + e.Error.Message;
                    exceptionSource = (e.Error as UITestControlNotFoundException).ExceptionSource;
                    if (exceptionSource is UITestControl)
                    {
                        Console.WriteLine(messageText + "\n" + (exceptionSource as UITestControl).FriendlyName);
                    }
                    e.Result = PlaybackErrorOptions.Retry;
                    break;
            }
        }

        [Given("The Warewolf Studio is running")]
        public void AssertStudioIsRunning()
        {
            Assert.IsTrue(MainStudioWindow.Exists, "Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
            Keyboard.SendKeys(MainStudioWindow, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(MainStudioWindow, "^%{F4}");
#if !DEBUG
            var TimeBefore = System.DateTime.Now;
            WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
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
            WorkflowTabUIMap.Drag_Toolbox_Random_Onto_DesignSurface();
            UtilityToolsUIMap.Enter_Dice_Roll_Values();
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

        [Given(@"I Click Close Clean Workflow Tab")]
        [When(@"I Click Close Clean Workflow Tab")]
        [Then(@"I Click Close Clean Workflow Tab")]
        public void ThenIClickCloseCleanWorkflowTab()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton);
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
            WaitForSpinner(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
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
            Assert.AreEqual("", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
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

        [Given(@"I Click Close FullScreen")]
        [When(@"I Click Close FullScreen")]
        [Then(@"I Click Close FullScreen")]
        public void Click_Close_FullScreen()
        {
            Mouse.Click(MainStudioWindow.ExitFullScreenF11Text.ExitFullScreenF11Hyperlink, new Point(64, 5));
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

        [Given(@"I Click Close Workflow Tab Button")]
        [When(@"I Click Close Workflow Tab Button")]
        [Then(@"I Click Close Workflow Tab Button")]
        public void Click_Close_Workflow_Tab_Button()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton, new Point(5, 5));
        }

        [Given(@"I Click ConfigureSetting From Menu")]
        [When(@"I Click ConfigureSetting From Menu")]
        [Then(@"I Click ConfigureSetting From Menu")]
        public void Click_ConfigureSetting_From_Menu()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 13));
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
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox.Checked = true;
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
            WaitForControlVisible(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after clicking new workflow ribbon button.");
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
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "Deploy tab does not exist after clicking deploy ribbon button.");
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
            MainStudioWindow.DebugInputDialog.WaitForControlExist(60000);
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input Dialog does not exist after clicking debug ribbon button.");
        }

        [Given(@"I Click Settings Ribbon Button")]
        [When(@"I Click Settings Ribbon Button")]
        [Then(@"I Click Settings Ribbon Button")]
        public void Click_Settings_RibbonButton()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 2));
            MainStudioWindow.DockManager.SplitPaneMiddle.DrawHighlight();
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.Exists, "Settings tab does not exist after clicking settings ribbon button.");
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
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.UIHelloWorldTreeItem1.UIHelloWorldButton, new Point(37, 10));
        }

        [Given(@"I Click New Workflow Tab")]
        [When(@"I Click New Workflow Tab")]
        [Then(@"I Click New Workflow Tab")]
        public void Click_New_Workflow_Tab()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, new Point(63, 18));
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
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Output OnVariable InVariableList")]
        [When(@"I Click Output OnVariable InVariableList")]
        [Then(@"I Click Output OnVariable InVariableList")]
        public void Click_Output_OnVariable_InVariableList()
        {
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Pin Toggle DebugOutput")]
        [When(@"I Click Pin Toggle DebugOutput")]
        [Then(@"I Click Pin Toggle DebugOutput")]
        public void Click_Pin_Toggle_DebugOutput()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputUnpinBtn, new Point(11, 10));
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
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.VariableUnpinBtn, new Point(10, 14));
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
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused, new Point(30, 4));
        }

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

        [When(@"I Select DeleteRow FromContextMenu")]
        public void Select_DeleteRow_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRow, new Point(74, 9));
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
            WaitForControlEnabled(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton, CreateTestButtonEnabledTimeout);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton.Enabled, "Debug Output New Test button not enabled after waiting for " + CreateTestButtonEnabledTimeout + "ms.");
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton, new Point(5, 5));
        }

        public void Save_Button_IsEnabled()
        {
            MainStudioWindow.SideMenuBar.SaveButton.EnsureClickable();
        }

        [Then(@"Hello World Workflow Tab Is Open")]
        [Given(@"Hello World Workflow Tab Is Open")]
        public void Hello_World_Workflow_Tab_Is_Open()
        {
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.UIHelloWorldText.Exists, "Hello World workflow tab does not exist.");
        }

        [When(@"I Expand Debug Output Recordset")]
        public void Expand_Debug_Output_Recordset()
        {
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.Expanded = true;
        }

        [Then(@"The GetCountries Recordset Is Visible in Debug Output")]
        public void ThenTheDebugOutputShowsGetCountriesRecordset()
        {
            Assert.AreEqual("[[dbo_GetCountries(204).CountryID]]", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetName.DisplayText, "Wrong recordset name in debug output for new DB connector.");
            Assert.AreEqual("155", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetValue.DisplayText, "Wrong recordset value in debug output for new DB connector.");
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
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnitTestsUrlWorkflowUrlText.UnitTestsUrlWorkflowUrlHyperlink.Exists, "UnitTestsUrlWorkflowUrl does not exist");
        }

        [Given(@"Resource Did not Open")]
        [When(@"Resource Did not Open")]
        [Then(@"Resource Did not Open")]
        public void ResourceDidNotOpen()
        {
            WaitForControlVisible(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsFalse(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("Hello World"));
        }

        [Given(@"I Filter Variable List ""(.*)""")]
        [When(@"I Filter Variable List ""(.*)""")]
        [Then(@"I Filter Variable List ""(.*)""")]
        public void Filter_VariableList(string text)
        {
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = text;
        }

        [Given(@"I Click Clear Variable List Filter")]
        [When(@"I Click Clear Variable List Filter")]
        [Then(@"I Click Clear Variable List Filter")]
        public void Click_Clear_Variable_List_Filter()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton);
        }

        public void Set_Input_Output_Variables()
        {
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.OutputCheckbox.Checked = true;
        }

        [Given(@"I drag a ""(.*)"" tool")]
        [When(@"I drag a ""(.*)"" tool")]
        [Then(@"I drag a ""(.*)"" tool")]
        public void WhenIDragATool(string tool)
        {
            WorkflowTabUIMap.Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface();
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

        #region UIMaps
        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

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

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        #endregion
    }
}
