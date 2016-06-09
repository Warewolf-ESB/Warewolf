using System;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Activities.DropBox2016.Result;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedVariable

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Delete
{
    [TestClass]
    public class DropBoxDeleteTests
    {
        private Mock<IDropBoxDelete> CreateDropboxDeleteMock()
        {
            var mock = new Mock<IDropBoxDelete>();
            var fileMetadata = new DropboxDeleteSuccessResult(new FileMetadata());
            mock.Setup(upload => upload.ExecuteTask(It.IsAny<DropboxClient>()))
                 .Returns(fileMetadata);
            return mock;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropboxDelete_CreateDropboxActivity")]
        public void DropboxDelete_CreateDropboxActivity_GivenIsNew_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var dropBoxDelete = CreateDropboxDeleteMock().Object;

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsNotNull(dropBoxDelete);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropboxDelete_ExecuteTask")]
        public void DropboxDelete_ExecuteTask_GivenDropBoxDelete_ShouldReturnFileMetadata()
        {
            //------------Setup for test--------------------------
            var dropBoxDelete = CreateDropboxDeleteMock();

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDelete);
            //------------Execute Test---------------------------
            dropBoxDelete.Object.ExecuteTask(It.IsAny<DropboxClient>());
            //------------Assert Results-------------------------
            dropBoxDelete.Verify(upload => upload.ExecuteTask(It.IsAny<DropboxClient>()));
        }
        

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropboxDelete_CreateDropboxActivity")]
        [ExpectedException(typeof(ArgumentException))]
        public void DropboxDelete_CreateDropboxActivity_GivenMissingDeletePath_ShouldBeInValid()
        {
            //---------------Set up test pack-------------------
            var dropBoxDelete = new DropboxDelete("");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropboxDelete_CreateDropboxActivity")]
        [ExpectedException(typeof(ArgumentException))]
        public void DropboxDelete_CreateDropboxActivity_GivenInvalidThenExecute_ShouldReturnFailureResult()
        {
            //---------------Set up test pack-------------------
            var dropBoxDelete = new DropboxDelete("");
        }
    }
}
