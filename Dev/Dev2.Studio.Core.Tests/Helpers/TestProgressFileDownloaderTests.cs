
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common.Interfaces.Utils;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.CustomControls.Progress;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    [TestClass]
    [ExcludeFromCodeCoverage]
   
    public class TestProgressFileDownloaderTests
    {
        
        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for ProgressFileDownloader's Download method, it is expected to start the download and show the progress dialog")]
        [Owner("Ashley Lewis")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_Download_AsyncDownloadStartedAndProgressDialogShown()
// ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Show()).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new ProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object,new Mock<ICryptoProvider>().Object);
            //exe
            testProgressFileDownloader.Download(It.IsAny<Uri>(), It.IsAny<string>(), false,It.IsAny<string>(), "");

            //assert
            mockWebClient.Verify(c => c.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()));

        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for ProgressFileDownloader's Cancel method, it is expected to stop the download and the progress dialog")]
        [Owner("Ashley Lewis")]
        public void ProgressFileDownloaderUnitTestDownloadAsyncDownloadStopedAndProgressDialogClosed()
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.CancelAsync()).Verifiable();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Close()).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object,new Mock<ICryptoProvider>().Object);
            //exe
            testProgressFileDownloader.TestCancelDownload();

            //assert
            mockWebClient.Verify(c => c.CancelAsync());

        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("Test for ProgressFileDownloader's RehydrateDialog method, it is expected to rehydrate the progress dialogs label and progress values")]
        [Owner("Ashley Lewis")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_UnitTest_RehydrateDialog_ProgressDialogRehydrated()
// ReSharper restore InconsistentNaming
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.StatusChanged(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>())).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object,new Mock<ICryptoProvider>().Object);
            //exe
            testProgressFileDownloader.TestRehydrateDialog("test file", 50, 2000);
#pragma warning disable 168
            var actual = testProgressFileDownloader.GetProgressDialog();
