using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Tests.Runtime.Util;
using System.Diagnostics;                
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data;
using System.Globalization;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Studio.Interfaces.DataList;
using Moq;
using Dev2.Studio.Interfaces;
using Dev2.Core.Tests;
using Caliburn.Micro;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core;

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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Properties.TestStrings.tokenizerBase };


            dtb.AddTokenOp("-", false);

            IDev2Tokenizer dt = dtb.Generate();

            var opCnt = 0;
            Stopwatch sw = new Stopwatch();
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
        [TestCategory("Load Tests")]
        public void Three_Token_Perfomance_Op()
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Properties.TestStrings.tokenizerBase };


            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            var opCnt = 0;
            Stopwatch sw = new Stopwatch();
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
        [TestCategory("Load Tests")]
        public void PulseTracker_Should()
        {
            var elapsed = false;
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

        [TestMethod]
        [TestCategory("Load Tests")]
        public void SortLargeListOfScalarsExpectedLessThan500Milliseconds()
        {
            //Initialize
            DataListViewModel _dataListViewModel;
            Mock<IContextualResourceModel> _mockResourceModel;
            _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);

            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IRecordSetItemModel carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));
            carRecordset.Input = true;
            carRecordset.Output = true;
            _dataListViewModel.RecsetCollection.Add(carRecordset);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));

            DataListSingleton.SetDataList(_dataListViewModel);
            for (var i = 2500; i > 0; i--)
            {
                _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("testVar" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0')));
            }
            var timeBefore = DateTime.Now;

            //Execute
            _dataListViewModel.SortCommand.Execute(null);

            TimeSpan endTime = DateTime.Now.Subtract(timeBefore);
            //Assert
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar1000", _dataListViewModel.ScalarCollection[1000].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar1750", _dataListViewModel.ScalarCollection[1750].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar2500", _dataListViewModel.ScalarCollection[2500].DisplayName, "Sort datalist with large list failed");
            Assert.IsTrue(endTime < TimeSpan.FromMilliseconds(500), $"Sort datalist took longer than 500 milliseconds to sort 2500 variables. Took {endTime}");

            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.RecsetCollection.Clear();

            carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.RecsetCollection.Add(carRecordset);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));
        }
    }
}
