using Dev2.DataList.Contract.Binary_Objects;
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
    public class FileWriteTests : BaseActivityUnitTest
    {
        public FileWriteTests()
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
        public void FileWriteActivity_GetInputs_Expected_Seven_Input()
        {
            DsfFileWrite testAct = new DsfFileWrite();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 7);
        }

        [TestMethod]
        public void FileWriteActivity_GetOutputs_Expected_One_Output()
        {
            DsfFileWrite testAct = new DsfFileWrite();


            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Write_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFileWrite act = new DsfFileWrite { FileContents = "[[CompanyName]]", OutputPath = "[[CompanyName]]", Result = "[[CompanyName]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(7, inRes.Count);
            Assert.AreEqual(1, inRes[0].Count);
            Assert.AreEqual(4, inRes[1].Count);
            Assert.AreEqual(4, inRes[2].Count);
            Assert.AreEqual(1, inRes[3].Count);
            Assert.AreEqual(1, inRes[4].Count);
            Assert.AreEqual(1, inRes[5].Count);
            Assert.AreEqual(1, inRes[6].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Write_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFileWrite act = new DsfFileWrite { FileContents = "[[Numeric(*).num]]", OutputPath = "[[Numeric(*).num]]", Result = "[[CompanyName]]" };
            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            Assert.AreEqual(7, inRes.Count);
            Assert.AreEqual(1, inRes[0].Count);
            Assert.AreEqual(31, inRes[1].Count);
            Assert.AreEqual(31, inRes[2].Count);
            Assert.AreEqual(1, inRes[3].Count);
            Assert.AreEqual(1, inRes[4].Count);
            Assert.AreEqual(1, inRes[5].Count);
            Assert.AreEqual(1, inRes[6].Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].Count);
        }

        #endregion
    }
}
