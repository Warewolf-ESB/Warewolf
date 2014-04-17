using System;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Scheduler
{
    public class SchedulerUiMap : UIMapBase, IDisposable
    {

        UITestControl _activeTab;
        readonly VisualTreeWalker _visualTreeWalker;
        readonly string[] _newButtonAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "New" };
        readonly string[] _nameTextboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_NameTextbox" };
        readonly string[] _enabledRadioButtonAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_EnabledRadioButton" };
        readonly string[] _workflowNameTextBoxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_WorkflowNameTextBox" };
        readonly string[] _runAsapCheckboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_RunAsapCheckBox" };
        readonly string[] _usernameTextboxAutoIds = { "Uia.ContentPane", "Uia.SchedulerView", "Uia.TabControl", "Settings", "UI_UserNameTextBox" };

        public SchedulerUiMap()
        {
            _visualTreeWalker = new VisualTreeWalker();
            RibbonUIMap.SchedulerShortcutKeyPress();
            _activeTab = TabManagerUIMap.GetActiveTab();
            Playback.Wait(3000);
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
            UITestControl newButton = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _newButtonAutoIds);
            newButton.Click();
        }

        public string GetNameText()
        {
            UITestControl nameTextbox = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _nameTextboxAutoIds);
            return nameTextbox.GetText();
        }

        public string GetStatus()
        {
            UITestControl statusRadioButton = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _enabledRadioButtonAutoIds);
            if(statusRadioButton.IsSelected())
            {
                return "Enabled";
            }
            return "Disabled";
        }

        public string GetWorkflowName()
        {
            UITestControl workflowNameTextbox = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _workflowNameTextBoxAutoIds);
            return workflowNameTextbox.GetText();
        }

        public bool GetRunAsap()
        {
            UITestControl runAsapCheckbox = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _runAsapCheckboxAutoIds);
            return runAsapCheckbox.IsChecked();
        }

        public string GetUsername()
        {
            UITestControl usernameTextbox = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, _usernameTextboxAutoIds);
            return usernameTextbox.GetText();
        }
    }
}
