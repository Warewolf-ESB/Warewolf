using System;
using Dev2.Activities.DropBox2016;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]
    public class DropboxFileNameValidatorShould
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenFilePath_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNameValidator = new DropboxFileNameValidator("a.file");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileNameValidator);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void validate_GivenEmpty_ShouldThrowArgumentNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNameValidator = new DropboxFileNameValidator("");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxFileNameValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        } 
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void validate_GivenPath_ShouldNotThrowArgumentNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNameValidator = new DropboxFileNameValidator("a.file");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxFileNameValidator.Validate();
            //---------------Test Result -----------------------
        } 
    }
}
