using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Simulation;
using Dev2.Tests.Activities.XML;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
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
            new TestActivity(null);
        }

        [TestMethod]
        public void ConstructorWithDebugDispatcher_Expected_NoArgumentNullException()
        {
            new TestActivity(DebugDispatcher.Instance);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfNativeActivity_Constructor")]
        public void DsfNativeActivity_Constructor_OnError_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var activity = new TestActivity(new Mock<IDebugDispatcher>().Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(string.IsNullOrEmpty(activity.OnErrorVariable));
            Assert.IsTrue(string.IsNullOrEmpty(activity.OnErrorWorkflow));
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfNativeActivity_Constructor")]
        public void DsfNativeActivity_Constructor_OnErrorVariable_HasFindMissingAttribute()
        {
            //------------Setup for test--------------------------
            var type = typeof(DsfNativeActivity<string>);
            var property = type.GetProperty("OnErrorVariable");

            //------------Execute Test---------------------------
            var attribute = property.GetCustomAttribute<FindMissingAttribute>();

            //------------Assert Results-------------------------
            Assert.IsNotNull(attribute);
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
                    if(errors.HasErrors())
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
            compiler.FetchBinaryDataList(simulationDataID, out errors);

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
                    if(errors.HasErrors())
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
                    catch(AssertFailedException)
                    {
                        // we know that we could not find the value in the datalist
                        Assert.IsTrue(true);
                    }


                    try
                    {
                        ValidateScalar(resultDataList, "A", "6");
                    }
                    catch(AssertFailedException)
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
            if(!string.IsNullOrEmpty(error))
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
            if(!string.IsNullOrEmpty(error))
            {
                Assert.Fail("Error fetching RecordSet '{0}' from Binary DataList", name);
            }
            else
            {
                IIndexIterator idxItr = entry.FetchRecordsetIndexes();
                while(idxItr.HasMore())
                {
                    var fields = entry.FetchRecordAt(idxItr.FetchNextIndex(), out error);
                    if(!string.IsNullOrEmpty(error))
                    {
                        Assert.Fail("Error fetching RecordSet '{0}' fields", name);
                    }
                    else
                    {
                        var foundCount = 0;
                        foreach(var field in fields)
                        {
                            foreach(var expectedValue in expectedValues)
                            {
                                if(field.FieldName == expectedValue.Key && expectedValue.Value.Contains(field.TheValue))
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
            dispatcher.Setup(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>())).Verifiable();

            var activity = new TestActivity(dispatcher.Object);

            Run(activity, dataObject,
                () => dispatcher.Verify(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()), Times.Exactly(expectedCount)));
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

        // BUG 9304 - 2013.05.08 - TWR - Refactored this to use expanded method signature
        protected static void Run(Activity activity, DsfDataObject dataObject, Action completed)
        {
            Run(activity, dataObject, null, (ex, outputs) => completed());
        }

        // BUG 9304 - 2013.05.08 - TWR - Expanded method signature and made public
        public static void Run(Activity activity, DsfDataObject dataObject, Dictionary<string, object> inputArgs, Action<Exception, IDictionary<string, object>> completed)
        {
            // MUST use WorkflowApplication as it calls CacheMetadata (WorkflowInvoker DOES NOT!)
            var wfApp = inputArgs == null ? new WorkflowApplication(activity) : new WorkflowApplication(activity, inputArgs);
            if(dataObject != null)
            {
                wfApp.Extensions.Add(dataObject);
            }
            wfApp.Extensions.Add(DataListFactory.CreateDataListCompiler());
            wfApp.Completed += args => completed(null, args.Outputs);
            wfApp.OnUnhandledException += args =>
            {
                completed(args.UnhandledException, null);
                return UnhandledExceptionAction.Cancel;
            };
            wfApp.Run();
        }

        #endregion

        #region CreateDataObject

        // BUG 9304 - 2013.05.08 - TWR - Made public
        public static DsfDataObject CreateDataObject(bool isDebug, bool isOnDemandSimulation)
        {
            return new DsfDataObject(string.Empty, GlobalConstants.NullDataListID)
            {
                IsDebug = isDebug,
                IsOnDemandSimulation = isOnDemandSimulation
            };
        }

        #endregion

        #region InitializeDebugState

        [TestMethod]
        [TestCategory("DsfNativeActivity_InitializeDebugState")]
        [Description("DsfNativeActivity InitializeDebugState must set the DebugState's properties.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfNativeActivity_UnitTest_InitializeDebugState_SetsPropertiesCorrectly()
        {
            var parentInstanceID = Guid.NewGuid();
            var remoteID = Guid.NewGuid();
            const bool HasError = true;
            const string ErrorMessage = "Error occurred..";
            const StateType StateType = StateType.All;

            var dataObj = new DsfDataObject(string.Empty, GlobalConstants.NullDataListID)
            {
                IsDebug = true,
                IsOnDemandSimulation = false,
                EnvironmentID = Guid.NewGuid(),
                WorkspaceID = Guid.NewGuid(),
                ServerID = Guid.NewGuid(),
                ResourceID = Guid.NewGuid(),
                OriginalInstanceID = Guid.NewGuid(),
                ParentInstanceID = parentInstanceID.ToString()
            };

            var activity = new TestActivity(new Mock<IDebugDispatcher>().Object)
            {
                IsSimulationEnabled = false,
                SimulationMode = SimulationMode.Never,
                ScenarioID = Guid.NewGuid().ToString(),
                IsWorkflow = true,
            };

            var actual = activity.TestInitializeDebugState(StateType, dataObj, remoteID, HasError, ErrorMessage);

            Assert.AreEqual(activity.UniqueGuid, actual.ID, "DispatchDebugState did not set the DebugState's ID.");
            Assert.AreEqual(parentInstanceID, actual.ParentID, "DispatchDebugState did not set the DebugState's ParentID.");
            Assert.AreEqual(StateType, actual.StateType, "DispatchDebugState did not set the DebugState's StateType.");
            Assert.AreEqual(HasError, actual.HasError, "DispatchDebugState did not set the DebugState's HasError.");
            Assert.AreEqual(ErrorMessage, actual.ErrorMessage, "DispatchDebugState did not set the DebugState's ErrorMessage.");
            Assert.AreEqual(remoteID.ToString(), actual.Server, "DispatchDebugState did not set the DebugState's Server.");

            Assert.AreEqual(dataObj.WorkspaceID, actual.WorkspaceID, "DispatchDebugState did not set the DebugState's WorkspaceID.");
            Assert.AreEqual(dataObj.ServerID, actual.ServerID, "DispatchDebugState did not set the DebugState's ServerID.");
            Assert.AreEqual(dataObj.ResourceID, actual.OriginatingResourceID, "DispatchDebugState did not set the DebugState's OriginatingResourceID.");
            Assert.AreEqual(dataObj.OriginalInstanceID, actual.OriginalInstanceID, "DispatchDebugState did not set the DebugState's OriginalInstanceID.");
            Assert.AreEqual(activity.DisplayName, actual.DisplayName, "DispatchDebugState did not set the DebugState's DisplayName.");
            Assert.IsFalse(actual.IsSimulation, "DispatchDebugState did not set the DebugState's IsSimulation.");
            Assert.AreEqual(ActivityType.Workflow, actual.ActivityType, "DispatchDebugState did not set the DebugState's ActivityType.");

            Assert.AreEqual(dataObj.EnvironmentID, actual.EnvironmentID, "DispatchDebugState did not set the DebugState's EnvironmentID.");
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNativeActivity_GetForEachItems")]
        public void DsfNativeActivity_GetForEachItems_NullStringList_EmptyList()
        {
            //------------Setup for test--------------------------
            var dsfNativeActivity = new TestNativeActivity(false, "Test");

            //------------Execute Test---------------------------
            var forEachItemsForTest = dsfNativeActivity.GetForEachItemsForTest(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(forEachItemsForTest.Any());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNativeActivity_GetForEachItems")]
        public void DsfNativeActivity_GetForEachItems_EmptyStringList_EmptyList()
        {
            //------------Setup for test--------------------------
            var dsfNativeActivity = new TestNativeActivity(false, "Test");

            //------------Execute Test---------------------------
            var forEachItemsForTest = dsfNativeActivity.GetForEachItemsForTest(new string[0]);
            //------------Assert Results-------------------------
            Assert.IsFalse(forEachItemsForTest.Any());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNativeActivity_GetForEachItems")]
        public void DsfNativeActivity_GetForEachItems_WhenHasStringItems_ReturnsForEachItemList()
        {
            //------------Setup for test--------------------------
            var dsfNativeActivity = new TestNativeActivity(false, "Test");
            const string item1NameAndValue = "Test";
            const string item2NameAndValue = "Test1";
            var stringList = new[] { item1NameAndValue, item2NameAndValue };
            //------------Execute Test---------------------------
            var forEachItemsForTest = dsfNativeActivity.GetForEachItemsForTest(stringList);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, forEachItemsForTest.Count);
            Assert.IsNotNull(forEachItemsForTest.FirstOrDefault(item => item.Name == item2NameAndValue));
            Assert.IsNotNull(forEachItemsForTest.FirstOrDefault(item => item.Value == item2NameAndValue));
            Assert.IsNotNull(forEachItemsForTest.FirstOrDefault(item => item.Name == item1NameAndValue));
            Assert.IsNotNull(forEachItemsForTest.FirstOrDefault(item => item.Value == item1NameAndValue));
        }
    }

    internal class TestNativeActivity : DsfNativeActivity<string>
    {

        public TestNativeActivity(bool isExecuteAsync, string displayName)
            : base(isExecuteAsync, displayName)
        {
        }

        public TestNativeActivity(bool isExecuteAsync, string displayName, IDebugDispatcher debugDispatcher)
            : base(isExecuteAsync, displayName, debugDispatcher)
        {
        }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public IList<DsfForEachItem> GetForEachItemsForTest(params string[] strings)
        {
            return GetForEachItems(strings);
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        #endregion
    }
}
