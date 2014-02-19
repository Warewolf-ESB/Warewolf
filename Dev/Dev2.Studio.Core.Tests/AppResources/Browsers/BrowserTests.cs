using System.Diagnostics.CodeAnalysis;
using CefSharp.Wpf;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BrowserTests
    {
        [TestMethod]
        public void BrowserLoadSafeExpectedAttachesBrowserHandler()
        {
            var browser = new WebView();
            browser.LoadSafe("myfake.url");
            Assert.IsNotNull(browser.LoadHandler);
            Assert.IsNotNull(browser.LifeSpanHandler);
            Assert.IsNotNull(browser.RequestHandler);
        }
    }
}
