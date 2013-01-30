using Dev2.Composition;
using Dev2.Integration.Tests;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Tests
{
    /// <summary>
    ///This is a result class for EnvironmentModelTest and is intended
    ///to contain all EnvironmentModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnvironmentModelTest
    {
        #region Test Variables

        private TestContext testContextInstance;
        private IEnvironmentModel _environmentModel;

        #endregion Test Variables

        #region Test Initialize and Clean Up

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_environmentModel != null)
            {
                _environmentModel = null;
            }
        }

        #endregion Test Initialize and Clean Up

        #region Connect Tests

        /// <summary>
        /// Tests that a connection can be established to the server using the 
        /// environment connection model
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_ConnectToAvailableServer_Expected_ConnectionSuccesful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");

            conn.Address = new Uri(ServerSettings.DsfAddress);
            conn.Connect();
            Assert.IsTrue(conn.IsConnected);
            conn.Disconnect();
        }

        #endregion Connect Tests

        #region Studio Request Tests

        /// <summary>
        /// Find Resource Service Tests
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_FindResources_Expected()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            string xmlString = CreateDataObject("FindResourceService", "*");

            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);

            conn.Connect();
            if (conn.IsConnected)
            {
                string returnData = conn.DataChannel.ExecuteCommand(xmlString, Guid.Empty, Guid.Empty);
                Assert.IsTrue(returnData.Contains("Workflow"));
            }
            else
            {
                Assert.Fail("Unable to create a connection to the server");
            }
            conn.Connect();
        }

        /// <summary>
        /// AddResource Service Test 
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_AddResource_NewResource_Expected_NewResourceAddedToServer()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            string xmlString = CreateDataObject("FindResourceService", "*");
            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);

            conn.Connect();
            if (conn.IsConnected)
            {
                string returnData = conn.DataChannel.ExecuteCommand(xmlString, Guid.Empty, Guid.Empty);
                Assert.IsTrue(returnData.Contains("Workflow"));
            }
            else
            {
                Assert.Fail("Unable to create a connection to the server");
            }
            conn.Connect();

            //Assert.Fail();
        }


        /// <summary>
        /// GetResource Service Test
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_GetResource_Expected_RetrieveResourceFromServer()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();
            string serviceName = "IntegrationTestWebsite";
            string xmlString = CreateDataObject("GetResourceService", serviceName);
            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);

            conn.Connect();
            if (conn.IsConnected)
            {
                string returnData = conn.DataChannel.ExecuteCommand(xmlString, Guid.Empty, Guid.Empty);
                Assert.IsTrue(returnData.Contains(serviceName));
            }
            else
            {
                Assert.Fail("Unable to create a connection to the server");
            }
            conn.Connect();
        }

        #endregion Studio Request Tests

        #region Private Test Methods

        private string CreateDataObject(string serviceName, string resourceName = null, string xmlFileLocation = null)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = serviceName;
            if (serviceName == "FindResourceService" || serviceName == "GetResourceService")
            {
                dataObj.ResourceName = resourceName;
                dataObj.ResourceType = ResourceType.WorkflowService;
            }
            else if (serviceName == "AddResourceService")
            {
                dataObj.ResourceXml = XmlTextReader.Create(xmlFileLocation).ReadContentAsString();
            }
            dataObj.Roles = string.Join(",", (new MockSecurityProvider("tester")).Roles);

            return dataObj.XmlString;
        }
        #endregion Private Test Methods
    }
}
