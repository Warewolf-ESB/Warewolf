using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Browsers
{
    [TestClass]
    public class BrowserPopupControllerTests
    {
        const string PopupTitle = "Test Title";

        [TestMethod]
        public void BrowserPopupControllerConstructorWithArgsExpectedSetsPopupTitle()
        {
            var controller = new TestBrowserPopupController(PopupTitle, 0);

            Assert.AreEqual(PopupTitle, controller.PopupTitle);
        }

        #region ConfigurePopup

        [TestMethod]
        public void BrowserPopupControllerConfigurePopupWithoutPopupHandleExpectedDoesNothing()
        {
            var controller = new TestBrowserPopupController(PopupTitle, 0);
            controller.ConfigurePopup();

            Assert.AreEqual(1, controller.FindPopupHitCount);
            Assert.AreEqual(0, controller.SetPopupForegroundHitCount);
            Assert.AreEqual(0, controller.SetPopupIconCount);
            Assert.AreEqual(0, controller.SetPopupTitleHitCount);
        }

        [TestMethod]
        public void BrowserPopupControllerConfigurePopupWithPopupHandleExpectedSetsPopupProperties()
        {
            var controller = new TestBrowserPopupController(PopupTitle, 1);
            controller.ConfigurePopup();

            Assert.AreEqual(1, controller.FindPopupHitCount);
            Assert.AreEqual(1, controller.SetPopupForegroundHitCount);
            Assert.AreEqual(1, controller.SetPopupIconCount);
            Assert.AreEqual(1, controller.SetPopupTitleHitCount);
        }

        #endregion

    }
}
