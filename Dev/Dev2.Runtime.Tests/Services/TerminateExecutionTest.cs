/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Execution;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
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
            Assert.AreEqual("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", ds.DataListSpecification.ToString());
        }

        [TestMethod]
        public void ExecuteExpectSuccessResult()
        {
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
            const string Expected = "Message: Failed to stop the workflow execution. It may have completed already.";
            ExecutableServiceRepository.Instance.Clear();
            var terminateExecution = new TerminateExecution();
            var result = terminateExecution.Execute(GetDictionary(), GetWorkspace().Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.AreEqual(Expected, obj.Message.ToString());
        }

        [TestMethod]
        public void TwoServicesAddedExpectServiceWithOneAssociatedServiceFromRepository()
        {
            var service1 = GetExecutableService();
            var service2 = GetExecutableService();
            service2.SetupGet(executableService => executableService.ParentID).Returns(service1.Object.ID);
            ExecutableServiceRepository.Instance.Add(service1.Object);
            ExecutableServiceRepository.Instance.Add(service2.Object);
            var service = ExecutableServiceRepository.Instance.Get(WorkspaceID, ResourceID);
            Assert.IsTrue(service != null && service.AssociatedServices.Count == 1);
        }

        [TestMethod]
        public void ThreeServicesAddedOneRemovedExpectsTwoInRepository()
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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var terminateExecution = new TerminateExecution();

            //------------Execute Test---------------------------
            var resId = terminateExecution.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var terminateExecution = new TerminateExecution();

            //------------Execute Test---------------------------
            var resId = terminateExecution.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
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
