
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
        private static readonly Guid WorkspaceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static readonly Guid ResourceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static readonly object SyncRoot = new object();
        private const string HandleType = "TerminateExecutionService";

        [TestInitialize]
        public void TerminateExecutionInit()
        {
            Monitor.Enter(SyncRoot);
        }

        [TestCleanup]
        public void TerminateExecutionCleanup()
        {
            Monitor.Exit(SyncRoot);
        }

        [TestMethod]
        public void CreateServiceEntryExpectsDynamicService()
        {
            var terminateExecution = new TerminateExecution();
            var ds = terminateExecution.CreateServiceEntry();
            Assert.AreEqual(HandleType, terminateExecution.HandlesType());
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

            const string Expected = "Message: Workflow successfully terminated";
            var obj = ConvertToMsg(result.ToString());

            Assert.AreEqual(Expected, obj.Message.ToString());

        }

        [TestMethod]
        public void ExecuteExpectFailResultIfNoServiceExist()
        {
            ExecutableServiceRepository.Instance.Clear();
            const string Expected = "Message: Failed to stop the workflow execution. It may have completed already.";
            var terminateExecution = new TerminateExecution();
            var result = terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.AreEqual(Expected, obj.Message.ToString());
        }

        [TestMethod]
        public void TwoServicesAddedExpectServiceWithOneAssociatedServiceFromRepository()
        {
            ExecutableServiceRepository.Instance.Clear();
            var service1 = GetExecutableService();
            var service2 = GetExecutableService();
            service2.SetupGet(executableService => executableService.ParentID).Returns(service1.Object.ID);
            ExecutableServiceRepository.Instance.Add(service1.Object);
            ExecutableServiceRepository.Instance.Add(service2.Object);
            var service = ExecutableServiceRepository.Instance.Get(WorkspaceID, ResourceID);
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
            Assert.AreEqual(2, ExecutableServiceRepository.Instance.Count);

            var service = ExecutableServiceRepository.Instance.Get(WorkspaceID, guid);
            Assert.AreEqual(0, service.AssociatedServices.Count);
        }

        private Dictionary<string, StringBuilder> GetDictionary()
        {
            var dict = new Dictionary<string, StringBuilder>();
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            dict["ResourceID"] = new StringBuilder(ResourceID.ToString());
            return dict;
        }

        private Mock<IWorkspace> GetWorkspace()
        {
            var mock = new Mock<IWorkspace>();
            mock.Setup(w => w.ID).Returns(WorkspaceID);
            return mock;
        }

        private static Mock<IExecutableService> GetExecutableService()
        {
            var service = new Mock<IExecutableService>();
            service.SetupGet(s => s.ID).Returns(ResourceID);
            service.SetupGet(s => s.WorkspaceID).Returns(WorkspaceID);
            service.SetupGet(s => s.AssociatedServices).Returns(new List<IExecutableService>());
            return service;
        }

        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }
    }
}