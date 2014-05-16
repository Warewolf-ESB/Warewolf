using System;
using System.Windows.Automation;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Settings
{
    public class SecuritySettingsUiMap : UIMapBase, IDisposable
    {
        readonly UITestControl _activeTab;

        #region Automation Id's

        private const string SaveButtonId = "UI_SaveSettingsbtn_AutoID";
        private const string ResourceViewCheckBoxId = "UI_SecuritySettingResourceViewchk_AutoID";
        private const string ResourceExecuteCheckBoxId = "UI_SecuritySettingResourceExecutechk_AutoID";
        private const string ResourceContributeCheckBoxId = "UI_SecuritySettingResourceContributechk_AutoID";
        private const string ServerHelpButtonId = "ServerHelpToggleButton";
        private const string ResourceHelpButtonId = "ResourceHelpToggleButton";
        private const string HelpViewCloseButtonId = "UI_HelpViewCloseHelpBtn_AutoID";
        #endregion

        #region VisualTreePaths
        private readonly string[] _resourceGridPath = new[] { "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel", "UI_SettingsView_AutoID", "SecurityViewContent", "ResourcePermissionsDataGrid" };
        private readonly string[] _savebuttonPath = new[] { "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel", "UI_SettingsView_AutoID", SaveButtonId };
        private readonly string[] _resourceHelpButtonPath = new[] { "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel", "UI_SettingsView_AutoID", "SecurityViewContent", ResourceHelpButtonId };
        private readonly string[] _serverHelpButtonPath = new[] { "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel", "UI_SettingsView_AutoID", "SecurityViewContent", ServerHelpButtonId };
        private readonly string[] _helpViewCloseButtonPath = new[] { "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel", "UI_SettingsView_AutoID", "SecurityViewContent", HelpViewCloseButtonId };
        #endregion

        public SecuritySettingsUiMap()
        {
            RibbonUIMap.ClickManageSecuritySettings();
            _activeTab = TabManagerUIMap.GetActiveTab();
            Playback.Wait(1500);
        }

        public void AddResource(string resourceName, string category, string folder)
        {
            UITestControl addResourceGrid = _activeTab.GetChildByAutomationIDPath(_resourceGridPath);

            var kids = addResourceGrid.GetChildren();

            var foundIt = false;

            if(kids.Count >= 2)
            {
                var theRow = kids[1];
                var grandKids = theRow.GetChildren();
                if(grandKids.Count > 0)
                {
                    // 0 = header 
                    var magicCell = grandKids[1];
                    var grandGrandKids = magicCell.GetChildren();
                    if(grandGrandKids.Count == 2)
                    {
                        var addResourceButton = grandGrandKids[1];
                        addResourceButton.Click();
                        PopupDialogUIMap.WaitForDialog();
                        PopupDialogUIMap.AddAResource("localhost", category, folder, resourceName);
                        foundIt = true;
                    }
                }
            }

            if(!foundIt)
            {
                throw new Exception("Failed to locate resource picker button");
            }
        }

        public UITestControl SetWindowsGroupText(string groupName)
        {
            UITestControl addWindowsGoupTextBox = FetchSecurityGroupTextbox();

            addWindowsGoupTextBox.Click();
            KeyboardCommands.SelectAll();
            KeyboardCommands.SendDel();
            KeyboardCommands.SendKey(groupName);

            return addWindowsGoupTextBox;
        }

        internal UITestControl FetchSecurityGroupTextbox()
        {
            UITestControl addWindowsGoupTextBox = _activeTab.GetChildByAutomationIDPath(_resourceGridPath);

            var kids = addWindowsGoupTextBox.GetChildren();

            if(kids.Count >= 2)
            {
                var theRow = kids[1];
                var grandKids = theRow.GetChildren();
                if(grandKids.Count > 0)
                {
                    // 0 = header 
                    var magicCell = grandKids[2];
                    var grandGrandKids = magicCell.GetChildren();
                    if(grandGrandKids.Count == 2)
                    {
                        var windowsGroup = grandGrandKids[0];
                        return windowsGroup;
                    }
                }
            }

            throw new Exception("Failed to locate security group text box");
        }

        internal void ClickSaveButton()
        {
            UITestControl saveButton = _activeTab.GetChildByAutomationIDPath(_savebuttonPath);
            saveButton.Click();
        }

        internal bool IsSaveButtonEnabled()
        {
            UITestControl saveButton = _activeTab.GetChildByAutomationIDPath(_savebuttonPath);
            return saveButton.IsEnabled();
        }

        internal UITestControl FetchCheckbox(int boxNumber = 0)
        {
            UITestControl row = _activeTab.GetChildByAutomationIDPath(_resourceGridPath);

            var kids = row.GetChildren();

            if(kids.Count >= 2)
            {
                var theRow = kids[1];
                var grandKids = theRow.GetChildren();
                if(grandKids.Count > 0)
                {
                    // 0 = header 
                    var idx = boxNumber + 3;
                    var magicCell = grandKids[idx];
                    var grandGrandKids = magicCell.GetChildren();
                    if(grandGrandKids.Count == 1)
                    {
                        var viewCheckbox = grandGrandKids[0];
                        return viewCheckbox;
                    }
                }
            }

            throw new Exception("Failed to locate permission checkbox");
        }

        internal void SetViewCheckBox(bool isChecked)
        {
            var checkBox = FetchCheckbox();
            checkBox.Check(isChecked);
        }

        internal void SetExecuteCheckBox(bool isChecked)
        {
            var checkBox = FetchCheckbox(1);
            checkBox.Check(isChecked);
        }

        internal void SetContributeCheckBox(bool isChecked)
        {
            var checkBox = FetchCheckbox(2);
            checkBox.Check(isChecked);
        }

        internal void ToggleServerHelpButton()
        {
            UITestControl control = _activeTab.GetChildByAutomationIDPath(_serverHelpButtonPath);
            control.Click();
        }

        internal void ToggleResourceHelpButton()
        {
            UITestControl control = _activeTab.GetChildByAutomationIDPath(_resourceHelpButtonPath);
            control.Click();
        }

        internal bool IsCloseHelpViewButtonEnabled()
        {
            UITestControl control = _activeTab.GetChildByAutomationIDPath(_helpViewCloseButtonPath);
            return control.IsEnabled();
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        public bool IsResourcePermissionScrollbarVisible()
        {
            UITestControl control = _activeTab.GetChildByAutomationIDPath(_resourceGridPath);
            WpfTable wpfTable = control as WpfTable;
            if(wpfTable != null)
            {
                AutomationElement automationElement = wpfTable.NativeElement as AutomationElement;
                if(automationElement != null)
                {
                    ScrollPattern scrollPattern = automationElement.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
                    if(scrollPattern != null && scrollPattern.Current.VerticalScrollPercent >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}