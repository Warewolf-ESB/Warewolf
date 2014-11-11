using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Webs
{
    [TestClass]
    public class DropboxSourceCallbackHandlerTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxSourceCallbackHandler_Ctor")]
        public void DropboxSourceCallbackHandler_Ctor_AssertValuesAreSet_()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentRepository>();
            var dropboxSourceCallbackHandler = new DropBoxSourceSourceCallbackHandler(env.Object,"bob","dave");
            
            //------------Execute Test---------------------------
            Assert.AreEqual(dropboxSourceCallbackHandler.Token,"bob");
            Assert.AreEqual(dropboxSourceCallbackHandler.Secret,"dave");
            Assert.AreEqual(dropboxSourceCallbackHandler.CurrentEnvironmentRepository,env.Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxSourceCallbackHandler_Ctor"),ExpectedException(typeof(ArgumentNullException))]
        public void DropboxSourceCallbackHandler_Ctor_NullParam1()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentRepository>();
            var dropboxSourceCallbackHandler = new DropBoxSourceSourceCallbackHandler(null, "bob", "dave");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxSourceCallbackHandler_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DropboxSourceCallbackHandler_Ctor_NullParam2()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentRepository>();
            var dropboxSourceCallbackHandler = new DropBoxSourceSourceCallbackHandler(env.Object, null, "dave");

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropboxSourceCallbackHandler_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DropboxSourceCallbackHandler_Ctor_NullParam3()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentRepository>();
            var dropboxSourceCallbackHandler = new DropBoxSourceSourceCallbackHandler(env.Object, "bob", null);

        }
    }
}
