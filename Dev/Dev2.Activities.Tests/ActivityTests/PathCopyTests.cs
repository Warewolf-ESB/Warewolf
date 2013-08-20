using System;
using System.IO;
using System.Threading;
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
    public class PathCopyTests : BaseActivityUnitTest
    {
        public PathCopyTests()
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
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Copy_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            var newGuid = Guid.NewGuid();
            File.WriteAllText(Path.Combine(TestContext.TestRunDirectory, newGuid + "Dev2.txt"), "TestData");
            DsfPathCopy act = new DsfPathCopy { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = "[[CompanyName]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Copy_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out dataListShape, out dataListWithData);
            DsfPathCopy act = new DsfPathCopy { InputPath = "[[FileNames(*).Name]]", OutputPath = string.Concat(TestContext.TestRunDirectory), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(13, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual("[[res]]", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("Failure", outRes[0].ResultsList[2].Value);

        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void PathCopyActivity_GetInputs_Expected_Six_Input()
        {
            DsfPathCopy testAct = new DsfPathCopy();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 6);
        }

        [TestMethod]
        public void PathCopyActivity_GetOutputs_Expected_One_Output()
        {
            DsfPathCopy testAct = new DsfPathCopy();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests


    }
}
