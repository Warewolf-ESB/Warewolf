using System;
using System.Collections.Generic;
using System.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EsbServicesEndpointTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once UnusedVariable
                var esbServicesEndpoint = new EsbServicesEndpoint();
            }
            catch(Exception ex)
            {
             Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Register_GivenNewUser_ShouldAddNewUser()
        {
            //---------------Set up test pack-------------------
            var contextChannel = new Mock<IContextChannel>();
            contextChannel.Setup(channel => channel.LocalAddress).Returns(new EndpointAddress("127.0.0.1"));
            var operationContext = new OperationContext(contextChannel.Object);
            var mock = new Mock<IExtensibleObject<OperationContext>>();
            var esbServicesEndpoint = new Mock<EsbServicesEndpoint>();
          
            PrivateObject privateObject = new PrivateObject(esbServicesEndpoint);
            //---------------Assert Precondition----------------
            var frameworkDuplexCallbackChannels = privateObject.GetField("_users") as Dictionary<string, IFrameworkDuplexCallbackChannel>;
            Assert.AreEqual(0, frameworkDuplexCallbackChannels?.Count);
            //---------------Execute Test ----------------------
            esbServicesEndpoint.Object.Register("NewUser");
            //---------------Test Result -----------------------
             frameworkDuplexCallbackChannels = privateObject.GetField("_users") as Dictionary<string, IFrameworkDuplexCallbackChannel>;
            Assert.AreEqual(1, frameworkDuplexCallbackChannels?.Count);
        }
    }
}
