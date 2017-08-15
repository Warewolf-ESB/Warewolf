using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.DropBox2016.Result;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]

    public class DropBoxFailureResultShould
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConstructDropBoxFailureResult_GivenException_ShouldRetunNewFailureResult()
        {
            //---------------Set up test pack-------------------
            var failureResult = new DropboxFailureResult(new Exception("Message"));
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(failureResult);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void failureResult_GivenException_ShouldRetunNewFailureResult()
        {
            //---------------Set up test pack-------------------
            var dpExc = new Exception("Message");
            var failureResult = new DropboxFailureResult(dpExc);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(failureResult);
            //---------------Execute Test ----------------------
            var exception = failureResult.GetException();
            //---------------Test Result -----------------------
            Assert.AreEqual(exception, dpExc);
        }
    }
}
