using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;

namespace Warewolf.UITests
{
    public partial class UIMap
    {
        const int _lenientSearchTimeout = 5000;
        const int _lenientMaximumRetryCount = 3;
        const int _strictSearchTimeout = 1000;
        const int _strictMaximumRetryCount = 1;

        public void SetGlobalPlaybackSettings()
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

        public void WaitForStudioStart(int timeout = 60000)
        {
            Console.WriteLine("Waiting for studio to start.");
            WaitForControlVisible(MainStudioWindow, timeout);
            if (!MainStudioWindow.Exists)
            {
                throw new InvalidOperationException("Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
            }
            TryClickMessageBoxOK();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);

            //TODO: remove this workaround for WOLF-2061
            //MainStudioWindow.SideMenuBar.NewWorkflowButton.WaitForControlEnabled();
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
                Enter_Text_Into_Explorer_Filter(SourceName);
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
            try
            {
                var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
                if (File.Exists(resourcesFolder + @"\" + ResourceName + ".xml"))
                {
                    Enter_Text_Into_Explorer_Filter(ResourceName);
                    WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
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
                Console.WriteLine("Cleanup failed to remove resource " + ResourceName + ". Test may have crashed before " + ResourceName + " was created.\n" + e.Message);
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
                TryClickMessageBoxOK();
                return control.TryGetClickablePoint(out point);
            }, searchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }

        public void WaitForControlNotVisible(UITestControl control, int searchTimeout = 60000)
        {
            control.WaitForControlCondition((uicontrol) =>
            {
                TryClickMessageBoxOK();
                var point = new Point();
                return !uicontrol.TryGetClickablePoint(out point);
            }, searchTimeout * int.Parse(Playback.PlaybackSettings.ThinkTimeMultiplier.ToString()));
        }

        public void WaitForSpinner(UITestControl control, int searchTimeout = 60000)
        {
            WaitForControlNotVisible(control, searchTimeout);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName)
        {
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        public void Enter_Text_Into_Explorer_Filter(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
        }

        public void Enter_GroupName_Into_Windows_Group_Dialog(string GroupName)
        {
            SelectWindowsGroupDialog.ItemPanel.ObjectNameTextbox.Text = GroupName;
            Assert.IsTrue(SelectWindowsGroupDialog.OKPanel.OK.Enabled, "Windows group dialog OK button is not enabled.");
        }

        public void Enter_ServiceName_Into_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.TreeItem1.SubTreeItem1, new Point(91, 9));
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
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

        public void Save_With_Ribbon_Button_And_Dialog(string Name)
        {
            Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            WaitForControlNotVisible(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
            Enter_Service_Name_Into_Save_Dialog(Name);
            Click_SaveDialog_Save_Button();
            Enter_Text_Into_Explorer_Filter(Name);
            Click_Explorer_Refresh_Button();
            WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Saved " + Name + " does not appear in the explorer tree.");
            Click_Explorer_Filter_Clear_Button();
        }

        public void Click_SaveDialog_Save_Button()
        {
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
            Assert.IsFalse(ControlExistsNow(SaveDialogWindow));
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
            Assert_RunDebug_Button_Disabled();
            Assert.IsTrue(MainStudioWindow.SideMenuBar.NewWorkflowButton.Exists, "New Workflow Ribbon Button Does Not Exist!");
            Mouse.Click(MainStudioWindow.SideMenuBar.NewWorkflowButton, new Point(3, 8));
            var getTimeBefore = System.DateTime.Now;
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode);
            var timeWaited = System.DateTime.Now - getTimeBefore;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node Does Not Exist after waiting for " + timeWaited.Milliseconds + "ms.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Exists, "Toolbox filter textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Exists, "Explorer does not exist in the studio");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ServerListComboBox.Exists, "Explorer connect control does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ConnectServerButton.Exists, "Connect in Explorer does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton.Exists, "Edit Connect control button does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.VariablesControl.Exists, "Variable list view does not exist");
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
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WebSourceWizardTab);
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
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Cell.ComboBox.Textbox.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Cell.ComboBox.Textbox.Text = text;
            }
            Assert.AreEqual(text, MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Cell.ComboBox.Textbox.Text, "Debug input data row1 textbox text is not equal to \'" + text + "\'.");
        }

