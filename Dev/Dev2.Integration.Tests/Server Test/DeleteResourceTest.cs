using System;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Controller;
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
            connection.Connect();
            const string ServiceName = "DeleteWorkflowTest";
            const string ResourceType = "WorkflowService";
            //----------Execute-----------------------------

            var coms = new CommunicationController { ServiceName = "DeleteResourceService" };

            coms.AddPayloadArgument("ResourceName", ServiceName);
            coms.AddPayloadArgument("ResourceType", ResourceType);

            var result = coms.ExecuteCommand<ExecuteMessage>(connection, Guid.Empty);

            Assert.IsTrue(result.Message.Contains("Success"), "Got [ " + result.Message.ToString() + " ]");
        }
//          
//        [TestMethod]
//        public void DeleteWorkflowSuccessCantDeleteDeletedWorkflow()
//        {
//            //---------Delete Workflow Success-------
//            var serviceName = "DeleteWorkflowTest2";
//            string request = BuildDeleteRequestString(serviceName, "WorkflowService");
//            string postData = String.Format("{0}{1}?{2}", _webserverUri, "DeleteResourceService", request);
//            var result = TestHelper.PostDataToWebserver(postData);
//            Assert.IsTrue(result.Contains("Success"), "Got [ " + result + " ]");
//
//            //---------Delete Workflow Failure-------
//            result = TestHelper.PostDataToWebserver(postData);
//            Assert.IsTrue(result.Contains("WorkflowService 'DeleteWorkflowTest2' was not found."), "Got [ " + result + " ]");
//        }
//
//        [TestMethod]
//        public void DeleteWorkflowSuccessCantCallDeletedWorkflow()
//        {
//            //---------Call Workflow Success-------
//            var serviceName = "DeleteWorkflowTest3";
//            var servicecall =  String.Format("{0}{1}", _webserverUri, serviceName);           
//            var result = TestHelper.PostDataToWebserver(servicecall);
//            Assert.IsTrue(result.Contains("<DataList></DataList>"), "Got [ " + result + " ]");
//
//            //---------Delete Workflow Success-------
//            string request = BuildDeleteRequestString(serviceName, "WorkflowService");
//            string postData = String.Format("{0}{1}?{2}", _webserverUri, "DeleteResourceService", request);
//            result = TestHelper.PostDataToWebserver(postData);
//            Assert.IsTrue(result.Contains("Success"), "Got [ " + result + " ]");
//
//            //---------Call Workflow Failure-------
//            result = TestHelper.PostDataToWebserver(servicecall);
//            Assert.IsTrue(result.Contains("Service [ DeleteWorkflowTest3 ] not found."), "Got [ " + result + " ]");
//            
//        }
//
//        [TestMethod]
//        public void DeleteWorkflowInvalidTypeCanCallWorkflowTest()
//        {
//            //---------Delete Workflow Failure (incorrect type)-------
//            var serviceName = "DeleteWorkflowTest4";
//            string request = BuildDeleteRequestString(serviceName, "InsertAnyTypeThatsNotCorrectHere");
//            string postData = String.Format("{0}{1}?{2}", _webserverUri, "DeleteResourceService", request);
//            var result = TestHelper.PostDataToWebserver(postData);
//            Assert.IsTrue(result.Contains("'DeleteWorkflowTest4' was not found"), "Got [ " + result + " ]");
//
//            //---------Call Workflow Success-------
//            var servicecall = String.Format("{0}{1}", _webserverUri, serviceName);
//            result = TestHelper.PostDataToWebserver(servicecall);
//            Assert.IsTrue(result.Contains("<DataList></DataList>"), "Got [ " + result + " ]");
//        }

        private string BuildDeleteRequestString(string resourceName, string resourceType, string roles = "Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access")
        {
            return string.Format("ResourceName={0}&ResourceType={1}&Roles={2}", resourceName, resourceType, roles);
        }
    }
}
