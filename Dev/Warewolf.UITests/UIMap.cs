using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;

namespace Warewolf.UITests
{

    public partial class UIMap
    {
        private int _lenientSearchTimeout = 3000;
        private int _lenientMaximumRetryCount = 3;
        private int _strictSearchTimeout = 1000;
        private int _strictMaximumRetryCount = 1;

        public void SetGlobalPlaybackSettings()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            if (Environment.ProcessorCount <= 4)
            {
                Playback.PlaybackSettings.ThinkTimeMultiplier = 2;
            }
            else
            {
                Playback.PlaybackSettings.ThinkTimeMultiplier = 1;
            }
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.SkipSetPropertyVerification = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackError -= new EventHandler<PlaybackErrorEventArgs>(PlaybackErrorHandler);
            Playback.PlaybackError += new EventHandler<PlaybackErrorEventArgs>(PlaybackErrorHandler);
        }

        void PlaybackErrorHandler(object sender, PlaybackErrorEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
            if (e.Error is UITestControlNotFoundException)
            {
                UITestControlNotFoundException asControlNotFoundException = e.Error as UITestControlNotFoundException;
                var exceptionSource = asControlNotFoundException.ExceptionSource;
                if (exceptionSource is UITestControl)
                {
                    UITestControl parent = (exceptionSource as UITestControl).Container;
                    while (parent != null && !parent.Exists)
                    {
                        parent = parent.Container;
                    }
                    if (parent != null && parent.Exists && parent != MainStudioWindow)
                    {
                        Console.WriteLine("Search actually failed at: " + parent.FriendlyName);
                        parent.SearchProperties.ToList().ForEach(prop => { Console.WriteLine(prop.PropertyName + ": \"" + prop.PropertyValue + "\""); });
                        parent.DrawHighlight();
                        e.Result = PlaybackErrorOptions.Retry;
                        return;
                    }
                }
            }
            if (e.Error is UITestControlNotAvailableException)
            {
                UITestControlNotAvailableException asControlNotAvailableException = e.Error as UITestControlNotAvailableException;
                var exceptionSource = asControlNotAvailableException.ExceptionSource;
                if (exceptionSource is UITestControl)
                {
                    (exceptionSource as UITestControl).DrawHighlight();
                    e.Result = PlaybackErrorOptions.Retry;
                    return;
                }
            }
            if (e.Error is FailedToPerformActionOnBlockedControlException)
            {
                FailedToPerformActionOnBlockedControlException asBlockedControlException = e.Error as FailedToPerformActionOnBlockedControlException;
                var exceptionSource = asBlockedControlException.ExceptionSource;
                if (exceptionSource is UITestControl)
                {
                    (exceptionSource as UITestControl).DrawHighlight();
                    e.Result = PlaybackErrorOptions.Retry;
                    return;
                }
            }
            Playback.Wait(_strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
            e.Result = PlaybackErrorOptions.Retry;
        }

        public void WaitIfStudioDoesNotExist()
        {
            var sleepTimer = 60;
            try
            {
                if (!MainStudioWindow.Exists)
                {
                    WaitForStudioStart(sleepTimer * _strictSearchTimeout);
                }
            }
            catch (UITestControlNotFoundException)
            {
                WaitForStudioStart(sleepTimer * _strictSearchTimeout);
            }
        }

        private void WaitForStudioStart(int timeout)
        {
            Console.WriteLine("Waiting for studio to start.");
            Playback.Wait(timeout);
            if (!MainStudioWindow.Exists)
            {
                throw new InvalidOperationException("Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
            }
        }

        public void InitializeABlankWorkflow()
        {
            Click_New_Workflow_Ribbon_Button();
        }

        public void CleanupWorkflow()
        {
            try
            {
                if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text != string.Empty)
                {
                    Click_Clear_Toolbox_Filter_Button();
                }
                Click_Close_Workflow_Tab_Button();
                Click_MessageBox_No();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during test cleanup: " + e.Message);
            }
        }

        public void Click_Settings_Resource_Permissions_Row1_Add_Resource_Button()
        {
            Mouse.Click(FindAddResourceButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.AreEqual(true, ServicePickerDialog.Exists, "Service picker dialog does not exist.");
        }

        public void Click_Settings_Resource_Permissions_Row1_Windows_Group_Button()
        {
            Mouse.Click(FindAddWindowsGroupButton(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1));
            Assert.AreEqual(true, SelectWindowsGroupDialog.Exists, "Select windows group dialog does not exist.");
            Assert.AreEqual(true, SelectWindowsGroupDialog.ItemPanel.ObjectNameTextbox.Exists, "Select windows group object name textbox does not exist.");
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
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var cancelButton = SaveDialogWindow.CancelButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (cancelButton)
                {
                    Click_SaveDialog_CancelButton();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup threw an unhandled exception trying to remove hanging save dialog. Test may have crashed without leaving a hanging dialog.\n" + e.Message);
            }
        }

        public void TryRemoveRemoteServerUITestWorkflowFromExplorer()
        {
            try
            {
                if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text == string.Empty)
                {
                    Enter_RemoteServerUITestWorkflow_Into_Explorer_Filter();
                    WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                }
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var wpfTreeItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (wpfTreeItem)
                {
                    RightClick_Explorer_Localhost_First_Item();
                    Select_Delete_FromExplorerContextMenu();
                    Click_MessageBox_Yes();
                }
                Click_Explorer_Filter_Clear_Button();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove RemoteServerUITestWorkflow. Test may have crashed before RemoteServerUITestWorkflow was created.\n" + e.Message);
            }
        }

        public void TryDisconnectFromCIREMOTEAndRemoveSourceFromExplorer()
        {
            try
            {
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var selectedItemAsTstciremoteConnected = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsTSTCIREMOTEConnected.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (selectedItemAsTstciremoteConnected)
                {
                    Click_Explorer_RemoteServer_Connect_Button();
                }
                else
                {
                    Click_Connect_Control_InExplorer();
                    Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                    Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                    var comboboxListItemAsTstciremoteConnected = MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected.Exists;
                    Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                    Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                    if (comboboxListItemAsTstciremoteConnected)
                    {
                        Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List();
                        Click_Explorer_RemoteServer_Connect_Button();
                    }
                }
                Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List();
                Enter_TSTCIREMOTE_Into_Explorer_Filter();
                WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var wpfTreeItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (wpfTreeItem)
                {
                    RightClick_Explorer_Localhost_First_Item();
                    Select_Delete_FromExplorerContextMenu();
                    Click_MessageBox_Yes();
                }
                Click_Explorer_Filter_Clear_Button();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server TST-CI-REMOTE. Test may have crashed before remote server TST-CI-REMOTE was connected.\n" + e.Message);
                Click_Explorer_Filter_Clear_Button();
            }
        }

        public void TryRemoveSomeWorkflowFromExplorer()
        {
            try
            {
                Enter_SomeWorkflow_Into_Explorer_Filter();
                WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var wpfTreeItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (wpfTreeItem)
                {
                    RightClick_Explorer_Localhost_First_Item();
                    Select_Delete_FromExplorerContextMenu();
                    Click_MessageBox_Yes();
                }
                Click_Explorer_Filter_Clear_Button();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server TST-CI-REMOTE. Test may have crashed before remote server TST-CI-REMOTE was connected.\n" + e.Message);
                Click_Explorer_Filter_Clear_Button();
            }
        }

        public void TryCloseHangingWindowsGroupDialog()
        {
            try
            {
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var selectWindowsGroupDialog = SelectWindowsGroupDialog.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if (selectWindowsGroupDialog)
                {
                    Click_Select_Windows_Group_Cancel_Button();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove hanging select windows group dialog. Test might not have left a hanging dialog.\n" + e.Message);
            }
        }

        /// <summary>
        /// Click_Settings_Security_Tab_ResourcePermissions_Execute_Checkbox - Use 'Click_Settings_Security_Tab_ResourcePermissions_Execute_CheckboxParams' to pass parameters into this method.
        /// </summary>
        public void Click_Settings_Security_Tab_ResourcePermissions_Row1_Execute_Checkbox()
        {
            #region Variable Declarations
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox executeCheckBox = FindExecutePermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            // Select 'UI__ExecutePermissionCheckBox_AutoID' check box
            executeCheckBox.Checked = true;

            // Verify that the 'Checked' property of 'UI__ExecutePermissionCheckBox_AutoID' check box equals 'True'
            Assert.AreEqual(true, executeCheckBox.Checked, "Settings security tab resource permissions row 1 execute checkbox is not checked.");

            // Verify that the 'Enabled' property of 'Save this tab' button equals 'True'
            Assert.AreEqual(true, saveButton.Enabled, "Save ribbon button is not enabled");
        }

        /// <summary>
        /// Click_Settings_Security_Tab_Resource_Permissions_View_Checkbox - Use 'Click_Settings_Security_Tab_Resource_Permissions_View_CheckboxParams' to pass parameters into this method.
        /// </summary>
        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_View_Checkbox()
        {
            #region Variable Declarations
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox viewCheckBox = FindViewPermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            // Select 'UI__ViewPermissionCheckBox_AutoID' check box
            viewCheckBox.Checked = true;

            // Verify that the 'Checked' property of 'UI__ViewPermissionCheckBox_AutoID' check box equals 'True'
            Assert.AreEqual(true, viewCheckBox.Checked, "Settings resource permissions row1 view checkbox is not checked.");

            // Verify that the 'Enabled' property of 'Save this tab' button equals 'True'
            Assert.AreEqual(true, saveButton.Enabled, "Save ribbon button is not enabled");
        }

        /// <summary>
        /// Click_Settings_Security_Tab_Resource_Permissions_Contribute_Checkbox - Use 'Click_Settings_Security_Tab_Resource_Permissions_Contribute_CheckboxParams' to pass parameters into this method.
        /// </summary>
        public void Click_Settings_Security_Tab_Resource_Permissions_Row1_Contribute_Checkbox()
        {
            #region Variable Declarations
            Row1 row1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1;
            WpfCheckBox contributeCheckBox = FindContributePermissionsCheckbox(row1);
            WpfButton saveButton = this.MainStudioWindow.SideMenuBar.SaveButton;
            #endregion

            // Select 'UI__ContributePermissionCheckBox_AutoID' check box
            contributeCheckBox.Checked = true;

            // Verify that the 'Checked' property of 'UI__ViewPermissionCheckBox_AutoID' check box equals 'True'
            Assert.AreEqual(true, contributeCheckBox.Checked, "Settings resource permissions row1 view checkbox is not checked.");

            // Verify that the 'Enabled' property of 'Save this tab' button equals 'True'
            Assert.AreEqual(true, saveButton.Enabled, "Save ribbon button is not enabled");
        }

        public void TryCloseAllTabs()
        {
            Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            var workflowTabCloseButtonExists = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.CloseButton.Exists;
            var settingsTabCloseButtonExists = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.CloseButton.Exists;
            var serverSourceWizardTabCloseButtonExists = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.TabCloseButton.Exists;
            Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
            while (workflowTabCloseButtonExists || settingsTabCloseButtonExists || serverSourceWizardTabCloseButtonExists)
            {
                TryCloseWorkflowTab();
                TryCloseSettingsTab();
                TryCloseServerSourceWizardTab();
            }
        }

        private void TryCloseWorkflowTab()
        {
            try
            {
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var workflowTabCloseButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.CloseButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(workflowTabCloseButton)
                {
                    Click_Close_Workflow_Tab_Button();
                }
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var messageBoxNoButton = MessageBoxWindow.NoButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(messageBoxNoButton)
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Workflow tab.\n" + e.Message);
            }
        }

        private void TryCloseSettingsTab()
        {
            try
            {
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var settingsTabCloseButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(settingsTabCloseButton)
                {
                    Click_Close_Settings_Tab_Button();
                }
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var messageBoxNoButton = MessageBoxWindow.NoButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(messageBoxNoButton)
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
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var serverSourceWizardTabCloseButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.TabCloseButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(serverSourceWizardTabCloseButton)
                {
                    Click_Close_Server_Source_Wizard_Tab_Button();
                }
                Playback.PlaybackSettings.MaximumRetryCount = _strictMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _strictSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                var messageBoxNoButton = MessageBoxWindow.NoButton.Exists;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString());
                if(messageBoxNoButton)
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryClose method failed to close Server Source tab.\n" + e.Message);
            }
        }

        public void WaitForSpinner(UITestControl spinner)
        {
            spinner.WaitForControlCondition((control) =>
            {
                var point = new Point();
                return !control.TryGetClickablePoint(out point);
            }, 60000 * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }
    }
}
