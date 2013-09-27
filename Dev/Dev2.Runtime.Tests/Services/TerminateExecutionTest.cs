#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TerminateExecutionTest
    {
        private static readonly Guid _workspaceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static readonly Guid _resourceID = Guid.Parse("34c0ce48-1f02-4a47-ad51-19ee3789ed4c");
        private static object _syncRoot = new object();
        private string _handleType = "TerminateExecutionService";

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
            Assert.AreEqual(enActionType.InvokeManagementDynamicService ,ds.Actions.First().ActionType);
            Assert.AreEqual("<DataList><Roles/><ResourceID/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>",ds.DataListSpecification);
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
            Assert.IsTrue(result == "<Result>Message: Workflow succesfully terminated</Result>");
        }

        [TestMethod]
        public void ExecuteExpectFailResultIfNoServiceExist()
        {
            ExecutableServiceRepository.Instance.Clear();
            const string expected = "<Result>Message: Termination of workflow failed</Result>";
            var terminateExecution = new TerminateExecution();
            var result = terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);
            Assert.IsTrue(result == expected);
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

        private IDictionary<string, string> GetDictionary()
        {
            var dict = new Dictionary<string, string>();
            dict["Roles"] = TestResources.TestRoles;
            dict["ResourceID"] = _resourceID.ToString();
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
    }
}