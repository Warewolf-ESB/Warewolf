using Dev2.Activities.DropBox2016.Result;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]

        public class DropboxListFolderResultShould
        {
            [TestMethod]
            [Owner("Nkosinathi Sangweni")]
            public void ConstructDropBoxSuccessResult_GivenListFolderResult_ShouldRetunNewSuccessResult()
            {
                //---------------Set up test pack-------------------
                var successResult = new DropboxListFolderSuccesResult(It.IsAny<ListFolderResult>());
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
                var fileMetadata = new ListFolderResult();
                var mock = new Mock<ListFolderResult>();

                var failureResult = new DropboxListFolderSuccesResult(mock.Object);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var expected = failureResult.GetListFolderResulResult();
                //---------------Test Result -----------------------
                Assert.AreEqual(expected, mock.Object);
            }
        }
    
}
