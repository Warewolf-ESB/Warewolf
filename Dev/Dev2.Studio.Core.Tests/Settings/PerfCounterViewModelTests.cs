using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Diagnostics.Test;
using Dev2.Dialogs;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Management;
using Dev2.Services.Security;
using Dev2.Settings.Perfcounters;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Dev2.Core.Tests.Settings
{
    /// <summary>
    /// Summary description for PerfCounterViewModelTests
    /// </summary>
    [TestClass]
    public class PerfCounterViewModelTests
    {
        private Mock<IEnvironmentModel> _mockEnvironment;
        private Mock<IEnvironmentConnection> _mockConnection;

        [TestInitialize]
        public void Setup()
        {

            AppSettings.LocalHost = "http://localhost:3142";
            _mockEnvironment = new Mock<IEnvironmentModel>();
            _mockConnection = new Mock<IEnvironmentConnection>();
            _mockConnection.Setup(connection => connection.ID).Returns(Guid.Empty);
            _mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            _mockEnvironment.Setup(model => model.Connection).Returns(_mockConnection.Object);
            EnvironmentRepository.Instance.ActiveEnvironment = _mockEnvironment.Object;
            CustomContainer.Register(new Mock<IShellViewModel>().Object);
        }

        [TestMethod]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_ServerCountersCompare_Given_Null_Server_Counters_Returns_False()
        {
            var authorizationService = new Mock<IAuthorizationService>();
            var securityService = new Mock<ISecurityService>();
            var permissions = new List<WindowsGroupPermission> { new WindowsGroupPermission() };
            securityService.Setup(service => service.Permissions).Returns(permissions);
            authorizationService.Setup(service => service.SecurityService).Returns(securityService.Object);
            _mockEnvironment.Setup(model => model.AuthorizationService).Returns(authorizationService.Object);
            EnvironmentRepository.Instance.ActiveEnvironment = _mockEnvironment.Object;
            var counters = new PrivateType(typeof(PerfcounterViewModel));
            //------------Setup for test------------------------
            //------------Execute Test--------------------------
            var invokeStatic = counters.InvokeStatic("GetEnvironment");
            //------------Assert Results-------------------------
            Assert.IsNotNull(invokeStatic);
        }

        [TestMethod]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_Equals_Given_Null_Server_Counters_Returns_False()
        {
            var perfCounterTo = new Mock<IPerformanceCounterTo>();
            perfCounterTo.Setup(to => to.ResourceCounters).Returns(new List<IResourcePerformanceCounter>());
            perfCounterTo.Setup(to => to.NativeCounters).Returns(new List<IPerformanceCounter>());
            var perfcounterViewModel = new PerfcounterViewModel(perfCounterTo.Object, new Mock<IEnvironmentModel>().Object);
            var counters = new PrivateObject(perfcounterViewModel);
            //------------Setup for test--------------------------
            var ItemServerCounters = perfcounterViewModel.ServerCounters = null;
            //------------Execute Test---------------------------
            var areEqual = counters.Invoke("Equals", args: new object[] { null, ItemServerCounters });
            //------------Assert Results-------------------------
            Assert.IsFalse(areEqual.Equals(true));
        }

        [TestMethod]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_Equals_Given_Null_Resource_Counters_Returns_False()
        {
            var perfCounterTo = new Mock<IPerformanceCounterTo>();
            perfCounterTo.Setup(to => to.ResourceCounters).Returns(new List<IResourcePerformanceCounter>());
            perfCounterTo.Setup(to => to.NativeCounters).Returns(new List<IPerformanceCounter>());
            var perfcounterViewModel = new PerfcounterViewModel(perfCounterTo.Object, new Mock<IEnvironmentModel>().Object);
            var counters = new PrivateObject(perfcounterViewModel);

            var ItemServerCounters = new List<IPerformanceCountersByMachine>();
            var ItemResourceCounters = perfcounterViewModel.ResourceCounters = null;
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var areEqual = counters.Invoke("Equals", args: new object[] { ItemServerCounters, ItemResourceCounters });
            //------------Assert Results-------------------------
            Assert.IsFalse(areEqual.Equals(true));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PerfcounterViewModel_Constructor_NullPerfCounters_ThrowsException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            new PerfcounterViewModel(null, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PerfcounterViewModel_Constructor_NullEnvironment_ThrowsException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            new PerfcounterViewModel(new PerformanceCounterTo(), null, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_Constructor_ResourcePickerFunction_ShouldCreateResourcePicker()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var perfcounterViewModel = new TestPerfCounterViewModel(new PerformanceCounterTo(), new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(perfcounterViewModel.ResourcePickerDialog);            
        }        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_Constructor_WithPerfCounters_ShouldSetupCollections()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));

            //------------Execute Test---------------------------
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1,perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2,perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            Assert.IsNotNull(serverCounter);
            Assert.IsTrue(serverCounter.AverageExecutionTime);
            Assert.IsTrue(serverCounter.ConcurrentRequests);
            Assert.IsFalse(serverCounter.NotAuthorisedErrors);
            Assert.IsFalse(serverCounter.RequestPerSecond);
            Assert.IsFalse(serverCounter.TotalErrors);
            Assert.IsFalse(serverCounter.WorkFlowsNotFound);
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            Assert.IsFalse(resourceCounter.IsNew);
            Assert.IsTrue(resourceCounter.AverageExecutionTime);
            Assert.IsTrue(resourceCounter.RequestPerSecond);
            Assert.IsFalse(resourceCounter.ConcurrentRequests);
            Assert.IsFalse(resourceCounter.TotalErrors);
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);
            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_UpdatePerfCounter_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1, perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2, perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);            
            Assert.IsNotNull(serverCounter);
            Assert.IsNotNull(resourceCounter);
            //------------Execute Test---------------------------
            serverCounter.TotalErrors = true;
            newResourceCounter.CounterName = "new resource";
            //------------Assert Results-------------------------
            Assert.IsTrue(perfcounterViewModel.IsDirty);
            Assert.IsTrue(serverCounter.TotalErrors);
            Assert.AreEqual(3,perfcounterViewModel.ResourceCounters.Count);
            Assert.IsFalse(newResourceCounter.IsNew);
            var newNewResourceCounter = perfcounterViewModel.ResourceCounters[2];
            Assert.IsTrue(newNewResourceCounter.IsNew);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_UpdatePerfCounter_ResourceCounterSetCounterNameEmpty_ShouldRemoveCounter()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1, perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2, perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);            
            Assert.IsNotNull(serverCounter);
            Assert.IsNotNull(resourceCounter);
            //------------Execute Test---------------------------
            resourceCounter.CounterName = "";
            //------------Assert Results-------------------------
            Assert.IsTrue(perfcounterViewModel.IsDirty);
            Assert.AreEqual(1,perfcounterViewModel.ResourceCounters.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_UpdatePerfCounter_ResourceCounterSetCounterNameNull_ShouldRemoveCounter()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1, perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2, perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);            
            Assert.IsNotNull(serverCounter);
            Assert.IsNotNull(resourceCounter);
            //------------Execute Test---------------------------
            resourceCounter.CounterName = null;
            //------------Assert Results-------------------------
            Assert.IsTrue(perfcounterViewModel.IsDirty);
            Assert.AreEqual(1,perfcounterViewModel.ResourceCounters.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_UpdatePerfCounter_ResourceCounterSetCounterNameNull_IsDirtyFalse()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1, perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2, perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);
            Assert.IsNotNull(serverCounter);
            Assert.IsNotNull(resourceCounter);
            //------------Execute Test---------------------------
            resourceCounter.TotalErrors = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(perfcounterViewModel.IsDirty);

            resourceCounter.TotalErrors = false;
            Assert.IsFalse(perfcounterViewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_Save_ShouldUpdatePerfCounterTo()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime,false));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests,false));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ExecutionErrors,false));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.NotAuthorisedErrors,false));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.RequestsPerSecond,false));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ServicesNotFound,false));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);
            Assert.IsNotNull(serverCounter);
            serverCounter.TotalErrors = true;
            serverCounter.AverageExecutionTime = true;
            serverCounter.ConcurrentRequests = true;
            serverCounter.NotAuthorisedErrors = true;
            serverCounter.RequestPerSecond = true;
            serverCounter.WorkFlowsNotFound = true;
            newResourceCounter.CounterName = "new resource";
            var newResourceId = Guid.NewGuid();
            newResourceCounter.ResourceId = newResourceId;
            newResourceCounter.RequestPerSecond=true;
            newResourceCounter.TotalErrors=true;
            newResourceCounter.AverageExecutionTime=true;
            newResourceCounter.ConcurrentRequests=true;
            //------------Execute Test---------------------------
            perfcounterViewModel.Save(performanceCounterTo);
            //------------Assert Results-------------------------
            Assert.AreEqual(6,performanceCounterTo.NativeCounters.Count);
            Assert.IsTrue(performanceCounterTo.NativeCounters[0].IsActive);
            Assert.IsTrue(performanceCounterTo.NativeCounters[1].IsActive);
            Assert.IsTrue(performanceCounterTo.NativeCounters[2].IsActive);
            Assert.IsTrue(performanceCounterTo.NativeCounters[3].IsActive);
            Assert.IsTrue(performanceCounterTo.NativeCounters[4].IsActive);
            Assert.IsTrue(performanceCounterTo.NativeCounters[5].IsActive);
            Assert.AreEqual(6,performanceCounterTo.ResourceCounters.Count);
            Assert.IsTrue(performanceCounterTo.ResourceCounters.Any(counter => counter.PerfCounterType==WarewolfPerfCounterType.AverageExecutionTime));
            Assert.IsTrue(performanceCounterTo.ResourceCounters.Any(counter => counter.PerfCounterType==WarewolfPerfCounterType.RequestsPerSecond));
            Assert.IsTrue(performanceCounterTo.ResourceCounters.Any(counter => counter.PerfCounterType==WarewolfPerfCounterType.ConcurrentRequests));
            Assert.IsTrue(performanceCounterTo.ResourceCounters.Any(counter => counter.PerfCounterType==WarewolfPerfCounterType.ExecutionErrors));
            Assert.AreEqual("new resource",performanceCounterTo.ResourceCounters[2].CategoryInstanceName);
            Assert.AreEqual(newResourceId,performanceCounterTo.ResourceCounters[2].ResourceId);
            Assert.AreEqual("new resource", performanceCounterTo.ResourceCounters[3].CategoryInstanceName);
            Assert.AreEqual(newResourceId, performanceCounterTo.ResourceCounters[3].ResourceId);
            Assert.AreEqual("new resource", performanceCounterTo.ResourceCounters[4].CategoryInstanceName);
            Assert.AreEqual(newResourceId, performanceCounterTo.ResourceCounters[4].ResourceId);
            Assert.AreEqual("new resource", performanceCounterTo.ResourceCounters[5].CategoryInstanceName);
            Assert.AreEqual(newResourceId, performanceCounterTo.ResourceCounters[5].ResourceId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_Constructor")]
        public void PerfcounterViewModel_UpdatePerfCounter_ResourceCounterSetDeleted_ShouldNotSaveCounter()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(perfcounterViewModel.ServerCounters);
            Assert.IsNotNull(perfcounterViewModel.ResourceCounters);
            Assert.AreEqual(1, perfcounterViewModel.ServerCounters.Count);
            Assert.AreEqual(2, perfcounterViewModel.ResourceCounters.Count);
            var serverCounter = perfcounterViewModel.ServerCounters[0];
            var resourceCounter = perfcounterViewModel.ResourceCounters[0];
            var newResourceCounter = perfcounterViewModel.ResourceCounters[1];
            Assert.IsTrue(newResourceCounter.IsNew);
            Assert.IsNotNull(serverCounter);
            Assert.IsNotNull(resourceCounter);
            Assert.IsFalse(resourceCounter.IsDeleted);
            //------------Execute Test---------------------------
            resourceCounter.RemoveRow.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(resourceCounter.IsDeleted);
            perfcounterViewModel.Save(performanceCounterTo);
            Assert.AreEqual(1, perfcounterViewModel.ResourceCounters.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_ResetCounters")]
        public void PerfcounterViewModel_ResetCounters_Command_NoError_ShouldCallCommunicationsController()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<MessageBoxButton>(),It.IsAny<MessageBoxImage>(),It.IsAny<string>(),It.IsAny<bool>(),It.IsAny<bool>(),It.IsAny<bool>(),It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            CustomContainer.Register(mockPopupController.Object);
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var mockCommsController = new Mock<ICommunicationController>();
            mockCommsController.SetupAllProperties();
            var executeMessage = new ExecuteMessage { HasError = false };
            mockCommsController.Setup(controller => controller.ExecuteCommand<IExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>())).Returns(executeMessage);
            perfcounterViewModel.CommunicationController = mockCommsController.Object;
            //------------Execute Test---------------------------
            perfcounterViewModel.ResetCountersCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("ResetPerformanceCounters", perfcounterViewModel.CommunicationController.ServiceName);
            mockCommsController.Verify(controller => controller.ExecuteCommand<IExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()));
            mockPopupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_ResetCounters")]
        public void PerfcounterViewModel_ResetCounters_Command_Error_ShouldCallCommunicationsController()
        {
            //------------Setup for test--------------------------
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(controller => controller.Show(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<MessageBoxButton>(),It.IsAny<MessageBoxImage>(),It.IsAny<string>(),It.IsAny<bool>(),It.IsAny<bool>(),It.IsAny<bool>(),It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Verifiable();
            CustomContainer.Register(mockPopupController.Object);
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var mockCommsController = new Mock<ICommunicationController>();
            mockCommsController.SetupAllProperties();
            var executeMessage = new ExecuteMessage { HasError = true,Message=new StringBuilder("Error") };
            mockCommsController.Setup(controller => controller.ExecuteCommand<IExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>())).Returns(executeMessage);
            perfcounterViewModel.CommunicationController = mockCommsController.Object;
            //------------Execute Test---------------------------
            perfcounterViewModel.ResetCountersCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("ResetPerformanceCounters", perfcounterViewModel.CommunicationController.ServiceName);
            mockCommsController.Verify(controller => controller.ExecuteCommand<IExecuteMessage>(It.IsAny<IEnvironmentConnection>(), It.IsAny<Guid>()));
            mockPopupController.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_PickResource")]
        public void PerfcounterViewModel_PickResource_WhenCounterNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);            
            //------------Execute Test---------------------------
            perfcounterViewModel.PickResourceCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_PickResource")]
        public void PerfcounterViewModel_PickResource_WhenNotCounterPassed_DoesNothing()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);            
            //------------Execute Test---------------------------
            perfcounterViewModel.PickResourceCommand.Execute(new object());
            //------------Assert Results-------------------------
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_PickResource")]
        public void PerfcounterViewModel_PickResource_WhenCounterPassed_SetsBasedOnResource()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            mockResourcePicker.Setup(dialog => dialog.ShowDialog(It.IsAny<IEnvironmentModel>())).Returns(true);
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            Guid newGuid = Guid.NewGuid();
            mockExplorerTreeItem.Setup(item => item.ResourceId).Returns(newGuid);
            mockExplorerTreeItem.Setup(item => item.ResourcePath).Returns("Hello World");
            mockExplorerTreeItem.Setup(item => item.ResourceName).Returns("Hello World");
            mockResourcePicker.Setup(dialog => dialog.SelectedResource).Returns(mockExplorerTreeItem.Object);
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, mockEnvironmentModel.Object, () => mockResourcePicker.Object);
            var performanceCountersByResource = perfcounterViewModel.ResourceCounters[0];
            //------------Execute Test---------------------------
            perfcounterViewModel.PickResourceCommand.Execute(performanceCountersByResource);
            //------------Assert Results-------------------------
            Assert.AreEqual(newGuid,performanceCountersByResource.ResourceId);
            Assert.AreEqual("Hello World",performanceCountersByResource.CounterName);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_PickResource")]
        public void PerfcounterViewModel_PickResource_WhenNoResource_DoesNothing()
        {
            //------------Setup for test--------------------------
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            mockResourcePicker.Setup(dialog => dialog.ShowDialog(It.IsAny<IEnvironmentModel>())).Returns(false);
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            Guid newGuid = Guid.NewGuid();
            mockExplorerTreeItem.Setup(item => item.ResourceId).Returns(newGuid);
            mockExplorerTreeItem.Setup(item => item.ResourcePath).Returns("Hello World");
            mockExplorerTreeItem.Setup(item => item.ResourceName).Returns("Hello World");
            mockResourcePicker.Setup(dialog => dialog.SelectedResource).Returns(mockExplorerTreeItem.Object);
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, mockEnvironmentModel.Object, () => mockResourcePicker.Object);
            var performanceCountersByResource = perfcounterViewModel.ResourceCounters[0];
            //------------Execute Test---------------------------
            perfcounterViewModel.PickResourceCommand.Execute(performanceCountersByResource);
            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId, performanceCountersByResource.ResourceId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("PerfcounterViewModel_UpdateHelpDescriptor")]
        public void PerfcounterViewModel_UpdateHelpDescriptor_HelpText_ShouldCallUpdateHelpText()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpWindowViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpWindowViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpWindowViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var performanceCounterTo = new PerformanceCounterTo();
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.AverageExecutionTime));
            performanceCounterTo.NativeCounters.Add(new TestCounter(WarewolfPerfCounterType.ConcurrentRequests));
            var resourceId = Guid.NewGuid();
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.AverageExecutionTime, resourceId));
            performanceCounterTo.ResourceCounters.Add(new TestResourceCounter(WarewolfPerfCounterType.RequestsPerSecond, resourceId));
            var perfcounterViewModel = new PerfcounterViewModel(performanceCounterTo, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);            
            //------------Execute Test---------------------------
            perfcounterViewModel.UpdateHelpDescriptor("Help");
            //------------Assert Results-------------------------
            mockHelpWindowViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()));
        }
    }

    public class TestPerfCounterViewModel:PerfcounterViewModel
    {
        public TestPerfCounterViewModel(IPerformanceCounterTo counters, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
            : base(counters, environment, createfunc)
        {
        }

        public IResourcePickerDialog ResourcePickerDialog => _resourcePicker;
    }
}
