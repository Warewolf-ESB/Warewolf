using System;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    public abstract class BrowserPopupControllerAbstract : IBrowserPopupController
    {

        protected BrowserPopupControllerAbstract(string popupTitle)
        {
            PopupTitle = popupTitle;
        }

        public string PopupTitle { get; private set; }

        public void ConfigurePopup()
        {
            var hwnd = FindPopup();
            if(hwnd != IntPtr.Zero)
            {
                SetPopupForeground(hwnd);
                SetPopupTitle(hwnd);
                SetPopupIcon(hwnd);
            }
        }


        protected abstract IntPtr FindPopup();

        protected abstract void SetPopupTitle(IntPtr hwnd);

        protected abstract void SetPopupForeground(IntPtr hwnd);

        protected abstract void SetPopupIcon(IntPtr hwnd);
    }
}