#pragma warning restore 168
            //assert
            mockProgressDialog.Verify(c => c.StatusChanged(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.Description("ProgressFileDownloader's OnDownloadFileCompleted event is expected to close the progress dialog")]
        [Owner("Ashley Lewis")]
        public void ProgressFileDownloader_UnitTest_OnDownloadFileCompleted_DialogClosed()
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Close()).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object,new Mock<ICryptoProvider>().Object);
            //exe
            testProgressFileDownloader.TestStartUpdate("test file", true);

            //assert
            mockProgressDialog.Verify(c => c.Close());
            Assert.IsFalse(testProgressFileDownloader.IsBusyDownloading);
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgresssFileDownloader_Ctor")]
// ReSharper disable InconsistentNaming
        public void ProgresssFileDownloader_Ctor_VerifyExceptionThrownIfWebClientsNull()
        {
// ReSharper disable ObjectCreationAsStatement
            new ProgressFileDownloader(null,  new Mock<IFile>().Object, new Mock<ICryptoProvider>().Object);
// ReSharper restore ObjectCreationAsStatement

        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgresssFileDownloader_Ctor")]
        public void ProgresssFileDownloader_Ctor_VerifyExceptionThrownIfFileClientsNull()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProgressFileDownloader(new Mock<IDev2WebClient>().Object, null, new Mock<ICryptoProvider>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgresssFileDownloader_Ctor")]
        public void ProgresssFileDownloader_Ctor_VerifyExceptionThrownIfCryptoIsNull()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProgressFileDownloader(new Mock<IDev2WebClient>().Object, new Mock<IFile>().Object, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Ctor ")]
        public void ProgressFileDownloader_Ctor_AssertCorrectConstruction()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
#pragma warning disable 168
            var a = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
         
#pragma warning restore 168
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Cleanup ")]
        public void ProgressFileDownloader_Cleanup_AssertFilesDeleted()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFiles("c:\bob")).Returns(new[] { "tmpa", "tmpb", "tmpc" });
            var file = new Mock<IFile>();
           
            ProgressFileDownloader.PerformCleanup(dir.Object,"c:\bob",file.Object);
             file.Verify(a=>a.Delete(It.IsAny<string>()),Times.Exactly(3));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Cleanup ")]
        public void ProgressFileDownloader_Cleanup_OnlyTempFilesDeleted()
        {
            //------------Setup for test--------------------------
            var dir = new Mock<IDirectory>();
            dir.Setup(a => a.GetFiles("c:\bob")).Returns(new[] { "tmpa", "tmpb", "c" });
            var file = new Mock<IFile>();

            ProgressFileDownloader.PerformCleanup(dir.Object, "c:\bob", file.Object);
            file.Verify(a => a.Delete(It.IsAny<string>()), Times.Exactly(2));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_PerformCheckSum ")]
        public void ProgressFileDownloader_PerformCheckSum_VerifyCryptoProviderCalledAndStringsCompared_ExpectTrueReturned()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
#pragma warning disable 168
            var ax = new ProgressFileDownloader(webClient.Object,  file.Object, crytpto.Object);
#pragma warning restore 168
            //------------Execute test--------------------------
            Assert.IsTrue( ax.PerformCheckSum("bob", "012"));
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_PerformCheckSum ")]
        public void ProgressFileDownloader_PerformCheckSum_VerifyCryptoProviderCalledAndStringsCompared_ExpectFalseReturned()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());

            var ax = new ProgressFileDownloader(webClient.Object,  file.Object, crytpto.Object);

            //------------Execute test--------------------------
            Assert.IsFalse(ax.PerformCheckSum("bob", "124"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Complted_HasNoError_ExpectSuccess()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var progressNotifier = new Mock<IProgressNotifier>();
            progressNotifier.Setup(a => a.Close()).Verifiable();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => progressNotifier.Object;
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave",true,"moo","012");
            webClient.Raise(a=>a.DownloadFileCompleted+= null,new AsyncCompletedEventArgs(null,false,"moo" ));

            //------------Assert Results-------------------------
            file.Verify(a=>a.Move("dave","moo"));
            progressNotifier.Verify(a=>a.Close(), Times.Once());
            Assert.IsFalse(ax.IsBusyDownloading);
        }






        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Complted_HasError_ExpectDelete()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var progressNotifier = new Mock<IProgressNotifier>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => progressNotifier.Object;
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave", true, "moo", "012");
            webClient.Raise(a => a.DownloadFileCompleted += null, new AsyncCompletedEventArgs(new Exception(), false, "moo"));

            //------------Assert Results-------------------------
            file.Verify(a => a.Move("dave", "moo"),Times.Never());
            progressNotifier.Verify(a => a.Close(), Times.Once());
            Assert.IsTrue(ax.IsBusyDownloading);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Completed_Cancelled_ExpectDelete()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var progressNotifier = new Mock<IProgressNotifier>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => progressNotifier.Object;
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave", true, "moo", "012");
            webClient.Raise(a => a.DownloadFileCompleted += null, new AsyncCompletedEventArgs(null, true, "moo"));

            //------------Assert Results-------------------------
            file.Verify(a => a.Move("dave", "moo"), Times.Never());
            file.Verify(a=>a.Delete(It.IsAny<string>()),Times.Once());
            progressNotifier.Verify(a => a.Close(), Times.Once());
            Assert.IsTrue(ax.IsBusyDownloading);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
// ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Completed_HasNoError_ExpectSuccessAndShutDown()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            var progressNotifier = new Mock<IProgressNotifier>();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => progressNotifier.Object;
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            bool shutdown = false;
            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave", false, "moo", "012");
            ax.ShutDownAction = s => shutdown = true;
            webClient.Raise(a => a.DownloadFileCompleted += null, new AsyncCompletedEventArgs(null, false, "moo"));

            //------------Assert Results-------------------------
            file.Verify(a => a.Move("dave", "moo"));

            Assert.IsTrue(shutdown);
            Assert.IsFalse(ax.IsBusyDownloading);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Cancel_HasNoErrorFileDoesNotExist_ExpectSuccess()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            ProgressFileDownloader.GetProgressDialogViewModel = ((a, b) => new Mock<IProgressNotifier>().Object);
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            file.Setup(a => a.Exists("dave")).Returns(true);
            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave", false, "moo", "012");
            ax.Cancel();

            //------------Assert Results-------------------------
            file.Verify(a => a.Delete("dave")); // once because it exists and then when cancelling
            webClient.Verify(a=>a.CancelAsync());
      
            
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Download")]
        // ReSharper disable InconsistentNaming
        public void ProgressFileDownloader_Download_Cancel_HasNoError_ExpectSuccessAndFileDeleted()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
            var crytpto = new Mock<ICryptoProvider>();
            ProgressFileDownloader.GetProgressDialogViewModel = ((a, b) => new Mock<IProgressNotifier>().Object);
            var ax = new ProgressFileDownloader(webClient.Object, file.Object, crytpto.Object);
            var stream = new MemoryStream();
            stream.WriteByte(1);
            stream.WriteByte(2);
            stream.WriteByte(3);
            crytpto.Setup(a => a.ComputeHash(It.IsAny<Stream>())).Returns(new byte[] { 0, 1, 2 });
            file.Setup(a => a.Open("bob", FileMode.Open)).Returns(new MemoryStream());
            

            //------------Execute Test---------------------------
            ax.Download(new Uri("http://bob"), "dave", false, "moo", "012");
            ax.Cancel();

            //------------Assert Results-------------------------
            file.Verify(a => a.Delete("dave"), Times.Never());
            webClient.Verify(a => a.CancelAsync());


    }
    }


}
