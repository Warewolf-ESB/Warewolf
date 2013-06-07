using CefSharp;
using Dev2.Studio.Core.Helpers;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    public class BrowserLoadHandler : ILoadHandler
    {
        public bool OnLoadError(IWebBrowser browser, string url, int errorCode, ref string errorText)
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_PageNotFound);
            browser.Load(path);
            return true;
        }
    }
}