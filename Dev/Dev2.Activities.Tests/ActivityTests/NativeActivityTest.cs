/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Simulation;
using Dev2.Tests.Activities.XML;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
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
            // ReSharper disable ObjectCreationAsStatement
            new TestActivity(null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void ConstructorWithDebugDispatcher_Expected_NoArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new TestActivity(DebugDispatcher.Instance);
            // ReSharper restore ObjectCreationAsStatement
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

        #region ValidateScalar

        #endregion

        #region ValidateRecordSet

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
            dispatcher.Setup(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>())).Verifiable();

            var activity = new TestActivity(dispatcher.Object);

            Run(activity, dataObject,
                () => dispatcher.Verify(d => d.Write(It.IsAny<IDebugState>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IDebugState>>()), Times.Exactly(expectedCount)));
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
                SimulationMode = SimulationMode.Never,
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
        // ReSharper disable InconsistentNaming
        public void DsfActivity_UpdateDebugParentID_UniqueIdSameIfNestingLevelNotChanged()
        // ReSharper restore InconsistentNaming
        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
            };

            TestNativeActivity act = new TestNativeActivity(false, "bob");
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfNativeActivity_UpdateDebugParentID")]
        // ReSharper disable InconsistentNaming
        public void DsfNativeActivity_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelIncreased()
        // ReSharper restore InconsistentNaming
        {
            var dataObject = new DsfDataObject("<Datalist></Datalist>", Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 1
            };

            TestNativeActivity act = new TestNativeActivity(false, "bob");
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


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


        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
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

        #endregion
    }
}
