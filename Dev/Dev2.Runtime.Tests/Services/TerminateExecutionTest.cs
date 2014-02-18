
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TerminateExecutionTest
    {
        private static readonly Guid _workspaceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static readonly Guid _resourceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static readonly object _syncRoot = new object();
        private const string _handleType = "TerminateExecutionService";

        [TestInitialize]
        public void TerminateExecutionInit()
        {
            Monitor.Enter(_syncRoot);
        }

        [TestCleanup]
        public void TerminateExecutionCleanup()
        {
            Monitor.Exit(_syncRoot);
        }

        [TestMethod]
        public void CreateServiceEntryExpectsDynamicService()
        {
            var terminateExecution = new TerminateExecution();
            var ds = terminateExecution.CreateServiceEntry();
            Assert.AreEqual(_handleType, terminateExecution.HandlesType());
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, ds.Actions.First().ActionType);
            Assert.AreEqual("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", ds.DataListSpecification);
        }

        [TestMethod]
        public void ExecuteExpectTaskTerminated()
        {
            ExecutableServiceRepository.Instance.Clear();
            var service = GetExecutableService();
            ExecutableServiceRepository.Instance.Add(service.Object);
            var terminateExecution = new TerminateExecution();
            terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);
            service.Verify(s => s.Terminate(), Times.Once());
        }

        [TestMethod]
        public void ExecuteExpectSuccessResult()
        {
            ExecutableServiceRepository.Instance.Clear();
            var service = GetExecutableService();
            ExecutableServiceRepository.Instance.Add(service.Object);
            var terminateExecution = new TerminateExecution();
            var result = terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);

            const string expected = "Message: Workflow succesfully terminated";
            var obj = ConvertToMsg(result.ToString());

            Assert.AreEqual(expected, obj.Message.ToString());

        }

        [TestMethod]
        public void ExecuteExpectFailResultIfNoServiceExist()
        {
            ExecutableServiceRepository.Instance.Clear();
            const string expected = "Message: Termination of workflow failed";
            var terminateExecution = new TerminateExecution();
            var result = terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.AreEqual(expected, obj.Message.ToString());
        }

        [TestMethod]
        public void TwoServicesAddedExpectServiceWithOneAssociatedServiceFromRepository()
        {
            ExecutableServiceRepository.Instance.Clear();
            var service1 = GetExecutableService();
            var service2 = GetExecutableService();
            ExecutableServiceRepository.Instance.Add(service1.Object);
            ExecutableServiceRepository.Instance.Add(service2.Object);
            var service = ExecutableServiceRepository.Instance.Get(_workspaceID, _resourceID);
            Assert.IsTrue(service != null && service.AssociatedServices.Count == 1);
        }

        [TestMethod]
        public void ThreeServicesAddedTwoRemovedExpectsOneRepository()
        {
            var guid = Guid.NewGuid();
            ExecutableServiceRepository.Instance.Clear();
            var service1 = GetExecutableService();
            var service2 = GetExecutableService();
            var service3 = GetExecutableService();
            service3.SetupGet(s => s.ID).Returns(guid);

            ExecutableServiceRepository.Instance.Add(service1.Object);
            ExecutableServiceRepository.Instance.Add(service2.Object);
            ExecutableServiceRepository.Instance.Add(service3.Object);

            ExecutableServiceRepository.Instance.Remove(service1.Object);
            Assert.IsTrue(ExecutableServiceRepository.Instance.Count == 1);

            var service = ExecutableServiceRepository.Instance.Get(_workspaceID, guid);
            Assert.IsTrue(service != null && service.AssociatedServices.Count == 0);
        }

        private Dictionary<string, StringBuilder> GetDictionary()
        {
            var dict = new Dictionary<string, StringBuilder>();
            dict["ResourceID"] = new StringBuilder(_resourceID.ToString());
            return dict;
        }

        private Mock<IWorkspace> GetWorkspace()
        {
            var mock = new Mock<IWorkspace>();
            mock.Setup(w => w.ID).Returns(_workspaceID);
            return mock;
        }

        private static Mock<IExecutableService> GetExecutableService()
        {
            var service = new Mock<IExecutableService>();
            service.SetupGet(s => s.ID).Returns(_resourceID);
            service.SetupGet(s => s.WorkspaceID).Returns(_workspaceID);
            service.SetupGet(s => s.AssociatedServices).Returns(new List<IExecutableService>());
            //service.Setup(s => s.Terminate()).Returns(async () => await TaskEx.FromResult("mock string")).Verifiable();
            return service;
        }

        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }
    }
}