using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Progress;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class VersionCheckerTests
    {
        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsLatestExpectedTake5()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(1, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(StringResources.Warewolf_Homepage_New, startPage);
        }

        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsNotLatestExpectedStart()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(2, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(StringResources.Warewolf_Homepage_Start, startPage);
        }

        //[TestMethod]
        //[Ignore] // This is ignored as it needs to call out to the web. This needs to move into a nightly regression pack
        //public void VersionCheckerStartPageUriExpectedChecksOnlineForLatestVersion()
        //{
        //    Version expected;
        //    using(var client = new WebClient())
        //    {
        //        var version = client.DownloadString(StringResources.Warewolf_Version);
        //        expected = new Version(version);
        //    }
        //    var checker = new VersionChecker();
        //    var actual = checker.Latest;

        //    Assert.AreEqual(expected, actual);
        //}

        #region IsLastest Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        // ReSharper disable InconsistentNaming
        public void IsLastest_UnitTest_CheckIfAppIsLatestVersionWithYesClickedInPopup_BrowserOpened()
        // ReSharper restore InconsistentNaming
        {
            Mock<IDev2WebClient> mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.2").Verifiable();

            Mock<IProgressFileDownloader> mockDownloader = new Mock<IProgressFileDownloader>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object);
            versionChecker.ShowPopupResult = MessageBoxResult.Yes;
            versionChecker.CurrentVersion = new Version(0, 0, 0, 1);
            versionChecker.IsLatest(mockDownloader.Object, new Mock<IProgressDialog>().Object, asyncWorker.Object);

            Assert.AreEqual(1, versionChecker.ShowPopUpHitCount, "Do you want to download new version not displayed if latest version if higher than current version");
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        // ReSharper disable InconsistentNaming
        public void IsLastest_UnitTest_CheckIfAppIsLatestVersionWithNoClickedInPopup_BrowserNotOpened()
        // ReSharper restore InconsistentNaming
        {
            Mock<IDev2WebClient> mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.2").Verifiable();

            Mock<IProgressDialog> mockPopUp = new Mock<IProgressDialog>();
            mockPopUp.Setup(c => c.ShowDialog()).Verifiable();

            Mock<IProgressFileDownloader> mockDownloader = new Mock<IProgressFileDownloader>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object);
            versionChecker.ShowPopupResult = MessageBoxResult.No;
            versionChecker.CurrentVersion = new Version(0, 0, 0, 1);
            versionChecker.IsLatest(mockDownloader.Object, mockPopUp.Object, asyncWorker.Object);

            mockPopUp.Verify(c => c.ShowDialog(), Times.Never());
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("VersionChecker")]
        [Description("Check if the app is the lastest version and show a pop up informing the user")]
        // ReSharper disable InconsistentNaming
        public void IsLastest_UnitTest_CheckIfAppIsLatestVersionWithAppIsLastestVersion_BrowserNotOpened()
        // ReSharper restore InconsistentNaming
        {
            Mock<IDev2WebClient> mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadString(It.IsAny<string>())).Returns("0.0.0.1").Verifiable();

            Mock<IProgressDialog> mockPopUp = new Mock<IProgressDialog>();
            mockPopUp.Setup(c => c.ShowDialog()).Verifiable();

            Mock<IProgressFileDownloader> mockDownloader = new Mock<IProgressFileDownloader>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object);
            versionChecker.ShowPopupResult = MessageBoxResult.Yes;
            versionChecker.CurrentVersion = new Version(0, 0, 0, 1);
            versionChecker.IsLatest(mockDownloader.Object, mockPopUp.Object, asyncWorker.Object);

            mockPopUp.Verify(c => c.ShowDialog(), Times.Never());
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Once());
        }

        #endregion
    }
}
