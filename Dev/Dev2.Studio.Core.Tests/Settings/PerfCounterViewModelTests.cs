using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Diagnostics.Test;
using Dev2.Dialogs;
using Dev2.PerformanceCounters.Management;
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

        [TestInitialize]
        public void Setup()
        {

            AppSettings.LocalHost = "http://localhost:3142";
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ID).Returns(Guid.Empty);
            mockConnection.Setup(connection => connection.ServerID).Returns(Guid.Empty);
            mockEnvironment.Setup(model => model.Connection).Returns(mockConnection.Object);
            EnvironmentRepository.Instance.ActiveEnvironment = mockEnvironment.Object;
            CustomContainer.Register(new Mock<IShellViewModel>().Object);
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
    }

    public class TestPerfCounterViewModel:PerfcounterViewModel
    {
        public TestPerfCounterViewModel(IPerformanceCounterTo counters, IEnvironmentModel environment)
            : base(counters, environment)
        {
        }

        public TestPerfCounterViewModel(IPerformanceCounterTo counters, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
            : base(counters, environment, createfunc)
        {
        }

        public IResourcePickerDialog ResourcePickerDialog
        {
            get
            {
                return _resourcePicker;
            }
        }
    }
}
