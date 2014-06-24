using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Windows;
using Dev2.Common.Wrappers;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Progress;
using Dev2.Helpers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    // ReSharper disable InconsistentNaming
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
            Assert.AreEqual(StringResources.Warewolf_Homepage_Start, startPage);
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
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Exactly(2));
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
            //mockPopUp.Setup(c => c.ShowDialog()).Verifiable();

            Mock<IProgressFileDownloader> mockDownloader = new Mock<IProgressFileDownloader>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object);
            versionChecker.ShowPopupResult = MessageBoxResult.No;
            versionChecker.CurrentVersion = new Version(0, 0, 0, 1);
            versionChecker.IsLatest(mockDownloader.Object, mockPopUp.Object, asyncWorker.Object);

           // mockPopUp.Verify(c => c.ShowDialog(), Times.Never());
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Exactly(2));
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
          //  mockPopUp.Setup(c => c.ShowDialog()).Verifiable();

            Mock<IProgressFileDownloader> mockDownloader = new Mock<IProgressFileDownloader>();
            var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
            VersionCheckerTestClass versionChecker = new VersionCheckerTestClass(mockWebClient.Object);
            versionChecker.ShowPopupResult = MessageBoxResult.Yes;
            versionChecker.CurrentVersion = new Version(0, 0, 0, 1);
            versionChecker.IsLatest(mockDownloader.Object, mockPopUp.Object, asyncWorker.Object);

          //  mockPopUp.Verify(c => c.ShowDialog(), Times.Never());
            mockWebClient.Verify(c => c.DownloadString(It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion


        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Ctor_NullFile_ExpectException()
        {
            //------------Setup for test--------------------------
// ReSharper disable ObjectCreationAsStatement
            new VersionChecker(null, new FileWrapper(), VersionInfo.FetchVersionInfoAsVersion, Dev2MessageBoxViewModel.ShowWithCustomButtons);
// ReSharper restore ObjectCreationAsStatement
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Currentr_WebClientNullFile_ExpectException()
        {
            //------------Setup for test--------------------------
// ReSharper disable ObjectCreationAsStatement
            new VersionChecker(new Dev2WebClient(new WebClient()), null, VersionInfo.FetchVersionInfoAsVersion, Dev2MessageBoxViewModel.ShowWithCustomButtons);
// ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_Ctor")]
        public void VersionChecker_Currentr_NullVersionChecker_ExpectException()
        {
            //------------Setup for test--------------------------
// ReSharper disable ObjectCreationAsStatement
            new VersionChecker(new Dev2WebClient(new WebClient()), new FileWrapper(), null, Dev2MessageBoxViewModel.ShowWithCustomButtons);
// ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_PerformDownLoad")]
        public void VersionChecker_PerformDownLoad_FileExists_ExpectPerformInstall()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var fileWrapper = new Mock<IFile>();
            webClient.Setup(a => a.DownloadString(It.IsAny<string>())).Returns("0.0.0.2");
            fileWrapper.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var progress = new Mock<IProgressFileDownloader>();
            var versionChecker = new VersionChecker(webClient.Object, fileWrapper.Object, () => new Version(0, 0, 0, 1),(a, b, c, d, e,f)=>MessageBoxResult.Yes);
 

            versionChecker.PerformDownLoad(progress.Object);
            //------------Execute Test---------------------------
            progress.Verify(a=>a.StartUpdate(It.IsAny<string>(),false));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_PerformDownLoad")]
        public void VersionChecker_PerformDownLoad_FileExists_AssertPopupIsShownExpectPerformInstall()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var fileWrapper = new Mock<IFile>();
            webClient.Setup(a => a.DownloadString(It.IsAny<string>())).Returns("0.0.0.2");
            fileWrapper.Setup(a => a.Exists(It.IsAny<string>())).Returns(false);
            var progress = new Mock<IProgressFileDownloader>();
            bool called = false;
            var versionChecker = new VersionChecker(webClient.Object, fileWrapper.Object, () => new Version(0, 0, 0, 1), (a, b, c, d, e, f) => { called = true; return MessageBoxResult.Yes; });


            versionChecker.PerformDownLoad(progress.Object);
            //------------Execute Test---------------------------
 
            Assert.IsTrue(called);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_PerformDownLoad")]
        public void VersionChecker_PerformDownLoad_FileExists_CallsCorrectMessageBoxFunc()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var fileWrapper = new Mock<IFile>();
            webClient.Setup(a => a.DownloadString(It.IsAny<string>())).Returns("0.0.0.2");
            fileWrapper.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var progress = new Mock<IProgressFileDownloader>();
            bool called = false;
            var versionChecker = new VersionChecker(webClient.Object, fileWrapper.Object, () => new Version(0, 0, 0, 1), (a, b, c, d, e, f) => { called = true; return MessageBoxResult.Yes; });


            versionChecker.PerformDownLoad(progress.Object);
            //------------Execute Test---------------------------
            progress.Verify(a => a.StartUpdate(It.IsAny<string>(), false));
            Assert.IsTrue(called);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionChecker_LatestVersionCheckSum")]
        public void VersionChecker_LatestVersionCheckSum_CallsCorrectDownloadString()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var fileWrapper = new Mock<IFile>();
            webClient.Setup(a => a.DownloadString(It.IsAny<string>())).Returns("1.2.1.1");
            fileWrapper.Setup(a => a.Exists(It.IsAny<string>())).Returns(true);
            var versionChecker = new VersionChecker(webClient.Object, fileWrapper.Object, () => new Version(0, 0, 0, 1), (a, b, c, d, e, f) => {  return MessageBoxResult.Yes; });


            var ax = versionChecker.LatestVersionCheckSum;
            //------------Execute Test---------------------------
            
            Assert.AreEqual("1.2.1.1",ax);
            //------------Assert Results-------------------------
        }
    }
}
// ReSharper restore InconsistentNaming