using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Tests.Runtime.Util;
using System.Diagnostics;
using System.Threading;
using Dev2.Core.Tests;
using Dev2.Studio.Core.Factories;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common;
using Dev2.Data;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Moq;

namespace Dev2.Integration.Tests
{
    [TestClass]
    [TestCategory("Load Tests")]
    public class Load_Tests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
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
        public void Single_Token_Perfomance_Op()
        {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Properties.TestStrings.tokenizerBase.ToStringBuilder() };


            dtb.AddTokenOp("-", false);

            var dt = dtb.Generate();

            var opCnt = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 100000)
            {
                dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            var exeTime = sw.ElapsedMilliseconds;

            Console.WriteLine(@"Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 100000 && exeTime < 1300, "Expecting it to take 1300 ms but it took " + exeTime + " ms.");
        }

        [TestMethod]
        public void Three_Token_Perfomance_Op()
        {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Properties.TestStrings.tokenizerBase.ToStringBuilder() };


            dtb.AddTokenOp("AB-", false);

            var dt = dtb.Generate();

            var opCnt = 0;
            var sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 35000)
            {
                dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            var exeTime = sw.ElapsedMilliseconds;

            Console.WriteLine("Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 35000 && exeTime < 2500, "It took [ " + exeTime + " ]");
        }

        [TestMethod]
        public void PulseTracker_Should()
        {
            var elapsed = false;
            var pulseTracker = new PulseTracker(2000);

            Assert.AreEqual(2000, pulseTracker.Interval);
            var pvt = new PrivateObject(pulseTracker);
            var timer = (System.Timers.Timer)pvt.GetField("_timer");
            timer.Elapsed += (sender, e) =>
            {
                elapsed = true;
            };
            Assert.AreEqual(false, timer.Enabled);
            pulseTracker.Start();
            Thread.Sleep(6000);
            Assert.IsTrue(elapsed);
        }

        [TestMethod]
        public void SortLargeListOfScalarsExpectedLessThan5500Milliseconds()
        {
            //Initialize
            var (_, _dataListViewModel) = DataListViewModelTests.Setup();
            for (var i = 2500; i > 0; i--)
            {
                _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("testVar" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0')));
            }
            var timeBefore = DateTime.Now;

            //Execute
            _dataListViewModel.SortCommand.Execute(null);

            var endTime = DateTime.Now.Subtract(timeBefore);
            //Assert
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar1000", _dataListViewModel.ScalarCollection[1000].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar1750", _dataListViewModel.ScalarCollection[1750].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar2500", _dataListViewModel.ScalarCollection[2500].DisplayName, "Sort datalist with large list failed");
            Assert.IsTrue(endTime < TimeSpan.FromMilliseconds(5500), $"Sort datalist took longer than 5500 milliseconds to sort 2500 variables. Took {endTime}");

            DataListViewModelTests.SortCleanup(_dataListViewModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void QueryManagerProxy_LoadExplorer_WhenLongerThan30Sec_ShouldLoadExplorerItemsShowPopup()
        {
            //------------Setup for test--------------------------
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            comms.Setup(a => a.CreateController("FetchExplorerItemsService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCompressedCommandAsync<IExplorerItem>(env.Object, It.IsAny<Guid>())).Returns(Task.Delay(70000).ContinueWith(t => new Mock<IExplorerItem>().Object));
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false)).Returns(MessageBoxResult.OK);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            //------------Execute Test---------------------------
            var item = queryManagerProxy.Load(false, mockPopupController.Object).Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockPopupController.Verify(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void QueryManagerProxy_LoadExplorer_WhenLongerThan30Sec__Localhost_ShouldLoadExplorerItemsNotShowPopup()
        {
            //------------Setup for test--------------------------
            var comms = new Mock<ICommunicationControllerFactory>();
            var env = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            env.Setup(a => a.WorkspaceID).Returns(Guid.NewGuid);
            env.Setup(a => a.DisplayName).Returns("localhost");
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(e => e.IsLocalHost).Returns(true);
            comms.Setup(a => a.CreateController("FetchExplorerItemsService")).Returns(controller.Object);
            controller.Setup(a => a.ExecuteCompressedCommandAsync<IExplorerItem>(env.Object, It.IsAny<Guid>())).Returns(Task.Delay(70000).ContinueWith(t => new Mock<IExplorerItem>().Object));
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false)).Returns(MessageBoxResult.OK);
            var queryManagerProxy = new QueryManagerProxy(comms.Object, env.Object);
            //------------Execute Test---------------------------
            var item = queryManagerProxy.Load(false, mockPopupController.Object).Result;
            //------------Assert Results-------------------------
            Assert.IsNotNull(item);
            mockPopupController.Verify(popup => popup.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Warning, "", false, false, true, false, false, false), Times.Never);
        }
    }
}