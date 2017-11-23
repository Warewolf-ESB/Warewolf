using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Tests.Runtime.Util;
using System.Diagnostics;
using Dev2.Integration.Tests.Properties;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class Load_Tests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Load Tests")]
        public void ExecutionManager_StartRefresh_WaitsForCurrentExecutions()
        {
            //------------Setup for test--------------------------
            var executionManager = ExecutionManagerTests.GetConstructedExecutionManager();
            executionManager.AddExecution();
            var stopwatch = new Stopwatch();
            var t = new Thread(() =>
            {
                stopwatch.Start();
                Thread.Sleep(2000);
                executionManager.CompleteExecution();
            });
            t.Start();
            //------------Execute Test---------------------------
            executionManager.StartRefresh();
            stopwatch.Stop();
            //------------Assert Results-------------------------
            Assert.IsTrue(stopwatch.ElapsedMilliseconds > 2000, stopwatch.ElapsedMilliseconds.ToString());
        }

        [TestMethod]
        [TestCategory("Load Tests")]
        public void Single_Token_Perfomance_Op()
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = TestStrings.tokenizerBase };


            dtb.AddTokenOp("-", false);

            IDev2Tokenizer dt = dtb.Generate();

            int opCnt = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 100000)
            {
                dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            long exeTime = sw.ElapsedMilliseconds;
            
            Console.WriteLine(@"Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 100000 && exeTime < 1300, "Expecting it to take 1300 ms but it took " + exeTime + " ms.");
        }

        [TestMethod]
        [TestCategory("Load Tests")]
        public void Three_Token_Perfomance_Op()
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = TestStrings.tokenizerBase };


            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            int opCnt = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 35000)
            {
                dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            long exeTime = sw.ElapsedMilliseconds;
            
            Console.WriteLine("Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 35000 && exeTime < 2500, "It took [ " + exeTime + " ]");
        }

        [TestMethod]
        public void PulseTracker_Should()
        {
            bool elapsed = false;
            var pulseTracker = new PulseTracker(2000);

            Assert.AreEqual(2000, pulseTracker.Interval);
            PrivateObject pvt = new PrivateObject(pulseTracker);
            System.Timers.Timer timer = (System.Timers.Timer)pvt.GetField("_timer");
            timer.Elapsed += (sender, e) =>
            {
                elapsed = true;
            };
            Assert.AreEqual(false, timer.Enabled);
            pulseTracker.Start();
            Thread.Sleep(6000);
            Assert.IsTrue(elapsed);
        }
    }
}
