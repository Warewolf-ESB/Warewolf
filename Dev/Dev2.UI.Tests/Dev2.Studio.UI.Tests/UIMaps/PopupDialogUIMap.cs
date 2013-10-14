using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    class PopupDialogUIMap
    {
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public static void WaitForDialog()
        {
            const int timeOut = 5000;
            var uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
            Type type = uiBusinessDesignStudioWindow.GetChildren()[0].GetType();
            const int interval = 100;
            var timeNow = 0;
            while(type != typeof(WpfWindow))
            {
                Playback.Wait(interval);
                timeNow = timeNow + interval;
                if(timeNow > timeOut)
                {
                    break;
                }
                var tryGetDialog = uiBusinessDesignStudioWindow.GetChildren()[0];
                type = tryGetDialog.GetType();
            }
            if (type != typeof (WpfWindow))
            {
                throw new UITestControlNotFoundException("Popup dialog not displayed within the given time out period.");
            }
        }
    }
}
