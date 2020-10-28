/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class ServiceTestExecutionContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_Constructor_GivenArgs_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(Datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_Execute_GivenArgs_ShouldPassThrough_ReturnsExecutedResults()
        {
            //---------------Set up test pack-------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction()
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService() { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test1";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());

            var fetch = JsonResource.Fetch("Sequence");
            var s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var cataLog = new Mock<ITestCatalog>();
            cataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            cataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, cataLog.Object, resourceCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
            });

            //---------------Test Result -----------------------
            try
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<ServiceTestModelTO>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
            }
            catch (Exception)
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<TestRunResult>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_Execute_AuthenticationType_Public_ReturnsExecutedResults()
        {
            //---------------Set up test pack-------------------
            var mockServiceTestInputs = new Mock<IEnumerable<IServiceTestInput>>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();

            var resourceId = Guid.NewGuid();

            const string TestName = "test1";
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var fetch = JsonResource.Fetch("Sequence");
            var jsonSerializer = new Dev2JsonSerializer();
            var testModelTO = jsonSerializer.Deserialize<ServiceTestModelTO>(fetch);
            var esbExecuteRequest = new EsbExecuteRequest();

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            mockDSFDataObject.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());
            mockDSFDataObject.Setup(o => o.ExecutingUser).Returns(GlobalConstants.GenericPrincipal);
            
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Public);
            mockTestCatalog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            mockTestCatalog.Setup(cat => cat.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            mockResourceCatalog.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockDev2Activity.Object);
            
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.AreEqual("00000000-0000-0000-0000-000000000000", execute.ToString());
            });
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_Execute_GivenStopExecutionAndUnAuthorized_ShouldAddFailureMessage()
        {
            //---------------Set up test pack-------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction()
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService() { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test1";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.StopExecution).Returns(true);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());
            dsfObj.Setup(o => o.IsDebugMode()).Returns(true);
            dsfObj.Setup(o => o.Environment.HasErrors()).Returns(true);
            dsfObj.Setup(o => o.Environment.FetchErrors()).Returns("Failed: The user running the test is not authorized to execute resource .");
            var fetch = JsonResource.Fetch("UnAuthorizedHelloWorld");
            var s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var cataLog = new Mock<ITestCatalog>();
            cataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            cataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, cataLog.Object, resourceCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = null;
            var identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
            Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");

            //---------------Test Result -----------------------


            try
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<ServiceTestModelTO>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
                dsfObj.Verify(o => o.IsDebugMode());
                dsfObj.Verify(o => o.Environment.HasErrors());
                dsfObj.Verify(o => o.Environment.FetchErrors());
                cataLog.Verify(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>()), Times.Never);
                cataLog.Verify(cat => cat.FetchTest(resourceId, TestName));
                Assert.AreNotEqual("", serviceTestModelTO.FailureMessage);
            }
            catch (Exception)
            {
                var testRunResult = esbExecuteRequest.ExecuteResult.DeserializeToObject<TestRunResult>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(testRunResult, "Execute results Deserialize returned Null.");
                Assert.IsNotNull(testRunResult, "Execute results Deserialize returned Null.");
                dsfObj.Verify(o => o.IsDebugMode());
                dsfObj.Verify(o => o.Environment.HasErrors());
                dsfObj.Verify(o => o.Environment.FetchErrors());
                cataLog.Verify(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>()), Times.Never);
                cataLog.Verify(cat => cat.FetchTest(resourceId, TestName));
                Assert.AreNotEqual("", testRunResult.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_CanExecute_GivenArgs_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(Datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer);
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            var execute = serviceTestExecutionContainer.CanExecute(Guid.NewGuid(), dsfObj.Object, AuthorizationContext.Administrator);
            //---------------Test Result -----------------------
            Assert.IsTrue(execute);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_ExecutingEvaluator_UpdateTestWithStepValues_Sets_The_Correct_FailureMessage()
        {
            //------------Setup for test-------------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test1";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.StopExecution).Returns(true);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());
            dsfObj.Setup(o => o.IsDebugMode()).Returns(true);
            dsfObj.Setup(o => o.Environment.HasErrors()).Returns(true);
            dsfObj.Setup(o => o.Environment.FetchErrors()).Returns("Failed: The user running the test is not authorized to execute resource .");
            var fetch = JsonResource.Fetch("UnAuthorizedHelloWorld");
            var s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var cataLog = new Mock<ITestCatalog>();
            cataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            cataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ExecutingEvaluator(dsfObj.Object, resourceCat.Object, workSpace.Object, esbExecuteRequest);
            var testObj = new PrivateType(serviceTestExecutionContainer.GetType());
            var test = SetupServiceTestSteps();
            testObj.InvokeStatic("UpdateTestWithStepValues", test.Object);
            //------------Execute Test---------------------------            
            var expectedMessage = @"Failed Step:  
Message: Test Failed because of some reasons
Failed Output For Variable:  
Message: This test has failed
Invalid Output for Variable: 
Message: This test is invalid
Pending Output for Variable: 
Test Failed because of some reasons
";
            //------------Assert Results-------------------------
            test.VerifySet(to =>
            {
                to.FailureMessage = expectedMessage.ToString();
            }, Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_ExecuteWf_GivenRecordSetsInputs_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test2";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());

            var fetch = JsonResource.Fetch("AssignWithRecSet");
            var s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var testCataLog = new Mock<ITestCatalog>();
            testCataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            testCataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, testCataLog.Object, resourceCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
            });
            //---------------Test Result -----------------------
            dsfObj.Verify(o => o.Environment.Assign("[[Person(1).Name]]", "Marry", 0), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_ExecuteWf_TestCatalog_Null_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
            });

            mockDSFDataObject.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_ExecuteWf_AuthenticationType_Windows_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_TryExecuteWf_Inputs_StartsWithAt_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_TryExecuteWf_DataObject_StopExecutionTrue_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
		[DoNotParallelize]
        public void ServiceTestExecutionContainer_TryExecuteWf_DataObject_StopExecutionFalse_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Returns(false);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
		[DoNotParallelize]
        public void ServiceTestExecutionContainer_ExecuteWf_DataObject_StopExecutionFalse_TestPassedTrue_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Returns(false);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);
            mockServiceTestModelTO.Setup(o => o.TestPassed).Returns(true);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
		[DoNotParallelize]
        public void ServiceTestExecutionContainer_TryExecuteWf_SetTestFailureBasedOnExpectedError_ErrorExpectedTrue_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";
            var failtureMsg = "test failure message111";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Returns(true);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);
            mockServiceTestModelTO.Setup(o => o.FailureMessage).Returns(failtureMsg);
            mockServiceTestModelTO.Setup(o => o.ErrorContainsText).Returns(failtureMsg);
            mockServiceTestModelTO.Setup(o => o.ErrorExpected).Returns(true);
            mockServiceTestModelTO.Setup(o => o.TestInvalid).Returns(true);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_TryExecuteWf_DataObject_StopExecution_ThrowInvalidWorkflowException_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            var failtureMsg = "test failure message111";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Throws<InvalidWorkflowException>();

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);
            mockServiceTestModelTO.Setup(o => o.ErrorContainsText).Returns(failtureMsg);
            mockServiceTestModelTO.Setup(o => o.ErrorExpected).Returns(true);
            mockServiceTestModelTO.Setup(o => o.TestInvalid).Returns(true);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
        public void ServiceTestExecutionContainer_TryExecuteWf_SetTestFailureBasedOnExpectedError_EnvironmentErrorsTrue_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";
            var failtureMsg = "test failure message111";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns("test_ParentServiceName");
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.StopExecution).Returns(true);
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);
            mockServiceTestModelTO.Setup(o => o.FailureMessage).Returns(failtureMsg);
            mockServiceTestModelTO.Setup(o => o.NoErrorExpected).Returns(true);
            mockServiceTestModelTO.Setup(o => o.Result).Returns(new TestRunResult());

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });

            mockDSFDataObject.VerifyAll();
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestExecutionContainer))]
		[DoNotParallelize]
        public void ServiceTestExecutionContainer_Execute_IsDebugTrue_EnvironmentHasErrors_Should_AssignAllRecordSetItems()
        {
            //------------Setup for test-------------------------
            var mockWorkspace = new Mock<IWorkspace>();
            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockTestCatalog = new Mock<ITestCatalog>();
            var mockServiceTestModelTO = new Mock<IServiceTestModelTO>();

            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            const string TestName = "test2";

            var esbExecuteRequest = new EsbExecuteRequest();
            var resourceId = Guid.NewGuid();
            var serviceAction = new ServiceAction
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService { ID = resourceId }
            };
            var json = @"{
                              CPU: 'Intel',
                              Drives: [
                                'DVD read/writer',
                                '500 gigabyte hard drive'
                              ]
                            }";

            mockDSFDataObject.SetupProperty(o => o.ResourceID);
            mockDSFDataObject.Setup(o => o.TestName).Returns(TestName);
            mockDSFDataObject.Setup(o => o.ParentServiceName).Returns(string.Empty);
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);
            mockDSFDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            mockDSFDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            mockDSFDataObject.Setup(o => o.IsDebug).Returns(true);

            mockServiceTestModelTO.Setup(o => o.Inputs).Returns(new List<IServiceTestInput> { new ServiceTestInputTO { Variable = "var", Value = "val" }, new ServiceTestInputTO { Variable = "[[@var]]", Value = json } });
            mockServiceTestModelTO.Setup(o => o.AuthenticationType).Returns(AuthenticationType.Windows);

            mockTestCatalog.Setup(o => o.FetchTest(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockServiceTestModelTO.Object);

            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, mockDSFDataObject.Object, mockWorkspace.Object, mockEsbChannel.Object, esbExecuteRequest, mockTestCatalog.Object, mockResourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out ErrorResultTO errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
                Assert.IsNotNull("{00000000-0000-0000-0000-000000000000}", execute.ToString());
            });
            
            mockServiceTestModelTO.VerifyAll();
            mockTestCatalog.VerifyAll();
            mockDSFDataObject.VerifyAll();
        }

        public class FakeWcfProxyService : IWcfProxyService
        {
            public IOutputDescription ExecuteWebService(WcfService src) => new OutputDescription();

            public object ExcecuteMethod(IWcfAction action, string endpointUrl)
            {
                return new object();
            }

            public Dictionary<MethodInfo, ParameterInfo[]> GetMethods(string endpoint) => new Dictionary<MethodInfo, ParameterInfo[]>();
            
            Dictionary<MethodInfo, ParameterInfo[]> IWcfProxyService.GetMethods(string endpoint)
            {
                throw new NotImplementedException();
            }
        }

        internal class ServiceTestExecutionContainerMock : ServiceTestExecutionContainer
        {
            public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
                : base(sa, dataObj, theWorkspace, esbChannel, request)
            {

            }
            public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, ITestCatalog catalog, IResourceCatalog resourceCatalog)
                : base(sa, dataObj, theWorkspace, esbChannel, request)
            {
                TstCatalog = catalog;
                ResourceCat = resourceCatalog;
            }
        }

        static Mock<IServiceTestModelTO> SetupServiceTestSteps()
        {
            var serviceTestModelTO = new Mock<IServiceTestModelTO>();
            var failingStep = new Mock<IServiceTestStep>();
            var failingTestRunResult = new TestRunResult
            {
                Message = "Test Failed because of some reasons",
                RunTestResult = RunResult.TestFailed,
                TestName = "Test 1",
            };
            failingStep.Setup(step => step.Result).Returns(failingTestRunResult);
            failingStep.Setup(step => step.Type).Returns(StepType.Assert);
            var steps = new List<IServiceTestStep>
            {
                failingStep.Object
            };
            var pendingOutput = new Mock<IServiceTestOutput>();
            var failingOutput = new Mock<IServiceTestOutput>();
            var invalidOutput = new Mock<IServiceTestOutput>();
            var pendingOutputResult = new TestRunResult
            {
                RunTestResult = RunResult.TestPending,
                Message = "This test is still pending",
                TestName = "Test 1"
            };
            var failingOutputResult = new TestRunResult
            {
                RunTestResult = RunResult.TestFailed,
                Message = "This test has failed",
                TestName = "Test 1"
            };
            var invalidOutputResult = new TestRunResult
            {
                RunTestResult = RunResult.TestInvalid,
                Message = "This test is invalid",
                TestName = "Test 1"
            };
            pendingOutput.Setup(output => output.Result).Returns(pendingOutputResult);
            failingOutput.Setup(output => output.Result).Returns(failingOutputResult);
            invalidOutput.Setup(output => output.Result).Returns(invalidOutputResult);
            var outputs = new List<IServiceTestOutput>
            {
                pendingOutput.Object,
                failingOutput.Object,
                invalidOutput.Object
            };

            serviceTestModelTO.Setup(to => to.TestSteps).Returns(steps);
            serviceTestModelTO.Setup(to => to.Outputs).Returns(outputs);
            return serviceTestModelTO;
        }
    }
}
