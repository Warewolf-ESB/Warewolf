using System;
using System.Linq;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps.Scheduler
{
    public class SchedulerUiMap : UIMapBase, IDisposable
    {
        readonly UITestControl _activeTab;
        readonly string[] _newButtonAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "New" };
        readonly string[] _nameTextboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_NameTextbox" };
        private readonly string[] _connectControl = new[] { "Uia.ContentPane", "Uia.SchedulerView", "ConnectUserControl" };
        readonly string[] _enabledRadioButtonAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_EnabledRadioButton" };
        readonly string[] _workflowNameTextBoxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_WorkflowNameTextBox" };
        readonly string[] _runAsapCheckboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_RunAsapCheckBox" };
        readonly string[] _usernameTextboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_UserNameTextBox" };

        public SchedulerUiMap()
        {
            RibbonUIMap.SchedulerShortcutKeyPress();
            _activeTab = TabManagerUIMap.GetActiveTab();
            Playback.Wait(1500);
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

        public void ClickNewTaskButton()
        {
            UITestControl newButton = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _newButtonAutoIds);
            newButton.Click();
        }

        public string GetNameText()
        {
            UITestControl nameTextbox = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _nameTextboxAutoIds);
            return nameTextbox.GetText();
        }

        public string GetStatus()
        {
            UITestControl statusRadioButton = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _enabledRadioButtonAutoIds);
            if(statusRadioButton.IsSelected())
            {
                return "Enabled";
            }
            return "Disabled";
        }

        public string GetWorkflowName()
        {
            UITestControl workflowNameTextbox = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _workflowNameTextBoxAutoIds);
            return workflowNameTextbox.GetText();
        }

        public bool GetRunAsap()
        {
            UITestControl runAsapCheckbox = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _runAsapCheckboxAutoIds);
            return runAsapCheckbox.IsChecked();
        }

        public string GetUsername()
        {
            UITestControl usernameTextbox = VisualTreeWalker.GetChildByAutomationIDPath(_activeTab, _usernameTextboxAutoIds);
            return usernameTextbox.GetText();
        }

        public void ChooseServerWithKeyboard(string connection)
        {
            UITestControl serverList = GetConnectControl("ComboBox");
            Mouse.Click(serverList);
            Keyboard.SendKeys("{UP}{UP}{UP}{UP}{UP}{UP}{UP}{ENTER}");
            Playback.Wait(2000);
        }

        public UITestControl GetConnectControl(string controlType)
        {
            var kids = _activeTab.GetChildByAutomationIDPath(_connectControl).GetChildren();

            if(kids != null)
            {
                return kids.FirstOrDefault(c => c.ControlType.Name == controlType);
            }

            return null;
        }

        public UITestControl GetConnectControl(string controlType, string controlName)
        {
            var kids = _activeTab.GetChildByAutomationIDPath(_connectControl).GetChildren();

            if(kids != null)
            {
                return kids.FirstOrDefault(c => c.ControlType.Name == controlType && c.FriendlyName == controlName);
            }

            return null;
        }

        public void ChooseServer(string connection)
        {
            UITestControl destinationServerList = GetConnectControl("ComboBox");
            WpfComboBox comboBox = (WpfComboBox)destinationServerList;
            var serverItem = comboBox.Items.ToList().FirstOrDefault(c => c.FriendlyName == connection);
            if(serverItem != null)
            {
                comboBox.SelectedIndex = comboBox.Items.IndexOf(serverItem);
            }
        }
    }
}
