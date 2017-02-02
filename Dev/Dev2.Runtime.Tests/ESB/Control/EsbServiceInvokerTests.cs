using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
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
        public void GenerateInvokeContainer_GivenValidArgsAndIsLocalInvokeTrueCacheContainsInternalServiceService_ShouldCorrectServiceInContainer()
        {
            //---------------Set up test pack-------------------
            var newGuid = Guid.NewGuid();
            var _cache = new ConcurrentDictionary<Guid, ServiceAction>();
            _cache.TryAdd(newGuid, new ServiceAction
            {
                Name = "Name"
                ,
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService
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
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService } } });
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
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService
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
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService } } });
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
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.Workflow
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
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService { ID = newGuid, Actions = { new ServiceAction { Name = "Name", ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService } } });
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
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService
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
                privateObject.Invoke("GetService", string.Empty, newGuid, locater.Object);
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
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService
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
                var dynamicService = privateObject.Invoke("GetService", string.Empty, Guid.Empty, locater.Object) as DynamicService;
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
                ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService
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
                var dynamicService = privateObject.Invoke("GetService", string.Empty, newGuid, locater.Object) as DynamicService;
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
                Actions = new List<ServiceAction> { new ServiceAction { ActionType = Common.Interfaces.Core.DynamicServices.enActionType.Workflow } },

            });
            locater.Setup(l => l.FindService(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DynamicService
            {
                Actions = new List<ServiceAction> { new ServiceAction { ActionType = Common.Interfaces.Core.DynamicServices.enActionType.Workflow } },

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
