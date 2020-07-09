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
        Mock<IDropboxDownload> CreateDropboxDownloadMock()
        {
            var mock = new Mock<IDropboxDownload>();
            var successResult = new DropboxDownloadSuccessResult(It.IsAny<IDownloadResponse<FileMetadata>>());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<IDropboxClient>()))
                 .Returns(successResult);
            return mock;
        }


        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTask_GivenDropBoxDownload_ShouldReturnFileMetadata()
        {
            //---------------Set up test pack-------------------
            var downloadMock = CreateDropboxDownloadMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(downloadMock);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            downloadMock.Object.ExecuteTask(It.IsAny<IDropboxClient>());
            downloadMock.Verify(upload => upload.ExecuteTask(It.IsAny<IDropboxClient>()));
        }
        
        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewDropboxDownload_GivenMissingToPath_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownLoad = new DropBoxDownLoad("a.file");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxDownLoad);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void ExecuteDropboxDownload_Throws_ShouldReturnFailedResult()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownLoad = new DropBoxDownLoad("a.file");
            var mockDropboxClient = new Mock<IDropboxClient>();
            mockDropboxClient.Setup(client => client.DownloadAsync(It.IsAny<DownloadArg>())).Throws(new Exception());
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var result = dropBoxDownLoad.ExecuteTask(mockDropboxClient.Object);
            //---------------Test Result -----------------------
            Assert.IsInstanceOfType(result, typeof(DropboxFailureResult), "Dropbox failure result not returned after exception");
        }
    }
}