using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class ConnectionsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Connections_CanConnectToServer_InvalidAddress_ExpectErrorResult()
        {
            //------------Setup for test--------------------------
            const string address = "http:://localhost:3142/dsf";
            var conn = new Connection
            {
                ResourceType = "Server",
                Address = address
            };
            var hubProxy = new Mock<IHubFactory>();
            var myConnections = new Connections(() => new List<string>(), hubProxy.Object);


            //------------Execute Test---------------------------
            var canConnectToServer = myConnections.CanConnectToServer(conn);

            //------------Assert Results-------------------------
            Assert.AreEqual("Invalid URI: The Authority/Host could not be parsed.", canConnectToServer.ErrorMessage);
            Assert.IsFalse(canConnectToServer.IsValid);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Connections_CanConnectToServer_ValidAddress_ExpectPassResult()
        {
            //------------Setup for test--------------------------
            const string address = "http://localhost:3142/dsf";
            var conn = new Connection
            {
                ResourceType = "Server",
                Address = address
            };
            var hubFactory = new Mock<IHubFactory>();
            var hubProxy = new Mock<IHubProxy>();
            hubFactory.Setup(factory => factory.CreateHubProxy(conn)).Returns(hubProxy.Object);
            var myConnections = new Connections(() => new List<string>(), hubFactory.Object);


            //------------Execute Test---------------------------
            var canConnectToServer = myConnections.CanConnectToServer(conn);

            //------------Assert Results-------------------------
            Assert.IsTrue(canConnectToServer.IsValid);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Connections_CanConnectToServer_ThrowsException_ExpectErrorResult()
        {
            //------------Setup for test--------------------------
            const string address = "http://localhost:3142/dsf";
            var conn = new Connection
            {
                ResourceType = "Server",
                Address = address
            };
            var hubFactory = new Mock<IHubFactory>();
            var clientException = new HttpClientException(new HttpResponseMessage(HttpStatusCode.BadRequest))
            {
                Response = { ReasonPhrase = "error" }
            };
            hubFactory.Setup(factory => factory.CreateHubProxy(conn)).Throws(new Exception("", clientException));
            var myConnections = new Connections(() => new List<string>(), hubFactory.Object);
            
            //------------Execute Test---------------------------
            var canConnectToServer = myConnections.CanConnectToServer(conn);

            //------------Assert Results-------------------------
            Assert.IsFalse(canConnectToServer.IsValid);
            Assert.AreEqual("Connection Error : error",canConnectToServer.ErrorMessage);
        }
    }
}
