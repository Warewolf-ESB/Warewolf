using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class WizardsUIMap : UIMapBase
    {
        private const int DefaultTimeOut = 5000;
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForWizard(int timeOut = DefaultTimeOut, bool throwIfNotFound = true)
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Playback.Wait(100);
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
        }


        public bool TryWaitForWizard(int timeOut)
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
            var type = tryGetDialog.GetType();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            return type == typeof(WpfImage);
        }
    }
}
