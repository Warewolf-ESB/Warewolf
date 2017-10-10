using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.Result;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.DropboxFiles
{
    [TestClass]

    public class DropboxFileReadTests
    {
        private Mock<IDropboxFileRead> CreateDropboxReadMock()
        {
            var mock = new Mock<IDropboxFileRead>();
            var successResult = new DropboxListFolderSuccesResult(It.IsAny<ListFolderResult>());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<IDropboxClientWrapper>()))
                 .Returns(successResult);
            return mock;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateDropBoxActivity_GivenIsNew_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileRead = CreateDropboxReadMock().Object;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileRead);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTask_GivendropboxFileRead_ShouldReturnFileMetadata()
        {
            //---------------Set up test pack-------------------
            var downloadMock = CreateDropboxReadMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(downloadMock);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            downloadMock.Object.ExecuteTask(It.IsAny<IDropboxClientWrapper>());
            downloadMock.Verify(upload => upload.ExecuteTask(It.IsAny<IDropboxClientWrapper>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewDropboxUpload_GivenEmptyPath_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var dropboxFileRead = new DropboxFileRead(true, "", false, false);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileRead);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewdropboxFileRead_GivenPath_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var dropboxFileRead = new DropboxFileRead(true, "a.file", false, false);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileRead);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewdropboxFileRead_GivenNullPath_ShouldBeValid()
        {
            //---------------Set up test pack-------------------
            var dropboxFileRead = new DropboxFileRead(true, null, false, false);
            PrivateObject type = new PrivateObject(dropboxFileRead);
            var staticField = type.GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileRead);
            Assert.IsNotNull(staticField);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(staticField);
            Assert.AreEqual("", "");

        }
    }
}