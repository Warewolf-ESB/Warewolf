using System;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
//using Dev2.Runtime.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Dev2.Runtime.Services;
using Dev2.Runtime.Diagnostics;


namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
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
            Connection testConnection = new Connection();
            testConnection.Address = "http://localhost:77/dsf";
            testConnection.AuthenticationType = AuthenticationType.Windows;
            testConnection.Password = "secret";
            testConnection.ResourceID = Guid.NewGuid();
            testConnection.ResourceName = "TestResourceIMadeUp";
            testConnection.ResourcePath = @"host\Server";
            testConnection.ResourceType = DynamicServices.enSourceType.Dev2Server;
            testConnection.UserName = @"Domain\User";
            testConnection.WebServerPort = 1234;

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
