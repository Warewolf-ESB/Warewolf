using System;
using CefSharp;
using CefSharp.Wpf;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added    
    [TestClass]
    public class BrowserLifeSpanHandlerTests
    {
        [TestMethod]
        public void BrowserLoadSafeExpectedAttachesBrowserLifeSpanHandler()
        {
            var browser = new WebView();
            browser.LoadSafe("myfake.url");
            Assert.IsNotNull(browser.LifeSpanHandler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BrowserLifeSpanHandlerConstructorWithNullExpectedThrowsArgumentNullException()
        {
            var handler = new BrowserLifeSpanHandler(null);
        }


        [TestMethod]
        public void BrowserLifeSpanHandlerOnBeforePopupExpectedInvokesPopupControllerShow()
        {
            var browser = new Mock<IWebBrowser>();

            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Verifiable();

            var x = int.MinValue;
            var y = int.MinValue;
            var width = int.MinValue;
            var height = int.MinValue;

            var handler = new BrowserLifeSpanHandler(popupController.Object);
            var result = handler.OnBeforePopup(browser.Object, "myfake.url", ref x, ref y, ref width, ref height);
            Assert.IsTrue(result);

            popupController.Verify(c => c.Show(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
        }
    }
}
