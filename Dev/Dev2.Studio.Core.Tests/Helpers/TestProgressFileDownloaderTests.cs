/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.CustomControls.Progress;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    [TestClass]
    public class TestProgressFileDownloaderTests
    {
        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("Test for ProgressFileDownloader's Cancel method, it is expected to stop the download and the progress dialog")]
        [Owner("Ashley Lewis")]
        public void ProgressFileDownloaderUnitTestDownloadAsyncDownloadStopedAndProgressDialogClosed()
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            mockWebClient.Setup(c => c.CancelAsync()).Verifiable();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.Close()).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object);
            //exe
            testProgressFileDownloader.TestCancelDownload();

            //assert
            mockWebClient.Verify(c => c.CancelAsync());
        }

        [TestMethod]
        [TestCategory("ProgressFileDownloaderUnitTest")]
        [Description("Test for ProgressFileDownloader's RehydrateDialog method, it is expected to rehydrate the progress dialogs label and progress values")]
        [Owner("Ashley Lewis")]
        public void ProgressFileDownloader_UnitTest_RehydrateDialog_ProgressDialogRehydrated()
        {
            //init
            var mockWebClient = new Mock<IDev2WebClient>();
            var mockProgressDialog = new Mock<IProgressDialog>();
            mockProgressDialog.Setup(c => c.StatusChanged(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>())).Verifiable();
            ProgressFileDownloader.GetProgressDialogViewModel = (x, y) => mockProgressDialog.Object;
            var testProgressFileDownloader = new TestProgressFileDownloader(mockWebClient.Object, new Mock<IFile>().Object);
            //exe
            testProgressFileDownloader.TestRehydrateDialog("test file", 50, 2000);
#pragma warning disable 168
            var actual = testProgressFileDownloader.GetProgressDialog();
#pragma warning restore 168
            //assert
            mockProgressDialog.Verify(c => c.StatusChanged(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()), Times.Once());
        }        

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgresssFileDownloader_Ctor")]
        public void ProgresssFileDownloader_Ctor_VerifyExceptionThrownIfWebClientsNull()
        {
            new ProgressFileDownloader(null,  new Mock<IFile>().Object);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgresssFileDownloader_Ctor")]
        public void ProgresssFileDownloader_Ctor_VerifyExceptionThrownIfFileClientsNull()
        {            
            new ProgressFileDownloader(new Mock<IDev2WebClient>().Object, null);            
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProgressFileDownloader_Ctor ")]
        public void ProgressFileDownloader_Ctor_AssertCorrectConstruction()
        {
            //------------Setup for test--------------------------
            var webClient = new Mock<IDev2WebClient>();
            var file = new Mock<IFile>();
#pragma warning disable 168
            var a = new ProgressFileDownloader(webClient.Object, file.Object);
         
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
    }    
}
