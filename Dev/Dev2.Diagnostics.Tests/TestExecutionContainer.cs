using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class TestExecutionContainer
    {
        Mock<IRealPerformanceCounterFactory> _mockPerformanceCounterFactory;
        IRealPerformanceCounterFactory _performanceCounterFactory;
        IWarewolfPerformanceCounterLocater _performanceCounterLocater;

        [TestInitialize]
        public void Init()
        {
            try
            {
                PerformanceCounterCategory.Delete("Warewolf");
            }

            catch
            {
                //Do Nothing
            }
            _mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            _performanceCounterFactory = _mockPerformanceCounterFactory.Object;
            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                        {
                                                            new WarewolfCurrentExecutionsPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfErrors(_performanceCounterFactory),
                                                            new WarewolfRequestsPerSecondPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfAverageExecutionTimePerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfAuthErrors(_performanceCounterFactory),
                                                            new WarewolfServicesNotFoundCounter(_performanceCounterFactory),
                                                        }, new List<IResourcePerformanceCounter>());

            _performanceCounterLocater = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, _performanceCounterFactory);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(_performanceCounterLocater);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerfmonContainer_Ctor")]
        public void PerfmonContainer_Ctor_ValidArg_No_Error()
        {
            var cont = new Mock<IEsbExecutionContainer>();
            //------------Setup for test--------------------------
            var perfmonContainer = new PerfmonExecutionContainer(cont.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(perfmonContainer.Container, cont.Object);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerfmonContainer_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void PerfmonContainer_Ctor_InValidArg_Has_Error()
        {
            var cont = new Mock<IEsbExecutionContainer>();
            //------------Setup for test--------------------------
            var perfmonContainer = new PerfmonExecutionContainer(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerfmonContainer_Ctor")]
        [DoNotParallelize]
        public void PerfmonContainer_Ctor_WrappedMethods()
        {
            var cont = new Cont();
            var mockPerformanceCounter = new Mock<IWarewolfPerformanceCounter>();
            var mockPerformanceCounter2 = new Mock<IWarewolfPerformanceCounter>();
            var mockPerformanceCounter3 = new Mock<IWarewolfPerformanceCounter>();
            var mockPerformanceCounter4 = new Mock<IWarewolfPerformanceCounter>();
            _mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, It.IsAny<string>(), GlobalConstants.GlobalCounterName)).Throws(new Exception("no other counters expected to be created"));
            _mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName)).Returns(mockPerformanceCounter.Object);
            _mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName)).Returns(mockPerformanceCounter2.Object);
            _mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName)).Returns(mockPerformanceCounter3.Object);
            _mockPerformanceCounterFactory.Setup(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName)).Returns(mockPerformanceCounter4.Object);

            //------------Setup for test--------------------------
            var perfmonContainer = new PerfmonExecutionContainer(cont);

            //------------Execute Test---------------------------
            perfmonContainer.Execute(out ErrorResultTO err, 3);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, cont.CallCount);
            Assert.AreEqual(perfmonContainer.InstanceInputDefinition, "bob");
            Assert.AreEqual(perfmonContainer.InstanceOutputDefinition, "dave");
            var counter = _performanceCounterLocater.GetCounter(WarewolfPerfCounterType.RequestsPerSecond).FromSafe();

            _mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Request Per Second", GlobalConstants.GlobalCounterName), Times.Once);
            _mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Concurrent requests currently executing", GlobalConstants.GlobalCounterName), Times.Once);
            _mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "Average workflow execution time", GlobalConstants.GlobalCounterName), Times.Once);
            _mockPerformanceCounterFactory.Verify(o => o.New(GlobalConstants.Warewolf, "average time per operation base", GlobalConstants.GlobalCounterName), Times.Once);
            mockPerformanceCounter.Verify(o => o.Increment(), Times.Once);
            mockPerformanceCounter2.Verify(o => o.Increment(), Times.Once);
            mockPerformanceCounter3.Verify(o => o.IncrementBy(It.IsAny<long>()), Times.Once);
            mockPerformanceCounter4.Verify(o => o.Increment(), Times.Once);

            counter = _performanceCounterLocater.GetCounter(WarewolfPerfCounterType.AverageExecutionTime).FromSafe();
        }
    }

    class Cont : IEsbExecutionContainer
    {
        string _instanceOutputDefinition;
        string _instanceInputDefinition;
        int _callCount;

        #region Implementation of IEsbExecutionContainer

        public Cont()
        {
            InstanceInputDefinition = "bob";
            InstanceOutputDefinition = "dave";
        }

        public Guid Execute(out ErrorResultTO errors, int update)
        {
            CallCount++;
            errors = new ErrorResultTO();
            return new Guid();

        }

        public IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            CallCount++;
            return null;
        }

        public int CallCount
        {
            get
            {
                return _callCount;
            }
            set
            {
                _callCount = value;
            }
        }

        public string InstanceOutputDefinition
        {
            get
            {
                return _instanceOutputDefinition;
            }
            set
            {
                _instanceOutputDefinition = value;
            }
        }
        public string InstanceInputDefinition
        {
            get
            {
                return _instanceInputDefinition;
            }
            set
            {
                _instanceInputDefinition = value;
            }
        }

        public IDSFDataObject GetDataObject()
        {

            return new DsfDataObject(string.Empty, Guid.NewGuid());
        }

        #endregion
    }
}