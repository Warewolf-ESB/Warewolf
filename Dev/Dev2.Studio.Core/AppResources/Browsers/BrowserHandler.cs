using System;
using System.Net;
using CefSharp;
using Dev2.Studio.Core.Helpers;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added   
    public class BrowserHandler : ILoadHandler, ILifeSpanHandler, IRequestHandler
    {
        readonly IBrowserPopupController _popupController;

        #region CTOR

        public BrowserHandler()
            : this(new BrowserPopupController())
        {
        }

        public BrowserHandler(IBrowserPopupController popupController)
        {
            if(popupController == null)
            {
                throw new ArgumentNullException("popupController");
            }
            _popupController = popupController;
        }

        #endregion

        public bool IsPopping { get; private set; }

        #region Implementation of ILoadHandler

        // PBI 9512 - 2013.06.07 - TWR: added
        // PBI 9644 - 2013.06.21 - TWR: merged
        public bool OnLoadError(IWebBrowser browser, string url, int errorCode, ref string errorText)
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_PageNotFound);
            browser.Load(path);
            return true;
        }

        #endregion

        #region Implementation of ILifeSpanHandler

        public bool OnBeforePopup(IWebBrowser browser, string url, ref int x, ref int y, ref int width, ref int height)
        {
            IsPopping = true;
            return false;
        }

        public void OnBeforeClose(IWebBrowser browser)
        {
        }

        #endregion

        #region Implementation of IRequestHandler

        public bool OnBeforeBrowse(IWebBrowser browser, IRequest request, NavigationType naigationvType, bool isRedirect)
        {
            return false;
        }

        public bool OnBeforeResourceLoad(IWebBrowser browser, IRequestResponse requestResponse)
        {
            return false;
        }

        public void OnResourceResponse(IWebBrowser browser, string url, int status, string statusText, string mimeType, WebHeaderCollection headers)
        {
            if(IsPopping)
            {
                IsPopping = false;
                _popupController.ConfigurePopup();
            }
        }

        public bool GetDownloadHandler(IWebBrowser browser, string mimeType, string fileName, long contentLength, ref IDownloadHandler handler)
        {
            return false;
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme, ref string username, ref string password)
        {
            return false;
        }

        #endregion

    }
}
