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
using TechTalk.SpecFlow;
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

        [Given("The Warewolf Studio is running")]
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
            try
            {
                controlExists = thisControl.Exists;
            }
            finally
            {
                OnErrorHandlerDisabled = false;
                Playback.PlaybackError += OnError;
                Playback.PlaybackSettings.MaximumRetryCount = _lenientMaximumRetryCount;
                Playback.PlaybackSettings.SearchTimeout = _lenientSearchTimeout;
            }
            return controlExists;
        }

        [When(@"I Try Click Message Box OK")]
        public void TryClickMessageBoxOK()
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

        [When(@"I Try Clear Toolbox Filter")]
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

        public void TryCloseWorkflowTestingTab()
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

        [When(@"I Try Close Settings Tab")]
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
            Enter_Service_Name_Into_Save_Dialog(ServiceName, true, true, true, SaveOrDuplicate.Duplicate);
        }

        [When(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, true, true, false, SaveOrDuplicate.Duplicate);
        }

        [When(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            Enter_Service_Name_Into_Save_Dialog(ServiceName, true, false, false, SaveOrDuplicate.Duplicate);
        }

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

        [When(@"I Filter the Explorer with ""(.*)""")]
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

        [When(@"I Select ""(.*)"" From Explorer Remote Server Dropdown List")]
        public void Select_From_Explorer_Remote_Server_Dropdown_List(WpfText comboboxListItem)
        {
            Select_From_Explorer_Remote_Server_Dropdown_List(comboboxListItem);
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

        [When(@"I Select NewRemoteServer From Explorer Server Dropdownlist")]
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

        [When(@"I Save With Ribbon Button and Dialog As ""(.*)"" and Append Unique Guid")]
        public void WhenISaveWithRibbonButtonAndDialogAsAndAppendUniqueGuid(string p0)
        {
            Save_With_Ribbon_Button_And_Dialog(p0 + Guid.NewGuid().ToString().Substring(0, 8));
        }

        [When(@"I Save With Ribbon Button And Dialog As ""(.*)""")]
        public void WhenISaveWithRibbonButtonAndDialogAs(string Name)
        {
            Save_With_Ribbon_Button_And_Dialog(Name);
        }

        [When(@"I Save With Ribbon Button And Dialog As ""(.*)"" without filtering the explorer")]
        public void Save_With_Ribbon_Button_And_Dialog_Without_Filtering(string name)
        {
            Save_With_Ribbon_Button_And_Dialog(name, true);
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

        [When("I Click New Workflow Ribbon Button")]
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

        [When(@"I Select Last Source From GET Web Large View Source Combobox")]
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

        [When(@"I Click New Web Source Ribbon Button")]
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
            if (MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2.InputValueCell.InputValueComboboxl.InputValueText.Text != text)
            {
                MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row2.InputValueCell.InputValueComboboxl.InputValueText.Text = text;
            }
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.PluginSourceWizardTab.WorkSurfaceContext.AssemblyNameTextbox.Text = text;
            Assert.IsTrue(MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after DLL has been selected in plugin source wizard.");
        }

        public void Enter_GroupName_Into_Settings_Dialog_Resource_Permissions_Row1_Windows_Group_Textbox(string GroupName)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text = GroupName;
            Assert.AreEqual(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.WindowsGroupCell.AddWindowsGroupsEdit.Text, GroupName, "Settings security tab resource permissions row 1 windows group textbox text does not equal Public.");
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
            CreateRemoteServerSource(ServerSourceName, ServerAddress);
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

        [When(@"I Click Deploy Ribbon Button")]
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
            WpfMenuItem showDependencies = this.MainStudioWindow.ExplorerContextMenu.ShowDependencies;
            WpfRadioButton showwhatdependsonthisRadioButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox;
            WpfButton refreshButton = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton;
            WpfText text = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.Node1.Text;
            #endregion

            Filter_Explorer(ServiceName);
            RightClick_Explorer_Localhost_First_Item();
            Mouse.Click(showDependencies, new Point(50, 15));
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(refreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.AreEqual(ServiceName, text.DisplayText, "Dependant workflow not shown in dependency diagram");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");
            Assert.IsTrue(showwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies" +
                    " is selected");
        }

        [When(@"I Click DB Source Wizard Test Connection Button")]
        public void Click_DB_Source_Wizard_Test_Connection_Button()
        {
            var point = new Point();
            Assert.IsTrue(!MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.TestConnectionButton, new Point(21, 16));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.DBSourceWizardTab.WorkSurfaceContext.ManageDatabaseSourceControl.DatabaseCombobox.TryGetClickablePoint(out point), "Database Combobox does not exist");
        }

        [When(@"I Deploy ""(.*)"" From Deploy View")]
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

        [When(@"I Update Test Name To ""(.*)""")]
        public void Update_Test_Name(string overrideName = null)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameTextbox, new Point(59, 16));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameTextbox.Text = "";
            if (!string.IsNullOrEmpty(overrideName))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameTextbox.Text = overrideName;
            else
                MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameTextbox.Text = "Dice_Test";
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.DuplicateButton, new Point(14, 10));
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
            WpfText testNameText = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestNameText;
            WpfEdit textbox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox;
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList;
            #endregion

            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));

            var currentTest = GetCurrentTest(testInstance);
            var testEnabledSelector = GetTestRunState(testInstance, currentTest).Checked;
            var testNeverRun = GetSelectedTestNeverRunDisplay(currentTest, testInstance);

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
            Click_Save_Ribbon_Button_With_No_Save_Dialog();
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
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.Listbox.Textbox.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.ValueCell.AssignValueCombobox.TextEdit.Text = "5";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.VariableCell.Listbox.Textbox.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.ValueCell.AssignValueCombobox.TextEdit.Text = "10";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.VariableCell.Listbox.Textbox.Text = "[[var]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.ValueCell.AssignValueCombobox.TextEdit.Text = "1";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.VariableCell.Listbox.Textbox.Text = "[[mr()]]";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.Exists, "var does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.Exists, "rec().a does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.Exists, "rec().b does not exist in the variable explorer");
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.TestButton.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.DoneButton.Exists);
        }

        [When(@"I Click New Web Source Test Connection Button")]
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

        [When("I Click Subworkflow Done Button")]
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

        [When(@"I Refresh Explorer")]
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void TryRemoveTests()
        {
            WpfList testsListBox = this.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList;
            if (testsListBox.GetContent().Length >= 4)
            {
                Select_Test(3);
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector.Checked)
                    Click_EnableDisable_This_Test_CheckBox(true, 3);
                Click_Delete_Test_Button(3);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 3)
            {
                Select_Test(2);
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector.Checked)
                    Click_EnableDisable_This_Test_CheckBox(true, 2);
                Click_Delete_Test_Button(2);
                Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 2)
            {
                Select_Test();
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector.Checked)
                    Click_EnableDisable_This_Test_CheckBox(true);
                Click_Delete_Test_Button();
                Click_MessageBox_Yes();
            }
            Click_Close_Tests_Tab();
        }

        [When(@"I Click View Tests In Explorer Context Menu for ""(.*)""")]
        public void Click_View_Tests_In_Explorer_Context_Menu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            Open_Explorer_First_Item_Tests_With_Context_Menu();
        }

        [Given(@"That The First Test ""(.*)"" Unsaved Star")]
        [Then(@"The First Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(bool HasStar)
        {
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.TestNameDisplay.DisplayText.Contains("*"), "First test title does not contain unsaved star.");
        }

        [Given(@"That The Second Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2.TestNameDisplay.DisplayText.Contains("*"), "Second test title does not contain unsaved star.");
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        [Given("The First Test Exists")]
        [Then("The First Test Exists")]
        public void Assert_Workflow_Testing_Tab_First_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Second Test Exists")]
        [Then("The Second Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test2.Exists, "No second test on workflow testing tab.");
        }

        [When("I Toggle First Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector, new Point(10, 10));
        }

        [When("I Click First Test Run Button")]
        public void Click_First_Test_Run_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.RunButton, new Point(10, 10));
        }

        [When("I Click First Test Delete Button")]
        public void Click_First_Test_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.DeleteButton, new Point(10, 10));
        }

        [When("I Click Run All Button")]
        public void Click_Workflow_Testing_Tab_Run_All_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.RunAllButton, new Point(35, 10));
        }

        [Given(@"That The First Test ""(.*)"" Passing")]
        [Then(@"The First Test ""(.*)"" Passing")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Passing(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(bool passing = true)
        {
            Point point;
            Assert.AreEqual(passing, MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Passing.TryGetClickablePoint(out point), (passing ? "First test is not passing." : "First test is passing."));
        }

        [Given(@"That The First Test ""(.*)"" Invalid")]
        [Then(@"The First Test ""(.*)"" Invalid")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(bool invalid = true)
        {
            Point point;
            Assert.AreEqual(invalid, MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.TestsTabPage.ServiceTestView.TestsListboxList.Test1.Invalid.TryGetClickablePoint(out point), (invalid ? "First test is not invalid." : "First test is invalid."));
        }

        public void Delete_Assign_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on design surface after deleting with context menu.");
        }

        public void Debug_Workflow_With_Ribbon_Button()
        {
            Click_Debug_Ribbon_Button();
            Click_DebugInput_Debug_Button();
            WaitForSpinner(MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        public void Remove_Assign_Row_1_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            Mouse.Click(MainStudioWindow.DesignSurfaceContextMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1.");
        }

        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2Params.TextboxText = VariableText;
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2Params.TextEditText = ValueText;
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1Params.TextboxText = VariableText;
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1Params.TextEditText = ValueText;
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1.");
                    break;
            }
        }

        public void Pin_Unpinned_Pane_To_Default_Position()
        {
            Mouse.StartDragging(MainStudioWindow.ToolWindow, new Point(5, 5));
            Mouse.StopDragging(MainStudioWindow.ToolWindow);
        }

        public void Unpin_Tab_With_Drag(UITestControl Tab)
        {
            Mouse.StartDragging(Tab);
            Mouse.StopDragging(25, 40);
        }
    }
}
