using ActivityUnitTests;
using Dev2.DynamicServices;
using Dev2.Tests.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DebugStateTests
    /// </summary>
    [TestClass]
    public class DebugStateTests : BaseActivityUnitTest
    {
        public DebugStateTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region CheckDebugStateTest

        //Added For Bug 8918 by Massimo.Guerrera
        [TestMethod]
        public void CheckDebugStateOnActivityWithDataObjectSetToDebugExpectedDebugStateIsNotNull()
        {
            SetupArguments();
            DsfDataObject dataObject = new DsfDataObject(CurrentDl, ExecutionID)
            {
                IsDebug = true,
                ServerID = Guid.NewGuid()
            };
            CurrentDl = "<DataList></DataList>";
            TestData = "<DataList></DataList>";

            IDSFDataObject result = ExecuteProcess(dataObject);
            bool state = (TestStartNode.Action as MockDsfNativeActivity).IsDebugStateNull;

            Assert.IsFalse(state);
        }

        //Added For Bug 8918 by Massimo.Guerrera
        [TestMethod]
        public void CheckDebugStateOnActivityWithDataObjectSetToNotDebugExpectedDebugStateIsNull()
        {
            SetupArguments();
            DsfDataObject dataObject = new DsfDataObject(CurrentDl, ExecutionID)
            {
                IsDebug = false,
                ServerID = Guid.NewGuid()
            };
            CurrentDl = "<DataList></DataList>";
            TestData = "<DataList></DataList>";

            IDSFDataObject result = ExecuteProcess(dataObject);
            bool state = (TestStartNode.Action as MockDsfNativeActivity).IsDebugStateNull;

            Assert.IsTrue(state);
        }

        #endregion

        #region Private Methods

        private void SetupArguments()
        {
            TestStartNode = new FlowStep
            {
                Action = new MockDsfNativeActivity(false, "TestMockNativeActivity")
            };
        }

        #endregion
    }
}
