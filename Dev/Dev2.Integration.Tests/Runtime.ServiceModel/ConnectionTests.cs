using System;
using System.DirectoryServices.ActiveDirectory;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connection_ServerMissingDsf")]
        public void Connection_ServerMissingDsf_ServerUrlWithSlashMissingDsf_ExpectConnectionOk()
        {
            //------------Setup for test--------------------------
            //Create Connection
            const string Address = "http://localhost:3142/";
            Connection conn = SetupUserConnection(Address);
            Connections connections = new Connections();

            //------------Execute Test---------------------------
            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(validationResult.IsValid, "Error connecting to " + Address + " " + validationResult.ErrorMessage);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Connection_ServerMissingDsf")]
        public void Connection_ServerMissingDsf_ServerUrlMissingSlash_ExpectConnectionOk()
        {
            //------------Setup for test--------------------------
            //Create Connection
            const string Address = "http://localhost:3142";
            Connection conn = SetupUserConnection(Address);
            Connections connections = new Connections();

            //------------Execute Test---------------------------
            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.IsTrue(validationResult.IsValid, "Error connecting to " + Address + " " + validationResult.ErrorMessage);
        }

        [TestMethod]
        public void Connection_Test_ValidServer_PositiveValidationResult()
        {
            //Create Connection
            Connection conn = SetupUserConnection();
            Connections connections = new Connections();

            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            Assert.IsTrue(validationResult.IsValid, validationResult.ErrorMessage);

        }

        [TestMethod]
        public void Connection_Test_InvalidServer_NegativeValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            // Invalidate connection
            conn.Address = "http://someserverImadeup:77/dsf";
            Connections connections = new Connections();
            // Attempt to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);
            Assert.IsFalse(validationResult.IsValid);

        }

        static Connection SetupDefaultConnection()
        {
            var testConnection = new Connection
            {
                Address = ServerSettings.DsfAddress,
                AuthenticationType = AuthenticationType.Windows,
                Password = "secret",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = @"Domain\User",
                WebServerPort = 3142
            };

            return testConnection;
        }


        static Connection SetupUserConnection(string overRideAddress = "")
        {
            var address = ServerSettings.DsfAddress;
            if(overRideAddress != "")
            {
                address = overRideAddress;
            }

            var testConnection = new Connection
            {
                Address = address,
                AuthenticationType = AuthenticationType.User,
                Password = "I73573r0",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = "IntegrationTester",
                WebServerPort = 3142
            };

            return testConnection;
        }

    }
}
