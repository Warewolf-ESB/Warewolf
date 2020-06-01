using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.ExceptionTests
{
    [TestClass]
    public class DropboxFileNotFoundExceptionTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNotFoundException = new DropboxFileNotFoundException();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileNotFoundException);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldHaveMessegeSet()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNotFoundException = new DropboxFileNotFoundException();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileNotFoundException);
            //---------------Execute Test ----------------------
            var message = dropboxFileNotFoundException.Message;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropboxPathNotFoundException, message);
            
        }
    } 
    
    [TestClass]
    public class DropboxPathNotFileFoundExceptionTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxPathNotFileFoundException = new DropboxPathNotFileFoundException();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxPathNotFileFoundException);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldHaveMessegeSet()
        {
            //---------------Set up test pack-------------------
            var dropboxPathNotFileFoundException = new DropboxPathNotFileFoundException();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxPathNotFileFoundException);
            //---------------Execute Test ----------------------
            var message = dropboxPathNotFileFoundException.Message;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropboxPathNotFileException, message);
            
        }
    }
    
    [TestClass]
    public class DropboxFileMalformdedExceptionTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileMalformdedException = new DropboxFileMalformdedException();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileMalformdedException);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldHaveMessegeSet()
        {
            //---------------Set up test pack-------------------
            var value = new DropboxFileMalformdedException();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(value);
            //---------------Execute Test ----------------------
            var message = value.Message;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropboxPathMalformdedException, message);
            
        }
    }
}
