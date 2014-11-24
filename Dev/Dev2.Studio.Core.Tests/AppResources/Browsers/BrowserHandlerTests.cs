
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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CefSharp;
using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    // PBI 9644 - 2013.06.21 - TWR: added    
    // PBI 9512 - 2013.06.07 - TWR: merged
    // BUG 9798 - 2013.06.25 - TWR : refactored for external
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BrowserHandlerTests
    {
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BrowserHandlerConstructorWithNullExpectedThrowsArgumentNullException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new BrowserHandler(null);
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
        public void BrowserHandler_OnLoadError_UrlContainsStudioHomePage_RedirectsToPageNotFound()
        {
            var browser = new Mock<IWebBrowser>();
            browser.Setup(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotAvailable)))).Verifiable();

            var popupController = new Mock<IBrowserPopupController>();

            var handler = new BrowserHandler(popupController.Object);
            var errorText = "Not found";
            var result = handler.OnLoadError(browser.Object, "StudioHomePage.url", 404, ref errorText);
            Assert.IsTrue(result);
            browser.Verify(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageNotAvailable))), Times.Once());
        }

        [TestMethod]
        public void BrowserHandler_OnLoadError_UrlDoesNotContainStudioHomePage_RedirectsToServerDisconnected()
        {
            var browser = new Mock<IWebBrowser>();
            browser.Setup(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageMissing)))).Verifiable();

            var popupController = new Mock<IBrowserPopupController>();

            var handler = new BrowserHandler(popupController.Object);
            var errorText = "Not found";
            var result = handler.OnLoadError(browser.Object, "myfake.url", 404, ref errorText);
            Assert.IsTrue(result);
            browser.Verify(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageMissing))), Times.Once());
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
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_OnResourceResponse")]
        public void BrowserHandler_OnResourceResponse_IsPoppingTrue_InvokesConfigurePopupAndResetsIsPopping()
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
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_OnResourceResponse")]
        public void BrowserHandler_OnResourceResponse_IsPoppingFalse_DoesNotInvokeConfigurePopup()
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_OnResourceResponse")]
        public void BrowserHandler_OnResourceResponse_StatusIsForbiddenOrUnauthorized_RedirectsToPageForbidden()
        {
            foreach(HttpStatusCode statusCode in Enum.GetValues(typeof(HttpStatusCode)))
            {
                var redirects = statusCode == HttpStatusCode.Forbidden || statusCode == HttpStatusCode.Unauthorized;
                Verify_OnResourceResponse_RedirectsToPageUnauthorized(statusCode, redirects);
            }
        }

        void Verify_OnResourceResponse_RedirectsToPageUnauthorized(HttpStatusCode statusCode, bool redirects)
        {
            var hitCount = redirects ? 1 : 0;

            //------------Setup for test--------------------------
            var browser = new Mock<IWebBrowser>();
            browser.Setup(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageRestrictedAccess)))).Verifiable();

            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);


            //------------Execute Test---------------------------
            handler.OnResourceResponse(browser.Object, string.Empty, (int)statusCode, statusCode.ToString(), string.Empty, new WebHeaderCollection());

            //------------Assert Results-------------------------
            browser.Verify(b => b.Load(It.Is<string>(s => s.EndsWith(StringResources.Uri_Studio_PageRestrictedAccess))), Times.Exactly(hitCount));
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_GetAuthCredentials")]
        public void BrowserHandler_GetAuthCredentials_SchemeIsNtlm_True()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);

            //------------Execute Test---------------------------
            var password = "";
            var username = "";
            var result = handler.GetAuthCredentials(new Mock<IWebBrowser>().Object, false, "", 0, "", "NTLM", ref username, ref password);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_GetAuthCredentials")]
        public void BrowserHandler_GetAuthCredentials_SchemeIsNotNtlm_False()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);

            //------------Execute Test---------------------------
            var password = "";
            var username = "";
            var result = handler.GetAuthCredentials(new Mock<IWebBrowser>().Object, false, "", 0, "", "Basic", ref username, ref password);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_GetAuthCredentials")]
        public void BrowserHandler_GetAuthCredentials_SchemeIsNull_False()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);

            //------------Execute Test---------------------------
            var password = "";
            var username = "";
            var result = handler.GetAuthCredentials(new Mock<IWebBrowser>().Object, false, "", 0, "", null, ref username, ref password);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_OnBeforeBrowse")]
        public void BrowserHandler_OnBeforeBrowse_False()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);

            //------------Execute Test---------------------------
            var result = handler.OnBeforeBrowse(new Mock<IWebBrowser>().Object, new Mock<IRequest>().Object, new NavigationType(), false);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_OnBeforeResourceLoad")]
        public void BrowserHandler_OnBeforeResourceLoad_False()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);

            //------------Execute Test---------------------------
            var result = handler.OnBeforeResourceLoad(new Mock<IWebBrowser>().Object, new Mock<IRequestResponse>().Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BrowserHandler_GetDownloadHandler")]
        public void BrowserHandler_GetDownloadHandler_False()
        {
            //------------Setup for test--------------------------
            var handler = new BrowserHandler(new Mock<IBrowserPopupController>().Object);
            var downloadHandler = new Mock<IDownloadHandler>().Object;

            //------------Execute Test---------------------------
            var result = handler.GetDownloadHandler(new Mock<IWebBrowser>().Object, "", "", 0, ref downloadHandler);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }
    }
}
