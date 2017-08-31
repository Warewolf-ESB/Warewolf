using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
    public class ExecutionManagerTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_Instance")]
        public void ExecutionManager_Instance_Accessed_ShouldHaveAllDefaults()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var executionManager =  new ExecutionManager();
            //------------Assert Results-------------------------
            Assert.IsNotNull(executionManager);
            Assert.IsFalse(executionManager.IsRefreshing);
        }

        public static ExecutionManager GetConstructedExecutionManager()
        {
            return new ExecutionManager();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_Instance")]
        public void ExecutionManager_StartRefreshing_ShouldSetIsRefreshingTrue()
        {
            //------------Setup for test--------------------------
            var executionManager = GetConstructedExecutionManager();
            //------------Execute Test---------------------------
            executionManager.StartRefresh();
            //------------Assert Results-------------------------
            Assert.IsNotNull(executionManager);
            Assert.IsTrue(executionManager.IsRefreshing);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_Instance")]
        public void ExecutionManager_StopRefreshing_ShouldSetIsRefreshingTrue()
        {
            //------------Setup for test--------------------------
            var executionManager = GetConstructedExecutionManager();
            //------------Execute Test---------------------------
            executionManager.StartRefresh();
            //------------Assert Results-------------------------
            Assert.IsNotNull(executionManager);
            Assert.IsTrue(executionManager.IsRefreshing);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_AddExecution")]
        public void ExecutionManager_AddExecution_ShouldIncreaseExecutionsCounts()
        {
            //------------Setup for test--------------------------
            var executionManager = GetConstructedExecutionManager();
            PrivateObject p = new PrivateObject(executionManager);
            //------------Execute Test---------------------------
            executionManager.AddExecution();
            //------------Assert Results-------------------------
            var currentExecutionsValue = p.GetFieldOrProperty("_currentExecutions");
            Assert.IsNotNull(currentExecutionsValue);
            Assert.AreEqual(1, currentExecutionsValue);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_AddExecution")]
        public void ExecutionManager_CompleteExecution_ShouldDecreaseExecutionsCounts()
        {
            //------------Setup for test--------------------------
            var executionManager = GetConstructedExecutionManager();
            PrivateObject p = new PrivateObject(executionManager);
            //------------PreExecution Asserts-------------------
            executionManager.AddExecution();
            var currentExecutionsValue = p.GetFieldOrProperty("_currentExecutions");
            Assert.IsNotNull(currentExecutionsValue);
            Assert.AreEqual(1, currentExecutionsValue);
            //------------Execute Test---------------------------
            executionManager.CompleteExecution();
            var updatedCurrentExecutionsValue = p.GetFieldOrProperty("_currentExecutions");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, updatedCurrentExecutionsValue);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionManager_StopExecution")]
        public void ExecutionManager_StopRefresh_AfterWaitHandlesSet_ShouldSetToFalse()
        {
            //------------Setup for test--------------------------
            var executionManager = GetConstructedExecutionManager();
            PrivateObject p = new PrivateObject(executionManager);
            var _threadTracker = false;
            var t = new Thread(()=>
            {
                executionManager.Wait();
                _threadTracker = true;
            });
            t.Start();
            //------------Execute Test---------------------------
            executionManager.StopRefresh();
            Thread.Sleep(1000);
            //------------Assert Results-------------------------
            Assert.IsTrue(_threadTracker);
        }
    }
}
