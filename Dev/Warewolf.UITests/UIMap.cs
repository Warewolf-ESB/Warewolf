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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    [Binding]
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
            Mouse.MouseDragSpeed = 350;
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
            TryCloseHangingWebBrowserErrorDialog();
            TryCloseHangingDecisionDialog();
            TryCloseSettingsTab();
            TryCloseWorkflowTestingTab();
            var TimeBefore = System.DateTime.Now;
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Console.WriteLine("Waited " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms for the explorer spinner to disappear.");
#endif
        }

        [When(@"I Try Click Message Box OK")]
        public void TryClickMessageBoxOK()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MessageBoxWindow.OKButton))
                {
                    Click_MessageBox_OK();
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

        public void TryCloseHangingDebugInputDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(MainStudioWindow.DebugInputDialog))
                {
                    Click_DebugInput_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Debug Input dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Debug Input dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingSaveDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(SaveDialogWindow.CancelButton))
                {
                    Click_SaveDialog_CancelButton();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Save dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Save dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
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

        private void TryCloseHangingServicePickerDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(ServicePickerDialog.Cancel))
                {
                    Click_Service_Picker_Dialog_Cancel();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Service Picker dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Service Picker dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingWindowsGroupDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(SelectWindowsGroupDialog))
                {
                    Click_Select_Windows_Group_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Windows Group dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Windows Group dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingErrorDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(ErrorWindow))
                {
                    Click_Close_Error_Dialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Error dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Error dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingCriticalErrorDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(CriticalErrorWindow))
                {
                    Click_Close_Critical_Error_Dialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Critical Error dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Critical Error dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingWebBrowserErrorDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(WebBrowserErrorWindow))
                {
                    Click_Web_Browser_Error_Messagebox_OK_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Web Browser Error dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging Web Browser Error dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void TryCloseHangingDecisionDialog()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (ControlExistsNow(DecisionOrSwitchDialog))
                {
                    Click_Decision_Dialog_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging decision dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging decision dialog to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        bool OnErrorHandlerDisabled = false;
        public void OnError(object sender, PlaybackErrorEventArgs e)
        {
            if (OnErrorHandlerDisabled) return;
            e.Result = PlaybackErrorOptions.Retry;
            var type = e.Error.GetType().ToString();
            var messageText = type + "\n" + e.Error.Message;
            switch (type)
            {
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotFoundException":
                    UITestControlNotFoundExceptionHandler(type, messageText, e.Error as UITestControlNotFoundException);
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.UITestControlNotAvailableException":
                    UITestControlNotAvailableExceptionHandler(type, messageText, e.Error as UITestControlNotAvailableException);
                    break;
                case "Microsoft.VisualStudio.TestTools.UITest.Extension.FailedToPerformActionOnBlockedControlException":
                    FailedToPerformActionOnBlockedControlExceptionHandler(type, messageText, e.Error as FailedToPerformActionOnBlockedControlException);
                    break;
                default:
                    Console.WriteLine(messageText);
                    break;

            }
#if DEBUG
            throw e.Error;
#endif
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
                    var messageText = message + "\n" + "Search actually failed at: " + parent.FriendlyName + "\n" + parentProperties;
                    Console.WriteLine(messageText);
                    parent.DrawHighlight();
                }
            }
        }

        void UITestControlNotAvailableExceptionHandler(string type, string message, UITestControlNotAvailableException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
                Console.WriteLine(message);
                (exceptionSource as UITestControl).DrawHighlight();
            }
        }

        void FailedToPerformActionOnBlockedControlExceptionHandler(string type, string message, FailedToPerformActionOnBlockedControlException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
                Console.WriteLine(message);
                (exceptionSource as UITestControl).DrawHighlight();
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

        [When(@"I Click Assign tool VariableTextbox")]
        public void Click_Assign_tool_VariableTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox);
        }

        [When(@"I Click Assign tool ValueTextbox")]
        public void Click_Assign_tool_ValueTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox);
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
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem commentToolboxItem = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector3 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3;
            WpfCustom commentOnTheDesignSurface = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion

            var switchRightAutoConnector = new Point(360, 200);
            flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(commentToolboxItem, new Point(16, 25));
            Mouse.StopDragging(flowchart, switchRightAutoConnector);
            Assert.IsTrue(DecisionOrSwitchDialog.Exists);
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(34, 10));
            Assert.IsTrue(connector3.Exists, "Third connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(commentOnTheDesignSurface.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        [Then(@"The Case Dialog Must Be Open")]
        public void ThenTheCaseDialogMustBeOpen()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch);
            Assert.IsTrue(DecisionOrSwitchDialog.Exists, "Switch case dialog does not exist after dragging onto switch case arm.");
            Mouse.Click(DecisionOrSwitchDialog.DoneButton);
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

        public void Click_Settings_Resource_Permissions_Row1_Delete_Button()
        {
            Mouse.Click(FindAddRemoveRowButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
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

        public void TryDisconnectFromRemoteServerAndRemoveSourceFromExplorer(string SourceName)
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected))
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

        [Given(@"I Try Remove ""(.*)"" From Explorer")]
        [When(@"I Try Remove ""(.*)"" From Explorer")]
        [Then(@"I Try Remove ""(.*)"" From Explorer")]
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

        public void TryCloseAllWorkflowTabs()
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

        [Given(@"I Try Close Workflow")]
        [When(@"I Try Close Workflow")]
        [Then(@"I Try Close Workflow")]
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

        [When(@"I Wait For Explorer Localhost Spinner")]
        public void WaitForExplorerLocalhostSpinner()
        {
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        [When(@"I Wait For Explorer First Remote Server Spinner")]
        public void WaitForExplorerFirstRemoteServerSpinner()
        {
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
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

        [Given(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, false, false, false, SaveOrDuplicate.Save);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName, bool duplicate = false, bool invalid = false, bool nameHasWhiteSpace = false, SaveOrDuplicate saveOrDuplicate = SaveOrDuplicate.Save)
        {

            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
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

        [Given(@"I Double Click Resource On The Service Picker")]
        [When(@"I Double Click Resource On The Service Picker")]
        [Then(@"I Double Click Resource On The Service Picker")]
        public void DoubleClick_FirstItem_From_ServicePicker_Tree()
        {
            var firstItem = ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1;
            Mouse.DoubleClick(firstItem);
        }

        [Given(@"I Double Click Resource On The Save Dialog")]
        [When(@"I Double Click Resource On The Save Dialog")]
        [Then(@"I Double Click Resource On The Save Dialog")]
        public void ThenIDoubleClickResourceOnTheSaveDialog()
        {
            Mouse.DoubleClick(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem);
        }

        public void Filter_ServicePicker_Explorer(string FilterText)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = FilterText;
            WaitForControlVisible(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Click Localhost")]
        [Then(@"I Click Localhost")]
        [Given(@"I Click Localhost")]
        public void Click_LocalHost_Once()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost);
        }

        [Given(@"I Filter the Explorer with ""(.*)""")]
        [When(@"I Filter the Explorer with ""(.*)""")]
        [Then(@"I Filter the Explorer with ""(.*)""")]
        public void Filter_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
        }

        [Given(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [When(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [Then(@"I Filter Save Dialog Explorer with ""(.*)""")]
        public void Filter_Save_Dialog_Explorer(string FilterText)
        {
            SaveDialogWindow.ExplorerView.SearchTextBox.Text = FilterText;
        }

        [When(@"I Move FirstSubItem Into FirstItem Folder")]
        public void Move_FirstSubItem_Into_FirstItem_Folder()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Then(@"I have one item in the explorer")]
        public void ExplorerItemCountEquals()
        {
            var secondItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem;
            Assert.IsFalse(ControlExistsNow(secondItem), "Second Item exists in the Explorer Exists");
        }

        [When(@"I Filter the ToolBox with ""(.*)""")]
        public void Filter_ToolBox(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = FilterText;
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
            if (ControlExistsNow(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1.TreeItem11))
            {
                Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1.TreeItem11);
            }
            else
            {
                Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            }
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... does not exist in explorer remote server drop down list");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsNewRemoteServer.NewRemoteServerItemText, new Point(114, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Exists, "Server source wizard does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown.Exists, "Server source wizard protocol dropdown does not exist.");
        }

        public void Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "localhost (connected) does not exist in explorer remote server drop down list");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(94, 10));
            Assert.AreEqual("localhost", MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsLocalhost.DisplayText, "Selected remote server is not localhost");
        }

        public void Select_localhost_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(174, 8));
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Text);
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
            Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            WaitForSpinner(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Enter_Service_Name_Into_Save_Dialog(Name);
            Click_SaveDialog_Save_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }


        [Given(@"I Click SaveDialog Save Button")]
        [When(@"I Click SaveDialog Save Button")]
        [Then(@"I Click SaveDialog Save Button")]
        public void Click_SaveDialog_Save_Button()
        {
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
            Playback.Wait(1000);
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow.SaveButton), "Save dialog still exists after clicking save button.");
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        public void TryCloseNewDotNetPluginSourceWizardTab()
        {
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.CloseButton))
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
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable]]\".");
        }

        [Given("I Click New Workflow Ribbon Button")]
        [When("I Click New Workflow Ribbon Button")]
        [Then("I Click New Workflow Ribbon Button")]
        public void Click_New_Workflow_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.NewWorkflowButton, new Point(3, 8));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after clicking new workflow ribbon button.");
        }

        [When(@"I Select Test Source From GET Web Large View Source Combobox")]
        public void Select_Test_Source_From_GET_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem6, new Point(163, 17));
        }

        [When(@"I Select Test Source From POST Web Large View Source Combobox")]
        public void Select_Test_Source_From_POST_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem10, new Point(163, 17));
        }

        [When(@"I Select Test Source From DELETE Web Large View Source Combobox")]
        public void Select_Test_Source_From_DELETE_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem1, new Point(163, 17));
        }

        [When(@"I Select Test Source From PUT Web Large View Source Combobox")]
        public void Select_Test_Source_From_PUT_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.WebServerSourceComboboxListItem11, new Point(163, 17));
        }

        [Given(@"I Click New Web Source Ribbon Button")]
        [When(@"I Click New Web Source Ribbon Button")]
        [Then(@"I Click New Web Source Ribbon Button")]
        public void Click_New_Web_Source_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.WebSourceButton, new Point(13, 18));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Web server address textbox does not exist on new web source wizard tab.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Web server test connection button does not exist on new web source wizard tab.");
        }

        [Given(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        [When(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        [Then(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
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

        [Given(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        [When(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        [Then(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        public void Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem commentToolboxItem = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector3 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3;
            WpfCustom commentOnTheDesignSurface = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion

            var switchRightAutoConnector = new Point(360, 200);
            flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(commentToolboxItem, new Point(16, 25));
            Mouse.StopDragging(flowchart, switchRightAutoConnector);
            Assert.IsTrue(DecisionOrSwitchDialog.DoneButton.Exists, "Switch case dialog done button does not exist after dragging onto switch case arm.");
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(34, 10));
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

        [Given(@"I Click Debug Ribbon Button")]
        [When(@"I Click Debug Ribbon Button")]
        [Then(@"I Click Debug Ribbon Button")]
        public void Click_Debug_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.RunAndDebugButton, new Point(13, 14));
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input window does not exist after clicking debug ribbon button.");
        }

        [Given(@"I Type ""(.*)"" into Plugin Source Wizard Assembly Textbox")]
        [When(@"I Type ""(.*)"" into Plugin Source Wizard Assembly Textbox")]
        [Then(@"I Type ""(.*)"" into Plugin Source Wizard Assembly Textbox")]
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Text = text;
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after DLL has been selected in plugin source wizard.");
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = GroupName;
            Assert.AreEqual(FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
        }

        [Given(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [When(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
        [Then(@"I Set Resource Permissions For ""(.*)"" to Group ""(.*)"" and Permissions for View to ""(.*)"" and Contribute to ""(.*)"" and Execute to ""(.*)""")]
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = true;
        }

        public void Click_Deploy_Tab_Deploy_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton);
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.Exists, "settings tab does not exist after clicking settings ribbon button.");
        }

        [When(@"I Click Deploy Ribbon Button")]
        public void Click_Deploy_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.DeployButton.Exists, "Deploy ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.DeployButton, new Point(16, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "Deploy tab does not exist after clicking deploy ribbon button.");
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

        [Given("I Drag Dice Roll Example Onto DesignSurface")]
        [When("I Drag Dice Roll Example Onto DesignSurface")]
        [Then("I Drag Dice Roll Example Onto DesignSurface")]
        public void Drag_Dice_Roll_Example_Onto_DesignSurface()
        {
            Filter_Explorer("Dice Roll");
            Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
        }

        [Given(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        [When(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        [Then(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        public void Select_Show_Dependencies_In_Explorer_Context_Menu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ShowDependencies);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.Exists, "Dependency graph tab is not showen after clicking show dependancies explorer content menu item.");
        }

        [Given(@"I Click DB Source Wizard Test Connection Button")]
        [When(@"I Click DB Source Wizard Test Connection Button")]
        [Then(@"I Click DB Source Wizard Test Connection Button")]
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            var point = new Point();
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox is visible.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox is not visible.");
        }

        [Given(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [When(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        [Then(@"The DB Source Wizard Test Succeeded Image Is Visible")]
        public void Assert_The_DB_Source_Wizard_Test_Succeeded_Image_Is_Visible()
        {
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ConnectionPassedImage.TryGetClickablePoint(out point), "New DB source wizard test succeeded image is not visible after testing with RSAKLFSVRGENDEV and waiting for the spinner.");
        }

        [Given(@"I Deploy ""(.*)"" From Deploy View")]
        [When(@"I Deploy ""(.*)"" From Deploy View")]
        [Then(@"I Deploy ""(.*)"" From Deploy View")]
        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            TryClickMessageBoxOK();
            Enter_DeployViewOnly_Into_Deploy_Source_Filter(ServiceName);
            TryClickMessageBoxOK();
            Select_Deploy_First_Source_Item();
            TryClickMessageBoxOK();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled,
                "Deploy button is not enabled after valid server and resource are selected.");
            Click_Deploy_Tab_Deploy_Button();
            TryClickMessageBoxOK();
            TryClickMessageBoxOK();
            TryClickMessageBoxOK();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Spinner);
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
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox;

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

        public void Scroll_Down_Then_Up_On_The_DataMerge_SmallView()
        {
            Mouse.MoveScrollWheel(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.UISmallDataGridTable, -1);
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

        [Given(@"I Update Test Name To ""(.*)""")]
        [When(@"I Update Test Name To ""(.*)""")]
        [Then(@"I Update Test Name To ""(.*)""")]
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));

            var currentTest = GetCurrentTest(testInstance);
            var testEnabledSelector = GetTestRunState(testInstance, currentTest).Checked;
            var testNeverRun = GetSelectedTestNeverRunDisplay(currentTest, testInstance);

            Assert.AreEqual("Never run", testNeverRun.DisplayText);
            AssertTestResults(TestResultEnum.Pending, testInstance, currentTest);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameText.Exists, string.Format("Test{0} Name textbox does not exist after clicking Create New Test", testInstance));
            Assert.IsTrue(testEnabledSelector, string.Format("Test {0} is diabled after clicking Create new test from context menu", testInstance));

            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        private void Assert_Display_Text_ContainStar(string control, bool containsStar, int instance = 1)
        {
            WpfList testsListBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            var test = GetCurrentTest(instance);
            string description = string.Empty;
            if (control == "Tab")
            {
                description = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText;
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


        public void Select_Service_From_Service_Picker(string serviceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = serviceName;
            Mouse.Click(ServicePickerDialog.Explorer.Refresh, new Point(5, 5));
            WaitForSpinner(ServicePickerDialog.Explorer.ExplorerTree.Localhost.Checkbox.Spinner);
            if (ControlExistsNow(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem2.TreeItem1))
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
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2;
                    break;
                case 3:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3;
                    break;
                case 4:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4;
                    break;
                default:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1;
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
            MainStudioWindow.SideMenuBar.SaveButton.WaitForControlCondition(uicontrol => !uicontrol.Enabled, WaitForSave * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
            Assert.IsFalse(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is still enabled after clicking it and waiting for " + WaitForSave + "ms.");
        }

        public void DeleteAssign_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem delete = MainStudioWindow.DesignSurfaceContextMenu.Delete;
            WpfWindow messageBoxWindow = MessageBoxWindow;
            WpfCustom multiAssign = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton.Exists, "New Source Button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "New DB source wizard tab does not exist after clicking the new db source button on Web PUT tool.");
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

        [Given(@"I Click DotNet DLL Large View Generate Outputs")]
        [When(@"I Click DotNet DLL Large View Generate Outputs")]
        [Then(@"I Click DotNet DLL Large View Generate Outputs")]
        public void Click_DotNet_DLL_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.DoneButton.Exists);
        }

        [Given(@"I Click New Web Source Test Connection Button")]
        [When(@"I Click New Web Source Test Connection Button")]
        [Then(@"I Click New Web Source Test Connection Button")]
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

        [Given("I Click Subworkflow Done Button")]
        [When("I Click Subworkflow Done Button")]
        [Then("I Click Subworkflow Done Button")]
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

        public void Create_New_Workflow_In_Explorer_First_Item_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.NewWorkflow, new Point(79, 13));
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

        [Given(@"I Refresh Explorer")]
        [When(@"I Refresh Explorer")]
        [Then(@"I Refresh Explorer")]
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void TryRemoveTests()
        {
            WpfList testsListBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
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

        [Given(@"I Select SharepointTestServer")]
        [When(@"I Select SharepointTestServer")]
        [Then(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer_FromSharepointDelete_tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [Given(@"I Select AcceptanceTestin delete")]
        [When(@"I Select AcceptanceTestin delete")]
        [Then(@"I Select AcceptanceTestin delete")]
        public void Select_AcceptanceTestin_From_DeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_DeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_UpdateTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [Given(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        [When(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        [Then(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        public void Click_View_Tests_In_Explorer_Context_Menu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            Show_Explorer_First_Item_Tests_With_Context_Menu();
        }

        [Given(@"That The First Test ""(.*)"" Unsaved Star")]
        [When(@"The First Test ""(.*)"" Unsaved Star")]
        [Then(@"The First Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        [Given(@"The Added Test ""(.*)"" Unsaved Star")]
        [When(@"The Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Added Test ""(.*)"" Unsaved Star")]
        public void ThenTheAddedTestUnsavedStar(string p0)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(p0 == "Has");
        }

        [Given(@"I delete Second Added Test")]
        [When(@"I delete Second Added Test")]
        [Then(@"I delete Second Added Test")]
        public void ThenIDeleteSecondAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector, new Point(10, 10));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.DeleteButton, new Point(10, 10));
            Click_MessageBox_Yes();
        }

        [Given(@"I delete Added Test")]
        [When(@"I delete Added Test")]
        [Then(@"I delete Added Test")]
        public void ThenIDeleteAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.DeleteButton, new Point(10, 10));
            Click_MessageBox_Yes();
        }

        [Given(@"That The Added Test ""(.*)"" Unsaved Star")]
        [When(@"That The Added Test ""(.*)"" Unsaved Star")]
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
        [When(@"The Second Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second test title does not contain unsaved star.");
        }
        [Given(@"That The Second Added Test ""(.*)"" Unsaved Star")]
        [When(@"The Second Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Added Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second Added test title does not contain unsaved star.");
        }

        [Given(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        [When(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        [Then(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        public void Click_Duplicate_From_ExplorerContextMenu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Duplicate_Explorer_Localhost_First_Item_With_Context_Menu();
        }

        [Given(@"I Click The Create a New Test Button")]
        [When(@"I Click The Create a New Test Button")]
        [Then(@"I Click The Create a New Test Button")]
        public void Click_Workflow_Testing_Tab_Create_New_Test_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        [Given("The First Test Exists")]
        [When("The First Test Exists")]
        [Then("The First Test Exists")]
        public void Assert_Workflow_Testing_Tab_First_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Added Test Exists")]
        [When("The Added Test Exists")]
        [Then("The Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Second Test Exists")]
        [When("The Second Test Exists")]
        [Then("The Second Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "No second test on workflow testing tab.");
        }

        [Given("The Second Added Test Exists")]
        [When("The Second Added Test Exists")]
        [Then("The Second Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.Exists, "No second Added test on workflow testing tab.");
        }

        [Given("I Toggle First Test Enabled")]
        [When("I Toggle First Test Enabled")]
        [Then("I Toggle First Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector, new Point(10, 10));
        }

        [Given("I Toggle First Added Test Enabled")]
        [Then("I Toggle First Added Test Enabled")]
        [When("I Toggle First Added Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Added_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector, new Point(10, 10));
        }

        [Given("I Click Test (.*) Run Button")]
        [When("I Click Test (.*) Run Button")]
        [Then("I Click Test (.*) Run Button")]
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

        [Given("I Click First Test Delete Button")]
        [When("I Click First Test Delete Button")]
        [Then("I Click First Test Delete Button")]
        public void Click_First_Test_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.DeleteButton, new Point(10, 10));
        }

        [Given(@"I Click First Test Run Button")]
        [When(@"I Click First Test Run Button")]
        [Then(@"I Click First Test Run Button")]
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
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointUpdate()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }
        public void Click_Sharepoint_RefreshButton_From_SharepointRead()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        [Given(@"I wait for output spinner")]
        [When(@"I wait for output spinner")]
        [Then(@"I wait for output spinner")]
        public void WhenIWaitForOutputSpinner()
        {
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        [Given("I Click Run All Button")]
        [When("I Click Run All Button")]
        [Then("I Click Run All Button")]
        public void Click_Workflow_Testing_Tab_Run_All_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton, new Point(35, 10));
        }

        [Given(@"I Open Explorer First Item Context Menu")]
        [When(@"I Open Explorer First Item Context Menu")]
        [Then(@"I Open Explorer First Item Context Menu")]
        public void WhenIOpenExplorerFirstItemContextMenu()
        {
            Open_Explorer_First_Item_With_Context_Menu();
        }


        [Given(@"That The First Test ""(.*)"" Passing")]
        [When(@"The First Test ""(.*)"" Passing")]
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
        [When(@"The First Test ""(.*)"" Invalid")]
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

        public void Debug_Unpinned_Workflow_With_F6()
        {
            Press_F6();
            WaitForSpinner(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        public void Remove_Assign_Row_1_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1.");
        }

        public void Remove_Assign_Row_1_With_Context_Menu_On_Unpinned_Tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.DrawHighlight();
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            StartNodePopupWindow.DesignSurfaceMenu.DeleteRowMenuItem.DrawHighlight();
            Mouse.Click(StartNodePopupWindow.DesignSurfaceMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1 on unpinned tab.");
        }

        [Given(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [Then(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
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

        public void Click_Explorer_RemoteServer_Edit_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton, new Point(11, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.Exists, "Server Source Tab was not open.");
        }

        public void Enter_Text_Into_Assign_QviLarge_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviVariableListBoxEdit.Text = "varOne,varTwo,varThree";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviSplitOnCharacterEdit.Text = ",";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PrefixEdit.Text = "some(<).";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.SuffixEdit.Text = "_suf";
        }

        public void Enter_Text_Into_EmailSource_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.HostTextBoxEdit.Text = "localhost";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.UserNameTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PasswordTextBoxEdit.Text = "test";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PortTextBoxEdit.Text = "2";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.FromTextBoxEdit.Text = "AThorLocal@norsegods.com";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.ToTextBoxEdit.Text = "dev2warewolf@gmail.com";
        }

        public void Enter_Text_Into_Exchange_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Text = "https://outlook.office365.com/EWS/Exchange.asmx";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.UserNameTextBox.Text = "Nkosinathi.Sangweni@TheUnlimited.co.za";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.PasswordTextBox.Text = "Password123";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.ToTextBox.Text = "dev2warewolf@gmail.com";
        }

        public void Enter_Number_To_Format()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.NumberInputComboBox.TextEdit.Text = "5.8961";
        }

        public void Enter_Decimals_To_Show()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.DecimalsToShowComboBox.TextEdit.Text = "2";
        }

        public void Enter_Result_Variable_On_Random_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Result_Variable_Into_DateTime()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Result_Variable_Into_DateTimeDifference()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Result_Variable_Into_Web_Request()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Result_Variable()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.ResultInputComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Aggregate_Calculate_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.fxComboBox.TextEdit.Text = "Sum(5,5)";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_DateTime_Input()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.InputComboBox.TextEdit.Text = "20/03/2016";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.InputFormatComboBox.TextEdit.Text = "dd/mm/yyyy";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.OutputFormatComboBox.TextEdit.Text = "yyyy mm";
        }

        public void Enter_Text_Into_DateTimeDiffetence_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.Input1ComboBox.TextEdit.Text = "20/03/2016";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.Input2ComboBox.TextEdit.Text = "25/03/2016";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.InputFormatComboBox.TextEdit.Text = "dd/mm/yyyy";
        }

        public void Enter_Text_Into_DateTime_AddTime_Amount()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeAmountComboBox.TextEdit.Text = "4";
        }

        public void Enter_Text_Into_Web_Request_Url()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeViewContentCustom.InputComboBox.TextEdit.Text = "https://warewolf.atlassian.net/secure/Dashboard.jspa";
        }

        public void Enter_Text_Into_Random_Length()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.LengthComboBox.TextEdit.Text = "5";
        }

        public void Enter_Text_Into_Xpath_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.SmallViewContentCustom.SourceStringComboBox.TextEdit.Text = "<Service>";
        }

        public void Enter_Text_On_Comment_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text = "Hello World";
        }

        public void Enter_Variable_On_System_Information_Tool_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable.Row1.VariableCell.VariableComboBox.TextEdit.Text = "[[rec()]]";
        }

        public void Enter_Recordset_On_Delete_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.RecordsetComboBox.TextEdit.Text = "[[rec()]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[result]]";
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
        }

        public void Enter_Recordset_On_Length_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.RecordsetComboBox.TextEdit.Text = "[[rec()]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[result]]";
        }

        public void Enter_Recordset_On_SortRecorsds_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords.SmallViewContentCustom.SortFieldComboBox.TextEdit.Text = "[[rec().a]]";
        }
        public void Enter_Recordset_On_UniqueRecorsds_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.InFieldsComboBox.TextEdit.Text = "[[rec().a]],[[rec().b]],[[rec().c]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ReturnFieldsComboBox.TextEdit.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ResultsComboBox.TextEdit.Text = "[[rec().c]]";

        }

        public void Enter_Recordset_On_CountRecordset_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.RecorsetComboBox.TextEdit.Text = "[[Recordset()]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[Result]]";
        }

        public void Enter_Person_Age_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Age]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldValueCell.FieldValueComboBox.TextEdit.Text = "10";
        }

        public void Check_Debug_Input_Dialog_Remember_Inputs_Checkbox()
        {
            MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Checked = true;
        }

        public void Enter_Person_Name_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Name]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldValueCell.FieldValueComboBox.TextEdit.Text = "Bob";
        }

        public void Enter_Values_Into_Case_Conversion_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "res";
        }

        public void Connect_Assign_to_Next_tool()
        {
            Mouse.Hover(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(200, 220));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 220));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, 0, 44);
        }

        public void Enter_Values_Into_Replace_Tool_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.InFiledsComboBox.TextEdit.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.FindComboBox.TextEdit.Text = "u";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ReplaceComboBox.TextEdit.Text = "o";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Values_Into_FindIndex_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.InFieldComboBox.TextEdit.Text = "SomeLongString";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(85, 13));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(62, 19));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit.Text = "r";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(45, 2));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(39, 12));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, "{Escape}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Text_Into_Copy_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var resourcesFolderCopy = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = resourcesFolderCopy;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }
        public void Enter_Text_Into_CommentTool(string comment)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text = comment;
        }

        public void Enter_Text_Into_Delete_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Write_Tool()
        {
            var file = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Test File.txt";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.FileNameComboBox.TextEdit.Text = file;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox.TextEdit.Text = "Some Content";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton.Selected = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Move_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = destinationFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Zip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Zip";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipNameComboBox.TextEdit.Text = destinationFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_UnZip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var unZipFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_UnZip";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = unZipFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Rename_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.NewNameComboBox.TextEdit.Text = "Acceptance Tests_New";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_ReadFolder_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton.Selected = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Read_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var file = resourcesFolder + @"\" + "Hello World" + ".xml";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox.TextEdit.Text = file;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Create_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Create";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.FileNameoComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Values_Into_Data_Split_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SourceStringComboBox.TextEdit.Text = "some long string here";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "[[res]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.AtIndexCell.AtIndexComboBox.TextEdit.Text = "5";
        }

        public void Drag_Toolbox_ASwitch_Onto_Foreach_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_Decision_Onto_Foreach_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        [Given(@"I Select New Sharepoint Server Source")]
        [When(@"I Select New Sharepoint Server Source")]
        [Then(@"I Select New Sharepoint Server Source")]
        public void WhenISelectNewSharepointServerSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server.NewSharePointSource);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.Exists);
        }

        [Given(@"I Click Close Sharepoint Server Source Tab")]
        [When(@"I Click Close Sharepoint Server Source Tab")]
        [Then(@"I Click Close Sharepoint Server Source Tab")]
        public void WhenIClickCloseSharepointServerSourceTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointSourceTabCloseButton);
        }

        [Given(@"I Click UserButton OnSharepointSource")]
        [When(@"I Click UserButton OnSharepointSource")]
        [Then(@"I Click UserButton OnSharepointSource")]
        public void WhenIClickUserButtonOnSharepointSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Exists);
        }

        [Given(@"I Click UserButton On Database Source")]
        [When(@"I Click UserButton On Database Source")]
        [Then(@"I Click UserButton On Database Source")]
        public void WhenIClickUserButtonOnDatabaseSource()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton);
        }

        [Given(@"I Enter RunAsUser Username And Password on Database source")]
        [When(@"I Enter RunAsUser Username And Password on Database source")]
        [Then(@"I Enter RunAsUser Username And Password on Database source")]
        public void WhenIEnterRunAsUserUsernameAndPasswordOnDatabaseSource()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.Text = "testuser";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.Text = "test123";
        }

        [Given(@"I Change Selected Database ToMySql DataBase")]
        [When(@"I Change Selected Database ToMySql DataBase")]
        [Then(@"I Change Selected Database ToMySql DataBase")]
        public void WhenIChangeSelectedDatabaseToMySqlDataBase()
        {
            Change_Selected_Database_ToMySql_DataBase();
        }

        [Given(@"I Change Selected Database ToPostgreSql DataBase")]
        [When(@"I Change Selected Database ToPostgreSql DataBase")]
        [Then(@"I Change Selected Database ToPostgreSql DataBase")]
        public void WhenIChangeSelectedDatabaseToPostgreSqlDataBase()
        {
            Change_Selected_Database_ToPostgreSql_DataBase();
        }

        [Given(@"I Change Selected Database ToOracle DataBase")]
        [When(@"I Change Selected Database ToOracle DataBase")]
        [Then(@"I Change Selected Database ToOracle DataBase")]
        public void WhenIChangeSelectedDatabaseToOracleDataBase()
        {
            Change_Selected_Database_ToOracle_DataBase();
        }

        [Given(@"I Change Selected Database ToODBC DataBase")]
        [When(@"I Change Selected Database ToODBC DataBase")]
        [Then(@"I Change Selected Database ToODBC DataBase")]
        public void WhenIChangeSelectedDatabaseToODBCDataBase()
        {
            Change_Selected_Database_ToODBC_DataBase();
        }

        [Given(@"I Click DotNet DLL Large View Test Cancel Done Button")]
        [When(@"I Click DotNet DLL Large View Test Cancel Done Button")]
        [Then(@"I Click DotNet DLL Large View Test Cancel Done Button")]
        public void WhenIClickDotNetDLLLargeViewTestCancelDoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CancelButton);
        }

        public void Drag_Toolbox_AssignObject_Onto_Foreach_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity);
        }

        public void Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Decision_Onto_Sequence_SmallTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Switch_Onto_Sequence_SmallTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Switch_Onto_Sequence_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity);
        }

        public void Drag_Toolbox_Decision_Onto_Sequence_LargeTool()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity);
        }

        [Given("Dice Is Selected InSettings Tab Permissions Row 1")]
        [When(@"I Assert Dice Is Selected InSettings Tab Permissions Row1")]
        [Then("Dice Is Selected InSettings Tab Permissions Row 1")]
        public void Assert_Dice_Is_Selected_InSettings_Tab_Permissions_Row_1()
        {
            Assert.AreEqual("Dice1", FindSelectedResourceText(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).DisplayText, "Resource Name is not set to Dice after selecting Dice from Service picker");
        }

        public void Add_Dotnet_Dll_Source(string sourceName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourcButton, new Point(30, 4));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ContentDockManager.FilterTextbox.Text = "CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=AMD64";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ContentDockManager.ExplorerTree.GACTreeItem.ExpansionIndicatorCheckbox, new Point(30, 4));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ContentDockManager.ExplorerTree.GACTreeItem.FirstTreeItem);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Save_With_Ribbon_Button_And_Dialog(sourceName);
            Click_Close_DotNetDll_Tab();
        }

        public void Enter_Dice_Roll_Values()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.From.FromTextEdit.Exists, "From textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.From.FromTextEdit.Text = "1";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.To.ToTextEdit.Exists, "To textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.To.ToTextEdit.Text = "6";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallView.ResultComboBox.TextEdit.Text = "[[out]]";
        }

        public void Drag_Toolbox_MultiAssign_Onto_Unpinned_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            Playback.Wait(1500);
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(2, 10));
            Mouse.StopDragging(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 126));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "MultiAssign does not exist on unpinned tab design surface after dragging from toolbox.");
        }

        public void Toggle_Between_Studio_and_Unpinned_Tab()
        {
            Keyboard.SendKeys(MainStudioWindow, "{ALT}{TAB}");
            Playback.Wait(100);
            Point point;
            Assert.IsFalse(MainStudioWindow.UnpinnedTab.TryGetClickablePoint(out point), "Unpinned pane still visible after Alt+TAB");
        }

        public void Show_Explorer_First_Item_Tests_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Tests, new Point(30, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after openning it by clicking the explorer context menu item.");
        }

        public void Debug_Using_Play_Icon()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
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

        [Given(@"I Click AddNew Web Source From PostWeb tool")]
        [When(@"I Click AddNew Web Source From PostWeb tool")]
        [Then(@"I Click AddNew Web Source From PostWeb tool")]
        public void Click_AddNew_Web_Source_From_PostWeb_tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton.Exists, "NewButton does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton, new Point(30, 4));
        }

        [Given(@"I Click AddNew Web Source From tool")]
        [When(@"I Click AddNew Web Source From tool")]
        [Then(@"I Click AddNew Web Source From tool")]
        public void Click_AddNew_Web_Source_From_tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton.Exists, "NewButton does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "New DB source wizard tab does not exist after clicking the new db source button on Web GET tool.");
        }

        [Given(@"I Click Assign Tool CollapseAll")]
        [When(@"I Click Assign Tool CollapseAll")]
        [Then(@"I Click Assign Tool CollapseAll")]
        public void Click_Assign_Tool_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [Given(@"I Click Assign Tool ExpandAll")]
        [When(@"I Click Assign Tool ExpandAll")]
        [Then(@"I Click Assign Tool ExpandAll")]
        public void Click_Assign_Tool_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [Given(@"I Click Assign Tool Large View Done Button")]
        [When(@"I Click Assign Tool Large View Done Button")]
        [Then(@"I Click Assign Tool Large View Done Button")]
        public void Click_Assign_Tool_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view.");
        }

        [Given(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        [When(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        [Then(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.AreEqual("SomeVariable", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text does not equal somevariable after using that variable on a unpinned tab.");
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view on an unpinned tab.");
        }

        [Given(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [Then(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [Given(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        [Then(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [Given(@"I Click Assign Tool QviLarge Preview")]
        [When(@"I Click Assign Tool QviLarge Preview")]
        [Then(@"I Click Assign Tool QviLarge Preview")]
        public void Click_Assign_Tool_QviLarge_Preview()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PreviewCustom.PreviewGroup.PreviewButton, new Point(30, 4));
        }

        [Given(@"I click AssignObject Done")]
        [When(@"I click AssignObject Done")]
        [Then(@"I click AssignObject Done")]
        public void click_AssignObject_Done()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton, new Point(18, 10));
        }

        [Given(@"I Click Base Convert Large View Done Button")]
        [When(@"I Click Base Convert Large View Done Button")]
        [Then(@"I Click Base Convert Large View Done Button")]
        public void Click_Base_Convert_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.DoneButton, new Point(36, 11));
            Assert.AreEqual("SomeData", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.SmallView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Text, "Base convert small view row1 variable textbox does not contain text SomeData.");
        }

        [Given(@"I Click Calculate Large View Done Button")]
        [When(@"I Click Calculate Large View Done Button")]
        [Then(@"I Click Calculate Large View Done Button")]
        public void Click_Calculate_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.DoneButton, new Point(45, 8));
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.Listbox.FunctionTextbox.Text, "Calculate small view function textbox text does not equal SomeVariable.");
        }

        [Given(@"I Click Cancel DebugInput Window")]
        [When(@"I Click Cancel DebugInput Window")]
        [Then(@"I Click Cancel DebugInput Window")]
        public void Click_Cancel_DebugInput_Window()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.CancelButton.Enabled, "CancelButton is not enabled after clicking RunDebug from Menu.");
            Mouse.Click(MainStudioWindow.DebugInputDialog.CancelButton, new Point(26, 13));
        }

        [Given(@"I Click Clear Toolbox Filter Clear Button")]
        [When(@"I Click Clear Toolbox Filter Clear Button")]
        [Then(@"I Click Clear Toolbox Filter Clear Button")]
        public void Click_Clear_Toolbox_Filter_Clear_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.ClearFilterButton, new Point(8, 7));
        }

        [Given(@"I Click Close Critical Error Dialog")]
        [When(@"I Click Close Critical Error Dialog")]
        [Then(@"I Click Close Critical Error Dialog")]
        public void Click_Close_Critical_Error_Dialog()
        {
            Mouse.Click(CriticalErrorWindow.CloseButton, new Point(9, 11));
        }

        [Given(@"I Click Close DB Source Wizard Tab Button")]
        [When(@"I Click Close DB Source Wizard Tab Button")]
        [Then(@"I Click Close DB Source Wizard Tab Button")]
        public void Click_Close_DB_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.CloseButton, new Point(13, 4));
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.TabCloseButton.Exists, "Settings close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.TabCloseButton, new Point(16, 6));
        }

        [Given(@"I Click Close DotNetDll Tab")]
        [When(@"I Click Close DotNetDll Tab")]
        [Then(@"I Click Close DotNetDll Tab")]
        public void Click_Close_DotNetDll_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.CloseButton, new Point(13, 4));
        }

        [Given(@"I Click Close EmailSource Tab")]
        [When(@"I Click Close EmailSource Tab")]
        [Then(@"I Click Close EmailSource Tab")]
        public void Click_Close_EmailSource_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.EmailSourceTabCloseButton, new Point(13, 10));
        }

        [Given(@"I Click Close Error Dialog")]
        [When(@"I Click Close Error Dialog")]
        [Then(@"I Click Close Error Dialog")]
        public void Click_Close_Error_Dialog()
        {
            Mouse.Click(ErrorWindow.CloseButton, new Point(8, 9));
        }

        [Given(@"I Click Close FullScreen")]
        [When(@"I Click Close FullScreen")]
        [Then(@"I Click Close FullScreen")]
        public void Click_Close_FullScreen()
        {
            Mouse.Click(MainStudioWindow.ExitFullScreenF11Text.ExitFullScreenF11Hyperlink, new Point(64, 5));
        }

        [Given(@"I Click Close Plugin Source Wizard Tab Button")]
        [When(@"I Click Close Plugin Source Wizard Tab Button")]
        [Then(@"I Click Close Plugin Source Wizard Tab Button")]
        public void Click_Close_Plugin_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.CloseButton, new Point(13, 4));
        }

        [Given(@"I Click Close Server Source Wizard Tab Button")]
        [When(@"I Click Close Server Source Wizard Tab Button")]
        [Then(@"I Click Close Server Source Wizard Tab Button")]
        public void Click_Close_Server_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.TabCloseButton, new Point(5, 5));
        }

        [Given(@"I Click Close Settings Tab Button")]
        [When(@"I Click Close Settings Tab Button")]
        [Then(@"I Click Close Settings Tab Button")]
        public void Click_Close_Settings_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton.Exists, "Settings close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton, new Point(16, 6));
        }

        [Given(@"I Click Close SharepointSource Tab Button")]
        [When(@"I Click Close SharepointSource Tab Button")]
        [Then(@"I Click Close SharepointSource Tab Button")]
        public void Click_Close_SharepointSource_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointSourceTabCloseButton, new Point(13, 7));
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

        [Given(@"I Click Close Tests Tab")]
        [When(@"I Click Close Tests Tab")]
        [Then(@"I Click Close Tests Tab")]
        [Given(@"I Click Close Tests Tab")]
        public void Click_Close_Tests_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.CloseTestTabButton, new Point(11, 5));
        }

        [Given(@"I Click Close Web Source Wizard Tab Button")]
        [When(@"I Click Close Web Source Wizard Tab Button")]
        [Then(@"I Click Close Web Source Wizard Tab Button")]
        public void Click_Close_Web_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.CloseButton, new Point(9, 6));
        }

        [Given(@"I Click Close Workflow Tab Button")]
        [When(@"I Click Close Workflow Tab Button")]
        [Then(@"I Click Close Workflow Tab Button")]
        public void Click_Close_Workflow_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton.Exists, "Close tab button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton, new Point(5, 5));
        }

        [Given(@"I Click ConfigureSetting From Menu")]
        [When(@"I Click ConfigureSetting From Menu")]
        [Then(@"I Click ConfigureSetting From Menu")]
        public void Click_ConfigureSetting_From_Menu()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.Exists, "Settings tab does not exist after the Configure/Setting Menu button is clicked");
        }

        [Given(@"I Click Connect Control InExplorer")]
        [When(@"I Click Connect Control InExplorer")]
        [Then(@"I Click Connect Control InExplorer")]
        public void Click_Connect_Control_InExplorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
        }

        [Given(@"I Click Debug Output Assign Cell")]
        [When(@"I Click Debug Output Assign Cell")]
        [Then(@"I Click Debug Output Assign Cell")]
        public void Click_Debug_Output_Assign_Cell()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus, "Multiassign small view is not selected.");
        }

        [Given(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        [When(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        [Then(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        public void Click_Debug_Output_Assign_Cell_For_Unpinned_Workflow_Tab()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
        }

        [Given(@"I Click Debug Output BaseConvert Cell")]
        [When(@"I Click Debug Output BaseConvert Cell")]
        [Then(@"I Click Debug Output BaseConvert Cell")]
        public void Click_Debug_Output_BaseConvert_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.BaseConversion1Button, new Point(33, 7));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.ItemStatus, "Base conversion small view is not selected.");
        }

        [Given(@"I Click Debug Output Calculate Cell")]
        [When(@"I Click Debug Output Calculate Cell")]
        [Then(@"I Click Debug Output Calculate Cell")]
        public void Click_Debug_Output_Calculate_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.CalculateButton, new Point(24, 10));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.ItemStatus, "Calculate tool small view is not selected.");
        }

        [Given(@"I Click Debug Output Workflow1 Name")]
        [When(@"I Click Debug Output Workflow1 Name")]
        [Then(@"I Click Debug Output Workflow1 Name")]
        public void Click_Debug_Output_Workflow1_Name()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.ServiceTreeItem.Workflow1Button, new Point(24, 8));
            Assert.AreEqual("workflow1", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Workflow1ciremoteText.DisplayText, "Workflow1 remote workflow tab is not open.");
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
            Playback.Wait(500);
            Assert.IsFalse(ControlExistsNow(DecisionOrSwitchDialog), "Decision large view dialog still exists after the done button is clicked.");
        }

        [Given(@"I Click Delete Done Button")]
        [When(@"I Click Delete Done Button")]
        [Then(@"I Click Delete Done Button")]
        public void Click_Delete_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click Deploy Tab Destination Server Combobox")]
        [When(@"I Click Deploy Tab Destination Server Combobox")]
        [Then(@"I Click Deploy Tab Destination Server Combobox")]
        public void Click_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
        }

        [Given(@"I Click Deploy Tab Destination Server Connect Button")]
        [When(@"I Click Deploy Tab Destination Server Connect Button")]
        [Then(@"I Click Deploy Tab Destination Server Connect Button")]
        public void Click_Deploy_Tab_Destination_Server_Connect_Button()
        {
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
        [Given(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
        [When(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
        [Then(@"I Click Deploy Tab Destination Server Remote Connection Intergration Item")]
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

        [Given(@"I Click Deploy Tab WarewolfStore Item")]
        [When(@"I Click Deploy Tab WarewolfStore Item")]
        [Then(@"I Click Deploy Tab WarewolfStore Item")]
        public void Click_Deploy_Tab_WarewolfStore_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsWarewolfStore, new Point(214, 9));
        }

        [Given(@"I Click DotNet DLL Large View Done Button")]
        [When(@"I Click DotNet DLL Large View Done Button")]
        [Then(@"I Click DotNet DLL Large View Done Button")]
        public void Click_DotNet_DLL_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.SmallView.Exists, "DotNet DLL small view does not exist after clicking done on large view.");
        }

        [Given(@"I Click DotNet DLL Large View Test Inputs Button")]
        [When(@"I Click DotNet DLL Large View Test Inputs Button")]
        [Then(@"I Click DotNet DLL Large View Test Inputs Button")]
        public void Click_DotNet_DLL_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click Duplicate From Duplicate Dialog")]
        [When(@"I Click Duplicate From Duplicate Dialog")]
        [Then(@"I Click Duplicate From Duplicate Dialog")]
        public void Click_Duplicate_From_Duplicate_Dialog()
        {
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Exists, "Duplicate button does not exist");
            Mouse.Click(SaveDialogWindow.DuplicateButton, new Point(26, 10));
            Assert.IsTrue(SaveDialogWindow.Exists, "Save Dialog does not exist after clicking Duplicate button");
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [Given(@"I Click EditSharepointSource Button")]
        [When(@"I Click EditSharepointSource Button")]
        [Then(@"I Click EditSharepointSource Button")]
        public void Click_EditSharepointSource_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [When(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [Then(@"I Click EditSharepointSource Button From SharePointUpdate")]
        public void Click_EditSharepointSource_Button_From_SharePointUpdate()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [When(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [Then(@"I Click EditSharepointSource Button FromSharePointDelete")]
        public void Click_EditSharepointSource_Button_FromSharePointDelete()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointRead")]
        [When(@"I Click EditSharepointSource Button FromSharePointRead")]
        [Then(@"I Click EditSharepointSource Button FromSharePointRead")]
        public void Click_EditSharepointSource_Button_FromSharePointRead()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EmailSource TestConnection Button")]
        [When(@"I Click EmailSource TestConnection Button")]
        [Then(@"I Click EmailSource TestConnection Button")]
        public void Click_EmailSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.Spinner);
        }

        [When(@"I Click ExchangeSource TestConnection Button")]
        public void Click_ExchangeSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.Spinner);
        }

        [When(@"I Click EndThisWF On XPath LargeView")]
        [Then(@"I Click EndThisWF On XPath LargeView")]
        public void Click_EndThisWF_On_XPath_LargeView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox.Checked = true;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox, "{Tab}", ModifierKeys.None);
        }

        [Given(@"I Click ExpandAndStepIn NestedWorkflow")]
        [When(@"I Click ExpandAndStepIn NestedWorkflow")]
        [Then(@"I Click ExpandAndStepIn NestedWorkflow")]
        public void Click_ExpandAndStepIn_NestedWorkflow()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.Expanded = true;
        }

        [Given(@"I Click Explorer Filter Clear Button")]
        [When(@"I Click Explorer Filter Clear Button")]
        [Then(@"I Click Explorer Filter Clear Button")]
        public void Click_Explorer_Filter_Clear_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.ClearFilterButton, new Point(6, 8));
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text, "Explorer Filter Textbox text is not blank after clicking the clear button.");
        }

        [When(@"I Click Explorer Localhost First Item")]
        [Given(@"I Click Explorer Localhost First Item")]
        [Then(@"I Click Explorer Localhost First Item")]
        public void Click_Explorer_Localhost_First_Item()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I Click Explorer Remote Server Dropdown List")]
        [When(@"I Click Explorer Remote Server Dropdown List")]
        [Then(@"I Click Explorer Remote Server Dropdown List")]
        public void Click_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(167, 10));
        }

        [Given(@"I Click Explorer Connect Remote Server Button")]
        [When(@"I Click Explorer Connect Remote Server Button")]
        [Then(@"I Click Explorer Connect Remote Server Button")]
        public void Click_Explorer_RemoteServer_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ConnectServerButton, new Point(11, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
        }

        [Given(@"I Click First Recordset Input Checkbox")]
        [When(@"I Click First Recordset Input Checkbox")]
        [Then(@"I Click First Recordset Input Checkbox")]
        public void Click_First_Recordset_Input_Checkbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [Given(@"I Click FormatNumber Done Button")]
        [When(@"I Click FormatNumber Done Button")]
        [Then(@"I Click FormatNumber Done Button")]
        public void Click_FormatNumber_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.DoneButton, new Point(36, 11));
        }

        [Given(@"I Click FullScreen TopRibbon Button")]
        [When(@"I Click FullScreen TopRibbon Button")]
        [Then(@"I Click FullScreen TopRibbon Button")]
        public void Click_FullScreen_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeRestoreStudioButton, new Point(12, 9));
        }

        [Given(@"I Click GET Web Large View Done Button")]
        [When(@"I Click GET Web Large View Done Button")]
        [Then(@"I Click GET Web Large View Done Button")]
        public void Click_GET_Web_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click POST Web Large View Done Button")]
        [When(@"I Click POST Web Large View Done Button")]
        [Then(@"I Click POST Web Large View Done Button")]
        public void Click_POST_Web_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.SmallView.Exists, "Web POST small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click DELETE Web Large View Done Button")]
        [When(@"I Click DELETE Web Large View Done Button")]
        [Then(@"I Click DELETE Web Large View Done Button")]
        public void Click_DELETE_Web_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click PUT Web Large View Done Button")]
        [When(@"I Click PUT Web Large View Done Button")]
        [Then(@"I Click PUT Web Large View Done Button")]
        public void Click_PUT_Web_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.SmallView.Exists, "Web PUT small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [When(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [Then(@"I Click GET Web Large View Done Button With Invalid Large View")]
        public void Click_GET_Web_Large_View_Done_Button_With_Invalid_Large_View()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Exists, "Error not exist after clicking large view done button on invalid large view.");
        }

        [Given(@"I Click GET Web Large View Generate Outputs")]
        [When(@"I Click GET Web Large View Generate Outputs")]
        [Then(@"I Click GET Web Large View Generate Outputs")]
        public void Click_GET_Web_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton, new Point(7, 7));
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton.Exists, "Web GET large view generate outputs test button does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton.Exists, "Web GET tool large view generate inputs done button does not exist.");
        }

        [Given(@"I Click POST Web Large View Generate Outputs")]
        [When(@"I Click POST Web Large View Generate Outputs")]
        [Then(@"I Click POST Web Large View Generate Outputs")]
        public void Click_POST_Web_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.GenerateOutputsButton, new Point(7, 7));
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.TestButton.Exists, "Web POST large view generate outputs test button does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.DoneButton.Exists, "Web POST tool large view generate inputs done button does not exist.");
        }

        [Given(@"I Click PUT Web Large View Generate Outputs")]
        [When(@"I Click PUT Web Large View Generate Outputs")]
        [Then(@"I Click PUT Web Large View Generate Outputs")]
        public void Click_PUT_Web_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton, new Point(7, 7));
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.TestButton.Exists, "Web PUT large view generate outputs test button does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.DoneButton.Exists, "Web PUT tool large view generate inputs done button does not exist.");
        }

        [Given(@"I Click DELETE Web Large View Generate Outputs")]
        [When(@"I Click DELETE Web Large View Generate Outputs")]
        [Then(@"I Click DELETE Web Large View Generate Outputs")]
        public void Click_DELETE_Web_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton, new Point(7, 7));
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.TestButton.Exists, "Web DELETE large view test inputs button does not exist after clicking generate outputs.");
        }

        [Given(@"I Click GET Web Large View Test Inputs Button")]
        [When(@"I Click GET Web Large View Test Inputs Button")]
        [Then(@"I Click GET Web Large View Test Inputs Button")]
        public void Click_GET_Web_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click GET Web Large View Test Inputs Done Button")]
        [When(@"I Click GET Web Large View Test Inputs Done Button")]
        [Then(@"I Click GET Web Large View Test Inputs Done Button")]
        public void Click_GET_Web_Large_View_Test_Inputs_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click POST Web Large View Test Inputs Button")]
        [When(@"I Click POST Web Large View Test Inputs Button")]
        [Then(@"I Click POST Web Large View Test Inputs Button")]
        public void Click_POST_Web_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click POST Web Large View Test Inputs Done Button")]
        [When(@"I Click POST Web Large View Test Inputs Done Button")]
        [Then(@"I Click POST Web Large View Test Inputs Done Button")]
        public void Click_POST_Web_Large_View_Test_Inputs_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click DELETE Web Large View Test Inputs Button")]
        [When(@"I Click DELETE Web Large View Test Inputs Button")]
        [Then(@"I Click DELETE Web Large View Test Inputs Button")]
        public void Click_DELETE_Web_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.TestButton, new Point(21, 11));
        }

        [Then(@"There is an error")]
        public void TheArdonerhasAnError()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.Exists);
        }

        [Given(@"I Click DELETE Web Large View Test Inputs Done Button")]
        [When(@"I Click DELETE Web Large View Test Inputs Done Button")]
        [Then(@"I Click DELETE Web Large View Test Inputs Done Button")]
        public void Click_DELETE_Web_Large_View_Test_Inputs_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click PUT Web Large View Test Inputs Button")]
        [When(@"I Click PUT Web Large View Test Inputs Button")]
        [Then(@"I Click PUT Web Large View Test Inputs Button")]
        public void Click_PUT_Web_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click PUT Web Large View Test Inputs Done Button")]
        [When(@"I Click PUT Web Large View Test Inputs Done Button")]
        [Then(@"I Click PUT Web Large View Test Inputs Done Button")]
        public void Click_PUT_Web_Large_View_Test_Inputs_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click HTTP Delete Web Tool New Button")]
        [When(@"I Click HTTP Delete Web Tool New Button")]
        [Then(@"I Click HTTP Delete Web Tool New Button")]
        public void Click_HTTP_Delete_Web_Tool_New_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.NewSourceButton, new Point(13, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.Exists, "New web source wizard tab is not open after clicking create new web source from delete tool.");
        }

        [Given(@"I Click HTTP Post Web Tool New Button")]
        [When(@"I Click HTTP Post Web Tool New Button")]
        [Then(@"I Click HTTP Post Web Tool New Button")]
        public void Click_HTTP_Post_Web_Tool_New_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton, new Point(17, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.Exists, "New web source wizard tab is not open after clicking create new web source from post tool on the design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "New DB source wizard tab does not exist after clicking the new db source button on Web POST tool.");
        }

        [Given(@"I Click Knowledge Ribbon Button")]
        [When(@"I Click Knowledge Ribbon Button")]
        [Then(@"I Click Knowledge Ribbon Button")]
        public void Click_Knowledge_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.KnowledgeBaseButton, new Point(4, 8));
        }

        [Given(@"I Click Lock Ribbon Button")]
        [When(@"I Click Lock Ribbon Button")]
        [Then(@"I Click Lock Ribbon Button")]
        public void Click_Lock_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.LockStudioButton, new Point(14, 5));
        }

        [Given(@"I Click Maximize Restore TopRibbon Button")]
        [When(@"I Click Maximize Restore TopRibbon Button")]
        [Then(@"I Click Maximize Restore TopRibbon Button")]
        public void Click_Maximize_Restore_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(9, 11));
        }

        [Given(@"I Click Maximize TopRibbon Button")]
        [When(@"I Click Maximize TopRibbon Button")]
        [Then(@"I Click Maximize TopRibbon Button")]
        public void Click_Maximize_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(14, 14));
        }

        [Given(@"I Click MessageBox No")]
        [When(@"I Click MessageBox No")]
        [Then(@"I Click MessageBox No")]
        public void Click_MessageBox_No()
        {
            Mouse.Click(MessageBoxWindow.NoButton, new Point(32, 5));
        }

        [Given(@"I Click MessageBox OK")]
        [When(@"I Click MessageBox OK")]
        [Then(@"I Click MessageBox OK")]
        [Given(@"I Click MessageBox OK")]
        public void Click_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Given(@"I Click MessageBox Yes")]
        [When(@"I Click MessageBox Yes")]
        [Then(@"I Click MessageBox Yes")]
        public void Click_MessageBox_Yes()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(32, 5));
            Assert.IsFalse(ControlExistsNow(MessageBoxWindow), "Message box does exist");
        }

        [Given(@"I Click Minimize TopRibbon Button")]
        [When(@"I Click Minimize TopRibbon Button")]
        [Then(@"I Click Minimize TopRibbon Button")]
        public void Click_Minimize_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MinimizeStudioButton, new Point(6, 14));
        }

        [Given(@"I Click Nested Workflow Name")]
        [When(@"I Click Nested Workflow Name")]
        [Then(@"I Click Nested Workflow Name")]
        public void Click_Nested_Workflow_Name()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.UIHelloWorldTreeItem1.UIHelloWorldButton, new Point(37, 10));
        }

        [Given(@"I Click New Database Source Ribbon Button")]
        [When(@"I Click New Database Source Ribbon Button")]
        [Then(@"I Click New Database Source Ribbon Button")]
        public void Click_New_Database_Source_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.DatabaseSourceButton, new Point(16, 15));
        }

        [Given(@"I Click New Workflow Tab")]
        [When(@"I Click New Workflow Tab")]
        [Then(@"I Click New Workflow Tab")]
        public void Click_New_Workflow_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, new Point(63, 18));
        }

        [Given(@"I Click NewPluginSource Ribbon Button")]
        [When(@"I Click NewPluginSource Ribbon Button")]
        [Then(@"I Click NewPluginSource Ribbon Button")]
        public void Click_NewPluginSource_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.PluginSourceButton, new Point(22, 13));
            Playback.Wait(1000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ContentDockManager.ExplorerTree.Exists, "Select assembly tree does not exist in new plugin source wizard tab.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Exists, "Assembly textbox does not exist in new plugin source wizard tab.");
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ContentDockManager.RefreshButton.Spinner);
        }

        [Given(@"I Click NewSource Button FromODBC Tool")]
        [When(@"I Click NewSource Button FromODBC Tool")]
        [Then(@"I Click NewSource Button FromODBC Tool")]
        public void Click_NewSource_Button_FromODBC_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "DBSourceWizardTab did not open");
        }

        [Given(@"I Click NewSource Button FromOracle Tool")]
        [When(@"I Click NewSource Button FromOracle Tool")]
        [Then(@"I Click NewSource Button FromOracle Tool")]
        public void Click_NewSource_Button_FromOracle_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "DBSourceWizardTab did not open");
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Output OnVariable InVariableList")]
        [When(@"I Click Output OnVariable InVariableList")]
        [Then(@"I Click Output OnVariable InVariableList")]
        public void Click_Output_OnVariable_InVariableList()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [Given(@"I Click Pin Toggle DebugOutput")]
        [When(@"I Click Pin Toggle DebugOutput")]
        [Then(@"I Click Pin Toggle DebugOutput")]
        public void Click_Pin_Toggle_DebugOutput()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputUnpinBtn, new Point(11, 10));
        }

        [Given(@"I Click Pin Toggle Documentor")]
        [When(@"I Click Pin Toggle Documentor")]
        [Then(@"I Click Pin Toggle Documentor")]
        public void Click_Pin_Toggle_Documentor()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Help.DocumentorUnpinBtn, new Point(2, 11));
        }

        [Given(@"I Click Pin Toggle Explorer")]
        [When(@"I Click Pin Toggle Explorer")]
        [Then(@"I Click Pin Toggle Explorer")]
        public void Click_Pin_Toggle_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerUnpinBtn, new Point(12, 9));
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.VariableUnpinBtn, new Point(10, 14));
        }

        [Given(@"I Click Position Button")]
        [When(@"I Click Position Button")]
        [Then(@"I Click Position Button")]
        public void Click_Position_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.FilesMenu.PositionButton, new Point(8, 7));
        }

        [Given(@"I Click Postgre Done Button")]
        [When(@"I Click Postgre Done Button")]
        [Then(@"I Click Postgre Done Button")]
        public void Click_Postgre_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.DoneButton, new Point(36, 11));
        }

        [Given(@"I Click PrefixContainsInvalidText Hyperlink")]
        [When(@"I Click PrefixContainsInvalidText Hyperlink")]
        [Then(@"I Click PrefixContainsInvalidText Hyperlink")]
        public void Click_PrefixContainsInvalidText_Hyperlink()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PrefixcontainsinvaliText.PrefixcontainsinvaliHyperlink, new Point(30, 4));
        }

        [Given(@"I Click PutWeb GenerateOutputs Button")]
        [When(@"I Click PutWeb GenerateOutputs Button")]
        [Then(@"I Click PutWeb GenerateOutputs Button")]
        public void Click_PutWeb_GenerateOutputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton, new Point(34, 13));
        }

        [Given(@"I Click Web Post Tool GenerateOutputs Button")]
        [When(@"I Click Web Post Tool GenerateOutputs Button")]
        [Then(@"I Click Web Post Tool GenerateOutputs Button")]
        public void Click_Web_Post_Tool_GenerateOutputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.GenerateOutputsButton, new Point(34, 13));
        }

        [Given(@"I Click GetWeb GenerateOutputs Button")]
        [When(@"I Click GetWeb GenerateOutputs Button")]
        [Then(@"I Click GetWeb GenerateOutputs Button")]
        public void Click_GetWeb_GenerateOutputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton, new Point(34, 13));
        }

        [Given(@"I Click Read Done Button")]
        [When(@"I Click Read Done Button")]
        [Then(@"I Click Read Done Button")]
        public void Click_Read_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click ReadFolder Done Button")]
        [When(@"I Click ReadFolder Done Button")]
        [Then(@"I Click ReadFolder Done Button")]
        public void Click_ReadFolder_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click Remove Unused Variables")]
        [When(@"I Click Remove Unused Variables")]
        [Then(@"I Click Remove Unused Variables")]
        public void Click_Remove_Unused_Variables()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused, new Point(30, 4));
        }

        [Given(@"I Click Rename Done Button")]
        [When(@"I Click Rename Done Button")]
        [Then(@"I Click Rename Done Button")]
        public void Click_Rename_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.DoneButton, new Point(35, 6));
        }

        [When(@"I Click RequireAllFieldsToMatch CheckBox")]
        public void Click_RequireAllFieldsToMatch_CheckBox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllFieldsToMatchCheckBox.Checked = true;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllFieldsToMatchCheckBox, "{Tab}", ModifierKeys.None);
        }

        [When(@"I Click Reset Perfomance Counter")]
        public void Click_Reset_Perfomance_Counter()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResetCounter.ItemHyperlink, new Point(49, 9));
            Assert.IsTrue(MessageBoxWindow.Exists, "MessageBoxWindow did not show after clicking reset counters");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(50, 12));
        }

        [Given(@"I Click Save Ribbon Button to Open Save Dialog")]
        [When(@"I Click Save Ribbon Button to Open Save Dialog")]
        [Then(@"I Click Save Ribbon Button to Open Save Dialog")]
        public void Click_Save_Ribbon_Button_to_Open_Save_Dialog()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist after clicking save ribbon button.");
        }

        [Given(@"I Click SaveDialog CancelButton")]
        [When(@"I Click SaveDialog CancelButton")]
        [Then(@"I Click SaveDialog CancelButton")]
        public void Click_SaveDialog_CancelButton()
        {
            Mouse.Click(SaveDialogWindow.CancelButton, new Point(6, 7));
        }

        [When(@"I Click Scheduler Create New Task Ribbon Button")]
        public void Click_Scheduler_Create_New_Task_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.SchedulerListItem.CreateTaskButton.NewTaskButton, new Point(151, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.EnabledRadioButton.Enabled, "Scheduler is disabled by default");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.EditTriggerButton.Exists, "Edit Schedule time buttom exist after clicking scheduler");
        }

        [When(@"I Click Scheduler Delete Task")]
        public void Click_Scheduler_Delete_Task()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.DeleteTaskButton, new Point(3, 17));
        }

        [When(@"I Click Scheduler Disable Task Radio Button")]
        public void Click_Scheduler_Disable_Task_Radio_Button()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.DisabledRadioButton.Selected = true;
        }

        [When(@"I Click Scheduler EditTrigger Button")]
        public void Click_Scheduler_EditTrigger_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.EditTriggerButton, new Point(10, 9));
        }

        [When(@"I Click Scheduler Enable Task Radio Button")]
        public void Click_Scheduler_Enable_Task_Radio_Button()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.UINameworkflow1ResourcListItem.StatusCheckBox.Checked = true;
        }

        [When(@"I Click Scheduler ResourcePicker Button")]
        public void Click_Scheduler_ResourcePicker_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton, new Point(14, 13));
        }

        [When(@"I Click Scheduler RunTask")]
        public void Click_Scheduler_RunTask()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.RunTaskCheckBox.Checked = true;
        }

        [When(@"I Click Select Resource Button")]
        public void Click_Select_Resource_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab.PerfmonViewContent.ResourceTable.Row1.ResourceCell.ResourceButton, new Point(9, 8));
        }

        [When(@"I Click Select Resource Button From Resource Permissions")]
        public void Click_Select_Resource_Button_From_Resource_Permissions()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1), new Point(13, 16));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service window does not exist after clicking SelectResource button");
        }

        [When(@"I Click Select Windows Group Cancel Button")]
        public void Click_Select_Windows_Group_Cancel_Button()
        {
            Assert.IsTrue(SelectWindowsGroupDialog.CancelPanel.Cancel.Exists, "Select Windows group dialog cancel buttton does not exist.");
            Mouse.Click(SelectWindowsGroupDialog.CancelPanel.Cancel, new Point(28, 9));
        }

        [When(@"I Click Select Windows Group OK Button")]
        public void Click_Select_Windows_Group_OK_Button()
        {
            Mouse.Click(SelectWindowsGroupDialog.OKPanel.OK, new Point(37, 9));
        }

        [When(@"I Click Server Log File Button")]
        public void Click_Server_Log_File_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.ServerLogs.ServerLogFile.ItemHyperlink, new Point(83, 6));
        }

        [When(@"I Click Server Source Wizard Address Protocol Dropdown")]
        public void Click_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
        }

        [When(@"I Click Server Source Wizard Test Connection Button")]
        public void Click_Server_Source_Wizard_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.TestConnectionButton, new Point(51, 8));
            Playback.Wait(3000);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled");
        }

        [When(@"I Click Service Picker Dialog First Service In Explorer")]
        public void Click_Service_Picker_Dialog_First_Service_In_Explorer()
        {
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1, new Point(91, 9));
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

        [When(@"I Click Show Dependencies In Explorer Context Menu")]
        public void Click_Show_Dependencies_In_Explorer_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ShowDependencies, new Point(50, 15));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.AreEqual("RemoteServerUITestWorkflow", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text.DisplayText, "Dependant workflow not shown in dependency diagram");
        }

        [When(@"I Click Show Server Version Explorer Context menu")]
        public void Click_Show_Server_Version_Explorer_Context_menu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ShowServerVersion, new Point(45, 13));
        }

        [When(@"I Click SQL Server Large View Done Button")]
        public void Click_SQL_Server_Large_View_Done_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.DoneButton.Exists, "SQL Server large view done button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.DoneButton, new Point(35, 6));
        }

        [When(@"I Click SQL Server Large View Generate Outputs")]
        public void Click_SQL_Server_Large_View_Generate_Outputs()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.GenerateOutputsButton.Exists, "SQL Server large view does not contain a generate outputs button.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Exists, "SQL Server large view test inputs row 1 test data textbox does not exist.");
        }

        [When(@"I Click SQL Server Large View Test Inputs Button")]
        public void Click_SQL_Server_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsButton, new Point(21, 11));
        }

        [When(@"I Click SQL Server Large View Test Inputs Done Button")]
        public void Click_SQL_Server_Large_View_Test_Inputs_Done_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsDoneButton.Exists, "SQL Server large view test inputs done button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsDoneButton, new Point(35, 6));
        }

        [When(@"I Click SqlBulkInsert Done Button")]
        public void Click_SqlBulkInsert_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Start Node")]
        public void Click_Start_Node()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, new Point(29, 76));
        }

        [When(@"I Click Studio Log File")]
        public void Click_Studio_Log_File()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsViewConte.StudioLogs.StudioLogFile.ItemHyperlink, new Point(79, 10));
        }

        [When(@"I Click Switch Dialog Done Button")]
        public void Click_Switch_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(24, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch.Exists, "Switch on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Click System Information Tool Done Button")]
        public void Click_System_Information_Tool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton, new Point(35, 6));
        }

        [When(@"I Click UnDock Explorer")]
        public void Click_UnDock_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerUnpinBtn, new Point(177, -13));
        }

        [When(@"I Click Unlock Ribbon Button")]
        public void Click_Unlock_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.LockMenuButton, new Point(8, 6));
        }

        [When(@"I Click Unlock TopRibbon Button")]
        public void Click_Unlock_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.LockStudioButton, new Point(10, 12));
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

        [When(@"I Click UnZip Done Button")]
        public void Click_UnZip_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.DoneButton, new Point(35, 6));
        }

        [When(@"I Click UpdateDuplicateRelationships")]
        public void Click_UpdateDuplicateRelationships()
        {
            SaveDialogWindow.UpdateDuplicatedRelat.Checked = true;
        }

        [When(@"I Click Variable IsInput")]
        public void Click_Variable_IsInput()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Exists, "Input Checkbox does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Enabled, "Input Checkbox is disabled.");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Recordset Row1 IsInputCheckbox")]
        public void Click_VariableList_Recordset_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row1 Delete Button")]
        public void Click_VariableList_Scalar_Row1_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Image, new Point(5, 8));
        }

        [When(@"I Click VariableList Scalar Row1 IsInputCheckbox")]
        public void Click_VariableList_Scalar_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click View Api From Context Menu")]
        public void Click_View_Api_From_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(85, 11));
            Mouse.Click(MainStudioWindow.ExplorerEnvironmentContextMenu.ViewApisJsonMenuItem, new Point(71, 13));
        }

        [When(@"I Click ViewSwagger From ExplorerContextMenu")]
        public void Click_ViewSwagger_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ViewSwagger, new Point(82, 16));
        }

        [When(@"I Click WebRequest Tool Large View Done Button")]
        public void Click_WebRequest_Tool_Large_View_Done_Button()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Write Done Button")]
        public void Click_Write_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Yes On The Confirm Delete")]
        public void Click_Yes_On_The_Confirm_Delete()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(39, 17));
        }

        [When(@"I Click Zip Done Button")]
        public void Click_Zip_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.DoneButton, new Point(35, 6));
        }

        [When(@"I Close Data Merge LargeView")]
        public void Close_Data_Merge_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(257, 7));
        }

        [When(@"I CopyAndPaste Decision Tool On The Designer")]
        public void CopyAndPaste_Decision_Tool_On_The_Designer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Copy, new Point(64, 15));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Paste, new Point(64, 15));
        }

        [When(@"I Create New Folder ""(.*)"" In Explorer Second Item With Context Menu")]
        public void Create_New_Folder_In_Explorer_Second_Item_With_Context_Menu(string FolderName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(126, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem, new Point(78, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.ItemEdit.Text = FolderName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, "{Enter}", ModifierKeys.None);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Delete Nested Hello World")]
        public void Delete_Nested_Hello_World()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(93, 14));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Delete, new Point(61, 15));
            Mouse.Click(MessageBoxWindow.YesButton, new Point(7, 12));
        }

        [When(@"I DisplayStartNodeContextMenu")]
        public void DisplayStartNodeContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, MouseButtons.Right, ModifierKeys.None, new Point(179, 31));
        }

        [When(@"I DoubleClick Explorer First Remote Server First Item")]
        public void DoubleClick_Explorer_First_Remote_Server_First_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(63, 11));
        }

        [Given(@"I DoubleClick Explorer Localhost First Item")]
        [When(@"I DoubleClick Explorer Localhost First Item")]
        [Then(@"I DoubleClick Explorer Localhost First Item")]
        public void DoubleClick_Explorer_Localhost_First_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [When(@"I Drag DeleteWeb Toolbox Onto Workflow Surface")]
        public void Drag_DeleteWeb_Toolbox_Onto_Workflow_Surface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Delete Web connector tool large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag Dice Onto Dice On The DesignSurface")]
        public void Drag_Dice_Onto_Dice_On_The_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(301, 228));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, new Point(49, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(301, 228));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow.DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
        }

        [When(@"I Drag DotNet DLL Connector Onto DesignSurface")]
        public void Drag_DotNet_DLL_Connector_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DotNet DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.DotNetDLL, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.Exists, "DotNet DLL tool large view does not exist on the design surface after dragging in from the toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Com DLL Connector Onto DesignSurface")]
        public void Drag_Com_DLL_Connector_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Com DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.ComDLL, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.Exists, "Com DLL tool large view does not exist on the design surface after dragging in from the toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag WCF Service Connector Onto DesignSurface")]
        public void Drag_WCF_Service_Connector_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "WCF";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.WCF, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WcfService.LargeView.Exists, "WCF Service tool large view does not exist on the design surface after dragging in from the toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost Second Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Explorer Remote workflow1 Onto Workflow Design Surface")]
        [When(@"I Drag Explorer Remote workflow1 Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer Remote workflow1 Onto Workflow Design Surface")]
        public void Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SubWorkflow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [When(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        public void Drag_Explorer_workflow_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SubWorkflow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I open ""(.*)"" in Remote Connection Integration")]
        public void WhenIOpenInRemoteConnectionIntegration(string resourceName)
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
        }


        [When(@"I Drag GET Web Connector Onto DesignSurface")]
        public void Drag_GET_Web_Connector_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "GET";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.GET, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.Exists, "GET Web connector large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag GetWeb RequestTool Onto DesignSurface")]
        public void Drag_GetWeb_RequestTool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Web Request";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 124));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest, new Point(12, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 124));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.Exists, "Web Get Request small view does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag PostWeb RequestTool Onto DesignSurface")]
        public void Drag_PostWeb_RequestTool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Web Post Request large view does not exist on the design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag PutWeb Tool Onto DesignSurface")]
        public void Drag_PutWeb_Tool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Put Web connector large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag Toolbox AggregateCalculate Onto DesignSurface")]
        public void Drag_Toolbox_AggregateCalculate_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Aggregate Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.AggregateCalculate, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.SmallViewContentCustom.fxComboBox.Exists, "fx combobox does not exist after dragging Aggregate Calculate tool onto design surface");
        }

        [When(@"I Drag Toolbox AssignObject Onto DesignSurface")]
        public void Drag_Toolbox_AssignObject_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject.Exists, "Toolbox AssignObject does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.Exists, "Assign object tool does not exist on the design surface after dragging in from the toolbox");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Base Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Base_Conversion_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Base Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.BaseConvert, new Point(12, 12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.Exists, "Base Conversion on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Calculate Onto DesignSurface")]
        public void Drag_Toolbox_Calculate_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Calculate, new Point(59, -17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.Exists, "Calculate tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Case Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Case_Conversion_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Case Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.CaseConvert, new Point(19, 13));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.Exists, "Case Conversion on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox CMD Line Onto DesignSurface")]
        public void Drag_Toolbox_CMD_Line_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "CMD Script";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 122));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.CMDScript, new Point(19, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.Exists, "CMD Line tool on the design surface tool does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Comment Onto DesignSurface")]
        public void Drag_Toolbox_Comment_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(40, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Copy Onto DesignSurface")]
        public void Drag_Toolbox_Copy_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(310, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Copy, new Point(19, -3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(310, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.Exists, "Copy on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Count Records Onto DesignSurface")]
        public void Drag_Toolbox_Count_Records_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Count";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Count, new Point(13, 18));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Create Onto DesignSurface")]
        public void Drag_Toolbox_Create_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Create, new Point(9, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.Exists, "Create tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Merge Onto DesignSurface")]
        public void Drag_Toolbox_Data_Merge_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Merge";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 133));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataMerge, new Point(54, 23));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data Merge on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Split Onto DesignSurface")]
        public void Drag_Toolbox_Data_Split_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Split";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataSplit, new Point(3, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.Exists, "Data Split on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [When(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [Then(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        public void Drag_Toolbox_Date_And_Time_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTime, new Point(20, -1));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.Exists, "Date and Time tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox DateTime Difference Onto DesignSurface")]
        public void Drag_Toolbox_DateTime_Difference_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time Diff";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTimeDifference, new Point(48, 7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.Exists, "Date And Time Difference tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Decision Onto DesignSurface")]
        public void Drag_Toolbox_Decision_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(DecisionOrSwitchDialog.DoneButton.Exists, "Decision dialog done button does not exist");
        }

        [When(@"I Drag Toolbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Delete, new Point(13, 9));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.Exists, "Delete tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Delete Record Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Record_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Delete, new Point(1, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Delete, new Point(240, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.Exists, "Dropbox delete tool does not exist on design surface after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox Dropbox Download Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Download_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Download, new Point(16, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload.Exists, "Dropbox download tool does not exist on design surface after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox Dropbox FileList Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.ListContents, new Point(124, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.Exists, "Dropbox list contents tool does not exist on design surface after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox Dropbox Upload Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Upload, new Point(66, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.Exists, "Dropbox upload tool does not exist on design surface after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox Exchange Email Onto DesignSurface")]
        public void Drag_Toolbox_Exchange_Email_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Exchange Email";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.ExchangeSend, new Point(16, -39));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Index_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Index";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.FindIndex, new Point(9, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.Exists, "Find Index on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Record Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Record_Index_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Records";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.FindRecords, new Point(8, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox For Each Onto DesignSurface")]
        public void Drag_Toolbox_For_Each_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ForEach";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.ForEach, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.Exists, "For Each tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Format Number Onto DesignSurface")]
        public void Drag_Toolbox_Format_Number_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Format Number";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.FormatNumber, new Point(18, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.Exists, "Format Number tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Javascript Onto DesignSurface")]
        public void Drag_Toolbox_Javascript_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Javascript";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.JavaScript, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.Exists, "Javascript tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox JSON Onto DesignSurface")]
        public void Drag_Toolbox_JSON_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create JSON";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.CreateJSON, new Point(0, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "Create JSON tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Length Onto DesignSurface")]
        public void Drag_Toolbox_Length_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Length";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Length, new Point(16, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.Exists, "Length tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Move Onto DesignSurface")]
        public void Drag_Toolbox_Move_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Move, new Point(32, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.Exists, "Move tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [When(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [Then(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        public void Drag_Toolbox_MultiAssign_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Exists, "Assign small view row 1 variable textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox MySql Database Onto DesignSurface")]
        public void Drag_Toolbox_MySql_Database_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "MySQL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.MySQL, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.Exists, "My SQL database tool large view does not exist after dragging in from the toolbox");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox ODBC Dtatbase Onto DesignSurface")]
        public void Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ODBC";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.ODBC, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.Exists, "ODBC database tool large view does not exist after dragging in from the toolbox");
        }

        [When(@"I Drag Toolbox Oracle Database Onto DesignSurface")]
        public void Drag_Toolbox_Oracle_Database_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Oracle";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Oracle, new Point(11, 20));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.Exists, "Oracle database tool large view does not exist after dragging in from the toolbox");
        }

        [When(@"I Drag Toolbox PostgreSql Onto DesignSurface")]
        public void Drag_Toolbox_PostgreSql_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Postgre";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Postgre, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.Exists, "Oracle database tool large view does not exist after dragging in from the toolbox");
        }

        [When(@"I Drag Toolbox Python Onto DesignSurface")]
        public void Drag_Toolbox_Python_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Python";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Python, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.Exists, "Python tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox RabbitMqConsume Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqConsume_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Consume";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQConsume, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.SmallViewContentCustom.Exists, "Small View does not exist after dragging RabbitMq tool onto the design surface");
        }

        [When(@"I Drag Toolbox RabbitMqPublish Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqPublish_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Publish";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQPublish, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.SmallViewContentCustom.Exists, "Small view does not exist after dragging RabbitMq publish tool onto the design surface");
        }

        [When(@"I Drag Toolbox Random Onto DesignSurface")]
        public void Drag_Toolbox_Random_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Random";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Random, new Point(9, -21));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.Exists, "Random tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read File Onto DesignSurface")]
        public void Drag_Toolbox_Read_File_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read File";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFile, new Point(12, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.Exists, "Read File tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Read_Folder_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read Folder";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFolder, new Point(14, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.Exists, "Read folder tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Rename Onto DesignSurface")]
        public void Drag_Toolbox_Rename_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Rename";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Rename, new Point(6, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.Exists, "Rename tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Replace Onto DesignSurface")]
        public void Drag_Toolbox_Replace_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Replace";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 121));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.Replace, new Point(16, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 121));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.Exists, "Replace on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Ruby Onto DesignSurface")]
        public void Drag_Toolbox_Ruby_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Ruby";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Ruby, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.Exists, "Ruby tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Selectandapply Onto DesignSurface")]
        public void Drag_Toolbox_Selectandapply_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Select and apply";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.Selectandapply, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.Exists, "Select and apply does not exist on design surface after dragging from toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging select and apply tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sequence Onto DesignSurface")]
        public void Drag_Toolbox_Sequence_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sequence";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Sequence, new Point(18, -12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.Exists, "Sequence on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Service Picker Onto DesignSurface")]
        public void Drag_Toolbox_Service_Picker_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Service";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.Service, new Point(50, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 126));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service picker does not exist on the Design Surface");
            Assert.IsTrue(ServicePickerDialog.Cancel.Exists, "Service picker dialog cancel button does not exist");
        }

        [When(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CopyFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server.Exists, "server lookup does not exist on the sharepoint small view.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Create Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CreateListItems, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.Exists, "server lookup does not exist on the sharepoin smal view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.List.Exists, "sharepint list does not exist on the sharepoint small view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Delete Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteListItems, new Point(16, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Download File Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Download_File_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Download";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DownloadFile, new Point(124, 593));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.SmallView.Exists, "Sharepoint delete tool small view does does not exist after dragging tool from toolbox.");
        }

        [When(@"I Drag Toolbox Sharepoint MoveFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_MoveFile_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.MoveFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server.Exists, "server lookup does not exist on the sharepoin smal view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_Folder_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read";
            Playback.Wait(1000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadFolder, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_List_Item_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadListItems, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Update Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Update List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UpdateListItems, new Point(17, 9));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint UploadFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Upload";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UploadFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.Exists, "Small view does not exist on sharepoint upload file after dragging in from the toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SMTP Email Onto DesignSurface")]
        public void Drag_Toolbox_SMTP_Email_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SMTP Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.SMTPSend, new Point(16, -39));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.Exists, "Email tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sort Record Onto DesignSurface")]
        public void Drag_Toolbox_Sort_Record_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sort";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 122));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Sort, new Point(7, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SQL Bulk Insert Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Bulk Insert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLBulkInsert, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.Exists, "Sql Bulk Insert tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SQL Server Tool Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Server";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLServer, new Point(10, -7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.Exists, "SQL Server database connector tool large view does not exist after dragging in from the toolbox");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Switch Onto DesignSurface")]
        public void Drag_Toolbox_Switch_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(22, 30));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 126));
            Assert.IsTrue(DecisionOrSwitchDialog.DoneButton.Exists, "Decision dialog done button does not exist");
        }

        [When(@"I Drag Toolbox System Information Onto DesignSurface")]
        public void Drag_Toolbox_System_Information_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sys Info";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.SysInfo, new Point(8, 12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.Exists, "System Info tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unique Records Onto DesignSurface")]
        public void Drag_Toolbox_Unique_Records_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unique";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 133));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.UniqueRecords, new Point(43, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unzip Onto DesignSurface")]
        public void Drag_Toolbox_Unzip_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unzip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.UnZip, new Point(15, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.Exists, "Unzip on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Web Request Onto DesignSurface")]
        public void Drag_Toolbox_Web_Request_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Web Request";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest, new Point(14, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.Exists, "Web Request on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Write File Onto DesignSurface")]
        public void Drag_Toolbox_Write_File_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Write File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 132));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.WriteFile, new Point(10, 18));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 132));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.Exists, "Write File tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox XPath Onto DesignSurface")]
        public void Drag_Toolbox_XPath_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "XPath";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 123));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.XPath, new Point(12, -13));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 123));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.Exists, "XPath tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Zip Onto DesignSurface")]
        public void Drag_Toolbox_Zip_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Zip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Zip, new Point(16, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.Exists, "Zip tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        public void Duplicate_Explorer_Localhost_First_Item_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate does not exist in explorer context menu.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Duplicate, new Point(62, 10));
            Assert.IsTrue(SaveDialogWindow.Exists, "Duplicate dialog does not exist after clicking duplicate in the explorer context menu.");
        }

        [Given(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        [When(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        [Then(@"I Enter ""(.*)"" Into Deploy Source Filter")]
        public void Enter_DeployViewOnly_Into_Deploy_Source_Filter(string SearchTextboxText)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = SearchTextboxText;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.Exists, "First deploy tab source explorer item does not exist after filter is applied.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Exists, "Deploy source server explorer tree first item checkbox does not exist.");
        }

        public void Filter_Deploy_Source_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.SearchTextbox.Text = FilterText;
        }

        [When(@"I Enter Duplicate workflow name")]
        public void Enter_Duplicate_workflow_name()
        {
            SaveDialogWindow.ServiceNameTextBox.Text = "DuplicatedWorkFlow";
        }

        [When(@"I Enter InputDebug value")]
        public void Enter_InputDebug_value()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Exists, "InputData row does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Exists, "InputData row does not exist.");
            MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = "100";
        }

        [When(@"I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab")]
        public void Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_Tab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.UserNameTextBoxEdit.Text = "LocalSchedulerAdmin";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.PasswordTextbox.Text = "987Sched#@!";
        }

        [When(@"I Enter Public As Windows Group")]
        public void Enter_Public_As_Windows_Group()
        {
            FindWindowsGroupTextbox(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = "Public";
        }

        [When(@"I Enter RunAsUser Username And Password")]
        public void Enter_RunAsUser_Username_And_Password()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit.Text = "testuser";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit, "a1cbgHEVu098QBN0jqs55wYP/bLfpGNMxw2YxtLIgKOALxPfITSBDjNERdIi/KEq", true);
        }

        [When(@"I Enter Sharepoint Server Path From OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.LocalPathFromIntellisenseCombobox.Textbox.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ServerPathToIntellisenseCombobox.Textbox.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint ServerSource ServerName")]
        public void Enter_Sharepoint_ServerSource_ServerName()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Text = "http://rsaklfsvrsharep/";
        }

        [When(@"I Enter Sharepoint ServerSource User Credentials")]
        public void Enter_Sharepoint_ServerSource_User_Credentials()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Text = "Bernartdt@dvtdev.onmicrosoft.com";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox, new Point(89, 1));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox, "YN/mQM5J9PSwtnVGttwUbqV2NkA27Xtb2Cs5ppSS77kjZgxPPM79nWlqEFRqmwY4KvuSBKnsLDU6spVwV" +
                                                                                                                                                                                       "rcWKXwSuKb7vBXD", true);
        }

        [When(@"I Enter SomeData Into Base Convert Large View Row1 Value Textbox")]
        public void Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Text = "SomeData";
        }

        [When(@"I Enter SomeVariable Into Calculate Large View Function Textbox")]
        public void Enter_SomeVariable_Into_Calculate_Large_View_Function_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.FunctionTextbox.Text = "[[SomeVariable]]";
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.FunctionTextbox.Text, "Calculate large view function textbox text does not equal \"[[SomeVariable]]\"");
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[Some$Invalid%Variable]]";
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid" +
                    "%Variable]]\".");
        }

        [Given(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [Then(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Home}", ModifierKeys.Shift);
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" af" +
                    "ter selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable On Unpinned Tab")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_Tab()
        {
            Keyboard.SendKeys(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" on" +
                    " unpinned tab after selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable Using Click Intellisense Suggestion")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.ListItem, new Point(39, 10));
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable UsingIntellisense")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[{Down}{Enter}", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Enter Text Into Workflow Tests OutPutTable Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITest()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox, "Helo User", ModifierKeys.None);
            Assert.AreEqual("Hello User", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox.Text, "Workflow tests output tabe row 1 value textbox text does not equal Helo User afte" +
                    "r typing that in.");
        }

        [When(@"I Enter Text Into Workflow Tests Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox, "User", ModifierKeys.None);
            Assert.AreEqual("User", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox.Text, "Workflow tests row 1 value textbox text does not equal User after typing that in." +
                    "");
        }

        [When(@"I Enter Vaiablelist Items")]
        public void Enter_Vaiablelist_Items()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, new Point(62, 3));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = "varableA";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, new Point(82, 2));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = "variableB";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, new Point(84, 2));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = "VariableC";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
        }

        [When(@"I Filter variables")]
        public void Filter_variables()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.FilterText.Exists, "Variable filter textbox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox, new Point(89, 7));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
        }

        [When(@"I I Open Explorer First Item Context Menu")]
        public void I_Open_Explorer_First_Item_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.ShowVersionHistory.Exists, "Show version history does not exist after right clicking a resource");
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.ViewSwagger.Exists, "View Swagger button does not exist");
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.ViewSwagger.Enabled, "View swagger is disabled");
        }

        [When(@"I Drag Explorer First Item Onto The Second Item")]
        public void Drag_Explorer_First_Item_Onto_The_Second_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(94, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 11));
        }

        [When(@"I Move Dice Roll To Localhost")]
        public void Move_Dice_Roll_To_Localhost()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.EnsureClickable(new Point(10, 10));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(92, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(10, 10));
        }

        [When(@"I Open AggregateCalculate Tool large view")]
        public void Open_AggregateCalculate_Tool_large_view()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat, new Point(136, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.Exists, "Tool large view does not exist after opening Aggregate Calculate tool large view");
        }

        [When(@"I Open Assign Tool Large View")]
        public void Open_Assign_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [When(@"I Add Variables The Variable List")]
        [Given(@"I Add Variables The Variable List")]
        [Then(@"I Add Variables The Variable List")]
        public void Add_Variables(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [When(@"I Add Recordset The Recordset List")]
        [Given(@"I Add Recordset The Recordset List")]
        [Then(@"I Add Recordset The Recordset List")]
        public void Add_Recordsets(string variables)
        {
            var strings = variables.Split(',');
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
        }


        [When(@"I Add Recordset Fields The Recordset List")]
        [Given(@"I Add Recordset Fields The Recordset List")]
        [Then(@"I Add Recordset Fields The Recordset List")]
        public void Add_Recordsets_Fields(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [Given(@"I Sort Variable List")]
        [When(@"I Sort Variable List")]
        [Then(@"I Sort Variable List")]
        public void Click_Sort_Variable_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.Sort);
        }

        [When(@"I Open Assign Tool On Unpinned Tab Large View")]
        public void Open_Assign_Tool_On_Unpinned_Tab_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the unpinned tab design surface does not exist");
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Exists, "Assign large view row 1 variable textbox does not exist after openning large view" +
                    " with a double click on an unpinned tab.");
        }

        [When(@"I Open Assign Tool Qvi Large View")]
        public void Open_Assign_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.Exists, "QVI on assign is not open");
        }

        [When(@"I Open Assign Tool Qvi Large View On Unpinned Tab")]
        public void Open_Assign_Tool_Qvi_Large_View_On_Unpinned_Tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.Exists, "QVI on assign is not open");
        }

        [When(@"I Open AssignObject Large Tool")]
        public void Open_AssignObject_Large_Tool()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject, new Point(159, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton.Exists, "Done button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Exists, "OpenQuickVariableInput button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.Exists, "Row1 does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.OnError.OnErrorGroup.Exists, "OnErrorGroup does not exist after dragging Assign Object tool on to the workflow surface");
        }

        [When(@"I Open AssignObject QVI LargeView")]
        public void Open_AssignObject_QVI_LargeView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent.Exists, "QVI on assign object is not open");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent.QviSplitOnCombobox.Exists, "QviSplitOnCombobox on assign object does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent.PreviewCustom.Exists, "Qvi PreviewCustom on assign object does not exist");
        }

        [When(@"I Open Base Conversion Tool Large View")]
        public void Open_Base_Conversion_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, new Point(160, 15));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Enabled, "Base convert large view row 1 data testbox does not exist.");
        }

        [When(@"I Open Base Conversion Tool Qvi Large View")]
        public void Open_Base_Conversion_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.Exists, "Base Conversion QVI Window does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.QuickVariableInputContent.Exists, "QVI on BaseConvert is not open");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.QuickVariableInputContent.Exists, "QVI on BaseConvert is not open");
        }

        [When(@"I Open Calculate Tool Large View")]
        public void Open_Calculate_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate, new Point(105, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Exists, "Calculate tool large view does not exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.Exists, "Autocomplete listbox does not exisst on Calculate tool large view.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.FunctionTextbox.Exists, "Function textbox does not exist on calculate tool large view.");
        }

        [When(@"I Open Case Conversion Tool Large View")]
        public void Open_Case_Conversion_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, new Point(136, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.DoneButton.Exists, "Done Button does not exist after opening Case Convert large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.SmallDataGridTable.Exists, "Inputs grid does not exist after opening Case Convert large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom group does not exist after opening Case Convert large view");
        }

        [When(@"I Open Case Conversion Tool Qvi Large View")]
        public void Open_Case_Conversion_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.Exists, "Case Conversion QVI Window does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.QuickVariableInputContent.Exists, "QVI on CaseConvert is not open");
        }

        [When(@"I Open CMD Line Tool Large View")]
        public void Open_CMD_Line_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, new Point(174, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.Exists, "CMD Line large view on the design surface does not exist");
        }

        [When(@"I Open Context Menu On Design Surface")]
        public void Open_Context_Menu_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, MouseButtons.Right, ModifierKeys.None, new Point(304, 286));
        }

        [When(@"I Open Copy Tool Large View")]
        public void Open_Copy_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy, new Point(144, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.Exists, "Copy Tool large view on the design surface does not exist");
        }

        [When(@"I Open CountRecords Large View")]
        public void Open_CountRecords_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset, new Point(130, 11));
        }

        [When(@"I Open Create JSON Large View")]
        public void Open_Create_JSON_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(124, 9));
        }

        [When(@"I Open Create Tool Large View")]
        public void Open_Create_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate, new Point(118, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.Exists, "Create Path large view on the design surface does not exist");
        }

        [When(@"I Open Data Merge Large View")]
        public void Open_Data_Merge_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(185, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data merge large view on the design surface does not exist");
        }

        [When(@"I Open Data Merge Tool Qvi Large View")]
        public void Open_Data_Merge_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data Merge QVi on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QuickVariableInputContent.Exists, "QVI on DataMerge is not open");
        }

        [When(@"I Open Data Split Large View")]
        public void Open_Data_Split_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, new Point(203, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.Exists, "Data Split large view on the design surface does not exist");
        }

        [When(@"I Open Data Split Tool Qvi Large View")]
        public void Open_Data_Split_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.Exists, "Data Split Qvi does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.QuickVariableInputContent.Exists, "QVI on DataSplit is not open");
        }

        [When(@"I Open DateTime LargeView")]
        public void Open_DateTime_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime, new Point(145, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.AddTimeAmountComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.OnErrorCustom.Exists, "ToComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.InputComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.DoneButton.Exists, "ToComboBox does not exist on the large view");
        }

        [When(@"I Open DateTimeDiff LargeView")]
        public void Open_DateTimeDiff_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference, new Point(145, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.DoneButton.Exists, "ToComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.LargeViewContentCustom.InputFormatComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.LargeViewContentCustom.Input1ComboBox.Exists, "ToComboBox does not exist on the large view");
        }

        [When(@"I Open Decision Large View")]
        public void Open_Decision_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision);
            Assert.IsTrue(DecisionOrSwitchDialog.Exists, "Decision Dialog does not exist after opening large Decision view");
        }

        [When(@"I Open Delete Tool Large View")]
        public void Open_Delete_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete, new Point(118, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.Exists, "Delete Path large view on the design surface does not exist");
        }

        [When(@"I Open DeleteRecords Large View")]
        public void Open_DeleteRecords_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord, new Point(133, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.DoneButton.Exists, "Done button does not exist after opening Delete Record large view");
        }

        [When(@"I Open DeleteWeb Tool Large View")]
        public void Open_DeleteWeb_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Web delete large view does not exist on the design surface");
        }

        [When(@"I Collapse DeleteWeb Tool Large View to Small View With Double Click")]
        public void Collapse_DeleteWeb_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Cannot collapse Web Delete tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(147, 16));
        }

        [When(@"I Open DotNet DLL Connector Tool Large View")]
        public void Open_DotNet_DLL_Connector_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll, new Point(238, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.Exists, "DotNet DLL tool large view does not exist after double clicking tool small view.");
        }

        [When(@"I Collapse DotNet DLL Connector Tool Large View to Small View With Double Click")]
        public void Collapse_DotNet_DLL_Connector_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.Exists, "Cannot collapse DotNet DLL tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll, new Point(238, 16));
        }

        [When(@"I Collapse Com DLL Connector Tool Large View to Small View With Double Click")]
        public void Collapse_Com_DLL_Connector_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.Exists, "Cannot collapse DotNet DLL tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll, new Point(238, 16));
        }

        [When(@"I Collapse WCF Service Connector Tool Large View to Small View With Double Click")]
        public void Collapse_WCF_Service_Connector_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WcfService.LargeView.Exists, "Cannot collapse WCF Service tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WcfService, new Point(238, 16));
        }

        [When(@"I Open Dropbox Delete Tool Large View With Double Click")]
        public void Open_Dropbox_Delete_Tool_Large_View_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete, new Point(174, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContentCustom.Exists, "Tool large does not exist after openning it with a double click.");
        }

        [When(@"I Open Dropbox List Contents Tool Large View With Double Click")]
        public void Open_Dropbox_List_Contents_Tool_Large_View_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList, new Point(166, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.Exists, "Tool large does not exist after openning it with a double click.");
        }

        [When(@"I Open Dropbox Upload Tool Large View With Double Click")]
        public void Open_Dropbox_Upload_Tool_Large_View_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload, new Point(151, 8));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.LargeViewContent.Exists, "Tool large does not exist after openning it with a double click.");
        }

        [When(@"I Open DropboxFileOperation Large View")]
        public void Open_DropboxFileOperation_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload, new Point(174, 14));
        }

        [When(@"I Open Exchange Email Tool Large View")]
        public void Open_Exchange_Email_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail, new Point(168, 11));
        }

        [When(@"I Open ExecuteCommandline LargeView")]
        public void Open_ExecuteCommandline_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, new Point(178, 10));
        }

        [When(@"I Open Explorer First Item Tests With Context Menu")]
        [Then(@"I Open Explorer First Item Tests With Context Menu")]
        [Given(@"I Open Explorer First Item Tests With Context Menu")]
        public void Open_Explorer_First_Item_Tests_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests does not exist in explorer context menu.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Tests, new Point(30, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run all button does not exist on tests tab");
        }

        [When(@"I Open Explorer First Item Version History With Context Menu")]
        public void Open_Explorer_First_Item_Version_History_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ShowVersionHistory, new Point(66, 15));
        }

        [When(@"I Open Explorer First Item With Context Menu")]
        public void Open_Explorer_First_Item_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(40, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open does not exist in explorer context menu.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Open);
        }

        [Given(@"I Right Click On The Folder Count")]
        [When(@"I Right Click On The Folder Count")]
        [Then(@"I Right Click On The Folder Count")]
        public void Right_Click_On_The_Folder_Count()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            #endregion

            // Right-Click 'Warewolf.Studio.ViewModels.EnvironmentViewModel' -> 'Warewolf.Studio.ViewModels.ExplorerItemViewModel' tree item
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(211, 8));
        }

        [When(@"I Open Explorer First SubItem With Context Menu")]
        public void Open_Explorer_FirstSubItem_Item_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(40, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open does not exist in explorer context menu.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Open);
        }

        [When(@"I Open Find Index Tool Large View")]
        public void Open_Find_Index_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, new Point(147, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.Exists, "Tool Large View does not exist after opening large Find Index view");
        }

        [When(@"I Open Find Record Index Tool Large View")]
        public void Open_Find_Record_Index_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, new Point(172, 5));
        }

        [When(@"I Open ForEach Large View")]
        public void Open_ForEach_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, new Point(131, 14));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.Exists, "ForEach large view does not exist after double clicking tool to open large view.");
        }

        [When(@"I Open GET Web Connector Tool Large View")]
        public void Open_GET_Web_Connector_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(238, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox.Exists, "Web GET large view does not exist after openning it with double click.");
        }

        [When(@"I Open GET Web Connector Tool Large View")]
        public void Collapse_GET_Web_Connector_Tool_Large_View_to_Small_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(238, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Open Javascript Large View")]
        public void Open_Javascript_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript, new Point(115, 14));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.ScriptIntellisenseCombobox.Exists, "Javascript script textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.AttachmentsIntellisenseCombobox.Exists, "Javascript Attachments textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.AttachFileButton.Exists, "Javascript Attach File Button does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.EscapesequencesCheckBox.Exists, "Javascript escape sequences checkbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.ResultIntellisenseCombobox.Exists, "Javascript result textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.OnErrorPane.Exists, "Javascript OnError pane does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.DoneButton.Exists, "Javascript Done button does not exist after openning large view with a double click.");
        }

        [When(@"I Open Json Tool Large View")]
        public void Open_Json_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(158, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "JSON tool large view on the design surface does not exist");
        }

        [When(@"I Open Json Tool Qvi Large View")]
        public void Open_Json_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "JSON QVI window does not exist on the design surface");
        }

        [When(@"I Open Length Tool Large View")]
        public void Open_Length_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length, new Point(136, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.RecordsetComboBox.Exists, "Recordset combobox does not exist after dragging Recordset Length on to Workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.ResultComboBox.Exists, "Result combobox does not exist after dragging Recordset Length on to Workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.OnErrorCustom.Exists, "On Error pane does not exist after dragging Recordset Length on to Workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.DoneButton.Exists, "DoneButton does not exist after dragging Recordset Length on to Workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.NullAsZeroCheckBox.Exists, "NullAsZero checkbox is does not exist after dragging Recordset Length on to Workflow surface");
        }

        [When(@"I Open Move Tool Large View")]
        public void Open_Move_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove, new Point(125, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.Exists, "Move tool large view does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom group does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox.Exists, "DestinationComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.DoneButton.Exists, "DoneButton does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
        }

        [When(@"I Open MySql Database Tool Large View")]
        public void Open_MySql_Database_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase, new Point(238, 15));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.Exists, "My SQL Database connector tool large view does not exist after openning it with a double click.");
        }

        [When(@"I Collapse MySql Database Tool Large View to Small View With Double Click")]
        public void Collapse_MySql_Database_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.Exists, "Cannot collapse tool large view to small view because large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase, new Point(238, 15));
        }

        [When(@"I Open NumberFormat Toolbox Large View")]
        public void Open_NumberFormat_Toolbox_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.DoneButton.Exists, "Done button does not exist after opening  Format Number tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.Exists, "On Error group does not exist after opening  Format Number tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.ResultInputComboBox.Exists, "Reult combobox does not exist after opening  Format Number tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.DecimalsToShowComboBox.Exists, "DecimalToShow combobox does not exist after opening  Format Number tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.Exists, "Rounding combobox does not exist after opening  Format Number tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.NumberInputComboBox.Exists, "NumberInput combobox does not exist after opening  Format Number tool large view");
        }

        [When(@"I Open ODBC Tool Large View")]
        public void Open_ODBC_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.Exists, "ODBC tool large view does not exist on the design surface.");
        }

        [When(@"I Collapse ODBC Tool Large View to Small View With Double Click")]
        public void Collapse_ODBC_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.Exists, "Cannot collapse tool large view to small view because large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom, new Point(145, 5));
        }

        [When(@"I Open Oracle Tool Large View")]
        public void Open_Oracle_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.Exists, "Oracle tool large view does not exist on the design surface.");
        }

        [When(@"I Collapse Oracle Tool Large View to Small View With Double Click")]
        public void Collapse_Oracle_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.Exists, "Cannot collapse tool large view to small view because large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom, new Point(145, 5));
        }

        [When(@"I Open Postgre Tool Large View")]
        public void Open_Postgre_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.Exists, "Postgre tool large view does not exist on the design surface.");
        }

        [When(@"I Collapse Postgre Tool Large View to Small View With Double Click")]
        public void Collapse_Postgre_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.Exists, "Cannot collapse tool large view to small view because large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom, new Point(173, 14));
        }

        [When(@"I Open PostWeb RequestTool Large View")]
        public void Open_PostWeb_RequestTool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(128, 8));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Post web request large view does not exist on design surface.");
        }

        [When(@"I Collapse PostWeb RequestTool Large View to Small View With Double Click")]
        public void Collapse_PostWeb_RequestTool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Cannot collapse post web request large view to small view because large view does not exist on design surface.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(173, 14));
        }

        [When(@"I Open PutWeb Tool large view")]
        public void Open_PutWeb_Tool_large_view()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(145, 5));
        }

        [When(@"I Collapse PutWeb RequestTool Large View to Small View With Double Click")]
        public void Collapse_PutWeb_RequestTool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Cannot collapse put web request large view to small view because large view does not exist on design surface.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(128, 8));
        }

        [When(@"I Open Python Large View")]
        public void Open_Python_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python, new Point(117, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ScriptIntellisenseCombobox.Exists, "Python script textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachmentsIntellisenseCombobox.Exists, "Python Attachments textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachFileButton.Exists, "Python Attach File Button does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.EscapesequencesCheckBox.Exists, "Python escape sequences checkbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ResultIntellisenseCombobox.Exists, "Python result textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.OnErrorPane.Exists, "Python OnError pane does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.DoneButton.Exists, "Python Done button does not exist after openning large view with a double click.");
        }

        [When(@"I Open RabbitMqConsume LargeView")]
        public void Open_RabbitMqConsume_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume, new Point(145, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.ResponseComboBox.Exists, "ResponseComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.AcknowledgeCheckBox.Exists, "AcknowledgeCheckBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.SourceComboBox.Exists, "SourceComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.DoneButton.Exists, "DoneButton does not exist on the large view");
        }

        [When(@"I Open RabbitMqPublish LargeView")]
        public void Open_RabbitMqPublish_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish, new Point(145, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.DurableCheckBox.Exists, "DurableCheckBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.NewSourceButton.Exists, "NewSourceButton does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.DoneButton.Exists, "DoneButton does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.Exists, "RabbitMQPublish does not exist on the large view");
        }

        [When(@"I Open Random Large Tool")]
        public void Open_Random_Large_Tool()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random, new Point(145, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.DoneButton.Exists, "DoneButton does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.FromComboBox.Exists, "FromComboBox does not exist on the large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.ToComboBox.Exists, "ToComboBox does not exist on the large view");
        }

        [When(@"I Open Read File Tool Large View")]
        public void Open_Read_File_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead, new Point(120, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.Exists, "Read file large view does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox.Exists, "ResultComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox.Exists, "FileNameComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.DoneButton.Exists, "DoneButton does not exist on the design surface");
        }

        [When(@"I Open Read Folder Tool Large View")]
        public void Open_Read_Folder_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead, new Point(138, 14));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.Exists, "Read Folder large view does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton.Exists, "FilesFoldersRadioButton does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom group does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.Exists, "DirectoryComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.Exists, "Read Folder large view does not exist on the design surface");
        }

        [When(@"I Open Rename Tool Large View")]
        public void Open_Rename_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename, new Point(159, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.Exists, "Rename tool large view on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox.Exists, "FileOrFolderComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.DoneButton.Exists, "DoneButton does not exist on the design surface");
        }

        [When(@"I Open Replace Tool Large View")]
        public void Open_Replace_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, new Point(159, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.DoneButton.Exists, "Done button does not exist after opening Replace tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.ResultComboBox.Exists, "Result combobox does not exist after opening Replace tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.ReplaceComboBox.Exists, "Replace combobox does not exist after opening Replace tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.FindComboBox.Exists, "Find combobox does not exist after opening Replace tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.InFiledsComboBox.Exists, "InFields combobox does not exist after opening Replace tool large view");
        }

        [When(@"I Open Ruby Large View")]
        public void Open_Ruby_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby, new Point(116, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ScriptIntellisenseCombobox.Exists, "Ruby script textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachmentsIntellisenseCombobox.Exists, "Ruby Attachments textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachFileButton.Exists, "Ruby Attach File Button does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.EscapesequencesCheckBox.Exists, "Ruby escape sequences checkbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ResultIntellisenseCombobox.Exists, "Ruby result textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.OnErrorPane.Exists, "Ruby OnError pane does not exist after openning large view with a double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.DoneButton.Exists, "Ruby Done button does not exist after openning large view with a double click.");
        }

        [When(@"I Open Selectandapply Large View")]
        public void Open_Selectandapply_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply, new Point(129, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.DoneButton.Exists, "Select and apply done button does not exist after openning tool large view with double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.SelectFromIntellisenseTextbox.Exists, "Select and apply select from textbox does not exist after openning tool large view with double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.AliasIntellisenseTextbox.Exists, "Select and apply alias textbox does not exist after openning tool large view with double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.DropActivityHere.Exists, "Select and apply activity drop box does not exist after openning tool large view with double click.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.OnErrorPane.Exists, "Select and apply OnError pane does not exist after openning tool large view with double click.");
        }

        [When(@"I Open Sequence Large tool View")]
        public void Open_Sequence_Large_tool_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence, new Point(139, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.Exists, "SequenceLargeView does not exist after opening Sequence tool large view");
        }

        [When(@"I Open Sharepoint Copy Tool Large View")]
        public void Open_Sharepoint_Copy_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile, new Point(230, 11));
        }

        [When(@"I Open Sharepoint Create Tool Large View")]
        public void Open_Sharepoint_Create_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem, new Point(195, 11));
        }

        [When(@"I Open Sharepoint Delete Tool Large View")]
        public void Open_Sharepoint_Delete_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, new Point(218, 11));
        }

        [Given(@"I Open Sharepoint Download File Tool Large View With Double Click")]
        [When(@"I Open Sharepoint Download File Tool Large View With Double Click")]
        [Then(@"I Open Sharepoint Download File Tool Large View With Double Click")]
        public void Open_Sharepoint_Download_File_Tool_Large_View_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile, new Point(185, 9));
        }

        [Given(@"I Open Sharepoint MoveFile Tool Large View")]
        [When(@"I Open Sharepoint MoveFile Tool Large View")]
        [Then(@"I Open Sharepoint MoveFile Tool Large View")]
        public void Open_Sharepoint_MoveFile_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Read Folder Tool Large View")]
        [When(@"I Open Sharepoint Read Folder Tool Large View")]
        [Then(@"I Open Sharepoint Read Folder Tool Large View")]
        public void Open_Sharepoint_Read_Folder_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Read List Item Tool Large View")]
        [When(@"I Open Sharepoint Read List Item Tool Large View")]
        [Then(@"I Open Sharepoint Read List Item Tool Large View")]
        public void Open_Sharepoint_Read_List_Item_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Update Tool Large View")]
        [When(@"I Open Sharepoint Update Tool Large View")]
        [Then(@"I Open Sharepoint Update Tool Large View")]
        public void Open_Sharepoint_Update_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Upload Tool Large View")]
        [When(@"I Open Sharepoint Upload Tool Large View")]
        [Then(@"I Open Sharepoint Upload Tool Large View")]
        public void Open_Sharepoint_Upload_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile, new Point(230, 11));
        }

        [Given(@"I Open SMTP Email Tool Large View")]
        [When(@"I Open SMTP Email Tool Large View")]
        [Then(@"I Open SMTP Email Tool Large View")]
        public void Open_SMTP_Email_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail, new Point(168, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeView.Exists, "Email Tool large view does not exist on the design surface");
        }

        [Given(@"I Open SortRecords Large View")]
        [When(@"I Open SortRecords Large View")]
        [Then(@"I Open SortRecords Large View")]
        public void Open_SortRecords_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords, new Point(114, 13));
        }

        [Given(@"I Open SQL Bulk Insert Tool Large View")]
        [When(@"I Open SQL Bulk Insert Tool Large View")]
        [Then(@"I Open SQL Bulk Insert Tool Large View")]
        public void Open_SQL_Bulk_Insert_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert, new Point(157, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.Exists, "Sql Bulk Insert large view on the design surface does not exist");
        }

        [Given(@"I Open SQL Bulk Insert Tool Qvi Large View")]
        [When(@"I Open SQL Bulk Insert Tool Qvi Large View")]
        [Then(@"I Open SQL Bulk Insert Tool Qvi Large View")]
        public void Open_SQL_Bulk_Insert_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.Exists, "Sql Bulk Insert Qvi window on the design surface does not exist");
        }

        [Given(@"I Open SQL Large View FromContextMenu")]
        [When(@"I Open SQL Large View FromContextMenu")]
        [Then(@"I Open SQL Large View FromContextMenu")]
        public void Open_SQL_Large_View_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, MouseButtons.Right, ModifierKeys.None, new Point(143, 6));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.ShowLargeView, new Point(43, 15));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton.Exists, "\"New button does not exist\"");
        }

        [Given(@"I Open Sql Server Tool Large View")]
        [When(@"I Open Sql Server Tool Large View")]
        [Then(@"I Open Sql Server Tool Large View")]
        public void Open_Sql_Server_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.Exists, "SQL Server tool large view does not exist on the design surface.");
        }

        [Given(@"I Collapse Sql Server Tool Large View to Small View With Double Click")]
        [When(@"I Collapse Sql Server Tool Large View to Small View With Double Click")]
        [Then(@"I Collapse Sql Server Tool Large View to Small View With Double Click")]
        public void Collapse_Sql_Server_Tool_Large_View_to_Small_View_With_Double_Click()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.Exists, "Cannot collapse tool large view to small view because large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, new Point(145, 5));
        }

        [Given(@"I Open Sql Server Tool small View")]
        [When(@"I Open Sql Server Tool small View")]
        [Then(@"I Open Sql Server Tool small View")]
        public void Open_Sql_Server_Tool_small_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, new Point(253, 18));
        }

        [Given(@"I Open Switch Tool Large View")]
        [When(@"I Open Switch Tool Large View")]
        [Then(@"I Open Switch Tool Large View")]
        public void Open_Switch_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch, new Point(39, 35));
            Assert.IsTrue(DecisionOrSwitchDialog.Enabled, "Switch dialog does not exist after opening switch large view");
        }

        [Given(@"I Open System Information Tool Large View")]
        [When(@"I Open System Information Tool Large View")]
        [Then(@"I Open System Information Tool Large View")]
        public void Open_System_Information_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable.Exists, "Variable Grid does not exist after opening Gather System information tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton.Exists, "Done button  does not exist after opening Gather System information tool large view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.Exists, "OnError group  does not exist after opening Gather System information tool large view");
        }

        [Given(@"I Open System Information Tool Qvi Large View")]
        [When(@"I Open System Information Tool Qvi Large View")]
        [Then(@"I Open System Information Tool Qvi Large View")]
        public void Open_System_Information_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.Exists, "System Info QVI window does not exist on the design surface");
        }

        [Given(@"I Open UniqueRecords Large View")]
        [When(@"I Open UniqueRecords Large View")]
        [Then(@"I Open UniqueRecords Large View")]
        public void Open_UniqueRecords_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique, new Point(134, 10));
        }

        [Given(@"I Open Unzip Tool Large View")]
        [When(@"I Open Unzip Tool Large View")]
        [Then(@"I Open Unzip Tool Large View")]
        public void Open_Unzip_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip, new Point(102, 14));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.Exists, "Unzip large view on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox.Exists, "UnZipNameComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.DoneButton.Exists, "DoneButton does not exist on the design surface");
        }

        [Given(@"I Open WebRequest LargeView")]
        [When(@"I Open WebRequest LargeView")]
        [Then(@"I Open WebRequest LargeView")]
        public void Open_WebRequest_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest, new Point(126, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeView.Exists, "Web request large view does not exist on design surface.");
        }

        [Given(@"I Open Write File Tool Large View")]
        [When(@"I Open Write File Tool Large View")]
        [Then(@"I Open Write File Tool Large View")]
        public void Open_Write_File_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite, new Point(149, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.Exists, "Write file large view on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox.Exists, "ContentsComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton.Exists, "OverwriteRadioButton does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.DoneButton.Exists, "DoneButton does not exist on the design surface");
        }

        [Given(@"I Open Xpath Tool Large View")]
        [When(@"I Open Xpath Tool Large View")]
        [Then(@"I Open Xpath Tool Large View")]
        public void Open_Xpath_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath, new Point(113, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.Exists, "Xpath large view does not exist on the design surface");
        }

        [Given(@"I Open Xpath Tool Qvi Large View")]
        [When(@"I Open Xpath Tool Qvi Large View")]
        [Then(@"I Open Xpath Tool Qvi Large View")]
        public void Open_Xpath_Tool_Qvi_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.OpenQuickVariableInpToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.Exists, "Xpath Qvi does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.QuickVariableInputContent.Exists, "QVI on XPath is not open");
        }

        [Given(@"I Open Zip Tool Large View")]
        [When(@"I Open Zip Tool Large View")]
        [Then(@"I Open Zip Tool Large View")]
        public void Open_Zip_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip, new Point(124, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.Exists, "Zip large view on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox.Exists, "SelectedCompressComboBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OnErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.Exists, "FileOrFolderComboBox does not exist on the design surface");
        }

        [Given(@"I Press F6")]
        [When(@"I Press F6")]
        [Given(@"I Press F6")]
        [Then(@"I Press F6")]
        public void Press_F6()
        {
            Keyboard.SendKeys(MainStudioWindow, "{F6}", ModifierKeys.None);
        }

        [Given(@"I PressF11 EnterFullScreen")]
        [When(@"I PressF11 EnterFullScreen")]
        [Then(@"I PressF11 EnterFullScreen")]
        public void PressF11_EnterFullScreen()
        {
            Keyboard.SendKeys(MainStudioWindow, "{F11}", ModifierKeys.None);
        }

        [Given(@"I RabbitMqAsserts")]
        [When(@"I RabbitMqAsserts")]
        [Then(@"I RabbitMqAsserts")]
        public void RabbitMqAsserts()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Exists, "VirtualHoast textbox does not exist after opening RabbitMq Source tab");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PasswordTextBoxEdit.Exists, "Password textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.UserNameTextBoxEdit.Exists, "Username textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.HostTextBoxEdit.Exists, "Host textbox does not exist after opening RabbitMq Source");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PortTextBoxEdit.Exists, "Port textbox does not exist after opening RabbitMq Source");
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

        [Given(@"I Rename FolderItem ToNewFolderItem")]
        [When(@"I Rename FolderItem ToNewFolderItem")]
        [Then(@"I Rename FolderItem ToNewFolderItem")]
        public void Rename_FolderItem_ToNewFolderItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Rename);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit.Text = "Control Flow - Decision2";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Then(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Given(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        public void Rename_Folder_Using_Shortcut(string newName)
        {
            var firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            Mouse.Click(firstItem);
            Keyboard.SendKeys(firstItem, "{F2}");
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Then(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Given(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        public void Rename_Remote_Folder_Using_Shortcut(string newName)
        {
            var firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem;
            Mouse.Click(firstItem);
            Keyboard.SendKeys(firstItem, "{F2}");
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Rename LocalFolder To SecondFolder")]
        [When(@"I Rename LocalFolder To SecondFolder")]
        [Then(@"I Rename LocalFolder To SecondFolder")]
        public void Rename_LocalFolder_To_SecondFolder()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Rename);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = "Example";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Delete FirstResource FromContextMenu")]
        [When(@"I Delete FirstResource FromContextMenu")]
        [Then(@"I Delete FirstResource FromContextMenu")]
        public void Delete_FirstResource_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Delete);
        }

        [Given(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        [When(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        [Then(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        public void WhenIRenameFirstRemoteResourceFromContextMenuTo(string newName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            MainStudioWindow.DrawHighlight();
            MainStudioWindow.ExplorerContextMenu.DrawHighlight();
            MainStudioWindow.ExplorerContextMenu.Rename.DrawHighlight();
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Rename);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }


        [Given(@"I Delete FirstResource FromContextMenu")]
        [When(@"I Delete FirstResource FromContextMenu")]
        [Then(@"I Delete FirstResource FromContextMenu")]
        public void Select_ShowVersionHistory_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.ShowVersionHistory);
        }

        [Given(@"I Duplicate FirstResource FromContextMenu")]
        [When(@"I Duplicate FirstResource FromContextMenu")]
        [Then(@"I Duplicate FirstResource FromContextMenu")]
        public void Duplicate_FirstResource_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Duplicate);
        }

        [Given(@"I Rename LocalWorkflow To SecodWorkFlow")]
        [When(@"I Rename LocalWorkflow To SecodWorkFlow")]
        [Then(@"I Rename LocalWorkflow To SecodWorkFlow")]
        public void Rename_LocalWorkflow_To_SecodWorkFlow()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Rename, new Point(73, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = "SecondWorkflow";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
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

        [Given(@"I RightClick BaseConvert OnDesignSurface")]
        [When(@"I RightClick BaseConvert OnDesignSurface")]
        [Then(@"I RightClick BaseConvert OnDesignSurface")]
        public void RightClick_BaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(148, 12));
        }

        [Given(@"I RightClick Calculate OnDesignSurface")]
        [When(@"I RightClick Calculate OnDesignSurface")]
        [Then(@"I RightClick Calculate OnDesignSurface")]
        public void RightClick_Calculate_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate, MouseButtons.Right, ModifierKeys.None, new Point(144, 10));
        }

        [Given(@"I RightClick CaseConvert OnDesignSurface")]
        [When(@"I RightClick CaseConvert OnDesignSurface")]
        [Then(@"I RightClick CaseConvert OnDesignSurface")]
        public void RightClick_CaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(156, 10));
        }

        [Given(@"I RightClick Comment OnDesignSurface")]
        [When(@"I RightClick Comment OnDesignSurface")]
        [Then(@"I RightClick Comment OnDesignSurface")]
        public void RightClick_Comment_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment, MouseButtons.Right, ModifierKeys.None, new Point(121, 10));
        }

        [Given(@"I RightClick Copy OnDesignSurface")]
        [When(@"I RightClick Copy OnDesignSurface")]
        [Then(@"I RightClick Copy OnDesignSurface")]
        public void RightClick_Copy_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy, MouseButtons.Right, ModifierKeys.None, new Point(104, 10));
        }

        [Given(@"I RightClick CountRecords OnDesignSurface")]
        [When(@"I RightClick CountRecords OnDesignSurface")]
        [Then(@"I RightClick CountRecords OnDesignSurface")]
        public void RightClick_CountRecords_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset, MouseButtons.Right, ModifierKeys.None, new Point(131, 10));
        }

        [Given(@"I RightClick CreateJSON OnDesignSurface")]
        [When(@"I RightClick CreateJSON OnDesignSurface")]
        [Then(@"I RightClick CreateJSON OnDesignSurface")]
        public void RightClick_CreateJSON_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, MouseButtons.Right, ModifierKeys.None, new Point(128, 9));
        }

        [Given(@"I RightClick CreateTool OnDesignSurface")]
        [When(@"I RightClick CreateTool OnDesignSurface")]
        [Then(@"I RightClick CreateTool OnDesignSurface")]
        public void RightClick_CreateTool_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate, MouseButtons.Right, ModifierKeys.None, new Point(108, 14));
        }

        [Given(@"I RightClick DataMerge OnDesignSurface")]
        [When(@"I RightClick DataMerge OnDesignSurface")]
        [Then(@"I RightClick DataMerge OnDesignSurface")]
        public void RightClick_DataMerge_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, MouseButtons.Right, ModifierKeys.None, new Point(140, 7));
        }

        [Given(@"I RightClick DataSplit OnDesignSurface")]
        [When(@"I RightClick DataSplit OnDesignSurface")]
        [Then(@"I RightClick DataSplit OnDesignSurface")]
        public void RightClick_DataSplit_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, MouseButtons.Right, ModifierKeys.None, new Point(153, 6));
        }

        [Given(@"I RightClick DateTime OnDesignSurface")]
        [When(@"I RightClick DateTime OnDesignSurface")]
        [Then(@"I RightClick DateTime OnDesignSurface")]
        public void RightClick_DateTime_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime, MouseButtons.Right, ModifierKeys.None, new Point(145, 13));
        }

        [Given(@"I RightClick DateTimeDifference OnDesignSurface")]
        [When(@"I RightClick DateTimeDifference OnDesignSurface")]
        [Then(@"I RightClick DateTimeDifference OnDesignSurface")]
        public void RightClick_DateTimeDifference_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference, MouseButtons.Right, ModifierKeys.None, new Point(174, 10));
        }

        [Given(@"I RightClick Decision OnDesignSurface")]
        [When(@"I RightClick Decision OnDesignSurface")]
        [Then(@"I RightClick Decision OnDesignSurface")]
        public void RightClick_Decision_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision, MouseButtons.Right, ModifierKeys.None, new Point(28, 22));
        }

        [Given(@"I RightClick Delete OnDesignSurface")]
        [When(@"I RightClick Delete OnDesignSurface")]
        [Then(@"I RightClick Delete OnDesignSurface")]
        public void RightClick_Delete_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete, MouseButtons.Right, ModifierKeys.None, new Point(100, 10));
        }

        [Given(@"I RightClick DeleteRecord OnDesignSurface")]
        [When(@"I RightClick DeleteRecord OnDesignSurface")]
        [Then(@"I RightClick DeleteRecord OnDesignSurface")]
        public void RightClick_DeleteRecord_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord, MouseButtons.Right, ModifierKeys.None, new Point(116, 9));
        }

        [When(@"I RightClick DotNetDllConnector OnDesignSurface")]
        public void RightClick_DotNetDllConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll, MouseButtons.Right, ModifierKeys.None, new Point(164, 10));
        }

        [Given(@"I RightClick DropboxFileOperation OnDesignSurface")]
        [When(@"I RightClick DropboxFileOperation OnDesignSurface")]
        [Then(@"I RightClick DropboxFileOperation OnDesignSurface")]
        public void RightClick_DropboxFileOperation_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload, MouseButtons.Right, ModifierKeys.None, new Point(181, 11));
        }

        [Given(@"I RightClick Email OnDesignSurface")]
        [When(@"I RightClick Email OnDesignSurface")]
        [Then(@"I RightClick Email OnDesignSurface")]
        public void RightClick_Email_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail, MouseButtons.Right, ModifierKeys.None, new Point(129, 11));
        }

        [Given(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [When(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [Then(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        public void RightClick_ExecuteCommandLine_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, MouseButtons.Right, ModifierKeys.None, new Point(165, 13));
        }

        [Given(@"I RightClick Explorer First Remote Server First Item")]
        [When(@"I RightClick Explorer First Remote Server First Item")]
        [Then(@"I RightClick Explorer First Remote Server First Item")]
        public void RightClick_Explorer_First_Remote_Server_First_Item()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
        }

        [Given(@"I RightClick Explorer Localhost First Item")]
        [When(@"I RightClick Explorer Localhost First Item")]
        [Then(@"I RightClick Explorer Localhost First Item")]
        public void RightClick_Explorer_Localhost_First_Item()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open does not exist in explorer context menu.");
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.ShowDependencies.Exists, "ShowDependencies does not exist in explorer context menu.");
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Delete.Exists, "Delete does not exist in ExplorerContextMenu");
        }

        [Given(@"I RightClick Localhost")]
        [When(@"I RightClick Localhost")]
        [Then(@"I RightClick Localhost")]
        public void RightClick_Localhost()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I RightClick Save Dialog Localhost")]
        [When(@"I RightClick Save Dialog Localhost")]
        [Then(@"I RightClick Save Dialog Localhost")]
        public void RightClick_Save_Dialog_Localhost()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem.Exists);
        }

        [Given(@"I RightClick Save Dialog Localhost First Item")]
        [When(@"I RightClick Save Dialog Localhost First Item")]
        [Then(@"I RightClick Save Dialog Localhost First Item")]
        public void RightClick_Save_Dialog_Localhost_First_Item()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [When(@"I RightClick FindIndex OnDesignSurface")]
        public void RightClick_FindIndex_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, MouseButtons.Right, ModifierKeys.None, new Point(113, 8));
        }

        [When(@"I RightClick FindRecordIndex OnDesignSurface")]
        public void RightClick_FindRecordIndex_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, MouseButtons.Right, ModifierKeys.None, new Point(191, 11));
        }

        [When(@"I RightClick ForEach OnDesignSurface")]
        public void RightClick_ForEach_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, MouseButtons.Right, ModifierKeys.None, new Point(137, 9));
        }

        [When(@"I RightClick FormatNumber OnDesignSurface")]
        public void RightClick_FormatNumber_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber, MouseButtons.Right, ModifierKeys.None, new Point(143, 9));
        }

        [When(@"I RightClick Length OnDesignSurface")]
        public void RightClick_Length_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length, MouseButtons.Right, ModifierKeys.None, new Point(97, 10));
        }

        [When(@"I RightClick Move OnDesignSurface")]
        public void RightClick_Move_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove, MouseButtons.Right, ModifierKeys.None, new Point(98, 11));
        }

        [When(@"I RightClick MySQLConnector OnDesignSurface")]
        public void RightClick_MySQLConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase, MouseButtons.Right, ModifierKeys.None, new Point(202, 10));
        }

        [When(@"I RightClick New Workflow Tab")]
        public void RightClick_New_Workflow_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, MouseButtons.Right, ModifierKeys.None, new Point(63, 18));
        }

        [When(@"I RightClick Random OnDesignSurface")]
        public void RightClick_Random_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random, MouseButtons.Right, ModifierKeys.None, new Point(107, 13));
        }

        [When(@"I RightClick ReadFile OnDesignSurface")]
        public void RightClick_ReadFile_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead, MouseButtons.Right, ModifierKeys.None, new Point(99, 14));
        }

        [When(@"I RightClick ReadFolder OnDesignSurface")]
        public void RightClick_ReadFolder_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead, MouseButtons.Right, ModifierKeys.None, new Point(115, 12));
        }

        [When(@"I RightClick Rename OnDesignSurface")]
        public void RightClick_Rename_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename, MouseButtons.Right, ModifierKeys.None, new Point(103, 7));
        }

        [When(@"I RightClick Replace OnDesignSurface")]
        public void RightClick_Replace_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, MouseButtons.Right, ModifierKeys.None, new Point(100, 7));
        }

        [When(@"I RightClick Sequence OnDesignSurface")]
        public void RightClick_Sequence_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence, MouseButtons.Right, ModifierKeys.None, new Point(119, 8));
        }

        [Given(@"I RightClick Ardoner Hyperlink")]
        [When(@"I RightClick Ardoner Hyperlink")]
        [Then(@"I RightClick Ardoner Hyperlink")]
        public void RightClick_Adorner_Control()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.NumbernHyperlink, MouseButtons.Right, ModifierKeys.None, new Point(88, 12));
        }

        [When(@"I RightClick SharepointCreateListItem OnDesignSurface")]
        public void RightClick_SharepointCreateListItem_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Copy, MouseButtons.Right, ModifierKeys.None, new Point(199, 12));
        }

        [When(@"I RightClick SharepointDelete OnDesignSurface")]
        public void RightClick_SharepointDelete_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, MouseButtons.Right, ModifierKeys.None, new Point(217, 8));
        }

        [When(@"I RightClick SharepointRead OnDesignSurface")]
        public void RightClick_SharepointRead_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, MouseButtons.Right, ModifierKeys.None, new Point(203, 9));
        }

        [When(@"I RightClick SharepointUpdate OnDesignSurface")]
        public void RightClick_SharepointUpdate_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate, MouseButtons.Right, ModifierKeys.None, new Point(210, 5));
        }

        [When(@"I RightClick SortRecords OnDesignSurface")]
        public void RightClick_SortRecords_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords, MouseButtons.Right, ModifierKeys.None, new Point(118, 8));
        }

        [When(@"I RightClick SQLConnector OnDesignSurface")]
        public void RightClick_SQLConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert, MouseButtons.Right, ModifierKeys.None, new Point(143, 6));
        }

        [When(@"I RightClick SqlServerConnector OnDesignSurface")]
        public void RightClick_SqlServerConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, MouseButtons.Right, ModifierKeys.None, new Point(198, 8));
        }

        [When(@"I RightClick Switch OnDesignSurface")]
        public void RightClick_Switch_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch, MouseButtons.Right, ModifierKeys.None, new Point(46, 15));
        }

        [When(@"I RightClick Unzip OnDesignSurface")]
        public void RightClick_Unzip_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox, MouseButtons.Right, ModifierKeys.None, new Point(101, 10));
        }

        [When(@"I RightClick WebRequest OnDesignSurface")]
        public void RightClick_WebRequest_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest, MouseButtons.Right, ModifierKeys.None, new Point(165, 8));
        }

        [When(@"I RightClick WriteFile OnDesignSurface")]
        public void RightClick_WriteFile_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite, MouseButtons.Right, ModifierKeys.None, new Point(96, 12));
        }

        [When(@"I RightClick XPath OnDesignSurface")]
        public void RightClick_XPath_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath, MouseButtons.Right, ModifierKeys.None, new Point(99, 8));
        }

        [When(@"I RightClick Zip OnDesignSurface")]
        public void RightClick_Zip_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip, MouseButtons.Right, ModifierKeys.None, new Point(95, 12));
        }

        [When(@"I Search And Select DiceRoll")]
        public void Search_And_Select_DiceRoll()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox, new Point(165, 9));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = "Dice Roll";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }

        [When(@"I Search And Select HelloWolrd")]
        public void Search_And_Select_HelloWolrd()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox, new Point(165, 9));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = "Hello World";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }

        [When(@"I Select AcceptanceTestin create")]
        public void Select_AcceptanceTestin_create()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [When(@"I Select Action")]
        public void Select_Action()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1, new Point(137, 7));
        }

        [When(@"I Select Action From PostgreTool")]
        public void Select_Action_From_PostgreTool()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDatabaseSource, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.ActionsComboBox, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.LargeDataGridTable.Enabled, "Inputs grid is not enabled after selecting an Action.");
        }

        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Select AppData From MethodList From ReadTool")]
        public void Select_AppData_From_MethodList_From_ReadTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox.TestingDb, new Point(43, 15));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.TableNameComboBox.Enabled, "Table combobox is not Enabled after selecting the database");
        }

        [Given(@"I Select Delete FromExplorerContextMenu")]
        [When(@"I Select Delete FromExplorerContextMenu")]
        [Then(@"I Select Delete FromExplorerContextMenu")]
        public void Select_Delete_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Delete, new Point(87, 12));
            Assert.IsTrue(MessageBoxWindow.YesButton.Exists, "Message box Yes button does not exist");
        }

        [When(@"I Select DeleteRow FromContextMenu")]
        public void Select_DeleteRow_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRow, new Point(74, 9));
        }

        [Given(@"I Select Deploy FromExplorerContextMenu")]
        [When(@"I Select Deploy FromExplorerContextMenu")]
        [Then(@"I Select Deploy FromExplorerContextMenu")]
        public void Select_Deploy_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Deploy, new Point(57, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "DeployTab does not exist after clicking Deploy");
        }

        [When(@"I Select Dev2TestingDB From DB Source Wizard Database Combobox")]
        public void Select_Dev2TestingDB_From_DB_Source_Wizard_Database_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox, new Point(221, 9));
            Mouse.Click(MainStudioWindow.ComboboxListItemAsDev2TestingDB, new Point(129, 19));
            Assert.AreEqual("Dev2TestingDB", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.UIDatabaseComboxBoxCustom.UIDev2TestingDBText.DisplayText);
        }

        [When(@"I Select First Item From DotNet DLL Large View Source Combobox")]
        public void Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.ListItem1, new Point(163, 17));
        }

        [When(@"I Select FirstItem From DotNet DLL Large View Action Combobox")]
        public void Select_FirstItem_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1, new Point(137, 7));
            Assert.AreEqual("Dev2.Common.Interfaces.PluginAction", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select GetCountries From SQL Server Large View Action Combobox")]
        public void Select_GetCountries_From_SQL_Server_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox.GetCountriesListItem, new Point(137, 7));
            Assert.AreEqual("dbo.GetCountries", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox.SelectedItem, "GetCountries is not selected in SQL server large view action combobox.");
        }

        [When(@"I Select GUID From Random Type Combobox")]
        public void Select_GUID_From_Random_Type_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox, new Point(133, 10));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox.GUID, new Point(31, 16));
        }

        [When(@"I Select http From Server Source Wizard Address Protocol Dropdown")]
        public void Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown, new Point(54, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsHttp, new Point(31, 12));
            Assert.AreEqual("http", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.HttpSelectedItemText.DisplayText, "Server source wizard address protocol is not equal to http.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.AddressEditBox.Exists, "Server source wizard address textbox does not exist");
        }

        [When(@"I Select InsertRow FromContextMenu")]
        public void Select_InsertRow_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.InsertRow, new Point(66, 19));
        }

        [When(@"I Select Letters From Random Type Combobox")]
        public void Select_Letters_From_Random_Type_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox, new Point(133, 10));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox.Letters, new Point(31, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.LengthComboBox.Exists, "Length combobox does not exist after selecting Letters as Random Type");
        }

        [When(@"I Select LocalhostConnected From Deploy Tab Destination Server Combobox")]
        public void Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(226, 13));
            Assert.AreEqual("Remote Connection Integration", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText.DisplayText, "Selected source server in deploy is not Remote Connection Integration.");
        }

        [When(@"I Select LoggingTab")]
        public void Select_LoggingTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab, new Point(57, 7));
        }

        [When(@"I Select Months From AddTime Type")]
        public void Select_Months_From_AddTime_Type()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox.Months, new Point(163, 17));
        }

        [When(@"I Select MSSQLSERVER From DB Source Wizard Address Protocol Dropdown")]
        public void Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.ToggleButton, new Point(625, 11));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsMicrosoftSQLServer.MicrosoftSQLServerText.Exists, "Microsoft SQL Server does not exist as an option in new DB source wizard type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsMicrosoftSQLServer.MicrosoftSQLServerText, new Point(118, 6));
            Assert.AreEqual("Microsoft SQL Server", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.MicrosoftSQLServer.DisplayText, "Microsoft SQL Server is not selected in DB source wizard.");
        }

        [When(@"I Select Namespace")]
        public void Select_Namespace()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject, new Point(137, 7));
        }

        [Given(@"I Select New Folder From SaveDialog ExplorerContextMenu")]
        [When(@"I Select New Folder From SaveDialog ExplorerContextMenu")]
        [Then(@"I Select New Folder From SaveDialog ExplorerContextMenu")]
        public void Select_NewFolder_From_SaveDialog_ExplorerContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem);
        }

        [Given(@"I Select New_Folder From SaveDialog ExplorerContextMenu")]
        [When(@"I Select New_Folder From SaveDialog ExplorerContextMenu")]
        [Then(@"I Select New_Folder From SaveDialog ExplorerContextMenu")]
        public void Select_New_Folder_From_SaveDialog_ExplorerContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem);
        }

        [Given(@"I Select Rename From SaveDialog ExplorerContextMenu")]
        [When(@"I Select Rename From SaveDialog ExplorerContextMenu")]
        [Then(@"I Select Rename From SaveDialog ExplorerContextMenu")]
        public void Select_Rename_From_SaveDialog_ExplorerContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem);
        }

        [Given(@"I Select Delete From SaveDialog ExplorerContextMenu")]
        [When(@"I Select Delete From SaveDialog ExplorerContextMenu")]
        [Then(@"I Select Delete From SaveDialog ExplorerContextMenu")]
        public void Select_Delete_From_SaveDialog_ExplorerContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem);
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeleteConfirmation.Exists);
        }

        [When(@"I Select NewDatabaseSource FromExplorerContextMenu")]
        public void Select_NewDatabaseSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDatabaseSource, new Point(72, 14));
        }

        [When(@"I Select NewDatabaseSource FromSqlServerTool")]
        public void Select_NewDatabaseSource_FromSqlServerTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton, new Point(16, 13));
            Assert.AreEqual("Microsoft SQL Server", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.MicrosoftSQLServer.DisplayText, "Microsoft SQL Server is not selected in DB source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.Exists, "User name testbox does not exist on db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.Exists, "Password textbox does not exist on database source wizard.");
        }

        [When(@"I Select NewDropboxSource FromExplorerContextMenu")]
        public void Select_NewDropboxSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDropboxSource, new Point(119, 15));
        }

        [When(@"I Select NewEmailSource FromExplorerContextMenu")]
        public void Select_NewEmailSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(MainStudioWindow.ExplorerEnvironmentContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(MainStudioWindow.ExplorerEnvironmentContextMenu.NewEmailSource, new Point(101, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.Exists, "New email source tab does not exist after opening Email source tab");
        }

        [When(@"I Select NewExchangeSource FromExplorerContextMenu")]
        public void Select_NewExchangeSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(MainStudioWindow.ExplorerEnvironmentContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(MainStudioWindow.ExplorerEnvironmentContextMenu.NewExchangeSource, new Point(101, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.Exists, "New exchange source tab does not exist after opening Email source tab");
        }

        [When(@"I Select NewPluginSource FromExplorerContextMenu")]
        public void Select_NewPluginSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SystemRandomListItem, new Point(78, 11));
        }

        [When(@"I Select NewServerSource FromExplorerContextMenu")]
        public void Select_NewServerSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewServerSource, new Point(44, 13));
        }

        [When(@"I Select NewSharepointSource FromExplorerContextMenu")]
        public void Select_NewSharepointSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewSharepointSource, new Point(126, 17));
        }

        [When(@"I Select NewSharepointSource FromServer Lookup")]
        public void Select_NewSharepointSource_FromServer_Lookup()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointCopyFile Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointMoveFile Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointUpload Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.SourceCombobox, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.SourceCombobox, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select NewWebSource FromExplorerContextMenu")]
        public void Select_NewWebSource_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewWebServiceSource, new Point(82, 20));
        }

        [Given(@"I Select NewWorkflow FromExplorerContextMenu")]
        [When(@"I Select NewWorkflow FromExplorerContextMenu")]
        [Then(@"I Select NewWorkflow FromExplorerContextMenu")]
        public void Select_NewWorkflow_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.NewWorkflow);
        }

        [When(@"I Select NewWorkFlowService From ContextMenu")]
        public void Select_NewWorkFlowService_From_ContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));
            Assert.IsTrue(MainStudioWindow.ExplorerEnvironmentContextMenu.NewWorkflow.Enabled, "NewWorkFlowService button is disabled.");
            Mouse.Click(MainStudioWindow.ExplorerEnvironmentContextMenu.NewWorkflow, new Point(79, 13));
        }

        [When(@"I Select Next From DotNet DLL Large View Action Combobox")]
        public void Select_Next_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.NextListItem, new Point(137, 7));
            Assert.AreEqual("Next", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select Open FromExplorerContextMenu")]
        public void Select_Open_FromExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Open, new Point(30, 11));
        }

        [When(@"I Select OutputIn Days")]
        public void Select_OutputIn_Days()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox.Days, new Point(114, 13));
        }

        [When(@"I Select Paste FromContextMenu")]
        public void Select_Paste_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Paste, new Point(52, 16));
        }

        [When(@"I Select PerfomanceCounterTab")]
        public void Select_PerfomanceCounterTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab, new Point(124, 14));
        }

        [Given(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        [Then(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText.Exists, "Selected destination server in deploy is not Remote Connection Integration.");
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
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
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

        [When(@"I Select RemoteConnectionIntegration From Explorer")]
        public void Select_RemoteConnectionIntegration_From_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "RemoteConnectionIntegration item does not exist in remote server combobox list.");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(138, 6));
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

        [When(@"I Select Round Up")]
        public void Select_Round_Up()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.RoungUP, new Point(114, 13));
        }

        [When(@"I Select RoundingType None")]
        public void Select_RoundingType_None()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.None, new Point(114, 13));
        }

        [When(@"I Select RoundingType Normal")]
        public void Select_RoundingType_Normal()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.Normal, new Point(114, 13));
        }

        [When(@"I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist")]
        public void Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV, new Point(97, 17));
            Assert.AreEqual("RSAKLFSVRGENDEV", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text, "RSAKLFSVRGENDEV is not selected as the server in the DB source wizard.");
        }

        [When(@"I Select SaveAsImage FromContextMenu")]
        public void Select_SaveAsImage_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.SaveasImage, new Point(38, 15));
        }

        [When(@"I Select SecurityTab")]
        public void Select_SecurityTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab, new Point(102, 10));
        }

        [When(@"I Select SetAsStartNode FromContextMenu")]
        public void Select_SetAsStartNode_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.SetasStartNode, new Point(67, 16));
        }

        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
        }

        [When(@"I Select SharepointTestServer From SharepointRead Tool")]
        public void Select_SharepointTestServer_From_SharepointRead_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [When(@"I Select SharepointTestServer From SharepointUpdate Tool")]
        public void Select_SharepointTestServer_From_SharepointUpdate_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [When(@"I Select ShowLargeView FromContextMenu")]
        public void Select_ShowLargeView_FromContextMenu()
        {
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.ShowLargeView, new Point(43, 15));
        }

        [When(@"I Select Source From DotnetTool")]
        public void Select_Source_From_DotnetTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.DotNetSource, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled, "ClassNameComboBox is not Enabled after selecting a source");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.AssemblyLocationGACCListItem, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Enabled, "ActionsComboBox is not Enabled after selecting ClassName");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.EqualsAction, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton.Enabled, "GenerateOutputsButton is not Enabled after selecting an Action");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1.Enabled, "InputsDataGridTable is not Enabled after selecting an Action");
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
        }

        [When(@"I Select Source From PostgreTool")]
        public void Select_Source_From_PostgreTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.SourcesComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.SourcesComboBox, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.ActionsComboBox.Enabled, "Action combobox is not enabled after selecting an Action.");
        }

        [When(@"I Select SystemObject From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject, new Point(137, 7));
            Assert.AreEqual("{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
                            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Object\",\"MethodName\":nul" +
                            "l}", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SelectedItem, "System.Object is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select SystemRandom From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SystemRandomListItem, new Point(137, 7));
            Assert.AreEqual("{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
                            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Random\",\"MethodName\":nul" +
                            "l}", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select Tests From Context Menu")]
        public void Select_Tests_From_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Tests, new Point(46, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.Exists, "TestsTabPage does not exist after clicking view tests in the explorer context menu.");
        }

        [When(@"I Select ToString From DotNet DLL Large View Action Combobox")]
        public void Select_ToString_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1, new Point(137, 7));
            Assert.AreEqual("ToString", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select TSTCIREMOTE From Server Source Wizard Dropdownlist")]
        public void Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.TSTCIREMOTE, new Point(70, 19));
            Assert.AreEqual("TST-CI-REMOTE", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.AddressEditBox.Text, "Server source address textbox text does not equal TST-CI-REMOTE");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.TestConnectionButton.Exists, "Server source wizard does not contain a test connection button");
        }

        [When(@"I Select UITestingDBSource From SQL Server Large View Source Combobox")]
        public void Select_UITestingDBSource_From_SQL_Server_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.UITestingDBSourceListItem, new Point(137, 7));
            Assert.AreEqual("UITestingDBSource", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.SelectedItem, "SQL Server large view source combobox selected item is not equal to UITestingDBSource.");
        }

        [When(@"I Select UITestingSource From Web Server Large View Source Combobox")]
        public void Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.UITesting, new Point(137, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton.Enabled, "Delete Web large view source combobox EDIT button is disabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Enabled, "Delete Web large view source combobox GenerateOutput button is disabled.");
        }

        [When(@"I Select User From RunTestAs")]
        public void Select_User_From_RunTestAs()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit.Exists, "Username textbox does not exist after clicking RunAsUser radio button");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit.Exists, "Password textbox does not exist after clicking RunAsUser radio button");
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save Ribbon Menu buton is disabled after changing test");
        }

        [When(@"I Select Zip Compression")]
        public void Select_Zip_Compression()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox.NormalDefault, new Point(114, 13));
        }

        [When(@"I Type 0 Into SQL Server Large View Inputs Row1 Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.InputsTable.Row1.DataCell.DataCombobox.DataTextbox.Text = "0";
            Assert.AreEqual("0", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.InputsTable.Row1.DataCell.DataCombobox.DataTextbox.Text, "SQL Server large view inputs row 1 data textbox text is not equal to S");
        }

        [When(@"I Type 0 Into SQL Server Large View Test Inputs Row1 Test Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Text = "0";
            Assert.AreEqual("0", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Text, "SQL Server large view test inputs row 1 test data textbox text is not equal to S");
        }

        [When(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        public void Type_rsaklfsvrgen_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "rsaklfsvrgen";
        }

        [Given(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        [Then(@"RSAKLFSVRGENDEV appears as an option in the DB source wizard server combobox")]
        public void Assert_RSAKLFSVRGENDEV_appears_as_an_option_in_the_DB_source_wizard_server_combobox()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV.Exists, "RSAKLFSVRGENDEV does not exist as an option in DB source wizard server combobox.");
        }

        [When(@"I Type RSAKLFSVRGENDEV into DB Source Wizard Server Textbox")]
        public void Type_RSAKLFSVRGENDEV_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "RSAKLFSVRGENDEV";
        }

        [When(@"I Type The Testing Site into Web GET Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_GET_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Get";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web get address.");
        }

        [When(@"I Type The Testing Site into Web POST Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_POST_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Post";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web post address.");
        }

        [When(@"I Type The Testing Site into Web DELETE Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_DELETE_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Delete";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web delete address.");
        }

        [When(@"I Type The Testing Site into Web PUT Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_PUT_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Put";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web put address.");
        }

        [When(@"I Click Decision Large View Match Type Combobox")]
        public void Click_Decision_Large_View_Match_Type_Combobox()
        {
            Mouse.Click(DecisionOrSwitchDialog.LargeView.Table.Row1.MatchTypeCell.MatchTypeCombobox, new Point(5, 5));
        }

        [When(@"I Make Workflow Savable")]
        public void Make_Workflow_Savable()
        {
            Drag_Toolbox_Comment_Onto_DesignSurface();
        }

        public void Move_Assign_Message_Tool_On_The_Design_Surface()
        {
            var multiAssign1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            multiAssign1.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(multiAssign1, new Point(94, 11));
            Mouse.StopDragging(multiAssign1, 70, 3);
        }

        [When(@"I Drag Explorer First Sub Item Onto Second Sub Item")]
        public void Drag_Explorer_First_Sub_Item_Onto_Second_Sub_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(94, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem, new Point(90, 7));
            Playback.Wait(2000);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Drag Explorer Second Sub Item Onto Third Sub Item")]
        public void Drag_Explorer_Second_Sub_Item_Onto_Third_Sub_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ThirdSubItem.EnsureClickable(new Point(90, 7));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ThirdSubItem.DrawHighlight();
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ThirdSubItem);
            Playback.Wait(2000);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Click Explorer Localhost First Item Expander")]
        public void Click_Explorer_Localhost_First_Item_Expander()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExpansionToggleButton);
        }

        [When(@"I Expand Explorer Localhost First Item By Double Click")]
        [Given(@"I Expand Explorer Localhost First Item By Double Click")]
        [Then(@"I Expand Explorer Localhost First Item By Double Click")]
        public void Expand_Explorer_Localhost_First_Item_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [When(@"I Click Web Browser Error Messagebox OK Button")]
        public void Click_Web_Browser_Error_Messagebox_OK_Button()
        {
            Mouse.Click(WebBrowserErrorWindow.Pane.OKButton, new Point(30, 8));
        }

        [When(@"I Click Sql Server Tool Large View New Source Button")]
        public void Click_Sql_Server_Tool_Large_View_New_Source_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton, new Point(16, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "New DB source wizard tab does not exist after openning it from the SQL Server db connector tool.");
        }

        [Then(@"I Click Create Test From Debug")]
        [Given(@"I Click Create Test From Debug")]
        [When(@"I Click Create Test From Debug")]
        public void Click_Create_Test_From_Debug()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CreateTestFromDebugButton, new Point(5, 5));
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.Exists, "Test tab does not exist after clicking Create Test from debug button");
        }

        public void Click_MockRadioButton_On_AssignValue_TestStep()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.ColumnHeadersPrHeader.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton.DrawHighlight();
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
            Assert.IsTrue(((WpfComboBox)MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.Row1.AssertOperatorCell.AssertOperatorComboBox).Enabled, "Operator combobox is still enabled");
        }

        public void Try_Click_Create_New_Tests()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        public void EnterOutMessageValue_On_OutputMessage_TestStep(string message)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.SmallDataGridTable.Row1.AssertValueCell.AssertValue.Text = message;
        }

        public void Click_Delete_On_AssignValue_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.AssertHeader.DeleteAssertButton, new Point(5, 5));
        }

        [When(@"I Click AssigName From DesignSurface")]
        public void Click_AssigName_From_DesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.DsfMultiAssignActiviCustom, new Point(5, 5));
        }

        public int Expand_Comment_Tool_Size()
        {
            var defaultHeight = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.EnsureClickable(new Point(226, 335));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.ItemResizer);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom, new Point(0, 350));
            var newHeight = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height;

            Assert.IsTrue(newHeight > defaultHeight, "Comment tool height has not changed after dragging the resize indicator downward.");
            return newHeight;
        }

        public void Click_MockRadioButton_On_Decision_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.SmallDataGridTable.Row1.AssertOperatorCell.AssertOperatorComboBox.Enabled, "Operator combobox is still enabled");
        }

        [When(@"I Expand Debug Output Recordset")]
        public void Expand_Debug_Output_Recordset()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.Expanded = true;
        }

        [Then(@"The GetCountries Recordset Is Visible in Debug Output")]
        public void ThenTheDebugOutputShowsGetCountriesRecordset()
        {
            Assert.AreEqual("[[dbo_GetCountries(204).CountryID]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetName.DisplayText, "Wrong recordset name in debug output for new DB connector.");
            Assert.AreEqual("155", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.RecordsetGroup.RecordsetValue.DisplayText, "Wrong recordset value in debug output for new DB connector.");
        }
        [Given(@"I have Hello World workflow on the Explorer")]
        [When(@"I have Hello World workflow on the Explorer")]
        [Then(@"I have Hello World workflow on the Explorer")]
        public void GivenIHaveHelloWorldWorkflowOnTheExplorer()
        {
            Filter_Explorer("Hello World");
        }

        [Given(@"I Click The Create ""(.*)""th test Button")]
        [When(@"I Click The Create ""(.*)""th test Button")]
        [Then(@"I Click The Create ""(.*)""th test Button")]
        public void GivenIClickTheCreateThTestButton(int testIntance)
        {
            Click_Create_New_Tests(true, testIntance);
        }

        [Then(@"Message box window appears")]
        [When(@"Message box window appears")]
        [Given(@"Message box window appears")]
        public void ThenMessageBoxWindowAppears()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
        }

        [Then(@"Test tab is open")]
        [Given(@"Test tab is open")]
        [When(@"Test tab is open")]
        public void ThenTestTabIsOpen()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.Exists);
        }

        [Then(@"I Click Close Clean Workflow Tab")]
        [When(@"I Click Close Clean Workflow Tab")]
        [Given(@"I Click Close Clean Workflow Tab")]
        public void ThenIClickCloseCleanWorkflowTab()
        {
            Click_Close_Workflow_Tab_Button();
        }

        [Then(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        [When(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        [Given(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        public void ThenIClickRunThTestExpecting(int testInstance, string status)
        {
            var statusEnum = GetStatus(status);
            Click_Run_Test_Button(statusEnum, testInstance);
        }
        private TestResultEnum GetStatus(string status)
        {
            if (status == "Pending")
                return TestResultEnum.Pending;
            else if (status == "Invalid")
                return TestResultEnum.Invalid;
            else if (status == "Fail")
                return TestResultEnum.Fail;
            else
                return TestResultEnum.Pass;
        }


        [Then(@"I Enter ""(.*)"" in the Output test step")]
        [When(@"I Enter ""(.*)"" in the Output test step")]
        [Given(@"I Enter ""(.*)"" in the Output test step")]
        public void ThenIEnterInTheOutputTestStep(string output)
        {
            EnterOutMessageValue_On_OutputMessage_TestStep(output);
        }

        [When(@"I Click Test Tab")]
        [Then(@"I Click Test Tab")]
        [Given(@"I Click Test Tab")]
        public void WhenIClickTestTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage);
        }

        [Then(@"The Test step in now ""(.*)""")]
        [When(@"The Test step in now ""(.*)""")]
        [Given(@"The Test step in now ""(.*)""")]
        public void ThenTheTestStepInNow(string status)
        {
            Assert.AreEqual(TestResultEnum.Invalid, GetStatus(status));
        }

        [Then(@"I Click Run all tests button")]
        [When(@"I Click Run all tests button")]
        [Given(@"I Click Run all tests button")]
        public void ThenIClickRunAllTestsButton()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton.DrawHighlight();
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton);
        }

        [Then(@"I Click workflow tab")]
        [Given(@"I Click workflow tab")]
        [When(@"I Click workflow tab")]
        public void ThenIClickWorkflowTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Exists);
        }

        [Then(@"I Enter ""(.*)"" in the Assign message tool")]
        [When(@"I Enter ""(.*)"" in the Assign message tool")]
        [Given(@"I Enter ""(.*)"" in the Assign message tool")]
        public void ThenIEnterInTheAssignMessageTool(string message)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = message;
        }


        /// <summary>
        /// Create_New_Folder_Using_Shortcut - Use 'Create_New_Folder_Using_ShortcutParams' to pass parameters into this method.
        /// </summary>
        public void Create_New_Folder_Using_Shortcut()
        {
            #region Variable Declarations
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            #endregion

            Mouse.Click(localhost, new Point(74, 8));

            Keyboard.SendKeys(localhost, "F", (ModifierKeys.Control | ModifierKeys.Shift));
        }

        public void Create_New_Workflow_In_Explorer_First_Item_With_Shortcut()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            #endregion

            Mouse.Click(firstItem, new Point(74, 8));

            Keyboard.SendKeys(firstItem, "W", (ModifierKeys.Control));
        }

        public void Create_New_Workflow_Using_Shortcut()
        {
            #region Variable Declarations
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            #endregion

            Mouse.Click(localhost, new Point(74, 8));

            Keyboard.SendKeys(localhost, "W", (ModifierKeys.Control));
        }

        public void Open_Deploy_Using_Shortcut()
        {
            Keyboard.SendKeys("D", (ModifierKeys.Control));
        }

        public void Save_Workflow_Using_Shortcut()
        {
            #region Variable Declarations
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            #endregion

            Mouse.Click(flowchart, new Point(74, 8));

            Keyboard.SendKeys(flowchart, "S", (ModifierKeys.Control));
        }

        [Given(@"I am connected on a remote server")]
        [When(@"I am connected on a remote server")]
        [Then(@"I am connected on a remote server")]
        public void GivenIAmConnectedOnARemoteServer()
        {
            Select_RemoteConnectionIntegration_From_Explorer();
            Click_Explorer_RemoteServer_Connect_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists);
        }

        [Then(@"Remote Server Refreshes")]
        public void ThenRemoteServerRefreshes()
        {
            Assert.IsTrue(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner));
            Assert.IsTrue(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Spinner));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Spinner));
        }

        [Then(@"Filtered Resourse Is Checked For Deploy")]
        public void ThenFilteredResourseIsCheckedForDeploy()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked);
        }

        [Then(@"Deploy Button Is Enabled")]
        public void ThenDeployButtonIsEnabled()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.DeployButton.Enabled);
        }

        [Then(@"Deploy Version Conflict Window Shows")]
        public void ThenDeployVersionConflictWindowShows()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeployVersionConflicText.Exists);
        }

        [Given(@"I Click MessageBox Cancel")]
        [When(@"I Click MessageBox Cancel")]
        [Then(@"I Click MessageBox Cancel")]
        public void ThenIClickMessageBoxCancel()
        {
            Mouse.Click(MessageBoxWindow.CancelButton);
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

        [Given(@"Filter Textbox is cleared")]
        [When(@"Filter Textbox is cleared")]
        [Then(@"Filter Textbox is cleared")]
        public void ThenFilterTextboxIsCleared()
        {
            Assert.IsTrue(string.IsNullOrEmpty(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text));
        }

        [Given(@"Filter Textbox has ""(.*)""")]
        [When(@"Filter Textbox has ""(.*)""")]
        [Then(@"Filter Textbox has ""(.*)""")]
        public void ThenFilterTextboxHas(string filterText)
        {
            Assert.AreEqual(filterText, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text);
        }

        [Given(@"Unit Tests Url Exists")]
        [When(@"Unit Tests Url Exists")]
        [Then(@"Unit Tests Url Exists")]
        public void UnitTestUrlExists()
        {
            #region Variable Declarations
            WpfHyperlink unitTestsUrlWorkflowUrlHyperlink = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnitTestsUrlWorkflowUrlText.UnitTestsUrlWorkflowUrlHyperlink;
            #endregion

            // Verify that the 'Exists' property of 'http://rsaklfsanele:3142/secure/Unit Tests/Unsaved...' link equals 'True'
            Assert.IsTrue(unitTestsUrlWorkflowUrlHyperlink.Exists, "UnitTestsUrlWorkflowUrl does not exist");
        }

        [Given(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [When(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [Then(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        public void Rename_Folder_From_Save_Dialog(string filterText)
        {
            #region Variable Declarations            
            WpfEdit uIItemEdit = SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            #endregion
            uIItemEdit.Text = filterText;
        }

        [Given(@"I Name New Folder as ""(.*)""")]
        [When(@"I Name New Folder as ""(.*)""")]
        [Then(@"I Name New Folder as ""(.*)""")]
        public void Name_New_Folder_From_Save_Dialog(string name)
        {
            #region Variable Declarations
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfEdit namedFolderExit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;
            #endregion

            // Type 'NewFolder' in text box
            newFolderEdit.Text = name;

            // Type '{Enter}' in text box
            Keyboard.SendKeys(namedFolderExit, "{Enter}", ModifierKeys.None);

            // Click 'Save' button
            Mouse.Click(saveButton, new Point(22, 16));
        }

        [Given(@"I Dont Name The Created Folder")]
        [When(@"I Dont Name The Created Folder")]
        [Then(@"I Dont Name The Created Folder")]
        public void ThenIDontNameTheCreatedFolder()
        {
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;

            Keyboard.SendKeys(newFolderEdit, "{Enter}", ModifierKeys.None);
            // Click 'Save' button
            Mouse.Click(saveButton, new Point(22, 16));
        }

        [Given(@"I Enter New Folder Name as ""(.*)""")]
        [When(@"I Enter New Folder Name as ""(.*)""")]
        [Then(@"I Enter New Folder Name as ""(.*)""")]
        public void ThenIEnterNewFolderNameAs(string name)
        {
            #region Variable Declarations
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfEdit namedFolderExit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;
            #endregion

            // Type 'NewFolder' in text box
            newFolderEdit.Text = name;

            // Type '{Enter}' in text box
            Keyboard.SendKeys(namedFolderExit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Enter New Sub Folder Name as ""(.*)""")]
        [When(@"I Enter New Sub Folder Name as ""(.*)""")]
        [Then(@"I Enter New Sub Folder Name as ""(.*)""")]
        public void ThenIEnterNewSubFolderNameAs(string name)
        {
            #region Variable Declarations
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit;
            WpfEdit namedFolderExit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;
            #endregion

            // Type 'NewFolder' in text box
            newFolderEdit.Text = name;

            // Type '{Enter}' in text box
            Keyboard.SendKeys(namedFolderExit, "{Enter}", ModifierKeys.None);
        }


        [Given(@"I Name New Sub Folder as ""(.*)""")]
        [When(@"I Name New Sub Folder as ""(.*)""")]
        [Then(@"I Name New Sub Folder as ""(.*)""")]
        public void I_Name_New_Sub_Folder_As(string name)
        {
            #region Variable Declarations
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit;
            WpfEdit namedFolderExit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;
            #endregion

            // Type 'NewFolder' in text box
            newFolderEdit.Text = name;

            // Type '{Enter}' in text box
            Keyboard.SendKeys(namedFolderExit, "{Enter}", ModifierKeys.None);

            // Click 'Save' button
            Mouse.Click(saveButton, new Point(22, 16));
        }

        [Given(@"Explorer Contain Item ""(.*)""")]
        [When(@"Explorer Contain Item ""(.*)""")]
        [Then(@"Explorer Contain Item ""(.*)""")]
        public void ExplorerContainItem(string itemName)
        {
            Assert.AreEqual(itemName, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text);
        }

        [Given(@"Explorer Does Not Contain Item ""(.*)""")]
        [When(@"Explorer Does Not Contain Item ""(.*)""")]
        [Then(@"Explorer Does Not Contain Item ""(.*)""")]
        public void ExplorerDoesNotContainItem(string p0)
        {
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem));
        }

        [Given(@"Explorer Contain Sub Item ""(.*)""")]
        [When(@"Explorer Contain Sub Item ""(.*)""")]
        [Then(@"Explorer Contain Sub Item ""(.*)""")]
        public void ExplorerContainSubFolder(string itemName)
        {
            Assert.AreEqual(itemName, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit.Text);
        }

        [Given(@"Explorer Items appear on the Explorer Tree")]
        [When(@"Explorer Items appear on the Explorer Tree")]
        [Then(@"Explorer Items appear on the Explorer Tree")]
        public void ExplorerItemsAppearOnTheExplorerTree()
        {
            Assert.IsTrue(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem));
            Assert.IsTrue(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem));
        }

        [Given(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [When(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [Then(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        public void ExplorerItemsAppearOnTheSaveDialogExplorerTree()
        {
            Assert.IsTrue(ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem));
            Assert.IsTrue(ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem));
        }

        [Given(@"Resource Did not Open")]
        [When(@"Resource Did not Open")]
        [Then(@"Resource Did not Open")]
        public void ResourceDidNotOpen()
        {
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("Hello World"));
        }

        [Given(@"""(.*)"" is child of ""(.*)""")]
        [When(@"""(.*)"" is child of ""(.*)""")]
        [Then(@"""(.*)"" is child of ""(.*)""")]
        public void ThenIsChildOf(string child, string parent)
        {
            Assert.AreEqual(parent, SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text);
            Assert.AreEqual(child, SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text);
        }

        [Given(@"""(.*)"" is child of localhost")]
        [When(@"""(.*)"" is child of localhost")]
        [Then(@"""(.*)"" is child of localhost")]
        public void ThenIsChildOfLocalhost(string child)
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Exists);
            Assert.AreEqual(child, SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text);
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

        [Then(@"""(.*)"" Resource Exists In Windows Directory ""(.*)""")]
        public void ResourceExistsInWindowsDirectory(string serviceName, string path)
        {
            var folder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Unit Tests";
            var allFiles = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
            Assert.IsTrue(allFiles.Any(p => p.Contains(serviceName)));
        }

        [Given(@"Context Menu Has Two Items")]
        [When(@"Context Menu Has Two Items")]
        [Then(@"Context Menu Has Two Items")]
        public void ThenContextMenuHasTwoItems()
        {
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem.Exists);
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow.SaveDialogContextMenu.SourcesMenuItem));
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem));
        }

        [Then(@"Folder Is Removed From Explorer")]
        public void ThenFolderIsRemovedFromExplorer()
        {
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem));
        }
        [Given(@"I Filter Variable List ""(.*)""")]
        [When(@"I Filter Variable List ""(.*)""")]
        [Then(@"I Filter Variable List ""(.*)""")]
        public void Filter_VariableList(string text)
        {
            #region Variable Declarations
            WpfEdit searchText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox;
            #endregion

            searchText.Text = text;
        }

        [Given(@"I Click Clear Variable List Filter")]
        [When(@"I Click Clear Variable List Filter")]
        [Then(@"I Click Clear Variable List Filter")]
        public void Click_Clear_Variable_List_Filter()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton);
        }

        public void Set_Input_Output_Variables()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.OutputCheckbox.Checked = true;
        }

    }
}
