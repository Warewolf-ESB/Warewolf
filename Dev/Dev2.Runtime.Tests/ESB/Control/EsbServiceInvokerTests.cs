using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EsbServiceInvokerTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var pCounter = new Mock<IWarewolfPerformanceCounterLocater>();
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(new EmptyCounter());
            CustomContainer.Register(pCounter.Object);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoker, "Cannot create new EsbServiceInvoker object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DispatchDebugErrors_GivenObjects_ShouldWritesCorrectly()
        {
            //---------------Set up test pack-------------------
            //DispatchDebugErrors(ErrorResultTO errors, IDSFDataObject dataObject, StateType stateType)
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.IsDebugMode()).Returns(true);
            obj.SetupGet(o => o.DataListID).Verifiable();
            obj.SetupGet(o => o.WorkspaceID).Verifiable();
            obj.SetupGet(o => o.IsOnDemandSimulation).Verifiable();
            obj.SetupGet(o => o.ServiceName).Verifiable();
            obj.SetupGet(o => o.ServerID).Verifiable();
            obj.SetupGet(o => o.ResourceID).Verifiable();
            obj.SetupGet(o => o.OriginalInstanceID).Verifiable();
            obj.SetupGet(o => o.DebugSessionID).Verifiable();
            obj.SetupGet(o => o.EnvironmentID).Verifiable();
            obj.SetupGet(o => o.ClientID).Verifiable();

            ErrorResultTO errors = new ErrorResultTO();
            errors.AddError("Error");
            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object);
            PrivateObject privateObject = new PrivateObject(invoker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            privateObject.Invoke("DispatchDebugErrors", errors, obj.Object, StateType.Start);
            //---------------Test Result -----------------------
            obj.VerifyGet(o => o.DataListID);
            obj.VerifyGet(o => o.WorkspaceID);
            obj.VerifyGet(o => o.IsOnDemandSimulation);
            obj.VerifyGet(o => o.ServiceName);
            obj.VerifyGet(o => o.ServerID);
            obj.VerifyGet(o => o.ResourceID);
            obj.VerifyGet(o => o.OriginalInstanceID);
            obj.VerifyGet(o => o.DebugSessionID);
            obj.VerifyGet(o => o.EnvironmentID);
            obj.VerifyGet(o => o.ClientID);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeFalse_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, Guid.NewGuid(), false);
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionContainer);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueEmptyCacheNullService_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = null;
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(default(DynamicService));
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, Guid.NewGuid(), true);
            //---------------Test Result -----------------------
            Assert.IsNull(executionContainer);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueEmptyCacheNewService_ShouldAddToCache()
        {
            //---------------Set up test pack-------------------
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            var executeRequest = new EsbExecuteRequest();
            var serviceId = Guid.NewGuid();
            var dynamicService = new DynamicService()
            {
                ID = serviceId,
                Actions = new List<ServiceAction>
                {
                    new ServiceAction()
                    {
                        ServiceName = "serviceName",
                        ServiceID = serviceId,
                        SourceName = "sourceName",
                        ActionType = enActionType.Workflow
                    }
                }
            };
            obj.SetupGet(o => o.ResourceID).Returns(serviceId);
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dynamicService);
            locater.Setup(l => l.FindSourceByName(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new Source());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, serviceId, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionContainer);
            Assert.IsInstanceOfType(executionContainer, typeof(PerfmonExecutionContainer));
            locater.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueEmptyCacheNewService_IsTestExecution_ShouldAddToCache()
        {
            //---------------Set up test pack-------------------
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            var executeRequest = new EsbExecuteRequest();
            var serviceId = Guid.NewGuid();
            var dynamicService = new DynamicService()
            {
                ID = serviceId,
                Actions = new List<ServiceAction>
                {
                    new ServiceAction()
                    {
                        ServiceName = "serviceName",
                        ServiceID = serviceId,
                        SourceName = "sourceName",
                        ActionType = enActionType.Workflow
                    }
                }
            };
            obj.SetupGet(o => o.ResourceID).Returns(serviceId);
            obj.SetupGet(o => o.IsServiceTestExecution).Returns(true);
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(dynamicService);
            locater.Setup(l => l.FindSourceByName(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new Source());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, serviceId, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionContainer);
            Assert.IsInstanceOfType(executionContainer, typeof(ServiceTestExecutionContainer));
            locater.VerifyAll();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueCacheContainsInternalServiceService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeManagementDynamicService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = enActionType.InvokeManagementDynamicService } } });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, newGuid, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionContainer);
            obj.VerifyGet(o => o.ResourceID);
            var condition = executionContainer is InternalServiceContainer;
            Assert.IsTrue(condition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueCacheContainsWebServiceService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeWebService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = enActionType.InvokeManagementDynamicService } } });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            try
            {
                var executionContainer = invoker.GenerateInvokeContainer(obj.Object, newGuid, true);
                //---------------Test Result -----------------------
                Assert.IsNotNull(executionContainer);
                obj.VerifyGet(o => o.ResourceID);
                var condition = executionContainer is WebServiceContainer;
                Assert.IsTrue(condition);
            }
            catch (Exception e)
            {
                //Expected break for Web services, 
                Assert.AreEqual("Root element is missing.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueCacheContainsPerfmonExecutionService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.Workflow
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = enActionType.InvokeManagementDynamicService } } });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            try
            {
                var executionContainer = invoker.GenerateInvokeContainer(obj.Object, newGuid, true);
                //---------------Test Result -----------------------
                Assert.IsNotNull(executionContainer);
                obj.VerifyGet(o => o.ResourceID);
                var condition = executionContainer is PerfmonExecutionContainer;
                Assert.IsTrue(condition);
            }
            catch (Exception e)
            {
                //Expected break for Web services, 
                Assert.AreEqual("Root element is missing.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_masterDataListId_GivenValidArgsAndIsLocalInvokeTrueCacheContainsRemoteService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var serviceId = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(serviceId);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService
            {
                ID = serviceId,
                Actions =
                {
                    new ServiceAction
                    {
                        Name = "Name"
                        , ActionType = enActionType.InvokeManagementDynamicService
                        , ServiceID = serviceId
                        , SourceName = "SourceName"
                    }
                }
            });
            locater.Setup(lo => lo.FindSourceByName("SourceName", It.IsAny<Guid>())).Returns(new Source());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            try
            {
                var executionContainer = invoker.GenerateInvokeContainer(obj.Object, "Name", true, serviceId);
                //---------------Test Result -----------------------
                Assert.IsNotNull(executionContainer);
                obj.VerifyGet(o => o.ResourceID);
                var condition = executionContainer is InternalServiceContainer;
                Assert.AreEqual(1, _cache.Count);
                Assert.IsTrue(condition);
                locater.VerifyAll();
            }
            catch (Exception e)
            {
                //Expected break for Web services, 
                Assert.AreEqual("Root element is missing.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_masterDataListId_GivenValidArgsAndIsNotLocalInvoke_ShouldReturnRemoteExecutionContainer()
        {
            //---------------Set up test pack-------------------
            var serviceId = Guid.NewGuid();
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            locater.Setup(lo => lo.FindSourceByName("SourceName", It.IsAny<Guid>())).Returns(new Source());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------

            var executionContainer = invoker.GenerateInvokeContainer(obj.Object, "Name", false, serviceId);
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionContainer);
            var isRemoteWorkFlowExecution = executionContainer is RemoteWorkflowExecutionContainer;
            Assert.IsTrue(isRemoteWorkFlowExecution);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenNullServiceNameAndEmptyId_ShouldAddErrors()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            locater.Setup(lo => lo.FindSourceByName("SourceName", It.IsAny<Guid>())).Returns(new Source());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;
            invoker.Invoke(obj.Object, out errorResultTO);
            //---------------Test Result -----------------------
            Assert.IsNotNull(errorResultTO);
            Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
            Assert.AreEqual(Resources.DynamicServiceError_ServiceNotSpecified, errorResultTO.FetchErrors().Single());

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenServiceNameAndEmptyId_ShouldFindByName()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            obj.Setup(o => o.ServiceName).Returns("Hello World");
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            locater.Setup(lo => lo.FindService("Hello World", It.IsAny<Guid>())).Returns(new DynamicService());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;
            invoker.Invoke(obj.Object, out errorResultTO);
            //---------------Test Result -----------------------
            Assert.IsNotNull(errorResultTO);
            Assert.AreEqual(0, errorResultTO.FetchErrors().Count);
            locater.VerifyAll();

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenIsTestExecutionServiceNameAndEmptyId_ShouldFindByNameInLocalhost()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var valueFunction = Guid.NewGuid();
            workSpace.SetupGet(p => p.ID).Returns(valueFunction);
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            obj.Setup(o => o.ServiceName).Returns("Hello World");
            obj.Setup(o => o.IsServiceTestExecution).Returns(true);
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            locater.Setup(lo => lo.FindService("Hello World", valueFunction)).Returns((DynamicService)null);
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;
            invoker.Invoke(obj.Object, out errorResultTO);
            //---------------Test Result -----------------------
            Assert.IsNotNull(errorResultTO);
            Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
            StringAssert.Contains(errorResultTO.FetchErrors().Single(), ErrorResource.ServiceNotFound);
            locater.VerifyAll();
            // ReSharper disable once RedundantNameQualifier
            var toTypes = typeof(Dev2.Data.ServiceTestModelTO);
            var common = typeof(Dev2.Common.Interfaces.TestRunResult);
            var enumerable = toTypes.Assembly.ExportedTypes.Where(type => !type.IsInterface);
            var types = enumerable as Type[] ?? enumerable.ToArray();
            var allTypes = types.Union(common.Assembly.ExportedTypes.Where(type => !type.IsInterface));
            var serviceTestModelTO = (ServiceTestModelTO)executeRequest.ExecuteResult.ToString().DeserializeToObject(toTypes, new KnownTypesBinder()
            {
                KnownTypes = allTypes.ToList()
            });
            Assert.AreEqual(false, serviceTestModelTO.TestPassed);
            Assert.AreEqual(true, serviceTestModelTO.TestInvalid);
            Assert.AreEqual("Resource has been deleted", serviceTestModelTO.FailureMessage);
            Assert.IsTrue(serviceTestModelTO.Result.RunTestResult == RunResult.TestResourceDeleted);
            Assert.AreEqual("Resource has been deleted", serviceTestModelTO.Result.Message);
            Assert.AreEqual(0, serviceTestModelTO.Result.DebugForTest.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenIsTestExecution_ShouldResetTheActionTypeAfterTestExecution()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var valueFunction = Guid.NewGuid();
            workSpace.SetupGet(p => p.ID).Returns(valueFunction);
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            obj.Setup(o => o.ServiceName).Returns("Hello World");
            obj.Setup(o => o.IsServiceTestExecution).Returns(true);
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            var serviceAction = new ServiceAction()
            {
                ActionType = enActionType.Workflow
            };
            locater.Setup(lo => lo.FindService("Hello World", valueFunction)).Returns(new DynamicService()
            {
                ID = valueFunction,
                Actions = new List<ServiceAction>()
                {
                    serviceAction
                }
            });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;
            invoker.Invoke(obj.Object, out errorResultTO);
            //---------------Test Result -----------------------
            Assert.AreEqual(enActionType.Workflow, serviceAction.ActionType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenIsFromWebServerNotWorFlow_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var valueFunction = Guid.NewGuid();
            workSpace.SetupGet(p => p.ID).Returns(valueFunction);
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            obj.Setup(o => o.ServiceName).Returns("Hello World");
            obj.Setup(o => o.IsFromWebServer).Returns(true);
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            var serviceAction = new ServiceAction()
            {
                ActionType = enActionType.InvokeWebService
            };
            locater.Setup(lo => lo.FindService("Hello World", valueFunction)).Returns(new DynamicService()
            {
                ID = valueFunction,
                Actions = new List<ServiceAction>()
                {
                    serviceAction
                }
            });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;

            invoker.Invoke(obj.Object, out errorResultTO);

            //---------------Test Result -----------------------
            Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
            Assert.AreEqual(ErrorResource.CanOnlyExecuteWorkflowsFromWebBrowser, errorResultTO.FetchErrors().Single());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenInvalidAction_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var valueFunction = Guid.NewGuid();
            workSpace.SetupGet(p => p.ID).Returns(valueFunction);
            var obj = new Mock<IDSFDataObject>();
            obj.Setup(o => o.Environment.HasErrors()).Returns(false);
            obj.Setup(o => o.ServiceName).Returns("Hello World");
            obj.Setup(o => o.IsFromWebServer).Returns(true);
            var locater = new Mock<IServiceLocator>();
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            var serviceAction = new ServiceAction()
            {
                ActionType = enActionType.InvokeWebService
            };
            locater.Setup(lo => lo.FindService("Hello World", valueFunction)).Returns(new DynamicService()
            {
                ID = valueFunction,
                Actions = new List<ServiceAction>()
                {
                    serviceAction, new ServiceAction()
                }
            });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;

            invoker.Invoke(obj.Object, out errorResultTO);

            //---------------Test Result -----------------------
            Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
            Assert.AreEqual(string.Format(ErrorResource.MalformedService, Guid.Empty), errorResultTO.FetchErrors().Single());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dispose_GivenIsNew_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest))
            {

            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInvokeContainer_masterDataListId_GivenValidArgsAndIsLocalInvokeTrueCacheContainsPerfmonExecutionService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var serviceId = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(serviceId, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeManagementDynamicService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>"),
                ServiceID = serviceId
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(serviceId);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = serviceId, Actions = { new ServiceAction { Name = "Name", ActionType = enActionType.InvokeManagementDynamicService } } });
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(invoker);
            //---------------Execute Test ----------------------
            try
            {
                var executionContainer = invoker.GenerateInvokeContainer(obj.Object, "Name", true, serviceId);
                //---------------Test Result -----------------------
                Assert.IsNotNull(executionContainer);
                obj.VerifyGet(o => o.ResourceID);
                var condition = executionContainer is InternalServiceContainer;
                Assert.IsTrue(condition);
            }
            catch (Exception e)
            {
                //Expected break for Web services, 
                Assert.AreEqual("Root element is missing.", e.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetService_GivenThrowsExc_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            //GetService(string serviceName, Guid resourceId, IServiceLocator sl)
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeWebService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Throws(new Exception("error"));
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                privateObject.Invoke("GetService", string.Empty, newGuid);
            }
            catch (Exception e)
            {
                Assert.AreEqual("error", e.Message);
            }
            //---------------Test Result -----------------------

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetService_GivenEmptyGuid_ShouldFindByName()
        {
            //---------------Set up test pack-------------------
            //GetService(string serviceName, Guid resourceId, IServiceLocator sl)
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeWebService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new DynamicService());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = privateObject.Invoke("GetService", string.Empty, Guid.Empty) as DynamicService;
                Assert.IsNotNull(dynamicService);
                locater.Verify(l => l.FindService(It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
            }
            catch (Exception e)
            {
                Assert.AreEqual("error", e.Message);
            }
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetService_GivenGuid_ShouldFindByResourceId()
        {
            //---------------Set up test pack-------------------
            //GetService(string serviceName, Guid resourceId, IServiceLocator sl)
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = enActionType.InvokeWebService
                ,
                DataListSpecification = new StringBuilder("<DataList></DataList>")
            });
            //GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var obj = new Mock<IDSFDataObject>();
            var locater = new Mock<IServiceLocator>();
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            EsbExecuteRequest executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService());
            // ReSharper disable once ExpressionIsAlwaysNull

            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            PrivateObject privateObject = new PrivateObject(invoker);
            privateObject.SetField("_serviceLocator", locater.Object);
            privateObject.SetField("_cache", _cache);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = privateObject.Invoke("GetService", string.Empty, newGuid) as DynamicService;
                Assert.IsNotNull(dynamicService);
                locater.Verify(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            }
            catch (Exception e)
            {
                Assert.AreEqual("error", e.Message);
            }
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Invoke_GivenHasErrors_ShouldReturnResult()
        {
            //---------------Set up test pack-------------------
            //Invoke(IDSFDataObject dataObject, out ErrorResultTO errors)
            var serviceId = Guid.NewGuid();
            var newGuid = Guid.NewGuid();
            var obj = new Mock<IDSFDataObject>();
            var channel = new Mock<IEsbChannel>();
            var workSpace = new Mock<IWorkspace>();
            var env = new Mock<IExecutionEnvironment>();
            obj.Setup(o => o.Environment).Returns(env.Object);
            obj.Setup(o => o.ResourceID).Returns(serviceId).Verifiable();
            obj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>()).Verifiable();
            obj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>()).Verifiable();
            env.Setup(o => o.Errors).Returns(new HashSet<string>()).Verifiable();
            env.Setup(o => o.Errors).Returns(new HashSet<string>()).Verifiable();
            var executeRequest = new EsbExecuteRequest
            {
                Args = new Dictionary<string, StringBuilder>(),
                ServiceName = "SomeService"
            };
            var invoker = new EsbServiceInvoker(channel.Object, workSpace.Object, executeRequest);
            var locater = new Mock<IServiceLocator>();
            locater.Setup(l => l.FindService(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new DynamicService
            {
                Actions = new List<ServiceAction> { new ServiceAction { ActionType = enActionType.Workflow } },

            });
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService
            {
                Actions = new List<ServiceAction> { new ServiceAction { ActionType = enActionType.Workflow } },

            });

            PrivateObject privateObject = new PrivateObject(invoker);
            obj.SetupGet(o => o.ResourceID).Returns(newGuid);
            privateObject.SetField("_serviceLocator", locater.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                obj.Setup(o => o.Environment.HasErrors()).Returns(true).Verifiable(); ;
                obj.Setup(o => o.RemoteInvoke).Verifiable();
                obj.Setup(o => o.Environment.FetchErrors()).Returns("Error").Verifiable();
                ErrorResultTO errors;
                invoker.Invoke(obj.Object, out errors);
                //weird expetion on execution when getting errors
                Assert.AreEqual("Object reference not set to an instance of an object.", errors.FetchErrors().Single());
                //---------------Test Result -----------------------
                obj.Verify(o => o.Environment.FetchErrors());
                obj.VerifyGet(o => o.ResourceID);

            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }

        }

    }
}
