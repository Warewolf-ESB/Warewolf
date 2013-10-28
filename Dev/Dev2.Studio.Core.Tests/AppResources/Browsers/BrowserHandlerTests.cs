using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CefSharp;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added    
    // PBI 9512 - 2013.06.07 - TWR: merged
    // BUG 9798 - 2013.06.25 - TWR : refactored for external
    [TestClass][ExcludeFromCodeCoverage]
    public class BrowserHandlerTests
    {
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BrowserHandlerConstructorWithNullExpectedThrowsArgumentNullException()
        {
            var handler = new BrowserHandler(null);
        }

        [TestMethod]
        public void BrowserHandlerConstructorWithNoArgsExpectedCreatesExternalBrowserPopupController()
        {
            var handler = new BrowserHandler();
            Assert.IsInstanceOfType(handler.PopupController, typeof(ExternalBrowserPopupController));
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
        public void BrowserHandlerOnBeforePopupWithControllerShowPopupReturningFalseExpectedSetsIsPoppingToTrueAndReturnsFalse()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Returns(false).Verifiable();

            var x = int.MinValue;
            var y = int.MinValue;
            var width = int.MinValue;
            var height = int.MinValue;

            var handler = new BrowserHandler(popupController.Object);
            var result = handler.OnBeforePopup(browser.Object, "myfake.url", ref x, ref y, ref width, ref height);

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()), Times.Once());
            Assert.IsTrue(handler.IsPopping);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BrowserHandlerOnBeforePopupWithControllerShowPopupReturningTrueExpectedSetsIsPoppingToFalseAndReturnsTrue()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Returns(true).Verifiable();

            var x = int.MinValue;
            var y = int.MinValue;
            var width = int.MinValue;
            var height = int.MinValue;

            var handler = new BrowserHandler(popupController.Object);
            var result = handler.OnBeforePopup(browser.Object, "myfake.url", ref x, ref y, ref width, ref height);

            popupController.Verify(p => p.ShowPopup(It.IsAny<string>()), Times.Once());
            Assert.IsFalse(handler.IsPopping);
            Assert.IsTrue(result);
        }

        #endregion

        #region OnResourceResponse

        [TestMethod]
        public void BrowserHandlerOnResourceResponseWithIsPoppingTrueExpectedInvokesConfigurePopupAndResetsIsPopping()
        {
            var browser = new Mock<IWebBrowser>();
            var popupController = new Mock<IBrowserPopupController>();
            popupController.Setup(p => p.ShowPopup(It.IsAny<string>())).Returns(false);
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
