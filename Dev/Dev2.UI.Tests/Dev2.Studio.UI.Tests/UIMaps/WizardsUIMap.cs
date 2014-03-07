using System;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    // ReSharper disable InconsistentNaming
    public class WizardsUIMap : UIMapBase
    // ReSharper restore InconsistentNaming
    {
        private const int DefaultTimeOut = 5000;
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForWizard(int timeOut = DefaultTimeOut, bool throwIfNotFound = true)
        {
            Playback.Wait(timeOut);
        }


        public bool TryWaitForWizard(int timeOut = 10000)
        {
            Playback.Wait(timeOut);
            var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[2];
            var type = tryGetDialog.GetType();
            return type == typeof(WpfImage);
        }

        public string GetLeftTitleText()
        {
            return GetTitleLabel("LeftTitle").DisplayText;
        }

        public string GetRightTitleText()
        {
            return GetTitleLabel("RightTitle").DisplayText;
        }

        private WpfText GetTitleLabel(string autoId)
        {
            UITestControlCollection uiTestControlCollection = StudioWindow.GetChildren();
            var tryGetDialog = uiTestControlCollection[0];
            VisualTreeWalker visualTreeWalker = new VisualTreeWalker();
            UITestControl childByAutomationIDPath = visualTreeWalker.GetChildByAutomationIDPath(tryGetDialog, autoId);
            WpfText wpfText = childByAutomationIDPath as WpfText;
            if(wpfText != null)
            {
                return wpfText;
            }
            throw new Exception("Could not find the " + autoId + " label.");
        }

    }
}
