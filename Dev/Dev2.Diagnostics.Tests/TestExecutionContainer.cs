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
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class TestExecutionContainer
    {
        [TestInitialize]
        public void Init()
        {
            try
            {
                try
                {
                    PerformanceCounterCategory.Delete("Warewolf");
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }

                WarewolfPerformanceCounterRegister register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                            {
                                                                new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),   
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                new WarewolfNumberOfAuthErrors(),
                                                                new WarewolfServicesNotFoundCounter()
                                                            }, new List<IResourcePerformanceCounter>());

                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(),  register, new Mock<IPerformanceCounterPersistence>().Object));
            }
            catch (Exception err)
            {
                // ignored
                Dev2Logger.Error(err);
            }
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
        public void PerfmonContainer_Ctor_WrappedMethods()
        {
            var cont = new Cont();
            ErrorResultTO err;

            //------------Setup for test--------------------------
            var perfmonContainer = new PerfmonExecutionContainer(cont);

            //------------Execute Test---------------------------
            perfmonContainer.Execute(out err, 3);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, cont.CallCount);
            Assert.AreEqual(perfmonContainer.InstanceInputDefinition, "bob");
            Assert.AreEqual(perfmonContainer.InstanceOutputDefinition, "dave");
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.RequestsPerSecond).FromSafe(); ;

            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreNotEqual(0, innerCounter.RawValue);

            counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.AverageExecutionTime).FromSafe();

            po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreNotEqual(0, innerCounter.RawValue);
        }
    }

    class Cont : IEsbExecutionContainer
    {
        private string _instanceOutputDefinition;
        private string _instanceInputDefinition;
        private int _callCount;

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