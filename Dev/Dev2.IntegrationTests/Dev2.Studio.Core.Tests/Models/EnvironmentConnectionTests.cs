using System;
using System.Text;
using System.Xml;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Network;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests.Models
{
    /// <summary>
    ///This is a result class for EnvironmentModelTest and is intended
    ///to contain all EnvironmentModelTest Unit Tests
    ///</summary>
    [TestClass]
    public class EnvironmentModelTest
    {
        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }



        #region Connect Tests

        /// <summary>
        /// Tests that a connection can be established to the server using the 
        /// environment connection model
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_ConnectToAvailableServer_Expected_ConnectionSuccesful()
        {
            IEnvironmentConnection conn = CreateConnection();

            conn.Connect(Guid.Empty);
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
            var request = CreateDataObject("FindResourceService", "*");

            IEnvironmentConnection conn = CreateConnection();

            conn.Connect(Guid.Empty);
            if(conn.IsConnected)
            {

                var returnData = conn.ExecuteCommand(request, Guid.Empty, Guid.Empty);
                Assert.IsTrue(returnData.Contains("Workflow"));
            }
            else
            {
                Assert.Fail("Unable to create a connection to the server");
            }
            conn.Connect(Guid.Empty);
        }

        /// <summary>
        /// AddResource Service Test 
        /// </summary>
        [TestMethod]
        public void EnvironmentConnection_AddResource_NewResource_Expected_NewResourceAddedToServer()
        {
            var xmlString = CreateDataObject("FindResourceService", "*");
            IEnvironmentConnection conn = CreateConnection();

            conn.Connect(Guid.Empty);
            if(conn.IsConnected)
            {
                var returnData = conn.ExecuteCommand(xmlString, Guid.Empty, Guid.Empty);
                Assert.IsTrue(returnData.Contains("Workflow"));
            }
            else
            {
                Assert.Fail("Unable to create a connection to the server");
            }
            conn.Connect(Guid.Empty);

        }

        #endregion Studio Request Tests

        #region Private Test Methods

        private StringBuilder CreateDataObject(string serviceName, string resourceName = null, string xmlFileLocation = null)
        {
            var request = new EsbExecuteRequest { ServiceName = serviceName };

            if(serviceName == "FindResourceService" || serviceName == "GetResourceService")
            {
                request.AddArgument("ResourceName", new StringBuilder(resourceName));
                request.AddArgument("ResourceType", new StringBuilder(ResourceType.WorkflowService.ToString()));
            }
            else if(serviceName == "AddResourceService")
            {
                if(xmlFileLocation != null)
                {
                    request.AddArgument("ResourceXml", new StringBuilder(XmlReader.Create(xmlFileLocation).ReadContentAsString()));
                }
            }

            var serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(request);
        }
        #endregion Private Test Methods


        #region CreateConnection

        static IEnvironmentConnection CreateConnection()
        {
            return CreateConnection(ServerSettings.DsfAddress);
        }

        static IEnvironmentConnection CreateConnection(string appServerUri)
        {

            return new ServerProxy(new Uri(appServerUri));
        }

        #endregion

    }
}
