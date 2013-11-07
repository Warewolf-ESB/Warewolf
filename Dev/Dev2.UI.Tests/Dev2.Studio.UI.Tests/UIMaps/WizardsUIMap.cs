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
            var type = StudioWindow.GetChildren()[0].GetChildren()[0].GetType();
            const int interval = 100;
            var timeNow = 0;
            while(type != typeof(WpfImage))
            {
                Playback.Wait(interval);
                timeNow = timeNow + interval;
                if(timeNow > timeOut)
                {
                    break;
                }
                var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
                type = tryGetDialog.GetType();
            }
            if(type != typeof(WpfImage) && throwIfNotFound)
            {
                throw new UITestControlNotFoundException("Popup dialog not displayed within the given time out period.");
            }
            //wait for render
            Playback.Wait(20000);
        }

        public bool TryWaitForWizard(int timeOut)
        {
            WaitForWizard(timeOut, false);
            var tryGetDialog = StudioWindow.GetChildren()[0].GetChildren()[0];
            var type = tryGetDialog.GetType();
            return type == typeof (WpfImage);
        }
    }
}
