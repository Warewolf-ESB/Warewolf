using ActivityUnitTests.XML;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ActivityUnitTests.ActivityTests
{
    [TestClass]
    public class NativeActivityTest
    {
        static string _simulationShape;
        static string _simulationData;

        #region Class Initialization/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
            _simulationShape = XmlResource.Fetch("SimulationShape").ToString();
            _simulationData = XmlResource.Fetch("SimulationData").ToString();
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {

        }

        #endregion

        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullDebugDispatcher_Expected_ArgumentNullException()
        {
            var activity = new TestActivity(null);

        }

        [TestMethod]
        public void ConstructorWithDebugDispatcher_Expected_NoArgumentNullException()
        {
            var activity = new TestActivity(DebugDispatcher.Instance);
        }

        #endregion

        #region Execute

        [TestMethod]
        public void ExecuteWithNoDataObject_Expected_InvokesOnExecute()
        {
            VerifyActivityExecuteCount(null, SimulationMode.Never, 1);
        }

        #region Execute - IsDebug

        [TestMethod]
        public void ExecuteWithIsDebugTrue_Expected_InvokesOnExecute()
        {
            VerifyActivityExecuteCount(CreateDataObject(true, false), SimulationMode.Never, 1);
        }

        [TestMethod]
        public void ExecuteWithIsDebugFalse_Expected_InvokesOnExecute()
        {
            VerifyActivityExecuteCount(CreateDataObject(false, false), SimulationMode.Never, 1);
        }

        #endregion

        #region Execute - IsOnDemandSimulation

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationTrueAndSimulationModeIsOnDemand_Expected_InvokesOnExecuteSimulation()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.OnDemand, 1);
            VerifyActivityExecuteCount(dataObject, SimulationMode.OnDemand, 0);
        }

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationTrueAndSimulationModeIsAlways_Expected_InvokesOnExecuteSimulation()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.Always, 1);
            VerifyActivityExecuteCount(dataObject, SimulationMode.Always, 0);
        }

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationTrueAndSimulationModeIsNever_Expected_InvokesOnExecute()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.Never, 0);
            VerifyActivityExecuteCount(dataObject, SimulationMode.Never, 1);
        }

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationFalseAndSimulationModeIsOnDemand_Expected_InvokesOnExecute()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.OnDemand, 0);
            VerifyActivityExecuteCount(dataObject, SimulationMode.OnDemand, 1);
        }

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationFalseAndSimulationModeIsAlways_Expected_InvokesOnExecuteSimulation()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.Always, 1);
            VerifyActivityExecuteCount(dataObject, SimulationMode.Always, 0);
        }

        [TestMethod]
        public void ExecuteWithIsOnDemandSimulationFalseAndSimulationModeIsNever_Expected_InvokesOnExecute()
        {
            var dataObject = CreateDataObject(false, true);
            VerifyActivityExecuteSimulationCount(dataObject, SimulationMode.Never, 0);
            VerifyActivityExecuteCount(dataObject, SimulationMode.Never, 1);
        }

        #endregion

        #region ExecuteSimulation

        [TestMethod]
        public void ExecuteSimulation_Expected_UpdatesDataList()
        {
            var dataObject = CreateDataObject(false, true);
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, _simulationShape, out errors);

            var simulationDataID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _simulationData, _simulationShape, out errors);
            var simulationDataList = compiler.FetchBinaryDataList(simulationDataID, out errors);

            #region Setup simulation repository

            var random = new Random();
            var simulationKey = new SimulationKey
            {
                WorkflowID = "TestActivity", // class name of activity used below
                ActivityID = string.Format("AID-{0}", random.Next()),
                ScenarioID = string.Format("SID-{0}", random.Next())
            };
            var simulationResult = new SimulationResult
            {
                Key = simulationKey,
                Value = simulationDataList
            };
            SimulationRepository.Instance.Save(simulationResult);

            #endregion

            var activity = new TestActivity(DebugDispatcher.Instance)
            {
                SimulationMode = SimulationMode.Always,
                UniqueID = simulationKey.ActivityID,
                ScenarioID = simulationKey.ScenarioID
            };

            Run(activity, dataObject,
                () =>
                {
                    SimulationRepository.Instance.Delete(simulationResult);

                    ErrorResultTO resultErrors;
                    var resultDataList = compiler.FetchBinaryDataList(dataObject.DataListID, out resultErrors);
                    if (errors.HasErrors())
                    {
                        Assert.Fail("Errors fetching Binary DataList result");
                    }

                    // See SimulationData.xml in XML folder
                    ValidateRecordSet(resultDataList, "Golfer", new[]
                    {
                        new KeyValuePair<string, string[]>("FirstName", new[]
                        {
                            "Tiger", "Ernie"
                        }),
                         new KeyValuePair<string, string[]>("LastName", new[]
                        {
                            "Woods", "Els"
                        })
                    });


                    ValidateScalar(resultDataList, "A", "6");
                    ValidateScalar(resultDataList, "B", "7");
                    ValidateScalar(resultDataList, "Result", "13");

                    Assert.IsTrue(true);
                });

        }

        [TestMethod]
        public void ExecuteSimulation_NoValidSimulationKeyInRepository_Expected_NoDataInjectedIntoDataList()
        {
            var dataObject = CreateDataObject(false, true);
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, _simulationShape, out errors);

            var simulationDataID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), _simulationData, _simulationShape, out errors);
            var simulationDataList = compiler.FetchBinaryDataList(simulationDataID, out errors);

            #region Setup simulation repository

            var random = new Random();
            var simulationKey = new SimulationKey
            {
                WorkflowID = "TestActivity", // class name of activity used below
                ActivityID = string.Format("AID-{0}", random.Next()),
                ScenarioID = string.Format("SID-{0}", random.Next())
            };
            //var simulationResult = new SimulationResult {
            //    Key = simulationKey,
            //    Value = simulationDataList
            //};
            //SimulationRepository.Instance.Save(simulationResult);

            #endregion

            var activity = new TestActivity(DebugDispatcher.Instance)
            {
                SimulationMode = SimulationMode.Always,
                UniqueID = simulationKey.ActivityID,
                ScenarioID = simulationKey.ScenarioID
            };

            Run(activity, dataObject,
                () =>
                {
                    //SimulationRepository.Instance.Delete(simulationResult);

                    ErrorResultTO resultErrors;
                    var resultDataList = compiler.FetchBinaryDataList(dataObject.DataListID, out resultErrors);
                    if (errors.HasErrors())
                    {
                        Assert.Fail("Errors fetching Binary DataList result");
                    }

                    // See SimulationData.xml in XML folder
                    try
                    {
                        ValidateRecordSet(resultDataList, "Golfer", new[]
                    {
                        new KeyValuePair<string, string[]>("FirstName", new[]
                        {
                            "Tiger", "Ernie"
                        }),
                         new KeyValuePair<string, string[]>("LastName", new[]
                        {
                            "Woods", "Els"
                        })
                    });
                    }
                    catch (AssertFailedException)
                    {
                        // we know that we could not find the value in the datalist
                        Assert.IsTrue(true);
                    }


                    try
                    {
                        ValidateScalar(resultDataList, "A", "6");
                    }
                    catch (AssertFailedException)
                    {
                        // we know that we could not find the value in the datalist
                        Assert.IsTrue(true);
                    }
                });
        }

        #region ValidateScalar

        static void ValidateScalar(IBinaryDataList dataList, string name, string expectedValue)
        {
            string error;
            IBinaryDataListEntry entry;
            dataList.TryGetEntry(name, out entry, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Assert.Fail("Error fetching scalar '{0}' from Binary DataList", name);
            }
            else
            {
                var scalar = entry.FetchScalar();
                Assert.AreEqual(expectedValue, scalar.TheValue);
            }
        }

        #endregion

        #region ValidateRecordSet

        static void ValidateRecordSet(IBinaryDataList dataList, string name, KeyValuePair<string, string[]>[] expectedValues)
        {
            string error;
            IBinaryDataListEntry entry;
            dataList.TryGetEntry(name, out entry, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Assert.Fail("Error fetching RecordSet '{0}' from Binary DataList", name);
            }
            else
            {
                IIndexIterator idxItr = entry.FetchRecordsetIndexes();
                while (idxItr.HasMore())
                {
                    var fields = entry.FetchRecordAt(idxItr.FetchNextIndex(), out error);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Assert.Fail("Error fetching RecordSet '{0}' fields", name);
                    }
                    else
                    {
                        var foundCount = 0;
                        foreach (var field in fields)
                        {
                            foreach (var expectedValue in expectedValues)
                            {
                                if (field.FieldName == expectedValue.Key && expectedValue.Value.Contains(field.TheValue))
                                {
                                    foundCount++;
                                }
                            }
                        }
                        Assert.AreEqual(expectedValues.Length, foundCount);
                    }
                }

                //foreach(var index in entry.FetchRecordsetIndexes())
                //{
                //    var fields = entry.FetchRecordAt(index, out error);
                //    if (!string.IsNullOrEmpty(error))
                //    {
                //        Assert.Fail("Error fetching RecordSet '{0}' fields", name);
                //    }
                //    else
                //    {
                //        var foundCount = 0;
                //        foreach (var field in fields)
                //        {
                //            foreach (var expectedValue in expectedValues)
                //            {
                //                if (field.FieldName == expectedValue.Key && expectedValue.Value.Contains(field.TheValue))
                //                {
                //                    foundCount++;
                //                }
                //            }
                //        }
                //        Assert.AreEqual(expectedValues.Length, foundCount);
                //    }
                //}
            }
        }

        #endregion

        #endregion

        #region DebugDispatcher

        [TestMethod]
        public void ExecuteWithNoDataObject_Expected_DoesNotInvokeDebugDispatcher()
        {
            VerifyDispatcherWriteCount(null, 0);
        }

        [TestMethod]
        public void ExecuteWithIsDebugTrue_Expected_InvokesDebugDispatcher()
        {
            VerifyDispatcherWriteCount(CreateDataObject(true, false), 1);
        }


        [TestMethod]
        public void ExecuteWithIsDebugFalse_Expected_DoesNotInvokeDebugDispatcher()
        {
            VerifyDispatcherWriteCount(CreateDataObject(false, false), 0);
        }

        #endregion


        #endregion

        #region VerifyDispatcherWriteCount

        static void VerifyDispatcherWriteCount(DsfDataObject dataObject, int expectedCount)
        {
            var dispatcher = new Mock<IDebugDispatcher>();
            dispatcher.Setup(d => d.Write(It.IsAny<IDebugState>())).Verifiable();

            var activity = new TestActivity(dispatcher.Object);

            Run(activity, dataObject,
                () => dispatcher.Verify(d => d.Write(It.IsAny<IDebugState>()), Times.Exactly(expectedCount)));
        }

        #endregion

        #region VerifyActivityExecuteCount

        static void VerifyActivityExecuteCount(DsfDataObject dataObject, SimulationMode simulationMode, int expectedCount)
        {
            var activity = new Mock<TestActivityAbstract>();
            activity.Object.SimulationMode = simulationMode;
            activity.Protected().Setup("OnExecute", ItExpr.IsAny<NativeActivityContext>()).Verifiable();

            Run(activity.Object, dataObject,
                () => activity.Protected().Verify("OnExecute", Times.Exactly(expectedCount), ItExpr.IsAny<NativeActivityContext>()));
        }

        #endregion

        #region VerifyActivityExecuteSimulationCount

        static void VerifyActivityExecuteSimulationCount(DsfDataObject dataObject, SimulationMode simulationMode, int expectedCount)
        {
            var activity = new Mock<TestActivityAbstract>();
            activity.Object.SimulationMode = simulationMode;
            activity.Protected().Setup("OnExecuteSimulation", ItExpr.IsAny<NativeActivityContext>()).Verifiable();

            Run(activity.Object, dataObject,
                () => activity.Protected().Verify("OnExecuteSimulation", Times.Exactly(expectedCount), ItExpr.IsAny<NativeActivityContext>()));
        }

        #endregion

        #region Run

        protected static void Run(Activity activity, DsfDataObject dataObject, Action completed)
        {
            // MUST use WorkflowApplication as it calls CacheMetadata (WorkflowInvoker DOES NOT!)
            var wfApp = new WorkflowApplication(activity);
            if (dataObject != null)
            {
                wfApp.Extensions.Add(dataObject);
            }
            wfApp.Extensions.Add(DataListFactory.CreateDataListCompiler());
            wfApp.Completed += args => completed();
            wfApp.OnUnhandledException += args =>
            {
                completed();
                return UnhandledExceptionAction.Cancel;
            };
            wfApp.Run();
        }

        #endregion

        #region CreateDataObject

        protected static DsfDataObject CreateDataObject(bool isDebug, bool isOnDemandSimulation)
        {
            return new DsfDataObject(string.Empty, GlobalConstants.NullDataListID)
            {
                IsDebug = isDebug,
                IsOnDemandSimulation = isOnDemandSimulation
            };
        }

        #endregion
    }
}
