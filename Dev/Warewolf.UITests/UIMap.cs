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
            Mouse.MouseDragSpeed = 450;
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
            var TimeBefore = System.DateTime.Now;
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
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Message Box dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging debug input dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Debug Input dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging save dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Save dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging unpinned pane to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to Pin Unpinned Pane To Default Position before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging service picker dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Service Picker dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging select windows group dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Windows Group dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging error dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Error dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
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
                else
                {
                    Console.WriteLine("No hanging critical error dialog to clean up.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging Critical Error dialog before the test starts.");
            }
            finally
            {
                Console.WriteLine("After trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
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
#if DEBUG
                    System.Windows.Forms.MessageBox.Show(messageText);
#else
                    Console.WriteLine(messageText);
#endif
                    parent.DrawHighlight();
#if DEBUG
                    throw e;
#endif
                }
            }
        }

        void UITestControlNotAvailableExceptionHandler(string type, string message, UITestControlNotAvailableException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show(type + "\n" + message);
#else
                Console.WriteLine(message);
#endif
                (exceptionSource as UITestControl).DrawHighlight();
#if DEBUG
                throw e;
#endif
            }
        }

        void FailedToPerformActionOnBlockedControlExceptionHandler(string type, string message, FailedToPerformActionOnBlockedControlException e)
        {
            var exceptionSource = e.ExceptionSource;
            if (exceptionSource is UITestControl)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show(type + "\n" + message);
#else
                Console.WriteLine(message);
#endif
                (exceptionSource as UITestControl).DrawHighlight();
#if DEBUG
                throw e;
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
            WpfButton saveButton = MainStudioWindow.SideMenuBar.SaveButton;
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
            WpfButton saveButton = MainStudioWindow.SideMenuBar.SaveButton;
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
            WpfButton saveButton = MainStudioWindow.SideMenuBar.SaveButton;
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
            WpfButton serverListComboBox = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox;
            WpfCustom comboboxListItemAsLocalhostConnected = MainStudioWindow.ComboboxListItemAsLocalhostConnected;
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
            WpfEdit textbox = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
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
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem showDependencies = MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfRadioButton showwhatdependsonthisRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton;
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox;
            WpfButton refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton;
            WpfText text = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text;
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
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            WpfCheckBox public_DeployToCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox;
            WpfCheckBox public_DeployFromCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox;
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
            WpfCheckBox resource_ContributeCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ViewCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ExecuteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfButton resource_DeleteButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.DeleteCell.DeleteButton;

            resource_ContributeCheckBox.Checked = true;
            Assert.IsTrue(resource_ViewCheckBox.Checked, "Resource View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_ExecuteCheckBox.Checked, "Resource Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_DeleteButton.Enabled, "Resource Delete button is disabled");

        }

        [When(@"I Check Public Contribute")]
        public void Check_Public_Contribute()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
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
            WpfCheckBox public_AdministratorCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
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
            WpfText testNameText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestNameText;
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            WpfList testsListBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList;
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
            #region Variable Declarations
            WpfButton newSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton;
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
            WpfTreeItem folder1 = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem newWorkflow = MainStudioWindow.ExplorerContextMenu.NewWorkflow;
            #endregion

            // Right-Click 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item
            Mouse.Click(folder1, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));

            // Verify that the 'Enabled' property of 'New Workflow Service' menu item equals 'True'
            Assert.AreEqual(Select_NewWorkFlowService_From_ContextMenuParams.NewWorkflowEnabled, newWorkflow.Enabled, "NewWorkFlowService button is disabled.");

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
        /// <summary>
        /// Select_SharepointTestServer - Use 'Select_SharepointTestServerParams' to pass parameters into this method.
        /// </summary>
        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer_FromSharepointDelete_tool()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server;
            WpfListItem sharepointTestServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.Server.SharepointTestServer;
            WpfButton editSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton;
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
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList;
            WpfListItem uIAcceptanceTesting_CrListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAcceptanceTesting_CrListItem;
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
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList;
            WpfListItem uIAppdataListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.MethodList.UIAppdataListItem;
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
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList;
            WpfListItem uIAppdataListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.MethodList.UIAppdataListItem;
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

        [When(@"I Open Explorer First Item Context Menu")]
        public void WhenIOpenExplorerFirstItemContextMenu()
        {
            Open_Explorer_First_Item_With_Context_Menu();
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
            WpfButton debugF6Button = MainStudioWindow.DebugInputDialog.DebugF6Button;
            WpfCustom debugOutput = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput;
            WpfButton settingsButton = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.SettingsButton;
            WpfButton expandCollapseButton = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.ExpandCollapseButton;
            WpfEdit searchTextBox = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.SearchTextBox;
            WpfTree debugOutputTree = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree;
            #endregion

            // Verify that the 'Enabled' property of 'Debug (F6)' button equals 'True'
            Assert.IsTrue(debugF6Button.Enabled, "DebugF6Button is not enabled after clicking RunDebug from Menu.");

            // Click 'Debug (F6)' button
            Mouse.Click(debugF6Button, new Point(34, 10));

            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug Output does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SettingsButton.Exists, "Output SettingsButton does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.SearchTextBox.Exists, "Output SearchTextBox does not exist after clicking Debug button from Debug Dialog");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Exists, "DebugOutputTree does not exist after clicking Debug button from Debug Dialog");
        }

        public void Debug_Unpinned_Workflow_With_Ribbon_Button()
        {
            Toggle_Between_Studio_and_Unpinned_Tab();
            Click_Debug_Ribbon_Button();
            Click_DebugInput_Debug_Button_For_UnpinnedWindow();
            Toggle_Between_Studio_and_Unpinned_Tab();
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
                    Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2 on unpinned tab.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab();
                    Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1 on unpinned tab.");
                    break;
            }
        }

        /// <summary>
        /// Click_Explorer_RemoteServer_Edit_Button - Use 'Click_Explorer_RemoteServer_Edit_ButtonParams' to pass parameters into this method.
        /// </summary>
        public void Click_Explorer_RemoteServer_Edit_Button()
        {
            #region Variable Declarations
            WpfButton editServerButton = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton;
            WpfTabPage serverSourceWizardTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab;
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
            WpfEdit recordset = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.RecordsetComboBox.TextEdit;
            WpfEdit result = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.SmallViewContentCustom.ResultComboBox.TextEdit;
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

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = resourcesFolderCopy;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Delete_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Write_Tool()
        {
            var file = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Test File.txt";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.FileNameComboBox.TextEdit;
            WpfEdit contents = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox.TextEdit;
            WpfRadioButton overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = file;
            contents.Text = "Some Content";
            overwrite.Selected = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Move_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfCheckBox overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = destinationFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Zip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Zip";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ZipNameComboBox.TextEdit;
            WpfCheckBox overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = destinationFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_UnZip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var unZipFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_UnZip";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox.TextEdit;
            WpfEdit destination = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.DestinationComboBox.TextEdit;
            WpfCheckBox overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = unZipFolder;
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Rename_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox.TextEdit;
            WpfEdit destination = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.NewNameComboBox.TextEdit;
            WpfCheckBox overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            destination.Text = "Acceptance Tests_New";
            overwrite.Checked = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_ReadFolder_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.TextEdit;
            WpfRadioButton filesAndFolders = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            filesAndFolders.Selected = true;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Read_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var file = resourcesFolder + @"\" + "Hello World" + ".xml";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox.TextEdit;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = file;
            results.Text = "[[results]]";
        }

        public void Enter_Text_Into_Create_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Create";

            WpfEdit fileOrFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.FileNameoComboBox.TextEdit;
            WpfCheckBox Overwrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OverwriteCheckBox;
            WpfEdit results = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.ResultComboBox.TextEdit;

            fileOrFolder.Text = resourcesFolder;
            Overwrite.Checked = true;
            results.Text = "[[results]]";
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DrawHighlight();
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = p0;
            Playback.Wait(1500);
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CopyFile, new Point(2, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 126));

            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.Exists, "Sharepoint tool does not exist on the Design Surface");
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
        
        public void Add_Dotnet_Dll_Source(string sourceName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourcButton, new Point(30, 4));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.DotNetPlugInSourceViewModelsCustom.SearchTextBoxEdit.Text = "CustomMarshalers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=AMD64";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.GACTreeItem.ExpansionIndicatorCheckBox, new Point(30, 4));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.GACTreeItem.FirstTreeItem);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Save_With_Ribbon_Button_And_Dialog(sourceName, true);
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
        }
        
        public void Show_Explorer_First_Item_Tests_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests does not exist in explorer context menu.");
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.Tests, new Point(30, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton.Exists, "Create new test button does not exist on tests tab after openning it with the explorer context menu.");
        }
        
        public void Debug_Using_Play_Icon()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
        }

        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2 On Unpinned tab")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2_On_Unpinned_tab()
        {
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [When(@"I Check Public Administrator")]
        public void Check_Public_Administrator()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox is NOT checked after Checking Administrator.");
        }

        [When(@"I Click AddNew Web Source From PostWeb tool")]
        public void Click_AddNew_Web_Source_From_PostWeb_tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton.Exists, "NewButton does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton, new Point(30, 4));
        }

        [When(@"I Click AddNew Web Source From tool")]
        public void Click_AddNew_Web_Source_From_tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton.Exists, "NewButton does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton, new Point(30, 4));
        }

        [When(@"I Click Assign Tool CollapseAll")]
        public void Click_Assign_Tool_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [When(@"I Click Assign Tool ExpandAll")]
        public void Click_Assign_Tool_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [When(@"I Click Assign Tool Large View Done Button")]
        public void Click_Assign_Tool_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view.");
        }

        [When(@"I Click Assign Tool Large View Done Button On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.AreEqual("SomeVariable", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text does not equal somevariable after using that variable on a unpinned tab.");
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view on an unpinned tab.");
        }

        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName On Unpinned Tab")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName_On_Unpinned_Tab()
        {
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [When(@"I Click Assign Tool QviLarge Preview")]
        public void Click_Assign_Tool_QviLarge_Preview()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PreviewCustom.PreviewGroup.PreviewButton, new Point(30, 4));
        }

        [When(@"I click AssignObject Done")]
        public void click_AssignObject_Done()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton, new Point(18, 10));
        }

        [When(@"I Click Base Convert Large View Done Button")]
        public void Click_Base_Convert_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.DoneButton, new Point(36, 11));
            Assert.AreEqual("SomeData", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.SmallView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Text, "Base convert small view row1 variable textbox does not contain text SomeData.");
        }

        [When(@"I Click Calculate Large View Done Button")]
        public void Click_Calculate_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.DoneButton, new Point(45, 8));
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.SmallView.Listbox.FunctionTextbox.Text, "Calculate small view function textbox text does not equal SomeVariable.");
        }

        [When(@"I Click Cancel DebugInput Window")]
        public void Click_Cancel_DebugInput_Window()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.CancelButton.Enabled, "CancelButton is not enabled after clicking RunDebug from Menu.");
            Mouse.Click(MainStudioWindow.DebugInputDialog.CancelButton, new Point(26, 13));
        }

        [When(@"I Click Clear Toolbox Filter Clear Button")]
        public void Click_Clear_Toolbox_Filter_Clear_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.ClearFilterButton, new Point(8, 7));
        }

        [When(@"I Click Close Critical Error Dialog")]
        public void Click_Close_Critical_Error_Dialog()
        {
            Mouse.Click(CriticalErrorWindow.CloseButton, new Point(9, 11));
        }

        [When(@"I Click Close DB Source Wizard Tab Button")]
        public void Click_Close_DB_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.CloseButton, new Point(13, 4));
        }

        [When(@"I Click Close Dependecy Tab")]
        public void Click_Close_Dependecy_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.CloseButton, new Point(13, 10));
        }

        [When(@"I Click Close Deploy Tab Button")]
        public void Click_Close_Deploy_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.TabCloseButton.Exists, "Settings close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.TabCloseButton, new Point(16, 6));
        }

        [When(@"I Click Close DotNetDll Tab")]
        public void Click_Close_DotNetDll_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetDllPlugInSource.ClosePlugInSourceTabButton, new Point(13, 4));
        }

        [When(@"I Click Close EmailSource Tab")]
        public void Click_Close_EmailSource_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.EmailSourceTabCloseButton, new Point(13, 10));
        }

        [When(@"I Click Close Error Dialog")]
        public void Click_Close_Error_Dialog()
        {
            Mouse.Click(ErrorWindow.CloseButton, new Point(8, 9));
        }

        [When(@"I Click Close FullScreen")]
        public void Click_Close_FullScreen()
        {
            Mouse.Click(MainStudioWindow.ExitFullScreenF11Text.ExitFullScreenF11Hyperlink, new Point(64, 5));
        }

        [When(@"I Click Close Plugin Source Wizard Tab Button")]
        public void Click_Close_Plugin_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.PluginSourceWizardTab.CloseButton, new Point(13, 4));
        }

        [When(@"I Click Close Server Source Wizard Tab Button")]
        public void Click_Close_Server_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.TabCloseButton, new Point(5, 5));
        }

        [When(@"I Click Close Settings Tab Button")]
        public void Click_Close_Settings_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton.Exists, "Settings close tab button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.CloseButton, new Point(16, 6));
        }

        [When(@"I Click Close SharepointSource Tab Button")]
        public void Click_Close_SharepointSource_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointSourceTabCloseButton, new Point(13, 7));
        }

        [When(@"I Click Close Studio TopRibbon Button")]
        public void Click_Close_Studio_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.CloseStudioButton, new Point(23, 1));
        }

        [When(@"I Click Close Tab Context Menu Button")]
        public void Click_Close_Tab_Context_Menu_Button()
        {
            Mouse.Click(MainStudioWindow.TabContextMenu.Close, new Point(27, 13));
        }

        [When(@"I Click Close Tests Tab")]
        public void Click_Close_Tests_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.CloseTestTabButton, new Point(11, 5));
        }

        [When(@"I Click Close Web Source Wizard Tab Button")]
        public void Click_Close_Web_Source_Wizard_Tab_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.CloseButton, new Point(9, 6));
        }

        [When(@"I Click Close Workflow Tab Button")]
        public void Click_Close_Workflow_Tab_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton.Exists, "Close tab button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton, new Point(5, 5));
        }

        [When(@"I Click ConfigureSetting From Menu")]
        public void Click_ConfigureSetting_From_Menu()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.Exists, "Settings tab does not exist after the Configure/Setting Menu button is clicked");
        }

        [When(@"I Click Connect Control InExplorer")]
        public void Click_Connect_Control_InExplorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(217, 8));
        }

        [When(@"I Click Debug Output Assign Cell")]
        public void Click_Debug_Output_Assign_Cell()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus, "Multiassign small view is not selected.");
        }

        [When(@"I Click Debug Output Assign Cell For Unpinned Workflow Tab")]
        public void Click_Debug_Output_Assign_Cell_For_Unpinned_Workflow_Tab()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
        }

        [When(@"I Click Debug Output BaseConvert Cell")]
        public void Click_Debug_Output_BaseConvert_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.BaseConversion1Button, new Point(33, 7));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.ItemStatus, "Base conversion small view is not selected.");
        }

        [When(@"I Click Debug Output Calculate Cell")]
        public void Click_Debug_Output_Calculate_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.CalculateButton, new Point(24, 10));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.ItemStatus, "Calculate tool small view is not selected.");
        }

        [When(@"I Click Debug Output Workflow1 Name")]
        public void Click_Debug_Output_Workflow1_Name()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.ServiceTreeItem.Workflow1Button, new Point(24, 8));
            Assert.AreEqual("workflow1", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Workflow1ciremoteText.DisplayText, "Workflow1 remote workflow tab is not open.");
        }

        [When(@"I Click DebugInput Cancel Button")]
        public void Click_DebugInput_Cancel_Button()
        {
            Mouse.Click(MainStudioWindow.DebugInputDialog.CancelButton, new Point(34, 10));
        }

        [When(@"I Click DebugInput Debug Button")]
        public void Click_DebugInput_Debug_Button()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.DebugF6Button.Enabled, "DebugF6Button is not enabled after clicking RunDebug from Menu.");
            Mouse.Click(MainStudioWindow.DebugInputDialog.DebugF6Button, new Point(34, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.Exists, "Debug output does not exist");
        }

        [When(@"I Click DebugInput ViewInBrowser Button")]
        public void Click_DebugInput_ViewInBrowser_Button()
        {
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "ViewInBrowserF7Button is not enabled after clicking RunDebug from Menu.");
            Mouse.Click(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button, new Point(82, 14));
        }

        [When(@"I Click Decision Dialog Cancel Button")]
        public void Click_Decision_Dialog_Cancel_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.CancelButton, new Point(10, 14));
        }

        [When(@"I Click Decision Dialog Done Button")]
        public void Click_Decision_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(10, 14));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision.Exists, "Decision on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Click Delete Done Button")]
        public void Click_Delete_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.DoneButton, new Point(35, 6));
        }

        [When(@"I Click DeleteWeb Generate Outputs")]
        public void Click_DeleteWeb_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton, new Point(85, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DsfWebDeleteOutputsLargeView.PasteButton.Exists, "Paste button does not exist after clicking generate outputs.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DsfWebDeleteOutputsLargeView.TestButton.Exists, "Test button does not exist after clicking generate outputs.");
        }

        [When(@"I Click Deploy Tab Destination Server Combobox")]
        public void Click_Deploy_Tab_Destination_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
        }

        [When(@"I Click Deploy Tab Destination Server Connect Button")]
        public void Click_Deploy_Tab_Destination_Server_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.ConnectDestinationButton, new Point(13, 12));
        }

        [When(@"I Click Deploy Tab Destination Server New Remote Server Item")]
        public void Click_Deploy_Tab_Destination_Server_New_Remote_Server_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsNewRemoteServer, new Point(223, 10));
        }

        [When(@"I Click Deploy Tab Source Server Combobox")]
        public void Click_Deploy_Tab_Source_Server_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.ToggleButton, new Point(230, 9));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Source server combobox.");
        }

        [When(@"I Click Deploy Tab Source Server Connect Button")]
        public void Click_Deploy_Tab_Source_Server_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceConnectControl.ConnectSourceButton, new Point(13, 8));
        }

        [When(@"I Click Deploy Tab WarewolfStore Item")]
        public void Click_Deploy_Tab_WarewolfStore_Item()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsWarewolfStore, new Point(214, 9));
        }

        [When(@"I Click DotNet DLL Large View Done Button")]
        public void Click_DotNet_DLL_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.SmallView.Exists, "DotNet DLL small view does not exist after clicking done on large view.");
        }

        [When(@"I Click DotNet DLL Large View Test Inputs Button")]
        public void Click_DotNet_DLL_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton, new Point(21, 11));
        }

        [When(@"I Click Duplicate From Duplicate Dialog")]
        public void Click_Duplicate_From_Duplicate_Dialog()
        {
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Exists, "Duplicate button does not exist");
            Mouse.Click(SaveDialogWindow.DuplicateButton, new Point(26, 10));
            Assert.IsTrue(SaveDialogWindow.Exists, "Save Dialog does not exist after clicking Duplicate button");
        }

        [When(@"I Click EditSharepointSource Button")]
        public void Click_EditSharepointSource_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [When(@"I Click EditSharepointSource Button From SharePointUpdate")]
        public void Click_EditSharepointSource_Button_From_SharePointUpdate()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.EditSourceButton, new Point(98, 12));
        }

        [When(@"I Click EditSharepointSource Button FromSharePointDelete")]
        public void Click_EditSharepointSource_Button_FromSharePointDelete()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton, new Point(98, 12));
        }

        [When(@"I Click EditSharepointSource Button FromSharePointRead")]
        public void Click_EditSharepointSource_Button_FromSharePointRead()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [When(@"I Click EmailSource TestConnection Button")]
        public void Click_EmailSource_TestConnection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.TestConnectionButton, new Point(58, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PassedText.Exists, "Connection test Failed");
        }

        [When(@"I Click EndThisWF On XPath LargeView")]
        public void Click_EndThisWF_On_XPath_LargeView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox.Checked = true;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox, "{Tab}", ModifierKeys.None);
        }

        [When(@"I Click ExpandAndStepIn NestedWorkflow")]
        public void Click_ExpandAndStepIn_NestedWorkflow()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.Expanded = true;
        }

        [When(@"I Click Explorer Filter Clear Button")]
        public void Click_Explorer_Filter_Clear_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.ClearFilterButton, new Point(6, 8));
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text, "Explorer Filter Textbox text is not blank after clicking the clear button.");
        }

        [When(@"I Click Explorer Localhost First Item")]
        public void Click_Explorer_Localhost_First_Item()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(63, 11));
        }

        [When(@"I Click Explorer Remote Server Dropdown List")]
        public void Click_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(167, 10));
        }

        [When(@"I Click Explorer RemoteServer Connect Button")]
        public void Click_Explorer_RemoteServer_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ConnectServerButton, new Point(11, 10));
            Playback.Wait(2000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists, "No remote servers in explorer.");
        }

        [When(@"I Click First Recordset Input Checkbox")]
        public void Click_First_Recordset_Input_Checkbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click FormatNumber Done Button")]
        public void Click_FormatNumber_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.DoneButton, new Point(36, 11));
        }

        [When(@"I Click FullScreen TopRibbon Button")]
        public void Click_FullScreen_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeRestoreStudioButton, new Point(12, 9));
        }

        [When(@"I Click GET Web Large View Done Button")]
        public void Click_GET_Web_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after clicking large view done button.");
        }

        [When(@"I Click GET Web Large View Done Button With Invalid Large View")]
        public void Click_GET_Web_Large_View_Done_Button_With_Invalid_Large_View()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Exists, "Error not exist after clicking large view done button on invalid large view.");
        }

        [When(@"I Click GET Web Large View Generate Outputs")]
        public void Click_GET_Web_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton, new Point(7, 7));
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton.Exists, "Web GET large view generate outputs test button does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton.Exists, "Web GET tool large view generate inputs done button does not exist.");
        }

        [When(@"I Click GET Web Large View Test Inputs Button")]
        public void Click_GET_Web_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton, new Point(21, 11));
        }

        [When(@"I Click GET Web Large View Test Inputs Done Button")]
        public void Click_GET_Web_Large_View_Test_Inputs_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton, new Point(35, 6));
        }

        [When(@"I Click HTTP Delete Web Tool New Button")]
        public void Click_HTTP_Delete_Web_Tool_New_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.NewSourceButton, new Point(13, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.Exists, "New web source wizard tab is not open after clicking create new web source from delete tool.");
        }

        [When(@"I Click HTTP Post Web Tool New Button")]
        public void Click_HTTP_Post_Web_Tool_New_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton, new Point(17, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.Exists, "New web source wizard tab is not open after clicking create new web source from post tool on the design surface.");
        }

        [When(@"I Click Knowledge Ribbon Button")]
        public void Click_Knowledge_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.KnowledgeBaseButton, new Point(4, 8));
        }

        [When(@"I Click Lock Ribbon Button")]
        public void Click_Lock_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.LockStudioButton, new Point(14, 5));
        }

        [When(@"I Click Maximize Restore TopRibbon Button")]
        public void Click_Maximize_Restore_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(9, 11));
        }

        [When(@"I Click Maximize TopRibbon Button")]
        public void Click_Maximize_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MaximizeStudioButton, new Point(14, 14));
        }

        [When(@"I Click MessageBox No")]
        public void Click_MessageBox_No()
        {
            Mouse.Click(MessageBoxWindow.NoButton, new Point(32, 5));
        }

        [When(@"I Click MessageBox OK")]
        public void Click_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [When(@"I Click MessageBox Yes")]
        public void Click_MessageBox_Yes()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(32, 5));
            Assert.IsFalse(ControlExistsNow(MessageBoxWindow), "Message box does exist");
        }

        [When(@"I Click Minimize TopRibbon Button")]
        public void Click_Minimize_TopRibbon_Button()
        {
            Mouse.Click(MainStudioWindow.MinimizeStudioButton, new Point(6, 14));
        }

        [When(@"I Click Nested Workflow Name")]
        public void Click_Nested_Workflow_Name()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.UIHelloWorldTreeItem1.UIHelloWorldButton, new Point(37, 10));
        }

        [When(@"I Click New Database Source Ribbon Button")]
        public void Click_New_Database_Source_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.DatabaseSourceButton, new Point(16, 15));
        }

        [When(@"I Click New Workflow Tab")]
        public void Click_New_Workflow_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, new Point(63, 18));
        }

        [When(@"I Click NewPluginSource Ribbon Button")]
        public void Click_NewPluginSource_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.PluginSourceButton, new Point(22, 13));
            Playback.Wait(1000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.PluginSourceWizardTab.WorkSurfaceContext.NewPluginSourceWizard.ScrollViewer.Tree.Exists, "Select assembly tree does not exist in new plugin source wizard tab.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.PluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Exists, "Assembly textbox does not exist in new plugin source wizard tab.");
        }

        [When(@"I Click NewSource Button FromODBC Tool")]
        public void Click_NewSource_Button_FromODBC_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeViewContentCustom.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "DBSourceWizardTab did not open");
        }

        [When(@"I Click NewSource Button FromOracle Tool")]
        public void Click_NewSource_Button_FromOracle_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeViewContentCustom.NewSourceButton, new Point(30, 4));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.Exists, "DBSourceWizardTab did not open");
        }

        [When(@"I Click NewVersion button")]
        public void Click_NewVersion_button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.NewVersionButton.Enabled, "New version available button is disabled");
            Mouse.Click(MainStudioWindow.SideMenuBar.NewVersionButton, new Point(17, 9));
        }

        [When(@"I Click Output OnRecordset InVariableList")]
        public void Click_Output_OnRecordset_InVariableList()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [When(@"I Click Output OnVariable InVariableList")]
        public void Click_Output_OnVariable_InVariableList()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.OutputCheckbox.Checked = true;
        }

        [When(@"I Click Pin Toggle DebugOutput")]
        public void Click_Pin_Toggle_DebugOutput()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputUnpinBtn, new Point(11, 10));
        }

        [When(@"I Click Pin Toggle Documentor")]
        public void Click_Pin_Toggle_Documentor()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Help.DocumentorUnpinBtn, new Point(2, 11));
        }

        [When(@"I Click Pin Toggle Explorer")]
        public void Click_Pin_Toggle_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerUnpinBtn, new Point(12, 9));
        }

        [When(@"I Click Pin Toggle Toolbox")]
        public void Click_Pin_Toggle_Toolbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolUnpinBtn, new Point(10, 8));
        }

        [When(@"I Click Pin Toggle VariableList")]
        public void Click_Pin_Toggle_VariableList()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.VariableUnpinBtn, new Point(10, 14));
        }

        [When(@"I Click Position Button")]
        public void Click_Position_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.FilesMenu.PositionButton, new Point(8, 7));
        }

        [When(@"I Click Postgre Done Button")]
        public void Click_Postgre_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.DoneButton, new Point(36, 11));
        }

        [When(@"I Click PrefixContainsInvalidText Hyperlink")]
        public void Click_PrefixContainsInvalidText_Hyperlink()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PrefixcontainsinvaliText.PrefixcontainsinvaliHyperlink, new Point(30, 4));
        }

        [When(@"I Click PutWeb GenerateOutputs Button")]
        public void Click_PutWeb_GenerateOutputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton, new Point(34, 13));
        }

        [When(@"I Click Read Done Button")]
        public void Click_Read_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.DoneButton, new Point(35, 6));
        }

        [When(@"I Click ReadFolder Done Button")]
        public void Click_ReadFolder_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Remove Unused Variables")]
        public void Click_Remove_Unused_Variables()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.RemoveUnused, new Point(30, 4));
        }

        [When(@"I Click Rename Done Button")]
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

        [When(@"I Click Save Ribbon Button to Open Save Dialog")]
        public void Click_Save_Ribbon_Button_to_Open_Save_Dialog()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Exists, "Save ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist after clicking save ribbon button.");
        }

        [When(@"I Click SaveDialog CancelButton")]
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ResourceCell.ItemButton, new Point(13, 16));
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ResourceCell.ItemButton, new Point(6, 15));
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
            Assert.AreEqual(Click_SQL_Server_Large_View_Generate_OutputsExpectedValues.TestDataTextboxExists, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Exists, "SQL Server large view test inputs row 1 test data textbox does not exist.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
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
            MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = Click_Unpinned_Workflow_ExpandAllParams.ExpandAllToggleButtonPressed;
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = Click_Variable_IsInputParams.InputCheckboxChecked;
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

        [When(@"I Click Workflow CollapseAll")]
        public void Click_Workflow_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [When(@"I Click Workflow ExpandAll")]
        public void Click_Workflow_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
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

        [When(@"I Create SubFolder In Folder1")]
        public void Create_SubFolder_In_Folder1()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(126, 12));
            Mouse.Click(MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem, new Point(78, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.ItemEdit.Text = "Acceptance Testing Resources";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, "{Enter}", ModifierKeys.None);
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

        [When(@"I DoubleClick Explorer Localhost First Item")]
        public void DoubleClick_Explorer_Localhost_First_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(63, 11));
        }

        [When(@"I Drag DeleteWeb Toolbox Onto Workflow Surface")]
        public void Drag_DeleteWeb_Toolbox_Onto_Workflow_Surface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.Exists, "Delete Web connectoer does not exist on the design surface after drag and drop from toolbox.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.Exists, "DotNet DLL tool does not exist on the design surface");
        }

        [When(@"I Drag Explorer Localhost First Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Explorer Localhost First Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Explorer Localhost Second Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Explorer Remote workflow1 Onto Workflow Design Surface")]
        public void Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SubWorkflow.Exists, "Workflow on the design surface does not exist");
        }

        [When(@"I Drag GET Web Connector Onto DesignSurface")]
        public void Drag_GET_Web_Connector_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "GET";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.GET, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.Exists, "GET Web connectoer does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag GetWeb RequestTool Onto DesignSurface")]
        public void Drag_GetWeb_RequestTool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Web Request";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 124));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest, new Point(12, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 124));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.Exists, "Web Get Request small view does not exist on the design surface");
        }

        [When(@"I Drag PostWeb RequestTool Onto DesignSurface")]
        public void Drag_PostWeb_RequestTool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.Exists, "Web Post Request small view does not exist on the design surface");
        }

        [When(@"I Drag PutWeb Tool Onto DesignSurface")]
        public void Drag_PutWeb_Tool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.Exists, "Put Web connectoer does not exist on the design surface after drag and drop from toolbox.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Base Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Base_Conversion_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Base Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.BaseConvert, new Point(12, 12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.Exists, "Base Conversion on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Calculate Onto DesignSurface")]
        public void Drag_Toolbox_Calculate_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Calculate, new Point(59, -17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.Exists, "Calculate tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Case Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Case_Conversion_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Case Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.CaseConvert, new Point(19, 13));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.Exists, "Case Conversion on the design surface does not exist");
        }

        [When(@"I Drag Toolbox CMD Line Onto DesignSurface")]
        public void Drag_Toolbox_CMD_Line_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "CMD Script";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 122));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.CMDScript, new Point(19, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.Exists, "CMD Line tool on the design surface tool does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.SmallViewContent.ScriptIntellisenseTextbox.Exists, "CMD script textbox does not exist after dragging onto design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.SmallViewContent.ResultIntellisenseTextbox.Exists, "CMD script result textbox does not exist after dragging onto design surface.");
        }

        [When(@"I Drag Toolbox Comment Onto DesignSurface")]
        public void Drag_Toolbox_Comment_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(40, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Copy Onto DesignSurface")]
        public void Drag_Toolbox_Copy_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(310, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Copy, new Point(19, -3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(310, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.Exists, "Copy on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Count Records Onto DesignSurface")]
        public void Drag_Toolbox_Count_Records_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Count";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Count, new Point(13, 18));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Create Onto DesignSurface")]
        public void Drag_Toolbox_Create_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Create, new Point(9, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.Exists, "Create tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Data Merge Onto DesignSurface")]
        public void Drag_Toolbox_Data_Merge_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Merge";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 133));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataMerge, new Point(54, 23));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data Merge on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Data Split Onto DesignSurface")]
        public void Drag_Toolbox_Data_Split_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Split";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataSplit, new Point(3, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.Exists, "Data Split on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        public void Drag_Toolbox_Date_And_Time_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTime, new Point(20, -1));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.Exists, "Date and Time tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox DateTime Difference Onto DesignSurface")]
        public void Drag_Toolbox_DateTime_Difference_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time Diff";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTimeDifference, new Point(48, 7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.Exists, "Date And Time Difference tool on the design surface does not exist");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.Exists, "Delete tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Delete Record Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Record_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Delete, new Point(1, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Find Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Index_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Index";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.FindIndex, new Point(9, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.Exists, "Find Index on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Find Record Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Record_Index_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Records";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.FindRecords, new Point(8, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox For Each Onto DesignSurface")]
        public void Drag_Toolbox_For_Each_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ForEach";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.ForEach, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.Exists, "For Each tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Format Number Onto DesignSurface")]
        public void Drag_Toolbox_Format_Number_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Format Number";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.FormatNumber, new Point(18, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.Exists, "Format Number tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Javascript Onto DesignSurface")]
        public void Drag_Toolbox_Javascript_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Javascript";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.JavaScript, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.Exists, "Javascript tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox JSON Onto DesignSurface")]
        public void Drag_Toolbox_JSON_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create JSON";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.CreateJSON, new Point(0, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "Create JSON tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Length Onto DesignSurface")]
        public void Drag_Toolbox_Length_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Length";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Length, new Point(16, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.Exists, "Length tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Move Onto DesignSurface")]
        public void Drag_Toolbox_Move_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Move, new Point(32, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.Exists, "Move tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        public void Drag_Toolbox_MultiAssign_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Exists, "Assign small view row 1 variable textbox does not exist");
        }

        [When(@"I Drag Toolbox MySql Database Onto DesignSurface")]
        public void Drag_Toolbox_MySql_Database_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "MySQL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.MySQL, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox ODBC Dtatbase Onto DesignSurface")]
        public void Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ODBC";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.ODBC, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
        }

        [When(@"I Drag Toolbox Oracle Database Onto DesignSurface")]
        public void Drag_Toolbox_Oracle_Database_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Oracle";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Oracle, new Point(11, 20));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
        }

        [When(@"I Drag Toolbox PostgreSql Onto DesignSurface")]
        public void Drag_Toolbox_PostgreSql_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Postgre";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Postgre, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
        }

        [When(@"I Drag Toolbox Python Onto DesignSurface")]
        public void Drag_Toolbox_Python_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Python";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Python, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.Exists, "Python tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ScriptIntellisenseCombobox.Exists, "Python script textbox does not exist after dragging on tool from the toolbox.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ResultIntellisenseCombobox.Exists, "Python result textbox does not exist after dragging on tool from the toolbox.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.Exists, "Random tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Read File Onto DesignSurface")]
        public void Drag_Toolbox_Read_File_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read File";
            Playback.Wait(2000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 125));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFile, new Point(12, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.Exists, "Read File tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Read_Folder_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read Folder";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFolder, new Point(14, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.Exists, "Read folder tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Rename Onto DesignSurface")]
        public void Drag_Toolbox_Rename_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Rename";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Rename, new Point(6, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.Exists, "Rename tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Replace Onto DesignSurface")]
        public void Drag_Toolbox_Replace_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Replace";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 121));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.Replace, new Point(16, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 121));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.Exists, "Replace on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Ruby Onto DesignSurface")]
        public void Drag_Toolbox_Ruby_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Ruby";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Ruby, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.Exists, "Ruby tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Selectandapply Onto DesignSurface")]
        public void Drag_Toolbox_Selectandapply_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Select and apply";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.Selectandapply, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging select and apply tool onto start node autoconnector.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.Exists, "Select and apply does not exist on design surface after dragging from toolbox.");
        }

        [When(@"I Drag Toolbox Sequence Onto DesignSurface")]
        public void Drag_Toolbox_Sequence_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sequence";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Sequence, new Point(18, -12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.Exists, "Sequence on the design surface does not exist");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server.Exists, "server lookup does not exist on the sharepoin smal view");
        }

        [When(@"I Drag Toolbox Sharepoint Create Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CreateListItems, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.Exists, "server lookup does not exist on the sharepoin smal view");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.List.Exists, "sharepint list does not exist on the sharepoint small view");
        }

        [When(@"I Drag Toolbox Sharepoint Delete Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Delete_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteListItems, new Point(16, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server.Exists, "server lookup does not exist on the sharepoin smal view");
        }

        [When(@"I Drag Toolbox Sharepoint Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_Folder_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read";
            Playback.Wait(1000);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadFolder, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Sharepoint Read List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_List_Item_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadListItems, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Sharepoint Update Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Update List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UpdateListItems, new Point(17, 9));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox Sharepoint UploadFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Upload";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UploadFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.Exists, "Small view does not exist on sharepoint upload file after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox SMTP Email Onto DesignSurface")]
        public void Drag_Toolbox_SMTP_Email_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SMTP Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.SMTPSend, new Point(16, -39));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.Exists, "Email tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Sort Record Onto DesignSurface")]
        public void Drag_Toolbox_Sort_Record_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sort";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 122));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Sort, new Point(7, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
        }

        [When(@"I Drag Toolbox SQL Bulk Insert Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Bulk Insert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLBulkInsert, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.Exists, "Sql Bulk Insert tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox SQL Server Tool Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Server";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLServer, new Point(10, -7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.Exists, "System Info tool on the design surface does not exist");
        }

        [When(@"I Drag Toolbox Unique Records Onto DesignSurface")]
        public void Drag_Toolbox_Unique_Records_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem uniqueRecords = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.UniqueRecords;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            #endregion
            searchTextBox.Text = "Unique";
            Playback.Wait(2000);
            flowchart.EnsureClickable(new Point(304, 133));
            Mouse.StartDragging(uniqueRecords, new Point(43, 6));
            Mouse.StopDragging(flowchart, new Point(304, 133));
            Assert.IsTrue(connector1.Exists, "No connectors exist on design surface.");
        }
        [When(@"I Drag Toolbox Unzip Onto DesignSurface")]
        public void Drag_Toolbox_Unzip_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem unZip = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.UnZip;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            WpfCustom unZip1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip;
            #endregion
            searchTextBox.Text = Drag_Toolbox_Unzip_Onto_DesignSurfaceParams.SearchTextBoxText;
            Playback.Wait(1000);
            flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(unZip, new Point(15, 15));
            Mouse.StopDragging(flowchart, new Point(306, 128));
            Assert.AreEqual(Drag_Toolbox_Unzip_Onto_DesignSurfaceParams.Connector1Exists, connector1.Exists, "No connectors exist on design surface.");
            Assert.AreEqual(Drag_Toolbox_Unzip_Onto_DesignSurfaceParams.UnZipExists, unZip1.Exists, "Unzip on the design surface does not exist");
        }
        [When(@"I Drag Toolbox Web Request Onto DesignSurface")]
        public void Drag_Toolbox_Web_Request_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem webRequest = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            WpfCustom webRequest1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest;
            #endregion
            searchTextBox.Text = Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams.SearchTextBoxText;
            flowchart.EnsureClickable(new Point(308, 128));
            Mouse.StartDragging(webRequest, new Point(14, 3));
            Mouse.StopDragging(flowchart, new Point(308, 128));
            Assert.AreEqual(Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams.Connector1Exists, connector1.Exists, "No connectors exist on design surface.");
            Assert.AreEqual(Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams.WebRequestExists, webRequest1.Exists, "Web Request on the design surface does not exist");
        }
        [When(@"I Drag Toolbox Write File Onto DesignSurface")]
        public void Drag_Toolbox_Write_File_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem writeFile = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.WriteFile;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            WpfCustom fileWrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite;
            #endregion
            searchTextBox.Text = Drag_Toolbox_Write_File_Onto_DesignSurfaceParams.SearchTextBoxText;
            Playback.Wait(1000);
            flowchart.EnsureClickable(new Point(306, 132));
            Mouse.StartDragging(writeFile, new Point(10, 18));
            Mouse.StopDragging(flowchart, new Point(306, 132));
            Assert.AreEqual(Drag_Toolbox_Write_File_Onto_DesignSurfaceParams.Connector1Exists, connector1.Exists, "No connectors exist on design surface.");
            Assert.AreEqual(Drag_Toolbox_Write_File_Onto_DesignSurfaceParams.FileWriteExists, fileWrite.Exists, "Write File tool on the design surface does not exist");
        }
        [When(@"I Drag Toolbox XPath Onto DesignSurface")]
        public void Drag_Toolbox_XPath_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem xPath = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.XPath;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            WpfCustom xPath1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath;
            #endregion
            searchTextBox.Text = Drag_Toolbox_XPath_Onto_DesignSurfaceParams.SearchTextBoxText;
            Playback.Wait(1500);
            flowchart.EnsureClickable(new Point(307, 123));
            Mouse.StartDragging(xPath, new Point(12, -13));
            Mouse.StopDragging(flowchart, new Point(307, 123));
            Assert.AreEqual(Drag_Toolbox_XPath_Onto_DesignSurfaceParams.Connector1Exists, connector1.Exists, "No connectors exist on design surface.");
            Assert.AreEqual(Drag_Toolbox_XPath_Onto_DesignSurfaceParams.XPathExists, xPath1.Exists, "XPath tool on the design surface does not exist");
        }
        [When(@"I Drag Toolbox Zip Onto DesignSurface")]
        public void Drag_Toolbox_Zip_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem zip = MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Zip;
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1;
            WpfCustom zip1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip;
            #endregion
            searchTextBox.Text = Drag_Toolbox_Zip_Onto_DesignSurfaceParams.SearchTextBoxText;
            Playback.Wait(1000);
            flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(zip, new Point(16, 4));
            Mouse.StopDragging(flowchart, new Point(306, 131));
            Assert.AreEqual(Drag_Toolbox_Zip_Onto_DesignSurfaceParams.Connector1Exists, connector1.Exists, "No connectors exist on design surface.");
            Assert.AreEqual(Drag_Toolbox_Zip_Onto_DesignSurfaceParams.ZipExists, zip1.Exists, "Zip tool on the design surface does not exist");
        }
        [When(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        public void Duplicate_Explorer_Localhost_First_Item_With_Context_Menu()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem duplicate = MainStudioWindow.ExplorerContextMenu.Duplicate;
            WpfWindow saveDialogWindow = SaveDialogWindow;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.AreEqual(Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams.DuplicateExists, duplicate.Exists, "Duplicate does not exist in explorer context menu.");
            Mouse.Click(duplicate, new Point(62, 10));
            Assert.AreEqual(Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams.SaveDialogWindowExists, saveDialogWindow.Exists, "Duplicate dialog does not exist after clicking duplicate in the explorer context " +
                    "menu.");
        }
        [When(@"I Enter DeployViewOnly Into Deploy Source Filter")]
        public void Enter_DeployViewOnly_Into_Deploy_Source_Filter()
        {
            #region Variable Declarations
            WpfEdit searchTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.SearchTextbox;
            WpfTreeItem firstExplorerTreeItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem;
            WpfCheckBox checkBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox;
            #endregion
            searchTextbox.Text = Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.SearchTextboxText;
            Assert.AreEqual(Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.FirstExplorerTreeItemExists, firstExplorerTreeItem.Exists, "First deploy tab source explorer item does not exist after filter is applied.");
            Assert.AreEqual(Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.CheckBoxExists, checkBox.Exists, "Deploy source server explorer tree first item checkbox does not exist.");
        }
        [When(@"I Enter Duplicate workflow name")]
        public void Enter_Duplicate_workflow_name()
        {
            #region Variable Declarations
            WpfEdit serviceNameTextBox = SaveDialogWindow.ServiceNameTextBox;
            #endregion
            serviceNameTextBox.Text = Enter_Duplicate_workflow_nameParams.ServiceNameTextBoxText;
        }
        [When(@"I Enter InputDebug value")]
        public void Enter_InputDebug_value()
        {
            #region Variable Declarations
            WpfPane row1 = MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1;
            WpfEdit inputValueText = MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText;
            #endregion
            Assert.AreEqual(Enter_InputDebug_valueParams.Row1Exists, row1.Exists, "InputData row does not exist.");
            Assert.AreEqual(Enter_InputDebug_valueParams.InputValueTextExists, inputValueText.Exists, "InputData row does not exist.");
            inputValueText.Text = Enter_InputDebug_valueParams.InputValueTextText;
        }
        [When(@"I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab")]
        public void Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_Tab()
        {
            #region Variable Declarations
            WpfEdit userNameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.UserNameTextBoxEdit;
            WpfEdit passwordTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.PasswordTextbox;
            #endregion
            userNameTextBoxEdit.Text = Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams.UserNameTextBoxEditText;
            passwordTextbox.Text = Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams.PasswordTextboxText;
        }
        [When(@"I Enter Public As Windows Group")]
        public void Enter_Public_As_Windows_Group()
        {
            #region Variable Declarations
            WpfEdit addWindowsGroupsEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit;
            #endregion
            addWindowsGroupsEdit.Text = Enter_Public_As_Windows_GroupParams.AddWindowsGroupsEditText;
        }
        [When(@"I Enter RunAsUser Username And Password")]
        public void Enter_RunAsUser_Username_And_Password()
        {
            #region Variable Declarations
            WpfEdit usernameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit;
            WpfEdit passwordTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit;
            #endregion
            usernameTextBoxEdit.Text = Enter_RunAsUser_Username_And_PasswordParams.UsernameTextBoxEditText;
            Keyboard.SendKeys(usernameTextBoxEdit, Enter_RunAsUser_Username_And_PasswordParams.UsernameTextBoxEditSendKeys, ModifierKeys.None);
            Keyboard.SendKeys(passwordTextBoxEdit, Enter_RunAsUser_Username_And_PasswordParams.PasswordTextBoxEditSendKeys, true);
        }
        [When(@"I Enter Sharepoint Server Path From OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnCopyFile_Tool()
        {
            #region Variable Declarations
            WpfEdit textEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.FromDirectoryComboBox.TextEdit;
            #endregion
            textEdit.Text = Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams.TextEditText;
        }
        [When(@"I Enter Sharepoint Server Path From OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnMoveFile_Tool()
        {
            #region Variable Declarations
            WpfEdit textEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.FromDirectoryComboBox.TextEdit;
            #endregion
            textEdit.Text = Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams.TextEditText;
        }
        [When(@"I Enter Sharepoint Server Path From OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnUpload_Tool()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.LocalPathFromIntellisenseCombobox.Textbox;
            #endregion
            textbox.Text = Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams.TextboxText;
        }
        [When(@"I Enter Sharepoint Server Path To OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnCopyFile_Tool()
        {
            #region Variable Declarations
            WpfEdit textEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.PathDirectoryComboBox.TextEdit;
            #endregion
            textEdit.Text = Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams.TextEditText;
        }
        [When(@"I Enter Sharepoint Server Path To OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnMoveFile_Tool()
        {
            #region Variable Declarations
            WpfEdit textEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.PathDirectoryComboBox.TextEdit;
            #endregion
            textEdit.Text = Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams.TextEditText;
        }
        [When(@"I Enter Sharepoint Server Path To OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnUpload_Tool()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ServerPathToIntellisenseCombobox.Textbox;
            #endregion
            textbox.Text = Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams.TextboxText;
        }
        [When(@"I Enter Sharepoint ServerSource ServerName")]
        public void Enter_Sharepoint_ServerSource_ServerName()
        {
            #region Variable Declarations
            WpfEdit serverNameEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit;
            #endregion
            serverNameEdit.Text = Enter_Sharepoint_ServerSource_ServerNameParams.ServerNameEditText;
        }
        [When(@"I Enter Sharepoint ServerSource User Credentials")]
        public void Enter_Sharepoint_ServerSource_User_Credentials()
        {
            #region Variable Declarations
            WpfEdit userNameTextBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox;
            WpfEdit passwordTextBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox;
            #endregion
            userNameTextBox.Text = Enter_Sharepoint_ServerSource_User_CredentialsParams.UserNameTextBoxText;
            Mouse.Click(passwordTextBox, new Point(89, 1));
            Keyboard.SendKeys(passwordTextBox, Enter_Sharepoint_ServerSource_User_CredentialsParams.PasswordTextBoxSendKeys, true);
        }
        [When(@"I Enter SomeData Into Base Convert Large View Row1 Value Textbox")]
        public void Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_Textbox()
        {
            #region Variable Declarations
            WpfEdit valueTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox;
            #endregion
            valueTextbox.Text = Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams.ValueTextboxText;
        }
        [When(@"I Enter SomeVariable Into Calculate Large View Function Textbox")]
        public void Enter_SomeVariable_Into_Calculate_Large_View_Function_Textbox()
        {
            #region Variable Declarations
            WpfEdit functionTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.FunctionTextbox;
            #endregion
            functionTextbox.Text = Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams.FunctionTextboxText;
            Assert.AreEqual(Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams.FunctionTextboxText1, functionTextbox.Text, "Calculate large view function textbox text does not equal \"[[SomeVariable]]\"");
        }
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            #endregion
            textbox.Text = Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams.TextboxText;
            Assert.AreEqual(Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams.TextboxText1, textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid" +
                    "%Variable]]\".");
        }
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            #endregion
            Keyboard.SendKeys(textbox, Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams.TextboxSendKeys, ModifierKeys.Shift);
            Keyboard.SendKeys(textbox, Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams.TextboxSendKeys1, ModifierKeys.None);
            Assert.AreEqual(Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams.TextboxText, textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" af" +
                    "ter selecting it from the intellisense using the keyboard.");
        }
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable On Unpinned Tab")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_Tab()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            #endregion
            Keyboard.SendKeys(textbox, Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams.TextboxSendKeys, ModifierKeys.None);
            Assert.AreEqual(Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams.TextboxText, textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" on" +
                    " unpinned tab after selecting it from the intellisense using the keyboard.");
        }
        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable Using Click Intellisense Suggestion")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            WpfListItem listItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.ListItem;
            #endregion
            textbox.Text = Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams.TextboxText;
            Mouse.Click(listItem, new Point(39, 10));
            Assert.AreEqual(Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams.TextboxText1, textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }
        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable UsingIntellisense")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox;
            #endregion
            Keyboard.SendKeys(textbox, Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams.TextboxSendKeys, ModifierKeys.None);
            Assert.AreEqual(Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams.TextboxText, textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }
        [When(@"I Enter Text Into Workflow Tests OutPutTable Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITest()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            #endregion
            Keyboard.SendKeys(textbox, Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams.TextboxSendKeys, ModifierKeys.None);
            Assert.AreEqual(Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams.TextboxText, textbox.Text, "Workflow tests output tabe row 1 value textbox text does not equal Helo User afte" +
                    "r typing that in.");
        }
        [When(@"I Enter Text Into Workflow Tests Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest()
        {
            #region Variable Declarations
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            #endregion
            Keyboard.SendKeys(textbox, Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams.TextboxSendKeys, ModifierKeys.None);
            Assert.AreEqual(Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams.TextboxText, textbox.Text, "Workflow tests row 1 value textbox text does not equal User after typing that in." +
                    "");
        }
        [When(@"I Enter Vaiablelist Items")]
        public void Enter_Vaiablelist_Items()
        {
            #region Variable Declarations
            WpfEdit nameTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox;
            WpfEdit nameTextbox1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox;
            WpfEdit nameTextbox2 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox;
            #endregion
            Mouse.Click(nameTextbox, new Point(62, 3));
            nameTextbox.Text = Enter_Vaiablelist_ItemsParams.NameTextboxText;
            Keyboard.SendKeys(nameTextbox, Enter_Vaiablelist_ItemsParams.NameTextboxSendKeys, ModifierKeys.None);
            Mouse.Click(nameTextbox1, new Point(82, 2));
            Keyboard.SendKeys(nameTextbox1, Enter_Vaiablelist_ItemsParams.NameTextboxSendKeys1, ModifierKeys.None);
            nameTextbox1.Text = Enter_Vaiablelist_ItemsParams.NameTextboxText1;
            Mouse.Click(nameTextbox2, new Point(84, 2));
            nameTextbox2.Text = Enter_Vaiablelist_ItemsParams.NameTextboxText2;
            Keyboard.SendKeys(nameTextbox2, Enter_Vaiablelist_ItemsParams.NameTextboxSendKeys2, ModifierKeys.None);
        }
        [When(@"I Filter variables")]
        public void Filter_variables()
        {
            #region Variable Declarations
            WpfText filterText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.FilterText;
            WpfEdit searchTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox;
            #endregion
            Assert.AreEqual(Filter_variablesParams.FilterTextExists, filterText.Exists, "Variable filter textbox does not exist");
            Mouse.Click(searchTextbox, new Point(89, 7));
            searchTextbox.Text = Filter_variablesParams.SearchTextboxText;
        }
        [When(@"I I Open Explorer First Item Context Menu")]
        public void I_Open_Explorer_First_Item_Context_Menu()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem showVersionHistory = MainStudioWindow.ExplorerContextMenu.ShowVersionHistory;
            WpfMenuItem viewSwagger = MainStudioWindow.ExplorerContextMenu.ViewSwagger;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Assert.AreEqual(I_Open_Explorer_First_Item_Context_MenuParams.ShowVersionHistoryExists, showVersionHistory.Exists, "Show version history does not exist after right clicking a resource");
            Assert.AreEqual(I_Open_Explorer_First_Item_Context_MenuParams.ViewSwaggerExists, viewSwagger.Exists, "View Swagger button does not exist");
            Assert.AreEqual(I_Open_Explorer_First_Item_Context_MenuParams.ViewSwaggerEnabled, viewSwagger.Enabled, "View swagger is disabled");
        }
        [When(@"I Move AcceptanceTestd To AcceptanceTestingResopurces")]
        public void Move_AcceptanceTestd_To_AcceptanceTestingResopurces()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfTreeItem secondItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem;
            #endregion
            secondItem.EnsureClickable(new Point(10, 10));
            Mouse.StartDragging(firstItem, new Point(94, 11));
            Mouse.StopDragging(secondItem, new Point(10, 10));
        }
        [When(@"I Move Dice Roll To Localhost")]
        public void Move_Dice_Roll_To_Localhost()
        {
            #region Variable Declarations
            WpfTreeItem secondItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem;
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            #endregion
            localhost.EnsureClickable(new Point(10, 10));
            Mouse.StartDragging(secondItem, new Point(92, 4));
            Mouse.StopDragging(localhost, new Point(10, 10));
        }
        [When(@"I Open AggregateCalculate Tool large view")]
        public void Open_AggregateCalculate_Tool_large_view()
        {
            #region Variable Declarations
            WpfCustom aggregateCalculat = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.DoneButton;
            WpfGroup onErrorGroup = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.OnErrorCustom.OnErrorGroup;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.ResultComboBox;
            WpfComboBox fxComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat.LargeViewContentCustom.fxComboBox;
            #endregion
            Mouse.DoubleClick(aggregateCalculat, new Point(136, 13));
            Assert.AreEqual(Open_AggregateCalculate_Tool_large_viewParams.DoneButtonExists, doneButton.Exists, "Done button does not exist after opening Aggregate Calculate tool large view");
            Assert.AreEqual(Open_AggregateCalculate_Tool_large_viewParams.OnErrorGroupExists, onErrorGroup.Exists, "On Error group does not exist after opening Aggregate Calculate tool large view");
            Assert.AreEqual(Open_AggregateCalculate_Tool_large_viewParams.ResultComboBoxExists, resultComboBox.Exists, "Results combobox does not exist after opening Aggregate Calculate tool large view" +
                    "");
            Assert.AreEqual(Open_AggregateCalculate_Tool_large_viewParams.fxComboBoxExists, fxComboBox.Exists, "fx combobox does not exist after opening Aggregate Calculate tool large view");
        }
        [When(@"I Open Assign Tool Large View")]
        public void Open_Assign_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom multiAssign = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            #endregion
            Mouse.DoubleClick(multiAssign, new Point(145, 5));
            Assert.AreEqual(Open_Assign_Tool_Large_ViewParams.MultiAssignExists, multiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }
        [When(@"I Open Assign Tool On Unpinned Tab Large View")]
        public void Open_Assign_Tool_On_Unpinned_Tab_Large_View()
        {
            #region Variable Declarations
            WpfCustom multiAssign = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            WpfComboBox intellisenseCombobox = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox;
            #endregion
            Mouse.DoubleClick(multiAssign, new Point(145, 5));
            Assert.AreEqual(Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams.MultiAssignExists, multiAssign.Exists, "Assign tool large view on the unpinned tab design surface does not exist");
            Assert.AreEqual(Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams.IntellisenseComboboxExists, intellisenseCombobox.Exists, "Assign large view row 1 variable textbox does not exist after openning large view" +
                    " with a double click on an unpinned tab.");
        }
        [When(@"I Open Assign Tool Qvi Large View")]
        public void Open_Assign_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Assign_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Assign_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on assign is not open");
        }
        [When(@"I Open Assign Tool Qvi Large View On Unpinned Tab")]
        public void Open_Assign_Tool_Qvi_Large_View_On_Unpinned_Tab()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton;
            WpfCustom quickVariableInputContent = MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on assign is not open");
        }
        [When(@"I Open AssignObject Large Tool")]
        public void Open_AssignObject_Large_Tool()
        {
            #region Variable Declarations
            WpfCustom assignObject = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton;
            WpfToggleButton openQuickVariableInput = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput;
            WpfRow row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1;
            WpfGroup onErrorGroup = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.OnError.OnErrorGroup;
            #endregion
            Mouse.DoubleClick(assignObject, new Point(159, 11));
            Assert.AreEqual(Open_AssignObject_Large_ToolParams.DoneButtonExists, doneButton.Exists, "Done button does not exist after dragging Assign Object tool on to the workflow s" +
                    "urface");
            Assert.AreEqual(Open_AssignObject_Large_ToolParams.OpenQuickVariableInputExists, openQuickVariableInput.Exists, "OpenQuickVariableInput button does not exist after dragging Assign Object tool on" +
                    " to the workflow surface");
            Assert.AreEqual(Open_AssignObject_Large_ToolParams.Row1Exists, row1.Exists, "Row1 does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.AreEqual(Open_AssignObject_Large_ToolParams.OnErrorGroupExists, onErrorGroup.Exists, "OnErrorGroup does not exist after dragging Assign Object tool on to the workflow " +
                    "surface");
        }
        [When(@"I Open AssignObject QVI LargeView")]
        public void Open_AssignObject_QVI_LargeView()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInput = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent;
            WpfComboBox qviSplitOnCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent.QviSplitOnCombobox;
            WpfCustom previewCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.QuickVariableInputContent.PreviewCustom;
            #endregion
            openQuickVariableInput.Pressed = Open_AssignObject_QVI_LargeViewParams.OpenQuickVariableInputPressed;
            Assert.AreEqual(Open_AssignObject_QVI_LargeViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on assign object is not open");
            Assert.AreEqual(Open_AssignObject_QVI_LargeViewParams.QviSplitOnComboboxExists, qviSplitOnCombobox.Exists, "QviSplitOnCombobox on assign object does not exist");
            Assert.AreEqual(Open_AssignObject_QVI_LargeViewParams.PreviewCustomExists, previewCustom.Exists, "Qvi PreviewCustom on assign object does not exist");
        }
        [When(@"I Open Base Conversion Tool Large View")]
        public void Open_Base_Conversion_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom baseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert;
            WpfEdit valueTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox;
            #endregion
            Mouse.DoubleClick(baseConvert, new Point(160, 15));
            Assert.AreEqual(Open_Base_Conversion_Tool_Large_ViewParams.ValueTextboxEnabled, valueTextbox.Enabled, "Base convert large view row 1 data testbox does not exist.");
        }
        [When(@"I Open Base Conversion Tool Qvi Large View")]
        public void Open_Base_Conversion_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.OpenQuickVariableInpToggleButton;
            WpfCustom baseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Base_Conversion_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Base_Conversion_Tool_Qvi_Large_ViewParams.BaseConvertExists, baseConvert.Exists, "Base Conversion QVI Window does not exist on the design surface");
            Assert.AreEqual(Open_Base_Conversion_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on BaseConvert is not open");
            Assert.AreEqual(Open_Base_Conversion_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists1, quickVariableInputContent.Exists, "QVI on BaseConvert is not open");
        }
        [When(@"I Open Calculate Tool Large View")]
        public void Open_Calculate_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom calculate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView;
            WpfControl listbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox;
            WpfEdit functionTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.Listbox.FunctionTextbox;
            #endregion
            Mouse.DoubleClick(calculate, new Point(105, 7));
            Assert.AreEqual(Open_Calculate_Tool_Large_ViewParams.LargeViewExists, largeView.Exists, "Calculate tool large view does not exist on design surface.");
            Assert.AreEqual(Open_Calculate_Tool_Large_ViewParams.ListboxExists, listbox.Exists, "Autocomplete listbox does not exisst on Calculate tool large view.");
            Assert.AreEqual(Open_Calculate_Tool_Large_ViewParams.FunctionTextboxExists, functionTextbox.Exists, "Function textbox does not exist on calculate tool large view.");
        }
        [When(@"I Open Case Conversion Tool Large View")]
        public void Open_Case_Conversion_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom caseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.DoneButton;
            WpfTable smallDataGridTable = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.SmallDataGridTable;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.LargeViewContentCustom.OnErrorCustom;
            #endregion
            Mouse.DoubleClick(caseConvert, new Point(136, 13));
            Assert.AreEqual(Open_Case_Conversion_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Done Button does not exist after opening Case Convert large view");
            Assert.AreEqual(Open_Case_Conversion_Tool_Large_ViewParams.SmallDataGridTableExists, smallDataGridTable.Exists, "Inputs grid does not exist after opening Case Convert large view");
            Assert.AreEqual(Open_Case_Conversion_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom group does not exist after opening Case Convert large view");
        }
        [When(@"I Open Case Conversion Tool Qvi Large View")]
        public void Open_Case_Conversion_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.OpenQuickVariableInpToggleButton;
            WpfCustom caseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Case_Conversion_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Case_Conversion_Tool_Qvi_Large_ViewParams.CaseConvertExists, caseConvert.Exists, "Case Conversion QVI Window does not exist on the design surface");
            Assert.AreEqual(Open_Case_Conversion_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on CaseConvert is not open");
        }
        [When(@"I Open CMD Line Tool Large View")]
        public void Open_CMD_Line_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom executeCommandLine = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine;
            WpfComboBox scriptIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.LargeViewContent.ScriptIntellisenseTextbox;
            WpfComboBox priorityComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.LargeViewContent.PriorityComboBox;
            WpfComboBox resultIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.LargeViewContent.ResultIntellisenseTextbox;
            WpfCustom onError = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.LargeViewContent.OnError;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine.DoneButton;
            #endregion
            Mouse.DoubleClick(executeCommandLine, new Point(174, 10));
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.ExecuteCommandLineExists, executeCommandLine.Exists, "CMD Line large view on the design surface does not exist");
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.ScriptIntellisenseTextboxExists, scriptIntellisenseTextbox.Exists, "CMD script textbox does not exist after openning tool large view with double clic" +
                    "k.");
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.PriorityComboBoxExists, priorityComboBox.Exists, "CMD script priority combobox does not exist after openning tool large view with d" +
                    "ouble click.");
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.ResultIntellisenseTextboxExists, resultIntellisenseTextbox.Exists, "CMD script result textbox does not exist after openning tool large view with doub" +
                    "le click.");
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.OnErrorExists, onError.Exists, "CMD script on error pane does not exist after openning tool large view with doubl" +
                    "e click.");
            Assert.AreEqual(Open_CMD_Line_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "CMD script done button does not exist after openning tool large view with double " +
                    "click.");
        }
        [When(@"I Open Context Menu OnDesignSurface")]
        public void Open_Context_Menu_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom flowchart = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            #endregion
            Mouse.Click(flowchart, MouseButtons.Right, ModifierKeys.None, new Point(304, 286));
        }
        [When(@"I Open Copy Tool Large View")]
        public void Open_Copy_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom pathCopy = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.DoneButton;
            WpfComboBox fileOrFolderComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox;
            WpfComboBox destinationComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.OnErrorCustom;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.OverwriteCheckBox;
            #endregion
            Mouse.DoubleClick(pathCopy, new Point(144, 11));
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.PathCopyExists, pathCopy.Exists, "Copy Tool large view on the design surface does not exist");
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton on the design surface does not exist");
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.FileOrFolderComboBoxExists, fileOrFolderComboBox.Exists, "FileOrFolderComboBox on the design surface does not exist");
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.DestinationComboBoxExists, destinationComboBox.Exists, "DestinationComboBox on the design surface does not exist");
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom group on the design surface does not exist");
            Assert.AreEqual(Open_Copy_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox on the design surface does not exist");
        }
        [When(@"I Open CountRecords Large View")]
        public void Open_CountRecords_Large_View()
        {
            #region Variable Declarations
            WpfCustom countRecordset = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset;
            #endregion
            Mouse.DoubleClick(countRecordset, new Point(130, 11));
        }
        [When(@"I Open Create JSON Large View")]
        public void Open_Create_JSON_Large_View()
        {
            #region Variable Declarations
            WpfCustom createJson = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson;
            #endregion
            Mouse.DoubleClick(createJson, new Point(124, 9));
        }
        [When(@"I Open Create Tool Large View")]
        public void Open_Create_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom pathCreate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate;
            WpfComboBox fileNameoComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.FileNameoComboBox;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OverwriteCheckBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OnErrorCustom;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.DoneButton;
            #endregion
            Mouse.DoubleClick(pathCreate, new Point(118, 13));
            Assert.AreEqual(Open_Create_Tool_Large_ViewParams.PathCreateExists, pathCreate.Exists, "Create Path large view on the design surface does not exist");
            Assert.AreEqual(Open_Create_Tool_Large_ViewParams.FileNameoComboBoxExists, fileNameoComboBox.Exists, "FileNameoComboBox on the design surface does not exist");
            Assert.AreEqual(Open_Create_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox on the design surface does not exist");
            Assert.AreEqual(Open_Create_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom group on the design surface does not exist");
            Assert.AreEqual(Open_Create_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton on the design surface does not exist");
        }
        [When(@"I Open Data Merge Large View")]
        public void Open_Data_Merge_Large_View()
        {
            #region Variable Declarations
            WpfCustom dataMerge = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge;
            #endregion
            Mouse.DoubleClick(dataMerge, new Point(185, 9));
            Assert.AreEqual(Open_Data_Merge_Large_ViewParams.DataMergeExists, dataMerge.Exists, "Data merge large view on the design surface does not exist");
        }
        [When(@"I Open Data Merge Tool Qvi Large View")]
        public void Open_Data_Merge_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.OpenQuickVariableInpToggleButton;
            WpfCustom dataMerge = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Data_Merge_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Data_Merge_Tool_Qvi_Large_ViewParams.DataMergeExists, dataMerge.Exists, "Data Merge QVi on the design surface does not exist");
            Assert.AreEqual(Open_Data_Merge_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on DataMerge is not open");
        }
        [When(@"I Open Data Split Large View")]
        public void Open_Data_Split_Large_View()
        {
            #region Variable Declarations
            WpfCustom dataSplit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit;
            #endregion
            Mouse.DoubleClick(dataSplit, new Point(203, 10));
            Assert.AreEqual(Open_Data_Split_Large_ViewParams.DataSplitExists, dataSplit.Exists, "Data Split large view on the design surface does not exist");
        }
        [When(@"I Open Data Split Tool Qvi Large View")]
        public void Open_Data_Split_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.OpenQuickVariableInpToggleButton;
            WpfCustom dataSplit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Data_Split_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Data_Split_Tool_Qvi_Large_ViewParams.DataSplitExists, dataSplit.Exists, "Data Split Qvi does not exist on the design surface");
            Assert.AreEqual(Open_Data_Split_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on DataSplit is not open");
        }
        [When(@"I Open DateTime LargeView")]
        public void Open_DateTime_LargeView()
        {
            #region Variable Declarations
            WpfCustom dateTime = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime;
            WpfComboBox addTimeAmountComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.AddTimeAmountComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox inputComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.LargeViewContentCustom.InputComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.DoneButton;
            #endregion
            Mouse.DoubleClick(dateTime, new Point(145, 7));
            Assert.AreEqual(Open_DateTime_LargeViewParams.AddTimeAmountComboBoxExists, addTimeAmountComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.AreEqual(Open_DateTime_LargeViewParams.OnErrorCustomExists, onErrorCustom.Exists, "ToComboBox does not exist on the large view");
            Assert.AreEqual(Open_DateTime_LargeViewParams.InputComboBoxExists, inputComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.AreEqual(Open_DateTime_LargeViewParams.DoneButtonExists, doneButton.Exists, "ToComboBox does not exist on the large view");
        }
        [When(@"I Open DateTimeDiff LargeView")]
        public void Open_DateTimeDiff_LargeView()
        {
            #region Variable Declarations
            WpfCustom dateTimeDifference = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.DoneButton;
            WpfComboBox inputFormatComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.LargeViewContentCustom.InputFormatComboBox;
            WpfComboBox input1ComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.LargeViewContentCustom.Input1ComboBox;
            #endregion
            Mouse.DoubleClick(dateTimeDifference, new Point(145, 7));
            Assert.AreEqual(Open_DateTimeDiff_LargeViewParams.DoneButtonExists, doneButton.Exists, "ToComboBox does not exist on the large view");
            Assert.AreEqual(Open_DateTimeDiff_LargeViewParams.InputFormatComboBoxExists, inputFormatComboBox.Exists, "ToComboBox does not exist on the large view");
            Assert.AreEqual(Open_DateTimeDiff_LargeViewParams.Input1ComboBoxExists, input1ComboBox.Exists, "ToComboBox does not exist on the large view");
        }
        [When(@"I Open Decision Large View")]
        public void Open_Decision_Large_View()
        {
            #region Variable Declarations
            WpfCustom decision = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision;
            WpfWindow decisionOrSwitchDialog = DecisionOrSwitchDialog;
            #endregion
            Mouse.DoubleClick(decision, new Point(55, 39));
            Assert.AreEqual(Open_Decision_Large_ViewParams.DecisionOrSwitchDialogExists, decisionOrSwitchDialog.Exists, "Decision Dialog does not exist after opening large Decision view");
        }
        [When(@"I Open Delete Tool Large View")]
        public void Open_Delete_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom pathDelete = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete;
            WpfComboBox fileNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileNameComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.OnErrorCustom;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.DoneButton;
            #endregion
            Mouse.DoubleClick(pathDelete, new Point(118, 13));
            Assert.AreEqual(Open_Delete_Tool_Large_ViewParams.PathDeleteExists, pathDelete.Exists, "Delete Path large view on the design surface does not exist");
            Assert.AreEqual(Open_Delete_Tool_Large_ViewParams.FileNameComboBoxExists, fileNameComboBox.Exists, "FileNameComboBox on the design surface does not exist");
            Assert.AreEqual(Open_Delete_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom on the design surface does not exist");
            Assert.AreEqual(Open_Delete_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton on the design surface does not exist");
        }
        [When(@"I Open DeleteRecords Large View")]
        public void Open_DeleteRecords_Large_View()
        {
            #region Variable Declarations
            WpfCustom deleteRecord = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.DoneButton;
            WpfGroup onErrorGroup = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.OnErrorCustom.OnErrorGroup;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.ResultComboBox;
            WpfComboBox recordsetComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.RecordsetComboBox;
            WpfCheckBox nullAsZeroCheckBoxCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.LargeViewContentCustom.NullAsZeroCheckBoxCheckBox;
            #endregion
            Mouse.DoubleClick(deleteRecord, new Point(133, 9));
            Assert.AreEqual(Open_DeleteRecords_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Done button does not exist after opening Delete Record large view");
            Assert.AreEqual(Open_DeleteRecords_Large_ViewParams.OnErrorGroupExists, onErrorGroup.Exists, "Error group does not exist after opening Delete Record large view");
            Assert.AreEqual(Open_DeleteRecords_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "Result combobox does not exist after opening Delete Record large view");
            Assert.AreEqual(Open_DeleteRecords_Large_ViewParams.RecordsetComboBoxExists, recordsetComboBox.Exists, "Recordset combobox does not exist after opening Delete Record large view");
            Assert.AreEqual(Open_DeleteRecords_Large_ViewParams.NullAsZeroCheckBoxCheckBoxExists, nullAsZeroCheckBoxCheckBox.Exists, "NullAszero checkbox does not exist after opening Delete Record large view");
        }
        [When(@"I Open DeleteWeb Tool Large View")]
        public void Open_DeleteWeb_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom webDelete = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView;
            #endregion
            Mouse.DoubleClick(webDelete, new Point(145, 5));
            Assert.AreEqual(Open_DeleteWeb_Tool_Large_ViewParams.LargeViewExists, largeView.Exists, "Web delete large view does not exist on the design surface");
        }
        [When(@"I Open DotNet DLL Connector Tool Large View")]
        public void Open_DotNet_DLL_Connector_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom dotNetDll = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll;
            #endregion
            Mouse.DoubleClick(dotNetDll, new Point(238, 16));
            Assert.AreEqual(Open_DotNet_DLL_Connector_Tool_Large_ViewParams.DotNetDllExists, dotNetDll.Exists, "DotNet DLL tool does not exist on the design surface");
        }
        [When(@"I Open Dropbox Delete Tool Large View With Double Click")]
        public void Open_Dropbox_Delete_Tool_Large_View_With_Double_Click()
        {
            #region Variable Declarations
            WpfCustom dropboxDelete = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete;
            WpfCustom largeViewContentCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContentCustom;
            #endregion
            Mouse.DoubleClick(dropboxDelete, new Point(174, 12));
            Assert.AreEqual(Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams.LargeViewContentCustomExists, largeViewContentCustom.Exists, "Tool large does not exist after openning it with a double click.");
        }
        [When(@"I Open Dropbox List Contents Tool Large View With Double Click")]
        public void Open_Dropbox_List_Contents_Tool_Large_View_With_Double_Click()
        {
            #region Variable Declarations
            WpfCustom dropboxFileList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList;
            WpfCustom largeViewContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent;
            #endregion
            Mouse.DoubleClick(dropboxFileList, new Point(166, 9));
            Assert.AreEqual(Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams.LargeViewContentExists, largeViewContent.Exists, "Tool large does not exist after openning it with a double click.");
        }
        [When(@"I Open Dropbox Upload Tool Large View With Double Click")]
        public void Open_Dropbox_Upload_Tool_Large_View_With_Double_Click()
        {
            #region Variable Declarations
            WpfCustom dropboxUpload = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload;
            WpfCustom largeViewContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.LargeViewContent;
            #endregion
            Mouse.DoubleClick(dropboxUpload, new Point(151, 8));
            Assert.AreEqual(Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams.LargeViewContentExists, largeViewContent.Exists, "Tool large does not exist after openning it with a double click.");
        }
        [When(@"I Open DropboxFileOperation Large View")]
        public void Open_DropboxFileOperation_Large_View()
        {
            #region Variable Declarations
            WpfCustom dropboxDownload = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload;
            #endregion
            Mouse.DoubleClick(dropboxDownload, new Point(174, 14));
        }
        [When(@"I Open Exchange Email Tool Large View")]
        public void Open_Exchange_Email_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom exchangeEmail = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail;
            #endregion
            Mouse.DoubleClick(exchangeEmail, new Point(168, 11));
        }
        [When(@"I Open ExecuteCommandline LargeView")]
        public void Open_ExecuteCommandline_LargeView()
        {
            #region Variable Declarations
            WpfCustom executeCommandLine = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine;
            #endregion
            Mouse.DoubleClick(executeCommandLine, new Point(178, 10));
        }
        [When(@"I Open Explorer First Item Tests With Context Menu")]
        public void Open_Explorer_First_Item_Tests_With_Context_Menu()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem tests = MainStudioWindow.ExplorerContextMenu.Tests;
            WpfButton runAllButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.RunAllButton;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.AreEqual(Open_Explorer_First_Item_Tests_With_Context_MenuParams.TestsExists, tests.Exists, "View tests does not exist in explorer context menu.");
            Mouse.Click(tests, new Point(30, 11));
            Assert.AreEqual(Open_Explorer_First_Item_Tests_With_Context_MenuParams.RunAllButtonExists, runAllButton.Exists, "Run all button does not exist on tests tab");
        }
        [When(@"I Open Explorer First Item Version History With Context Menu")]
        public void Open_Explorer_First_Item_Version_History_With_Context_Menu()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem showVersionHistory = MainStudioWindow.ExplorerContextMenu.ShowVersionHistory;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(showVersionHistory, new Point(66, 15));
        }
        [When(@"I Open Explorer First Item With Context Menu")]
        public void Open_Explorer_First_Item_With_Context_Menu()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem open = MainStudioWindow.ExplorerContextMenu.Open;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.AreEqual(Open_Explorer_First_Item_With_Context_MenuParams.OpenExists, open.Exists, "Open does not exist in explorer context menu.");
            Mouse.Click(open, new Point(30, 11));
        }
        [When(@"I Open Find Index Tool Large View")]
        public void Open_Find_Index_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom findIndex = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.ResultComboBox;
            WpfComboBox inFieldComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.InFieldComboBox;
            WpfComboBox indexComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.IndexComboBox;
            WpfComboBox charactersComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.CharactersComboBox;
            WpfComboBox directionComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.LargeViewContentCustom.DirectionComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.DoneButton;
            #endregion
            Mouse.DoubleClick(findIndex, new Point(147, 11));
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "ResultComboBox does not exist after opening large Find Index view");
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.InFieldComboBoxExists, inFieldComboBox.Exists, "InFieldComboBox does not exist after opening large Find Index view");
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.IndexComboBoxExists, indexComboBox.Exists, "IndexComboBox does not exist after opening large Find Index view");
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.CharactersComboBoxExists, charactersComboBox.Exists, "CharactersComboBox does not exist after opening large Find Index view");
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.DirectionComboBoxExists, directionComboBox.Exists, "DirectionComboBox does not exist after opening large Find Index view");
            Assert.AreEqual(Open_Find_Index_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist after opening large Find Index view");
        }
        [When(@"I Open Find Record Index Tool Large View")]
        public void Open_Find_Record_Index_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom findRecordsIndex = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex;
            #endregion
            Mouse.DoubleClick(findRecordsIndex, new Point(172, 5));
        }
        [When(@"I Open ForEach Large View")]
        public void Open_ForEach_Large_View()
        {
            #region Variable Declarations
            WpfCustom forEach = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach;
            WpfComboBox typeCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.TypeCombobox;
            WpfComboBox forEachFromIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ForEachFromIntellisenseTextbox;
            WpfComboBox toIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.ToIntellisenseTextbox;
            WpfCustom dropActivityHere = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.DropActivityHere;
            WpfCustom onErrorPane = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.LargeView.OnErrorPane;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.DoneButton;
            #endregion
            Mouse.DoubleClick(forEach, new Point(131, 14));
            Assert.AreEqual(Open_ForEach_Large_ViewParams.TypeComboboxExists, typeCombobox.Exists, "ForEach large view type combobox does not exist after double clicking tool to ope" +
                    "n large view.");
            Assert.AreEqual(Open_ForEach_Large_ViewParams.ForEachFromIntellisenseTextboxExists, forEachFromIntellisenseTextbox.Exists, "Foreach from textbox does not exist after openning large view with a double click" +
                    ".");
            Assert.AreEqual(Open_ForEach_Large_ViewParams.ToIntellisenseTextboxExists, toIntellisenseTextbox.Exists, "For each to textbox does not exist after double click openning large view.");
            Assert.AreEqual(Open_ForEach_Large_ViewParams.DropActivityHereExists, dropActivityHere.Exists, "For each activity drop box does not exist after openning large view with a double" +
                    " click.");
            Assert.AreEqual(Open_ForEach_Large_ViewParams.OnErrorPaneExists, onErrorPane.Exists, "For each OnError pane does not exist after double click openning large view.");
            Assert.AreEqual(Open_ForEach_Large_ViewParams.DoneButtonExists, doneButton.Exists, "For each done button does not exist after double click openning large view.");
        }
        [When(@"I Open GET Web Connector Tool Large View")]
        public void Open_GET_Web_Connector_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom webGet = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet;
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox;
            WpfButton generateOutputsButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton;
            #endregion
            Mouse.DoubleClick(webGet, new Point(238, 16));
            Assert.AreEqual(Open_GET_Web_Connector_Tool_Large_ViewParams.SourcesComboBoxExists, sourcesComboBox.Exists, "Web GET large view sources combobox does not exist.");
            Assert.AreEqual(Open_GET_Web_Connector_Tool_Large_ViewParams.GenerateOutputsButtonExists, generateOutputsButton.Exists, "Web GET large view generate inputs button does not exist.");
            Assert.AreEqual(Open_GET_Web_Connector_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Web GET large view done does not exist.");
        }
        [When(@"I Open GetWeb RequestTool small View")]
        public void Open_GetWeb_RequestTool_small_View()
        {
            #region Variable Declarations
            WpfCustom webRequest = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest;
            #endregion
            Mouse.DoubleClick(webRequest, new Point(237, 7));
        }
        [When(@"I Open Javascript Large View")]
        public void Open_Javascript_Large_View()
        {
            #region Variable Declarations
            WpfCustom javascript = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript;
            WpfComboBox scriptIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.ScriptIntellisenseCombobox;
            WpfComboBox attachmentsIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.AttachmentsIntellisenseCombobox;
            WpfButton attachFileButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.AttachFileButton;
            WpfCheckBox escapesequencesCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.EscapesequencesCheckBox;
            WpfComboBox resultIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.ResultIntellisenseCombobox;
            WpfCustom onErrorPane = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.LargeView.OnErrorPane;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript.DoneButton;
            #endregion
            Mouse.DoubleClick(javascript, new Point(115, 14));
            Assert.AreEqual(Open_Javascript_Large_ViewParams.ScriptIntellisenseComboboxExists, scriptIntellisenseCombobox.Exists, "Javascript script textbox does not exist after openning large view with a double " +
                    "click.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.AttachmentsIntellisenseComboboxExists, attachmentsIntellisenseCombobox.Exists, "Javascript Attachments textbox does not exist after openning large view with a do" +
                    "uble click.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.AttachFileButtonExists, attachFileButton.Exists, "Javascript Attach File Button does not exist after openning large view with a dou" +
                    "ble click.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.EscapesequencesCheckBoxExists, escapesequencesCheckBox.Exists, "Javascript escape sequences checkbox does not exist after openning large view wit" +
                    "h a double click.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.ResultIntellisenseComboboxExists, resultIntellisenseCombobox.Exists, "Javascript result textbox does not exist after openning large view with a double " +
                    "click.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.OnErrorPaneExists, onErrorPane.Exists, "Javascript OnError pane does not exist after openning large view with a double cl" +
                    "ick.");
            Assert.AreEqual(Open_Javascript_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Javascript Done button does not exist after openning large view with a double cli" +
                    "ck.");
        }
        [When(@"I Open Json Tool Large View")]
        public void Open_Json_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom createJson = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson;
            #endregion
            Mouse.DoubleClick(createJson, new Point(158, 13));
            Assert.AreEqual(Open_Json_Tool_Large_ViewParams.CreateJsonExists, createJson.Exists, "JSON tool large view on the design surface does not exist");
        }
        [When(@"I Open Json Tool Qvi Large View")]
        public void Open_Json_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.OpenQuickVariableInpToggleButton;
            WpfCustom createJson = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Json_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Json_Tool_Qvi_Large_ViewParams.CreateJsonExists, createJson.Exists, "JSON QVI window does not exist on the design surface");
        }
        [When(@"I Open Length Tool Large View")]
        public void Open_Length_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom length = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length;
            WpfComboBox recordsetComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.RecordsetComboBox;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.ResultComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.OnErrorCustom;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.DoneButton;
            WpfCheckBox nullAsZeroCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.LargeViewContentCustom.NullAsZeroCheckBox;
            #endregion
            Mouse.DoubleClick(length, new Point(136, 13));
            Assert.AreEqual(Open_Length_Tool_Large_ViewParams.RecordsetComboBoxExists, recordsetComboBox.Exists, "Recordset combobox does not exist after dragging Recordset Length on to Workflow " +
                    "surface");
            Assert.AreEqual(Open_Length_Tool_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "Result combobox does not exist after dragging Recordset Length on to Workflow sur" +
                    "face");
            Assert.AreEqual(Open_Length_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "On Error pane does not exist after dragging Recordset Length on to Workflow surfa" +
                    "ce");
            Assert.AreEqual(Open_Length_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist after dragging Recordset Length on to Workflow surface");
            Assert.AreEqual(Open_Length_Tool_Large_ViewParams.NullAsZeroCheckBoxExists, nullAsZeroCheckBox.Exists, "NullAsZero checkbox is does not exist after dragging Recordset Length on to Workf" +
                    "low surface");
        }
        [When(@"I Open Move Tool Large View")]
        public void Open_Move_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom pathMove = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox destinationComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.DoneButton;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox;
            #endregion
            Mouse.DoubleClick(pathMove, new Point(125, 6));
            Assert.AreEqual(Open_Move_Tool_Large_ViewParams.PathMoveExists, pathMove.Exists, "Move tool large view does not exist on the design surface");
            Assert.AreEqual(Open_Move_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom group does not exist on the design surface");
            Assert.AreEqual(Open_Move_Tool_Large_ViewParams.DestinationComboBoxExists, destinationComboBox.Exists, "DestinationComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Move_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the design surface");
            Assert.AreEqual(Open_Move_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
        }
        [When(@"I Open MySql Database Tool Large View")]
        public void Open_MySql_Database_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom mySqlDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase;
            #endregion
            Mouse.DoubleClick(mySqlDatabase, new Point(238, 15));
        }
        [When(@"I Open NumberFormat Toolbox Large View")]
        public void Open_NumberFormat_Toolbox_Large_View()
        {
            #region Variable Declarations
            WpfCustom formatNumber = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.DoneButton;
            WpfGroup onErrorGroup = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.OnErrorCustom.OnErrorGroup;
            WpfComboBox resultInputComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.ResultInputComboBox;
            WpfComboBox decimalsToShowComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.DecimalsToShowComboBox;
            WpfComboBox roundingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox;
            WpfComboBox numberInputComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.NumberInputComboBox;
            #endregion
            Mouse.DoubleClick(formatNumber, new Point(145, 5));
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Done button does not exist after opening  Format Number tool large view");
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.OnErrorGroupExists, onErrorGroup.Exists, "On Error group does not exist after opening  Format Number tool large view");
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.ResultInputComboBoxExists, resultInputComboBox.Exists, "Reult combobox does not exist after opening  Format Number tool large view");
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.DecimalsToShowComboBoxExists, decimalsToShowComboBox.Exists, "DecimalToShow combobox does not exist after opening  Format Number tool large vie" +
                    "w");
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.RoundingComboBoxExists, roundingComboBox.Exists, "Rounding combobox does not exist after opening  Format Number tool large view");
            Assert.AreEqual(Open_NumberFormat_Toolbox_Large_ViewParams.NumberInputComboBoxExists, numberInputComboBox.Exists, "NumberInput combobox does not exist after opening  Format Number tool large view");
        }
        [When(@"I Open ODBC Tool Large View")]
        public void Open_ODBC_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom oDBCDatabaseActivCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom;
            WpfCustom largeViewContentCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeViewContentCustom;
            #endregion
            Mouse.DoubleClick(oDBCDatabaseActivCustom, new Point(145, 5));
            Assert.AreEqual(Open_ODBC_Tool_Large_ViewExpectedValues.LargeViewContentCustomExists, largeViewContentCustom.Exists, "ODBC tool large view does not exist on the design surface.");
        }
        [When(@"I Open Oracle Tool Large View")]
        public void Open_Oracle_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom oracleDatabaseActCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom;
            WpfCustom largeViewContentCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeViewContentCustom;
            #endregion
            Mouse.DoubleClick(oracleDatabaseActCustom, new Point(145, 5));
            Assert.AreEqual(Open_Oracle_Tool_Large_ViewExpectedValues.LargeViewContentCustomExists, largeViewContentCustom.Exists, "Oracle tool large view does not exist on the design surface.");
        }
        [When(@"I Open Postgre Tool Large View")]
        public void Open_Postgre_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom postgreSqlActivitCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom;
            WpfCustom largeViewContentCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeViewContentCustom;
            #endregion
            Mouse.DoubleClick(postgreSqlActivitCustom, new Point(145, 5));
            Assert.AreEqual(Open_Postgre_Tool_Large_ViewExpectedValues.LargeViewContentCustomExists, largeViewContentCustom.Exists, "Postgre tool large view does not exist on the design surface.");
        }
        [When(@"I Open PostWeb RequestTool Large View")]
        public void Open_PostWeb_RequestTool_Large_View()
        {
            #region Variable Declarations
            WpfCustom webPost = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView;
            #endregion
            Mouse.DoubleClick(webPost, new Point(128, 8));
            Assert.AreEqual(Open_PostWeb_RequestTool_Large_ViewParams.LargeViewExists, largeView.Exists, "Post web request large view does not exist on design surface.");
        }
        [When(@"I Open PutWeb Tool large view")]
        public void Open_PutWeb_Tool_large_view()
        {
            #region Variable Declarations
            WpfCustom webPut = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut;
            #endregion
            Mouse.DoubleClick(webPut, new Point(145, 5));
        }
        [When(@"I Open Python Large View")]
        public void Open_Python_Large_View()
        {
            #region Variable Declarations
            WpfCustom python = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python;
            WpfComboBox scriptIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ScriptIntellisenseCombobox;
            WpfComboBox attachmentsIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachmentsIntellisenseCombobox;
            WpfButton attachFileButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachFileButton;
            WpfCheckBox escapesequencesCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.EscapesequencesCheckBox;
            WpfComboBox resultIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ResultIntellisenseCombobox;
            WpfCustom onErrorPane = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.OnErrorPane;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.DoneButton;
            #endregion
            Mouse.DoubleClick(python, new Point(117, 9));
            Assert.AreEqual(Open_Python_Large_ViewParams.ScriptIntellisenseComboboxExists, scriptIntellisenseCombobox.Exists, "Python script textbox does not exist after openning large view with a double clic" +
                    "k.");
            Assert.AreEqual(Open_Python_Large_ViewParams.AttachmentsIntellisenseComboboxExists, attachmentsIntellisenseCombobox.Exists, "Python Attachments textbox does not exist after openning large view with a double" +
                    " click.");
            Assert.AreEqual(Open_Python_Large_ViewParams.AttachFileButtonExists, attachFileButton.Exists, "Python Attach File Button does not exist after openning large view with a double " +
                    "click.");
            Assert.AreEqual(Open_Python_Large_ViewParams.EscapesequencesCheckBoxExists, escapesequencesCheckBox.Exists, "Python escape sequences checkbox does not exist after openning large view with a " +
                    "double click.");
            Assert.AreEqual(Open_Python_Large_ViewParams.ResultIntellisenseComboboxExists, resultIntellisenseCombobox.Exists, "Python result textbox does not exist after openning large view with a double clic" +
                    "k.");
            Assert.AreEqual(Open_Python_Large_ViewParams.OnErrorPaneExists, onErrorPane.Exists, "Python OnError pane does not exist after openning large view with a double click." +
                    "");
            Assert.AreEqual(Open_Python_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Python Done button does not exist after openning large view with a double click.");
        }
        [When(@"I Open RabbitMqConsume LargeView")]
        public void Open_RabbitMqConsume_LargeView()
        {
            #region Variable Declarations
            WpfCustom rabbitMQConsume = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume;
            WpfComboBox responseComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.ResponseComboBox;
            WpfCheckBox acknowledgeCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.AcknowledgeCheckBox;
            WpfComboBox sourceComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.SourceComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.DoneButton;
            #endregion
            Mouse.DoubleClick(rabbitMQConsume, new Point(145, 7));
            Assert.AreEqual(Open_RabbitMqConsume_LargeViewParams.ResponseComboBoxExists, responseComboBox.Exists, "ResponseComboBox does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqConsume_LargeViewParams.AcknowledgeCheckBoxExists, acknowledgeCheckBox.Exists, "AcknowledgeCheckBox does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqConsume_LargeViewParams.SourceComboBoxExists, sourceComboBox.Exists, "SourceComboBox does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqConsume_LargeViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the large view");
        }
        [When(@"I Open RabbitMqPublish LargeView")]
        public void Open_RabbitMqPublish_LargeView()
        {
            #region Variable Declarations
            WpfCustom rabbitMQPublish = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish;
            WpfCheckBox durableCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.DurableCheckBox;
            WpfButton newSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.NewSourceButton;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.OnErrorCustom;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.DoneButton;
            #endregion
            Mouse.DoubleClick(rabbitMQPublish, new Point(145, 7));
            Assert.AreEqual(Open_RabbitMqPublish_LargeViewParams.DurableCheckBoxExists, durableCheckBox.Exists, "DurableCheckBox does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqPublish_LargeViewParams.NewSourceButtonExists, newSourceButton.Exists, "NewSourceButton does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqPublish_LargeViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqPublish_LargeViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the large view");
            Assert.AreEqual(Open_RabbitMqPublish_LargeViewParams.RabbitMQPublishExists, rabbitMQPublish.Exists, "RabbitMQPublish does not exist on the large view");
        }
        [When(@"I Open Random Large Tool")]
        public void Open_Random_Large_Tool()
        {
            #region Variable Declarations
            WpfCustom random = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.DoneButton;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox fromComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.FromComboBox;
            WpfComboBox toComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.LargeViewContentCustom.ToComboBox;
            #endregion
            Mouse.DoubleClick(random, new Point(145, 7));
            Assert.AreEqual(Open_Random_Large_ToolParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the large view");
            Assert.AreEqual(Open_Random_Large_ToolParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the large view");
            Assert.AreEqual(Open_Random_Large_ToolParams.FromComboBoxExists, fromComboBox.Exists, "FromComboBox does not exist on the large view");
            Assert.AreEqual(Open_Random_Large_ToolParams.ToComboBoxExists, toComboBox.Exists, "ToComboBox does not exist on the large view");
        }
        [When(@"I Open Read File Tool Large View")]
        public void Open_Read_File_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom fileRead = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox fileNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.DoneButton;
            #endregion
            Mouse.DoubleClick(fileRead, new Point(120, 5));
            Assert.AreEqual(Open_Read_File_Tool_Large_ViewParams.FileReadExists, fileRead.Exists, "Read file large view does not exist on the design surface");
            Assert.AreEqual(Open_Read_File_Tool_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "ResultComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Read_File_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.AreEqual(Open_Read_File_Tool_Large_ViewParams.FileNameComboBoxExists, fileNameComboBox.Exists, "FileNameComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Read_File_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the design surface");
        }
        [When(@"I Open Read Folder Tool Large View")]
        public void Open_Read_Folder_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom folderRead = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead;
            WpfRadioButton filesFoldersRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox directoryComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox;
            #endregion
            Mouse.DoubleClick(folderRead, new Point(138, 14));
            Assert.AreEqual(Open_Read_Folder_Tool_Large_ViewParams.FolderReadExists, folderRead.Exists, "Read Folder large view does not exist on the design surface");
            Assert.AreEqual(Open_Read_Folder_Tool_Large_ViewParams.FilesFoldersRadioButtonExists, filesFoldersRadioButton.Exists, "FilesFoldersRadioButton does not exist on the design surface");
            Assert.AreEqual(Open_Read_Folder_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom group does not exist on the design surface");
            Assert.AreEqual(Open_Read_Folder_Tool_Large_ViewParams.DirectoryComboBoxExists, directoryComboBox.Exists, "DirectoryComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Read_Folder_Tool_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "Read Folder large view does not exist on the design surface");
        }
        [When(@"I Open Rename Tool Large View")]
        public void Open_Rename_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom pathRename = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OnErrorCustom;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox;
            WpfComboBox fileOrFolderComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.DoneButton;
            #endregion
            Mouse.DoubleClick(pathRename, new Point(159, 11));
            Assert.AreEqual(Open_Rename_Tool_Large_ViewParams.PathRenameExists, pathRename.Exists, "Rename tool large view on the design surface does not exist");
            Assert.AreEqual(Open_Rename_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.AreEqual(Open_Rename_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.AreEqual(Open_Rename_Tool_Large_ViewParams.FileOrFolderComboBoxExists, fileOrFolderComboBox.Exists, "FileOrFolderComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Rename_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the design surface");
        }
        [When(@"I Open Replace Tool Large View")]
        public void Open_Replace_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom replace = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.DoneButton;
            WpfComboBox resultComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.ResultComboBox;
            WpfComboBox replaceComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.ReplaceComboBox;
            WpfComboBox findComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.FindComboBox;
            WpfComboBox inFiledsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.LargeViewContentCustom.InFiledsComboBox;
            #endregion
            Mouse.DoubleClick(replace, new Point(159, 11));
            Assert.AreEqual(Open_Replace_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Done button does not exist after opening Replace tool large view");
            Assert.AreEqual(Open_Replace_Tool_Large_ViewParams.ResultComboBoxExists, resultComboBox.Exists, "Result combobox does not exist after opening Replace tool large view");
            Assert.AreEqual(Open_Replace_Tool_Large_ViewParams.ReplaceComboBoxExists, replaceComboBox.Exists, "Replace combobox does not exist after opening Replace tool large view");
            Assert.AreEqual(Open_Replace_Tool_Large_ViewParams.FindComboBoxExists, findComboBox.Exists, "Find combobox does not exist after opening Replace tool large view");
            Assert.AreEqual(Open_Replace_Tool_Large_ViewParams.InFiledsComboBoxExists, inFiledsComboBox.Exists, "InFields combobox does not exist after opening Replace tool large view");
        }
        [When(@"I Open Ruby Large View")]
        public void Open_Ruby_Large_View()
        {
            #region Variable Declarations
            WpfCustom ruby = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby;
            WpfComboBox scriptIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ScriptIntellisenseCombobox;
            WpfComboBox attachmentsIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachmentsIntellisenseCombobox;
            WpfButton attachFileButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.AttachFileButton;
            WpfCheckBox escapesequencesCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.EscapesequencesCheckBox;
            WpfComboBox resultIntellisenseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.ResultIntellisenseCombobox;
            WpfCustom onErrorPane = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.LargeView.OnErrorPane;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.DoneButton;
            #endregion
            Mouse.DoubleClick(ruby, new Point(116, 12));
            Assert.AreEqual(Open_Ruby_Large_ViewParams.ScriptIntellisenseComboboxExists, scriptIntellisenseCombobox.Exists, "Ruby script textbox does not exist after openning large view with a double click." +
                    "");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.AttachmentsIntellisenseComboboxExists, attachmentsIntellisenseCombobox.Exists, "Ruby Attachments textbox does not exist after openning large view with a double c" +
                    "lick.");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.AttachFileButtonExists, attachFileButton.Exists, "Ruby Attach File Button does not exist after openning large view with a double cl" +
                    "ick.");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.EscapesequencesCheckBoxExists, escapesequencesCheckBox.Exists, "Ruby escape sequences checkbox does not exist after openning large view with a do" +
                    "uble click.");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.ResultIntellisenseComboboxExists, resultIntellisenseCombobox.Exists, "Ruby result textbox does not exist after openning large view with a double click." +
                    "");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.OnErrorPaneExists, onErrorPane.Exists, "Ruby OnError pane does not exist after openning large view with a double click.");
            Assert.AreEqual(Open_Ruby_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Ruby Done button does not exist after openning large view with a double click.");
        }
        [When(@"I Open Selectandapply Large View")]
        public void Open_Selectandapply_Large_View()
        {
            #region Variable Declarations
            WpfCustom selectAndApply = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.DoneButton;
            WpfComboBox selectFromIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.SelectFromIntellisenseTextbox;
            WpfComboBox aliasIntellisenseTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.AliasIntellisenseTextbox;
            WpfCustom dropActivityHere = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.DropActivityHere;
            WpfCustom onErrorPane = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.LargeView.OnErrorPane;
            #endregion
            Mouse.DoubleClick(selectAndApply, new Point(129, 10));
            Assert.AreEqual(Open_Selectandapply_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Select and apply done button does not exist after openning tool large view with d" +
                    "ouble click.");
            Assert.AreEqual(Open_Selectandapply_Large_ViewParams.SelectFromIntellisenseTextboxExists, selectFromIntellisenseTextbox.Exists, "Select and apply select from textbox does not exist after openning tool large vie" +
                    "w with double click.");
            Assert.AreEqual(Open_Selectandapply_Large_ViewParams.AliasIntellisenseTextboxExists, aliasIntellisenseTextbox.Exists, "Select and apply alias textbox does not exist after openning tool large view with" +
                    " double click.");
            Assert.AreEqual(Open_Selectandapply_Large_ViewParams.DropActivityHereExists, dropActivityHere.Exists, "Select and apply activity drop box does not exist after openning tool large view " +
                    "with double click.");
            Assert.AreEqual(Open_Selectandapply_Large_ViewParams.OnErrorPaneExists, onErrorPane.Exists, "Select and apply OnError pane does not exist after openning tool large view with " +
                    "double click.");
        }
        [When(@"I Open Sequence Large tool View")]
        public void Open_Sequence_Large_tool_View()
        {
            #region Variable Declarations
            WpfCustom sequence = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence;
            WpfCustom sequenceLargeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView;
            #endregion
            Mouse.DoubleClick(sequence, new Point(139, 12));
            Assert.AreEqual(Open_Sequence_Large_tool_ViewParams.SequenceLargeViewExists, sequenceLargeView.Exists, "SequenceLargeView does not exist after opening Sequence tool large view");
        }
        [When(@"I Open Sharepoint Copy Tool Large View")]
        public void Open_Sharepoint_Copy_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointCopyFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile;
            #endregion
            Mouse.DoubleClick(sharepointCopyFile, new Point(230, 11));
        }
        [When(@"I Open Sharepoint Create Tool Large View")]
        public void Open_Sharepoint_Create_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointCreateListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem;
            #endregion
            Mouse.DoubleClick(sharepointCreateListItem, new Point(195, 11));
        }
        [When(@"I Open Sharepoint Delete Tool Large View")]
        public void Open_Sharepoint_Delete_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointDeleteFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile;
            #endregion
            Mouse.DoubleClick(sharepointDeleteFile, new Point(218, 11));
        }
        [When(@"I Open Sharepoint Download File Tool Large View With Double Click")]
        public void Open_Sharepoint_Download_File_Tool_Large_View_With_Double_Click()
        {
            #region Variable Declarations
            WpfCustom sharepointDownloadFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile;
            #endregion
            Mouse.DoubleClick(sharepointDownloadFile, new Point(185, 9));
        }
        [When(@"I Open Sharepoint MoveFile Tool Large View")]
        public void Open_Sharepoint_MoveFile_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointMoveFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile;
            #endregion
            Mouse.DoubleClick(sharepointMoveFile, new Point(230, 11));
        }
        [When(@"I Open Sharepoint Read Folder Tool Large View")]
        public void Open_Sharepoint_Read_Folder_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointReadFolder = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder;
            #endregion
            Mouse.DoubleClick(sharepointReadFolder, new Point(195, 7));
        }
        [When(@"I Open Sharepoint Read List Item Tool Large View")]
        public void Open_Sharepoint_Read_List_Item_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointReadListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem;
            #endregion
            Mouse.DoubleClick(sharepointReadListItem, new Point(195, 7));
        }
        [When(@"I Open Sharepoint Update Tool Large View")]
        public void Open_Sharepoint_Update_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointUpdate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate;
            #endregion
            Mouse.DoubleClick(sharepointUpdate, new Point(230, 11));
        }
        [When(@"I Open Sharepoint Upload Tool Large View")]
        public void Open_Sharepoint_Upload_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sharepointUploadFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile;
            #endregion
            Mouse.DoubleClick(sharepointUploadFile, new Point(230, 11));
        }
        [When(@"I Open SMTP Email Tool Large View")]
        public void Open_SMTP_Email_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sMTPEmail = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeView;
            #endregion
            Mouse.DoubleClick(sMTPEmail, new Point(168, 11));
            Assert.AreEqual(Open_SMTP_Email_Tool_Large_ViewParams.LargeViewExists, largeView.Exists, "Email Tool large view does not exist on the design surface");
        }
        [When(@"I Open SortRecords Large View")]
        public void Open_SortRecords_Large_View()
        {
            #region Variable Declarations
            WpfCustom sortRecords = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords;
            #endregion
            Mouse.DoubleClick(sortRecords, new Point(114, 13));
        }
        [When(@"I Open SQL Bulk Insert Tool Large View")]
        public void Open_SQL_Bulk_Insert_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sqlBulkInsert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert;
            #endregion
            Mouse.DoubleClick(sqlBulkInsert, new Point(157, 6));
            Assert.AreEqual(Open_SQL_Bulk_Insert_Tool_Large_ViewParams.SqlBulkInsertExists, sqlBulkInsert.Exists, "Sql Bulk Insert large view on the design surface does not exist");
        }
        [When(@"I Open SQL Bulk Insert Tool Qvi Large View")]
        public void Open_SQL_Bulk_Insert_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.OpenQuickVariableInpToggleButton;
            WpfCustom sqlBulkInsert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams.SqlBulkInsertExists, sqlBulkInsert.Exists, "Sql Bulk Insert Qvi window on the design surface does not exist");
        }
        [When(@"I Open SQL Large View FromContextMenu")]
        public void Open_SQL_Large_View_FromContextMenu()
        {
            #region Variable Declarations
            WpfCustom sqlServerDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase;
            WpfMenuItem showLargeView = MainStudioWindow.DesignSurfaceContextMenu.ShowLargeView;
            WpfButton newDbSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton;
            #endregion
            Mouse.Click(sqlServerDatabase, MouseButtons.Right, ModifierKeys.None, new Point(143, 6));
            Mouse.Click(showLargeView, new Point(43, 15));
            Assert.AreEqual(Open_SQL_Large_View_FromContextMenuParams.NewDbSourceButtonExists, newDbSourceButton.Exists, "\"New button does not exist\"");
        }
        [When(@"I Open Sql Server Tool Large View")]
        public void Open_Sql_Server_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom sqlServerDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView;
            #endregion
            Mouse.DoubleClick(sqlServerDatabase, new Point(145, 5));
            Assert.AreEqual(Open_Sql_Server_Tool_Large_ViewExpectedValues.LargeViewExists, largeView.Exists, "SQL Server tool large view does not exist on the design surface.");
        }
        [When(@"I Open Sql Server Tool small View")]
        public void Open_Sql_Server_Tool_small_View()
        {
            #region Variable Declarations
            WpfCustom sqlServerDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase;
            #endregion
            Mouse.DoubleClick(sqlServerDatabase, new Point(253, 18));
        }
        [When(@"I Open Switch Tool Large View")]
        public void Open_Switch_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom switch1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch;
            WpfWindow switchCaseDialog = SwitchCaseDialog;
            #endregion
            Mouse.DoubleClick(switch1, new Point(39, 35));
            Assert.AreEqual(Open_Switch_Tool_Large_ViewParams.SwitchCaseDialogEnabled, switchCaseDialog.Enabled, "Switch dialog does not exist after opening switch large view");
        }
        [When(@"I Open System Information Tool Large View")]
        public void Open_System_Information_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom gatherSystemInfo = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo;
            WpfTable smallDataGridTable = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.SmallDataGridTable;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton;
            WpfGroup onErrorGroup = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.LargeViewContentCustom.OnErrorCustom.OnErrorGroup;
            #endregion
            Mouse.DoubleClick(gatherSystemInfo, new Point(145, 5));
            Assert.AreEqual(Open_System_Information_Tool_Large_ViewParams.SmallDataGridTableExists, smallDataGridTable.Exists, "Variable Grid does not exist after opening Gather System information tool large v" +
                    "iew");
            Assert.AreEqual(Open_System_Information_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "Done button  does not exist after opening Gather System information tool large vi" +
                    "ew");
            Assert.AreEqual(Open_System_Information_Tool_Large_ViewParams.OnErrorGroupExists, onErrorGroup.Exists, "OnError group  does not exist after opening Gather System information tool large " +
                    "view");
        }
        [When(@"I Open System Information Tool Qvi Large View")]
        public void Open_System_Information_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.OpenQuickVariableInpToggleButton;
            WpfCustom gatherSystemInfo = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_System_Information_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_System_Information_Tool_Qvi_Large_ViewParams.GatherSystemInfoExists, gatherSystemInfo.Exists, "System Info QVI window does not exist on the design surface");
        }
        [When(@"I Open UniqueRecords Large View")]
        public void Open_UniqueRecords_Large_View()
        {
            #region Variable Declarations
            WpfCustom unique = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique;
            #endregion
            Mouse.DoubleClick(unique, new Point(134, 10));
        }
        [When(@"I Open Unzip Tool Large View")]
        public void Open_Unzip_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom unZip = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox;
            WpfComboBox unZipNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.DoneButton;
            #endregion
            Mouse.DoubleClick(unZip, new Point(102, 14));
            Assert.AreEqual(Open_Unzip_Tool_Large_ViewParams.UnZipExists, unZip.Exists, "Unzip large view on the design surface does not exist");
            Assert.AreEqual(Open_Unzip_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.AreEqual(Open_Unzip_Tool_Large_ViewParams.UnZipNameComboBoxExists, unZipNameComboBox.Exists, "UnZipNameComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Unzip_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the design surface");
        }
        [When(@"I Open WebRequest LargeView")]
        public void Open_WebRequest_LargeView()
        {
            #region Variable Declarations
            WpfCustom webRequest = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest;
            WpfCustom largeView = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.LargeView;
            #endregion
            Mouse.DoubleClick(webRequest, new Point(126, 13));
            Assert.AreEqual(Open_WebRequest_LargeViewParams.LargeViewExists, largeView.Exists, "Web request large view does not exist on design surface.");
        }
        [When(@"I Open Write File Tool Large View")]
        public void Open_Write_File_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom fileWrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OnErrorCustom;
            WpfComboBox contentsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox;
            WpfRadioButton overwriteRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton;
            WpfButton doneButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.DoneButton;
            #endregion
            Mouse.DoubleClick(fileWrite, new Point(149, 13));
            Assert.AreEqual(Open_Write_File_Tool_Large_ViewParams.FileWriteExists, fileWrite.Exists, "Write file large view on the design surface does not exist");
            Assert.AreEqual(Open_Write_File_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.AreEqual(Open_Write_File_Tool_Large_ViewParams.ContentsComboBoxExists, contentsComboBox.Exists, "ContentsComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Write_File_Tool_Large_ViewParams.OverwriteRadioButtonExists, overwriteRadioButton.Exists, "OverwriteRadioButton does not exist on the design surface");
            Assert.AreEqual(Open_Write_File_Tool_Large_ViewParams.DoneButtonExists, doneButton.Exists, "DoneButton does not exist on the design surface");
        }
        [When(@"I Open Xpath Tool Large View")]
        public void Open_Xpath_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom xPath = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath;
            #endregion
            Mouse.DoubleClick(xPath, new Point(113, 12));
            Assert.AreEqual(Open_Xpath_Tool_Large_ViewParams.XPathExists, xPath.Exists, "Xpath large view does not exist on the design surface");
        }
        [When(@"I Open Xpath Tool Qvi Large View")]
        public void Open_Xpath_Tool_Qvi_Large_View()
        {
            #region Variable Declarations
            WpfToggleButton openQuickVariableInpToggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.OpenQuickVariableInpToggleButton;
            WpfCustom xPath = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath;
            WpfCustom quickVariableInputContent = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.QuickVariableInputContent;
            #endregion
            openQuickVariableInpToggleButton.Pressed = Open_Xpath_Tool_Qvi_Large_ViewParams.OpenQuickVariableInpToggleButtonPressed;
            Assert.AreEqual(Open_Xpath_Tool_Qvi_Large_ViewParams.XPathExists, xPath.Exists, "Xpath Qvi does not exist on the design surface");
            Assert.AreEqual(Open_Xpath_Tool_Qvi_Large_ViewParams.QuickVariableInputContentExists, quickVariableInputContent.Exists, "QVI on XPath is not open");
        }
        [When(@"I Open Zip Tool Large View")]
        public void Open_Zip_Tool_Large_View()
        {
            #region Variable Declarations
            WpfCustom zip = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip;
            WpfComboBox selectedCompressComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox;
            WpfCustom onErrorCustom = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OnErrorCustom;
            WpfCheckBox overwriteCheckBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox;
            WpfComboBox fileOrFolderComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox;
            #endregion
            Mouse.DoubleClick(zip, new Point(124, 12));
            Assert.AreEqual(Open_Zip_Tool_Large_ViewParams.ZipExists, zip.Exists, "Zip large view on the design surface does not exist");
            Assert.AreEqual(Open_Zip_Tool_Large_ViewParams.SelectedCompressComboBoxExists, selectedCompressComboBox.Exists, "SelectedCompressComboBox does not exist on the design surface");
            Assert.AreEqual(Open_Zip_Tool_Large_ViewParams.OnErrorCustomExists, onErrorCustom.Exists, "OnErrorCustom does not exist on the design surface");
            Assert.AreEqual(Open_Zip_Tool_Large_ViewParams.OverwriteCheckBoxExists, overwriteCheckBox.Exists, "OverwriteCheckBox does not exist on the design surface");
            Assert.AreEqual(Open_Zip_Tool_Large_ViewParams.FileOrFolderComboBoxExists, fileOrFolderComboBox.Exists, "FileOrFolderComboBox does not exist on the design surface");
        }
        [When(@"I Press F6")]
        public void Press_F6()
        {
            #region Variable Declarations
            WpfWindow mainStudioWindow = MainStudioWindow;
            #endregion
            Keyboard.SendKeys(mainStudioWindow, Press_F6Params.MainStudioWindowSendKeys, ModifierKeys.None);
        }
        [When(@"I PressF11 EnterFullScreen")]
        public void PressF11_EnterFullScreen()
        {
            #region Variable Declarations
            WpfWindow mainStudioWindow = MainStudioWindow;
            #endregion
            Keyboard.SendKeys(mainStudioWindow, PressF11_EnterFullScreenParams.MainStudioWindowSendKeys, ModifierKeys.None);
        }
        [When(@"I RabbitMqAsserts")]
        public void RabbitMqAsserts()
        {
            #region Variable Declarations
            WpfEdit virtualHostTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.VirtualHostTextBoxEdit;
            WpfEdit passwordTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PasswordTextBoxEdit;
            WpfEdit userNameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.UserNameTextBoxEdit;
            WpfEdit hostTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.HostTextBoxEdit;
            WpfEdit portTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PortTextBoxEdit;
            #endregion
            Assert.AreEqual(RabbitMqAssertsExpectedValues.VirtualHostTextBoxEditExists, virtualHostTextBoxEdit.Exists, "VirtualHoast textbox does not exist after opening RabbitMq Source tab");
            Assert.AreEqual(RabbitMqAssertsExpectedValues.PasswordTextBoxEditExists, passwordTextBoxEdit.Exists, "Password textbox does not exist after opening RabbitMq Source");
            Assert.AreEqual(RabbitMqAssertsExpectedValues.UserNameTextBoxEditExists, userNameTextBoxEdit.Exists, "Username textbox does not exist after opening RabbitMq Source");
            Assert.AreEqual(RabbitMqAssertsExpectedValues.HostTextBoxEditExists, hostTextBoxEdit.Exists, "Host textbox does not exist after opening RabbitMq Source");
            Assert.AreEqual(RabbitMqAssertsExpectedValues.PortTextBoxEditExists, portTextBoxEdit.Exists, "Port textbox does not exist after opening RabbitMq Source");
        }
        [When(@"I Remove WorkflowName From Save Dialog")]
        public void Remove_WorkflowName_From_Save_Dialog()
        {
            #region Variable Declarations
            WpfEdit serviceNameTextBox = SaveDialogWindow.ServiceNameTextBox;
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            WpfButton saveButton = SaveDialogWindow.SaveButton;
            #endregion
            serviceNameTextBox.Text = Remove_WorkflowName_From_Save_DialogParams.ServiceNameTextBoxText;
            Assert.AreEqual(Remove_WorkflowName_From_Save_DialogParams.ErrorLabelDisplayText, errorLabel.DisplayText, "Name cannot be null validation message does not appear");
            Assert.AreEqual(Remove_WorkflowName_From_Save_DialogParams.SaveButtonEnabled, saveButton.Enabled, "Save button on the Save dialog is enabled");
        }
        [When(@"I Rename FolderItem ToNewFolderItem")]
        public void Rename_FolderItem_ToNewFolderItem()
        {
            #region Variable Declarations
            WpfTreeItem firstSubItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem;
            WpfMenuItem rename = MainStudioWindow.ExplorerContextMenu.Rename;
            WpfEdit itemEdit = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit;
            #endregion
            Mouse.Click(firstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(rename, new Point(27, 18));
            itemEdit.Text = Rename_FolderItem_ToNewFolderItemParams.ItemEditText;
            Keyboard.SendKeys(itemEdit, Rename_FolderItem_ToNewFolderItemParams.ItemEditSendKeys, ModifierKeys.None);
        }
        [When(@"I Rename LocalFolder To SecondFolder")]
        public void Rename_LocalFolder_To_SecondFolder()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem rename = MainStudioWindow.ExplorerContextMenu.Rename;
            WpfEdit itemEdit = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(rename, new Point(27, 18));
            itemEdit.Text = Rename_LocalFolder_To_SecondFolderParams.ItemEditText;
            Keyboard.SendKeys(itemEdit, Rename_LocalFolder_To_SecondFolderParams.ItemEditSendKeys, ModifierKeys.None);
        }
        [When(@"I Rename LocalWorkflow To SecodWorkFlow")]
        public void Rename_LocalWorkflow_To_SecodWorkFlow()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem rename = MainStudioWindow.ExplorerContextMenu.Rename;
            WpfEdit itemEdit = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(rename, new Point(73, 15));
            itemEdit.Text = Rename_LocalWorkflow_To_SecodWorkFlowParams.ItemEditText;
            Keyboard.SendKeys(itemEdit, Rename_LocalWorkflow_To_SecodWorkFlowParams.ItemEditSendKeys, ModifierKeys.None);
        }
        [When(@"I Restore Unpinned Tab Using Context Menu")]
        public void Restore_Unpinned_Tab_Using_Context_Menu()
        {
            #region Variable Declarations
            WpfCustom unpinnedTab = MainStudioWindow.UnpinnedTab;
            WpfMenuItem tabbedDocument = MainStudioWindow.UnpinnedTabContextMenu.TabbedDocument;
            #endregion
            Mouse.Click(unpinnedTab, MouseButtons.Right, ModifierKeys.None, new Point(14, 12));
            tabbedDocument.Checked = Restore_Unpinned_Tab_Using_Context_MenuExpectedValues.TabbedDocumentChecked;
        }
        [When(@"I Right Click Help Tab")]
        public void Right_Click_Help_Tab()
        {
            #region Variable Declarations
            WpfTabPage helpTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.HelpTab;
            #endregion
            Mouse.Click(helpTab, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
        }
        [When(@"I RightClick BaseConvert OnDesignSurface")]
        public void RightClick_BaseConvert_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom baseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert;
            #endregion
            Mouse.Click(baseConvert, MouseButtons.Right, ModifierKeys.None, new Point(148, 12));
        }
        [When(@"I RightClick Calculate OnDesignSurface")]
        public void RightClick_Calculate_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom calculate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate;
            #endregion
            Mouse.Click(calculate, MouseButtons.Right, ModifierKeys.None, new Point(144, 10));
        }
        [When(@"I RightClick CaseConvert OnDesignSurface")]
        public void RightClick_CaseConvert_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom caseConvert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert;
            #endregion
            Mouse.Click(caseConvert, MouseButtons.Right, ModifierKeys.None, new Point(156, 10));
        }
        [When(@"I RightClick Comment OnDesignSurface")]
        public void RightClick_Comment_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom comment = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion
            Mouse.Click(comment, MouseButtons.Right, ModifierKeys.None, new Point(121, 10));
        }
        [When(@"I RightClick Copy OnDesignSurface")]
        public void RightClick_Copy_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom pathCopy = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy;
            #endregion
            Mouse.Click(pathCopy, MouseButtons.Right, ModifierKeys.None, new Point(104, 10));
        }
        [When(@"I RightClick CountRecords OnDesignSurface")]
        public void RightClick_CountRecords_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom countRecordset = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset;
            #endregion
            Mouse.Click(countRecordset, MouseButtons.Right, ModifierKeys.None, new Point(131, 10));
        }
        [When(@"I RightClick CreateJSON OnDesignSurface")]
        public void RightClick_CreateJSON_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom createJson = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson;
            #endregion
            Mouse.Click(createJson, MouseButtons.Right, ModifierKeys.None, new Point(128, 9));
        }
        [When(@"I RightClick CreateTool OnDesignSurface")]
        public void RightClick_CreateTool_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom pathCreate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate;
            #endregion
            Mouse.Click(pathCreate, MouseButtons.Right, ModifierKeys.None, new Point(108, 14));
        }
        [When(@"I RightClick DataMerge OnDesignSurface")]
        public void RightClick_DataMerge_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dataMerge = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge;
            #endregion
            Mouse.Click(dataMerge, MouseButtons.Right, ModifierKeys.None, new Point(140, 7));
        }
        [When(@"I RightClick DataSplit OnDesignSurface")]
        public void RightClick_DataSplit_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dataSplit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit;
            #endregion
            Mouse.Click(dataSplit, MouseButtons.Right, ModifierKeys.None, new Point(153, 6));
        }
        [When(@"I RightClick DateTime OnDesignSurface")]
        public void RightClick_DateTime_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dateTime = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime;
            #endregion
            Mouse.Click(dateTime, MouseButtons.Right, ModifierKeys.None, new Point(145, 13));
        }
        [When(@"I RightClick DateTimeDifference OnDesignSurface")]
        public void RightClick_DateTimeDifference_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dateTimeDifference = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference;
            #endregion
            Mouse.Click(dateTimeDifference, MouseButtons.Right, ModifierKeys.None, new Point(174, 10));
        }
        [When(@"I RightClick Decision OnDesignSurface")]
        public void RightClick_Decision_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom decision = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision;
            #endregion
            Mouse.Click(decision, MouseButtons.Right, ModifierKeys.None, new Point(28, 22));
        }
        [When(@"I RightClick Delete OnDesignSurface")]
        public void RightClick_Delete_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom pathDelete = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete;
            #endregion
            Mouse.Click(pathDelete, MouseButtons.Right, ModifierKeys.None, new Point(100, 10));
        }
        [When(@"I RightClick DeleteRecord OnDesignSurface")]
        public void RightClick_DeleteRecord_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom deleteRecord = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord;
            #endregion
            Mouse.Click(deleteRecord, MouseButtons.Right, ModifierKeys.None, new Point(116, 9));
        }
        [When(@"I RightClick DotNetDllConnector OnDesignSurface")]
        public void RightClick_DotNetDllConnector_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dotNetDll = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll;
            #endregion
            Mouse.Click(dotNetDll, MouseButtons.Right, ModifierKeys.None, new Point(164, 10));
        }
        [When(@"I RightClick DropboxFileOperation OnDesignSurface")]
        public void RightClick_DropboxFileOperation_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom dropboxDownload = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload;
            #endregion
            Mouse.Click(dropboxDownload, MouseButtons.Right, ModifierKeys.None, new Point(181, 11));
        }
        [When(@"I RightClick Email OnDesignSurface")]
        public void RightClick_Email_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sMTPEmail = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail;
            #endregion
            Mouse.Click(sMTPEmail, MouseButtons.Right, ModifierKeys.None, new Point(129, 11));
        }
        [When(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        public void RightClick_ExecuteCommandLine_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom executeCommandLine = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine;
            #endregion
            Mouse.Click(executeCommandLine, MouseButtons.Right, ModifierKeys.None, new Point(165, 13));
        }
        [When(@"I RightClick Explorer First Remote Server First Item")]
        public void RightClick_Explorer_First_Remote_Server_First_Item()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
        }
        [When(@"I RightClick Explorer Localhost First Item")]
        public void RightClick_Explorer_Localhost_First_Item()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfMenuItem open = MainStudioWindow.ExplorerContextMenu.Open;
            WpfMenuItem showDependencies = MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfMenuItem delete = MainStudioWindow.ExplorerContextMenu.Delete;
            #endregion
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Assert.AreEqual(RightClick_Explorer_Localhost_First_ItemParams.OpenExists, open.Exists, "Open does not exist in explorer context menu.");
            Assert.AreEqual(RightClick_Explorer_Localhost_First_ItemParams.ShowDependenciesExists, showDependencies.Exists, "ShowDependencies does not exist in explorer context menu.");
            Assert.AreEqual(RightClick_Explorer_Localhost_First_ItemParams.DeleteExists, delete.Exists, "Delete does not exist in ExplorerContextMenu");
        }
        [When(@"I RightClick FindIndex OnDesignSurface")]
        public void RightClick_FindIndex_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom findIndex = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex;
            #endregion
            Mouse.Click(findIndex, MouseButtons.Right, ModifierKeys.None, new Point(113, 8));
        }
        [When(@"I RightClick FindRecordIndex OnDesignSurface")]
        public void RightClick_FindRecordIndex_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom findRecordsIndex = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex;
            #endregion
            Mouse.Click(findRecordsIndex, MouseButtons.Right, ModifierKeys.None, new Point(191, 11));
        }
        [When(@"I RightClick ForEach OnDesignSurface")]
        public void RightClick_ForEach_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom forEach = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach;
            #endregion
            Mouse.Click(forEach, MouseButtons.Right, ModifierKeys.None, new Point(137, 9));
        }
        [When(@"I RightClick FormatNumber OnDesignSurface")]
        public void RightClick_FormatNumber_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom formatNumber = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber;
            #endregion
            Mouse.Click(formatNumber, MouseButtons.Right, ModifierKeys.None, new Point(143, 9));
        }
        [When(@"I RightClick Length OnDesignSurface")]
        public void RightClick_Length_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom length = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length;
            #endregion
            Mouse.Click(length, MouseButtons.Right, ModifierKeys.None, new Point(97, 10));
        }
        [When(@"I RightClick Move OnDesignSurface")]
        public void RightClick_Move_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom pathMove = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove;
            #endregion
            Mouse.Click(pathMove, MouseButtons.Right, ModifierKeys.None, new Point(98, 11));
        }
        [When(@"I RightClick MySQLConnector OnDesignSurface")]
        public void RightClick_MySQLConnector_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom mySqlDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase;
            #endregion
            Mouse.Click(mySqlDatabase, MouseButtons.Right, ModifierKeys.None, new Point(202, 10));
        }
        [When(@"I RightClick New Workflow Tab")]
        public void RightClick_New_Workflow_Tab()
        {
            #region Variable Declarations
            WpfTabPage workflowTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab;
            #endregion
            Mouse.Click(workflowTab, MouseButtons.Right, ModifierKeys.None, new Point(63, 18));
        }
        [When(@"I RightClick Random OnDesignSurface")]
        public void RightClick_Random_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom random = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random;
            #endregion
            Mouse.Click(random, MouseButtons.Right, ModifierKeys.None, new Point(107, 13));
        }
        [When(@"I RightClick ReadFile OnDesignSurface")]
        public void RightClick_ReadFile_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom fileRead = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead;
            #endregion
            Mouse.Click(fileRead, MouseButtons.Right, ModifierKeys.None, new Point(99, 14));
        }
        [When(@"I RightClick ReadFolder OnDesignSurface")]
        public void RightClick_ReadFolder_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom folderRead = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead;
            #endregion
            Mouse.Click(folderRead, MouseButtons.Right, ModifierKeys.None, new Point(115, 12));
        }
        [When(@"I RightClick Rename OnDesignSurface")]
        public void RightClick_Rename_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom pathRename = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename;
            #endregion
            Mouse.Click(pathRename, MouseButtons.Right, ModifierKeys.None, new Point(103, 7));
        }
        [When(@"I RightClick Replace OnDesignSurface")]
        public void RightClick_Replace_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom replace = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace;
            #endregion
            Mouse.Click(replace, MouseButtons.Right, ModifierKeys.None, new Point(100, 7));
        }
        [When(@"I RightClick Sequence OnDesignSurface")]
        public void RightClick_Sequence_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sequence = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence;
            #endregion
            Mouse.Click(sequence, MouseButtons.Right, ModifierKeys.None, new Point(119, 8));
        }
        [When(@"I RightClick SharepointCreateListItem OnDesignSurface")]
        public void RightClick_SharepointCreateListItem_OnDesignSurface()
        {
            #region Variable Declarations
            WpfMenuItem copy = MainStudioWindow.DesignSurfaceContextMenu.Copy;
            #endregion
            Mouse.Click(copy, MouseButtons.Right, ModifierKeys.None, new Point(199, 12));
        }
        [When(@"I RightClick SharepointDelete OnDesignSurface")]
        public void RightClick_SharepointDelete_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sharepointDeleteFile = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile;
            #endregion
            Mouse.Click(sharepointDeleteFile, MouseButtons.Right, ModifierKeys.None, new Point(217, 8));
        }
        [When(@"I RightClick SharepointRead OnDesignSurface")]
        public void RightClick_SharepointRead_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sharepointReadListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem;
            #endregion
            Mouse.Click(sharepointReadListItem, MouseButtons.Right, ModifierKeys.None, new Point(203, 9));
        }
        [When(@"I RightClick SharepointUpdate OnDesignSurface")]
        public void RightClick_SharepointUpdate_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sharepointUpdate = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate;
            #endregion
            Mouse.Click(sharepointUpdate, MouseButtons.Right, ModifierKeys.None, new Point(210, 5));
        }
        [When(@"I RightClick SortRecords OnDesignSurface")]
        public void RightClick_SortRecords_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sortRecords = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords;
            #endregion
            Mouse.Click(sortRecords, MouseButtons.Right, ModifierKeys.None, new Point(118, 8));
        }
        [When(@"I RightClick SQLConnector OnDesignSurface")]
        public void RightClick_SQLConnector_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sqlBulkInsert = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert;
            #endregion
            Mouse.Click(sqlBulkInsert, MouseButtons.Right, ModifierKeys.None, new Point(143, 6));
        }
        [When(@"I RightClick SqlServerConnector OnDesignSurface")]
        public void RightClick_SqlServerConnector_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom sqlServerDatabase = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase;
            #endregion
            Mouse.Click(sqlServerDatabase, MouseButtons.Right, ModifierKeys.None, new Point(198, 8));
        }
        [When(@"I RightClick Switch OnDesignSurface")]
        public void RightClick_Switch_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom switch1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch;
            #endregion
            Mouse.Click(switch1, MouseButtons.Right, ModifierKeys.None, new Point(46, 15));
        }
        [When(@"I RightClick Unzip OnDesignSurface")]
        public void RightClick_Unzip_OnDesignSurface()
        {
            #region Variable Declarations
            WpfComboBox actionsCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox;
            #endregion
            Mouse.Click(actionsCombobox, MouseButtons.Right, ModifierKeys.None, new Point(101, 10));
        }
        [When(@"I RightClick WebRequest OnDesignSurface")]
        public void RightClick_WebRequest_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom webRequest = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest;
            #endregion
            Mouse.Click(webRequest, MouseButtons.Right, ModifierKeys.None, new Point(165, 8));
        }
        [When(@"I RightClick WriteFile OnDesignSurface")]
        public void RightClick_WriteFile_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom fileWrite = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite;
            #endregion
            Mouse.Click(fileWrite, MouseButtons.Right, ModifierKeys.None, new Point(96, 12));
        }
        [When(@"I RightClick XPath OnDesignSurface")]
        public void RightClick_XPath_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom xPath = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath;
            #endregion
            Mouse.Click(xPath, MouseButtons.Right, ModifierKeys.None, new Point(99, 8));
        }
        [When(@"I RightClick Zip OnDesignSurface")]
        public void RightClick_Zip_OnDesignSurface()
        {
            #region Variable Declarations
            WpfCustom zip = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip;
            #endregion
            Mouse.Click(zip, MouseButtons.Right, ModifierKeys.None, new Point(95, 12));
        }
        [When(@"I Search And Select DiceRoll")]
        public void Search_And_Select_DiceRoll()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox;
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            #endregion
            Mouse.Click(searchTextBox, new Point(165, 9));
            searchTextBox.Text = Search_And_Select_DiceRollParams.SearchTextBoxText;
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }
        [When(@"I Search And Select HelloWolrd")]
        public void Search_And_Select_HelloWolrd()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox;
            WpfTreeItem firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            #endregion
            Mouse.Click(searchTextBox, new Point(165, 9));
            searchTextBox.Text = Search_And_Select_HelloWolrdParams.SearchTextBoxText;
            Mouse.Click(firstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }
        [When(@"I Select AcceptanceTestin create")]
        public void Select_AcceptanceTestin_create()
        {
            #region Variable Declarations
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList;
            WpfListItem uIAcceptanceTesting_CrListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem;
            #endregion
            Mouse.Click(methodList, new Point(119, 7));
            Mouse.Click(uIAcceptanceTesting_CrListItem, new Point(114, 13));
        }
        [When(@"I Select Action")]
        public void Select_Action()
        {
            #region Variable Declarations
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox;
            WpfListItem item1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1;
            #endregion
            Mouse.Click(actionsComboBox, new Point(216, 7));
            Mouse.Click(item1, new Point(137, 7));
        }
        [When(@"I Select Action From PostgreTool")]
        public void Select_Action_From_PostgreTool()
        {
            #region Variable Declarations
            WpfMenuItem newDatabaseSource = MainStudioWindow.ExplorerContextMenu.NewDatabaseSource;
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeViewContentCustom.ActionsComboBox;
            WpfTable largeDataGridTable = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeViewContentCustom.LargeDataGridTable;
            #endregion
            Mouse.Click(newDatabaseSource, new Point(119, 7));
            Mouse.Click(actionsComboBox, new Point(114, 13));
            Assert.AreEqual(Select_Action_From_PostgreToolParams.LargeDataGridTableEnabled, largeDataGridTable.Enabled, "Inputs grid is not enabled after selecting an Action.");
        }
        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList()
        {
            #region Variable Declarations
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList;
            WpfListItem uIAppdataListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAppdataListItem;
            #endregion
            Mouse.Click(methodList, new Point(174, 7));
            Mouse.Click(uIAppdataListItem, new Point(43, 15));
        }
        [When(@"I Select AppData From MethodList From ReadTool")]
        public void Select_AppData_From_MethodList_From_ReadTool()
        {
            #region Variable Declarations
            WpfComboBox methodList = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList;
            WpfListItem uIAppdataListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList.UIAppdataListItem;
            #endregion
            Mouse.Click(methodList, new Point(174, 7));
            Mouse.Click(uIAppdataListItem, new Point(43, 15));
        }
        [When(@"I Select Copy FromContextMenu")]
        public void Select_Copy_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem copy = MainStudioWindow.DesignSurfaceContextMenu.Copy;
            #endregion
            Mouse.Click(copy, new Point(27, 18));
        }
        [When(@"I Select CopyAsImage FromContextMenu")]
        public void Select_CopyAsImage_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem copyasImage = MainStudioWindow.DesignSurfaceContextMenu.CopyasImage;
            #endregion
            Mouse.Click(copyasImage, new Point(62, 22));
        }
        [When(@"I Select Cut FromContextMenu")]
        public void Select_Cut_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem cut = MainStudioWindow.DesignSurfaceContextMenu.Cut;
            #endregion
            Mouse.Click(cut, new Point(53, 16));
        }
        [When(@"I Select DatabaseAndTable From BulkInsert Tool")]
        public void Select_DatabaseAndTable_From_BulkInsert_Tool()
        {
            #region Variable Declarations
            WpfComboBox databaseComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox;
            WpfListItem testingDb = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox.TestingDb;
            WpfComboBox tableNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.TableNameComboBox;
            #endregion
            Mouse.Click(databaseComboBox, new Point(174, 7));
            Mouse.Click(testingDb, new Point(43, 15));
            Assert.AreEqual(Select_DatabaseAndTable_From_BulkInsert_ToolParams.TableNameComboBoxEnabled, tableNameComboBox.Enabled, "Table combobox is not Enabled after selecting the database");
        }
        [When(@"I Select Delete FromExplorerContextMenu")]
        public void Select_Delete_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem delete = MainStudioWindow.ExplorerContextMenu.Delete;
            WpfButton yesButton = MessageBoxWindow.YesButton;
            #endregion
            Mouse.Click(delete, new Point(87, 12));
            Assert.AreEqual(Select_Delete_FromExplorerContextMenuParams.YesButtonExists, yesButton.Exists, "Message box Yes button does not exist");
        }
        [When(@"I Select DeleteRow FromContextMenu")]
        public void Select_DeleteRow_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem deleteRow = MainStudioWindow.DesignSurfaceContextMenu.DeleteRow;
            #endregion
            Mouse.Click(deleteRow, new Point(74, 9));
        }
        [When(@"I Select Deploy FromExplorerContextMenu")]
        public void Select_Deploy_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem deploy = MainStudioWindow.ExplorerContextMenu.Deploy;
            WpfTabPage deployTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab;
            #endregion
            Mouse.Click(deploy, new Point(57, 11));
            Assert.AreEqual(Select_Deploy_FromExplorerContextMenuParams.DeployTabExists, deployTab.Exists, "DeployTab does not exist after clicking Deploy");
        }
        [When(@"I Select Dev2TestingDB From DB Source Wizard Database Combobox")]
        public void Select_Dev2TestingDB_From_DB_Source_Wizard_Database_Combobox()
        {
            #region Variable Declarations
            WpfCustom databaseCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox;
            WpfCustom comboboxListItemAsDev2TestingDB = MainStudioWindow.ComboboxListItemAsDev2TestingDB;
            WpfText uIDev2TestingDBText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.UIDatabaseComboxBoxCustom.UIDev2TestingDBText;
            #endregion
            Mouse.Click(databaseCombobox, new Point(221, 9));
            Mouse.Click(comboboxListItemAsDev2TestingDB, new Point(129, 19));
            Assert.AreEqual(Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams.UIDev2TestingDBTextDisplayText, uIDev2TestingDBText.DisplayText);
        }
        [When(@"I Select First Item From DotNet DLL Large View Source Combobox")]
        public void Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox()
        {
            #region Variable Declarations
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox;
            WpfListItem listItem1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.ListItem1;
            #endregion
            Mouse.Click(sourcesComboBox, new Point(175, 9));
            Mouse.Click(listItem1, new Point(163, 17));
        }
        [When(@"I Select FirstItem From DotNet DLL Large View Action Combobox")]
        public void Select_FirstItem_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            #region Variable Declarations
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox;
            WpfListItem item1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1;
            #endregion
            Mouse.Click(actionsComboBox, new Point(216, 7));
            Mouse.Click(item1, new Point(137, 7));
            Assert.AreEqual(Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues.ActionsComboBoxSelectedItem, actionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }
        [When(@"I Select GetCountries From SQL Server Large View Action Combobox")]
        public void Select_GetCountries_From_SQL_Server_Large_View_Action_Combobox()
        {
            #region Variable Declarations
            WpfComboBox actionsCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox;
            WpfListItem getCountriesListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox.GetCountriesListItem;
            #endregion
            Mouse.Click(actionsCombobox, new Point(216, 7));
            Mouse.Click(getCountriesListItem, new Point(137, 7));
            Assert.AreEqual(Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues.ActionsComboboxSelectedItem, actionsCombobox.SelectedItem, "GetCountries is not selected in SQL server large view action combobox.");
        }
        [When(@"I Select GUID From Random Type Combobox")]
        public void Select_GUID_From_Random_Type_Combobox()
        {
            #region Variable Declarations
            WpfComboBox typeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox;
            WpfListItem gUID = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox.GUID;
            #endregion
            Mouse.Click(typeComboBox, new Point(133, 10));
            Mouse.Click(gUID, new Point(31, 16));
        }
        [When(@"I Select http From Server Source Wizard Address Protocol Dropdown")]
        public void Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown()
        {
            #region Variable Declarations
            WpfButton toggleDropdown = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown;
            WpfCustom comboboxListItemAsHttp = MainStudioWindow.ComboboxListItemAsHttp;
            WpfText httpSelectedItemText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.HttpSelectedItemText;
            WpfEdit addressEditBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.AddressEditBox;
            #endregion
            Mouse.Click(toggleDropdown, new Point(54, 8));
            Assert.AreEqual(Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams.ComboboxListItemAsHttpExists, comboboxListItemAsHttp.Exists, "Http does not exist in server source wizard address protocol dropdown list.");
            Mouse.Click(comboboxListItemAsHttp, new Point(31, 12));
            Assert.AreEqual(Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams.HttpSelectedItemTextDisplayText, httpSelectedItemText.DisplayText, "Server source wizard address protocol is not equal to http.");
            Assert.AreEqual(Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams.AddressEditBoxExists, addressEditBox.Exists, "Server source wizard address textbox does not exist");
        }
        [When(@"I Select InsertRow FromContextMenu")]
        public void Select_InsertRow_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem insertRow = MainStudioWindow.DesignSurfaceContextMenu.InsertRow;
            #endregion
            Mouse.Click(insertRow, new Point(66, 19));
        }
        [When(@"I Select Letters From Random Type Combobox")]
        public void Select_Letters_From_Random_Type_Combobox()
        {
            #region Variable Declarations
            WpfComboBox typeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox;
            WpfListItem letters = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.TypeComboBox.Letters;
            WpfComboBox lengthComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random.SmallViewContentCustom.LengthComboBox;
            #endregion
            Mouse.Click(typeComboBox, new Point(133, 10));
            Mouse.Click(letters, new Point(31, 16));
            Assert.AreEqual(Select_Letters_From_Random_Type_ComboboxParams.LengthComboBoxExists, lengthComboBox.Exists, "Length combobox does not exist after selecting Letters as Random Type");
        }
        [When(@"I Select LocalhostConnected From Deploy Tab Destination Server Combobox")]
        public void Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_Combobox()
        {
            #region Variable Declarations
            WpfButton toggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.ToggleButton;
            WpfCustom comboboxListItemAsNewRemoteServer = MainStudioWindow.ComboboxListItemAsNewRemoteServer;
            WpfCustom comboboxListItemAsLocalhostConnected = MainStudioWindow.ComboboxListItemAsLocalhostConnected;
            WpfText remoteConnectionIntegrationText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText;
            #endregion
            Mouse.Click(toggleButton, new Point(230, 9));
            Assert.AreEqual(Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams.ComboboxListItemAsNewRemoteServerExists, comboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.AreEqual(Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams.ComboboxListItemAsLocalhostConnectedExists, comboboxListItemAsLocalhostConnected.Exists, "Remote Connection Integration option does not exist in Destination server combobo" +
                    "x.");
            Mouse.Click(comboboxListItemAsLocalhostConnected, new Point(226, 13));
            Assert.AreEqual(Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams.RemoteConnectionIntegrationTextDisplayText, remoteConnectionIntegrationText.DisplayText, "Selected source server in deploy is not Remote Connection Integration.");
        }
        [When(@"I Select LoggingTab")]
        public void Select_LoggingTab()
        {
            #region Variable Declarations
            WpfTabPage loggingTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab;
            #endregion
            Mouse.Click(loggingTab, new Point(57, 7));
        }
        [When(@"I Select Months From AddTime Type")]
        public void Select_Months_From_AddTime_Type()
        {
            #region Variable Declarations
            WpfComboBox addTimeTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox;
            WpfListItem months = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox.Months;
            #endregion
            Mouse.Click(addTimeTypeComboBox, new Point(175, 9));
            Mouse.Click(months, new Point(163, 17));
        }
        [When(@"I Select MSSQLSERVER From DB Source Wizard Address Protocol Dropdown")]
        public void Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_Dropdown()
        {
            #region Variable Declarations
            WpfButton toggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.ToggleButton;
            WpfText microsoftSQLServerText = MainStudioWindow.ComboboxListItemAsMicrosoftSQLServer.MicrosoftSQLServerText;
            WpfText microsoftSQLServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.MicrosoftSQLServer;
            #endregion
            Mouse.Click(toggleButton, new Point(625, 11));
            Assert.AreEqual(Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams.MicrosoftSQLServerTextExists, microsoftSQLServerText.Exists, "Microsoft SQL Server does not exist as an option in new DB source wizard type com" +
                    "bobox.");
            Mouse.Click(microsoftSQLServerText, new Point(118, 6));
            Assert.AreEqual(Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams.MicrosoftSQLServerDisplayText, microsoftSQLServer.DisplayText, "Microsoft SQL Server is not selected in DB source wizard.");
        }
        [When(@"I Select Namespace")]
        public void Select_Namespace()
        {
            #region Variable Declarations
            WpfComboBox classNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox;
            WpfListItem comboboxlistItemAsSystemObject = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject;
            #endregion
            Mouse.Click(classNameComboBox, new Point(216, 7));
            Assert.AreEqual(Select_NamespaceExpectedValues.ComboboxlistItemAsSystemObjectExists, comboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(comboboxlistItemAsSystemObject, new Point(137, 7));
        }
        [When(@"I Select NewDatabaseSource FromExplorerContextMenu")]
        public void Select_NewDatabaseSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newDatabaseSource = MainStudioWindow.ExplorerContextMenu.NewDatabaseSource;
            #endregion
            Mouse.Click(newDatabaseSource, new Point(72, 14));
        }
        [When(@"I Select NewDatabaseSource FromSqlServerTool")]
        public void Select_NewDatabaseSource_FromSqlServerTool()
        {
            #region Variable Declarations
            WpfButton newDbSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton;
            WpfText microsoftSQLServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox.MicrosoftSQLServer;
            WpfEdit userNameTextBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox;
            WpfEdit passwordTextBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox;
            #endregion
            Mouse.Click(newDbSourceButton, new Point(16, 13));
            Assert.AreEqual(Select_NewDatabaseSource_FromSqlServerToolParams.MicrosoftSQLServerDisplayText, microsoftSQLServer.DisplayText, "Microsoft SQL Server is not selected in DB source wizard.");
            Assert.AreEqual(Select_NewDatabaseSource_FromSqlServerToolParams.UserNameTextBoxExists, userNameTextBox.Exists, "User name testbox does not exist on db source wizard.");
            Assert.AreEqual(Select_NewDatabaseSource_FromSqlServerToolParams.PasswordTextBoxExists, passwordTextBox.Exists, "Password textbox does not exist on database source wizard.");
        }
        [When(@"I Select NewDropboxSource FromExplorerContextMenu")]
        public void Select_NewDropboxSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newDropboxSource = MainStudioWindow.ExplorerContextMenu.NewDropboxSource;
            #endregion
            Mouse.Click(newDropboxSource, new Point(119, 15));
        }
        [When(@"I Select NewEmailSource FromExplorerContextMenu")]
        public void Select_NewEmailSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            WpfMenu explorerEnvironmentContextMenu = MainStudioWindow.ExplorerEnvironmentContextMenu;
            WpfMenuItem newEmailSource = MainStudioWindow.ExplorerEnvironmentContextMenu.NewEmailSource;
            WpfEdit hostTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.HostTextBoxEdit;
            WpfEdit userNameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.UserNameTextBoxEdit;
            WpfEdit passwordTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PasswordTextBoxEdit;
            WpfEdit portTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.PortTextBoxEdit;
            WpfEdit timeoutTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTabPage.SendTestModelsCustom.TimeoutTextBoxEdit;
            #endregion
            Mouse.Click(localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.ExplorerEnvironmentContextMenuExists, explorerEnvironmentContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(newEmailSource, new Point(101, 13));
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.HostTextBoxEditExists, hostTextBoxEdit.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.UserNameTextBoxEditExists, userNameTextBoxEdit.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.PasswordTextBoxEditExists, passwordTextBoxEdit.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.PortTextBoxEditExists, portTextBoxEdit.Exists, "Port textbox does not exist after opening Email source tab");
            Assert.AreEqual(Select_NewEmailSource_FromExplorerContextMenuParams.TimeoutTextBoxEditExists, timeoutTextBoxEdit.Exists, "Timeout textbox does not exist after opening Email source tab");
        }
        [When(@"I Select NewPluginSource FromExplorerContextMenu")]
        public void Select_NewPluginSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            WpfListItem systemRandomListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SystemRandomListItem;
            #endregion
            Mouse.Click(localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(systemRandomListItem, new Point(78, 11));
        }
        [When(@"I Select NewServerSource FromExplorerContextMenu")]
        public void Select_NewServerSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newServerSource = MainStudioWindow.ExplorerContextMenu.NewServerSource;
            #endregion
            Mouse.Click(newServerSource, new Point(44, 13));
        }
        [When(@"I Select NewSharepointSource FromExplorerContextMenu")]
        public void Select_NewSharepointSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newSharepointSource = MainStudioWindow.ExplorerContextMenu.NewSharepointSource;
            #endregion
            Mouse.Click(newSharepointSource, new Point(126, 17));
        }
        [When(@"I Select NewSharepointSource FromServer Lookup")]
        public void Select_NewSharepointSource_FromServer_Lookup()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server;
            #endregion
            Mouse.Click(server, new Point(107, 13));
            Keyboard.SendKeys(server, Select_NewSharepointSource_FromServer_LookupParams.ServerSendKeys, ModifierKeys.None);
        }
        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointCopyFile Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_Tool()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.Server;
            #endregion
            Mouse.Click(server, new Point(107, 13));
            Keyboard.SendKeys(server, Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams.ServerSendKeys, ModifierKeys.None);
        }
        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointMoveFile Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_Tool()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.Server;
            #endregion
            Mouse.Click(server, new Point(107, 13));
            Keyboard.SendKeys(server, Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams.ServerSendKeys, ModifierKeys.None);
        }
        [When(@"I Select NewSharepointSource FromServer Lookup On SharepointUpload Tool")]
        public void Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_Tool()
        {
            #region Variable Declarations
            WpfComboBox sourceCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.SourceCombobox;
            #endregion
            Mouse.Click(sourceCombobox, new Point(107, 13));
            Keyboard.SendKeys(sourceCombobox, Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams.SourceComboboxSendKeys, ModifierKeys.None);
        }
        [When(@"I Select NewWebSource FromExplorerContextMenu")]
        public void Select_NewWebSource_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newWebServiceSource = MainStudioWindow.ExplorerContextMenu.NewWebServiceSource;
            #endregion
            Mouse.Click(newWebServiceSource, new Point(82, 20));
        }
        [When(@"I Select NewWorkflow FromExplorerContextMenu")]
        public void Select_NewWorkflow_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem newWorkflow = MainStudioWindow.ExplorerContextMenu.NewWorkflow;
            #endregion
            Mouse.Click(newWorkflow, new Point(30, 11));
        }
        [When(@"I Select NewWorkFlowService From ContextMenu")]
        public void Select_NewWorkFlowService_From_ContextMenu()
        {
            #region Variable Declarations
            WpfTreeItem localhost = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost;
            WpfMenuItem newWorkflow = MainStudioWindow.ExplorerEnvironmentContextMenu.NewWorkflow;
            #endregion
            Mouse.Click(localhost, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));
            Assert.AreEqual(Select_NewWorkFlowService_From_ContextMenuParams.NewWorkflowEnabled, newWorkflow.Enabled, "NewWorkFlowService button is disabled.");
            Mouse.Click(newWorkflow, new Point(79, 13));
        }
        [When(@"I Select Next From DotNet DLL Large View Action Combobox")]
        public void Select_Next_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            #region Variable Declarations
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox;
            WpfListItem nextListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.NextListItem;
            #endregion
            Mouse.Click(actionsComboBox, new Point(216, 7));
            Mouse.Click(nextListItem, new Point(137, 7));
            Assert.AreEqual(Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues.ActionsComboBoxSelectedItem, actionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }
        [When(@"I Select Open FromExplorerContextMenu")]
        public void Select_Open_FromExplorerContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem open = MainStudioWindow.ExplorerContextMenu.Open;
            #endregion
            Mouse.Click(open, new Point(30, 11));
        }
        [When(@"I Select OutputIn Days")]
        public void Select_OutputIn_Days()
        {
            #region Variable Declarations
            WpfComboBox outputInComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox;
            WpfListItem days = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox.Days;
            #endregion
            Mouse.Click(outputInComboBox, new Point(119, 7));
            Mouse.Click(days, new Point(114, 13));
        }
        [When(@"I Select Paste FromContextMenu")]
        public void Select_Paste_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem paste = MainStudioWindow.DesignSurfaceContextMenu.Paste;
            #endregion
            Mouse.Click(paste, new Point(52, 16));
        }
        [When(@"I Select PerfomanceCounterTab")]
        public void Select_PerfomanceCounterTab()
        {
            #region Variable Declarations
            WpfTabPage perfomanceCounterTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.PerfomanceCounterTab;
            #endregion
            Mouse.Click(perfomanceCounterTab, new Point(124, 14));
        }
        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Destination Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox()
        {
            #region Variable Declarations
            WpfButton toggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.ToggleButton;
            WpfCustom comboboxListItemAsNewRemoteServer = MainStudioWindow.ComboboxListItemAsNewRemoteServer;
            WpfCustom comboboxListItemAsRemoteConnectionIntegration = MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration;
            WpfText remoteConnectionIntegrationText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.RemoteConnectionIntegrationText;
            #endregion
            Mouse.Click(toggleButton, new Point(230, 9));
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams.ComboboxListItemAsNewRemoteServerExists, comboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams.ComboboxListItemAsRemoteConnectionIntegrationExists, comboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobo" +
                    "x.");
            Mouse.Click(comboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams.RemoteConnectionIntegrationTextDisplayText, remoteConnectionIntegrationText.DisplayText, "Selected destination server in deploy is not Remote Connection Integration.");
        }
        [When(@"I Select RemoteConnectionIntegration From Deploy Tab Source Server Combobox")]
        public void Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_Combobox()
        {
            #region Variable Declarations
            WpfButton toggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.ToggleButton;
            WpfCustom comboboxListItemAsNewRemoteServer = MainStudioWindow.ComboboxListItemAsNewRemoteServer;
            WpfCustom comboboxListItemAsRemoteConnectionIntegration = MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration;
            WpfText remoteConnectionIntegrationText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText;
            #endregion
            Mouse.Click(toggleButton, new Point(230, 9));
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams.ComboboxListItemAsNewRemoteServerExists, comboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams.ComboboxListItemAsRemoteConnectionIntegrationExists, comboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Destination server combobo" +
                    "x.");
            Mouse.Click(comboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
            Assert.AreEqual(Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams.RemoteConnectionIntegrationTextDisplayText, remoteConnectionIntegrationText.DisplayText, "Selected source server in deploy is not Remote Connection Integration.");
        }
        [When(@"I Select RemoteConnectionIntegration From Explorer")]
        public void Select_RemoteConnectionIntegration_From_Explorer()
        {
            #region Variable Declarations
            WpfButton serverListComboBox = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox;
            WpfCustom comboboxListItemAsRemoteConnectionIntegration = MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration;
            #endregion
            Mouse.Click(serverListComboBox, new Point(174, 8));
            Mouse.Click(comboboxListItemAsRemoteConnectionIntegration, new Point(226, 13));
        }
        [When(@"I Select RemoteConnectionIntegrationConnected From Deploy Tab Source Server Combobox")]
        public void Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_Combobox()
        {
            #region Variable Declarations
            WpfButton toggleButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.ToggleButton;
            WpfCustom comboboxListItemAsNewRemoteServer = MainStudioWindow.ComboboxListItemAsNewRemoteServer;
            WpfCustom comboboxListItemAsRemoteConnectionIntegrationConnected = MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected;
            WpfText remoteConnectionIntegrationText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.SourceServerConectControl.Combobox.RemoteConnectionIntegrationText;
            #endregion
            Mouse.Click(toggleButton, new Point(230, 9));
            Assert.AreEqual(Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams.ComboboxListItemAsNewRemoteServerExists, comboboxListItemAsNewRemoteServer.Exists, "New Remote Server... option does not exist in Destination server combobox.");
            Assert.AreEqual(Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams.ComboboxListItemAsRemoteConnectionIntegrationConnectedExists, comboboxListItemAsRemoteConnectionIntegrationConnected.Exists, "Remote Connection Integration option does not exist in Source server combobox.");
            Mouse.Click(comboboxListItemAsRemoteConnectionIntegrationConnected, new Point(226, 13));
            Assert.AreEqual(Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams.RemoteConnectionIntegrationTextDisplayText, remoteConnectionIntegrationText.DisplayText, "Selected source server in deploy is not Remote Connection Integration.");
        }
        [When(@"I Select Round Up")]
        public void Select_Round_Up()
        {
            #region Variable Declarations
            WpfComboBox roundingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox;
            WpfListItem roungUP = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.RoungUP;
            #endregion
            Mouse.Click(roundingComboBox, new Point(119, 7));
            Mouse.Click(roungUP, new Point(114, 13));
        }
        [When(@"I Select RoundingType None")]
        public void Select_RoundingType_None()
        {
            #region Variable Declarations
            WpfComboBox roundingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox;
            WpfListItem none = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.None;
            #endregion
            Mouse.Click(roundingComboBox, new Point(119, 7));
            Mouse.Click(none, new Point(114, 13));
        }
        [When(@"I Select RoundingType Normal")]
        public void Select_RoundingType_Normal()
        {
            #region Variable Declarations
            WpfComboBox roundingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox;
            WpfListItem normal = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.Normal;
            #endregion
            Mouse.Click(roundingComboBox, new Point(119, 7));
            Mouse.Click(normal, new Point(114, 13));
        }
        [When(@"I Select RSAKLFSVRGENDEV From Server Source Wizard Dropdownlist")]
        public void Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_Dropdownlist()
        {
            #region Variable Declarations
            WpfListItem rSAKLFSVRGENDEV = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV;
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox;
            #endregion
            Mouse.Click(rSAKLFSVRGENDEV, new Point(97, 17));
            Assert.AreEqual(Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues.TextboxText, textbox.Text, "RSAKLFSVRGENDEV is not selected as the server in the DB source wizard.");
        }
        [When(@"I Select SaveAsImage FromContextMenu")]
        public void Select_SaveAsImage_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem saveasImage = MainStudioWindow.DesignSurfaceContextMenu.SaveasImage;
            #endregion
            Mouse.Click(saveasImage, new Point(38, 15));
        }
        [When(@"I Select SecurityTab")]
        public void Select_SecurityTab()
        {
            #region Variable Declarations
            WpfTabPage securityTab = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab;
            #endregion
            Mouse.Click(securityTab, new Point(102, 10));
        }
        [When(@"I Select SetAsStartNode FromContextMenu")]
        public void Select_SetAsStartNode_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem setasStartNode = MainStudioWindow.DesignSurfaceContextMenu.SetasStartNode;
            #endregion
            Mouse.Click(setasStartNode, new Point(67, 16));
        }
        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server;
            WpfListItem sharepointTestServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.SharepointTestServer;
            #endregion
            Mouse.Click(server, new Point(98, 12));
            Mouse.Click(sharepointTestServer, new Point(67, 13));
        }
        [When(@"I Select SharepointTestServer From SharepointRead Tool")]
        public void Select_SharepointTestServer_From_SharepointRead_Tool()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server;
            WpfListItem sharepointTestServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server.SharepointTestServer;
            WpfButton editSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton;
            #endregion
            Mouse.Click(server, new Point(98, 12));
            Mouse.Click(sharepointTestServer, new Point(67, 13));
            Assert.AreEqual(Select_SharepointTestServer_From_SharepointRead_ToolParams.EditSourceButtonEnabled, editSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }
        [When(@"I Select SharepointTestServer From SharepointUpdate Tool")]
        public void Select_SharepointTestServer_From_SharepointUpdate_Tool()
        {
            #region Variable Declarations
            WpfComboBox server = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.Server;
            WpfListItem sharepointTestServer = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.Server.SharepointTestServer;
            WpfButton editSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.SmallView.EditSourceButton;
            #endregion
            Mouse.Click(server, new Point(98, 12));
            Mouse.Click(sharepointTestServer, new Point(67, 13));
            Assert.AreEqual(Select_SharepointTestServer_From_SharepointUpdate_ToolParams.EditSourceButtonEnabled, editSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }
        [When(@"I Select ShowLargeView FromContextMenu")]
        public void Select_ShowLargeView_FromContextMenu()
        {
            #region Variable Declarations
            WpfMenuItem showLargeView = MainStudioWindow.DesignSurfaceContextMenu.ShowLargeView;
            #endregion
            Mouse.Click(showLargeView, new Point(43, 15));
        }
        [When(@"I Select Source From DotnetTool")]
        public void Select_Source_From_DotnetTool()
        {
            #region Variable Declarations
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox;
            WpfListItem dotNetSource = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.DotNetSource;
            WpfComboBox classNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox;
            WpfListItem assemblyLocationGACCListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.AssemblyLocationGACCListItem;
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox;
            WpfListItem equalsAction = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.EqualsAction;
            WpfButton generateOutputsButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton;
            WpfRow row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1;
            WpfButton saveButton = MainStudioWindow.SideMenuBar.SaveButton;
            #endregion
            Mouse.Click(sourcesComboBox, new Point(119, 7));
            Mouse.Click(dotNetSource, new Point(114, 13));
            Assert.AreEqual(Select_Source_From_DotnetToolParams.ClassNameComboBoxEnabled, classNameComboBox.Enabled, "ClassNameComboBox is not Enabled after selecting a source");
            Mouse.Click(classNameComboBox, new Point(119, 7));
            Mouse.Click(assemblyLocationGACCListItem, new Point(114, 13));
            Assert.AreEqual(Select_Source_From_DotnetToolParams.ActionsComboBoxEnabled, actionsComboBox.Enabled, "ActionsComboBox is not Enabled after selecting ClassName");
            Mouse.Click(actionsComboBox, new Point(119, 7));
            Mouse.Click(equalsAction, new Point(114, 13));
            Assert.AreEqual(Select_Source_From_DotnetToolParams.GenerateOutputsButtonEnabled, generateOutputsButton.Enabled, "GenerateOutputsButton is not Enabled after selecting an Action");
            Assert.AreEqual(Select_Source_From_DotnetToolParams.Row1Enabled, row1.Enabled, "InputsDataGridTable is not Enabled after selecting an Action");
            Mouse.Click(saveButton, new Point(10, 5));
        }
        [When(@"I Select Source From PostgreTool")]
        public void Select_Source_From_PostgreTool()
        {
            #region Variable Declarations
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeViewContentCustom.SourcesComboBox;
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeViewContentCustom.ActionsComboBox;
            #endregion
            Mouse.Click(sourcesComboBox, new Point(119, 7));
            Mouse.Click(sourcesComboBox, new Point(114, 13));
            Assert.AreEqual(Select_Source_From_PostgreToolParams.ActionsComboBoxEnabled, actionsComboBox.Enabled, "Action combobox is not enabled after selecting an Action.");
        }
        [When(@"I Select SystemObject From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            #region Variable Declarations
            WpfComboBox classNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox;
            WpfListItem comboboxlistItemAsSystemObject = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject;
            #endregion
            Mouse.Click(classNameComboBox, new Point(216, 7));
            Assert.AreEqual(Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues.ComboboxlistItemAsSystemObjectExists, comboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(comboboxlistItemAsSystemObject, new Point(137, 7));
            Assert.AreEqual(Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues.ClassNameComboBoxSelectedItem, classNameComboBox.SelectedItem, "System.Object is not selected in DotNet DLL tool large view namespace combobox.");
        }
        [When(@"I Select SystemRandom From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            #region Variable Declarations
            WpfComboBox classNameComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox;
            WpfListItem systemRandomListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SystemRandomListItem;
            #endregion
            Mouse.Click(classNameComboBox, new Point(216, 7));
            Mouse.Click(systemRandomListItem, new Point(137, 7));
            Assert.AreEqual(Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues.ClassNameComboBoxSelectedItem, classNameComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }
        [When(@"I Select TestingReturnText Web Put Source")]
        public void Select_TestingReturnText_Web_Put_Source()
        {
            #region Variable Declarations
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox;
            WpfListItem comboboxListItemAsTestingReturnText = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox.ComboboxListItemAsTestingReturnText;
            #endregion
            Mouse.Click(sourcesComboBox, new Point(196, 11));
            Mouse.Click(comboboxListItemAsTestingReturnText, new Point(129, 13));
        }
        [When(@"I Select Tests From Context Menu")]
        public void Select_Tests_From_Context_Menu()
        {
            #region Variable Declarations
            WpfMenuItem tests = MainStudioWindow.ExplorerContextMenu.Tests;
            WpfTabPage testsTabPage = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage;
            #endregion
            Mouse.Click(tests, new Point(46, 16));
            Assert.AreEqual(Select_Tests_From_Context_MenuParams.TestsTabPageExists, testsTabPage.Exists, "TestsTabPage does not exist after clicking view tests in the explorer context men" +
                    "u.");
        }
        [When(@"I Select ToString From DotNet DLL Large View Action Combobox")]
        public void Select_ToString_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            #region Variable Declarations
            WpfComboBox actionsComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox;
            WpfListItem item1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1;
            #endregion
            Mouse.Click(actionsComboBox, new Point(216, 7));
            Mouse.Click(item1, new Point(137, 7));
            Assert.AreEqual(Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues.ActionsComboBoxSelectedItem, actionsComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }
        [When(@"I Select TSTCIREMOTE From Server Source Wizard Dropdownlist")]
        public void Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist()
        {
            #region Variable Declarations
            WpfListItem tSTCIREMOTE = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.TSTCIREMOTE;
            WpfEdit addressEditBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.AddressEditBox;
            WpfButton testConnectionButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.TestConnectionButton;
            #endregion
            Mouse.Click(tSTCIREMOTE, new Point(70, 19));
            Assert.AreEqual(Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams.AddressEditBoxText, addressEditBox.Text, "Server source address textbox text does not equal TST-CI-REMOTE");
            Assert.AreEqual(Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams.TestConnectionButtonExists, testConnectionButton.Exists, "Server source wizard does not contain a test connection button");
        }
        [When(@"I Select UITestingDBSource From SQL Server Large View Source Combobox")]
        public void Select_UITestingDBSource_From_SQL_Server_Large_View_Source_Combobox()
        {
            #region Variable Declarations
            WpfComboBox sourcesCombobox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox;
            WpfListItem uITestingDBSourceListItem = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.UITestingDBSourceListItem;
            #endregion
            Mouse.Click(sourcesCombobox, new Point(216, 7));
            Mouse.Click(uITestingDBSourceListItem, new Point(137, 7));
            Assert.AreEqual(Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams.SourcesComboboxSelectedItem, sourcesCombobox.SelectedItem, "SQL Server large view source combobox selected item is not equal to UITestingDBSo" +
                    "urce.");
        }
        [When(@"I Select UITestingSource From Web Server Large View Source Combobox")]
        public void Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox()
        {
            #region Variable Declarations
            WpfComboBox sourcesComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox;
            WpfListItem uITesting = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.UITesting;
            WpfButton editSourceButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton;
            WpfButton generateOutputsButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton;
            #endregion
            Mouse.Click(sourcesComboBox, new Point(216, 7));
            Mouse.Click(uITesting, new Point(137, 7));
            Assert.AreEqual(Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams.EditSourceButtonEnabled, editSourceButton.Enabled, "Delete Web large view source combobox EDIT button is disabled.");
            Assert.AreEqual(Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams.GenerateOutputsButtonEnabled, generateOutputsButton.Enabled, "Delete Web large view source combobox GenerateOutput button is disabled.");
        }
        [When(@"I Select User From RunTestAs")]
        public void Select_User_From_RunTestAs()
        {
            #region Variable Declarations
            WpfRadioButton userRadioButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UserRadioButton;
            WpfEdit usernameTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit;
            WpfEdit passwordTextBoxEdit = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTabPage.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit;
            WpfButton saveButton = MainStudioWindow.SideMenuBar.SaveButton;
            #endregion
            userRadioButton.Selected = Select_User_From_RunTestAsParams.UserRadioButtonSelected;
            Assert.AreEqual(Select_User_From_RunTestAsParams.UsernameTextBoxEditExists, usernameTextBoxEdit.Exists, "Username textbox does not exist after clicking RunAsUser radio button");
            Assert.AreEqual(Select_User_From_RunTestAsParams.PasswordTextBoxEditExists, passwordTextBoxEdit.Exists, "Password textbox does not exist after clicking RunAsUser radio button");
            Assert.AreEqual(Select_User_From_RunTestAsParams.SaveButtonEnabled, saveButton.Enabled, "Save Ribbon Menu buton is disabled after changing test");
        }
        [When(@"I Select Zip Compression")]
        public void Select_Zip_Compression()
        {
            #region Variable Declarations
            WpfComboBox selectedCompressComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox;
            WpfListItem normalDefault = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox.NormalDefault;
            #endregion
            Mouse.Click(selectedCompressComboBox, new Point(119, 7));
            Mouse.Click(normalDefault, new Point(114, 13));
        }
        [When(@"I Type 0 Into SQL Server Large View Inputs Row1 Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_Textbox()
        {
            #region Variable Declarations
            WpfEdit dataTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.InputsTable.Row1.DataCell.DataCombobox.DataTextbox;
            #endregion
            dataTextbox.Text = Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues.DataTextboxText;
            Assert.AreEqual(Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues.DataTextboxText1, dataTextbox.Text, "SQL Server large view inputs row 1 data textbox text is not equal to S");
        }
        [When(@"I Type 0 Into SQL Server Large View Test Inputs Row1 Test Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_Textbox()
        {
            #region Variable Declarations
            WpfEdit testDataTextbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox;
            #endregion
            testDataTextbox.Text = Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues.TestDataTextboxText;
            Assert.AreEqual(Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues.TestDataTextboxText1, testDataTextbox.Text, "SQL Server large view test inputs row 1 test data textbox text is not equal to S");
        }
        [When(@"I Type rsaklfsvrgen into DB Source Wizard Server Textbox")]
        public void Type_rsaklfsvrgen_into_DB_Source_Wizard_Server_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Textbox.Text = "rsaklfsvrgen";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.RSAKLFSVRGENDEV.Exists, "RSAKLFSVRGENDEV does not exist as an option in DB source wizard server combobox.");
        }
        [When(@"I Type TestSite into Web Source Wizard Address Textbox")]
        public void Type_TestSite_into_Web_Source_Wizard_Address_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld/IntegrationTestSite/Proxy.ashx";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a vali" +
                    "d web address.");
        }
        #region Properties
        public virtual Click_RunDebug_buttonParams Click_RunDebug_buttonParams
        {
            get
            {
                if ((mClick_RunDebug_buttonParams == null))
                {
                    mClick_RunDebug_buttonParams = new Click_RunDebug_buttonParams();
                }
                return mClick_RunDebug_buttonParams;
            }
        }
        public virtual Click_Save_Ribbon_Button_to_Open_Save_DialogParams Click_Save_Ribbon_Button_to_Open_Save_DialogParams
        {
            get
            {
                if ((mClick_Save_Ribbon_Button_to_Open_Save_DialogParams == null))
                {
                    mClick_Save_Ribbon_Button_to_Open_Save_DialogParams = new Click_Save_Ribbon_Button_to_Open_Save_DialogParams();
                }
                return mClick_Save_Ribbon_Button_to_Open_Save_DialogParams;
            }
        }
        public virtual Click_SQL_Server_Large_View_Generate_OutputsExpectedValues Click_SQL_Server_Large_View_Generate_OutputsExpectedValues
        {
            get
            {
                if ((mClick_SQL_Server_Large_View_Generate_OutputsExpectedValues == null))
                {
                    mClick_SQL_Server_Large_View_Generate_OutputsExpectedValues = new Click_SQL_Server_Large_View_Generate_OutputsExpectedValues();
                }
                return mClick_SQL_Server_Large_View_Generate_OutputsExpectedValues;
            }
        }
        public virtual Click_Unpinned_Workflow_CollapseAllParams Click_Unpinned_Workflow_CollapseAllParams
        {
            get
            {
                if ((mClick_Unpinned_Workflow_CollapseAllParams == null))
                {
                    mClick_Unpinned_Workflow_CollapseAllParams = new Click_Unpinned_Workflow_CollapseAllParams();
                }
                return mClick_Unpinned_Workflow_CollapseAllParams;
            }
        }
        public virtual Click_Unpinned_Workflow_ExpandAllParams Click_Unpinned_Workflow_ExpandAllParams
        {
            get
            {
                if ((mClick_Unpinned_Workflow_ExpandAllParams == null))
                {
                    mClick_Unpinned_Workflow_ExpandAllParams = new Click_Unpinned_Workflow_ExpandAllParams();
                }
                return mClick_Unpinned_Workflow_ExpandAllParams;
            }
        }
        public virtual Click_Variable_IsInputParams Click_Variable_IsInputParams
        {
            get
            {
                if ((mClick_Variable_IsInputParams == null))
                {
                    mClick_Variable_IsInputParams = new Click_Variable_IsInputParams();
                }
                return mClick_Variable_IsInputParams;
            }
        }
        public virtual Drag_Toolbox_Unzip_Onto_DesignSurfaceParams Drag_Toolbox_Unzip_Onto_DesignSurfaceParams
        {
            get
            {
                if ((mDrag_Toolbox_Unzip_Onto_DesignSurfaceParams == null))
                {
                    mDrag_Toolbox_Unzip_Onto_DesignSurfaceParams = new Drag_Toolbox_Unzip_Onto_DesignSurfaceParams();
                }
                return mDrag_Toolbox_Unzip_Onto_DesignSurfaceParams;
            }
        }
        public virtual Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams
        {
            get
            {
                if ((mDrag_Toolbox_Web_Request_Onto_DesignSurfaceParams == null))
                {
                    mDrag_Toolbox_Web_Request_Onto_DesignSurfaceParams = new Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams();
                }
                return mDrag_Toolbox_Web_Request_Onto_DesignSurfaceParams;
            }
        }
        public virtual Drag_Toolbox_Write_File_Onto_DesignSurfaceParams Drag_Toolbox_Write_File_Onto_DesignSurfaceParams
        {
            get
            {
                if ((mDrag_Toolbox_Write_File_Onto_DesignSurfaceParams == null))
                {
                    mDrag_Toolbox_Write_File_Onto_DesignSurfaceParams = new Drag_Toolbox_Write_File_Onto_DesignSurfaceParams();
                }
                return mDrag_Toolbox_Write_File_Onto_DesignSurfaceParams;
            }
        }
        public virtual Drag_Toolbox_XPath_Onto_DesignSurfaceParams Drag_Toolbox_XPath_Onto_DesignSurfaceParams
        {
            get
            {
                if ((mDrag_Toolbox_XPath_Onto_DesignSurfaceParams == null))
                {
                    mDrag_Toolbox_XPath_Onto_DesignSurfaceParams = new Drag_Toolbox_XPath_Onto_DesignSurfaceParams();
                }
                return mDrag_Toolbox_XPath_Onto_DesignSurfaceParams;
            }
        }
        public virtual Drag_Toolbox_Zip_Onto_DesignSurfaceParams Drag_Toolbox_Zip_Onto_DesignSurfaceParams
        {
            get
            {
                if ((mDrag_Toolbox_Zip_Onto_DesignSurfaceParams == null))
                {
                    mDrag_Toolbox_Zip_Onto_DesignSurfaceParams = new Drag_Toolbox_Zip_Onto_DesignSurfaceParams();
                }
                return mDrag_Toolbox_Zip_Onto_DesignSurfaceParams;
            }
        }
        public virtual Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams
        {
            get
            {
                if ((mDuplicate_Explorer_Localhost_First_Item_With_Context_MenuParams == null))
                {
                    mDuplicate_Explorer_Localhost_First_Item_With_Context_MenuParams = new Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams();
                }
                return mDuplicate_Explorer_Localhost_First_Item_With_Context_MenuParams;
            }
        }
        public virtual Enter_DeployViewOnly_Into_Deploy_Source_FilterParams Enter_DeployViewOnly_Into_Deploy_Source_FilterParams
        {
            get
            {
                if ((mEnter_DeployViewOnly_Into_Deploy_Source_FilterParams == null))
                {
                    mEnter_DeployViewOnly_Into_Deploy_Source_FilterParams = new Enter_DeployViewOnly_Into_Deploy_Source_FilterParams();
                }
                return mEnter_DeployViewOnly_Into_Deploy_Source_FilterParams;
            }
        }
        public virtual Enter_Duplicate_workflow_nameParams Enter_Duplicate_workflow_nameParams
        {
            get
            {
                if ((mEnter_Duplicate_workflow_nameParams == null))
                {
                    mEnter_Duplicate_workflow_nameParams = new Enter_Duplicate_workflow_nameParams();
                }
                return mEnter_Duplicate_workflow_nameParams;
            }
        }
        public virtual Enter_InputDebug_valueParams Enter_InputDebug_valueParams
        {
            get
            {
                if ((mEnter_InputDebug_valueParams == null))
                {
                    mEnter_InputDebug_valueParams = new Enter_InputDebug_valueParams();
                }
                return mEnter_InputDebug_valueParams;
            }
        }
        public virtual Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams
        {
            get
            {
                if ((mEnter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams == null))
                {
                    mEnter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams = new Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams();
                }
                return mEnter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams;
            }
        }
        public virtual Enter_Public_As_Windows_GroupParams Enter_Public_As_Windows_GroupParams
        {
            get
            {
                if ((mEnter_Public_As_Windows_GroupParams == null))
                {
                    mEnter_Public_As_Windows_GroupParams = new Enter_Public_As_Windows_GroupParams();
                }
                return mEnter_Public_As_Windows_GroupParams;
            }
        }
        public virtual Enter_RunAsUser_Username_And_PasswordParams Enter_RunAsUser_Username_And_PasswordParams
        {
            get
            {
                if ((mEnter_RunAsUser_Username_And_PasswordParams == null))
                {
                    mEnter_RunAsUser_Username_And_PasswordParams = new Enter_RunAsUser_Username_And_PasswordParams();
                }
                return mEnter_RunAsUser_Username_And_PasswordParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams = new Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams = new Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_From_OnUpload_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_From_OnUpload_ToolParams = new Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_From_OnUpload_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams = new Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams = new Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams
        {
            get
            {
                if ((mEnter_Sharepoint_Server_Path_To_OnUpload_ToolParams == null))
                {
                    mEnter_Sharepoint_Server_Path_To_OnUpload_ToolParams = new Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams();
                }
                return mEnter_Sharepoint_Server_Path_To_OnUpload_ToolParams;
            }
        }
        public virtual Enter_Sharepoint_ServerSource_ServerNameParams Enter_Sharepoint_ServerSource_ServerNameParams
        {
            get
            {
                if ((mEnter_Sharepoint_ServerSource_ServerNameParams == null))
                {
                    mEnter_Sharepoint_ServerSource_ServerNameParams = new Enter_Sharepoint_ServerSource_ServerNameParams();
                }
                return mEnter_Sharepoint_ServerSource_ServerNameParams;
            }
        }
        public virtual Enter_Sharepoint_ServerSource_User_CredentialsParams Enter_Sharepoint_ServerSource_User_CredentialsParams
        {
            get
            {
                if ((mEnter_Sharepoint_ServerSource_User_CredentialsParams == null))
                {
                    mEnter_Sharepoint_ServerSource_User_CredentialsParams = new Enter_Sharepoint_ServerSource_User_CredentialsParams();
                }
                return mEnter_Sharepoint_ServerSource_User_CredentialsParams;
            }
        }
        public virtual Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams
        {
            get
            {
                if ((mEnter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams == null))
                {
                    mEnter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams = new Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams();
                }
                return mEnter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams;
            }
        }
        public virtual Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams
        {
            get
            {
                if ((mEnter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams == null))
                {
                    mEnter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams = new Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams();
                }
                return mEnter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams;
            }
        }
        public virtual Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams
        {
            get
            {
                if ((mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams == null))
                {
                    mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams = new Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams();
                }
                return mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams;
            }
        }
        public virtual Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams
        {
            get
            {
                if ((mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams == null))
                {
                    mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams = new Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams();
                }
                return mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams;
            }
        }
        public virtual Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams
        {
            get
            {
                if ((mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams == null))
                {
                    mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams = new Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams();
                }
                return mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams;
            }
        }
        public virtual Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams
        {
            get
            {
                if ((mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams == null))
                {
                    mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams = new Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams();
                }
                return mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams;
            }
        }
        public virtual Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams
        {
            get
            {
                if ((mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams == null))
                {
                    mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams = new Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams();
                }
                return mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams;
            }
        }
        public virtual Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams
        {
            get
            {
                if ((mEnter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams == null))
                {
                    mEnter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams = new Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams();
                }
                return mEnter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams;
            }
        }
        public virtual Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams
        {
            get
            {
                if ((mEnter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams == null))
                {
                    mEnter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams = new Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams();
                }
                return mEnter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams;
            }
        }
        public virtual Enter_Vaiablelist_ItemsParams Enter_Vaiablelist_ItemsParams
        {
            get
            {
                if ((mEnter_Vaiablelist_ItemsParams == null))
                {
                    mEnter_Vaiablelist_ItemsParams = new Enter_Vaiablelist_ItemsParams();
                }
                return mEnter_Vaiablelist_ItemsParams;
            }
        }
        public virtual Filter_variablesParams Filter_variablesParams
        {
            get
            {
                if ((mFilter_variablesParams == null))
                {
                    mFilter_variablesParams = new Filter_variablesParams();
                }
                return mFilter_variablesParams;
            }
        }
        public virtual I_Open_Explorer_First_Item_Context_MenuParams I_Open_Explorer_First_Item_Context_MenuParams
        {
            get
            {
                if ((mI_Open_Explorer_First_Item_Context_MenuParams == null))
                {
                    mI_Open_Explorer_First_Item_Context_MenuParams = new I_Open_Explorer_First_Item_Context_MenuParams();
                }
                return mI_Open_Explorer_First_Item_Context_MenuParams;
            }
        }
        public virtual Open_AggregateCalculate_Tool_large_viewParams Open_AggregateCalculate_Tool_large_viewParams
        {
            get
            {
                if ((mOpen_AggregateCalculate_Tool_large_viewParams == null))
                {
                    mOpen_AggregateCalculate_Tool_large_viewParams = new Open_AggregateCalculate_Tool_large_viewParams();
                }
                return mOpen_AggregateCalculate_Tool_large_viewParams;
            }
        }
        public virtual Open_Assign_Tool_Large_ViewParams Open_Assign_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Assign_Tool_Large_ViewParams == null))
                {
                    mOpen_Assign_Tool_Large_ViewParams = new Open_Assign_Tool_Large_ViewParams();
                }
                return mOpen_Assign_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams
        {
            get
            {
                if ((mOpen_Assign_Tool_On_Unpinned_Tab_Large_ViewParams == null))
                {
                    mOpen_Assign_Tool_On_Unpinned_Tab_Large_ViewParams = new Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams();
                }
                return mOpen_Assign_Tool_On_Unpinned_Tab_Large_ViewParams;
            }
        }
        public virtual Open_Assign_Tool_Qvi_Large_ViewParams Open_Assign_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Assign_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Assign_Tool_Qvi_Large_ViewParams = new Open_Assign_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Assign_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams
        {
            get
            {
                if ((mOpen_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams == null))
                {
                    mOpen_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams = new Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams();
                }
                return mOpen_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams;
            }
        }
        public virtual Open_AssignObject_Large_ToolParams Open_AssignObject_Large_ToolParams
        {
            get
            {
                if ((mOpen_AssignObject_Large_ToolParams == null))
                {
                    mOpen_AssignObject_Large_ToolParams = new Open_AssignObject_Large_ToolParams();
                }
                return mOpen_AssignObject_Large_ToolParams;
            }
        }
        public virtual Open_AssignObject_QVI_LargeViewParams Open_AssignObject_QVI_LargeViewParams
        {
            get
            {
                if ((mOpen_AssignObject_QVI_LargeViewParams == null))
                {
                    mOpen_AssignObject_QVI_LargeViewParams = new Open_AssignObject_QVI_LargeViewParams();
                }
                return mOpen_AssignObject_QVI_LargeViewParams;
            }
        }
        public virtual Open_Base_Conversion_Tool_Large_ViewParams Open_Base_Conversion_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Base_Conversion_Tool_Large_ViewParams == null))
                {
                    mOpen_Base_Conversion_Tool_Large_ViewParams = new Open_Base_Conversion_Tool_Large_ViewParams();
                }
                return mOpen_Base_Conversion_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Base_Conversion_Tool_Qvi_Large_ViewParams Open_Base_Conversion_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Base_Conversion_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Base_Conversion_Tool_Qvi_Large_ViewParams = new Open_Base_Conversion_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Base_Conversion_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Calculate_Tool_Large_ViewParams Open_Calculate_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Calculate_Tool_Large_ViewParams == null))
                {
                    mOpen_Calculate_Tool_Large_ViewParams = new Open_Calculate_Tool_Large_ViewParams();
                }
                return mOpen_Calculate_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Case_Conversion_Tool_Large_ViewParams Open_Case_Conversion_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Case_Conversion_Tool_Large_ViewParams == null))
                {
                    mOpen_Case_Conversion_Tool_Large_ViewParams = new Open_Case_Conversion_Tool_Large_ViewParams();
                }
                return mOpen_Case_Conversion_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Case_Conversion_Tool_Qvi_Large_ViewParams Open_Case_Conversion_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Case_Conversion_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Case_Conversion_Tool_Qvi_Large_ViewParams = new Open_Case_Conversion_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Case_Conversion_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_CMD_Line_Tool_Large_ViewParams Open_CMD_Line_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_CMD_Line_Tool_Large_ViewParams == null))
                {
                    mOpen_CMD_Line_Tool_Large_ViewParams = new Open_CMD_Line_Tool_Large_ViewParams();
                }
                return mOpen_CMD_Line_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Copy_Tool_Large_ViewParams Open_Copy_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Copy_Tool_Large_ViewParams == null))
                {
                    mOpen_Copy_Tool_Large_ViewParams = new Open_Copy_Tool_Large_ViewParams();
                }
                return mOpen_Copy_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Create_Tool_Large_ViewParams Open_Create_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Create_Tool_Large_ViewParams == null))
                {
                    mOpen_Create_Tool_Large_ViewParams = new Open_Create_Tool_Large_ViewParams();
                }
                return mOpen_Create_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Data_Merge_Large_ViewParams Open_Data_Merge_Large_ViewParams
        {
            get
            {
                if ((mOpen_Data_Merge_Large_ViewParams == null))
                {
                    mOpen_Data_Merge_Large_ViewParams = new Open_Data_Merge_Large_ViewParams();
                }
                return mOpen_Data_Merge_Large_ViewParams;
            }
        }
        public virtual Open_Data_Merge_Tool_Qvi_Large_ViewParams Open_Data_Merge_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Data_Merge_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Data_Merge_Tool_Qvi_Large_ViewParams = new Open_Data_Merge_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Data_Merge_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Data_Split_Large_ViewParams Open_Data_Split_Large_ViewParams
        {
            get
            {
                if ((mOpen_Data_Split_Large_ViewParams == null))
                {
                    mOpen_Data_Split_Large_ViewParams = new Open_Data_Split_Large_ViewParams();
                }
                return mOpen_Data_Split_Large_ViewParams;
            }
        }
        public virtual Open_Data_Split_Tool_Qvi_Large_ViewParams Open_Data_Split_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Data_Split_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Data_Split_Tool_Qvi_Large_ViewParams = new Open_Data_Split_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Data_Split_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_DateTime_LargeViewParams Open_DateTime_LargeViewParams
        {
            get
            {
                if ((mOpen_DateTime_LargeViewParams == null))
                {
                    mOpen_DateTime_LargeViewParams = new Open_DateTime_LargeViewParams();
                }
                return mOpen_DateTime_LargeViewParams;
            }
        }
        public virtual Open_DateTimeDiff_LargeViewParams Open_DateTimeDiff_LargeViewParams
        {
            get
            {
                if ((mOpen_DateTimeDiff_LargeViewParams == null))
                {
                    mOpen_DateTimeDiff_LargeViewParams = new Open_DateTimeDiff_LargeViewParams();
                }
                return mOpen_DateTimeDiff_LargeViewParams;
            }
        }
        public virtual Open_Decision_Large_ViewParams Open_Decision_Large_ViewParams
        {
            get
            {
                if ((mOpen_Decision_Large_ViewParams == null))
                {
                    mOpen_Decision_Large_ViewParams = new Open_Decision_Large_ViewParams();
                }
                return mOpen_Decision_Large_ViewParams;
            }
        }
        public virtual Open_Delete_Tool_Large_ViewParams Open_Delete_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Delete_Tool_Large_ViewParams == null))
                {
                    mOpen_Delete_Tool_Large_ViewParams = new Open_Delete_Tool_Large_ViewParams();
                }
                return mOpen_Delete_Tool_Large_ViewParams;
            }
        }
        public virtual Open_DeleteRecords_Large_ViewParams Open_DeleteRecords_Large_ViewParams
        {
            get
            {
                if ((mOpen_DeleteRecords_Large_ViewParams == null))
                {
                    mOpen_DeleteRecords_Large_ViewParams = new Open_DeleteRecords_Large_ViewParams();
                }
                return mOpen_DeleteRecords_Large_ViewParams;
            }
        }
        public virtual Open_DeleteWeb_Tool_Large_ViewParams Open_DeleteWeb_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_DeleteWeb_Tool_Large_ViewParams == null))
                {
                    mOpen_DeleteWeb_Tool_Large_ViewParams = new Open_DeleteWeb_Tool_Large_ViewParams();
                }
                return mOpen_DeleteWeb_Tool_Large_ViewParams;
            }
        }
        public virtual Open_DotNet_DLL_Connector_Tool_Large_ViewParams Open_DotNet_DLL_Connector_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_DotNet_DLL_Connector_Tool_Large_ViewParams == null))
                {
                    mOpen_DotNet_DLL_Connector_Tool_Large_ViewParams = new Open_DotNet_DLL_Connector_Tool_Large_ViewParams();
                }
                return mOpen_DotNet_DLL_Connector_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams
        {
            get
            {
                if ((mOpen_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams == null))
                {
                    mOpen_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams = new Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams();
                }
                return mOpen_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams;
            }
        }
        public virtual Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams
        {
            get
            {
                if ((mOpen_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams == null))
                {
                    mOpen_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams = new Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams();
                }
                return mOpen_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams;
            }
        }
        public virtual Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams
        {
            get
            {
                if ((mOpen_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams == null))
                {
                    mOpen_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams = new Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams();
                }
                return mOpen_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams;
            }
        }
        public virtual Open_Explorer_First_Item_Tests_With_Context_MenuParams Open_Explorer_First_Item_Tests_With_Context_MenuParams
        {
            get
            {
                if ((mOpen_Explorer_First_Item_Tests_With_Context_MenuParams == null))
                {
                    mOpen_Explorer_First_Item_Tests_With_Context_MenuParams = new Open_Explorer_First_Item_Tests_With_Context_MenuParams();
                }
                return mOpen_Explorer_First_Item_Tests_With_Context_MenuParams;
            }
        }
        public virtual Open_Explorer_First_Item_With_Context_MenuParams Open_Explorer_First_Item_With_Context_MenuParams
        {
            get
            {
                if ((mOpen_Explorer_First_Item_With_Context_MenuParams == null))
                {
                    mOpen_Explorer_First_Item_With_Context_MenuParams = new Open_Explorer_First_Item_With_Context_MenuParams();
                }
                return mOpen_Explorer_First_Item_With_Context_MenuParams;
            }
        }
        public virtual Open_Find_Index_Tool_Large_ViewParams Open_Find_Index_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Find_Index_Tool_Large_ViewParams == null))
                {
                    mOpen_Find_Index_Tool_Large_ViewParams = new Open_Find_Index_Tool_Large_ViewParams();
                }
                return mOpen_Find_Index_Tool_Large_ViewParams;
            }
        }
        public virtual Open_ForEach_Large_ViewParams Open_ForEach_Large_ViewParams
        {
            get
            {
                if ((mOpen_ForEach_Large_ViewParams == null))
                {
                    mOpen_ForEach_Large_ViewParams = new Open_ForEach_Large_ViewParams();
                }
                return mOpen_ForEach_Large_ViewParams;
            }
        }
        public virtual Open_GET_Web_Connector_Tool_Large_ViewParams Open_GET_Web_Connector_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_GET_Web_Connector_Tool_Large_ViewParams == null))
                {
                    mOpen_GET_Web_Connector_Tool_Large_ViewParams = new Open_GET_Web_Connector_Tool_Large_ViewParams();
                }
                return mOpen_GET_Web_Connector_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Javascript_Large_ViewParams Open_Javascript_Large_ViewParams
        {
            get
            {
                if ((mOpen_Javascript_Large_ViewParams == null))
                {
                    mOpen_Javascript_Large_ViewParams = new Open_Javascript_Large_ViewParams();
                }
                return mOpen_Javascript_Large_ViewParams;
            }
        }
        public virtual Open_Json_Tool_Large_ViewParams Open_Json_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Json_Tool_Large_ViewParams == null))
                {
                    mOpen_Json_Tool_Large_ViewParams = new Open_Json_Tool_Large_ViewParams();
                }
                return mOpen_Json_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Json_Tool_Qvi_Large_ViewParams Open_Json_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Json_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Json_Tool_Qvi_Large_ViewParams = new Open_Json_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Json_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Length_Tool_Large_ViewParams Open_Length_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Length_Tool_Large_ViewParams == null))
                {
                    mOpen_Length_Tool_Large_ViewParams = new Open_Length_Tool_Large_ViewParams();
                }
                return mOpen_Length_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Move_Tool_Large_ViewParams Open_Move_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Move_Tool_Large_ViewParams == null))
                {
                    mOpen_Move_Tool_Large_ViewParams = new Open_Move_Tool_Large_ViewParams();
                }
                return mOpen_Move_Tool_Large_ViewParams;
            }
        }
        public virtual Open_NumberFormat_Toolbox_Large_ViewParams Open_NumberFormat_Toolbox_Large_ViewParams
        {
            get
            {
                if ((mOpen_NumberFormat_Toolbox_Large_ViewParams == null))
                {
                    mOpen_NumberFormat_Toolbox_Large_ViewParams = new Open_NumberFormat_Toolbox_Large_ViewParams();
                }
                return mOpen_NumberFormat_Toolbox_Large_ViewParams;
            }
        }
        public virtual Open_ODBC_Tool_Large_ViewExpectedValues Open_ODBC_Tool_Large_ViewExpectedValues
        {
            get
            {
                if ((mOpen_ODBC_Tool_Large_ViewExpectedValues == null))
                {
                    mOpen_ODBC_Tool_Large_ViewExpectedValues = new Open_ODBC_Tool_Large_ViewExpectedValues();
                }
                return mOpen_ODBC_Tool_Large_ViewExpectedValues;
            }
        }
        public virtual Open_Oracle_Tool_Large_ViewExpectedValues Open_Oracle_Tool_Large_ViewExpectedValues
        {
            get
            {
                if ((mOpen_Oracle_Tool_Large_ViewExpectedValues == null))
                {
                    mOpen_Oracle_Tool_Large_ViewExpectedValues = new Open_Oracle_Tool_Large_ViewExpectedValues();
                }
                return mOpen_Oracle_Tool_Large_ViewExpectedValues;
            }
        }
        public virtual Open_Postgre_Tool_Large_ViewExpectedValues Open_Postgre_Tool_Large_ViewExpectedValues
        {
            get
            {
                if ((mOpen_Postgre_Tool_Large_ViewExpectedValues == null))
                {
                    mOpen_Postgre_Tool_Large_ViewExpectedValues = new Open_Postgre_Tool_Large_ViewExpectedValues();
                }
                return mOpen_Postgre_Tool_Large_ViewExpectedValues;
            }
        }
        public virtual Open_PostWeb_RequestTool_Large_ViewParams Open_PostWeb_RequestTool_Large_ViewParams
        {
            get
            {
                if ((mOpen_PostWeb_RequestTool_Large_ViewParams == null))
                {
                    mOpen_PostWeb_RequestTool_Large_ViewParams = new Open_PostWeb_RequestTool_Large_ViewParams();
                }
                return mOpen_PostWeb_RequestTool_Large_ViewParams;
            }
        }
        public virtual Open_Python_Large_ViewParams Open_Python_Large_ViewParams
        {
            get
            {
                if ((mOpen_Python_Large_ViewParams == null))
                {
                    mOpen_Python_Large_ViewParams = new Open_Python_Large_ViewParams();
                }
                return mOpen_Python_Large_ViewParams;
            }
        }
        public virtual Open_RabbitMqConsume_LargeViewParams Open_RabbitMqConsume_LargeViewParams
        {
            get
            {
                if ((mOpen_RabbitMqConsume_LargeViewParams == null))
                {
                    mOpen_RabbitMqConsume_LargeViewParams = new Open_RabbitMqConsume_LargeViewParams();
                }
                return mOpen_RabbitMqConsume_LargeViewParams;
            }
        }
        public virtual Open_RabbitMqPublish_LargeViewParams Open_RabbitMqPublish_LargeViewParams
        {
            get
            {
                if ((mOpen_RabbitMqPublish_LargeViewParams == null))
                {
                    mOpen_RabbitMqPublish_LargeViewParams = new Open_RabbitMqPublish_LargeViewParams();
                }
                return mOpen_RabbitMqPublish_LargeViewParams;
            }
        }
        public virtual Open_Random_Large_ToolParams Open_Random_Large_ToolParams
        {
            get
            {
                if ((mOpen_Random_Large_ToolParams == null))
                {
                    mOpen_Random_Large_ToolParams = new Open_Random_Large_ToolParams();
                }
                return mOpen_Random_Large_ToolParams;
            }
        }
        public virtual Open_Read_File_Tool_Large_ViewParams Open_Read_File_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Read_File_Tool_Large_ViewParams == null))
                {
                    mOpen_Read_File_Tool_Large_ViewParams = new Open_Read_File_Tool_Large_ViewParams();
                }
                return mOpen_Read_File_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Read_Folder_Tool_Large_ViewParams Open_Read_Folder_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Read_Folder_Tool_Large_ViewParams == null))
                {
                    mOpen_Read_Folder_Tool_Large_ViewParams = new Open_Read_Folder_Tool_Large_ViewParams();
                }
                return mOpen_Read_Folder_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Rename_Tool_Large_ViewParams Open_Rename_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Rename_Tool_Large_ViewParams == null))
                {
                    mOpen_Rename_Tool_Large_ViewParams = new Open_Rename_Tool_Large_ViewParams();
                }
                return mOpen_Rename_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Replace_Tool_Large_ViewParams Open_Replace_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Replace_Tool_Large_ViewParams == null))
                {
                    mOpen_Replace_Tool_Large_ViewParams = new Open_Replace_Tool_Large_ViewParams();
                }
                return mOpen_Replace_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Ruby_Large_ViewParams Open_Ruby_Large_ViewParams
        {
            get
            {
                if ((mOpen_Ruby_Large_ViewParams == null))
                {
                    mOpen_Ruby_Large_ViewParams = new Open_Ruby_Large_ViewParams();
                }
                return mOpen_Ruby_Large_ViewParams;
            }
        }
        public virtual Open_Selectandapply_Large_ViewParams Open_Selectandapply_Large_ViewParams
        {
            get
            {
                if ((mOpen_Selectandapply_Large_ViewParams == null))
                {
                    mOpen_Selectandapply_Large_ViewParams = new Open_Selectandapply_Large_ViewParams();
                }
                return mOpen_Selectandapply_Large_ViewParams;
            }
        }
        public virtual Open_Sequence_Large_tool_ViewParams Open_Sequence_Large_tool_ViewParams
        {
            get
            {
                if ((mOpen_Sequence_Large_tool_ViewParams == null))
                {
                    mOpen_Sequence_Large_tool_ViewParams = new Open_Sequence_Large_tool_ViewParams();
                }
                return mOpen_Sequence_Large_tool_ViewParams;
            }
        }
        public virtual Open_SMTP_Email_Tool_Large_ViewParams Open_SMTP_Email_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_SMTP_Email_Tool_Large_ViewParams == null))
                {
                    mOpen_SMTP_Email_Tool_Large_ViewParams = new Open_SMTP_Email_Tool_Large_ViewParams();
                }
                return mOpen_SMTP_Email_Tool_Large_ViewParams;
            }
        }
        public virtual Open_SQL_Bulk_Insert_Tool_Large_ViewParams Open_SQL_Bulk_Insert_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_SQL_Bulk_Insert_Tool_Large_ViewParams == null))
                {
                    mOpen_SQL_Bulk_Insert_Tool_Large_ViewParams = new Open_SQL_Bulk_Insert_Tool_Large_ViewParams();
                }
                return mOpen_SQL_Bulk_Insert_Tool_Large_ViewParams;
            }
        }
        public virtual Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams = new Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_SQL_Large_View_FromContextMenuParams Open_SQL_Large_View_FromContextMenuParams
        {
            get
            {
                if ((mOpen_SQL_Large_View_FromContextMenuParams == null))
                {
                    mOpen_SQL_Large_View_FromContextMenuParams = new Open_SQL_Large_View_FromContextMenuParams();
                }
                return mOpen_SQL_Large_View_FromContextMenuParams;
            }
        }
        public virtual Open_Sql_Server_Tool_Large_ViewExpectedValues Open_Sql_Server_Tool_Large_ViewExpectedValues
        {
            get
            {
                if ((mOpen_Sql_Server_Tool_Large_ViewExpectedValues == null))
                {
                    mOpen_Sql_Server_Tool_Large_ViewExpectedValues = new Open_Sql_Server_Tool_Large_ViewExpectedValues();
                }
                return mOpen_Sql_Server_Tool_Large_ViewExpectedValues;
            }
        }
        public virtual Open_Switch_Tool_Large_ViewParams Open_Switch_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Switch_Tool_Large_ViewParams == null))
                {
                    mOpen_Switch_Tool_Large_ViewParams = new Open_Switch_Tool_Large_ViewParams();
                }
                return mOpen_Switch_Tool_Large_ViewParams;
            }
        }
        public virtual Open_System_Information_Tool_Large_ViewParams Open_System_Information_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_System_Information_Tool_Large_ViewParams == null))
                {
                    mOpen_System_Information_Tool_Large_ViewParams = new Open_System_Information_Tool_Large_ViewParams();
                }
                return mOpen_System_Information_Tool_Large_ViewParams;
            }
        }
        public virtual Open_System_Information_Tool_Qvi_Large_ViewParams Open_System_Information_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_System_Information_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_System_Information_Tool_Qvi_Large_ViewParams = new Open_System_Information_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_System_Information_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Unzip_Tool_Large_ViewParams Open_Unzip_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Unzip_Tool_Large_ViewParams == null))
                {
                    mOpen_Unzip_Tool_Large_ViewParams = new Open_Unzip_Tool_Large_ViewParams();
                }
                return mOpen_Unzip_Tool_Large_ViewParams;
            }
        }
        public virtual Open_WebRequest_LargeViewParams Open_WebRequest_LargeViewParams
        {
            get
            {
                if ((mOpen_WebRequest_LargeViewParams == null))
                {
                    mOpen_WebRequest_LargeViewParams = new Open_WebRequest_LargeViewParams();
                }
                return mOpen_WebRequest_LargeViewParams;
            }
        }
        public virtual Open_Write_File_Tool_Large_ViewParams Open_Write_File_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Write_File_Tool_Large_ViewParams == null))
                {
                    mOpen_Write_File_Tool_Large_ViewParams = new Open_Write_File_Tool_Large_ViewParams();
                }
                return mOpen_Write_File_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Xpath_Tool_Large_ViewParams Open_Xpath_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Xpath_Tool_Large_ViewParams == null))
                {
                    mOpen_Xpath_Tool_Large_ViewParams = new Open_Xpath_Tool_Large_ViewParams();
                }
                return mOpen_Xpath_Tool_Large_ViewParams;
            }
        }
        public virtual Open_Xpath_Tool_Qvi_Large_ViewParams Open_Xpath_Tool_Qvi_Large_ViewParams
        {
            get
            {
                if ((mOpen_Xpath_Tool_Qvi_Large_ViewParams == null))
                {
                    mOpen_Xpath_Tool_Qvi_Large_ViewParams = new Open_Xpath_Tool_Qvi_Large_ViewParams();
                }
                return mOpen_Xpath_Tool_Qvi_Large_ViewParams;
            }
        }
        public virtual Open_Zip_Tool_Large_ViewParams Open_Zip_Tool_Large_ViewParams
        {
            get
            {
                if ((mOpen_Zip_Tool_Large_ViewParams == null))
                {
                    mOpen_Zip_Tool_Large_ViewParams = new Open_Zip_Tool_Large_ViewParams();
                }
                return mOpen_Zip_Tool_Large_ViewParams;
            }
        }
        public virtual Press_F6Params Press_F6Params
        {
            get
            {
                if ((mPress_F6Params == null))
                {
                    mPress_F6Params = new Press_F6Params();
                }
                return mPress_F6Params;
            }
        }
        public virtual PressF11_EnterFullScreenParams PressF11_EnterFullScreenParams
        {
            get
            {
                if ((mPressF11_EnterFullScreenParams == null))
                {
                    mPressF11_EnterFullScreenParams = new PressF11_EnterFullScreenParams();
                }
                return mPressF11_EnterFullScreenParams;
            }
        }
        public virtual RabbitMqAssertsExpectedValues RabbitMqAssertsExpectedValues
        {
            get
            {
                if ((mRabbitMqAssertsExpectedValues == null))
                {
                    mRabbitMqAssertsExpectedValues = new RabbitMqAssertsExpectedValues();
                }
                return mRabbitMqAssertsExpectedValues;
            }
        }
        public virtual Remove_WorkflowName_From_Save_DialogParams Remove_WorkflowName_From_Save_DialogParams
        {
            get
            {
                if ((mRemove_WorkflowName_From_Save_DialogParams == null))
                {
                    mRemove_WorkflowName_From_Save_DialogParams = new Remove_WorkflowName_From_Save_DialogParams();
                }
                return mRemove_WorkflowName_From_Save_DialogParams;
            }
        }
        public virtual Rename_FolderItem_ToNewFolderItemParams Rename_FolderItem_ToNewFolderItemParams
        {
            get
            {
                if ((mRename_FolderItem_ToNewFolderItemParams == null))
                {
                    mRename_FolderItem_ToNewFolderItemParams = new Rename_FolderItem_ToNewFolderItemParams();
                }
                return mRename_FolderItem_ToNewFolderItemParams;
            }
        }
        public virtual Rename_LocalFolder_To_SecondFolderParams Rename_LocalFolder_To_SecondFolderParams
        {
            get
            {
                if ((mRename_LocalFolder_To_SecondFolderParams == null))
                {
                    mRename_LocalFolder_To_SecondFolderParams = new Rename_LocalFolder_To_SecondFolderParams();
                }
                return mRename_LocalFolder_To_SecondFolderParams;
            }
        }
        public virtual Rename_LocalWorkflow_To_SecodWorkFlowParams Rename_LocalWorkflow_To_SecodWorkFlowParams
        {
            get
            {
                if ((mRename_LocalWorkflow_To_SecodWorkFlowParams == null))
                {
                    mRename_LocalWorkflow_To_SecodWorkFlowParams = new Rename_LocalWorkflow_To_SecodWorkFlowParams();
                }
                return mRename_LocalWorkflow_To_SecodWorkFlowParams;
            }
        }
        public virtual Restore_Unpinned_Tab_Using_Context_MenuExpectedValues Restore_Unpinned_Tab_Using_Context_MenuExpectedValues
        {
            get
            {
                if ((mRestore_Unpinned_Tab_Using_Context_MenuExpectedValues == null))
                {
                    mRestore_Unpinned_Tab_Using_Context_MenuExpectedValues = new Restore_Unpinned_Tab_Using_Context_MenuExpectedValues();
                }
                return mRestore_Unpinned_Tab_Using_Context_MenuExpectedValues;
            }
        }
        public virtual RightClick_Explorer_Localhost_First_ItemParams RightClick_Explorer_Localhost_First_ItemParams
        {
            get
            {
                if ((mRightClick_Explorer_Localhost_First_ItemParams == null))
                {
                    mRightClick_Explorer_Localhost_First_ItemParams = new RightClick_Explorer_Localhost_First_ItemParams();
                }
                return mRightClick_Explorer_Localhost_First_ItemParams;
            }
        }
        public virtual Search_And_Select_DiceRollParams Search_And_Select_DiceRollParams
        {
            get
            {
                if ((mSearch_And_Select_DiceRollParams == null))
                {
                    mSearch_And_Select_DiceRollParams = new Search_And_Select_DiceRollParams();
                }
                return mSearch_And_Select_DiceRollParams;
            }
        }
        public virtual Search_And_Select_HelloWolrdParams Search_And_Select_HelloWolrdParams
        {
            get
            {
                if ((mSearch_And_Select_HelloWolrdParams == null))
                {
                    mSearch_And_Select_HelloWolrdParams = new Search_And_Select_HelloWolrdParams();
                }
                return mSearch_And_Select_HelloWolrdParams;
            }
        }
        public virtual Select_Action_From_PostgreToolParams Select_Action_From_PostgreToolParams
        {
            get
            {
                if ((mSelect_Action_From_PostgreToolParams == null))
                {
                    mSelect_Action_From_PostgreToolParams = new Select_Action_From_PostgreToolParams();
                }
                return mSelect_Action_From_PostgreToolParams;
            }
        }
        public virtual Select_DatabaseAndTable_From_BulkInsert_ToolParams Select_DatabaseAndTable_From_BulkInsert_ToolParams
        {
            get
            {
                if ((mSelect_DatabaseAndTable_From_BulkInsert_ToolParams == null))
                {
                    mSelect_DatabaseAndTable_From_BulkInsert_ToolParams = new Select_DatabaseAndTable_From_BulkInsert_ToolParams();
                }
                return mSelect_DatabaseAndTable_From_BulkInsert_ToolParams;
            }
        }
        public virtual Select_Delete_FromExplorerContextMenuParams Select_Delete_FromExplorerContextMenuParams
        {
            get
            {
                if ((mSelect_Delete_FromExplorerContextMenuParams == null))
                {
                    mSelect_Delete_FromExplorerContextMenuParams = new Select_Delete_FromExplorerContextMenuParams();
                }
                return mSelect_Delete_FromExplorerContextMenuParams;
            }
        }
        public virtual Select_Deploy_FromExplorerContextMenuParams Select_Deploy_FromExplorerContextMenuParams
        {
            get
            {
                if ((mSelect_Deploy_FromExplorerContextMenuParams == null))
                {
                    mSelect_Deploy_FromExplorerContextMenuParams = new Select_Deploy_FromExplorerContextMenuParams();
                }
                return mSelect_Deploy_FromExplorerContextMenuParams;
            }
        }
        public virtual Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams
        {
            get
            {
                if ((mSelect_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams == null))
                {
                    mSelect_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams = new Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams();
                }
                return mSelect_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams;
            }
        }
        public virtual Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues == null))
                {
                    mSelect_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues = new Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues();
                }
                return mSelect_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
            }
        }
        public virtual Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues == null))
                {
                    mSelect_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues = new Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues();
                }
                return mSelect_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues;
            }
        }
        public virtual Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams
        {
            get
            {
                if ((mSelect_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams == null))
                {
                    mSelect_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams = new Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams();
                }
                return mSelect_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams;
            }
        }
        public virtual Select_Letters_From_Random_Type_ComboboxParams Select_Letters_From_Random_Type_ComboboxParams
        {
            get
            {
                if ((mSelect_Letters_From_Random_Type_ComboboxParams == null))
                {
                    mSelect_Letters_From_Random_Type_ComboboxParams = new Select_Letters_From_Random_Type_ComboboxParams();
                }
                return mSelect_Letters_From_Random_Type_ComboboxParams;
            }
        }
        public virtual Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams
        {
            get
            {
                if ((mSelect_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams == null))
                {
                    mSelect_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams = new Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams();
                }
                return mSelect_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams;
            }
        }
        public virtual Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams
        {
            get
            {
                if ((mSelect_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams == null))
                {
                    mSelect_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams = new Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams();
                }
                return mSelect_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams;
            }
        }
        public virtual Select_NamespaceExpectedValues Select_NamespaceExpectedValues
        {
            get
            {
                if ((mSelect_NamespaceExpectedValues == null))
                {
                    mSelect_NamespaceExpectedValues = new Select_NamespaceExpectedValues();
                }
                return mSelect_NamespaceExpectedValues;
            }
        }
        public virtual Select_NewDatabaseSource_FromSqlServerToolParams Select_NewDatabaseSource_FromSqlServerToolParams
        {
            get
            {
                if ((mSelect_NewDatabaseSource_FromSqlServerToolParams == null))
                {
                    mSelect_NewDatabaseSource_FromSqlServerToolParams = new Select_NewDatabaseSource_FromSqlServerToolParams();
                }
                return mSelect_NewDatabaseSource_FromSqlServerToolParams;
            }
        }
        public virtual Select_NewEmailSource_FromExplorerContextMenuParams Select_NewEmailSource_FromExplorerContextMenuParams
        {
            get
            {
                if ((mSelect_NewEmailSource_FromExplorerContextMenuParams == null))
                {
                    mSelect_NewEmailSource_FromExplorerContextMenuParams = new Select_NewEmailSource_FromExplorerContextMenuParams();
                }
                return mSelect_NewEmailSource_FromExplorerContextMenuParams;
            }
        }
        public virtual Select_NewSharepointSource_FromServer_LookupParams Select_NewSharepointSource_FromServer_LookupParams
        {
            get
            {
                if ((mSelect_NewSharepointSource_FromServer_LookupParams == null))
                {
                    mSelect_NewSharepointSource_FromServer_LookupParams = new Select_NewSharepointSource_FromServer_LookupParams();
                }
                return mSelect_NewSharepointSource_FromServer_LookupParams;
            }
        }
        public virtual Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams
        {
            get
            {
                if ((mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams == null))
                {
                    mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams = new Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams();
                }
                return mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams;
            }
        }
        public virtual Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams
        {
            get
            {
                if ((mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams == null))
                {
                    mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams = new Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams();
                }
                return mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams;
            }
        }
        public virtual Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams
        {
            get
            {
                if ((mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams == null))
                {
                    mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams = new Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams();
                }
                return mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams;
            }
        }
        public virtual Select_NewWorkFlowService_From_ContextMenuParams Select_NewWorkFlowService_From_ContextMenuParams
        {
            get
            {
                if ((mSelect_NewWorkFlowService_From_ContextMenuParams == null))
                {
                    mSelect_NewWorkFlowService_From_ContextMenuParams = new Select_NewWorkFlowService_From_ContextMenuParams();
                }
                return mSelect_NewWorkFlowService_From_ContextMenuParams;
            }
        }
        public virtual Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues == null))
                {
                    mSelect_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues = new Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues();
                }
                return mSelect_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
            }
        }
        public virtual Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams
        {
            get
            {
                if ((mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams == null))
                {
                    mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams = new Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams();
                }
                return mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams;
            }
        }
        public virtual Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams
        {
            get
            {
                if ((mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams == null))
                {
                    mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams = new Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams();
                }
                return mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams;
            }
        }
        public virtual Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams
        {
            get
            {
                if ((mSelect_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams == null))
                {
                    mSelect_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams = new Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams();
                }
                return mSelect_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams;
            }
        }
        public virtual Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues
        {
            get
            {
                if ((mSelect_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues == null))
                {
                    mSelect_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues = new Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues();
                }
                return mSelect_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues;
            }
        }
        public virtual Select_SharepointTestServer_From_SharepointRead_ToolParams Select_SharepointTestServer_From_SharepointRead_ToolParams
        {
            get
            {
                if ((mSelect_SharepointTestServer_From_SharepointRead_ToolParams == null))
                {
                    mSelect_SharepointTestServer_From_SharepointRead_ToolParams = new Select_SharepointTestServer_From_SharepointRead_ToolParams();
                }
                return mSelect_SharepointTestServer_From_SharepointRead_ToolParams;
            }
        }
        public virtual Select_SharepointTestServer_From_SharepointUpdate_ToolParams Select_SharepointTestServer_From_SharepointUpdate_ToolParams
        {
            get
            {
                if ((mSelect_SharepointTestServer_From_SharepointUpdate_ToolParams == null))
                {
                    mSelect_SharepointTestServer_From_SharepointUpdate_ToolParams = new Select_SharepointTestServer_From_SharepointUpdate_ToolParams();
                }
                return mSelect_SharepointTestServer_From_SharepointUpdate_ToolParams;
            }
        }
        public virtual Select_Source_From_DotnetToolParams Select_Source_From_DotnetToolParams
        {
            get
            {
                if ((mSelect_Source_From_DotnetToolParams == null))
                {
                    mSelect_Source_From_DotnetToolParams = new Select_Source_From_DotnetToolParams();
                }
                return mSelect_Source_From_DotnetToolParams;
            }
        }
        public virtual Select_Source_From_PostgreToolParams Select_Source_From_PostgreToolParams
        {
            get
            {
                if ((mSelect_Source_From_PostgreToolParams == null))
                {
                    mSelect_Source_From_PostgreToolParams = new Select_Source_From_PostgreToolParams();
                }
                return mSelect_Source_From_PostgreToolParams;
            }
        }
        public virtual Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues == null))
                {
                    mSelect_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues = new Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues();
                }
                return mSelect_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues;
            }
        }
        public virtual Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues == null))
                {
                    mSelect_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues = new Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues();
                }
                return mSelect_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues;
            }
        }
        public virtual Select_Tests_From_Context_MenuParams Select_Tests_From_Context_MenuParams
        {
            get
            {
                if ((mSelect_Tests_From_Context_MenuParams == null))
                {
                    mSelect_Tests_From_Context_MenuParams = new Select_Tests_From_Context_MenuParams();
                }
                return mSelect_Tests_From_Context_MenuParams;
            }
        }
        public virtual Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
        {
            get
            {
                if ((mSelect_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues == null))
                {
                    mSelect_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues = new Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues();
                }
                return mSelect_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
            }
        }
        public virtual Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams
        {
            get
            {
                if ((mSelect_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams == null))
                {
                    mSelect_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams = new Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams();
                }
                return mSelect_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams;
            }
        }
        public virtual Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams
        {
            get
            {
                if ((mSelect_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams == null))
                {
                    mSelect_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams = new Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams();
                }
                return mSelect_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams;
            }
        }
        public virtual Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams
        {
            get
            {
                if ((mSelect_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams == null))
                {
                    mSelect_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams = new Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams();
                }
                return mSelect_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams;
            }
        }
        public virtual Select_User_From_RunTestAsParams Select_User_From_RunTestAsParams
        {
            get
            {
                if ((mSelect_User_From_RunTestAsParams == null))
                {
                    mSelect_User_From_RunTestAsParams = new Select_User_From_RunTestAsParams();
                }
                return mSelect_User_From_RunTestAsParams;
            }
        }
        public virtual Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues
        {
            get
            {
                if ((mType_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues == null))
                {
                    mType_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues = new Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues();
                }
                return mType_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues;
            }
        }
        public virtual Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues
        {
            get
            {
                if ((mType_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues == null))
                {
                    mType_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues = new Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues();
                }
                return mType_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues;
            }
        }
        #endregion
        #region Fields
        private Click_RunDebug_buttonParams mClick_RunDebug_buttonParams;
        private Click_Save_Ribbon_Button_to_Open_Save_DialogParams mClick_Save_Ribbon_Button_to_Open_Save_DialogParams;
        private Click_SQL_Server_Large_View_Generate_OutputsExpectedValues mClick_SQL_Server_Large_View_Generate_OutputsExpectedValues;
        private Click_Unpinned_Workflow_CollapseAllParams mClick_Unpinned_Workflow_CollapseAllParams;
        private Click_Unpinned_Workflow_ExpandAllParams mClick_Unpinned_Workflow_ExpandAllParams;
        private Click_Variable_IsInputParams mClick_Variable_IsInputParams;
        private Drag_Toolbox_Unzip_Onto_DesignSurfaceParams mDrag_Toolbox_Unzip_Onto_DesignSurfaceParams;
        private Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams mDrag_Toolbox_Web_Request_Onto_DesignSurfaceParams;
        private Drag_Toolbox_Write_File_Onto_DesignSurfaceParams mDrag_Toolbox_Write_File_Onto_DesignSurfaceParams;
        private Drag_Toolbox_XPath_Onto_DesignSurfaceParams mDrag_Toolbox_XPath_Onto_DesignSurfaceParams;
        private Drag_Toolbox_Zip_Onto_DesignSurfaceParams mDrag_Toolbox_Zip_Onto_DesignSurfaceParams;
        private Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams mDuplicate_Explorer_Localhost_First_Item_With_Context_MenuParams;
        private Enter_DeployViewOnly_Into_Deploy_Source_FilterParams mEnter_DeployViewOnly_Into_Deploy_Source_FilterParams;
        private Enter_Duplicate_workflow_nameParams mEnter_Duplicate_workflow_nameParams;
        private Enter_InputDebug_valueParams mEnter_InputDebug_valueParams;
        private Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams mEnter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams;
        private Enter_Public_As_Windows_GroupParams mEnter_Public_As_Windows_GroupParams;
        private Enter_RunAsUser_Username_And_PasswordParams mEnter_RunAsUser_Username_And_PasswordParams;
        private Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams mEnter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams;
        private Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams mEnter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams;
        private Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams mEnter_Sharepoint_Server_Path_From_OnUpload_ToolParams;
        private Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams mEnter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams;
        private Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams mEnter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams;
        private Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams mEnter_Sharepoint_Server_Path_To_OnUpload_ToolParams;
        private Enter_Sharepoint_ServerSource_ServerNameParams mEnter_Sharepoint_ServerSource_ServerNameParams;
        private Enter_Sharepoint_ServerSource_User_CredentialsParams mEnter_Sharepoint_ServerSource_User_CredentialsParams;
        private Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams mEnter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams;
        private Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams mEnter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams;
        private Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams;
        private Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams;
        private Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams mEnter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams;
        private Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams;
        private Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams mEnter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams;
        private Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams mEnter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams;
        private Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams mEnter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams;
        private Enter_Vaiablelist_ItemsParams mEnter_Vaiablelist_ItemsParams;
        private Filter_variablesParams mFilter_variablesParams;
        private I_Open_Explorer_First_Item_Context_MenuParams mI_Open_Explorer_First_Item_Context_MenuParams;
        private Open_AggregateCalculate_Tool_large_viewParams mOpen_AggregateCalculate_Tool_large_viewParams;
        private Open_Assign_Tool_Large_ViewParams mOpen_Assign_Tool_Large_ViewParams;
        private Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams mOpen_Assign_Tool_On_Unpinned_Tab_Large_ViewParams;
        private Open_Assign_Tool_Qvi_Large_ViewParams mOpen_Assign_Tool_Qvi_Large_ViewParams;
        private Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams mOpen_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams;
        private Open_AssignObject_Large_ToolParams mOpen_AssignObject_Large_ToolParams;
        private Open_AssignObject_QVI_LargeViewParams mOpen_AssignObject_QVI_LargeViewParams;
        private Open_Base_Conversion_Tool_Large_ViewParams mOpen_Base_Conversion_Tool_Large_ViewParams;
        private Open_Base_Conversion_Tool_Qvi_Large_ViewParams mOpen_Base_Conversion_Tool_Qvi_Large_ViewParams;
        private Open_Calculate_Tool_Large_ViewParams mOpen_Calculate_Tool_Large_ViewParams;
        private Open_Case_Conversion_Tool_Large_ViewParams mOpen_Case_Conversion_Tool_Large_ViewParams;
        private Open_Case_Conversion_Tool_Qvi_Large_ViewParams mOpen_Case_Conversion_Tool_Qvi_Large_ViewParams;
        private Open_CMD_Line_Tool_Large_ViewParams mOpen_CMD_Line_Tool_Large_ViewParams;
        private Open_Copy_Tool_Large_ViewParams mOpen_Copy_Tool_Large_ViewParams;
        private Open_Create_Tool_Large_ViewParams mOpen_Create_Tool_Large_ViewParams;
        private Open_Data_Merge_Large_ViewParams mOpen_Data_Merge_Large_ViewParams;
        private Open_Data_Merge_Tool_Qvi_Large_ViewParams mOpen_Data_Merge_Tool_Qvi_Large_ViewParams;
        private Open_Data_Split_Large_ViewParams mOpen_Data_Split_Large_ViewParams;
        private Open_Data_Split_Tool_Qvi_Large_ViewParams mOpen_Data_Split_Tool_Qvi_Large_ViewParams;
        private Open_DateTime_LargeViewParams mOpen_DateTime_LargeViewParams;
        private Open_DateTimeDiff_LargeViewParams mOpen_DateTimeDiff_LargeViewParams;
        private Open_Decision_Large_ViewParams mOpen_Decision_Large_ViewParams;
        private Open_Delete_Tool_Large_ViewParams mOpen_Delete_Tool_Large_ViewParams;
        private Open_DeleteRecords_Large_ViewParams mOpen_DeleteRecords_Large_ViewParams;
        private Open_DeleteWeb_Tool_Large_ViewParams mOpen_DeleteWeb_Tool_Large_ViewParams;
        private Open_DotNet_DLL_Connector_Tool_Large_ViewParams mOpen_DotNet_DLL_Connector_Tool_Large_ViewParams;
        private Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams mOpen_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams;
        private Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams mOpen_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams;
        private Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams mOpen_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams;
        private Open_Explorer_First_Item_Tests_With_Context_MenuParams mOpen_Explorer_First_Item_Tests_With_Context_MenuParams;
        private Open_Explorer_First_Item_With_Context_MenuParams mOpen_Explorer_First_Item_With_Context_MenuParams;
        private Open_Find_Index_Tool_Large_ViewParams mOpen_Find_Index_Tool_Large_ViewParams;
        private Open_ForEach_Large_ViewParams mOpen_ForEach_Large_ViewParams;
        private Open_GET_Web_Connector_Tool_Large_ViewParams mOpen_GET_Web_Connector_Tool_Large_ViewParams;
        private Open_Javascript_Large_ViewParams mOpen_Javascript_Large_ViewParams;
        private Open_Json_Tool_Large_ViewParams mOpen_Json_Tool_Large_ViewParams;
        private Open_Json_Tool_Qvi_Large_ViewParams mOpen_Json_Tool_Qvi_Large_ViewParams;
        private Open_Length_Tool_Large_ViewParams mOpen_Length_Tool_Large_ViewParams;
        private Open_Move_Tool_Large_ViewParams mOpen_Move_Tool_Large_ViewParams;
        private Open_NumberFormat_Toolbox_Large_ViewParams mOpen_NumberFormat_Toolbox_Large_ViewParams;
        private Open_ODBC_Tool_Large_ViewExpectedValues mOpen_ODBC_Tool_Large_ViewExpectedValues;
        private Open_Oracle_Tool_Large_ViewExpectedValues mOpen_Oracle_Tool_Large_ViewExpectedValues;
        private Open_Postgre_Tool_Large_ViewExpectedValues mOpen_Postgre_Tool_Large_ViewExpectedValues;
        private Open_PostWeb_RequestTool_Large_ViewParams mOpen_PostWeb_RequestTool_Large_ViewParams;
        private Open_Python_Large_ViewParams mOpen_Python_Large_ViewParams;
        private Open_RabbitMqConsume_LargeViewParams mOpen_RabbitMqConsume_LargeViewParams;
        private Open_RabbitMqPublish_LargeViewParams mOpen_RabbitMqPublish_LargeViewParams;
        private Open_Random_Large_ToolParams mOpen_Random_Large_ToolParams;
        private Open_Read_File_Tool_Large_ViewParams mOpen_Read_File_Tool_Large_ViewParams;
        private Open_Read_Folder_Tool_Large_ViewParams mOpen_Read_Folder_Tool_Large_ViewParams;
        private Open_Rename_Tool_Large_ViewParams mOpen_Rename_Tool_Large_ViewParams;
        private Open_Replace_Tool_Large_ViewParams mOpen_Replace_Tool_Large_ViewParams;
        private Open_Ruby_Large_ViewParams mOpen_Ruby_Large_ViewParams;
        private Open_Selectandapply_Large_ViewParams mOpen_Selectandapply_Large_ViewParams;
        private Open_Sequence_Large_tool_ViewParams mOpen_Sequence_Large_tool_ViewParams;
        private Open_SMTP_Email_Tool_Large_ViewParams mOpen_SMTP_Email_Tool_Large_ViewParams;
        private Open_SQL_Bulk_Insert_Tool_Large_ViewParams mOpen_SQL_Bulk_Insert_Tool_Large_ViewParams;
        private Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams mOpen_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams;
        private Open_SQL_Large_View_FromContextMenuParams mOpen_SQL_Large_View_FromContextMenuParams;
        private Open_Sql_Server_Tool_Large_ViewExpectedValues mOpen_Sql_Server_Tool_Large_ViewExpectedValues;
        private Open_Switch_Tool_Large_ViewParams mOpen_Switch_Tool_Large_ViewParams;
        private Open_System_Information_Tool_Large_ViewParams mOpen_System_Information_Tool_Large_ViewParams;
        private Open_System_Information_Tool_Qvi_Large_ViewParams mOpen_System_Information_Tool_Qvi_Large_ViewParams;
        private Open_Unzip_Tool_Large_ViewParams mOpen_Unzip_Tool_Large_ViewParams;
        private Open_WebRequest_LargeViewParams mOpen_WebRequest_LargeViewParams;
        private Open_Write_File_Tool_Large_ViewParams mOpen_Write_File_Tool_Large_ViewParams;
        private Open_Xpath_Tool_Large_ViewParams mOpen_Xpath_Tool_Large_ViewParams;
        private Open_Xpath_Tool_Qvi_Large_ViewParams mOpen_Xpath_Tool_Qvi_Large_ViewParams;
        private Open_Zip_Tool_Large_ViewParams mOpen_Zip_Tool_Large_ViewParams;
        private Press_F6Params mPress_F6Params;
        private PressF11_EnterFullScreenParams mPressF11_EnterFullScreenParams;
        private RabbitMqAssertsExpectedValues mRabbitMqAssertsExpectedValues;
        private Remove_WorkflowName_From_Save_DialogParams mRemove_WorkflowName_From_Save_DialogParams;
        private Rename_FolderItem_ToNewFolderItemParams mRename_FolderItem_ToNewFolderItemParams;
        private Rename_LocalFolder_To_SecondFolderParams mRename_LocalFolder_To_SecondFolderParams;
        private Rename_LocalWorkflow_To_SecodWorkFlowParams mRename_LocalWorkflow_To_SecodWorkFlowParams;
        private Restore_Unpinned_Tab_Using_Context_MenuExpectedValues mRestore_Unpinned_Tab_Using_Context_MenuExpectedValues;
        private RightClick_Explorer_Localhost_First_ItemParams mRightClick_Explorer_Localhost_First_ItemParams;
        private Search_And_Select_DiceRollParams mSearch_And_Select_DiceRollParams;
        private Search_And_Select_HelloWolrdParams mSearch_And_Select_HelloWolrdParams;
        private Select_Action_From_PostgreToolParams mSelect_Action_From_PostgreToolParams;
        private Select_DatabaseAndTable_From_BulkInsert_ToolParams mSelect_DatabaseAndTable_From_BulkInsert_ToolParams;
        private Select_Delete_FromExplorerContextMenuParams mSelect_Delete_FromExplorerContextMenuParams;
        private Select_Deploy_FromExplorerContextMenuParams mSelect_Deploy_FromExplorerContextMenuParams;
        private Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams mSelect_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams;
        private Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues mSelect_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
        private Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues mSelect_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues;
        private Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams mSelect_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams;
        private Select_Letters_From_Random_Type_ComboboxParams mSelect_Letters_From_Random_Type_ComboboxParams;
        private Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams mSelect_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams;
        private Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams mSelect_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams;
        private Select_NamespaceExpectedValues mSelect_NamespaceExpectedValues;
        private Select_NewDatabaseSource_FromSqlServerToolParams mSelect_NewDatabaseSource_FromSqlServerToolParams;
        private Select_NewEmailSource_FromExplorerContextMenuParams mSelect_NewEmailSource_FromExplorerContextMenuParams;
        private Select_NewSharepointSource_FromServer_LookupParams mSelect_NewSharepointSource_FromServer_LookupParams;
        private Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams;
        private Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams;
        private Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams mSelect_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams;
        private Select_NewWorkFlowService_From_ContextMenuParams mSelect_NewWorkFlowService_From_ContextMenuParams;
        private Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues mSelect_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
        private Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams;
        private Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams mSelect_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams;
        private Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams mSelect_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams;
        private Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues mSelect_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues;
        private Select_SharepointTestServer_From_SharepointRead_ToolParams mSelect_SharepointTestServer_From_SharepointRead_ToolParams;
        private Select_SharepointTestServer_From_SharepointUpdate_ToolParams mSelect_SharepointTestServer_From_SharepointUpdate_ToolParams;
        private Select_Source_From_DotnetToolParams mSelect_Source_From_DotnetToolParams;
        private Select_Source_From_PostgreToolParams mSelect_Source_From_PostgreToolParams;
        private Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues mSelect_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues;
        private Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues mSelect_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues;
        private Select_Tests_From_Context_MenuParams mSelect_Tests_From_Context_MenuParams;
        private Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues mSelect_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues;
        private Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams mSelect_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams;
        private Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams mSelect_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams;
        private Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams mSelect_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams;
        private Select_User_From_RunTestAsParams mSelect_User_From_RunTestAsParams;
        private Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues mType_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues;
        private Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues mType_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_RunDebug_buttonParams
    {
        #region Fields
        public bool DebugInputDialogExists = true;
        public bool CancelButtonEnabled = true;
        public bool RememberDebugInputCheckBoxEnabled = true;
        public bool InputDataTabEnabled = true;
        public bool RememberDebugInputCheckBoxChecked = true;
        public bool XMLTabExists = true;
        public bool JSONTabExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_Save_Ribbon_Button_to_Open_Save_DialogParams
    {
        #region Fields

        public bool ErrorLabelExists = true;
        public bool ExplorerTreeExists = true;
        public bool ExplorerViewExists = true;
        public bool SearchTextBoxExists = true;
        public bool NameLabelExists = true;
        public bool RefreshButtonExists = true;
        public bool SaveButtonExists1 = true;
        public bool SaveDialogWindowExists = true;
        public bool ServiceNameTextBoxExists = true;
        public bool SaveDialogWindowExists1 = true;
        public bool ServiceNameTextBoxExists1 = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_SQL_Server_Large_View_Generate_OutputsExpectedValues
    {
        #region Fields

        public bool TestDataTextboxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_Unpinned_Workflow_CollapseAllParams
    {
        #region Fields

        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_Unpinned_Workflow_ExpandAllParams
    {
        #region Fields

        public bool ExpandAllToggleButtonPressed = true;

        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Click_Variable_IsInputParams
    {
        #region Fields

        public bool InputCheckboxChecked = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class DisplayStartNodeContextMenuExpectedValues
    {
        #region Fields
        public bool DebugInputsMenuItemExists = true;
        public bool DebugStudioMenuItemExists = true;
        public bool DebugBrowserMenuItemExists = true;
        public bool ScheduleMenuItemExists = true;
        public bool TestEditorMenuItemExists = true;
        public bool TestEditorMenuItemEnabled = false;
        public bool DebugInputsMenuItemEnabled = true;
        public bool DebugStudioMenuItemEnabled = true;
        public bool DebugBrowserMenuItemEnabled = true;
        public bool ScheduleMenuItemEnabled = false;
        public bool RunAllTestsMenuItemExists = true;
        public bool RunAllTestsMenuItemEnabled = false;
        public bool DuplicateMenuItemEnabled = false;
        public bool DuplicateMenuItemExists = true;
        public bool DeployMenuItemExists = true;
        public bool DeployMenuItemEnabled = false;
        public bool ShowDependenciesMenuItemEnabled = false;
        public bool ShowDependenciesMenuItemExists = true;
        public bool ViewSwaggerMenuItemExists = true;
        public bool ViewSwaggerMenuItemEnabled = false;
        public bool CopyURLtoClipboardMenuItemEnabled = false;
        public bool CopyURLtoClipboardMenuItemExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_Unzip_Onto_DesignSurfaceParams
    {
        #region Fields
        public string SearchTextBoxText = "Unzip";
        public bool Connector1Exists = true;
        public bool UnZipExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_Web_Request_Onto_DesignSurfaceParams
    {
        #region Fields
        public string SearchTextBoxText = "Web Request";
        public bool Connector1Exists = true;
        public bool WebRequestExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_Write_File_Onto_DesignSurfaceParams
    {
        #region Fields
        public string SearchTextBoxText = "Write File";
        public bool Connector1Exists = true;
        public bool FileWriteExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_XPath_Onto_DesignSurfaceParams
    {
        #region Fields
        public string SearchTextBoxText = "XPath";
        public bool Connector1Exists = true;
        public bool XPathExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Toolbox_Zip_Onto_DesignSurfaceParams
    {
        #region Fields
        public string SearchTextBoxText = "Zip";
        public bool Connector1Exists = true;
        public bool ZipExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Duplicate_Explorer_Localhost_First_Item_With_Context_MenuParams
    {
        #region Fields
        public bool DuplicateExists = true;
        public bool SaveDialogWindowExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_DeployViewOnly_Into_Deploy_Source_FilterParams
    {
        #region Fields
        public string SearchTextboxText = "DeployViewOnly";
        public bool FirstExplorerTreeItemExists = true;
        public bool CheckBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Duplicate_workflow_nameParams
    {
        #region Fields
        public string ServiceNameTextBoxText = "DuplicatedWorkFlow";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_InputDebug_valueParams
    {
        #region Fields
        public bool Row1Exists = true;
        public bool InputValueTextExists = true;
        public string InputValueTextText = "100";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_LocalSchedulerAdmin_Credentials_Into_Scheduler_TabParams
    {
        #region Fields
        public string UserNameTextBoxEditText = "LocalSchedulerAdmin";
        public string PasswordTextboxText = "987Sched#@!";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Public_As_Windows_GroupParams
    {
        #region Fields
        public string AddWindowsGroupsEditText = "Public";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_RunAsUser_Username_And_PasswordParams
    {
        #region Fields
        public string UsernameTextBoxEditText = "testuser";
        public string UsernameTextBoxEditSendKeys = "{Tab}";
        public string PasswordTextBoxEditSendKeys = "a1cbgHEVu098QBN0jqs55wYP/bLfpGNMxw2YxtLIgKOALxPfITSBDjNERdIi/KEq";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_From_OnCopyFile_ToolParams
    {
        #region Fields
        public string TextEditText = "clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_From_OnMoveFile_ToolParams
    {
        #region Fields
        public string TextEditText = "clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_From_OnUpload_ToolParams
    {
        #region Fields
        public string TextboxText = "clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_To_OnCopyFile_ToolParams
    {
        #region Fields
        public string TextEditText = "TestFolder/clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_To_OnMoveFile_ToolParams
    {
        #region Fields
        public string TextEditText = "TestFolder/clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_Server_Path_To_OnUpload_ToolParams
    {
        #region Fields
        public string TextboxText = "TestFolder/clocks.dat";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_ServerSource_ServerNameParams
    {
        #region Fields
        public string ServerNameEditText = "http://rsaklfsvrsharep/";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Sharepoint_ServerSource_User_CredentialsParams
    {
        #region Fields
        public string UserNameTextBoxText = "Bernartdt@dvtdev.onmicrosoft.com";
        public string PasswordTextBoxSendKeys = "YN/mQM5J9PSwtnVGttwUbqV2NkA27Xtb2Cs5ppSS77kjZgxPPM79nWlqEFRqmwY4KvuSBKnsLDU6spVwV" +
            "rcWKXwSuKb7vBXD";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_SomeData_Into_Base_Convert_Large_View_Row1_Value_TextboxParams
    {
        #region Fields
        public string ValueTextboxText = "SomeData";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_SomeVariable_Into_Calculate_Large_View_Function_TextboxParams
    {
        #region Fields
        public string FunctionTextboxText = "[[SomeVariable]]";
        public string FunctionTextboxText1 = "[[SomeVariable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableNameParams
    {
        #region Fields
        public string TextboxText = "[[Some$Invalid%Variable]]";
        public string TextboxText1 = "[[Some$Invalid%Variable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariableParams
    {
        #region Fields
        public string TextboxSendKeys = "{Home}";
        public string TextboxSendKeys1 = "[[Some{Down}{Enter}Variable]]";
        public string TextboxText = "[[SomeVariable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_TabParams
    {
        #region Fields
        public string TextboxSendKeys = "[[Some{Down}{Enter}Variable]]";
        public string TextboxText = "[[SomeVariable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_SuggestionParams
    {
        #region Fields
        public string TextboxText = "[[";
        public string TextboxText1 = "[[SomeVariable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisenseParams
    {
        #region Fields
        public string TextboxSendKeys = "[[{Down}{Enter}";
        public string TextboxText = "[[SomeVariable]]";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITestParams
    {
        #region Fields
        public string TextboxSendKeys = "Helo User";
        public string TextboxText = "Hello User";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITestParams
    {
        #region Fields
        public string TextboxSendKeys = "User";
        public string TextboxText = "User";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Enter_Vaiablelist_ItemsParams
    {
        #region Fields
        public string NameTextboxText = "varableA";
        public string NameTextboxSendKeys = "{CapsLock}";
        public string NameTextboxSendKeys1 = "{CapsLock}";
        public string NameTextboxText1 = "variableB";
        public string NameTextboxText2 = "VariableC";
        public string NameTextboxSendKeys2 = "{CapsLock}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Filter_variablesParams
    {
        #region Fields
        public bool FilterTextExists = true;
        public string SearchTextboxText = "Other";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class I_Open_Explorer_First_Item_Context_MenuParams
    {
        #region Fields
        public bool ShowVersionHistoryExists = true;
        public bool ViewSwaggerExists = true;
        public bool ViewSwaggerEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_AggregateCalculate_Tool_large_viewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool OnErrorGroupExists = true;
        public bool ResultComboBoxExists = true;
        public bool fxComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Assign_Tool_Large_ViewParams
    {
        #region Fields
        public bool MultiAssignExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Assign_Tool_On_Unpinned_Tab_Large_ViewParams
    {
        #region Fields
        public bool MultiAssignExists = true;
        public bool IntellisenseComboboxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Assign_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Assign_Tool_Qvi_Large_View_On_Unpinned_TabParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_AssignObject_Large_ToolParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool OpenQuickVariableInputExists = true;
        public bool Row1Exists = true;
        public bool OnErrorGroupExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_AssignObject_QVI_LargeViewParams
    {
        #region Fields
        public bool OpenQuickVariableInputPressed = true;
        public bool QuickVariableInputContentExists = true;
        public bool QviSplitOnComboboxExists = true;
        public bool PreviewCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Base_Conversion_Tool_Large_ViewParams
    {
        #region Fields
        public bool ValueTextboxEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Base_Conversion_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool BaseConvertExists = true;
        public bool QuickVariableInputContentExists = true;
        public bool QuickVariableInputContentExists1 = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Calculate_Tool_Large_ViewParams
    {
        #region Fields
        public bool LargeViewExists = true;
        public bool ListboxExists = true;
        public bool FunctionTextboxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Case_Conversion_Tool_Large_ViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool SmallDataGridTableExists = true;
        public bool OnErrorCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Case_Conversion_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool CaseConvertExists = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_CMD_Line_Tool_Large_ViewParams
    {
        #region Fields
        public bool ExecuteCommandLineExists = true;
        public bool ScriptIntellisenseTextboxExists = true;
        public bool PriorityComboBoxExists = true;
        public bool ResultIntellisenseTextboxExists = true;
        public bool OnErrorExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Copy_Tool_Large_ViewParams
    {
        #region Fields
        public bool PathCopyExists = true;
        public bool DoneButtonExists = true;
        public bool FileOrFolderComboBoxExists = true;
        public bool DestinationComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool OverwriteCheckBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Create_Tool_Large_ViewParams
    {
        #region Fields
        public bool PathCreateExists = true;
        public bool FileNameoComboBoxExists = true;
        public bool OverwriteCheckBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Data_Merge_Large_ViewParams
    {
        #region Fields
        public bool DataMergeExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Data_Merge_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool DataMergeExists = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Data_Split_Large_ViewParams
    {
        #region Fields
        public bool DataSplitExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Data_Split_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool DataSplitExists = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_DateTime_LargeViewParams
    {
        #region Fields
        public bool AddTimeAmountComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool InputComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_DateTimeDiff_LargeViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool InputFormatComboBoxExists = true;
        public bool Input1ComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Decision_Large_ViewParams
    {
        #region Fields
        public bool DecisionOrSwitchDialogExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Delete_Tool_Large_ViewParams
    {
        #region Fields
        public bool PathDeleteExists = true;
        public bool FileNameComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_DeleteRecords_Large_ViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool OnErrorGroupExists = true;
        public bool ResultComboBoxExists = true;
        public bool RecordsetComboBoxExists = true;
        public bool NullAsZeroCheckBoxCheckBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_DeleteWeb_Tool_Large_ViewParams
    {
        #region Fields
        public bool LargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_DotNet_DLL_Connector_Tool_Large_ViewParams
    {
        #region Fields
        public bool DotNetDllExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Dropbox_Delete_Tool_Large_View_With_Double_ClickParams
    {
        #region Fields
        public bool LargeViewContentCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Dropbox_List_Contents_Tool_Large_View_With_Double_ClickParams
    {
        #region Fields
        public bool LargeViewContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Dropbox_Upload_Tool_Large_View_With_Double_ClickParams
    {
        #region Fields
        public bool LargeViewContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Explorer_First_Item_Tests_With_Context_MenuParams
    {
        #region Fields
        public bool TestsExists = true;
        public bool RunAllButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Explorer_First_Item_With_Context_MenuParams
    {
        #region Fields
        public bool OpenExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Find_Index_Tool_Large_ViewParams
    {
        #region Fields
        public bool ResultComboBoxExists = true;
        public bool InFieldComboBoxExists = true;
        public bool IndexComboBoxExists = true;
        public bool CharactersComboBoxExists = true;
        public bool DirectionComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_ForEach_Large_ViewParams
    {
        #region Fields
        public bool TypeComboboxExists = true;
        public bool ForEachFromIntellisenseTextboxExists = true;
        public bool ToIntellisenseTextboxExists = true;
        public bool DropActivityHereExists = true;
        public bool OnErrorPaneExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_GET_Web_Connector_Tool_Large_ViewParams
    {
        #region Fields
        public bool SourcesComboBoxExists = true;
        public bool GenerateOutputsButtonExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Javascript_Large_ViewParams
    {
        #region Fields
        public bool ScriptIntellisenseComboboxExists = true;
        public bool AttachmentsIntellisenseComboboxExists = true;
        public bool AttachFileButtonExists = true;
        public bool EscapesequencesCheckBoxExists = true;
        public bool ResultIntellisenseComboboxExists = true;
        public bool OnErrorPaneExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Json_Tool_Large_ViewParams
    {
        #region Fields
        public bool CreateJsonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Json_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool CreateJsonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Length_Tool_Large_ViewParams
    {
        #region Fields
        public bool RecordsetComboBoxExists = true;
        public bool ResultComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool DoneButtonExists = true;
        public bool NullAsZeroCheckBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Move_Tool_Large_ViewParams
    {
        #region Fields
        public bool PathMoveExists = true;
        public bool OnErrorCustomExists = true;
        public bool DestinationComboBoxExists = true;
        public bool DoneButtonExists = true;
        public bool OverwriteCheckBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_NumberFormat_Toolbox_Large_ViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool OnErrorGroupExists = true;
        public bool ResultInputComboBoxExists = true;
        public bool DecimalsToShowComboBoxExists = true;
        public bool RoundingComboBoxExists = true;
        public bool NumberInputComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_ODBC_Tool_Large_ViewExpectedValues
    {
        #region Fields
        public bool LargeViewContentCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Oracle_Tool_Large_ViewExpectedValues
    {
        #region Fields
        public bool LargeViewContentCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Postgre_Tool_Large_ViewExpectedValues
    {
        #region Fields
        public bool LargeViewContentCustomExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_PostWeb_RequestTool_Large_ViewParams
    {
        #region Fields
        public bool LargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Python_Large_ViewParams
    {
        #region Fields
        public bool ScriptIntellisenseComboboxExists = true;
        public bool AttachmentsIntellisenseComboboxExists = true;
        public bool AttachFileButtonExists = true;
        public bool EscapesequencesCheckBoxExists = true;
        public bool ResultIntellisenseComboboxExists = true;
        public bool OnErrorPaneExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_RabbitMqConsume_LargeViewParams
    {
        #region Fields
        public bool ResponseComboBoxExists = true;
        public bool AcknowledgeCheckBoxExists = true;
        public bool SourceComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_RabbitMqPublish_LargeViewParams
    {
        #region Fields
        public bool DurableCheckBoxExists = true;
        public bool NewSourceButtonExists = true;
        public bool OnErrorCustomExists = true;
        public bool DoneButtonExists = true;
        public bool RabbitMQPublishExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Random_Large_ToolParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool OnErrorCustomExists = true;
        public bool FromComboBoxExists = true;
        public bool ToComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Read_File_Tool_Large_ViewParams
    {
        #region Fields
        public bool FileReadExists = true;
        public bool ResultComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool FileNameComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Read_Folder_Tool_Large_ViewParams
    {
        #region Fields
        public bool FolderReadExists = true;
        public bool FilesFoldersRadioButtonExists = true;
        public bool OnErrorCustomExists = true;
        public bool DirectoryComboBoxExists = true;
        public bool ResultComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Rename_Tool_Large_ViewParams
    {
        #region Fields
        public bool PathRenameExists = true;
        public bool OnErrorCustomExists = true;
        public bool OverwriteCheckBoxExists = true;
        public bool FileOrFolderComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Replace_Tool_Large_ViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool ResultComboBoxExists = true;
        public bool ReplaceComboBoxExists = true;
        public bool FindComboBoxExists = true;
        public bool InFiledsComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Ruby_Large_ViewParams
    {
        #region Fields
        public bool ScriptIntellisenseComboboxExists = true;
        public bool AttachmentsIntellisenseComboboxExists = true;
        public bool AttachFileButtonExists = true;
        public bool EscapesequencesCheckBoxExists = true;
        public bool ResultIntellisenseComboboxExists = true;
        public bool OnErrorPaneExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Selectandapply_Large_ViewParams
    {
        #region Fields
        public bool DoneButtonExists = true;
        public bool SelectFromIntellisenseTextboxExists = true;
        public bool AliasIntellisenseTextboxExists = true;
        public bool DropActivityHereExists = true;
        public bool OnErrorPaneExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Sequence_Large_tool_ViewParams
    {
        #region Fields
        public bool SequenceLargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_SMTP_Email_Tool_Large_ViewParams
    {
        #region Fields
        public bool LargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_SQL_Bulk_Insert_Tool_Large_ViewParams
    {
        #region Fields
        public bool SqlBulkInsertExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_SQL_Bulk_Insert_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool SqlBulkInsertExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_SQL_Large_View_FromContextMenuParams
    {
        #region Fields
        public bool NewDbSourceButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Sql_Server_Tool_Large_ViewExpectedValues
    {
        #region Fields
        public bool LargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Switch_Tool_Large_ViewParams
    {
        #region Fields
        public bool SwitchCaseDialogEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_System_Information_Tool_Large_ViewParams
    {
        #region Fields
        public bool SmallDataGridTableExists = true;
        public bool DoneButtonExists = true;
        public bool OnErrorGroupExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_System_Information_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool GatherSystemInfoExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Unzip_Tool_Large_ViewParams
    {
        #region Fields
        public bool UnZipExists = true;
        public bool OverwriteCheckBoxExists = true;
        public bool UnZipNameComboBoxExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_WebRequest_LargeViewParams
    {
        #region Fields
        public bool LargeViewExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Write_File_Tool_Large_ViewParams
    {
        #region Fields
        public bool FileWriteExists = true;
        public bool OnErrorCustomExists = true;
        public bool ContentsComboBoxExists = true;
        public bool OverwriteRadioButtonExists = true;
        public bool DoneButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Xpath_Tool_Large_ViewParams
    {
        #region Fields
        public bool XPathExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Xpath_Tool_Qvi_Large_ViewParams
    {
        #region Fields
        public bool OpenQuickVariableInpToggleButtonPressed = true;
        public bool XPathExists = true;
        public bool QuickVariableInputContentExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Open_Zip_Tool_Large_ViewParams
    {
        #region Fields
        public bool ZipExists = true;
        public bool SelectedCompressComboBoxExists = true;
        public bool OnErrorCustomExists = true;
        public bool OverwriteCheckBoxExists = true;
        public bool FileOrFolderComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Press_F6Params
    {
        #region Fields
        public string MainStudioWindowSendKeys = "{F6}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class PressF11_EnterFullScreenParams
    {
        #region Fields
        public string MainStudioWindowSendKeys = "{F11}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class RabbitMqAssertsExpectedValues
    {
        #region Fields
        public bool VirtualHostTextBoxEditExists = true;
        public bool PasswordTextBoxEditExists = true;
        public bool UserNameTextBoxEditExists = true;
        public bool HostTextBoxEditExists = true;
        public bool PortTextBoxEditExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Remove_WorkflowName_From_Save_DialogParams
    {
        #region Fields
        public string ServiceNameTextBoxText = "";
        public string ErrorLabelDisplayText = "Cannot be null";
        public bool SaveButtonEnabled = false;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Rename_FolderItem_ToNewFolderItemParams
    {
        #region Fields
        public string ItemEditText = "Control Flow - Decision2";
        public string ItemEditSendKeys = "{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Rename_LocalFolder_To_SecondFolderParams
    {
        #region Fields
        public string ItemEditText = "Example";
        public string ItemEditSendKeys = "{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Rename_LocalWorkflow_To_SecodWorkFlowParams
    {
        #region Fields
        public string ItemEditText = "SecondWorkflow";
        public string ItemEditSendKeys = "{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Restore_Unpinned_Tab_Using_Context_MenuExpectedValues
    {
        #region Fields
        public bool TabbedDocumentChecked = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class RightClick_Explorer_Localhost_First_ItemParams
    {
        #region Fields
        public bool OpenExists = true;
        public bool ShowDependenciesExists = true;
        public bool DeleteExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Search_And_Select_DiceRollParams
    {
        #region Fields
        public string SearchTextBoxText = "Dice Roll";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Search_And_Select_HelloWolrdParams
    {
        #region Fields
        public string SearchTextBoxText = "Hello World";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Action_From_PostgreToolParams
    {
        #region Fields
        public bool LargeDataGridTableEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_DatabaseAndTable_From_BulkInsert_ToolParams
    {
        #region Fields
        public bool TableNameComboBoxEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Delete_FromExplorerContextMenuParams
    {
        #region Fields
        public bool YesButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Deploy_FromExplorerContextMenuParams
    {
        #region Fields
        public bool DeployTabExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Dev2TestingDB_From_DB_Source_Wizard_Database_ComboboxParams
    {
        #region Fields
        public string UIDev2TestingDBTextDisplayText = "Dev2TestingDB";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_FirstItem_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
    {
        #region Fields
        public string ActionsComboBoxSelectedItem = "Dev2.Common.Interfaces.PluginAction";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_GetCountries_From_SQL_Server_Large_View_Action_ComboboxExpectedValues
    {
        #region Fields
        public string ActionsComboboxSelectedItem = "dbo.GetCountries";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_http_From_Server_Source_Wizard_Address_Protocol_DropdownParams
    {
        #region Fields
        public bool ComboboxListItemAsHttpExists = true;
        public string HttpSelectedItemTextDisplayText = "http";
        public bool AddressEditBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Letters_From_Random_Type_ComboboxParams
    {
        #region Fields
        public bool LengthComboBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_ComboboxParams
    {
        #region Fields
        public bool ComboboxListItemAsNewRemoteServerExists = true;
        public bool ComboboxListItemAsLocalhostConnectedExists = true;
        public string RemoteConnectionIntegrationTextDisplayText = "Remote Connection Integration";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_MSSQLSERVER_From_DB_Source_Wizard_Address_Protocol_DropdownParams
    {
        #region Fields
        public bool MicrosoftSQLServerTextExists = true;
        public string MicrosoftSQLServerDisplayText = "Microsoft SQL Server";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NamespaceExpectedValues
    {
        #region Fields
        public bool ComboboxlistItemAsSystemObjectExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewDatabaseSource_FromSqlServerToolParams
    {
        #region Fields
        public string MicrosoftSQLServerDisplayText = "Microsoft SQL Server";
        public bool UserNameTextBoxExists = true;
        public bool PasswordTextBoxExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewEmailSource_FromExplorerContextMenuParams
    {
        #region Fields
        public bool ExplorerEnvironmentContextMenuExists = true;
        public bool HostTextBoxEditExists = true;
        public bool UserNameTextBoxEditExists = true;
        public bool PasswordTextBoxEditExists = true;
        public bool PortTextBoxEditExists = true;
        public bool TimeoutTextBoxEditExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewSharepointSource_FromServer_LookupParams
    {
        #region Fields
        public string ServerSendKeys = "{Down}{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_ToolParams
    {
        #region Fields
        public string ServerSendKeys = "{Down}{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_ToolParams
    {
        #region Fields
        public string ServerSendKeys = "{Down}{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_ToolParams
    {
        #region Fields
        public string SourceComboboxSendKeys = "{Down}{Enter}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewWorkFlowService_From_ContextMenuParams
    {
        #region Fields
        public bool NewWorkflowEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Next_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
    {
        #region Fields
        public string ActionsComboBoxSelectedItem = "Next";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_ComboboxParams
    {
        #region Fields
        public bool ComboboxListItemAsNewRemoteServerExists = true;
        public bool ComboboxListItemAsRemoteConnectionIntegrationExists = true;
        public string RemoteConnectionIntegrationTextDisplayText = "Remote Connection Integration";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_RemoteConnectionIntegration_From_Deploy_Tab_Source_Server_ComboboxParams
    {
        #region Fields
        public bool ComboboxListItemAsNewRemoteServerExists = true;
        public bool ComboboxListItemAsRemoteConnectionIntegrationExists = true;
        public string RemoteConnectionIntegrationTextDisplayText = "Remote Connection Integration";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_ComboboxParams
    {
        #region Fields
        public bool ComboboxListItemAsNewRemoteServerExists = true;
        public bool ComboboxListItemAsRemoteConnectionIntegrationConnectedExists = true;
        public string RemoteConnectionIntegrationTextDisplayText = "Remote Connection Integration";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_RSAKLFSVRGENDEV_From_Server_Source_Wizard_DropdownlistExpectedValues
    {
        #region Fields
        public string TextboxText = "RSAKLFSVRGENDEV";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_SharepointTestServer_From_SharepointRead_ToolParams
    {
        #region Fields
        public bool EditSourceButtonEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_SharepointTestServer_From_SharepointUpdate_ToolParams
    {
        #region Fields
        public bool EditSourceButtonEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Source_From_DotnetToolParams
    {
        #region Fields
        public bool ClassNameComboBoxEnabled = true;
        public bool ActionsComboBoxEnabled = true;
        public bool GenerateOutputsButtonEnabled = true;
        public bool Row1Enabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Source_From_PostgreToolParams
    {
        #region Fields
        public bool ActionsComboBoxEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues
    {
        #region Fields
        public bool ComboboxlistItemAsSystemObjectExists = true;
        public string ClassNameComboBoxSelectedItem = "{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Object\",\"MethodName\":nul" +
            "l}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_ComboboxExpectedValues
    {
        #region Fields
        public string ClassNameComboBoxSelectedItem = "{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Random\",\"MethodName\":nul" +
            "l}";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Tests_From_Context_MenuParams
    {
        #region Fields
        public bool TestsTabPageExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_ToString_From_DotNet_DLL_Large_View_Action_ComboboxExpectedValues
    {
        #region Fields
        public string ActionsComboBoxSelectedItem = "ToString";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_TSTCIREMOTE_From_Server_Source_Wizard_DropdownlistParams
    {
        #region Fields
        public string AddressEditBoxText = "TST-CI-REMOTE";
        public bool TestConnectionButtonExists = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_UITestingDBSource_From_SQL_Server_Large_View_Source_ComboboxParams
    {
        #region Fields
        public string SourcesComboboxSelectedItem = "UITestingDBSource";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_UITestingSource_From_Web_Server_Large_View_Source_ComboboxParams
    {
        #region Fields
        public bool EditSourceButtonEnabled = true;
        public bool GenerateOutputsButtonEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_User_From_RunTestAsParams
    {
        #region Fields
        public bool UserRadioButtonSelected = true;
        public bool UsernameTextBoxEditExists = true;
        public bool PasswordTextBoxEditExists = true;
        public bool SaveButtonEnabled = true;
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_TextboxExpectedValues
    {
        #region Fields
        public string DataTextboxText = "0";
        public string DataTextboxText1 = "0";
        #endregion
    }
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_TextboxExpectedValues
    {
        #region Fields
        public string TestDataTextboxText = "0";
        public string TestDataTextboxText1 = "0";
        #endregion
    }
}
