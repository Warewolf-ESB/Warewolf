using Dev2.Studio.Core.AppResources.Browsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    [TestClass]
    public class BrowserPopupControllerTests
    {
        [TestMethod]
        public void BrowserPopupControllerConstructorWithNoArgsExpectedSetsInvokeOnDispatcherThreadToTrue()
        {
            var controller = new BrowserPopupController();

            Assert.IsTrue(controller.InvokeOnDispatcherThread);
        }

        #region Show

        [TestMethod]
        public void BrowserPopupControllerShowWithEmptyUrlExpectedDoesNothing()
        {
            var controller = new TestBrowserPopupController();
            controller.Show(null, 0, 0);

            Assert.AreEqual(0, controller.ShowDialogHitCount);
            Assert.IsNull(controller.Popup);
        }

        [TestMethod]
        public void BrowserPopupControllerShowWithUrlExpectedShowsDialog()
        {
            const string TestUrl = "myfake.url";
            const int TestWidth = 150;
            const int TestHeight = 100;

            var controller = new TestBrowserPopupController();
            controller.Show(TestUrl, TestWidth, TestHeight);

            Assert.AreEqual(1, controller.ShowDialogHitCount);
            Assert.IsNotNull(controller.Popup);
            Assert.AreEqual(controller.MainWindow.Title, controller.Popup.Title);
            Assert.AreEqual(controller.MainWindow.Icon, controller.Popup.Icon);
            Assert.AreEqual(TestUrl, controller.Popup.WebView.Address);
            Assert.AreEqual(TestWidth, controller.Popup.Width);
            Assert.AreEqual(TestHeight, controller.Popup.Height);
            Assert.IsNotNull(controller.Popup.WebView.LifeSpanHandler);
        }

        #endregion

    }
}
