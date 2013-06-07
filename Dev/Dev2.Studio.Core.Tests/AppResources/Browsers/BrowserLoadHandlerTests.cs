using CefSharp;
using CefSharp.Wpf;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    [TestClass]
    public class BrowserLoadHandlerTests
    {
        [TestMethod]
        public void BrowserLoadHandlerOnLoadErrorExpectedRedirectsToPageNotFound()
        {
            var browser = new Mock<IWebBrowser>();
            browser.Setup(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotFound)))).Verifiable();

            var handler = new BrowserLoadHandler();
            var errorText = "Not found";
            var result = handler.OnLoadError(browser.Object, "myfake.url", 404, ref errorText);
            Assert.IsTrue(result);
            browser.Verify(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotFound))), Times.Once());
        }

        [TestMethod]
        public void BrowserLoadSafeExpectedAttachesBrowserLoadHandler()
        {
            var browser = new WebView();
            browser.LoadSafe("myfake.url");
            Assert.IsNotNull(browser.LoadHandler);
        }
    }
}
