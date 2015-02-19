using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.AntiCorruptionLayer.Tests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Server_Ctor")]
        public void Server_Ctor_NullParams_ExpectExceptions()
        {
            //------------Setup for test--------------------------
            // ReSharper disable once NotAccessedVariable
            var server = new Server("http://localhost:3142",new Mock<ICredentials>().Object);
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { "http://localhost:3142", new Mock<ICredentials>().Object }, typeof(Server));
            //------------Execute Test---------------------------
            // ReSharper disable RedundantAssignment
            server = new Server("http://localhost:3142","no","go");
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { "http://localhost:3142", "no", "go" }, typeof(Server));



            // ReSharper restore RedundantAssignment
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Server_Ctor")]
        public void Server_Ctor_ValidParams_ExpectItemsSetup()
        {
            //------------Setup for test--------------------------
           
            var server = new Server("http://moocowimpi:3142", "no", "go");
            Assert.IsNotNull(server.EnvironmentConnection); 
            Assert.IsNotNull(server.ExplorerRepository);
            Assert.AreEqual(server.EnvironmentConnection.AppServerUri.ToString(), "http://moocowimpi:3142/dsf");



            // ReSharper restore RedundantAssignment
            //------------Assert Results-------------------------
        }

    }
}
