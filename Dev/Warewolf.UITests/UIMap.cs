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
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using Warewolf.UITests.Common;

namespace Warewolf.UITests
{
    public partial class UIMap
    {
        const int _lenientSearchTimeout = 9000;
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
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.SkipSetPropertyVerification = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackError -= OnError;
            Playback.PlaybackError += OnError;
        }

        public void CloseHangingDialogs()
        {
            Assert.IsTrue(MainStudioWindow.Exists, "Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
            TryClickMessageBoxOK();
            TryCloseHangingDebugInputDialog();
            TryCloseHangingSaveDialog();
            TryCloseHangingServicePickerDialog();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        private void TryCloseHangingServicePickerDialog()
        {
            try
            {
                if (ControlExistsNow(ServicePickerDialog.Cancel))
                {
                    Click_Service_Picker_Dialog_Cancel();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("No hanging service picker dialog to clean up.\n" + e.Message);
            }
        }

        public void OnError(object sender, PlaybackErrorEventArgs e)
        {
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
                while (parent != null && !parent.Exists)
                {
                    parent = parent.Container;
                }
                if (parent != null && parent.Exists && parent != MainStudioWindow)
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
            Playback.Wait(Playback.PlaybackSettings.SearchTimeout);
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
            Playback.Wait(Playback.PlaybackSettings.SearchTimeout);
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
            Playback.Wait(Playback.PlaybackSettings.SearchTimeout);
        }

        public bool ControlExistsNow(UITestControl thisControl)
        {
            Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackError -= OnError;
            bool controlExists = false;
            try
            {
                controlExists = thisControl.Exists;
            }
            finally
            {
                Playback.PlaybackError += OnError;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            }
            return controlExists;
        }

        private void TryClickMessageBoxOK()
        {
            if (ControlExistsNow(MessageBoxWindow.OKButton))
            {
                Click_MessageBox_OK();
            }
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

        public void TryClearToolboxFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text != string.Empty)
            {
                Click_Clear_Toolbox_Filter_Clear_Button();
                Click_Clear_Toolbox_Filter_Clear_Button();
            }
#if DEBUG
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text == string.Empty, "Toolbox filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text + " is not empty after clicking clear filter button.");
#endif
        }

        public void Click_Settings_Resource_Permissions_Row1_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.IsTrue(ServicePickerDialog.Exists, "Service picker dialog does not exist.");
        }

        public void Click_Settings_Resource_Permissions_Row1_Windows_Group_Button()
        {
            Mouse.Click(FindAddWindowsGroupButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
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

        public void TryCloseHangingSaveDialog()
        {
            try
            {
                if (ControlExistsNow(SaveDialogWindow.CancelButton))
                {
                    Click_SaveDialog_CancelButton();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup threw an unhandled exception trying to remove hanging save dialog. Test may have crashed without leaving a hanging dialog.\n" + e.Message);
            }
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

        public void TryCloseHangingWindowsGroupDialog()
        {
            try
            {
                if (ControlExistsNow(SelectWindowsGroupDialog))
                {
                    Click_Select_Windows_Group_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove hanging select windows group dialog. Test might not have left a hanging dialog.\n" + e.Message);
            }
        }

        public void Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox()
        {
            #region Variable Declarations
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
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
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
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
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
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
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.CloseButton))
                {
                    TryCloseWorkflowTab();
                }
                else
                {
                    workflowTabCloseButtonExists = false;
                }
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.CloseButton))
                {
                    TryCloseSettingsTab();
                }
                else
                {
                    settingsTabCloseButtonExists = false;
                }
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.TabCloseButton))
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
                    if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.CloseButton))
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
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.CloseButton))
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

        public void TryCloseSettingsTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab))
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
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.TabCloseButton))
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

        public void WaitForSpinner(UITestControl control, int searchTimeout = 60000)
        {
            WaitForControlNotVisible(control, searchTimeout);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName, bool duplicate = false, bool invalid = false, bool nameHasWhiteSpace = false)
        {
            WpfText errorLabel = this.SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            if (duplicate || invalid || nameHasWhiteSpace)
            {
                if (duplicate)
                {
                    Assert.AreEqual(string.Format("An item with name '{0}' already exists in this folder.", ServiceName), errorLabel.DisplayText, "Error is not the same as expected");
                    Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
                if (invalid)
                {
                    Assert.AreEqual("'Name' contains invalid characters", errorLabel.DisplayText, "Error is not the same as expected");
                    Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
                if (nameHasWhiteSpace)
                {
                    Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
                    Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
                }
            }
            else
                Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        public void Filter_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        public void Enter_GroupName_Into_Windows_Group_Dialog(string GroupName)
        {
            SelectWindowsGroupDialog.ItemPanel.ObjectNameTextbox.Text = GroupName;
            Assert.IsTrue(SelectWindowsGroupDialog.OKPanel.OK.Enabled, "Windows group dialog OK button is not enabled.");
        }

        public void Select_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Click_Service_Picker_Dialog_Refresh_Button();
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1, new Point(91, 9));
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        public void TryCloseHangingDebugInputDialog()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DebugInputDialog))
                {
                    Click_DebugInput_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove hanging select windows group dialog. Test might not have left a hanging dialog.\n" + e.Message);
            }
        }

        public void TryRefreshExplorerUntilOneItemOnly(int retries = 3)
        {
            while ((ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem) || ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.SecondItem)) && retries-- > 0)
            {
                Click_Explorer_Refresh_Button();
            }
        }

        public void Select_From_Explorer_Remote_Server_Dropdown_List(WpfText comboboxListItem, int openComboboxListRetries = 3)
        {
            while (!ControlExistsNow(comboboxListItem) && openComboboxListRetries-- > 0)
            {
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(217, 8));
            }
            Assert.IsTrue(comboboxListItem.Exists, "TSTCIREMOTE does not exist in explorer remote server drop down list.");
            Mouse.Click(comboboxListItem, new Point(79, 8));
        }

        public void Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected, new Point(80, 13));
        }

        public void Select_NewRemoteServer_From_Explorer_Server_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox, new Point(217, 8));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... does not exist in explorer remote server drop down list");
            Mouse.Click(MainStudioWindow.ComboboxListItemAsNewRemoteServer.NewRemoteServerItemText, new Point(114, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.Exists, "Server source wizard does not exist.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.ProtocolCombobox.ToggleDropdown.Exists, "Server source wizard protocol dropdown does not exist.");
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
            Mouse.Click(MainStudioWindow.ComboboxListItemAsLocalhost, new Point(94, 10));
        }

        public void Save_With_Ribbon_Button_And_Dialog(string Name, bool skipExplorerFilter = false)
        {
            Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            WaitForSpinner(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Enter_Service_Name_Into_Save_Dialog(Name);
            Click_SaveDialog_Save_Button();
            if (!skipExplorerFilter)
            {
                Filter_Explorer(Name);
                Click_Explorer_Refresh_Button();
            }
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Saved " + Name + " does not appear in the explorer tree.");
            Click_Explorer_Filter_Clear_Button();
        }

        public void Click_SaveDialog_Save_Button()
        {
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        public void TryCloseNewPluginSourceWizardTab()
        {
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.PluginSourceWizardTab.CloseButton))
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
            if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab.CloseButton))
            {
                Click_Close_Web_Source_Wizard_Tab_Button();
                if (ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
        }

        public void Click_New_Workflow_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.NewWorkflowButton.Exists, "New Workflow Ribbon Button Does Not Exist!");
            Mouse.Click(MainStudioWindow.SideMenuBar.NewWorkflowButton, new Point(3, 8));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after clicking new workflow ribbon button.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Exists, "Toolbox filter textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Exists, "Explorer does not exist in the studio");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox.Exists, "Explorer connect control does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ConnectServerButton.Exists, "Connect in Explorer does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton.Exists, "Edit Connect control button does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.Exists, "Variable list view does not exist");
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save menu button not enabled for new workflow.");
        }

        public void Select_Last_Source_From_GET_Web_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
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
        }

        public void Click_New_Web_Source_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.WebSourceButton, new Point(13, 18));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Web server address textbox does not exist on new web source wizard tab.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Web server test connection button does not exist on new web source wizard tab.");
        }

        public void First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            var switchLeftAutoConnector = new Point(250, 200);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(switchLeftAutoConnector);
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, switchLeftAutoConnector);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        public void Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface()
        {
            #region Variable Declarations
            WpfEdit searchTextBox = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox;
            WpfListItem commentToolboxItem = this.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment;
            WpfCustom flowchart = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfCustom connector3 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3;
            WpfCustom commentOnTheDesignSurface = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment;
            #endregion

            var switchRightAutoConnector = new Point(360, 200);
            flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(commentToolboxItem, new Point(16, 25));
            Mouse.StopDragging(flowchart, switchRightAutoConnector);
            Assert.IsTrue(SwitchCaseDialog.DoneButton.Exists, "Switch case dialog done button does not exist after dragging onto switch case arm.");
            Mouse.Click(SwitchCaseDialog.DoneButton, new Point(34, 10));
            Assert.IsTrue(connector3.Exists, "Third auto connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(commentOnTheDesignSurface.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
            TryClearToolboxFilter();
        }

        public void Enter_Text_Into_Debug_Input_Row1_Value_Textbox(string text)
        {
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            }
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text, "Debug input data row1 textbox text is not equal to \'" + text + "\'.");
        }

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

        public void Type_dll_into_Plugin_Source_Wizard_Assembly_Textbox(string text)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.PluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Text = text;
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after DLL has been selected in plugin source wizard.");
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text = GroupName;
            Assert.AreEqual(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
        }

        public void SetResourcePermissions(string ResourceName, string WindowsGroupName, bool setView = false, bool setExecute = false, bool setContribute = false)
        {
            Click_Settings_Ribbon_Button();
            Click_Settings_Resource_Permissions_Row1_Add_Resource_Button();
            Select_Service_From_Service_Picker_Dialog(ResourceName);
            Click_Service_Picker_Dialog_OK();
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

        public void CreateRemoteServerSource(string ServerSourceName, string ServerAddress, bool PublicAuth = false)
        {
            Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext
                .NewServerSourceWizard.AddressComboBox.AddressEditBox.Text = ServerAddress;
            if (ServerAddress == "tst-ci-")
            {
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.NewServerSourceWizard.AddressComboBox.TSTCIREMOTE.Exists, "TSTCIREMOTE does not exist in server source wizard drop down list after starting by typing tst-ci-.");
                Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist();
            }
            if (PublicAuth)
            {
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.PublicRadioButton.Selected = true;
            }
            Click_Server_Source_Wizard_Test_Connection_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.ErrorText.Spinner);
            Save_With_Ribbon_Button_And_Dialog(ServerSourceName);
            Click_Close_Server_Source_Wizard_Tab_Button();
        }

        public void Select_Deploy_First_Source_Item()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.FirstExplorerTreeItem.CheckBox.Checked = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Enabled,
                "Deploy button is not enable after valid server and resource are selected.");
        }

        public void Click_Deploy_Tab_Deploy_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButton);
            WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Spinner);
            Assert.AreEqual("Success", MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButtonMessageText.DisplayText);
        }

        public void Change_Selected_Database_ToMySql_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MicrosoftSQLServer, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemMySqlDatabase.Exists, "ComboboxListItemMySqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemMySqlDatabase.MySqlDatabaseText, new Point(106, 19));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToMicrosoftSqlServer_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MicrosoftSQLServer, new Point(87, 7));
            Assert.IsTrue(serverTypeComboBox.Exists, "ComboboxListItemMySqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(serverTypeComboBox, new Point(106, 19));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToOracle_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.PostgreSQLDatabaseText, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemOracleDatabase.Exists, "ComboboxListItemOracleDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemOracleDatabase);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToODBC_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.OracleDatabase, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemODBCDatabase.Exists, "ComboboxListItemODBCDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemODBCDatabase.ODBCDatabaseText);
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Change_Selected_Database_ToPostgreSql_DataBase()
        {
            var serverTypeComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerTypeComboBox;
            Mouse.Click(serverTypeComboBox.MySqlDatabase, new Point(87, 7));
            Assert.IsTrue(MainStudioWindow.ComboboxListItemPostgreSqlDatabase.Exists, "ComboboxListItemPostgreSqlDatabase does not exist after clicking db type combobox.");
            Mouse.Click(MainStudioWindow.ComboboxListItemPostgreSqlDatabase.PostgreSQLDatabase);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserRadioButton.Enabled, "User authentification rabio button is not enabled.");
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows authentification type radio button not enabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.ServerComboBox.Enabled, "Server textbox is disabled in db source wizard.");
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.UserNameTextBox.TryGetClickablePoint(out point), "Username textbox is visible in db source wizard.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.PasswordTextBox.TryGetClickablePoint(out point), "Password textbox is visible in db source wizard.");
        }

        public void Click_Settings_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.ConfigureSettingsButton.Exists, "Settings ribbon does not exist.");
            Mouse.Click(MainStudioWindow.SideMenuBar.ConfigureSettingsButton, new Point(7, 2));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "settings tab does not exist after clicking settings ribbon button.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "Security tab does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Exists, "Logging tab does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Exists, "Resource Permissions does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Server Permissions does not exist in the settings window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.Exists, "Settings security tab resource permissions row1 does not exist");
        }

        public void Click_Deploy_Ribbon_Button()
        {
            Assert.IsTrue(MainStudioWindow.SideMenuBar.DeployButton.Exists, "Deploy ribbon button does not exist");
            Mouse.Click(MainStudioWindow.SideMenuBar.DeployButton, new Point(16, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.Exists, "Deploy tab does not exist after clicking deploy ribbon button.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.Exists, "Source explorer tree does not exist on deploy.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.ExplorerTree.SourceServerName.Exists, "Source server name in deploy window does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.RefreshSourceServerButton.Exists, "Refresh button source server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceServerExplorer.FilterText.Exists, "Filter source server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.OverrideHyperlink.Exists, "Override count in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.NewResourceHyperlink.Exists, "New Resource count in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.EditSourceButton.Exists, "Edit source server button does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceConnectButton.Exists, "Connect button in the Source server does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.EditDestinationButton.Exists, "Edit Destination Server button does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.ConnectDestinationButton.Exists, "Connect Button in Destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceConnectControl.Exists, "Source Server connect control does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.ShowDependenciesButton.Exists, "Select All Dependencies button Destination Server does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.ServicesText.Exists, "Services Label in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.ServiceCountText.Exists, "Service Count value in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourcesText.Exists, "Source label in destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.SourceCountText.Exists, "Source Count value in the destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.NewResourcesText.Exists, "New Resource Label in the destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.OverrideText.Exists, "Override label on Destination Server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButton.Exists, "Deploy button in Destination server does not exist in the deploy window");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DeployButtonMessageText.Exists, "Success message label does not exist in destination server of the deploy window");
        }

        public void TryCloseDeployTab()
        {
            try
            {
                if (ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab))
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

        public void Click_EnableDisable_This_Test_CheckBox(bool nameContainsStar = false)
        {
            WpfCheckBox testEnabledSelector = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector;
            WpfButton deleteButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.DeleteButton;
            var beforeClick = testEnabledSelector.Checked;

            Mouse.Click(testEnabledSelector);
            WaitForControlVisible(testEnabledSelector);
            Assert.AreNotEqual(beforeClick, testEnabledSelector.Checked);

            WaitForControlVisible(deleteButton);
            if (beforeClick)
                Assert.IsTrue(deleteButton.Enabled, "Delete button is disabled");
            Assert_Display_Text_ContainStar(Tab, nameContainsStar);
            Assert_Display_Text_ContainStar(Test, nameContainsStar);
        }

        public void Drag_From_Explorer_Onto_DesignSurface(string ServicePath)
        {
            Filter_Explorer(ServicePath);
            Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface();
        }

        public void Drag_Dice_Roll_Example_Onto_DesignSurface()
        {
            Filter_Explorer("Dice Roll");
            Drag_Explorer_Localhost_Second_Items_First_Sub_Item_Onto_Workflow_Design_Surface();
        }

        public void Select_Show_Dependencies_In_Explorer_Context_Menu(string ServiceName)
        {
            #region Variable Declarations
            WpfMenuItem showDependencies = this.MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfRadioButton showwhatdependsonthisRadioButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox;
            WpfButton refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton;
            WpfText text = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text;
            #endregion

            Mouse.Click(showDependencies, new Point(50, 15));
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(refreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.AreEqual(ServiceName, text.DisplayText, "Dependant workflow not shown in dependency diagram");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies" +
                    " is selected");
        }

        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
        }

        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.SearchTextboxText = ServiceName;
            Enter_DeployViewOnly_Into_Deploy_Source_Filter();
            Select_Deploy_First_Source_Item();
            Click_Deploy_Tab_Deploy_Button();
        }

        public void Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest()
        {
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox;

            var helloUser = "Hello User.";
            Keyboard.SendKeys(textbox, helloUser, ModifierKeys.None);

            // Verify that the 'Text' property of 'Text' text box equals 'User'
            Assert.AreEqual(helloUser, textbox.Text, "Workflow tests output row 1 value textbox text does not equal 'Hello User' after typing that in.");
        }

        public void Select_Test(int instance = 1)
        {
            var currentTest = GetCurrentTest(instance);
            //var test = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1;
            Mouse.Click(currentTest);
        }

        public void Click_RunAll_Button(string BrokenRule = null)
        {
            string DuplicateNameError = "DuplicateNameError";
            string UnsavedResourceError = "UnsavedResourceError";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.RunAllButton, new Point(35, 10));
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

        public void UnCheck_Public_Administrator()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            WpfCheckBox public_DeployToCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox;
            WpfCheckBox public_DeployFromCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox;
            #endregion

            public_AdministratorCheckBox.Checked = false;
            Assert.IsFalse(public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Administrator.");
            Assert.IsTrue(public_ViewCheckBox.Checked, "Public View checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox unchecked after unChecking Administrator.");
            Assert.IsTrue(public_ContributeCheckBox.Checked, "Public Contribute checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_DeployFromCheckBox.Checked, "Public DeplotFrom checkbox is unchecked after unChecking Administrator.");
            Assert.IsTrue(public_DeployToCheckBox.Checked, "Public DeployTo checkbox is unchecked after unChecking Administrator.");
        }
        public void Check_Resource_Contribute()
        {
            WpfCheckBox resource_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfCheckBox resource_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.ContributeCell.ContributeCheckBox;
            WpfButton resource_DeleteButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.DeleteCell.DeleteButton;

            resource_ContributeCheckBox.Checked = true;
            Assert.IsTrue(resource_ViewCheckBox.Checked, "Resource View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_ExecuteCheckBox.Checked, "Resource Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(resource_DeleteButton.Enabled, "Resource Delete button is disabled");

        }
        public void Check_Public_Contribute()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            public_ContributeCheckBox.Checked = true;
            Assert.IsTrue(public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Contribute.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
        }

        public void UnCheck_Public_View()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            public_ViewCheckBox.Checked = false;
            Assert.IsFalse(public_ViewCheckBox.Checked, "Public View checkbox is checked after Checking Contribute.");
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsFalse(public_ContributeCheckBox.Checked, "Public Contribute checkbox is checked after UnChecking Execute/View.");
            Assert.IsFalse(public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Contribute.");
        }

        public void Update_Test_Name(string overrideName = null)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText.UIItemEdit, new Point(59, 16));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText.UIItemEdit.Text = "";
            if (!string.IsNullOrEmpty(overrideName))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText.UIItemEdit.Text = overrideName;
            else
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText.UIItemEdit.Text = "Dice_Test";
        }

        public void Click_Delete_Test_Button()
        {
            WpfButton deleteButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.DeleteButton;
            Mouse.Click(deleteButton);
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
            //if (expectedTestResultEnum != null)
            //    AssertTestResults(expectedTestResultEnum.Value, instance, currentTest);
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.DuplicateButton, new Point(14, 10));
        }

        public void Assert_MultiAssign_Does_Not_Exist_On_DesignSurface()
        {
            var multiAssign = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            Assert.IsFalse(multiAssign.Exists);
        }

        public void Assert_Test_Result(string result)
        {
            WpfText passing = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing;
            WpfText invalid = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Invalid;
            WpfText failing = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Failing;
            WpfText pending = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Pending;
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
            WpfButton createTestButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton;
            WpfText testNameText = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText;
            var currentTest = GetCurrentTest(testInstance);
            var testEnabledSelector = GetTestRunState(testInstance, currentTest);
            var testNeverRun = GetSelectedTestNeverRunDisplay(currentTest, testInstance);
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList;

            #endregion

            Mouse.Click(createTestButton, new Point(158, 10));

            Assert.AreEqual(testInstance + 1, testsListBox.GetContent().Length);
            Assert.AreEqual("Never run", testNeverRun.DisplayText);
            AssertTestResults(TestResultEnum.Pending, testInstance, currentTest);
            Assert.IsTrue(testNameText.Exists, string.Format("Test{0} Name textbox does not exist after clicking Create New Test", testInstance));
            Assert.IsTrue(testEnabledSelector, string.Format("Test {0} is diabled after clicking Create new test from context menu", testInstance));
            //Assert.IsTrue(textbox.Exists, "Row 1 input value textbox does not exist on workflow tests tab.");

            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        private void Assert_Display_Text_ContainStar(string control, bool containsStar, int instance = 1)
        {
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList;
            var test = GetCurrentTest(instance);
            string description = string.Empty;
            if (control == "Tab")
            {
                description = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText;
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

        private bool GetTestRunState(int testInstance, WpfListItem test)
        {
            bool value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.TestEnabledSelector.Checked;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.TestEnabledSelector.Checked;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.TestEnabledSelector.Checked;
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
                default:
                    var test1 = test as Test1;
                    property = test1.TestNameDisplay;
                    break;
            }

            return property;
        }

        public void Select_Test_From_TestList(int testInstance = 1)
        {
            var test = GetCurrentTest(testInstance);

            if (test != null)
                Mouse.Click(test);
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
                case 1:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1;
                    break;
                case 2:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2;
                    break;
                case 3:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test3;
                    break;
                default:
                    test = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1;
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
                default:
                    var test1 = test as Test1;
                    value = test1.RunButton;
                    break;
            }
            return value;
        }

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
            WpfCustom multiAssign = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
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
            #region Variable Declarations
            WpfCell variableCell = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox;
            WpfEdit textEdit = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.ValueCell.AssignValueCombobox.TextEdit;
            WpfEdit textbox1 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.VariableCell.Listbox.Textbox;
            WpfEdit row2Value = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.ValueCell.AssignValueCombobox.TextEdit;
            WpfEdit textbox2 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.VariableCell.Listbox.Textbox;
            WpfEdit textEdit1 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.ValueCell.AssignValueCombobox.TextEdit;
            WpfEdit textbox3 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.VariableCell.Listbox.Textbox;
            WpfEdit textEdit2 = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.ValueCell.AssignValueCombobox.TextEdit;

            var firstVariable = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1;
            var rec = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1;
            var mr = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2;
            #endregion

            //// Click 'Item: Unlimited.Applications.BusinessDesignStudio....' cell
            //Mouse.Click(variableCell, new Point(33, 28));

            //// Type '[[rec().a]]' in 'UI__Row1_FieldName_AutoID' text box
            //textbox.Text = this.Enter_Recordset_valuesParams.TextboxText;

            //// Type '5' in 'Text' text box
            //textEdit.Text = this.Enter_Recordset_valuesParams.TextEditText;

            //// Type '{Tab}' in 'Text' text box
            //Keyboard.SendKeys(textEdit, this.Enter_Recordset_valuesParams.TextEditSendKeys, ModifierKeys.None);

            //// Type '[[rec().b]]' in 'UI__Row2_FieldName_AutoID' text box
            //textbox1.Text = this.Enter_Recordset_valuesParams.TextboxText1;

            //// Type '[[var]]' in 'UI__Row3_FieldName_AutoID' text box
            //textbox2.Text = this.Enter_Recordset_valuesParams.TextboxText2;

            //// Type '{Tab}' in 'UI__Row3_FieldName_AutoID' text box
            //Keyboard.SendKeys(textbox2, this.Enter_Recordset_valuesParams.TextboxSendKeys, ModifierKeys.None);

            //// Type '1' in 'Text' text box
            //textEdit1.Text = this.Enter_Recordset_valuesParams.TextEditText1;

            //// Type '{Tab}' in 'Text' text box
            //Keyboard.SendKeys(textEdit1, this.Enter_Recordset_valuesParams.TextEditSendKeys1, ModifierKeys.None);

            //// Type '[[mr()]]' in 'UI__Row4_FieldName_AutoID' text box
            //textbox3.Text = this.Enter_Recordset_valuesParams.TextboxText3;

            //// Type '{Tab}' in 'Text' text box
            //Keyboard.SendKeys(textEdit2, this.Enter_Recordset_valuesParams.TextEditSendKeys2, ModifierKeys.None);

            Assert.IsTrue(firstVariable.Exists, "var does not exist in the variable explorer");
            Assert.IsTrue(rec.Exists, "rec().a does not exist in the variable explorer");
            Assert.IsTrue(mr.Exists, "rec().a does not exist in the variable explorer");
        }

        public void Select_InputOutput_CheckBox_Recordset_values()
        {
            #region Variable Declarations

            var IsOutputCheckboxRecA = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.OutputCheckbox;
            var IsInputCheckboxVar = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox;

            #endregion

            // Click 'Records' table
            IsInputCheckboxVar.Checked = true;
            IsOutputCheckboxRecA.Checked = true;
        }

        public void Type_Value_And_Click_Enter_Keyboard_Then_Backspace()
        {
            var varValue = MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText;

            var heightBeforeEnterClick = varValue.Height;
            varValue.Text = "Bob";
            Keyboard.SendKeys(varValue, "{Enter}", ModifierKeys.None);
            Assert.IsTrue(varValue.Height > heightBeforeEnterClick);

            Keyboard.SendKeys(varValue, "{Back}", ModifierKeys.None);
            Assert.AreEqual(heightBeforeEnterClick, varValue.Height);
        }

        public void F5_Keyboard_Click()
        {
            WpfCheckBox uIUI_IsOutputCheckbox_CheckBox = MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field.OutputCheckbox;
            var varText = MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText;

            Keyboard.SendKeys(uIUI_IsOutputCheckbox_CheckBox, "{F5}", ModifierKeys.None);
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.DebugF6Button.Exists, "Debug button in Debug Input window does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.CancelButton.Exists, "Cancel Debug Input Window button does not exist.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.RememberDebugInputCheckBox.Exists, "Remember Checkbox does not exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.ViewInBrowserF7Button.Enabled, "View in Browser button does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Exists, "Input Data Window does not exist in Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.XMLTab.Exists, "Xml tab does not Exist in the Debug Input window.");
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.TabItemsTabList.JSONTab.Exists, "Assert Json tab does not exist in the debug input window.");

            Assert.AreEqual(varText.Text, "var");
        }

        public void Click_DotNet_DLL_Large_View_Generate_Outputs()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.DoneButton.Exists);
        }

        public void Click_New_Web_Source_Test_Connection_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(52, 14));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after testing a valid web source.");
        }

        public void Click_Scheduler_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.SchedulerButton, new Point(4, 12));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SchedulerTab.Exists, "Scheduler tab does not exist after clicking scheduler ribbon button.");
        }

        public void Click_Scheduler_ResourcePicker()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton, new Point(20, 12));
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

        public void Click_Subworkflow_Done_Button()
        {
            Assert.IsTrue(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
            Mouse.Click(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton, new Point(53, 16));
        }

        public void Click_Assign_Tool_url()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
            Assert.IsTrue(MessageBoxWindow.Exists, "Did you know popup does not exis");
            Assert.IsTrue(MessageBoxWindow.OKButton.Exists, "Ok button does not exist on the DidYouKnow button");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(38, 12));
        }

        public void Click_Assign_Tool_Remove_Variable_From_Tool()
        {
            Click_Assign_Tool_ExpandAll();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox.Text = "[[SomeOtherVariable]]";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox, "{Right}{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid%Variable]]\".");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox.Exists, "Assign large view row 1 variable textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Exists, "Variable filter textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton.Exists, "Variable clear filter button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton, new Point(8, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Exists, "Variable delete does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneRight.Variables.VariablesControl.XtgDataPresenter.Table.VariableDataItem.List.ListItem.Table.DataItem1.VariableNameCell.ValueEditor.TextBox.DeleteButton, new Point(9, 8));
        }
        
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }
    }
}
