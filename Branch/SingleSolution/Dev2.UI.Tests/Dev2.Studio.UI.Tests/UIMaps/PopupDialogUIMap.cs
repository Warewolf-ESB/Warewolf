using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public class PopupDialogUIMap : UIMapBase
    {
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public void WaitForDialog()
        {
            Playback.Wait(2500);
        }
    }
}
