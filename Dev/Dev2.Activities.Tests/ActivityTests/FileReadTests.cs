using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;
using Dev2.DataList;

using Dev2.Diagnostics;
using Dev2.DataList.Contract.Binary_Objects;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class FileReadTests : BaseActivityUnitTest
    {
        public FileReadTests()
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

        #region Get Input/Output Tests

        [TestMethod]
        public void FileReadActivity_GetInputs_Expected_Four_Input()
        {
            DsfFileRead testAct = new DsfFileRead();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 4);
        }

        [TestMethod]
        public void FileReadActivity_GetOutputs_Expected_One_Output()
        {
            DsfFileRead testAct = new DsfFileRead();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void FileRead_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFileRead act = new DsfFileRead { InputPath = "[[CompanyName]]", Result = "[[CompanyName]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].Count);
            Assert.AreEqual(1, inRes[1].Count);
            Assert.AreEqual(1, inRes[2].Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void FileRead_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFileRead act = new DsfFileRead { InputPath = "[[Numeric(*).num]]", Result = "[[CompanyName]]" };
            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(31, inRes[0].Count);
            Assert.AreEqual(1, inRes[1].Count);
            Assert.AreEqual(1, inRes[2].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);          
        }

        #endregion
    }
}
