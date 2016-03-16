using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.DropBox2016;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DropBoxSuccessResultShould
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConstructDropBoxSuccessResult_GivenFileMetadata_ShouldRetunNewSuccessResult()
        {
            //---------------Set up test pack-------------------
            var successResult = new DropboxSuccessResult(new FileMetadata());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(successResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void failureResult_GivenException_ShouldRetunNewFailureResult()
        {
            //---------------Set up test pack-------------------
            var fileMetadata = new FileMetadata();
            var failureResult = new DropboxSuccessResult(fileMetadata);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var expected = failureResult.GerFileMetadata();
            //---------------Test Result -----------------------
            Assert.AreEqual(expected, fileMetadata);
        }
    }
}