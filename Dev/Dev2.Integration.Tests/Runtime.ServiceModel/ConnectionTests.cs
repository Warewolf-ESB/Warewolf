using System;
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
        public void Connection_Test_ValidServer_PositiveValidationResult()
        {
            //Create Connection
            Connection conn = SetupUserConnection();
            Connections connections = new Connections();

            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            Assert.IsTrue(validationResult.IsValid);

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


        static Connection SetupUserConnection()
        {
            var testConnection = new Connection
            {
                Address = ServerSettings.DsfAddress,
                AuthenticationType = AuthenticationType.User,
                Password = "I73573r0",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = @"Dev2\IntegrationTester",
                WebServerPort = 3142
            };

            return testConnection;
        }

    }
}
