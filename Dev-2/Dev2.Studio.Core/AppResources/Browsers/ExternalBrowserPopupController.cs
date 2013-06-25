using System;
using System.Diagnostics;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // BUG 9798 - 2013.06.25 - TWR : added
    public class ExternalBrowserPopupController : BrowserPopupControllerAbstract
    {
        public override bool ShowPopup(string url)
        {
            if(!string.IsNullOrEmpty(url))
            {
                Process.Start(url);
            }
            return true;
        }

        public override IntPtr FindPopup()
        {
            return IntPtr.Zero;
        }

        public override void SetPopupTitle(IntPtr hwnd)
        {
        }

        public override void SetPopupForeground(IntPtr hwnd)
        {
        }

        public override void SetPopupIcon(IntPtr hwnd)
        {
        }
    }
}
