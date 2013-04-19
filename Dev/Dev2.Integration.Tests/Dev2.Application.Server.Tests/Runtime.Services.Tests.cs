using System;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    [Ignore]
    public class ConnectionsTests
    {
        [TestMethod]
        public void ConnectionsTest_ValidServer_Expected_PositiveValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            Connections connections = new Connections();

            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            Assert.IsTrue(validationResult.IsValid);

        }

        [TestMethod]
        public void ConnectionsTest_InvalidServer_Expected_NegativeValidationResult()
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

        #region Private Test Methods

        private Connection SetupDefaultConnection()
        {
            var testConnection = new Connection
            {
                Address = "http://localhost:77/dsf",
                AuthenticationType = AuthenticationType.Windows,
                Password = "secret",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = @"Domain\User",
                WebServerPort = 1234
            };

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
