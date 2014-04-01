
using System;
using Dev2.Studio.UI.Tests.Extensions;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps.Scheduler
{
    public class SchedulerUiMap : UIMapBase, IDisposable
    {

        UITestControl _activeTab;
        VisualTreeWalker _visualTreeWalker;

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
            UITestControl newButton = _visualTreeWalker.GetChildByAutomationIDPath(_activeTab, new[] { "", "" });
            newButton.Click();
        }

        public string GetNameText()
        {
            return string.Empty;
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public string GetWorkflowName()
        {
            return string.Empty;
        }

        public bool GetRunAsap()
        {
            return false;
        }

        public string GetUsername()
        {
            return string.Empty;
        }

        public string GetPassword()
        {
            return string.Empty;
        }
    }
}
