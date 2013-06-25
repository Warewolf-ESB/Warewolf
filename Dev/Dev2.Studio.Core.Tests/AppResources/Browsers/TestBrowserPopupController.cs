using System;
using Dev2.Studio.Core.AppResources.Browsers;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    public class TestBrowserPopupController : BrowserPopupControllerAbstract
    {
        readonly IntPtr _hwndPopup;

        public TestBrowserPopupController(string popupTitle, int hwndPopup)
            : base(popupTitle)
        {
            _hwndPopup = new IntPtr(hwndPopup);
        }

        public int FindPopupHitCount { get; set; }
        public int SetPopupTitleHitCount { get; set; }
        public int SetPopupForegroundHitCount { get; set; }
        public int SetPopupIconCount { get; set; }

        protected override IntPtr FindPopup()
        {
            FindPopupHitCount++;
            return _hwndPopup;
        }

        protected override void SetPopupTitle(IntPtr hwnd)
        {
            SetPopupTitleHitCount++;
        }

        protected override void SetPopupForeground(IntPtr hwnd)
        {
            SetPopupForegroundHitCount++;
        }

        protected override void SetPopupIcon(IntPtr hwnd)
        {
            SetPopupIconCount++;
        }

    }
}
