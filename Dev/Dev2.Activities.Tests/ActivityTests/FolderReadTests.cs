using System.Activities.Statements;
using System.IO;
using System.Threading;
using Dev2;
using Dev2.Common;
using Dev2.Data.Decision;
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
    public class FolderReadTests : BaseActivityUnitTest
    {
        static TestContext myTestContext;
        public FolderReadTests()
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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            myTestContext = testContext;
        }
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

        object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }
        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void FolderReadActivity_GetInputs_Expected_Four_Input()
        {
            DsfFolderRead testAct = new DsfFolderRead();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 4);
        }

        [TestMethod]
        public void FolderReadActivity_GetOutputs_Expected_One_Output()
        {
            DsfFolderRead testAct = new DsfFolderRead();

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
        public void FolderRead_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "Dev2\\Dev2.txt"));           

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "Dev2"));            

            foreach (string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }


            DsfFolderRead act = new DsfFolderRead { InputPath = string.Concat(myTestContext.TestRunDirectory,"\\","[[CompanyName]]"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void FolderRead_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder\\testFile1.txt"));
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder\\testFile2.txt"));
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2\\testFile3.txt"));
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2\\testFile4.txt"));

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder"));
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2"));

            foreach(string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }
            

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(directoryNames, "FileNames", "Name", out dataListShape, out dataListWithData);

            DsfFolderRead act = new DsfFolderRead { InputPath = "[[FileNames(*).Name]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);


            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(2, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[1].FetchResultsList().Count);
        }

        #endregion

        #region Execute

        [TestMethod]
        public void FolderReadWithInvalidPathExpectedDecisionPicksErrorUp()
        {

            TestStartNode = new FlowStep
            {
                Action = new DsfFolderRead
                {
                    InputPath = "xyz:\\",
                    Result = "[[Result]]"
                }
            };

            CurrentDl = "<DL></DL>";
            TestData = "<root></root>";
            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            var actual = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(@"{!TheStack!:[{!Col1!:!!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:0,!EvaluationFn!:!IsError!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!Error?!}", new List<string>{result.DataListID.ToString()} );

            Assert.AreEqual(string.Empty, error, "There was an error retrieving the error payload from the datalist");
            Assert.IsTrue(actual);
        }

        #endregion
    }
}
