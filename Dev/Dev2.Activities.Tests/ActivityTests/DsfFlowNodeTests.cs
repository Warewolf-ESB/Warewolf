using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.Data;
using Dev2;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DsfFlowNodeTests : BaseActivityUnitTest
    {
        public DsfFlowNodeTests()
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

        #region GetDebugInputs/Outputs

        #region Decision tests
        /// <summary>
        /// Author : Massimo Guerrera Bug 8104
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        //Bug 8104
        public void FileRead_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[CompanyName]]", Col2 = string.Empty, Col3 = "2", EvaluationFn = enDecisionType.IsContains });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;


            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(3, inRes[0].Count);
            Assert.AreEqual(3, inRes[1].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        //Bug 8104
        public void FileRead_Get_Debug_Input_Output_With_Recordset_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(1).FirstName]]", Col2 = string.Empty, Col3 = "b", EvaluationFn = enDecisionType.IsContains });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;


            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(3, inRes[0].Count);
            Assert.AreEqual(3, inRes[1].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].Count);
        }

        //2013.02.13: Ashley Lewis - Bug 8725, Task 8913
        [TestMethod]
        [ExpectedException(typeof(InvalidExpressionException))]
        public void DecisionWithQuotesInScalarExpectedNoUnhandledExceptions()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[var]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");


            CurrentDl = "<ADL><var/></ADL>";
            TestData = "<root><var>\"</var></root>";
            TestStartNode = new FlowStep
            {
                Action = act
            };
            IDSFDataObject result = ExecuteProcess();

            IList<string> getDatalistID = new List<string>(){result.DataListID.ToString()};
            Assert.IsTrue(new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID));
        }

        #endregion

        #region Switch Tests

        [TestMethod]
        // ReSharper disable InconsistentNaming
        //Bug 8104
        public void FileRead_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            //DsfFileRead act = new DsfFileRead { InputPath = "[[Numeric(*).num]]", Result = "[[CompanyName]]" };
            //IList<IDebugItem> inRes;
            //IList<IDebugItem> outRes;

            //CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
            //                                                    ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            //Assert.AreEqual(4, inRes.Count);
            //Assert.AreEqual(31, inRes[0].Count);
            //Assert.AreEqual(1, inRes[1].Count);
            //Assert.AreEqual(1, inRes[2].Count);

            //Assert.AreEqual(1, outRes.Count);
            //Assert.AreEqual(3, outRes[0].Count);
            Assert.Inconclusive();
        }

        #endregion

        #endregion
    }
}
