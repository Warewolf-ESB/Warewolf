using System;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Integration.Tests.Helpers;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    public class DeleteResourceTest
    {
        private readonly string _webserverUri = ServerSettings.DsfAddress;

        public TestContext TestContext { get; set; }


        [TestMethod]
        public void DeleteWorkflowExpectsSuccessResponse()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect(Guid.Empty);
            const string ServiceName = "DeleteWorkflowTest";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);
            coms.AddPayloadArgument("ResourceID", "c25610b9-b28a-49f0-9074-d743ea729c6a");
            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            Assert.IsTrue(result.Message.Contains("Success"), "Got [ " + result.Message + " ]");
        }

        [TestMethod]
        public void DeleteWorkflowSuccessCantDeleteDeletedWorkflow()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect(Guid.Empty);
            const string ServiceName = "DeleteWorkflowTest2";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);
            coms.AddPayloadArgument("ResourceID", "f2b78836-91dd-44f0-a43f-b3ecf4c53cd5");

            // Execute
            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            // Assert
            Assert.IsTrue(result.Message.Contains("Success"), "Got [ " + result.Message + " ]");

            result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);
            StringAssert.Contains(result.Message.ToString(), "WorkflowService 'f2b78836-91dd-44f0-a43f-b3ecf4c53cd5' was not found.");
        }

        [TestMethod]
        public void DeleteWorkflowSuccessCantCallDeletedWorkflow()
        {
            //---------Setup-------------------------------
            IEnvironmentConnection connection = new ServerProxy(new Uri(_webserverUri));
            connection.Connect(Guid.Empty);
            const string ServiceName = "DeleteWorkflowTest3";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);

            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            //---------Call Workflow Failure-------
// ReSharper disable InconsistentNaming
            const string serviceName = "DeleteWorkflowTest3";
// ReSharper restore InconsistentNaming
            var servicecall = String.Format("{0}{1}", ServerSettings.WebserverURI, serviceName);
            var result2 = TestHelper.PostDataToWebserver(servicecall);
            Assert.IsTrue(result2.Contains("Service [ DeleteWorkflowTest3 ] not found."), "Got [ " + result + " ]");

        }
    }
}
