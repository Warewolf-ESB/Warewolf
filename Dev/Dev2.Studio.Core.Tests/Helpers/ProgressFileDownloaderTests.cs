using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.CustomControls.Progress;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ProgressFileDownloaderTests
    {
        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("Test for ProgressFileDownloader's Download method, it is expected to start the download and show the progress dialog")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_Download_AsyncDownloadStartedAndProgressDialogShown()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.ShowDialog()).Verifiable();
            var testProgressFileDownloader = new ProgressFileDownloader(mockWebClient.Object, mockProgressDialog.Object);

            //exe
            testProgressFileDownloader.Download(It.IsAny<Uri>(), It.IsAny<string>(), false);

            //assert
            mockWebClient.Verify(c => c.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()));
            mockProgressDialog.Verify(c => c.ShowDialog());
        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("Test for ProgressFileDownloader's Cancel method, it is expected to stop the download and the progress dialog")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_Download_AsyncDownloadStopedAndProgressDialogClosed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.CancelAsync()).Verifiable();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Close()).Verifiable();
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, mockProgressDialog.Object);

            //exe
            testProgressFileDownloader.TestCancelDownload();

            //assert
            mockWebClient.Verify(c => c.CancelAsync());
            mockProgressDialog.Verify(c => c.Close());
        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("Test for ProgressFileDownloader's RehydrateDialog method, it is expected to rehydrate the progress dialogs label and progress values")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_RehydrateDialog_ProgressDialogRehydrated()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.SetupProperty(c => c.Label);
            mockProgressDialog.SetupProperty(c => c.ProgressValue);
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, mockProgressDialog.Object);

            //exe
            testProgressFileDownloader.TestRehydrateDialog("test file", 50, 2000);
            var actual = testProgressFileDownloader.GetProgressDialog();

            //assert
            Assert.AreEqual("test file downloaded 50% of 1 KB", actual.Label, "Progress dialog label not updated on progress change");
            Assert.AreEqual(50.0, actual.ProgressValue, "Progress dialog label not updated on progress change");
        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("ProgressFileDownloader's OnDownloadFileCompleted event is expected to close the progress dialog")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_OnDownloadFileCompleted_DialogClosed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Close()).Verifiable();
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, mockProgressDialog.Object);

            //exe
            testProgressFileDownloader.TestStartUpdate("test file", true);

            //assert
            mockProgressDialog.Verify(c => c.Close());
        }
    }
}