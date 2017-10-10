using System;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common.Interfaces.Wrappers;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Download
{
    [TestClass]

    public class DropBoxDownloadTests
    {
        private Mock<IDropboxDownload> CreateDropboxDownloadMock()
        {
            var mock = new Mock<IDropboxDownload>();
            var successResult = new DropboxDownloadSuccessResult(It.IsAny<IDownloadResponse<FileMetadata>>());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<IDropboxClientWrapper>()))
                 .Returns(successResult);
            return mock;
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateDropBoxActivity_GivenIsNew_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxDownload = CreateDropboxDownloadMock().Object;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxDownload);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTask_GivenDropBoxDownload_ShouldReturnFileMetadata()
        {
            //---------------Set up test pack-------------------
            var downloadMock = CreateDropboxDownloadMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(downloadMock);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            downloadMock.Object.ExecuteTask(It.IsAny<IDropboxClientWrapper>());
            downloadMock.Verify(upload => upload.ExecuteTask(It.IsAny<IDropboxClientWrapper>()));
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewDropboxUpload_GivenMissingToPath_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownLoad = new DropBoxDownLoad("");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownLoad);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
        } 
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewDropboxDownload_GivenMissingToPath_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownLoad = new DropBoxDownLoad( "a.file");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxDownLoad);
        } 
    }
}