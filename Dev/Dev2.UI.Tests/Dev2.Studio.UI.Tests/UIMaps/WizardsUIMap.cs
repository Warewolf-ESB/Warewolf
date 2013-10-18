using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    class WizardsUIMap
    {
        private const int DefaultTimeOut = 5000;
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public static void WaitForWizard(int timeOut = DefaultTimeOut, bool throwIfNotFound = true)
        {
            var uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
            var type = uiBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0].GetType();
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
                var tryGetDialog = uiBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
                type = tryGetDialog.GetType();
            }
            if(type != typeof(WpfImage) && throwIfNotFound)
            {
                throw new UITestControlNotFoundException("Popup dialog not displayed within the given time out period.");
            }
            //wait for render
            Playback.Wait(2000);
        }

        public static bool TryWaitForWizard(int timeOut)
        {
            WaitForWizard(timeOut, false);
            var uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
            var tryGetDialog = uiBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            var type = tryGetDialog.GetType();
            return type == typeof (WpfImage);
        }
    }
}
