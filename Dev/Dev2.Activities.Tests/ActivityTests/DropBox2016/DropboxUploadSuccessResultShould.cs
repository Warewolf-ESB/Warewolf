using Dev2.Activities.DropBox2016.Result;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]

    public class DropboxUploadSuccessResultShould
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void ConstructDropBoxSuccessResult_GivenFileMetadata_ShouldRetunNewSuccessResult()
        {
            //---------------Set up test pack-------------------
            var successResult = new DropboxUploadSuccessResult(new FileMetadata());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(successResult);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void failureResult_GivenException_ShouldRetunNewFailureResult()
        {
            //---------------Set up test pack-------------------
            var fileMetadata = new FileMetadata();
            var failureResult = new DropboxUploadSuccessResult(fileMetadata);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var expected = failureResult.GerFileMetadata();
            //---------------Test Result -----------------------
            Assert.AreEqual(expected, fileMetadata);
        }
    }
}