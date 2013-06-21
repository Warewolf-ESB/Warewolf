using System;
using CefSharp;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added   
    public class BrowserLifeSpanHandler : ILifeSpanHandler
    {
        readonly IBrowserPopupController _popupController;

        public BrowserLifeSpanHandler()
            : this(new BrowserPopupController())
        {
        }

        public BrowserLifeSpanHandler(IBrowserPopupController popupController)
        {
            if(popupController == null)
            {
                throw new ArgumentNullException("popupController");
            }
            _popupController = popupController;
        }

        public bool OnBeforePopup(IWebBrowser browser, string url, ref int x, ref int y, ref int width, ref int height)
        {
            _popupController.Show(url, width, height);
            return true;
        }

        public void OnBeforeClose(IWebBrowser browser)
        {
        }
    }
}
