using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.CodeDom.Compiler;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.CodeDom;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    public partial class UIMap
    {
        const int _lenientSearchTimeout = 30000;
        const int _lenientMaximumRetryCount = 3;
        const int _strictSearchTimeout = 3000;
        const int _strictMaximumRetryCount = 1;

        public void SetPlaybackSettings()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
#if DEBUG
            Playback.PlaybackSettings.ThinkTimeMultiplier = 1;
#else  
            Playback.PlaybackSettings.ThinkTimeMultiplier = 2;
#endif
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.SkipSetPropertyVerification = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackError -= OnError;
            Playback.PlaybackError += OnError;
        }

        [Given("The Warewolf Studio is running")]
        public void CloseHangingDialogs()
        {
            Assert.IsTrue(MainStudioWindow.Exists, "Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
#if !DEBUG
            TryClickMessageBoxOK();
            TryCloseHangingDebugInputDialog();
            TryCloseHangingSaveDialog();
            TryCloseHangingServicePickerDialog();
            TryCloseHangingWindowsGroupDialog();
            TryPin_Unpinned_Pane_To_Default_Position();
            TryCloseHangingCriticalErrorDialog();
            TryCloseHangingErrorDialog();
#endif
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        [When(@"I Try Click Message Box OK")]
        public void TryClickMessageBoxOK()
        {
            try
            {
                if (ControlExistsNow(MessageBoxWindow.OKButton))
                {
                    Click_MessageBox_OK();
                }
                else
                {
                    Console.WriteLine("No hanging message box to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryCloseHangingDebugInputDialog()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DebugInputDialog))
                {
                    Click_DebugInput_Cancel_Button();
                }
                else
                {
                    Console.WriteLine("No hanging debug input dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryCloseHangingSaveDialog()
        {
            try
            {
                if (ControlExistsNow(SaveDialogWindow.CancelButton))
                {
                    Click_SaveDialog_CancelButton();
                }
                else
                {
                    Console.WriteLine("No hanging save dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryPin_Unpinned_Pane_To_Default_Position()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.UnpinnedTab))
                {
                    Restore_Unpinned_Tab_Using_Context_Menu();
                }
                else
                {
                    Console.WriteLine("No hanging unpinned pane to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        private void TryCloseHangingServicePickerDialog()
        {
            try
            {
                if (ControlExistsNow(ServicePickerDialog.Cancel))
                {
                    Click_Service_Picker_Dialog_Cancel();
                }
                else
                {
                    Console.WriteLine("No hanging service picker dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryCloseHangingWindowsGroupDialog()
        {
            try
            {
                if (ControlExistsNow(SelectWindowsGroupDialog))
                {
                    Click_Select_Windows_Group_Cancel_Button();
                }
                else
                {
                    Console.WriteLine("No hanging select windows group dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryCloseHangingErrorDialog()
        {
            try
            {
                if (ControlExistsNow(ErrorWindow))
                {
                    Click_Close_Error_Dialog();
                }
                else
                {
                    Console.WriteLine("No hanging error dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void TryCloseHangingCriticalErrorDialog()
        {
            try
            {
                if (ControlExistsNow(CriticalErrorWindow))
                {
                    Click_Close_Critical_Error_Dialog();
                }
                else
                {
                    Console.WriteLine("No hanging critical error dialog to clean up.");
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Caught a null reference exception trying to close a hanging dialog before the test starts.");
            }
        }

        public void Click_Assign_tool_VariableTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox);
        }
        public void Click_Assign_tool_ValueTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox);
        }

        bool OnErrorHandlerDisabled = false;
        public void OnError(object sender, PlaybackErrorEventArgs e)
        {
            if (OnErrorHandlerDisabled) return;
            e.Result = PlaybackErrorOptions.Retry;
            var type = e.Error.GetType().ToString();
            var message = e.Error.Message;
            switch (type)
            {
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException":
                    UITestControlNotFoundExceptionHandler(type, message, e.Error as UITestControlNotFoundException);
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotAvailableException":
                    UITestControlNotAvailableExceptionHandler(type, message, e.Error as UITestControlNotAvailableException);
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.FailedToPerformActionOnBlockedControlException":
                    FailedToPerformActionOnBlockedControlExceptionHandler(type, message, e.Error as FailedToPerformActionOnBlockedControlException);
                    break;
                default:
                    var messageText = type + "\n" + message;
#if DEBUG
                    System.Windows.Forms.MessageBox.Show(messageText);
                    throw e.Error;
#else
                    Console.WriteLine(messageText);
                    Playback.Wait(Playback.PlaybackSettings.SearchTimeout);
                    break;
#endif
            }
        }

        void UITestControlNotFoundExceptionHandler(string type, string message, UITestControlNotFoundException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
                UITestControl parent = (exceptionSource as UITestControl).Container;
                var parentExists = ControlExistsNow(parent);
                while (parent != null && !parentExists)
                {
                    parent = parent.Container;
                    if (parent != null)
                    {
                        parentExists = ControlExistsNow(parent);
                    }
                }
                if (parent != null && parentExists && parent != MainStudioWindow)
                {
                    string parentProperties = string.Empty;
                    parent.SearchProperties.ToList().ForEach(prop => { parentProperties += prop.PropertyName + ": \'" + prop.PropertyValue + "\'\n"; });
                    var messageText = type + "\n" + message + "\n" + "Search actually failed at: " + parent.FriendlyName + "\n" + parentProperties;
                    parent.DrawHighlight();
#if DEBUG
                    System.Windows.Forms.MessageBox.Show(messageText);
                    throw e;
#else
                    Console.WriteLine(messageText);
                    return;
#endif
                }
            }
        }

        void UITestControlNotAvailableExceptionHandler(string type, string message, UITestControlNotAvailableException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
                (exceptionSource as UITestControl).DrawHighlight();
#if DEBUG
                System.Windows.Forms.MessageBox.Show(type + "\n" + message);
                throw e;
#else
                Console.WriteLine(message);
                return;
#endif
            }
        }

        void FailedToPerformActionOnBlockedControlExceptionHandler(string type, string message, FailedToPerformActionOnBlockedControlException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
                (exceptionSource as UITestControl).DrawHighlight();
#if DEBUG
                System.Windows.Forms.MessageBox.Show(type + "\n" + message);
                throw e;
#else
                Console.WriteLine(message);
                return;
#endif
            }
        }

        public bool ControlExistsNow(UITestControl thisControl)
        {
            Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout;
            Playback.PlaybackError -= OnError;
            OnErrorHandlerDisabled = true;
            bool controlExists = false;
            controlExists = thisControl.TryFind();
            OnErrorHandlerDisabled = false;
            Playback.PlaybackError += OnError;
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount;
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout;
            return controlExists;
        }

        public void InitializeABlankWorkflow()
        {
            Click_New_Workflow_Ribbon_Button();
        }

        public void CleanupABlankWorkflow()
        {
            Playback.PlaybackError -= OnError;
            try
            {
                TryClearToolboxFilter();
                TryClearExplorerFilter();
                Click_Close_Workflow_Tab_Button();
                Click_MessageBox_No();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during test cleanup: " + e.Message);
            }
        }

        public void TryClearExplorerFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text != string.Empty)
            {
                Click_Explorer_Filter_Clear_Button();
                Click_Explorer_Refresh_Button();
            }
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text == string.Empty, "Explorer filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text + " is not empty after clicking clear filter button.");
        }

        [When(@"I Try Clear Toolbox Filter")]
        public void TryClearToolboxFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text != string.Empty)
            {
                Click_Clear_Toolbox_Filter_Clear_Button();
                Click_Clear_Toolbox_Filter_Clear_Button();
            }
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text == string.Empty, "Toolbox filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text + " is not empty after clicking clear filter button.");
        }

        [When(@"I First Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        public void WhenIFirstDragToolboxCommentOntoSwitchLeftArmOnDesignSurface()
        {
            First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface();
        }

        [When(@"I Then Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        public void WhenIThenDragToolboxCommentOntoSwitchRightArmOnDesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem commentToolboxItem = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment;
            WpfCustom flowchart = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector3 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3;
            WpfCustom commentOnTheDesignSurface = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion

            var switchRightAutoConnector = new Point(360, 200);
            flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(commentToolboxItem, new Point(16, 25));
            Mouse.StopDragging(flowchart, switchRightAutoConnector);
            Assert.IsTrue(SwitchCaseDialog.Exists);
            Mouse.Click(SwitchCaseDialog.DoneButton, new Point(34, 10));
            Assert.IsTrue(connector3.Exists, "Third connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(commentOnTheDesignSurface.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        [Then(@"The Case Dialog Must Be Open")]
        public void ThenTheCaseDialogMustBeOpen()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch);
            Assert.IsTrue(SwitchCaseDialog.Exists, "Switch case dialog does not exist after dragging onto switch case arm.");
            Mouse.Click(SwitchCaseDialog.DoneButton);
        }
        [When(@"I Click Close Workflow Tab")]
        [Then(@"I Click Close Workflow Tab")]
        public void ThenIClickCloseWorkflowTab()
        {
            Click_Close_Workflow_Tab_Button();
            Click_MessageBox_No();
        }

        public void Click_Settings_Resource_Permissions_Row1_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service picker dialog does not exist.");
        }

        public void Click_Settings_Resource_Permissions_Row1_Windows_Group_Button()
        {
            Mouse.Click(FindAddWindowsGroupButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.IsTrue(SelectWindowsGroupDialog.Exists, "Select windows group dialog does not exist.");
            Assert.IsTrue(SelectWindowsGroupDialog.ItemPanel.ObjectNameTextbox.Exists, "Select windows group object name textbox does not exist.");
        }

        public UITestControl FindAddResourceButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(0);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
        }

        public UITestControl FindAddWindowsGroupButton(UITestControl row)
        {
            var firstOrDefaultCell = row.GetChildren().Where(child => child.ControlType == ControlType.Cell).ElementAtOrDefault(1);
            return firstOrDefaultCell?.GetChildren().FirstOrDefault(child => child.ControlType == ControlType.Button);
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

        public void TryDisconnectFromRemoteServerAndRemoveSourceFromExplorer(string SourceName)
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsTSTCIREMOTEConnected))
                {
                    Click_Explorer_RemoteServer_Connect_Button();
                }
                else
                {
                    Click_Connect_Control_InExplorer();
                    if (ControlExistsNow(MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected))
                    {
                        Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List();
                        Click_Explorer_RemoteServer_Connect_Button();
                    }
                }
                Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List();
                Filter_Explorer(SourceName);
                WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem))
                {
                    RightClick_Explorer_Localhost_First_Item();
                    Select_Delete_FromExplorerContextMenu();
                    Click_MessageBox_Yes();
                }
                TryClearExplorerFilter();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server " + SourceName + ". Test may have crashed before remote server " + SourceName + " was connected.\n" + e.Message);
                TryClearExplorerFilter();
            }
        }

        [When(@"I Try Remove ""(.*)"" From Explorer")]
        public void WhenITryRemoveFromExplorer(string ResourceName)
        {
            Filter_Explorer(ResourceName);
            try
            {
                var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
                if (File.Exists(resourcesFolder + @"\" + ResourceName + ".xml"))
                {
                    WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                    if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem))
                    {
                        RightClick_Explorer_Localhost_First_Item();
                        Select_Delete_FromExplorerContextMenu();
                        Click_MessageBox_Yes();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove resource " + ResourceName + " from the explorer.\n" + e.Message);
            }
            finally
            {
                TryClearExplorerFilter();
            }
        }

        /*
        [When(@"I Try Remove ""(.*)"" from Explorer")]
        public void TryRemoveFromExplorer(string ResourceName)
        {
            Filter_Explorer(ResourceName);
            try
            {
                var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
                if (File.Exists(resourcesFolder + @"\" + ResourceName + ".xml"))
                {
                    WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                    if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem))
                    {
                        RightClick_Explorer_Localhost_First_Item();
                        Select_Delete_FromExplorerContextMenu();
                        Click_MessageBox_Yes();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove resource " + ResourceName + " from the explorer.\n" + e.Message);
            }
            finally
            {
                TryClearExplorerFilter();
            }
        }
        */
        public void Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox()
        {
            #region Variable Declarations
            var row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox executeCheckBox = FindExecutePermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            executeCheckBox.Checked = true;
            Assert.IsTrue(executeCheckBox.Checked, "Settings security tab resource permissions row 1 execute checkbox is not checked.");
            Assert.IsTrue(saveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox()
        {
            #region Variable Declarations
            var row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox viewCheckBox = FindViewPermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            viewCheckBox.Checked = true;
            Assert.IsTrue(viewCheckBox.Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(saveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox()
        {
            #region Variable Declarations
            var row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox contributeCheckBox = FindContributePermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            contributeCheckBox.Checked = true;
            Assert.IsTrue(contributeCheckBox.Checked, "Settings resource permissions row1 view checkbox is not checked.");
            Assert.IsTrue(saveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void TryCloseAllTabs()
        {
            var workflowTabCloseButtonExists = true;
            var settingsTabCloseButtonExists = true;
            var serverSourceWizardTabCloseButtonExists = true;
            while (workflowTabCloseButtonExists || settingsTabCloseButtonExists || serverSourceWizardTabCloseButtonExists)
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton))
                {
                    TryCloseWorkflowTab();
                }
                else
                {
                    workflowTabCloseButtonExists = false;
                }
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton))
                {
                    TryCloseSettingsTab();
                }
                else
                {
                    settingsTabCloseButtonExists = false;
                }
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.TabCloseButton))
                {
                    TryCloseServerSourceWizardTab();
                }
                else
                {
                    serverSourceWizardTabCloseButtonExists = false;
                }
            }
        }

        public void TryCloseWorkflowTabs()
        {
            var workflowTabCloseButtonExists = true;
            while (workflowTabCloseButtonExists)
            {
                try
                {
                    if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton))
                    {
                        TryCloseWorkflowTab();
                    }
                    else
                    {
                        workflowTabCloseButtonExists = false;
                    }
                }
                catch (Exception e)
                {
                    workflowTabCloseButtonExists = false;
                    Console.WriteLine("TryClose method failed to close all Workflow tabs.\n" + e.Message);
                }
            }
            Assert.IsFalse(MainStudioWindow.SideMenuBar.RunAndDebugButton.Enabled, "RunDebug button is enabled");
        }

        public void TryCloseWorkflowTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton))
                {
                    Click_Close_Workflow_Tab_Button();
                }
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Workflow tab.\n" + e.Message);
            }
        }

        public void TryCloseWorkflowTestingTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton))
                {
                    Click_Close_Workflow_Tab_Button();
                }
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Workflow tab.\n" + e.Message);
            }
        }

        [When(@"I Try Close Settings Tab")]
        public void TryCloseSettingsTab()
        {
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
                Console.WriteLine("TryClose method failed to close Settings tab.\n" + e.Message);
            }
        }

        private void TryCloseServerSourceWizardTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.TabCloseButton))
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

        public void WaitForControlVisible(UITestControl control, int searchTimeout = 60000)
        {
            control.WaitForControlCondition((uicontrol) =>
            {
                var point = new Point();
                return control.TryGetClickablePoint(out point);
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

        [When(@"I Wait For Spinner ""(.*)""")]
        public void WaitForSpinner(String control)
        {
            var SpinnerTokens = control.Split(new char[] { '.' });
            if (SpinnerTokens.Length > 1)
            {
                switch (SpinnerTokens[0])
                {
                    case "ExplorerTree":
                        switch (SpinnerTokens[1])
                        {
                            case "FirstRemoteServer":
                                WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
                                break;
                            case "Localhost":
                                WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                                break;

                        }
                        break;
                }
            }
        }

        public void WaitForSpinner(UITestControl control, int searchTimeout = 60000)
        {
            WaitForControlNotVisible(control, searchTimeout);
        }

        [When(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_With_Whitespace_Into_Duplicate_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, false, false, true, SaveOrDuplicate.Duplicate);
        }

        [When(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, false, true, false, SaveOrDuplicate.Duplicate);
        }

        [When(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, false, false, false, SaveOrDuplicate.Duplicate);
        }

        //[When(@"I Click Duplicate From Duplicate Dialog")]
        //public void WhenIClickDuplicateFromDuplicateDialog()
        //{
        //    Click_Duplicate_From_Duplicate_Dialog();
        //    Point point;
        //    // Verify that the 'Exists' property of 'SaveDialogView' window equals 'True'
        //    Assert.IsFalse(SaveDialogWindow.TryGetClickablePoint(out point), "Save Dialog does not exist after clicking Duplicate button");
        //}


        [When(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, false, false, false, SaveOrDuplicate.Save);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName, bool duplicate = false, bool invalid = false, bool nameHasWhiteSpace = false, SaveOrDuplicate saveOrDuplicate = SaveOrDuplicate.Save)
        {

            WpfText errorLabel = this.SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;

            if (duplicate || invalid || nameHasWhiteSpace)
            {
                if (duplicate)
                {
                    Assert.AreEqual(string.Format("An item with name '{0}' already exists in this folder.", ServiceName), errorLabel.DisplayText, "Error is not the same as expected");
                    if (saveOrDuplicate == SaveOrDuplicate.Duplicate)
                        Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                    else
                        Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
                if (invalid)
                {
                    Assert.AreEqual("'Name' contains invalid characters", errorLabel.DisplayText, "Error is not the same as expected");
                    if (saveOrDuplicate == SaveOrDuplicate.Duplicate)
                        Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                    else
                        Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
                if (nameHasWhiteSpace)
                {
                    Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
                    if (saveOrDuplicate == SaveOrDuplicate.Duplicate)
                        Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                    else
                        Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
            }
            else
            {
                if (saveOrDuplicate == SaveOrDuplicate.Duplicate)
                    Assert.IsTrue(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                else
                    Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
            }
        }
        public void Select_FirstItem_From_ServicePicker_Tree()
        {
            var firstItem = ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1;
            Mouse.Click(firstItem);

        }
        public void Filter_ServicePicker_Explorer(string FilterText)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = FilterText;
            WaitForControlVisible(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Filter the Explorer with ""(.*)""")]
        public void Filter_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void Enter_GroupName_Into_Windows_Group_Dialog(string GroupName)
        {
            SelectWindowsGroupDialog.ItemPanel.ObjectNameTextbox.Text = GroupName;
            Assert.IsTrue(SelectWindowsGroupDialog.OKPanel.OK.Enabled, "Windows group dialog OK button is not enabled.");
        }

        [When(@"I Select ""(.*)"" From Service Picker")]
        public void Select_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Click_Service_Picker_Dialog_Refresh_Button();
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        public void TryRefreshExplorerUntilOneItemOnly(int retries = 3)
        {
            while ((ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem) || ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.SecondItem)) && retries-- > 0)
            {
                Click_Explorer_Refresh_Button();
            }
        }

        [When(@"I Select ""(.*)"" From Explorer Remote Server Dropdown List")]
        public void Select_From_Explorer_Remote_Server_Dropdown_List(string serverName)
        {
            switch (serverName)
            {
                default:
                case "Remote Connection Integration":
                    Select_From_Explorer_Remote_Server_Dropdown_List(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text);
                    break;
            }
        }

        public void Select_From_Explorer_Remote_Server_Dropdown_List(WpfText comboboxListItem)
        {
            Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(comboboxListItem.Exists, "Server does not exist in explorer remote server drop down list.");
            Mouse.Click(comboboxListItem, new Point(79, 8));
        }

        public void Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected, new Point(80, 13));
        }

        [When(@"I Select NewRemoteServer From Explorer Server Dropdownlist")]
        public void Select_NewRemoteServer_From_Explorer_Server_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(217, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... does not exist in explorer remote server drop down list");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsNewRemoteServer.NewRemoteServerItemText, new Point(114, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Exists, "Server source wizard does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown.Exists, "Server source wizard protocol dropdown does not exist.");
        }

        public void Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(217, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "localhost (connected) does not exist in explorer remote server drop down list");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(94, 10));
            Assert.AreEqual("localhost", MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsLocalhost.DisplayText, "Selected remote server is not localhost");
        }

        public void Select_localhost_From_Explorer_Remote_Server_Dropdown_List()
        {
            WpfButton serverListComboBox = this.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox;
            WpfCustom comboboxListItemAsLocalhostConnected = this.MainStudioWindow.ComboboxListItemAsLocalhostConnected;
            Mouse.Click(serverListComboBox, new Point(174, 8));
            Mouse.Click(comboboxListItemAsLocalhostConnected);
        }

        [When(@"I Save With Ribbon Button And Dialog As ""(.*)""")]
        public void WhenISaveWithRibbonButtonAndDialogAs(string Name)
        {
            Save_With_Ribbon_Button_And_Dialog(Name, false);
        }

        [When(@"I Save With Ribbon Button And Dialog As ""(.*)"" without filtering the explorer")]
        public void Save_With_Ribbon_Button_And_Dialog_Without_Filtering(string name)
        {
            Save_With_Ribbon_Button_And_Dialog(name, true);
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

        public void Save_With_Ribbon_Button_And_Dialog(string Name, bool skipExplorerFilter = false)
        {
            Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            WaitForSpinner(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Enter_Service_Name_Into_Save_Dialog(Name);
            Click_SaveDialog_Save_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            if (!skipExplorerFilter)
            {
                Filter_Explorer(Name);
                Click_Explorer_Refresh_Button();
                WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Saved " + Name + " does not appear in the explorer tree.");
            }
        }


        [When(@"I Click SaveDialog Save Button")]
        public void Click_SaveDialog_Save_Button()
        {
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow.SaveButton), "Save dialog still exists after clicking save button.");
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        public void TryCloseNewPluginSourceWizardTab()
        {
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.PluginSourceWizardTab.CloseButton))
            {
                Click_Close_Plugin_Source_Wizard_Tab_Button();
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
        }

        public void TryCloseNewWebSourceWizardTab()
        {
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.CloseButton))
            {
                Click_Close_Web_Source_Wizard_Tab_Button();
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
        }
        public void Enter_Text_Into_Unpinned_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariabeName()
        {
            #region Variable Declarations
            WpfEdit textbox = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            #endregion

            // Type '[[SomeVariable]]' in 'UI__Row1_FieldName_AutoID' text box
            textbox.Text = "[[SomeVariable]]";

            // Verify that the 'Text' property of 'UI__Row1_FieldName_AutoID' text box equals '[[SomeVariable]]'
            Assert.AreEqual("[[SomeVariable]]", textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable]]\".");
        }

        [When("I Click New Workflow Ribbon Button")]
        public void Click_New_Workflow_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.NewWorkflowButton, new Point(3, 8));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after clicking new workflow ribbon button.");
        }

        [When(@"I Select Last Source From GET Web Large View Source Combobox")]
        public void Select_Last_Source_From_GET_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
            if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem10))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem10, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem9))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem9, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem8))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem8, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem7))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem7, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem6))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem6, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem5))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem5, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem4))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem4, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem3))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem3, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem2))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem2, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem1))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem1, new Point(163, 17));
            }
            else
            {
                throw new InvalidOperationException("Cannot select last list item from a list with no items.");
            }
        }

        [When(@"I Select Second to Last Source From GET Web Large View Source Combobox")]
        public void Select_Second_to_Last_Source_From_GET_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
            if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem10))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem9, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem9))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem8, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem8))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem7, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem7))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem6, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem6))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem5, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem5))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem4, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem4))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem3, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem3))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem2, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem2))
            {
                Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem1, new Point(163, 17));
            }
            else if (ControlExistsNow(MainStudioWindow.WebServerSourceComboboxListItem1))
            {
                throw new InvalidOperationException("Cannot select second to last list item from a list with only one item.");
            }
            else
            {
                throw new InvalidOperationException("Cannot select second to last list item from a list with no items.");
            }
        }

        [When(@"I Click New Web Source Ribbon Button")]
        public void Click_New_Web_Source_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.WebSourceButton, new Point(13, 18));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Web server address textbox does not exist on new web source wizard tab.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Web server test connection button does not exist on new web source wizard tab.");
        }

        public void First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            var switchLeftAutoConnector = new Point(250, 200);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(switchLeftAutoConnector);
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, switchLeftAutoConnector);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        public void Resize_Assign_LargeTool()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton.ItemIndicator, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(500, 562));
        }

        public void Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem commentToolboxItem = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment;
            WpfCustom flowchart = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector3 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3;
            WpfCustom commentOnTheDesignSurface = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion

            var switchRightAutoConnector = new Point(360, 200);
            flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(commentToolboxItem, new Point(16, 25));
            Mouse.StopDragging(flowchart, switchRightAutoConnector);
            Assert.IsTrue(SwitchCaseDialog.DoneButton.Exists, "Switch case dialog done button does not exist after dragging onto switch case arm.");
            Mouse.Click(SwitchCaseDialog.DoneButton, new Point(34, 10));
            Assert.IsTrue(connector3.Exists, "Third connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(commentOnTheDesignSurface.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
            TryClearToolboxFilter();
        }

        public void Enter_Text_Into_Debug_Input_Row1_Value_Textbox(string text)
        {
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            }
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debug input data row1 textbox text is not equal to \'" + text + "\' after typing that in.");
        }

        public void Enter_Text_Into_Debug_Input_Row2_Value_Textbox(string text)
        {
            MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debug input data row2 textbox text is not equal to \'" + text + "\' after typing that in.");
        }

        [When(@"I Click Debug Ribbon Button")]
        public void Click_Debug_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.RunAndDebugButton, new Point(13, 14));
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input window does not exist after clicking debug ribbon button.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.DebugF6Button.Exists, "Debug button in Debug Input window does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.CancelButton.Exists, "Cancel Debug Input Window button does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Exists, "Remember Checkbox does not exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "View in Browser button does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Exists, "Input Data Window does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists, "Xml tab does not Exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.Exists, "Assert Json tab does not exist in the debug input window.");
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.PluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Text = text;
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after DLL has been selected in plugin source wizard.");
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text = GroupName;
            Assert.AreEqual(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
        }

        [When(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        public void SetResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {
            Click_Settings_Ribbon_Button();
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Select_Service_From_Service_Picker_Dialog(ResourceName);
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

        [When(@"I Create Remote Server Source As ""(.*)"" with address ""(.*)""")]
        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress)
        {
            CreateRemoteServerSource(ServerSourceName, ServerAddress, false);
        }

        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress, bool PublicAuth = false)
        {
            Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext
                .NewServerSourceWizard.AddressComboBox.AddressEditBox.Text = ServerAddress;
            if (ServerAddress == "tst-ci-")
            {
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.TSTCIREMOTE.Exists, "TSTCIREMOTE does not exist in server source wizard drop down list after starting by typing tst-ci-.");
                Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            }
            if (PublicAuth)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
            }
            Click_Server_Source_Wizard_Test_Connection_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.ErrorText.Spinner);
            Save_With_Ribbon_Button_And_Dialog(ServerSourceName);
            Click_Close_Server_Source_Wizard_Tab_Button();
        }

        public void Select_Deploy_First_Source_Item()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Enabled,
                "Deploy button is not enable after valid server and resource are selected.");
        }

        public void Click_Deploy_Tab_Deploy_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButton);
            if (ControlExistsNow(MessageBoxWindow))
            {
                //Dismiss Server Version Conflict Dialog
                Mouse.Click(MessageBoxWindow.OKButton);
            }
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Spinner);
            var displayText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButtonMessageText.DisplayText;
            Assert.IsTrue(MessageBoxWindow.Exists);
        }

        public void Change_Selected_Database_ToMySql_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MicrosoftSQLServer, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemMySqlDatabase.Exists, "ComboboxListItemMySqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemMySqlDatabase.MySqlDatabaseText, new Point(106, 19));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToMicrosoftSqlServer_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MicrosoftSQLServer, new Point(87, 7));
            Assert.IsTrue(serverTypeComboBox.Exists, "ComboboxListItemMySqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(serverTypeComboBox, new Point(106, 19));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToOracle_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.PostgreSQLDatabaseText, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemOracleDatabase.Exists, "ComboboxListItemOracleDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemOracleDatabase);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToODBC_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.OracleDatabase, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemODBCDatabase.Exists, "ComboboxListItemODBCDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemODBCDatabase.ODBCDatabaseText);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToPostgreSql_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MySqlDatabase, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemPostgreSqlDatabase.Exists, "ComboboxListItemPostgreSqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemPostgreSqlDatabase.PostgreSQLDatabase);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Click_Settings_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.ConfigureSettingsButton.Exists, "Settings ribbon does not exist.");
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 2));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "settings tab does not exist after clicking settings ribbon button.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "Security tab does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Exists, "Logging tab does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Exists, "Resource Permissions does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Server Permissions does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.Exists, "Settings security tab resource permissions row1 does not exist");
        }

        [When(@"I Click Deploy Ribbon Button")]
        public void Click_Deploy_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.DeployButton.Exists, "Deploy ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.DeployButton, new Point(16, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "Deploy tab does not exist after clicking deploy ribbon button.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.Exists, "Source explorer tree does not exist on deploy.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.Exists, "Source server name in deploy window does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.RefreshSourceServerButton.Exists, "Refresh button source server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.FilterText.Exists, "Filter source server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.OverrideHyperlink.Exists, "Override count in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.NewResourceHyperlink.Exists, "New Resource count in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.EditSourceButton.Exists, "Edit source server button does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceConnectButton.Exists, "Connect button in the Source server does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.EditDestinationButton.Exists, "Edit Destination Server button does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.ConnectDestinationButton.Exists, "Connect Button in Destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceConnectControl.Exists, "Source Server connect control does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ShowDependenciesButton.Exists, "Select All Dependencies button Destination Server does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ServicesText.Exists, "Services Label in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.ServiceCountText.Exists, "Service Count value in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourcesText.Exists, "Source label in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceCountText.Exists, "Source Count value in the destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.NewResourcesText.Exists, "New Resource Label in the destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.OverrideText.Exists, "Override label on Destination Server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Exists, "Deploy button in Destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DeployButtonMessageText.Exists, "Success message label does not exist in destination server of the deploy window");
        }

        public void TryCloseDeployTab()
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

        public void Click_EnableDisable_This_Test_CheckBox(bool nameContainsStar = false, int testInstance = 1)
        {
            var currentTest = GetCurrentTest(testInstance);
            var testRunState = GetTestRunState(testInstance, currentTest);
            var selectedTestDeleteButton = GetSelectedTestDeleteButton(currentTest, testInstance);
            var beforeClick = testRunState.Checked;
            testRunState.DrawHighlight();

            Mouse.Click(testRunState);
            WaitForControlVisible(testRunState);
            Assert.AreNotEqual(beforeClick, testRunState.Checked);

            WaitForControlVisible(selectedTestDeleteButton);
            if (beforeClick)
                Assert.IsTrue(selectedTestDeleteButton.Enabled, "Delete button is disabled");
            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        public void Drag_From_Explorer_Onto_DesignSurface(string ServicePath)
        {
            Filter_Explorer(ServicePath);
            Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface();
        }

        [When("I Drag Dice Roll Example Onto DesignSurface")]
        public void Drag_Dice_Roll_Example_Onto_DesignSurface()
        {
            Filter_Explorer("Dice Roll");
            Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
        }

        [When(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        public void Select_Show_Dependencies_In_Explorer_Context_Menu(string ServiceName)
        {
            #region Variable Declarations
            WpfTreeItem firstItem = this.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem showDependencies = this.MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfRadioButton showwhatdependsonthisRadioButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox;
            WpfButton refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton;
            WpfText text = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text;
            #endregion

            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Mouse.Click(showDependencies, new Point(50, 15));
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(refreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies" +
                    " is selected");
        }

        [When(@"I Click DB Source Wizard Test Connection Button")]
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
        }

        [When(@"I Deploy ""(.*)"" From Deploy View")]
        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.SearchTextboxText = ServiceName;
            Enter_DeployViewOnly_Into_Deploy_Source_Filter();
            Select_Deploy_First_Source_Item();
            Click_Deploy_Tab_Deploy_Button();
            Click_MessageBox_OK();
        }

        public void Enter_Values_Into_Data_Merge_Tool_Large_View()
        {
            var row1InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row.InputCell.Row1InputVariabComboBox;
            var row1UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row.UsingCell.Row1UsingComboBox;
            var row2InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row2.InputCell.Row2InputVariabComboBox;
            var row2UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row2.UsingCell.Row2UsingComboBox;

            row1InputVariabComboBox.TextEdit.Text = "VarA";
            row1UsingComboBox.TextEdit.Text = "1";
            row2InputVariabComboBox.TextEdit.Text = "VarB";
            row2UsingComboBox.TextEdit.Text = "2";

            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox, ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DatGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox.NewLineListItem, ModifierKeys.None);
        }

        public void Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest()
        {
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox;

            var helloUser = "Hello User.";
            Keyboard.SendKeys(textbox, helloUser, ModifierKeys.None);

            // Verify that the 'Text' property of 'Text' text box equals 'User'
            Assert.AreEqual(helloUser, textbox.Text, "Workflow tests output row 1 value textbox text does not equal 'Hello User' after typing that in.");
        }

        public void Select_Test(int instance = 1)
        {
            var currentTest = GetCurrentTest(instance);
            Mouse.Click(currentTest);
        }

        public void Click_RunAll_Button(string BrokenRule = null)
        {
            string DuplicateNameError = "DuplicateNameError";
            string UnsavedResourceError = "UnsavedResourceError";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton, new Point(35, 10));
            Assert.AreEqual("Window", MessageBoxWindow.ControlType.ToString(), "Messagebox does not exist after clicking RunAll button");

            if (!string.IsNullOrEmpty(BrokenRule))
            {
                if (BrokenRule.ToUpper().Equals(UnsavedResourceError))
                    Assert.AreEqual("Please save currently edited Test(s) before running the tests.", MessageBoxWindow.UIPleasesavecurrentlyeText.DisplayText, "Message is not Equal to Please save currently edited Test(s) before running the t" +
                            "ests.");
                if (BrokenRule.ToUpper().Equals(DuplicateNameError))
                    Assert.AreEqual("Please save currently edited Test(s) before running the tests.", MessageBoxWindow.UIPleasesavecurrentlyeText.DisplayText, "Messagebox does not show duplicated name error");
            }
        }

        public void CreateAndSave_Dice_Workflow(string WorkflowName)
        {
            Select_NewWorkFlowService_From_ContextMenu();
            Drag_Toolbox_Random_Onto_DesignSurface();
            Enter_Dice_Roll_Values();
            Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            Click_Close_Workflow_Tab_Button();
        }

        [When(@"I UnCheck Public Administrator")]
        public void UnCheck_Public_Administrator()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            WpfCheckBox public_DeployToCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox;
            WpfCheckBox public_DeployFromCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox;
            #endregion
            
            public_AdministratorCheckBox.Checked = false;
            Assert.IsFalse(public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Administrator.");
            Assert.IsTrue(public_ViewCheckBox.Checked, "Public View checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox unchecked after unChecking Administrator.");
            Assert.IsTrue(public_ContributeCheckBox.Checked, "Public Contribute checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_DeployFromCheckBox.Checked, "Public DeplotFrom checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_DeployToCheckBox.Checked, "Public DeployTo checkbox is unchecked after unChecking Administrator.");
        }

        [When(@"I Check Resource Contribute")]
        public void Check_Resource_Contribute()
        {
            WpfCheckBox resource_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfButton resource_DeleteButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.DeleteCell.DeleteButton;

            resource_ContributeCheckBox.Checked = true;
            Assert.IsTrue(resource_ViewCheckBox.Checked, "Resource View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_ExecuteCheckBox.Checked, "Resource Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_DeleteButton.Enabled, "Resource Delete button is disabled");

        }
        
       

        [When(@"I Check Public Contribute")]
        public void Check_Public_Contribute()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            public_ContributeCheckBox.Checked = true;
            Assert.IsTrue(public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
        }

        public void Scroll_Down_Then_Up_On_The_DataMerge_SmallView()
        {
            Mouse.MoveScrollWheel(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable, -1);
        }

        [When(@"I UnCheck Public View")]
        public void UnCheck_Public_View()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            public_ViewCheckBox.Checked = false;
            Assert.IsFalse(public_ViewCheckBox.Checked, "Public View checkbox is checked after Checking Contribute.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsFalse(public_ContributeCheckBox.Checked, "Public Contribute checkbox is checked after UnChecking Execute/View.");
            Assert.IsFalse(public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Contribute.");
        }

        [When(@"I Update Test Name To ""(.*)""")]
        public void Update_Test_Name(string overrideName = null)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameTextbox, new Point(59, 16));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = "";
            if (!string.IsNullOrEmpty(overrideName))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = overrideName;
            else
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = "Dice_Test";
        }

        public void Click_Delete_Test_Button(int testInstance = 1)
        {
            var currentTest = GetCurrentTest(testInstance);
            var selectedTestDeleteButton = GetSelectedTestDeleteButton(currentTest, testInstance);
            selectedTestDeleteButton.DrawHighlight();
            Mouse.Click(selectedTestDeleteButton);
            Assert.IsTrue(MessageBoxWindow.Exists);
        }

        private static WpfText GetSelectedTestRunTimeDisplay(WpfListItem test, int instance)
        {
            WpfText testRunTimeDisplay;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    testRunTimeDisplay = test2.RunTimeDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    testRunTimeDisplay = test3.RunTimeDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    testRunTimeDisplay = test4.RunTimeDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    testRunTimeDisplay = test1.RunTimeDisplay;
                    break;
            }
            return testRunTimeDisplay;
        }

        private static WpfText GetSelectedTestNeverRunDisplay(WpfListItem test, int instance)
        {
            WpfText neverRunDisplay;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    neverRunDisplay = test2.NeverRunDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    neverRunDisplay = test3.NeverRunDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    neverRunDisplay = test4.NeverRunDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    neverRunDisplay = test1.NeverRunDisplay;
                    break;
            }
            return neverRunDisplay;
        }

        public void Click_Run_Test_Button(TestResultEnum? expectedTestResultEnum = null, int instance = 1)
        {
            var currentTest = GetCurrentTest(instance);
            var selectedTestRunButton = GetSelectedTestRunButton(currentTest, instance);

            Mouse.Click(selectedTestRunButton);
            if (expectedTestResultEnum != null)
                AssertTestResults(expectedTestResultEnum.Value, instance, currentTest);
        }

        private void AssertTestResults(TestResultEnum expectedTestResultEnum, int instance, WpfListItem currentTest)
        {
            switch (expectedTestResultEnum)
            {
                case TestResultEnum.Invalid:
                    TestResults.GetSelectedTestInvalidResult(currentTest, instance);
                    break;
                case TestResultEnum.Pending:
                    TestResults.GetSelectedTestPendingResult(currentTest, instance);
                    break;
                case TestResultEnum.Pass:
                    TestResults.GetSelectedTestPassingResult(currentTest, instance);
                    break;
                case TestResultEnum.Fail:
                    TestResults.GetSelectedTestFailingResult(currentTest, instance);
                    break;
            }
        }

        public void Click_Duplicate_Test_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.DuplicateButton, new Point(14, 10));
        }

        public void Assert_Test_Result(string result)
        {
            WpfText passing = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing;
            WpfText invalid = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Invalid;
            WpfText failing = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing;
            WpfText pending = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending;
            if (result == "Passing")
                Assert.IsTrue(passing.Exists, "Test is not passing");
            if (result == "Failing")
                Assert.IsTrue(failing.Exists, "Test is not failing");
            if (result == "Invalid")
                Assert.IsTrue(invalid.Exists, "Test is not invalid");
            if (result == "Pending")
                Assert.IsTrue(pending.Exists, "Test is not pending");
        }

        const string Tab = "Tab";
        const string Test = "Test";
        public void Click_Create_New_Tests(bool nameContainsStar = false, int testInstance = 1)
        {
            #region Variable Declarations
            WpfText testNameText = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameText;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            #endregion

            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));

            var currentTest = GetCurrentTest(testInstance);
            var testEnabledSelector = GetTestRunState(testInstance, currentTest).Checked;
            var testNeverRun = GetSelectedTestNeverRunDisplay(currentTest, testInstance);

            Assert.AreEqual("Never run", testNeverRun.DisplayText);
            AssertTestResults(TestResultEnum.Pending, testInstance, currentTest);
            Assert.IsTrue(testNameText.Exists, string.Format("Test{0} Name textbox does not exist after clicking Create New Test", testInstance));
            Assert.IsTrue(testEnabledSelector, string.Format("Test {0} is diabled after clicking Create new test from context menu", testInstance));

            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        private void Assert_Display_Text_ContainStar(string control, bool containsStar, int instance = 1)
        {
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            var test = GetCurrentTest(instance);
            string description = string.Empty;
            if (control == "Tab")
            {
                description = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText;
                if (containsStar)
                    Assert.IsTrue(description.Contains("*"), description + " DOES NOT contain a Star");
                else
                    Assert.IsFalse(description.Contains("*"), description + " contains a Star");
            }
            else if (control == "Test")
            {
                description = GetTestNameDisplayText(instance, test).DisplayText;
                if (containsStar)
                    Assert.IsTrue(description.Contains("*"), description + " DOES NOT contain a Star");
                else
                    Assert.IsFalse(description.Contains("*"), description + " contains a Star");
            }

            if (containsStar)
                Assert.IsTrue(description.Contains("*"));
            else
                Assert.IsFalse(description.Contains("*"));
            if (instance == 0)
            {
                var descriptions = testsListBox.GetContent();
                Assert.IsFalse(descriptions.Contains("*"));
            }
        }

        private WpfCheckBox GetTestRunState(int testInstance, WpfListItem test)
        {
            WpfCheckBox value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.TestEnabledSelector;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.TestEnabledSelector;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.TestEnabledSelector;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.TestEnabledSelector;
                    break;
            }
            return value;
        }
        private WpfText GetTestNameDisplayText(int instance, WpfListItem test)
        {
            WpfText property;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    property = test2.TestNameDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    property = test3.TestNameDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    property = test4.TestNameDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    property = test1.TestNameDisplay;
                    break;
            }

            return property;
        }

        
        public void Select_Service_From_Service_Picker(string serviceName, bool inSubFolder = false)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = serviceName;
            Mouse.Click(ServicePickerDialog.Explorer.Refresh, new Point(5, 5));
            WaitForSpinner(ServicePickerDialog.Explorer.ExplorerTree.Localhost.Checkbox.Spinner);
            if (inSubFolder)
            {
                Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem2.TreeItem1, new Point(73, 12));
            }
            else
            {
                Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1, new Point(53, 12));
            }
            Mouse.Click(ServicePickerDialog.OK, new Point(52, 15));
        }

        public WpfListItem GetCurrentTest(int testInstance)
        {
            WpfListItem test;
            switch (testInstance)
            {
                case 2:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2;
                    break;
                case 3:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3;
                    break;
                case 4:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4;
                    break;
                default:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1;
                    break;
            }
            return test;
        }

        public WpfButton GetSelectedTestRunButton(WpfListItem test, int testInstance = 1)
        {
            WpfButton value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.RunButton;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.RunButton;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.RunButton;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.RunButton;
                    break;
            }
            return value;
        }

        public WpfButton GetSelectedTestDeleteButton(WpfListItem test, int testInstance = 1)
        {
            WpfButton value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.DeleteButton;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.DeleteButton;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.DeleteButton;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.DeleteButton;
                    break;
            }
            return value;
        }

        [When("I Click Save Ribbon Button Without Expecting a Dialog")]
        public void Click_Save_Ribbon_Button_Without_Expecting_A_Dialog()
        {
            Click_Save_Ribbon_Button_With_No_Save_Dialog(2000);
        }

        [When(@"I Click Save Ribbon Button With No Save Dialog")]
        public void Click_Save_Ribbon_Button_With_No_Save_Dialog(int WaitForSave = 2000)
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Exists, "Save ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
            MainStudioWindow.SideMenuBar.SaveButton.WaitForControlCondition(uicontrol => !uicontrol.Enabled, WaitForSave * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
            Assert.IsFalse(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is still enabled after clicking it and waiting for " + WaitForSave + "ms.");
        }

        public void DeleteAssign_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem delete = this.MainStudioWindow.DesignSurfaceContextMenu.Delete;
            WpfWindow messageBoxWindow = this.MessageBoxWindow;
            WpfCustom multiAssign = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            #endregion

            var point = new Point();
            Assert.IsTrue(multiAssign.TryGetClickablePoint(out point));
            // Right-Click 'DsfMultiAssignActivity' custom control
            Mouse.Click(multiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));

            // Click 'Delete' menu item
            Mouse.Click(delete, new Point(27, 18));
            Assert.IsFalse(multiAssign.TryGetClickablePoint(out point));
        }

        public void Enter_Recordset_values()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "5";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "10";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.VariableCell.IntellisenseCombobox.Textbox.Text = "[[var]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.ValueCell.IntellisenseCombobox.Textbox.Text = "1";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.VariableCell.IntellisenseCombobox.Textbox.Text = "[[mr()]]";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.Exists, "var does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.Exists, "rec().a does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.Exists, "rec().b does not exist in the variable explorer");
        }

        public void Click_AddNew_Web_Source_From_PutWebtool()
        {
            #region Variable Declarations
            WpfButton newSourceButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton;
            #endregion

            // Verify that the 'Exists' property of 'New' button equals 'True'
            Assert.IsTrue(newSourceButton.Exists, "New Source Button does not exist");

            // Click 'New' button
            Mouse.Click(newSourceButton, new Point(30, 4));
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

        [When(@"I Click DotNet DLL Large View Generate Outputs")]
        public void Click_DotNet_DLL_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.DoneButton.Exists);
        }

        [When(@"I Click New Web Source Test Connection Button")]
        public void Click_New_Web_Source_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(52, 14));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after testing a valid web source.");
        }

        public void Click_Scheduler_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SchedulerButton, new Point(4, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.Exists, "Scheduler tab does not exist after clicking scheduler ribbon button.");
        }

        public void Click_Scheduler_ResourcePicker()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton, new Point(20, 12));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service picker dialog doesn't exist after clicking the resource picker button.");
        }

        public void Click_Service_Picker_Dialog_OK()
        {
            Mouse.Click(ServicePickerDialog.OK, new Point(52, 10));
            Assert.IsFalse(ControlExistsNow(ServicePickerDialog), "Service picker dialog still exists after clicking OK button.");
        }

        public void Click_Service_Picker_Dialog_Cancel()
        {
            Mouse.Click(ServicePickerDialog.Cancel, new Point(57, 6));
            Assert.IsFalse(ControlExistsNow(ServicePickerDialog), "Service picker dialog still exists after clicking cancel button.");
        }

        public void Click_Service_Picker_Dialog_Refresh_Button()
        {
            Mouse.Click(ServicePickerDialog.Explorer.Refresh, new Point(10, 11));
            WaitForSpinner(ServicePickerDialog.Explorer.ExplorerTree.Localhost.Checkbox.Spinner);
        }

        [When("I Click Subworkflow Done Button")]
        public void Click_Subworkflow_Done_Button()
        {
            Assert.IsTrue(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
            Mouse.Click(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton, new Point(53, 16));
        }

        public void Click_Assign_Tool_url()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
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

        public void Create_Resource_In_Folder1()
        {
            #region Variable Declarations
            WpfTreeItem folder1 = this.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem newWorkflow = this.MainStudioWindow.ExplorerContextMenu.NewWorkflow;
            #endregion

            // Right-Click 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item
            Mouse.Click(folder1, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));

            // Verify that the 'Enabled' property of 'New Workflow Service' menu item equals 'True'
            Assert.AreEqual(this.Select_NewWorkFlowService_From_ContextMenuParams.NewWorkflowEnabled, newWorkflow.Enabled, "NewWorkFlowService button is disabled.");

            // Click 'New Workflow Service' menu item
            Mouse.Click(newWorkflow, new Point(79, 13));

            Drag_Toolbox_Random_Onto_DesignSurface();
        }

        public void Click_Assign_Tool_Remove_Variable_From_Tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Right}{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid%Variable]]\".");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Exists, "Assign large view row 1 variable textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Exists, "Variable filter textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton.Exists, "Variable clear filter button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton, new Point(8, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Exists, "Variable delete does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton, new Point(9, 8));
        }

        [When(@"I Refresh Explorer")]
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void TryRemoveTests()
        {
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            if (testsListBox.GetContent().Length >= 6)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 5);
                Click_Delete_Test_Button(5);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 5)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 4);
                Click_Delete_Test_Button(4);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 4)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 3);
                Click_Delete_Test_Button(3);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 3)
            {
                Select_Test(2);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 2);
                Click_Delete_Test_Button(2);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 2)
            {
                Select_Test();
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true);
                Click_Delete_Test_Button();
                Click_MessageBox_Yes();
            }
            Click_Close_Tests_Tab();
        }
        /// <summary>
        /// Select_SharepointTestServer - Use 'Select_SharepointTestServerParams' to pass parameters into this method.
        /// </summary>
        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer_FromSharepointDelete_tool()
        {
            #region Variable Declarations
            WpfComboBox server = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server;
            WpfListItem sharepointTestServer = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server.SharepointTestServer;
            WpfButton editSourceButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton;
            #endregion

            // Click 'UI__Database_AutoID' combo box
            Mouse.Click(server, new Point(98, 12));

            // Click '{"Server":"http://rsaklfsvrsharep/","Authenticatio...' list item
            Mouse.Click(sharepointTestServer, new Point(67, 13));

            // Verify that the 'Enabled' property of '...' button equals 'True'
            Assert.IsTrue(editSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }
        /// <summary>
        /// Select_AcceptanceTestin_create
        /// </summary>
        [When(@"I Select AcceptanceTestin delete")]
        public void Select_AcceptanceTestin_From_DeleteTool()
        {
            #region Variable Declarations
            WpfComboBox methodList = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList;
            WpfListItem uIAcceptanceTesting_CrListItem = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAcceptanceTesting_CrListItem;
            #endregion

            // Click 'UI__TableName_AutoID' combo box
            Mouse.Click(methodList, new Point(119, 7));

            // Click 'AcceptanceTesting_Create' list item
            Mouse.Click(uIAcceptanceTesting_CrListItem, new Point(114, 13));
        }
        /// <summary>
        /// Select_AppData_From_MethodList
        /// </summary>
        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_DeleteTool()
        {
            #region Variable Declarations
            WpfComboBox methodList = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList;
            WpfListItem uIAppdataListItem = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAppdataListItem;
            #endregion

            // Click 'UI__TableName_AutoID' combo box
            Mouse.Click(methodList, new Point(174, 7));

            // Click 'appdata' list item
            Mouse.Click(uIAppdataListItem, new Point(43, 15));
        }

        /// <summary>
        /// Select_AppData_From_MethodList
        /// </summary>
        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_UpdateTool()
        {
            #region Variable Declarations
            WpfComboBox methodList = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList;
            WpfListItem uIAppdataListItem = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList.UIAppdataListItem;
            #endregion

            // Click 'UI__TableName_AutoID' combo box
            Mouse.Click(methodList, new Point(174, 7));

            // Click 'appdata' list item
            Mouse.Click(uIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        public void Click_View_Tests_In_Explorer_Context_Menu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Show_Explorer_First_Item_Tests_With_Context_Menu();
        }

        [Given(@"That The First Test ""(.*)"" Unsaved Star")]
        [Then(@"The First Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        [Then(@"The Added Test ""(.*)"" Unsaved Star")]
        public void ThenTheAddedTestUnsavedStar(string p0)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(p0 == "Has");
        }

        [Then(@"I delete Second Added Test")]
        public void ThenIDeleteSecondAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector, new Point(10, 10));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.DeleteButton, new Point(10, 10));
            Click_MessageBox_Yes();
        }

        [Then(@"I delete Added Test")]
        public void ThenIDeleteAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.DeleteButton, new Point(10, 10));
            Click_MessageBox_Yes();
        }

        [Given(@"That The Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Added ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(bool HasStar)
        {
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestNameDisplay.DisplayText.Contains("*"), "First test title does not contain unsaved star.");
        }
        public void Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(bool HasStar)
        {
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestNameDisplay.DisplayText.Contains("*"), "First test title does not contain unsaved star.");
        }

        [Given(@"That The Second Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second test title does not contain unsaved star.");
        }
        [Given(@"That The Second Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Added Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second Added test title does not contain unsaved star.");
        }

        [When(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        public void Click_Duplicate_From_ExplorerContextMenu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Duplicate_Explorer_Localhost_First_Item_With_Context_Menu();
        }

        [When(@"I Click The Create a New Test Button")]
        public void Click_Workflow_Testing_Tab_Create_New_Test_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        [Given("The First Test Exists")]
        [Then("The First Test Exists")]
        public void Assert_Workflow_Testing_Tab_First_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Added Test Exists")]
        [Then("The Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Second Test Exists")]
        [Then("The Second Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "No second test on workflow testing tab.");
        }

        [Given("The Second Added Test Exists")]
        [Then("The Second Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.Exists, "No second Added test on workflow testing tab.");
        }

        [When("I Toggle First Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector, new Point(10, 10));
        }

        [When("I Toggle First Added Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Added_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector, new Point(10, 10));
        }

        [When("I Click Test (.*) Run Button")]
        public void Click_Test_Run_Button(int index)
        {
            switch (index)
            {
                default:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.RunButton, new Point(10, 10));
                    break;
                case 2:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.RunButton, new Point(10, 10));
                    break;
                case 3:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.RunButton, new Point(10, 10));
                    break;
                case 4:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.RunButton, new Point(10, 10));
                    break;
            }
        }

        [When("I Click First Test Delete Button")]
        public void Click_First_Test_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.DeleteButton, new Point(10, 10));
        }

        [When(@"I Click First Test Run Button")]
        public void Click_First_Test_Run_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.RunButton, new Point(10, 10));
        }

        public void Select_First_Test()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1, new Point(80, 10));
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointDelete()
        {
            var refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointUpdate()
        {
            var refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton()
        {
            var refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }
        public void Click_Sharepoint_RefreshButton_From_SharepointRead()
        {
            var refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        [When(@"I wait for output spinner")]
        public void WhenIWaitForOutputSpinner()
        {
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        [When("I Click Run All Button")]
        public void Click_Workflow_Testing_Tab_Run_All_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton, new Point(35, 10));
        }

        [Given(@"That The First Test ""(.*)"" Passing")]
        [Then(@"The First Test ""(.*)"" Passing")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Passing(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(bool passing = true)
        {
            Assert.AreEqual(passing, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists, (passing ? "First test is not passing." : "First test is passing."));
        }

        [Given(@"That The First Test ""(.*)"" Invalid")]
        [Then(@"The First Test ""(.*)"" Invalid")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(bool invalid = true)
        {
            Assert.AreEqual(invalid, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Invalid.Exists, (invalid ? "First test is not invalid." : "First test is invalid."));
        }

        public void Delete_Assign_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on design surface after deleting with context menu.");
        }
        public void Delete_HelloWorld_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Point newPoint;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.TryGetClickablePoint(out newPoint), "HelloWorldWorkFlow still exists on design surface after deleting with context menu.");
        }

        public void Delete_Assign_With_Context_Menu_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on unpinned design surface after deleting with context menu.");
        }

        public void Debug_Workflow_With_Ribbon_Button()
        {
            Click_Debug_Ribbon_Button();
            Click_DebugInput_Debug_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        public void Click_DebugInput_Debug_Button_For_UnpinnedWindow()
        {            
            #region Variable Declarations
            WpfButton debugF6Button = this.MainStudioWindow.DebugInputDialog.DebugF6Button;
            WpfCustom debugOutput = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput;
            WpfButton settingsButton = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.SettingsButton;
            WpfButton expandCollapseButton = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.ExpandCollapseButton;
            WpfEdit searchTextBox = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.SearchTextBox;
            WpfTree debugOutputTree = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree;
            #endregion

            // Verify that the 'Enabled' property of 'Debug (F6)' button equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.DebugF6ButtonEnabled, debugF6Button.Enabled, "DebugF6Button is not enabled after clicking RunDebug from Menu.");

            // Click 'Debug (F6)' button
            Mouse.Click(debugF6Button, new Point(34, 10));

            // Verify that the 'Exists' property of 'OUTPUT' custom control equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.DebugOutputExists, debugOutput.Exists, "Debug output does not exist");
            debugOutput.DrawHighlight();
            // Verify that the 'Exists' property of '?' button equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.SettingsButtonExists, settingsButton.Exists, "Debug output settings button does not exist");

            // Verify that the 'Exists' property of '+' button equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.ExpandCollapseButtonExists, expandCollapseButton.Exists, "Debug output expand collapse button does not exist");

            // Verify that the 'Exists' property of 'SearchTextBox' text box equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.SearchTextBoxExists, searchTextBox.Exists, "Debug output filter textbox does not exist");

            // Verify that the 'Exists' property of 'DebugOutputTree' tree equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.DebugOutputTreeExists, debugOutputTree.Exists, "Debug output results tree does not exist");
            debugOutputTree.DrawHighlight();
            // Verify that the 'Exists' property of '?' button equals 'True'
            Assert.AreEqual(this.Click_DebugInput_Debug_ButtonParams.SettingsButtonExists1, settingsButton.Exists, "Debug output settings button does not exist");
        }

        public void Debug_Unpinned_Workflow_With_Ribbon_Button()
        {
            Click_Debug_Ribbon_Button();
            Click_DebugInput_Debug_Button_For_UnpinnedWindow();
            this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.StatusBar.DrawHighlight();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        public void Remove_Assign_Row_1_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1.");
        }

        public void Remove_Assign_Row_1_With_Context_Menu_On_Unpinned_Tab()
        {            
            this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.DrawHighlight();
            Mouse.Click(this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            StartNodePopupWindow.DesignSurfaceMenu.DeleteRowMenuItem.DrawHighlight();
            Mouse.Click(StartNodePopupWindow.DesignSurfaceMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(ControlExistsNow(this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1 on unpinned tab.");
        }

        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText)
        {
            Enter_Variable_And_Value_Into_Assign(VariableText, ValueText, 1);
        }

        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1.");
                    break;
            }
        }

        
        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1 on unpinned tab")]
        public void Enter_Variable_And_Value_Into_Assign_On_Unpinned_Tab(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2_On_Unpinned_tab();
                    Assert.IsTrue(this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2 on unpinned tab.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab();
                    Assert.IsTrue(this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1 on unpinned tab.");
                    break;
            }
        }

        /// <summary>
        /// Click_Explorer_RemoteServer_Edit_Button - Use 'Click_Explorer_RemoteServer_Edit_ButtonParams' to pass parameters into this method.
        /// </summary>
        public void Click_Explorer_RemoteServer_Edit_Button()
        {
            #region Variable Declarations
            WpfButton editServerButton = this.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton;
            WpfTabPage serverSourceWizardTab = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab;
            //            WpfText uIRemoteConnectionInteText = this.UIWarewolfDEV2SANELEMTWindow.UIUI_SplitPane_AutoIDCustom.UIUI_TabManager_AutoIDTabList.UIDev2ViewModelsSourceTabPage.UIRemoteConnectionInteText;
            #endregion

            // Click '...' button
            Mouse.Click(editServerButton, new Point(11, 10));

            // Verify that the 'Exists' property of 'Dev2.ViewModels.SourceViewModel`1[Dev2.Common.Inte...' tab equals 'True'
            Assert.IsTrue(serverSourceWizardTab.Exists, "Server Source Tab was not open.");

            // Verify that the 'DisplayText' property of 'Remote Connection Integration *' label contains '*'
            //Assert.IsFalse(uIRemoteConnectionInteText.DisplayText.Contains("*"), "Remote Connection Intergration Tab does not contain the star");
        }

        public void Enter_Text_Into_Assign_QviLarge_View()
        {
            #region Variable Declarations
                        
            var qviVariableListBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviVariableListBoxEdit;
            var qviSplitOnCharacterEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviSplitOnCharacterEdit;
            var prefixEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PrefixEdit;
            var suffixEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.SuffixEdit;
            #endregion

            qviVariableListBoxEdit.Text = "varOne,varTwo,varThree";
            qviSplitOnCharacterEdit.Text = ",";
            prefixEdit.Text = "some(<).";
            suffixEdit.Text = "_suf";
        }

        public void Enter_Text_Into_EmailSource_Tab()
        {
            #region Variable Declarations
            WpfEdit hostTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.HostTextBoxEdit;
            WpfEdit userNameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.UserNameTextBoxEdit;
            WpfEdit passwordTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PasswordTextBoxEdit;
            WpfEdit portTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PortTextBoxEdit;
            WpfEdit timeoutTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.TimeoutTextBoxEdit;
            WpfEdit fromTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.FromTextBoxEdit;
            WpfEdit toTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.ToTextBoxEdit;
            #endregion

            hostTextBoxEdit.Text = "localhost";
            userNameTextBoxEdit.Text = "test";
            passwordTextBoxEdit.Text = "test";
            portTextBoxEdit.Text = "2";
            fromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            toTextBoxEdit.Text = "dev2warewolf@gmail.com";
        }

        public void Enter_Number_To_Format()
        {
            #region Variable Declarations
            WpfEdit numberToFormat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.NumberInputComboBox.TextEdit;
            #endregion

            numberToFormat.Text = "5.8961";
        }
        public void Enter_Decimals_To_Show()
        {
            #region Variable Declarations
            WpfEdit numberToFormat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.DecimalsToShowComboBox.TextEdit;
            #endregion

            numberToFormat.Text = "2";
        }
        public void Enter_Result_Variable_On_Random_Tool()
        {
            #region Variable Declarations
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Result_Variable_Into_DateTime()
        {
            #region Variable Declarations
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Result_Variable_Into_DateTimeDifference()
        {
            #region Variable Declarations
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Result_Variable_Into_Web_Request()
        {
            #region Variable Declarations
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Result_Variable()
        {
            #region Variable Declarations
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.ResultInputComboBox.TextEdit;
            #endregion

            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Text_Into_Aggregate_Calculate_Large_View()
        {
            #region Variable Declarations
            WpfEdit fx = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.fxComboBox.TextEdit;
            WpfEdit resultscombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            fx.Text = "Sum(5,5)";
            resultscombobox.Text = "[[results]]";
        }
        public void Enter_Text_Into_DateTime_Input()
        {
            #region Variable Declarations
            WpfEdit input = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.InputComboBox.TextEdit;
            WpfEdit inputFormat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.InputFormatComboBox.TextEdit;
            WpfEdit outputFormat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.OutputFormatComboBox.TextEdit;
            #endregion
            input.Text = "20/03/2016";
            inputFormat.Text = "dd/mm/yyyy";
            outputFormat.Text = "yyyy mm";
        }
        public void Enter_Text_Into_DateTimeDiffetence_Tool()
        {
            #region Variable Declarations
            WpfEdit input1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.Input1ComboBox.TextEdit;
            WpfEdit input2 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.Input2ComboBox.TextEdit;
            WpfEdit inputFormat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.InputFormatComboBox.TextEdit;
            #endregion
            input1.Text = "20/03/2016";
            input2.Text = "25/03/2016";
            inputFormat.Text = "dd/mm/yyyy";
        }
        public void Enter_Text_Into_DateTime_AddTime_Amount()
        {
            #region Variable Declarations            
            WpfEdit addAmount = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeAmountComboBox.TextEdit;
            #endregion

            addAmount.Text = "4";
        }
        public void Enter_Text_Into_Web_Request_Url()
        {
            #region Variable Declarations
            WpfEdit url = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeViewContentCustom.InputComboBox.TextEdit;
            #endregion

            url.Text = "https://warewolf.atlassian.net/secure/Dashboard.jspa";
        }
        public void Enter_Text_Into_Random_Length()
        {
            #region Variable Declarations
            WpfEdit lenght = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.LengthComboBox.TextEdit;
            #endregion

            lenght.Text = "5";
        }
        public void Enter_Text_Into_Xpath_Tool()
        {
            #region Variable Declarations
            WpfEdit xpath = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.SmallViewContentCustom.SourceStringComboBox.TextEdit;
            #endregion

            xpath.Text = "<Service>";
        }
        public void Enter_Text_On_Comment_Tool()
        {
            #region Variable Declarations
            WpfEdit variable = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit;
            #endregion

            variable.Text = "Hello World";

        }
        public void Enter_Variable_On_System_Information_Tool_Large_View()
        {
            #region Variable Declarations
            WpfEdit variable = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable.Row1.VariableCell.VariableComboBox.TextEdit;
            #endregion

            variable.Text = "[[rec()]]";
        }

        public void Enter_Recordset_On_Delete_tool()
        {
            #region Variable Declarations
            WpfEdit recordset = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.RecordsetComboBox.TextEdit;
            WpfEdit result = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            recordset.Text = "[[rec()]]";
            result.Text = "[[result]]";
        }

        public void Pin_Unpinned_Pane_To_Default_Position()
        {
            Mouse.StartDragging(MainStudioWindow.UnpinnedTab, new Point(5, 5));
            Mouse.StopDragging(MainStudioWindow.UnpinnedTab);
        }

        public void Unpin_Tab_With_Drag(UITestControl Tab)
        {
            Mouse.StartDragging(Tab);
            Mouse.StopDragging(25, 40);
        }

        public void Enter_Recordset_On_Length_tool()
        {
            #region Variable Declarations
            WpfEdit inFields = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.RecordsetComboBox.TextEdit;
            WpfEdit result = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            inFields.Text = "[[rec()]]";
            result.Text = "[[result]]";
        }

        public void Enter_Recordset_On_SortRecorsds_tool()
        {
            #region Variable Declarations
            WpfEdit sortField = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords.SmallViewContentCustom.SortFieldComboBox.TextEdit;
            #endregion

            sortField.Text = "[[rec().a]]";
        }
        public void Enter_Recordset_On_UniqueRecorsds_tool()
        {
            #region Variable Declarations
            WpfEdit inFields = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.InFieldsComboBox.TextEdit;
            WpfEdit returnFields = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ReturnFieldsComboBox.TextEdit;
            WpfEdit result = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ResultsComboBox.TextEdit;
            #endregion

            inFields.Text = "[[rec().a]],[[rec().b]],[[rec().c]]";
            returnFields.Text = "[[rec().b]]";
            result.Text = "[[rec().c]]";

        }
        public void Enter_Recordset_On_CountRecordset_tool()
        {
            #region Variable Declarations
            WpfEdit recordset = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.RecorsetComboBox.TextEdit;
            WpfEdit result = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            recordset.Text = "[[Recordset()]]";
            result.Text = "[[Result]]";
        }
        /// <summary>
        /// Enter_Person_Age_On_Assign_Object_tool - Use 'Enter_Person_Age_On_Assign_Object_toolParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Person_Age_On_Assign_Object_tool()
        {
            string FieldCellValue = "[[@Person.Age]]";
            string FieldValueCellValue = "10";
            #region Variable Declarations
            WpfEdit fieldCell = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldCell.FieldNameComboBox.TextEdit;
            WpfEdit fieldValueCell = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldValueCell.FieldValueComboBox.TextEdit;
            #endregion

            // Type '[[@Person.Age]]' in 'Item: Dev2.TO.AssignObjectDTO, Column Display Inde...' cell
            fieldCell.Text = FieldCellValue;

            // Type '10' in 'Item: Dev2.TO.AssignObjectDTO, Column Display Inde...' cell
            fieldValueCell.Text = FieldValueCellValue;
        }

        public void Check_Debug_Input_Dialog_Remember_Inputs_Checkbox()
        {
            MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked = true;
        }

        /// <summary>
        /// Enter_Person_Name_On_Assign_Object_tool - Use 'Enter_Person_Name_On_Assign_Object_toolParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Person_Name_On_Assign_Object_tool()
        {
            string FieldCellValue = "[[@Person.Name]]";
            string FieldValueCellValue = "Bob";
            #region Variable Declarations
            WpfEdit fieldCell = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldCell.FieldNameComboBox.TextEdit;
            WpfEdit fieldValueCell = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldValueCell.FieldValueComboBox.TextEdit;
            #endregion

            // Type '[[@Person.Name]]' in 'Item: Dev2.TO.AssignObjectDTO, Column Display Inde...' cell
            fieldCell.Text = FieldCellValue;

            // Type 'Bob' in 'Item: Dev2.TO.AssignObjectDTO, Column Display Inde...' cell
            fieldValueCell.Text = FieldValueCellValue;
        }

        /// <summary>
        /// Enter_Values_Into_Case_Conversion_Tool - Use 'Enter_Values_Into_Case_Conversion_ToolParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Values_Into_Case_Conversion_Tool()
        {
            string ValueComboBoxEditableItem = "res";

            #region Variable Declarations
            WpfEdit valueComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit;
            #endregion

            // Select 'res' in 'UI__Row1_OutputVariable_AutoID' combo box
            valueComboBox.Text = ValueComboBoxEditableItem;
        }

        /// <summary>
        /// Connect_Assign_to_Next_tool
        /// </summary>
        public void Connect_Assign_to_Next_tool()
        {
            #region Variable Declarations
            WpfCustom uIFlowchartCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            #endregion

            Mouse.Hover(uIFlowchartCustom, new Point(200, 220));
            // Move 'Flowchart' custom control
            //System parameter 'Show window contents while dragging' is not set.This could lead to incorrect recording of drag actions.
            Mouse.StartDragging(uIFlowchartCustom, new Point(300, 220));
            Mouse.StopDragging(uIFlowchartCustom, 0, 44);
        }

        /// <summary>
        /// Enter_Values_Into_Replace_Tool_Large_View - Use 'Enter_Values_Into_Replace_Tool_Large_ViewParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Values_Into_Replace_Tool_Large_View()
        {
            string FindComboBoxEditableItem = "u";
            string InFiledsComboBoxEditableItem = "[[rec().a]]";
            string ReplaceComboBoxEditableItem = "o";
            string ResultComboBoxEditableItem = "res";

            #region Variable Declarations
            WpfEdit inFiledsComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.InFiledsComboBox.TextEdit;
            WpfEdit findComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.FindComboBox.TextEdit;
            WpfEdit replaceComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ReplaceComboBox.TextEdit;
            WpfEdit resultComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            // Select '[[rec().a]]' in 'UI__InFiledstxt_AutoID' combo box
            inFiledsComboBox.Text = InFiledsComboBoxEditableItem;

            // Select 'u' in 'UI__Findtxt_AutoID' combo box
            findComboBox.Text = FindComboBoxEditableItem;

            // Select 'o' in 'UI__Replacetxt_AutoID' combo box
            replaceComboBox.Text = ReplaceComboBoxEditableItem;

            // Select 'res' in 'UI__Resulttxt_AutoID' combo box
            resultComboBox.Text = ResultComboBoxEditableItem;
        }

        /// <summary>
        /// Enter_Values_Into_FindIndex_Tool - Use 'Enter_Values_Into_FindIndex_ToolParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Values_Into_FindIndex_Tool()
        {

            string InFieldComboBoxEditableItem = "SomeLongString";
            string CharactersComboBoxEditableItem = "r";
            string CharactersComboBoxSendKeys = "{Escape}";
            string ResultComboBoxEditableItem = "res";

            #region Variable Declarations
            WpfEdit inFieldComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.InFieldComboBox.TextEdit;
            WpfComboBox indexComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox;
            WpfEdit charactersComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit;
            WpfEdit resultComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.ResultComboBox.TextEdit;
            #endregion

            // Select 'SomeLongString' in 'UI__InFieldtxt_AutoID' combo box
            inFieldComboBox.Text = InFieldComboBoxEditableItem;

            // Click 'UI__Indextcbx_AutoID' combo box
            Mouse.Click(indexComboBox, new Point(85, 13));

            // Click 'UI__Indextcbx_AutoID' combo box
            Mouse.Click(indexComboBox, new Point(62, 19));

            // Select 'r' in 'UI__Characterstxt_AutoID' combo box
            charactersComboBox.Text = CharactersComboBoxEditableItem;

            // Click 'UI__Characterstxt_AutoID' combo box
            Mouse.Click(charactersComboBox, new Point(45, 2));

            // Click 'UI__Characterstxt_AutoID' combo box
            Mouse.Click(charactersComboBox, new Point(39, 12));

            // Type '{Escape}' in 'UI__Characterstxt_AutoID' combo box
            Keyboard.SendKeys(charactersComboBox, CharactersComboBoxSendKeys, ModifierKeys.None);

            // Select 'res' in 'UI__Resulttxt_AutoID' combo box
            resultComboBox.Text = ResultComboBoxEditableItem;
        }

        public void Enter_Text_Into_Copy_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var resourcesFolderCopy = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = resourcesFolderCopy;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Delete_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            results.Text = "[[results]]";
        }   
        public void Enter_Text_Into_Write_Tool()
        {
            var file = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Test File.txt";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.FileNameComboBox.TextEdit;
            WpfEdit contents = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox.TextEdit;
            WpfRadioButton overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = file;
            contents.Text = "Some Content";
            overwrite.Selected = true;
            results.Text = "[[results]]";
        }     
        public void Enter_Text_Into_Move_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfCheckBox overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = destinationFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_Zip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Zip";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipNameComboBox.TextEdit;
            WpfCheckBox overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = destinationFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_UnZip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var unZipFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_UnZip";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox.TextEdit;
            WpfEdit destination = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfCheckBox overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = unZipFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_Rename_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.NewNameComboBox.TextEdit;
            WpfCheckBox overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = "Acceptance Tests_New";
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_ReadFolder_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.TextEdit;
            WpfRadioButton filesAndFolders = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            filesAndFolders.Selected = true;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_Read_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var file = resourcesFolder + @"\" + "Hello World" + ".xml";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox.TextEdit;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = file;
            results.Text = "[[results]]";
        }
        public void Enter_Text_Into_Create_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Create";

            WpfEdit fileOrFolder = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.FileNameoComboBox.TextEdit;
            WpfCheckBox Overwrite = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            Overwrite.Checked = true;
            results.Text = "[[results]]";
        }
        /// <summary>
        /// Enter_Values_Into_Data_Split_Tool - Use 'Enter_Values_Into_Data_Split_Tool_Large_ViewParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Values_Into_Data_Split_Tool()
        {
            string SourceStringComboBoxEditableItem = "some long string here";
            string ValueCellValue = "[[res]]";
            string AtIndexCellValue = "5";

            #region Variable Declarations
            WpfEdit sourceStringComboBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SourceStringComboBox.TextEdit;
            WpfEdit value = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit;
            WpfEdit atIndex = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.AtIndexCell.AtIndexComboBox.TextEdit;
            #endregion

            // Select 'some long string here' in 'UI__SourceStringtxt_AutoID' combo box
            sourceStringComboBox.Text = SourceStringComboBoxEditableItem;

            // Type '[[res]]' in 'Item: Unlimited.Applications.BusinessDesignStudio....' cell
            value.Text = ValueCellValue;

            // Type '5' in 'Item: Unlimited.Applications.BusinessDesignStudio....' cell
            atIndex.Text = AtIndexCellValue;
        }

        public void Drag_Toolbox_ASwitch_Onto_Foreach_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem switchTool = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch;
            WpfCustom forEach = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom;
            #endregion

            searchTextBox.Text = "Switch";

            forEach.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(switchTool, new Point(13, 17));
            Mouse.StopDragging(forEach);
        }
        
        public void Drag_Toolbox_Decision_Onto_Foreach_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem decision = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision;
            WpfCustom dropActivityHereCustom = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom;
            #endregion
            
            searchTextBox.Text = "Decision";
            
            dropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(decision, new Point(13, 17));
            Mouse.StopDragging(dropActivityHereCustom);
        }

        [When(@"I Select New Sharepoint Server Source")]
        public void WhenISelectNewSharepointServerSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server.NewSharePointSource);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.Exists);
        }

        [When(@"I Click Close Sharepoint Server Source Tab")]
        public void WhenIClickCloseSharepointServerSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointSourceTabCloseButton);
        }


        [When(@"I Click UserButton OnSharepointSource")]
        public void WhenIClickUserButtonOnSharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Exists);
        }


        [When(@"I drag a ""(.*)"" tool")]
        public void WhenIDragATool(string p0)
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem copyFile = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CopyFile;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom sharepointTool = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile;
            #endregion

            flowchart.DrawHighlight();
            // Type 'Assign' in 'SearchTextBox' text box
            searchTextBox.Text = p0;
            Playback.Wait(1500);
            // Move 'Warewolf.Studio.ViewModels.ToolBox.ToolDescriptorV...' list item to 'Flowchart' custom control
            Mouse.StartDragging(copyFile, new Point(2, 10));
            Mouse.StopDragging(flowchart, new Point(307, 126));
            
            Assert.IsTrue(sharepointTool.Exists, "Sharepoint tool does not exist on the Design Surface");
        }
        [When(@"I Click UserButton On Database Source")]
        public void WhenIClickUserButtonOnDatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton);
        }

        [When(@"I Enter RunAsUser Username And Password on Database source")]
        public void WhenIEnterRunAsUserUsernameAndPasswordOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.Text = "testuser";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [When(@"I Change Selected Database ToMySql DataBase")]
        public void WhenIChangeSelectedDatabaseToMySqlDataBase()
        {
            Change_Selected_Database_ToMySql_DataBase();
        }

        [When(@"I Change Selected Database ToPostgreSql DataBase")]
        public void WhenIChangeSelectedDatabaseToPostgreSqlDataBase()
        {
            Change_Selected_Database_ToPostgreSql_DataBase();
        }

        [When(@"I Change Selected Database ToOracle DataBase")]
        public void WhenIChangeSelectedDatabaseToOracleDataBase()
        {
            Change_Selected_Database_ToOracle_DataBase();
        }

        [When(@"I Change Selected Database ToODBC DataBase")]
        public void WhenIChangeSelectedDatabaseToODBCDataBase()
        {
            Change_Selected_Database_ToODBC_DataBase();
        }

        [When(@"I Click DotNet DLL Large View Test Cancel Done Button")]
        public void WhenIClickDotNetDLLLargeViewTestCancelDoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CancelButton);
        }

        public void Drag_Toolbox_AssignObject_Onto_Foreach_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem assignObject = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject;
            WpfCustom dropActivityHereCustom = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom;
            #endregion
                    
            searchTextBox.Text = this.Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams.SearchTextBoxText;
            
            dropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(assignObject, new Point(13, 17));
            Mouse.StopDragging(dropActivityHereCustom);
        }

        public void Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem assignObject = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity;
            #endregion
            
            searchTextBox.Text = this.Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams.SearchTextBoxText;

            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(assignObject, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }
        
        public void Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem assignObject = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence;
            #endregion
            
            searchTextBox.Text = this.Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams.SearchTextBoxText;
            
            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(assignObject, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }
        
        public void Drag_Toolbox_Decision_Onto_Sequence_SmallTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem decision = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence;
            #endregion

            searchTextBox.Text = "Decision";

            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(decision, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }
               
        public void Drag_Toolbox_Switch_Onto_Sequence_SmallTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem switchTool = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence;
            #endregion

            searchTextBox.Text = "Switch";
        
            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(switchTool, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }
      
        public void Drag_Toolbox_Switch_Onto_Sequence_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem switchTool = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity;
            #endregion

            searchTextBox.Text = "Switch";
      
            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(switchTool, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }
     
        public void Drag_Toolbox_Decision_Onto_Sequence_LargeTool()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem decision = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision;
            WpfCustom sequence = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity;
            #endregion

            searchTextBox.Text = "Decision";
            
            sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(decision, new Point(13, 17));
            Mouse.StopDragging(sequence);
        }

        public virtual Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams
        {
            get
            {
                if ((this.mDrag_Toolbox_AssignObject_Onto_Sequence_ToolParams == null))
                {
                    this.mDrag_Toolbox_AssignObject_Onto_Sequence_ToolParams = new Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams();
                }
                return this.mDrag_Toolbox_AssignObject_Onto_Sequence_ToolParams;
            }
        }

        private Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams mDrag_Toolbox_AssignObject_Onto_Sequence_ToolParams;

        [Then("Deploy Was Successful")]
        public void Assert_Deploy_Was_Successful()
        {
            Assert.AreEqual("Resource(s) Deployed Successfully", MessageBoxWindow.ResourcesDeployedSucText.DisplayText
                , "Deploy message text does not equal 'Resource Deployed Successfully'.");
            Click_MessageBox_OK();
        }

        [Then("Dice Is Selected InSettings Tab Permissions Row 1")]
        [When(@"I Assert Dice Is Selected InSettings Tab Permissions Row1")]
        public void Assert_Dice_Is_Selected_InSettings_Tab_Permissions_Row_1()
        {
            Assert.AreEqual("Dice1", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ResourceCell.AddResourceText.DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
        }

        /// <summary>
        /// Add_Dotnet_Dll_Source - Use 'Add_Dotnet_Dll_SourceParams' to pass parameters into this method.
        /// </summary>
        public void Add_Dotnet_Dll_Source(string sourceName)
        {
            #region Variable Declarations
            WpfButton newSourcButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourcButton;
            WpfEdit searchTextBoxEdit = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.DotNetPlugInSourceViewModelsCustom.SearchTextBoxEdit;
            WpfCheckBox expansionIndicatorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.GACTreeItem.ExpansionIndicatorCheckBox;
            WpfTreeItem firstTreeItem = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.GACTreeItem.FirstTreeItem;
            #endregion

            // Click 'New' button
            Mouse.Click(newSourcButton, new Point(30, 4));

            // Type 'CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=AMD64' in 'SearchTextBox' text box
            searchTextBoxEdit.Text = "CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=AMD64";

            // Click 'ExpansionIndicator' check box
            Mouse.Click(expansionIndicatorCheckBox, new Point(30, 4));

            // Click 'GAC' -> 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item
            Mouse.Click(firstTreeItem);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Save_With_Ribbon_Button_And_Dialog(sourceName, true);
            Click_Close_DotNetDll_Tab();
        }

        /// <summary>
        /// Enter_Dice_Roll_Values - Use 'Enter_Dice_Roll_ValuesParams' to pass parameters into this method.
        /// </summary>
        public void Enter_Dice_Roll_Values()
        {
            #region Variable Declarations
            WpfEdit fromTextEdit = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.From.FromTextEdit;
            WpfEdit toTextEdit = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.To.ToTextEdit;
            WpfEdit resultTextEdit = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.ResultComboBox.TextEdit;
            #endregion

            // Verify that the 'Exists' property of 'Text' text box equals 'True'
            Assert.AreEqual(this.Enter_Dice_Roll_ValuesParams.FromTextEditExists, fromTextEdit.Exists, "From textbox does not exist");

            // Type '1' in 'Text' text box
            fromTextEdit.Text = this.Enter_Dice_Roll_ValuesParams.FromTextEditText;

            // Verify that the 'Exists' property of 'Text' text box equals 'True'
            Assert.AreEqual(this.Enter_Dice_Roll_ValuesParams.ToTextEditExists, toTextEdit.Exists, "To textbox does not exist");

            // Type '6' in 'Text' text box
            toTextEdit.Text = this.Enter_Dice_Roll_ValuesParams.ToTextEditText;

            resultTextEdit.Text = "[[out]]";
        }

        public virtual Enter_Dice_Roll_ValuesParams Enter_Dice_Roll_ValuesParams
        {
            get
            {
                if ((this.mEnter_Dice_Roll_ValuesParams == null))
                {
                    this.mEnter_Dice_Roll_ValuesParams = new Enter_Dice_Roll_ValuesParams();
                }
                return this.mEnter_Dice_Roll_ValuesParams;
            }
        }

        private Enter_Dice_Roll_ValuesParams mEnter_Dice_Roll_ValuesParams;

        /// <summary>
        /// Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface - Use 'Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams' to pass parameters into this method.
        /// </summary>
        public void Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem multiAssign = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign;
            WpfCustom flowchart = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom multiAssign1 = this.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            #endregion

            // Type 'Assign' in 'SearchTextBox' text box
            searchTextBox.Text = this.Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams.SearchTextBoxText;
            Playback.Wait(1500);
            // Move 'Warewolf.Studio.ViewModels.ToolBox.ToolDescriptorV...' list item to 'Flowchart' custom control
            Mouse.StartDragging(multiAssign, new Point(2, 10));
            Mouse.StopDragging(flowchart, new Point(307, 126));

            // Verify that the 'Exists' property of 'DsfMultiAssignActivity' custom control equals 'True'
            Assert.AreEqual(this.Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams.MultiAssignExists, multiAssign1.Exists, "MultiAssign does not exist on unpinned tab design surface after dragging from too" +
                    "lbox.");
            multiAssign1.DrawHighlight();
        }

        public virtual Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams
        {
            get
            {
                if ((this.mDrag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams == null))
                {
                    this.mDrag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams = new Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams();
                }
                return this.mDrag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams;
            }
        }

        private Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams mDrag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams;
    }
    /// <summary>
    /// Parameters to be passed into 'Drag_Toolbox_AssignObject_Onto_Sequence_Tool'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_AssignObject_Onto_Sequence_ToolParams
    {

        #region Fields
        /// <summary>
        /// Type 'Assign Object' in 'SearchTextBox' text box
        /// </summary>
        public string SearchTextBoxText = "Assign Object";
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'Add_Dotnet_Dll_Source'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Add_Dotnet_Dll_SourceParams
    {

        #region Fields
        /// <summary>
        /// Type 'CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=AMD64' in 'SearchTextBox' text box
        /// </summary>
        public string SearchTextBoxEditText = "CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a" +
            "3a, processorArchitecture=AMD64";
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'Enter_Dice_Roll_Values'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Dice_Roll_ValuesParams
    {

        #region Fields
        /// <summary>
        /// Verify that the 'Exists' property of 'Text' text box equals 'True'
        /// </summary>
        public bool FromTextEditExists = true;

        /// <summary>
        /// Type '1' in 'Text' text box
        /// </summary>
        public string FromTextEditText = "1";

        /// <summary>
        /// Verify that the 'Exists' property of 'Text' text box equals 'True'
        /// </summary>
        public bool ToTextEditExists = true;

        /// <summary>
        /// Type '6' in 'Text' text box
        /// </summary>
        public string ToTextEditText = "6";
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurfaceParams
    {

        #region Fields
        /// <summary>
        /// Type 'Assign' in 'SearchTextBox' text box
        /// </summary>
        public string SearchTextBoxText = "Assign";

        /// <summary>
        /// Verify that the 'Exists' property of 'DsfMultiAssignActivity' custom control equals 'True'
        /// </summary>
        public bool MultiAssignExists = true;
        #endregion
    }

}
