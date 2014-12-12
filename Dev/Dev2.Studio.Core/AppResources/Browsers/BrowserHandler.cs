
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using CefSharp;
using Dev2.Studio.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added   
    public class BrowserHandler : ILoadHandler, ILifeSpanHandler, IRequestHandler
    {
        #region CTOR

        public BrowserHandler()
            : this(new ExternalBrowserPopupController())  // BUG 9798 - 2013.06.25 - TWR : modified to always popup out externally
        {
        }

        public BrowserHandler(IBrowserPopupController popupController)
        {
            if(popupController == null)
            {
                throw new ArgumentNullException("popupController");
            }
            PopupController = popupController;
        }

        #endregion

        public bool IsPopping { get; private set; }

        public IBrowserPopupController PopupController { get; private set; }

        #region Implementation of ILoadHandler

        // PBI 9512 - 2013.06.07 - TWR: added
        // PBI 9644 - 2013.06.21 - TWR: merged
        public bool OnLoadError(IWebBrowser browser, string url, int errorCode, ref string errorText)
        {
            if(string.IsNullOrEmpty(url) || url.ToLower().Contains("studiohomepage"))
            {
                ShowErrorPage(browser, Warewolf.Studio.Resources.Languages.Core.Uri_Studio_PageNotAvailable);
            }
            else
            {
                ShowErrorPage(browser, Warewolf.Studio.Resources.Languages.Core.Uri_Studio_PageMissing);
            }
            return true;
        }

        static void ShowErrorPage(IWebBrowser browser, string pageUri)
        {
                var path = FileHelper.GetFullPath(pageUri);
                browser.Load(path);
        }

        #endregion

        #region Implementation of ILifeSpanHandler

        public bool OnBeforePopup(IWebBrowser browser, string url, ref int x, ref int y, ref int width, ref int height)
        {
            // BUG 9798 - 2013.06.25 - TWR : modified to always popup out externally
            if(PopupController.ShowPopup(url))
            {
                return true;
            }

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
            switch(status)
            {
                case 401:
                case 403:
                    ShowErrorPage(browser, Warewolf.Studio.Resources.Languages.Core.Uri_Studio_PageRestrictedAccess);
                    return;
            }

            if(IsPopping)
            {
                IsPopping = false;
                PopupController.ConfigurePopup();
            }
        }

        public bool GetDownloadHandler(IWebBrowser browser, string mimeType, string fileName, long contentLength, ref IDownloadHandler handler)
        {
            return false;
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme, ref string username, ref string password)
        {
            return "ntlm".Equals(scheme, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

    }
}
