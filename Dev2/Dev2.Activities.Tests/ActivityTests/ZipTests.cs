using System;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.PathOperations;
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
    public class ZipTests : BaseActivityUnitTest
    {
        static TestContext myTestContext;
        static string tempFile;
        const string NewFileName = "ZippedTempFile";
        public ZipTests()
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
        
         //Use ClassCleanup to run code after all tests in a class have run
         [ClassCleanup()]
         public static void MyClassCleanup()
         {
             if(tempFile != null)
             {
                 try
                 {
                     File.Delete(tempFile);
                 }
                 catch(Exception e)
                 {
                     if (e.GetType() != typeof(FileNotFoundException))// file not found is fine cos we're deleting
                     {
                         throw;
                     }
                 }

                 try
                 {
                     File.Delete(Path.GetTempPath() + NewFileName + ".zip");
                 }
                 catch(Exception e)
                 {
                     if(e.GetType() != typeof(FileNotFoundException))// file not found is fine cos we're deleting
                     {
                         throw;
                     }
                 }
             }
         }
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
        public void ZipActivity_GetInputs_Expected_Eight_Input()
        {
            DsfZip testAct = new DsfZip();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 8);
        }

        [TestMethod]
        public void ZipActivity_GetOutputs_Expected_One_Output()
        {
            DsfZip testAct = new DsfZip();

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
        public void Zip_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            Guid randomFileName = Guid.NewGuid();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2.txt"));

            List<string> zipfileNames = new List<string>();
            zipfileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2Zip.zip"));

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            DsfZip preact = new DsfZip { InputPath = Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "[[CompanyName]].txt"), OutputPath = Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "[[CompanyName]]Zip.zip"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(preact, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(7, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[5].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[6].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Zip_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));

            List<string> zipfileNames = new List<string>();
            zipfileNames.Add(Path.Combine(myTestContext.TestRunDirectory, Guid.NewGuid() + ".zip"));
            zipfileNames.Add(Path.Combine(myTestContext.TestRunDirectory, Guid.NewGuid() + ".zip"));

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }
            List<List<string>> recsetList = new List<List<string>>();
            recsetList.Add(fileNames);
            recsetList.Add(zipfileNames);

            List<string> Recsetnames = new List<string>();
            Recsetnames.Add("FileNames");
            Recsetnames.Add("ZipNames");

            List<string> Fieldnames = new List<string>();
            Fieldnames.Add("Name");
            Fieldnames.Add("Zips");

            string dataListWithData;
            string dataListShape;

            CreateDataListWithMultipleRecsetAndCreateShape(recsetList, Recsetnames, Fieldnames, out dataListShape, out dataListWithData);

            DsfZip preact = new DsfZip { InputPath = "[[FileNames(*).Name]]", OutputPath = "[[ZipNames(*).Zips]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(preact, dataListShape,
                                                                dataListWithData, out inRes, out outRes);


            Assert.AreEqual(7, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(7, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[5].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[6].FetchResultsList().Count);            

            Assert.AreEqual(2, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[1].FetchResultsList().Count);
        }

        #endregion

        #region Blank Output Test

        //2013.05.29: Ashley Lewis for bug 9507 - null output defaults to input
        [TestMethod]
        public void ZipFileWithBlankOutputPathExpectedDefaultstoInputPath()
        {
            tempFile = Path.GetTempFileName();
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(NewFileName, string.Empty, null, true));

            Dev2ZipOperationTO zipTO = ActivityIOFactory.CreateZipTO(null, null, null);
            ActivityIOFactory.CreateOperationsBroker().Zip(scrEndPoint, dstEndPoint, zipTO);
            Assert.IsTrue(File.Exists(Path.GetTempPath() + NewFileName+ ".zip"));
        }

        #endregion
    }
}