        public void Click_Debug_Ribbon_Button()
        {
            Mouse.Click(MainStudioWindow.SideMenuBar.RunAndDebugButton, new Point(13, 14));
            var getTimeBefore = System.DateTime.Now;
            WaitForControlVisible(MainStudioWindow.DebugInputDialog);
            var timeWaited = System.DateTime.Now - getTimeBefore;
            Assert.IsTrue(MainStudioWindow.DebugInputDialog.Exists, "Debug Input window does not exist after waiting for " + timeWaited.Milliseconds + "ms.");
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
            Enter_ServiceName_Into_Service_Picker_Dialog(ResourceName);
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
            Click_Server_Source_Wizard_Address_Protocol_Dropdown();
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
            WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.ServerSourceWizardTab.WorkSurfaceContext.ErrorText.Spinner);
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
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab);
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
            WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab);
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

        /// <summary>
        /// Click_DB_Source_Wizard_Test_Connection_Button
        /// </summary>
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            #region Variable Declarations
            WpfButton testConnectionButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton;
            #endregion
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
            // Click 'Test Connection' button
            Mouse.Click(testConnectionButton, new Point(21, 16));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
        }

        public void Deploy_Service_From_Deploy_View(string ServiceName)
        {
            Enter_DeployViewOnly_Into_Deploy_Source_FilterParams.SearchTextboxText = ServiceName;
            Enter_DeployViewOnly_Into_Deploy_Source_Filter();
            Select_Deploy_First_Source_Item();
            Click_Deploy_Tab_Deploy_Button();
        }

        /// <summary>
        /// Assert_Help_Text_Exist - Use 'Assert_Help_Text_ExistParams' to pass parameters into this method.
        /// </summary>
        public void Assert_Help_Text_Exist()
        {
            #region Variable Declarations
            WpfCustom helpTextEditor = this.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor;
            #endregion

            // Verify that the 'Exists' property of 'XamRichTextEditor' custom control equals 'True'
            Assert.IsTrue(helpTextEditor.Exists, "Help text does not exist");
        }

        /// <summary>
        /// Assert_RunDebug_Button_Disabled - Use 'Assert_RunDebug_Button_DisabledExpectedValues' to pass parameters into this method.
        /// </summary>
        public void Assert_RunDebug_Button_Disabled()
        {
            #region Variable Declarations
            WpfButton runAndDebugButton = this.MainStudioWindow.SideMenuBar.RunAndDebugButton;
            #endregion
            // Verify that the 'Enabled' property of 'Run and debug your workflow service' button equals 'False'
            Assert.IsFalse(runAndDebugButton.Enabled, "RunDebug button is enabled");
        }

        /// <summary>
        /// Select_New_Remote_Server_From_Destination_Server_DropDown
        /// </summary>
        public void Select_New_Remote_Server_From_Destination_Server_DropDown()
        {
            #region Variable Declarations
            WpfText newRemoteServer = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DeployTab.WorkSurfaceContext.DestinationServerConectControl.Combobox.NewRemoteServer;
            #endregion

            // Click 'New Remote Server...' label
            Mouse.Click(newRemoteServer);
        }

        /// <summary>
        /// Select_Show_Dependencies_In_Explorer_Context_Menu - Use 'Select_Show_Dependencies_In_Explorer_Context_MenuParams' to pass parameters into this method.
        /// </summary>
        /// <param name="workflowName"></param>
        public void Select_Show_Dependencies_In_Explorer_Context_Menu(string workflowName)
        {
            #region Variable Declarations
            WpfMenuItem showDependencies = this.MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfRadioButton showwhatdependsonthisRadioButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox;
            WpfButton refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton;
            WpfText text = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text;
            #endregion

            // Click 'Show Dependencies' menu item
            Mouse.Click(showDependencies, new Point(50, 15));

            // Verify that the 'Selected' property of 'Show what depends on this' radio button equals 'True'
            Assert.AreEqual(this.Select_Show_Dependencies_In_Explorer_Context_MenuParams.ShowwhatdependsonthisRadioButtonSelected, showwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");

            // Verify that the 'Exists' property of first text box next to 'Nesting Levels' label equals 'True'
            Assert.AreEqual(this.Select_Show_Dependencies_In_Explorer_Context_MenuParams.TextboxExists, textbox.Exists, "Dependency graph nesting levels textbox does not exist.");

            // Verify that the 'Exists' property of 'Refresh' button equals 'True'
            Assert.AreEqual(this.Select_Show_Dependencies_In_Explorer_Context_MenuParams.RefreshButtonExists, refreshButton.Exists, "Refresh button does not exist on dependency graph");

            // Verify that the 'DisplayText' property of 'RemoteServerUITestWorkflow' label equals 'RemoteServerUITestWorkflow'
            Assert.AreEqual(workflowName, text.DisplayText, "Dependant workflow not shown in dependency diagram");

            // Verify that the 'Exists' property of 'Show what depends on this' radio button equals 'True'
            Assert.AreEqual(this.Select_Show_Dependencies_In_Explorer_Context_MenuParams.ShowwhatdependsonthisRadioButtonExists, showwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");

            // Verify that the 'Selected' property of 'Show what depends on this' radio button equals 'True'
            Assert.AreEqual(this.Select_Show_Dependencies_In_Explorer_Context_MenuParams.ShowwhatdependsonthisRadioButtonSelected1, showwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies" +
                    " is selected");
        }

        public virtual Select_Show_Dependencies_In_Explorer_Context_MenuParams Select_Show_Dependencies_In_Explorer_Context_MenuParams
        {
            get
            {
                if ((this.mSelect_Show_Dependencies_In_Explorer_Context_MenuParams == null))
                {
                    this.mSelect_Show_Dependencies_In_Explorer_Context_MenuParams = new Select_Show_Dependencies_In_Explorer_Context_MenuParams();
                }
                return this.mSelect_Show_Dependencies_In_Explorer_Context_MenuParams;
            }
        }

        private Select_Show_Dependencies_In_Explorer_Context_MenuParams mSelect_Show_Dependencies_In_Explorer_Context_MenuParams;

        /// <summary>
        /// Drag_Dice_Onto_DesignSurface - Use 'Drag_Dice_Onto_DesignSurfaceParams' to pass parameters into this method.
        /// </summary>
        public void Drag_Dice_Onto_DesignSurface()
        {
            #region Variable Declarations
            WpfTreeItem firstItem = this.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            WpfCustom flowchart = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart;
            WpfButton doneButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow.DoneButton;
            #endregion

            // Click 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' -> 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item
            Mouse.Click(firstItem, new Point(49, 10));

            // Move 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' -> 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item to 'Flowchart' custom control
            flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(firstItem, new Point(49, 10));
            Mouse.StopDragging(flowchart, new Point(308, 127));

            // Verify that the 'Exists' property of 'Done' button equals 'True'
            Assert.AreEqual(this.Drag_Dice_Onto_DesignSurfaceParams.DoneButtonExists, doneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");

            // Click 'Done' button
            Mouse.Click(doneButton, new Point(53, 16));
        }

        public virtual Drag_Dice_Onto_DesignSurfaceParams Drag_Dice_Onto_DesignSurfaceParams
        {
            get
            {
                if ((this.mDrag_Dice_Onto_DesignSurfaceParams == null))
                {
                    this.mDrag_Dice_Onto_DesignSurfaceParams = new Drag_Dice_Onto_DesignSurfaceParams();
                }
                return this.mDrag_Dice_Onto_DesignSurfaceParams;
            }
        }

        private Drag_Dice_Onto_DesignSurfaceParams mDrag_Dice_Onto_DesignSurfaceParams;

        public virtual AssertMethod1ExpectedValues AssertMethod1ExpectedValues
        {
            get
            {
                if ((this.mAssertMethod1ExpectedValues == null))
                {
                    this.mAssertMethod1ExpectedValues = new AssertMethod1ExpectedValues();
                }
                return this.mAssertMethod1ExpectedValues;
            }
        }

        private AssertMethod1ExpectedValues mAssertMethod1ExpectedValues;

        /// <summary>
        /// UnCheck_Public_Contribute - Use 'Click_Public_ContributeParams' to pass parameters into this method.
        /// </summary>
        public void UnCheck_Public_Contribute()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            // Select 'UI_Public_ContributePermissionCheckBox_AutoID' check box
            public_ViewCheckBox.Checked = false;

            // Verify that the 'Checked' property of 'UI_Public_ViewPermissionCheckBox_AutoID' check box equals 'True'
            Assert.IsFalse(public_ViewCheckBox.Checked, "Public View checkbox is checked after Checking Contribute.");

            // Verify that the 'Checked' property of 'UI_Public_ExecutePermissionCheckBox_AutoID' check box equals 'True'
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
            Assert.IsFalse(public_ContributeCheckBox.Checked, "Public Contribute checkbox is checked after UnChecking Execute/View.");
            Assert.IsFalse(public_AdministratorCheckBox.Checked, "Public Administrator checkbox is checked after UnChecking Contribute.");
        }
           
        /// <summary>
        /// Check_Public_Contribute - Use 'Click_Public_ContributeParams' to pass parameters into this method.
        /// </summary>
        public void Check_Public_Contribute()
        {
            #region Variable Declarations
            WpfCheckBox public_AdministratorCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox;
            WpfCheckBox public_ContributeCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox;
            WpfCheckBox public_ViewCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox;
            WpfCheckBox public_ExecuteCheckBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox;
            #endregion

            // Select 'UI_Public_ContributePermissionCheckBox_AutoID' check box
            public_ContributeCheckBox.Checked = true;

            // Verify that the 'Checked' property of 'UI_Public_ViewPermissionCheckBox_AutoID' check box equals 'True'
            Assert.IsTrue(public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Contribute.");

            // Verify that the 'Checked' property of 'UI_Public_ExecutePermissionCheckBox_AutoID' check box equals 'True'
            Assert.IsTrue(public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Contribute.");
        }
    }
    /// <summary>
    /// Parameters to be passed into 'Select_Show_Dependencies_In_Explorer_Context_Menu'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_Show_Dependencies_In_Explorer_Context_MenuParams
    {

        #region Fields
        /// <summary>
        /// Verify that the 'Selected' property of 'Show what depends on this' radio button equals 'True'
        /// </summary>
        public bool ShowwhatdependsonthisRadioButtonSelected = true;

        /// <summary>
        /// Verify that the 'Exists' property of first text box next to 'Nesting Levels' label equals 'True'
        /// </summary>
        public bool TextboxExists = true;

        /// <summary>
        /// Verify that the 'Exists' property of 'Refresh' button equals 'True'
        /// </summary>
        public bool RefreshButtonExists = true;

        /// <summary>
        /// Verify that the 'DisplayText' property of 'RemoteServerUITestWorkflow' label equals 'RemoteServerUITestWorkflow'
        /// </summary>
        public string TextDisplayText = "RemoteServerUITestWorkflow";

        /// <summary>
        /// Verify that the 'Exists' property of 'Show what depends on this' radio button equals 'True'
        /// </summary>
        public bool ShowwhatdependsonthisRadioButtonExists = true;

        /// <summary>
        /// Verify that the 'Selected' property of 'Show what depends on this' radio button equals 'True'
        /// </summary>
        public bool ShowwhatdependsonthisRadioButtonSelected1 = true;
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'Drag_Dice_Onto_DesignSurface'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Drag_Dice_Onto_DesignSurfaceParams
    {

        #region Fields
        /// <summary>
        /// Verify that the 'Exists' property of 'Done' button equals 'True'
        /// </summary>
        public bool DoneButtonExists = true;
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'Select_NewFolder_From_ExplorerContextMenu'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class Select_NewFolder_From_ExplorerContextMenuParams
    {

        #region Fields
        /// <summary>
        /// Type 'NewFolder' in first text box next to 'ResourceImage' image
        /// </summary>
        public string UIItemEditText = "NewFolder";
        #endregion
    }
    /// <summary>
    /// Parameters to be passed into 'AssertMethod1'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "14.0.23107.0")]
    public class AssertMethod1ExpectedValues
    {

        #region Fields
        /// <summary>
        /// Verify that the 'Exists' property of 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' -> 'Infragistics.Controls.Menus.XamDataTreeNodeDataCon...' tree item equals 'True'
        /// </summary>
        public bool UIInfragisticsControlsTreeItem1Exists = true;

        /// <summary>
        /// Verify that the 'Exists' property of 'XamRichTextEditor' custom control equals 'True'
        /// </summary>
        public bool HelpTextEditorExists = true;
        #endregion
    }
}
