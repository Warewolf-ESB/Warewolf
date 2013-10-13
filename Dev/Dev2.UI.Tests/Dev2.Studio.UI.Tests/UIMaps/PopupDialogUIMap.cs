using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    class PopupDialogUIMap
    {
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public static bool WaitForDialog(int timeOut)
        {
            var uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
            Type type = null;
            var timeNow = 0;
            while(type != typeof(WpfWindow))
            {
                timeNow = timeNow + 100;
                Playback.Wait(100);
                var tryGetDialog = uiBusinessDesignStudioWindow.GetChildren()[0];
                type = tryGetDialog.GetType();
                if(timeNow > timeOut)
                {
                    break;
                }
            }
            return type == typeof(WpfWindow);
        }
    }
}
