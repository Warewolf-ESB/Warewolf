using System;
using System.Net;
using CefSharp;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added    
    // PBI 9512 - 2013.06.07 - TWR: merged
    [TestClass]
    public class BrowserHandlerTests
    {
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BrowserHandlerConstructorWithNullExpectedThrowsArgumentNullException()
        {
            var handler = new BrowserHandler(null);
        }

        #endregion

        #region OnLoadError

        [TestMethod]
        public void BrowserHandlerOnLoadErrorExpectedRedirectsToPageNotFound()
        {
            var browser = new Mock<IWebBrowser>();
            browser.Setup(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotFound)))).Verifiable();

            var popupController = new Mock<IBrowserPopupController>();

            var handler = new BrowserHandler(popupController.Object);
            var errorText = "Not found";
            var result = handler.OnLoadError(browser.Object, "myfake.url", 404, ref errorText);
            Assert.IsTrue(result);
            browser.Verify(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotFound))), Times.Once());
        }

        #endregion

        #region OnBeforePopup

        [TestMethod]
        public void BrowserHandlerOnBeforePopupExpectedSetsIsPoppingToTrueAndReturnsFalse()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();

            var x = int.MinValue;
            var y = int.MinValue;
            var width = int.MinValue;
            var height = int.MinValue;

            var handler = new BrowserHandler(popupController.Object);
            var result = handler.OnBeforePopup(browser.Object, "myfake.url", ref x, ref y, ref width, ref height);

            Assert.IsTrue(handler.IsPopping);
            Assert.IsFalse(result);
        }

        #endregion

        #region OnResourceResponse

        [TestMethod]
        public void BrowserHandlerOnResourceResponseWithIsPoppingTrueExpectedInvokesConfigurePopupAndResetsIsPopping()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(c => c.ConfigurePopup()).Verifiable();

            var x = int.MinValue;
            var y = int.MinValue;
            var width = int.MinValue;
            var height = int.MinValue;

            var handler = new BrowserHandler(popupController.Object);
            handler.OnBeforePopup(browser.Object, "myfake.url", ref x, ref y, ref width, ref height);

            Assert.IsTrue(handler.IsPopping);
            handler.OnResourceResponse(browser.Object, string.Empty, 0, string.Empty, string.Empty, new WebHeaderCollection());

            Assert.IsFalse(handler.IsPopping);
            popupController.Verify(c => c.ConfigurePopup(), Times.Once());
        }

        [TestMethod]
        public void BrowserHandlerOnResourceResponseWithIsPoppingFalseExpectedDoesNotInvokeConfigurePopup()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(c => c.ConfigurePopup()).Verifiable();

            var handler = new BrowserHandler(popupController.Object);

            Assert.IsFalse(handler.IsPopping);

            handler.OnResourceResponse(browser.Object, string.Empty, 0, string.Empty, string.Empty, new WebHeaderCollection());

            Assert.IsFalse(handler.IsPopping);
            popupController.Verify(c => c.ConfigurePopup(), Times.Never());
        }

        #endregion

    }
}
