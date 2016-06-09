using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable ObjectCreationAsStatement

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DropBoxUploadTests
    {
        private Mock<IDropBoxUpload> CreateDropboxUploadMock()
        {
            var mock = new Mock<IDropBoxUpload>();
            var fileMetadata = new DropboxUploadSuccessResult(new FileMetadata());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<DropboxClient>()))
                 .Returns(fileMetadata);
            return mock;
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateDropBoxActivity_GivenIsNew_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = CreateDropboxUploadMock().Object;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxUpload);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTask_GivenDropBoxUpload_ShouldReturnFileMetadata()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = CreateDropboxUploadMock();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxUpload);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            dropBoxUpload.Object.ExecuteTask(It.IsAny<DropboxClient>());
            dropBoxUpload.Verify(upload => upload.ExecuteTask(It.IsAny<DropboxClient>()));
        }
        
      
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewDropboxUpload_GivenMissingFromPath_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            //---------------Execute Test ----------------------
            new DropBoxUpload(null, "random.txt", "");
            //---------------Test Result -----------------------
        } 
        
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewDropboxUpload_GivenInvalidThenExecute_ShouldReturnFailureResult()
        {
            //---------------Set up test pack-------------------
            var dropBoxUpload = new DropBoxUpload(null, "", "random.txt");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxUpload);
            //---------------Execute Test ----------------------
            var metadata = dropBoxUpload.ExecuteTask(It.IsAny<DropboxClient>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(metadata);
            Assert.IsInstanceOfType(metadata,typeof(DropboxFailureResult));
        }

       
    }
}