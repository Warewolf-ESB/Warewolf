/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ActivityUnitTests;
using ActivityUnitTests.ActivityTest;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]

    public class NativeActivityTest : BaseActivityUnitTest
    {

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {

        }

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

        static void VerifyDispatcherWriteCount(DsfDataObject dataObject, int expectedCount)
        {
            var dispatcher = new Mock<IDebugDispatcher>();
            dispatcher.Setup(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>())).Verifiable();

            var activity = new TestActivity(dispatcher.Object);

            Run(activity, dataObject,
                () => dispatcher.Verify(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()), Times.Exactly(expectedCount)));
        }

        protected static void Run(Activity activity, DsfDataObject dataObject, Action completed)
        {
            Run(activity, dataObject, null, (ex, outputs) => completed?.Invoke());
        }

        public static void Run(Activity activity, DsfDataObject dataObject, Dictionary<string, object> inputArgs, Action<Exception, IDictionary<string, object>> completed)
        {
            // MUST use WorkflowApplication as it calls CacheMetadata (WorkflowInvoker DOES NOT!)
            var wfApp = inputArgs == null ? new WorkflowApplication(activity) : new WorkflowApplication(activity, inputArgs);
            if (dataObject != null)
            {
                wfApp.Extensions.Add(dataObject);
            }
            wfApp.Completed += args => completed?.Invoke(null, args.Outputs);
            wfApp.OnUnhandledException += args =>
            {
                completed?.Invoke(args.UnhandledException, null);
                return UnhandledExceptionAction.Cancel;
            };
            wfApp.Run();
        }

        public static DsfDataObject CreateDataObject(bool isDebug, bool isOnDemandSimulation)
        {
            return new DsfDataObject(string.Empty, GlobalConstants.NullDataListID)
            {
                IsDebug = isDebug,
                IsOnDemandSimulation = isOnDemandSimulation
            };
        }

        [TestMethod]
        [TestCategory("DsfNativeActivity_InitializeDebugState")]
        [Description("DsfNativeActivity InitializeDebugState must set the DebugState's properties.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfNativeActivity_UnitTest_InitializeDebugState_SetsPropertiesCorrectly()
        {
            var parentInstanceID = Guid.NewGuid();
            var remoteID = Guid.Empty;
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
                ScenarioID = Guid.NewGuid().ToString(),
                IsWorkflow = true,
            };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            activity.ResourceCatalog = cat.Object;
            var actual = activity.TestInitializeDebugState(StateType, dataObj, remoteID, HasError, ErrorMessage);

            Assert.AreEqual(activity.UniqueGuid, actual.ID, "DispatchDebugState did not set the DebugState's ID.");
            Assert.AreEqual(parentInstanceID, actual.ParentID, "DispatchDebugState did not set the DebugState's ParentID.");
            Assert.AreEqual(StateType, actual.StateType, "DispatchDebugState did not set the DebugState's StateType.");
            Assert.AreEqual(HasError, actual.HasError, "DispatchDebugState did not set the DebugState's HasError.");
            Assert.AreEqual(ErrorMessage, actual.ErrorMessage, "DispatchDebugState did not set the DebugState's ErrorMessage.");
            Assert.AreEqual("localhost", actual.Server, "DispatchDebugState did not set the DebugState's Server.");

            Assert.AreEqual(dataObj.WorkspaceID, actual.WorkspaceID, "DispatchDebugState did not set the DebugState's WorkspaceID.");
            Assert.AreEqual(dataObj.ServerID, actual.ServerID, "DispatchDebugState did not set the DebugState's ServerID.");
            Assert.AreEqual(dataObj.ResourceID, actual.OriginatingResourceID, "DispatchDebugState did not set the DebugState's OriginatingResourceID.");
            Assert.AreEqual(dataObj.OriginalInstanceID, actual.OriginalInstanceID, "DispatchDebugState did not set the DebugState's OriginalInstanceID.");
            Assert.AreEqual(activity.DisplayName, actual.DisplayName, "DispatchDebugState did not set the DebugState's DisplayName.");
            Assert.IsFalse(actual.IsSimulation, "DispatchDebugState did not set the DebugState's IsSimulation.");
            Assert.AreEqual(ActivityType.Workflow, actual.ActivityType, "DispatchDebugState did not set the DebugState's ActivityType.");

            Assert.AreEqual(dataObj.DebugEnvironmentId, actual.EnvironmentID, "DispatchDebugState did not set the DebugState's EnvironmentID.");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfNativeActivity_DispatchDebugState_Before_SetsStartTimeCorrectly()
        {
            var parentInstanceID = Guid.NewGuid();
            const StateType StateType = StateType.Before;

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

            var activity = new TestActivity
            {
                IsSimulationEnabled = false,
                ScenarioID = Guid.NewGuid().ToString(),
                IsWorkflow = true,
            };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            activity.ResourceCatalog = cat.Object;
            var actual = activity.TestDispatchDebugState(dataObj, StateType);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.StartTime);
            Assert.AreNotEqual(DateTime.MinValue, actual.StartTime);
            Assert.AreEqual(DateTime.MinValue, actual.EndTime);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfNativeActivity_DispatchDebugState_After_SetsEndTimeCorrectly()
        {
            var parentInstanceID = Guid.NewGuid();
            const StateType StateType = StateType.After;

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
            IDebugState passedDebugState = null;
            var mockDebugDispatcher = new Mock<IDebugDispatcher>();
            mockDebugDispatcher.Setup(dispatcher => dispatcher.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()))
                .Callback((IDebugState ds, bool isTestExecution, bool isDebugFromWeb, string testName, bool isRemoteInvoke, string remoteID, string parentId, IList<IDebugState> remoteItems) =>
                  {
                      passedDebugState = ds;
                  });
            var activity = new TestActivity(mockDebugDispatcher.Object)
            {
                IsSimulationEnabled = false,
                ScenarioID = Guid.NewGuid().ToString(),
                IsWorkflow = true,
            };
            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            activity.ResourceCatalog = cat.Object;
            activity.TestDispatchDebugState(dataObj, StateType);
            Assert.IsNotNull(passedDebugState);
            Assert.IsNotNull(passedDebugState.EndTime);
            Assert.AreNotEqual(DateTime.MinValue, passedDebugState.StartTime);
            Assert.AreNotEqual(DateTime.MinValue, passedDebugState.EndTime);
        }


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
            var forEachItemsForTest = dsfNativeActivity.GetForEachItemsForTest();
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

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfActivity_UpdateDebugParentID")]

        public void DsfActivity_UpdateDebugParentID_UniqueIdSameIfNestingLevelNotChanged()

        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
            };

            var act = new TestNativeActivity(false, "bob");
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfNativeActivity_UpdateDebugParentID")]

        public void DsfNativeActivity_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelIncreased()

        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 1
            };

            var act = new TestNativeActivity(false, "bob");
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNativeActivity_UpdateDebugParentID")]
        public void DsfNativeActivity_ExecuteTool_DebugFalse_TestExecution()
        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                ServerID = Guid.NewGuid(),
                IsDebug = false,
                ForEachNestingLevel = 0,
                IsServiceTestExecution = true
            };

            var act = new TestNativeActivity(false, "bob");

            var serviceTestModelTO = new ServiceTestModelTO
            {
                TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        UniqueId = Guid.Parse(act.UniqueID),
                        Type = StepType.Assert,
                        StepOutputs = new ObservableCollection<IServiceTestOutput>
                        {
                            new ServiceTestOutputTO
                            {
                                AssertOp = "=",
                                Value = "Bob",
                                Variable = "[[var]]"
                            }
                        }
                    }
                }
            };
            dataObject.ServiceTest = serviceTestModelTO;

            act.Execute(dataObject, 0);

            Assert.AreEqual(RunResult.TestFailed, serviceTestModelTO.Result.RunTestResult);
            Assert.IsTrue(dataObject.StopExecution);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfNativeActivity_UpdateDebugParentID")]
        public void DsfNativeActivity_ExecuteTool_Errors_PerformsErrorHandling()
        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                ServerID = Guid.NewGuid(),
                IsDebug = false,
                ForEachNestingLevel = 0,
                IsServiceTestExecution = true
            };

            var act = new TestNativeActivity(false, "bob");
            act.IsEndedOnError = true;
            act.OnErrorVariable = "[[Error]]";
            dataObject.Environment.AddError("There is an error");
            ExecutableServiceRepository.Instance.Add(new TestExecutionService
            {
                WorkspaceID = dataObject.WorkspaceID,
                ID = dataObject.ResourceID
            });
            act.Execute(dataObject, 0);

            var warewolfEvalResult = dataObject.Environment.Eval("[[Error]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            Assert.IsNotNull(warewolfEvalResult);
            var errorMessage = warewolfEvalResult.Item.ToString();
            Assert.IsTrue(dataObject.StopExecution);
            Assert.AreEqual("There is an error", errorMessage);

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfNativeActivity_DispatchDebugState_After_TestExecution()
        {
            var parentInstanceID = Guid.NewGuid();
            const StateType StateType = StateType.After;

            var dataObj = new DsfDataObject(string.Empty, GlobalConstants.NullDataListID)
            {
                IsDebug = true,
                IsOnDemandSimulation = false,
                IsServiceTestExecution = true,
                EnvironmentID = Guid.NewGuid(),
                WorkspaceID = Guid.NewGuid(),
                ServerID = Guid.NewGuid(),
                ResourceID = Guid.NewGuid(),
                OriginalInstanceID = Guid.NewGuid(),
                ParentInstanceID = parentInstanceID.ToString()
            };
            var mockDebugDispatcher = new Mock<IDebugDispatcher>();
            var activity = new TestActivity(mockDebugDispatcher.Object)
            {
                IsSimulationEnabled = false,
                ScenarioID = Guid.NewGuid().ToString(),
                IsWorkflow = true,
            };

            var serviceTestModelTO = new ServiceTestModelTO
            {
                TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        UniqueId = Guid.Parse(activity.UniqueID),
                        Type = StepType.Assert,
                        StepOutputs = new ObservableCollection<IServiceTestOutput>
                        {
                            new ServiceTestOutputTO
                            {
                                AssertOp = "=",
                                Value = "Bob",
                                Variable = "[[var]]"
                            }
                        }
                    }
                }
            };
            dataObj.ServiceTest = serviceTestModelTO;

            var cat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            res.Setup(a => a.ResourceName).Returns("bob");
            cat.Setup(a => a.GetResource(Guid.Empty, It.IsAny<Guid>())).Returns(res.Object);
            activity.ResourceCatalog = cat.Object;
            activity.TestDispatchDebugState(dataObj, StateType);
            Assert.IsTrue(dataObj.StopExecution);
            Assert.AreEqual(RunResult.TestFailed, serviceTestModelTO.TestSteps[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfNativeActivity_ExecuteTool")]
        public void DsfNativeActivity_ExecuteTool_NoTestStepOutputs_TestExecution()
        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 0,
                IsServiceTestExecution = true
            };

            var act = new TestNativeActivity(false, "bob");

            var serviceTestModelTO = new ServiceTestModelTO
            {
                TestSteps = new List<IServiceTestStep>
                {
                    new ServiceTestStepTO
                    {
                        UniqueId = Guid.Parse(act.UniqueID),
                        Type = StepType.Assert,
                        StepOutputs = new ObservableCollection<IServiceTestOutput>
                        {
                        }
                    }
                }
            };
            dataObject.ServiceTest = serviceTestModelTO;
            act.Execute(dataObject, 0);
            act.DispatchDebugState(dataObject, StateType.After, 0);

            Assert.AreEqual(RunResult.TestPending, serviceTestModelTO.TestSteps[0].Result.RunTestResult);
            Assert.IsFalse(dataObject.StopExecution);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DsfNativeActivity_ExecuteTool")]
        public void DsfNativeActivity_ExecuteTool_TestErrors_TestExecution()
        {
            CurrentDl = "<ADL><result></result></ADL>";
            TestData = "<root><ADL><result></result></ADL></root>";
            var act = new TestNativeActivity(false, "bob");
            act.IsEndedOnError = true;
            act.OnErrorVariable = "[[Error]]";
            act.OnErrorWorkflow = "[[OnErrorWorkflow]]";
            TestStartNode = new FlowStep
            {
                Action = act
            };
            var executionEnvironment = new ExecutionEnvironment();
            executionEnvironment.AddError("First Error");
            var dataObjectMock = new Mock<IDSFDataObject>();            
            var channel = new Mock<IEsbChannel>();
            var errorResultTO = new ErrorResultTO();
            errorResultTO.AddError("asdasd");
            channel.Setup(o => o.ExecuteLogErrorRequest(dataObjectMock.Object, It.IsAny<Guid>(), It.IsAny<string>(), out errorResultTO, 0));
            dataObjectMock.Setup(o => o.Environment).Returns(executionEnvironment);
            dataObjectMock.Setup(o => o.IsDebugMode()).Returns(true);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(channel.Object);
            var dataObject = act.Execute(dataObjectMock.Object, 0);
            Assert.AreEqual("First Error"+Environment.NewLine+"asdasd",dataObjectMock.Object.Environment.FetchErrors());
        }
    }

    class TestNativeActivity : DsfNativeActivity<string>
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


        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
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

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {

        }

        public bool Equals(TestNativeActivity other)
        {
            return ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is TestNativeActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }

        #endregion
    }

    class TestExecutionService : IExecutableService
    {
        #region Implementation of IExecutableService

        public Guid ID { get; set; }
        public Guid WorkspaceID { get; set; }
        public IList<IExecutableService> AssociatedServices { get; }
        public Guid ParentID { get; set; }

        public void Terminate()
        {
        }

        #endregion
    }
}
