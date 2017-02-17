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

        public UITestControl FindAddResourceButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(0);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public WpfText FindSelectedResourceText(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(0);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Text) as WpfText;
        }

        public UITestControl FindAddWindowsGroupButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(1);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public WpfEdit FindWindowsGroupTextbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(1);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Edit) as WpfEdit;
        }

        public WpfCheckBox FindViewPermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(2);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public WpfCheckBox FindExecutePermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(3);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public WpfCheckBox FindContributePermissionsCheckbox(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(4);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.CheckBox) as WpfCheckBox;
        }

        public UITestControl FindAddRemoveRowButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(5);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public void Close_And_Lock_Side_Menu_Bar()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.LockMenuButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer);
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
            Enter_Service_Name_Into_Save_Dialog(Name);
            Click_SaveDialog_Save_Button();
        }

        public void Enter_Text_Into_Debug_Input_Row1_Value_Textbox(string text)
        {
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            }
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debug input data row1 textbox text is not equal to \'" + text + "\' after typing that in.");
        }

        [Given(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [When(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [Then(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        public void SetResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {
            Click_Settings_RibbonButton();
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Select_SubItem_Service_From_Service_Picker_Dialog(ResourceName);
            Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(WindowsGroupName);
            if (setView)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            }
            if (setExecute)
            {
                Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            }
            if (setContribute)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox();
            }
            Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }

        public void Set_FirstResource_ResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {            
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Select_First_Service_From_Service_Picker_Dialog(ResourceName);
            Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(WindowsGroupName);
            if (setView)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox();
            }
            if (setExecute)
            {
                Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox();
            }
            if (setContribute)
            {
                Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox();
            }
            Click_Save_Ribbon_Button_With_No_Save_Dialog();
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

        [When(@"I Select ""(.*)"" from the source tab")]
        [Then(@"I Select ""(.*)"" from the source tab")]
        [Given(@"I Select ""(.*)"" from the source tab")]
        public void WhenISelectFromTheSourceTab(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
            Select_Deploy_First_Source_Item();
        }

        [When(@"I filter for ""(.*)"" on the source filter")]
        [Then(@"I filter for ""(.*)"" on the source filter")]
        [Given(@"I filter for ""(.*)"" on the source filter")]
        public void WhenIFilterForOnTheSourceFilter(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
        }

        [Then(@"I Click Deploy button")]
        [Given(@"I Click Deploy button")]
        [When(@"I Click Deploy button")]
        public void ThenIClickDeployButton()
        {
            Click_Deploy_Tab_Deploy_Button();
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

        [Given("Dice Is Selected InSettings Tab Permissions Row 1")]
        [When(@"I Assert Dice Is Selected InSettings Tab Permissions Row1")]
        [Then("Dice Is Selected InSettings Tab Permissions Row 1")]
        public void Assert_Dice_Is_Selected_InSettings_Tab_Permissions_Row_1()
        {
            Assert.AreEqual("Dice1", FindSelectedResourceText(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
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

        #region Scheduler\Scheduler.uitest

        public void Create_Scheduler_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList, new Point(151, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList, "N", ModifierKeys.Control);
        }

        [When(@"I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab")]
        public void Enter_LocalSchedulerAdminCredentials_Into_SchedulerTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.UserNameTextBoxEdit.Text = @"Warewolf Administrators\IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.PasswordTextbox.Text = "I73573r0";
        }

        [When(@"I Click Scheduler Create New Task Button")]
        public void Click_Scheduler_NewTaskButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.ScheduleNewTaskListItem.SchedulerNewTaskButton, new Point(151, 13));
        }

        public void Click_SchedulerTab_CloseButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.CloseButton);
        }

        [When(@"I Click Hello World Erase Schedule Button")]
        public void Click_HelloWorldSchedule_EraseSchedulerButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.EraseScheduleButton, new Point(6, 16));
        }

        [When(@"I Click Scheduler Enable Disable Checkbox Button")]
        public void Click_HelloWorldSchedule_EnableOrDisableCheckbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.EnableOrDisableCheckBox);
        }

        [When(@"I Click Scheduler ResourcePicker Button")]
        public void Click_Scheduler_ResourcePickerButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton, new Point(14, 13));
        }

        #endregion
        #region SourceWizards.uitest

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
            Assert.IsTrue(SelectFilesWindow.Exists, "Select Files Window did not open after clicking Assembly Directory Button");
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
        #region Settings\Settings.uitest

        [When(@"I Select SecurityTab")]
        public void Select_SecurityTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab, new Point(102, 10));
        }

        [When(@"I Select PerfomanceCounterTab")]
        public void Select_PerfomanceCounterTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab, new Point(124, 14));
        }

        [When(@"I Select LoggingTab")]
        public void Select_LoggingTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab, new Point(57, 7));
        }

        public void Click_Settings_Resource_Permissions_Row1_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service picker dialog does not exist.");
        }

        [When(@"I Click Select Resource Button From Resource Permissions")]
        public void Click_Select_Resource_Button_From_Resource_Permissions()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1), new Point(13, 16));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service window does not exist after clicking SelectResource button");
        }

        [When(@"I Click Reset Perfomance Counter")]
        public void Click_Reset_Perfomance_Counter()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResetCounter.ItemHyperlink, new Point(49, 9));
            Assert.IsTrue(MessageBoxWindow.Exists, "MessageBoxWindow did not show after clicking reset counters");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(50, 12));
        }

        [When(@"I Click Select Resource Button")]
        public void Click_Select_ResourceButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceButton, new Point(9, 8));
        }

        [Given(@"I Check Public Administrator")]
        [When(@"I Check Public Administrator")]
        [Then(@"I Check Public Administrator")]
        public void Check_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = true;
        }

        [Given(@"I Check Public Deploy To")]
        [When(@"I Check Public Deploy To")]
        [Then(@"I Check Public Deploy To")]
        public void Check_Public_Deploy_To()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked = true;
        }

        [Given(@"I Check Public Deploy From")]
        [When(@"I Check Public Deploy From")]
        [Then(@"I Check Public Deploy From")]
        public void Check_Public_Deploy_From()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked = true;
        }

        [Given(@"I Check Public View")]
        [When(@"I Check Public View")]
        [Then(@"I Check Public View")]
        public void Check_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = true;
        }

        [Given(@"I Check Public Execute")]
        [When(@"I Check Public Execute")]
        [Then(@"I Check Public Execute")]
        public void Check_Public_Execute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked = true;
        }

        [Given(@"I Check Public Contribute")]
        [When(@"I Check Public Contribute")]
        [Then(@"I Check Public Contribute")]
        public void Check_Public_Contribute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked = true;
        }

        [Given(@"I Uncheck Public Administrator")]
        [When(@"I Uncheck Public Administrator")]
        [Then(@"I Uncheck Public Administrator")]
        public void Uncheck_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Deploy To")]
        [When(@"I Uncheck Public Deploy To")]
        [Then(@"I Uncheck Public Deploy To")]
        public void Uncheck_Public_Deploy_To()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Deploy From")]
        [When(@"I Uncheck Public Deploy From")]
        [Then(@"I Uncheck Public Deploy From")]
        public void Uncheck_Public_Deploy_From()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public View")]
        [When(@"I Uncheck Public View")]
        [Then(@"I Uncheck Public View")]
        public void Uncheck_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Execute")]
        [When(@"I Uncheck Public Execute")]
        [Then(@"I Uncheck Public Execute")]
        public void Uncheck_Public_Execute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked = false;
        }

        [Given(@"I Uncheck Public Contribute")]
        [When(@"I Uncheck Public Contribute")]
        [Then(@"I Uncheck Public Contribute")]
        public void Uncheck_Public_Contribute()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked = false;
        }

        [When(@"I UnCheck Public Administrator")]
        public void UnCheck_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = false;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeplotFrom checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox is unchecked after unChecking Administrator.");
        }

        [Given(@"I Check Resource Contribute")]
        [When(@"I Check Resource Contribute")]
        [Then(@"I Check Resource Contribute")]
        public void Check_Resource_Contribute()
        {
            FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Resource View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Resource Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(FindAddRemoveRowButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Enabled, "Resource Delete button is disabled");
        }

        [Given(@"I UnCheck Public View")]
        [When(@"I UnCheck Public View")]
        [Then(@"I UnCheck Public View")]
        public void UnCheck_Public_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked = false;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is checked after Checking Contribute.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is checked after UnChecking Execute/View.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Contribute.");
        }

        [Given(@"I setup Public Permissions for ""(.*)"" for localhost")]
        public void SetupPublicPermissionsForForLocalhost(string resource)
        {
            Click_Settings_RibbonButton();
            var deleteFirstResourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.RemovePermissionButton;
            if (deleteFirstResourceButton.Enabled)
            {
                var isViewChecked = FindViewPermissionsCheckbox(
                    MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                var isExecuteChecked = FindExecutePermissionsCheckbox(
                    MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                if (isViewChecked && isExecuteChecked)
                {
                    Click_Close_Settings_Tab_Button();
                    return;
                }
            }
            Set_FirstResource_ResourcePermissions(resource, "Public", true, true);
            Click_Close_Settings_Tab_Button();
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = GroupName;
            Assert.AreEqual(FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
        }

        public void Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox()
        {
            FindExecutePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindExecutePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings security tab resource permissions row 1 execute checkbox is not checked.");
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox()
        {
            FindViewPermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindViewPermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox()
        {
            FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked = true;
            Assert.IsTrue(FindContributePermissionsCheckbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Resource_Permissions_Row1_Delete_Button()
        {
            Mouse.Click(FindAddRemoveRowButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
        }

        #endregion
        #region Deploy\Deploy.uitest

        [Given(@"I validate the Resource tree is loaded")]
        [When(@"I validate the Resource tree is loaded")]
        [Then(@"I validate the Resource tree is loaded")]
        public void WhenIValidateTheResourceTreeIsLoaded()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.Exists);
        }

        [Given(@"I change Server Authentication type")]
        [When(@"I change Server Authentication type")]
        [Then(@"I change Server Authentication type")]
        public void ChangeServerAuthenticationType()
        {
            var publicRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton;
            var windowsRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton;
            if (publicRadioButton.Selected)
            {
                windowsRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
                Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration);
                Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(windowsRadioButton.Selected);
            }
            else
            {
                publicRadioButton.Selected = true;
                Click_Server_Source_Wizard_Test_Connection_Button();
                Click_Save_Ribbon_Button_With_No_Save_Dialog();
                Playback.Wait(1000);
                Click_Close_Server_Source_Wizard_Tab_Button();
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
                Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration);
                Click_Deploy_Tab_Source_Server_Edit_Button();
                Assert.IsTrue(publicRadioButton.Selected);
            }
        }

        [Given(@"Destination Remote Server Is Connected")]
        [Then(@"Destination Remote Server Is Connected")]
        public void ThenDestinationRemoteServerIsConnected()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedRemoteConnectionText.Exists, "Remote Server is Disconnected");
        }

        [Then(@"The deploy validation message is ""(.*)""")]
        [When(@"The deploy validation message is ""(.*)""")]
        [Given(@"The deploy validation message is ""(.*)""")]
        public void ThenTheDeployValidationMessageIs(string message)
        {
            Assert.AreEqual(message, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButtonMessageText.DisplayText);
        }

        [Given(@"Deploy Window Is Still Open")]
        [When(@"Deploy Window Is Still Open")]
        [Then(@"Deploy Window Is Still Open")]
        public void ThenDeployWindowIsStillOpen()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.Exists);
        }

        [Then(@"Destination Deploy Information Clears")]
        public void ThenDestinationDeployInformationClears()
        {
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton.Enabled);
        }

        public void Click_SelectAllDependencies_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.ShowDependenciesButton);
        }

        [Then(@"Deploy Button Is Enabled")]
        public void ThenDeployButtonIsEnabled()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled, "Deploy button is not enabled");
        }

        [Then(@"Filtered Resourse Is Checked For Deploy")]
        public void ThenFilteredResourseIsCheckedForDeploy()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked);
        }

        [Given(@"I Click Edit Deploy Destination Server Button")]
        [When(@"I Click Edit Deploy Destination Server Button")]
        [Then(@"I Click Edit Deploy Destination Server Button")]
        public void Click_Edit_Deploy_Destination_Server_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditDestinationButton);
        }

        [When(@"I Select localhost From Deploy Tab Destination Server Combobox")]
        public void Select_localhost_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "localhost (Connected) option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ConnectedLocalhostText.Exists, "Selected destination server in deploy is not localhost (Connected).");
        }

        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        [Then(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        [Given(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox()
        {
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected source server in deploy is not Remote Connection Integration.");
        }

        [When(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        [Then(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        [Given(@"I Select LocalhostConnected From Deploy Tab Source Server Combobox")]
        public void WhenISelectLocalhostConnectedFromDeployWizardTabSourceServerCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton);
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhost);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected source server in deploy is not Remote Connection Integration.");
        }


        [When(@"I Select localhost From Deploy Tab Source Server Combobox")]
        public void Select_localhost_From_Deploy_Tab_Source_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "localhost (Connected) option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.LocalhostText.Exists, "Selected source server in deploy is not localhost (Connected).");
        }

        [When(@"I Select RemoteConnectionIntegration \(Connected\) From Deploy Tab Source Server Combobox")]
        public void Select_ConnectedRemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Exists, "Remote Connection Integration option does not exist in Source server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Text, new Point(226, 13));
            Assert.AreEqual("Remote Connection Integration", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Selected source server in deploy is not Remote Connection Integration.");
        }

        [Given(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        [Then(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected destination server in deploy is not Remote Connection Integration.");
        }

        [When(@"I Select LocalhostConnected From Deploy Tab Destination Server Combobox")]
        public void Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(226, 13));
        }

        [Given(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        [When(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        [Then(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        public void Enter_DeployViewOnly_Into_Deploy_Source_Filter(string SearchTextboxText)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = SearchTextboxText;
            if (SearchTextboxText.ToLower() == "localhost".ToLower()) return;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.Exists, "First deploy tab source explorer item does not exist after filter is applied.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Exists, "Deploy source server explorer tree first item checkbox does not exist.");
        }

        public void Filter_Deploy_Source_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = FilterText;
        }

        [Given(@"I Click Deploy Tab Destination Server Combobox")]
        [When(@"I Click Deploy Tab Destination Server Combobox")]
        [Then(@"I Click Deploy Tab Destination Server Combobox")]
        public void Click_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
        }

        [When(@"I Click Deploy Tab Destination Server Connect Button")]
        [Given(@"I Click Deploy Tab Destination Server Connect Button")]
        [Then(@"I Click Deploy Tab Destination Server Connect Button")]
        public void Click_Deploy_Tab_Destination_Server_Connect_Button()
        {
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.ConnectDestinationButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.ConnectDestinationButton, new Point(13, 12));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Spinner);
        }

        [Given(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        [When(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        [Then(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        public void Click_Deploy_Tab_Destination_Server_New_Remote_Server_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsNewRemoteServer, new Point(223, 10));
        }

        [When(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
        [Then(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
        [Given(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
        public void Click_Deploy_Tab_Destination_Server_Remote_Connection_Intergration_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration, new Point(223, 10));
        }

        [Given(@"I Click Deploy Tab Source Server Combobox")]
        [When(@"I Click Deploy Tab Source Server Combobox")]
        [Then(@"I Click Deploy Tab Source Server Combobox")]
        public void Click_Deploy_Tab_Source_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Source server combobox.");
        }

        [Given(@"I Click Deploy Tab Source Server Connect Button")]
        [When(@"I Click Deploy Tab Source Server Connect Button")]
        [Then(@"I Click Deploy Tab Source Server Connect Button")]
        public void Click_Deploy_Tab_Source_Server_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.ConnectSourceButton, new Point(13, 8));
        }

        [Given(@"I Click Deploy Tab Source Server Edit Button")]
        [When(@"I Click Deploy Tab Source Server Edit Button")]
        [Then(@"I Click Deploy Tab Source Server Edit Button")]
        public void Click_Deploy_Tab_Source_Server_Edit_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.EditSourceButton, new Point(13, 8));
        }

        [Given(@"I Click Deploy Tab Source Refresh Button")]
        [When(@"I Click Deploy Tab Source Refresh Button")]
        [Then(@"I Click Deploy Tab Source Refresh Button")]
        public void Click_Deploy_Tab_Source_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.RefreshButton);
        }

        [Given(@"I Click Deploy Tab WarewolfStore Item")]
        [When(@"I Click Deploy Tab WarewolfStore Item")]
        [Then(@"I Click Deploy Tab WarewolfStore Item")]
        public void Click_Deploy_Tab_WarewolfStore_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsWarewolfStore, new Point(214, 9));
        }

        [Given(@"I Deploy ""(.*)"" From Deploy View")]
        [When(@"I Deploy ""(.*)"" From Deploy View")]
        [Then(@"I Deploy ""(.*)"" From Deploy View")]
        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
            Select_Deploy_First_Source_Item();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled,
                "Deploy button is not enabled after valid server and resource are selected.");
            Click_Deploy_Tab_Deploy_Button();
        }

        [When(@"Resources is visible on the tree")]
        [Then(@"Resources is visible on the tree")]
        public void WhenResourcesIsVisibleOnTheTree()
        {
            var controlExistsNow = ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem);
            Assert.IsTrue(controlExistsNow);
        }


        [Then(@"Deploy Button is enabled  ""(.*)""")]
        [When(@"Deploy Button is enabled  ""(.*)""")]
        [Given(@"Deploy Button is enabled  ""(.*)""")]
        public void ThenDeployButtonIsEnabled(string enabled)
        {
            var isEnabled = bool.Parse(enabled);
            if (isEnabled)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.WaitForControlEnabled();
            }
            Assert.AreEqual(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled, isEnabled);
        }

        [Given(@"I Select localhost from the source tab")]
        [When(@"I Select localhost from the source tab")]
        [Then(@"I Select localhost from the source tab")]
        public void WhenISelectLocalhostFromTheSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.EnvironmentNameCheckCheckBox);
        }

        [Then(@"I validate I can not Deploy ""(.*)""")]
        public void ValidateICanNotDeploy(string resource)
        {
            Filter_Deploy_Source_Explorer(resource);
            Playback.Wait(2000);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.RemoteServer.FirstRemoteResource.FirstRemoteResourceCheckBox.Enabled, "The Deploy selection checkbox is Enabled");
        }

        [Then(@"I validate I can Deploy ""(.*)""")]
        public void ValidateICanDeploy(string resource)
        {
            Filter_Deploy_Source_Explorer(resource);
            Playback.Wait(1000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.RemoteServer.FirstRemoteResource.FirstRemoteResourceCheckBox.Enabled, "The Deploy selection checkbox is not Enabled");
        }

        [When(@"I Select Deploy First Source Item")]
        [Then(@"I Select Deploy First Source Item")]
        [Given(@"I Select Deploy First Source Item")]
        public void Select_Deploy_First_Source_Item()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = true;
        }

        [Given(@"I Click Deploy Tab Deploy Button")]
        [When(@"I Click Deploy Tab Deploy Button")]
        [Then(@"I Click Deploy Tab Deploy Button")]
        public void Click_Deploy_Tab_Deploy_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton);
        }

        #endregion
        #region Dialogs\Dialogs.uitest

        [Given(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [When(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [Then(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        public void ExplorerItemsAppearOnTheSaveDialogExplorerTree()
        {
            Assert.IsTrue(ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem));
            Assert.IsTrue(ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem));
        }

        public void Enter_ConfigFile_In_SelectFilesWindow()
        {
            Mouse.Click(SelectFilesWindow.DrivesDataTree.CTreeItem.swapfile);
            Mouse.Click(SelectFilesWindow.SelectButton);
        }

        public void Select_Attachments_From_SelectFilesWindow()
        {
            Mouse.DoubleClick(SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder);
            SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder.attachment1.CheckBox.Checked = true;
            SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder.attachment2.CheckBox.Checked = true;
            Assert.IsNotNull(SelectFilesWindow.FileNameTextBox.Text, "Files Name is empty even after selecting Files..");
            Mouse.Click(SelectFilesWindow.SelectButton);
        }

        public void Select_DLLAssemblyFile_From_ChooseDLLWindow(string fileName)
        {
            ChooseDLLWindow.FilterTextBox.Text = fileName.Replace(@"C:\", "");
            Mouse.Click(ChooseDLLWindow.DLLDataTree.CDrive, new Point(11, 14));
            Mouse.Click(ChooseDLLWindow.DLLDataTree.CDrive.FirstItem, new Point(69, 34));
            Assert.AreEqual(fileName, ChooseDLLWindow.FilesTextBox.Text);
            Mouse.Click(ChooseDLLWindow.SelectButton);
        }

        public void Select_GACAssemblyFile_From_ChooseDLLWindow(string filter)
        {
            ChooseDLLWindow.FilterTextBox.Text = filter;
            ChooseDLLWindow.DLLDataTree.GAC.DataTreeItem.DrawHighlight();
            Mouse.Click(ChooseDLLWindow.DLLDataTree.GAC.DataTreeItem, new Point(122, 6));
            Assert.IsFalse(string.IsNullOrEmpty(ChooseDLLWindow.FilesTextBox.Text), "Files Textbox is empty.");
            ChooseDLLWindow.SelectButton.DrawHighlight();
            Mouse.Click(ChooseDLLWindow.SelectButton);
        }

        [Given(@"I Click Close Critical Error Dialog")]
        [When(@"I Click Close Critical Error Dialog")]
        [Then(@"I Click Close Critical Error Dialog")]
        public void Click_Close_Critical_Error_Dialog()
        {
            Mouse.Click(CriticalErrorWindow.CloseButton, new Point(9, 11));
        }

        [When(@"I Click Web Browser Error Messagebox OK Button")]
        public void Click_Web_Browser_Error_Messagebox_OK_Button()
        {
            Mouse.Click(WebBrowserErrorWindow.Pane.OKButton, new Point(30, 8));
        }

        [Given(@"I Click Close Error Dialog")]
        [When(@"I Click Close Error Dialog")]
        [Then(@"I Click Close Error Dialog")]
        public void Click_Close_Error_Dialog()
        {
            Mouse.Click(ErrorWindow.CloseButton, new Point(8, 9));
        }

        [When(@"I Click Select Windows Group OK Button")]
        public void Click_Select_Windows_Group_OK_Button()
        {
            Mouse.Click(SelectWindowsGroupDialog.OKPanel.OK, new Point(37, 9));
        }

        public void Resize_Decision_LargeTool()
        {
            Mouse.StartDragging(DecisionOrSwitchDialog, new Point(396, 387));
            Mouse.StopDragging(DecisionOrSwitchDialog, new Point(0, 450));
        }

        [Given(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        [When(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        [Then(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        public void WhenIHitEscapeKeyOnTheKeyboardOnActivityDefaultWindow()
        {
            Keyboard.SendKeys(DecisionOrSwitchDialog, "{Escape}", ModifierKeys.None);
        }

        [Then(@"The Case Dialog Must Be Open")]
        public void ThenTheCaseDialogMustBeOpen()
        {
            Mouse.DoubleClick(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch);
            Assert.IsTrue(DecisionOrSwitchDialog.Exists, "Switch case dialog does not exist after dragging onto switch case arm.");
            Mouse.Click(DecisionOrSwitchDialog.DoneButton);
        }

        [Given(@"I Click Decision Dialog Cancel Button")]
        [When(@"I Click Decision Dialog Cancel Button")]
        [Then(@"I Click Decision Dialog Cancel Button")]
        public void Click_Decision_Dialog_Cancel_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.CancelButton, new Point(10, 14));
        }

        [Given(@"I Click Decision Dialog Done Button")]
        [When(@"I Click Decision Dialog Done Button")]
        [Then(@"I Click Decision Dialog Done Button")]
        public void Click_Decision_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(10, 14));
            Assert.IsFalse(ControlExistsNow(DecisionOrSwitchDialog), "Decision large view dialog still exists after the done button is clicked.");
        }

        [Given(@"I Click Switch Dialog Done Button")]
        [When(@"I Click Switch Dialog Done Button")]
        [Then(@"I Click Switch Dialog Done Button")]
        public void Click_Switch_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(24, 7));
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch.Exists, "Switch on the design surface does not exist");
        }

        [Given(@"Filtered Item Exists")]
        [When(@"Filtered Item Exists")]
        [Then(@"Filtered Item Exists")]
        public void FilteredItemExists()
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.Exists);
        }

        [Given(@"""(.*)"" is child of ""(.*)""")]
        [When(@"""(.*)"" is child of ""(.*)""")]
        [Then(@"""(.*)"" is child of ""(.*)""")]
        public void FolderIsChildOfParentFolder(string child, string parent)
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text.Contains(parent));
            Assert.AreEqual(child, SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text);
        }

        [Given(@"""(.*)"" is child of localhost")]
        [When(@"""(.*)"" is child of localhost")]
        [Then(@"""(.*)"" is child of localhost")]
        public void ResourceIsChildOfLocalhost(string child)
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Exists);
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text.Contains(child));
        }

        [Given(@"I Move resource to localhost")]
        [When(@"I Move resource to localhost")]
        [Then(@"I Move resource to localhost")]
        public void MoveResourceToLocalhost()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, new Point(90, 11));
        }

        [Given(@"I Move FolderToMove into FolderToRename")]
        [When(@"I Move FolderToMove into FolderToRename")]
        [Then(@"I Move FolderToMove into FolderToRename")]
        public void MoveFolderToMoveIntoFolderToRename()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.ThirdItem.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.ThirdItem, new Point(90, 11));
        }

        [Given(@"I Move FolderToRename into localhost")]
        [When(@"I Move FolderToRename into localhost")]
        [Then(@"I Move FolderToRename into localhost")]
        public void MoveFolderToRenameIntoLocalhost()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, new Point(90, 11));
        }

        [Given(@"Context Menu Has Two Items")]
        [When(@"Context Menu Has Two Items")]
        [Then(@"Context Menu Has Two Items")]
        public void ThenContextMenuHasTwoItems()
        {
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem.Exists);
            Point point;
            Assert.IsFalse(SaveDialogWindow.SaveDialogContextMenu.SourcesMenuItem.TryGetClickablePoint(out point));
            Assert.IsFalse(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem.TryGetClickablePoint(out point));
        }

        [Given(@"I Enter New Folder Name as ""(.*)""")]
        [When(@"I Enter New Folder Name as ""(.*)""")]
        [Then(@"I Enter New Folder Name as ""(.*)""")]
        public void EnterNewFolderNameAs(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Enter New Sub Folder Name as ""(.*)""")]
        [When(@"I Enter New Sub Folder Name as ""(.*)""")]
        [Then(@"I Enter New Sub Folder Name as ""(.*)""")]
        public void ThenIEnterNewSubFolderNameAs(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Name New Sub Folder as ""(.*)""")]
        [When(@"I Name New Sub Folder as ""(.*)""")]
        [Then(@"I Name New Sub Folder as ""(.*)""")]
        public void I_Name_New_Sub_Folder_As(string name)
        {
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Back}", ModifierKeys.None);
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Dont Name The Created Folder")]
        [When(@"I Dont Name The Created Folder")]
        [Then(@"I Dont Name The Created Folder")]
        public void ThenIDontNameTheCreatedFolder()
        {
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;

            Keyboard.SendKeys(newFolderEdit, "{Right}{Enter}", ModifierKeys.None);
            // Click 'Save' button
            Mouse.Click(saveButton, new Point(22, 16));
        }

        [Given(@"I Name New Folder as ""(.*)""")]
        [When(@"I Name New Folder as ""(.*)""")]
        [Then(@"I Name New Folder as ""(.*)""")]
        public void Name_New_Folder_From_Save_Dialog(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Hit Escape Key On The Keyboard")]
        [When(@"I Hit Escape Key On The Keyboard")]
        [Then(@"I Hit Escape Key On The Keyboard")]
        public void ThenIHitEscapeKeyOnTheKeyboard()
        {
            Keyboard.SendKeys(this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Escape}", ModifierKeys.None);
        }

        [Given(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [When(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [Then(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        public void Rename_Folder_From_Save_Dialog(string filterText)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = filterText;
        }

        [Given(@"I Click Save Ribbon Button to Open Save Dialog")]
        [When(@"I Click Save Ribbon Button to Open Save Dialog")]
        [Then(@"I Click Save Ribbon Button to Open Save Dialog")]
        public void Click_Save_Ribbon_Button_to_Open_Save_Dialog()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton);
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist after clicking save ribbon button.");
        }

        [Given(@"I Select New Folder From SaveDialog Context Menu")]
        [When(@"I Select New Folder From SaveDialog Context Menu")]
        [Then(@"I Select New Folder From SaveDialog Context Menu")]
        public void Select_NewFolder_From_SaveDialogContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem);
        }

        [Given(@"I RightClick Save Dialog Localhost First Item")]
        [When(@"I RightClick Save Dialog Localhost First Item")]
        [Then(@"I RightClick Save Dialog Localhost First Item")]
        public void RightClick_Save_Dialog_Localhost_First_Item()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I Rename Item using Shortcut")]
        [When(@"I Rename Item using Shortcut")]
        [Then(@"I Rename Item using Shortcut")]
        public void RenameItemUsingShortcut()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(77, 9));
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, "{F2}");
        }

        [Given(@"I Create New Folder Item using Shortcut")]
        [When(@"I Create New Folder Item using Shortcut")]
        [Then(@"I Create New Folder Item using Shortcut")]
        public void ThenICreateNewFolderItemUsingShortcut()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(77, 9));
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, "F", (ModifierKeys.Control | ModifierKeys.Shift));
        }

        [Given(@"I RightClick Save Dialog Localhost")]
        [When(@"I RightClick Save Dialog Localhost")]
        [Then(@"I RightClick Save Dialog Localhost")]
        public void RightClick_Save_Dialog_Localhost()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem.Exists);
        }

        [Given(@"I Select Rename From SaveDialog Context Menu")]
        [When(@"I Select Rename From SaveDialog Context Menu")]
        [Then(@"I Select Rename From SaveDialog Context Menu")]
        private void Select_Rename_From_SaveDialog_ContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem);
        }

        [Given(@"I Remove WorkflowName From Save Dialog")]
        [When(@"I Remove WorkflowName From Save Dialog")]
        [Then(@"I Remove WorkflowName From Save Dialog")]
        public void Remove_WorkflowName_From_Save_Dialog()
        {
            SaveDialogWindow.ServiceNameTextBox.Text = "";
            Assert.AreEqual("Cannot be null", SaveDialogWindow.ErrorLabel.DisplayText, "Name cannot be null validation message does not appear");
            Assert.AreEqual(false, SaveDialogWindow.SaveButton.Enabled, "Save button on the Save dialog is enabled");
        }

        [Given(@"I Select Delete From SaveDialog Context Menu")]
        [When(@"I Select Delete From SaveDialog Context Menu")]
        [Then(@"I Select Delete From SaveDialog Context Menu")]
        public void Select_Delete_From_SaveDialog_ContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem);
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeleteConfirmation.Exists);
        }

        [Given(@"I Click SaveDialog CancelButton")]
        [When(@"I Click SaveDialog CancelButton")]
        [Then(@"I Click SaveDialog CancelButton")]
        public void Click_SaveDialog_CancelButton()
        {
            Mouse.Click(SaveDialogWindow.CancelButton, new Point(6, 7));
        }

        [Given(@"I Click Duplicate From Duplicate Dialog")]
        [When(@"I Click Duplicate From Duplicate Dialog")]
        [Then(@"I Click Duplicate From Duplicate Dialog")]
        public void Click_Duplicate_From_Duplicate_Dialog()
        {
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Exists, "Duplicate button does not exist");
            Mouse.Click(SaveDialogWindow.DuplicateButton, new Point(26, 10));
        }

        [Given(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        [When(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        [Then(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        public void Enter_Service_Name_Into_Save_Dialog_and_Append_Unique_Guid(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName + Guid.NewGuid().ToString().Substring(0, 8);
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Click SaveDialog Save Button")]
        [When(@"I Click SaveDialog Save Button")]
        [Then(@"I Click SaveDialog Save Button")]
        public void Click_SaveDialog_Save_Button()
        {
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save button on the Save Dialog is not Enabled");
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
        }

        [When(@"I Wait For Save Dialog Explorer Spinner")]
        public void WaitForSaveDialogExplorerSpinner()
        {
            WaitForSpinner(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
        }

        [Given(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        public void I_Enter_Invalid_Service_Name_With_Whitespace_Into_SaveDialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_With_Whitespace_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        public void I_Enter_Invalid_Service_Name_Into_SaveDialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains invalid characters", errorLabel.DisplayText, "Error is not the same as expected");
        }

        [Given(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        public void Enter_Valid_Service_Name_Into_Save_Dialog(string ServiceName)
        {
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist on the Surface.");
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
        }

        [Given(@"same name error message is shown")]
        public void GivenSameNameErrorMessageIsShown()
        {
            Assert.AreEqual("An item with this name already exists in this folder.", SaveDialogWindow.ErrorLabel.DisplayText);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName, bool duplicate = false, bool invalid = false, bool nameHasWhiteSpace = false, SaveOrDuplicate saveOrDuplicate = SaveOrDuplicate.Save)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Double Click Resource On The Save Dialog")]
        [When(@"I Double Click Resource On The Save Dialog")]
        [Then(@"I Double Click Resource On The Save Dialog")]
        public void DoubleClickResourceOnTheSaveDialog()
        {
            Mouse.DoubleClick(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I Select LocalHost on the Save Dialog")]
        [When(@"I Select LocalHost on the Save Dialog")]
        [Then(@"I Select LocalHost on the Save Dialog")]
        public void WhenISelectLocalHostOnTheSaveDialog()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost);
        }

        [Given(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [When(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [Then(@"I Filter Save Dialog Explorer with ""(.*)""")]
        public void Filter_Save_Dialog_Explorer(string FilterText)
        {
            var searchTextBox = SaveDialogWindow.ExplorerView.SearchTextBox;
            searchTextBox.Text = FilterText;
        }

        [When(@"I Click UpdateDuplicateRelationships")]
        public void Click_UpdateDuplicateRelationships()
        {
            SaveDialogWindow.UpdateDuplicatedRelat.Checked = true;
        }

        [Given(@"I Click MessageBox Cancel")]
        [When(@"I Click MessageBox Cancel")]
        [Then(@"I Click MessageBox Cancel")]
        public void ThenIClickMessageBoxCancel()
        {
            Mouse.Click(MessageBoxWindow.CancelButton);
        }

        [Then(@"Deploy Version Conflict Window Shows")]
        public void ThenDeployVersionConflictWindowShows()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeployVersionConflicText.Exists);
        }

        [Then(@"Deploy is Successfully")]
        [When(@"Deploy is Successfully")]
        [Given(@"Deploy is Successfully")]
        public void ThenDeployIsSuccessfully()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.ResourcesDeployedSucText.Exists);
        }

        [Then(@"Message box window appears")]
        [When(@"Message box window appears")]
        [Given(@"Message box window appears")]
        public void ThenMessageBoxWindowAppears()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
        }

        [Given(@"I Click MessageBox No")]
        [When(@"I Click MessageBox No")]
        [Then(@"I Click MessageBox No")]
        public void Click_MessageBox_No()
        {
            MessageBoxWindow.NoButton.DrawHighlight();
            Mouse.Click(MessageBoxWindow.NoButton);
        }

        [Given(@"I Click MessageBox OK")]
        [When(@"I Click MessageBox OK")]
        [Then(@"I Click MessageBox OK")]
        [Given(@"I Click MessageBox OK")]
        public void Click_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Duplicate_Test_Name_MessageBox_Ok()
        {
            Assert.IsTrue(MessageBoxWindow.DuplicateTestNameText.Exists, "Duplicate test name message box does not appear on the surface.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Save Before Continuing MessageBox OK")]
        public void Click_Save_Before_Continuing_MessageBox_OK()
        {
            Assert.IsTrue(MessageBoxWindow.SaveBeforeAddingNewTestText.Exists, "Messagebox does not warn about unsaved tests after clicking create new test.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Click_DropNotAllowed_MessageBox_OK()
        {
            Assert.IsTrue(MessageBoxWindow.DropnotallowedText.Exists, "The Shown dialog is not Drop Not 'Allowed MessageBox'");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Click_DeleteAnyway_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy version conflicts MessageBox OK")]
        [When(@"I Click Deploy version conflicts MessageBox OK")]
        public void ClickDeployVersionConflictsMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.DeployVersionConflicText.Exists, "Deploy Version Conflicts MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy conflicts MessageBox OK")]
        [When(@"I Click Deploy conflicts MessageBox OK")]
        public void ClickDeployConflictsMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.DeployConflictsText.Exists, "Deploy Conflicts MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy Successful MessageBox OK")]
        [When(@"I Click Deploy Successful MessageBox OK")]
        public void ClickDeploySuccessfulMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.ResourcesDeployedSucText.Exists, "Deploy Successful MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Given(@"I Click MessageBox DeleteAnyway")]
        [When(@"I Click MessageBox DeleteAnyway")]
        [Then(@"I Click MessageBox DeleteAnyway")]
        [Given(@"I Click MessageBox DeleteAnyway")]
        public void Click_MessageBox_DeleteAnyway()
        {
            Mouse.Click(MessageBoxWindow.DeleteAnyway, new Point(35, 11));
        }

        [Given(@"I Click MessageBox Yes")]
        [When(@"I Click MessageBox Yes")]
        [Then(@"I Click MessageBox Yes")]
        public void Click_MessageBox_Yes()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(32, 5));
        }

        public void Click_Assign_Tool_url()
        {
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist");
            Mouse.Click(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
            Assert.IsTrue(MessageBoxWindow.OKButton.Exists, "Did you know popup does not exist after clicking workflow hyperlink.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(38, 12));
        }

        public void Click_Assign_Tool_url_On_Unpinned_Tab()
        {
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist on unpinned tab.");
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
            Assert.IsTrue(MessageBoxWindow.OKButton.Exists, "Did you know popup does not exist after clicking workflow hyperlink on unpinned tab.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(38, 12));
        }

        public void TryCloseDeployWizardTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab))
                {
                    Click_Close_Deploy_Tab_Button();
                }
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Deploy tab.\n" + e.Message);
            }
        }

        public void TryCloseNewWebSourceWizardTab()
        {
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.CloseButton))
            {
                Click_Close_Web_Source_Wizard_Tab_Button();
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
        }

        private void TryCloseServerSourceWizardTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.CloseButton))
                {
                    Click_Close_Server_Source_Wizard_Tab_Button();
                }
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Server Source tab.\n" + e.Message);
            }
        }

        [When(@"I Try Close Settings Tab")]
        public void TryCloseSettingsWizardTab()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab))
                {
                    Click_Close_Settings_Tab_Button();
                }
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception trying to close settings tab.\n" + e.Message);
            }
            finally
            {
                Console.WriteLine("No hanging settings tab to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        [When(@"I Click Yes On The Confirm Delete")]
        public void Click_Yes_On_The_Confirm_Delete()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(39, 17));
        }

        [Given(@"I Try Click MessageBox No")]
        [When(@"I Try Click MessageBox No")]
        [Then(@"I Try Click MessageBox No")]
        public void TryClickMessageBoxNo()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging message box before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging message box to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }

        }

        [When(@"I Try Click Message Box OK")]
        [Then(@"I Try Click Message Box OK")]
        [Given(@"I Try Click Message Box OK")]
        public void TryClickMessageBoxOK()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MessageBoxWindow.OKButton))
                {
                    Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging message box before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging message box to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void Filter_ServicePicker_Explorer(string FilterText)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = FilterText;
            WaitForControlVisible(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void Click_Service_Picker_Dialog_OK()
        {
            Mouse.Click(ServicePickerDialog.OK, new Point(52, 10));
        }

        public void Click_ServicePickerDialog_CancelButton()
        {
            Mouse.Click(ServicePickerDialog.Cancel, new Point(57, 6));
        }

        public void Click_Service_Picker_Dialog_Refresh_Button()
        {
            Mouse.Click(ServicePickerDialog.Explorer.Refresh, new Point(10, 11));
            WaitForSpinner(ServicePickerDialog.Explorer.ExplorerTree.Localhost.Checkbox.Spinner);
        }

        public void Select_First_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            Playback.Wait(500);
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        [Given(@"I Select ""(.*)"" From Service Picker")]
        [When(@"I Select ""(.*)"" From Service Picker")]
        [Then(@"I Select ""(.*)"" From Service Picker")]
        public void Select_SubItem_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1.TreeItem11);
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        [Given(@"I Double Click Resource On The Service Picker")]
        [When(@"I Double Click Resource On The Service Picker")]
        [Then(@"I Double Click Resource On The Service Picker")]
        public void DoubleClick_FirstItem_From_ServicePicker_Tree()
        {
            var firstItem = ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1;
            Mouse.DoubleClick(firstItem);
        }

        public void Select_FirstItem_From_ServicePicker_Tree()
        {
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
        }

        [When(@"I Click Service Picker Dialog First Service In Explorer")]
        public void Click_Service_Picker_Dialog_First_Service_In_Explorer()
        {
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1, new Point(91, 9));
        }

        #endregion
        #region Settings\Settings.uitest

        [When(@"I Click Server Log File Button")]
        public void Click_Server_Log_File_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.ServerLogs.ServerLogFile.ItemHyperlink, new Point(83, 6));
        }

        [When(@"I Click Settings Security Resource Permissions Add Resource Button")]
        public void Click_Settings_Security_Resource_Permissions_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1), new Point(6, 15));
        }

        [When(@"I Click Sharepoint Server Source TestConnection")]
        public void Click_Sharepoint_Server_Source_TestConnection()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton, new Point(58, 16));
        }

        [When(@"I Click Studio Log File")]
        public void Click_Studio_Log_File()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.StudioLogs.StudioLogFile.ItemHyperlink, new Point(79, 10));
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

        public void Create_New_Workflow_In_Explorer_First_Item_With_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, "W", (ModifierKeys.Control));
        }

        public void Create_New_Workflow_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, "W", (ModifierKeys.Control));
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
                Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                Click_Deploy_Tab_Source_Server_Edit_Button();
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
                Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox();
                Click_Deploy_Tab_Source_Server_Edit_Button();
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
