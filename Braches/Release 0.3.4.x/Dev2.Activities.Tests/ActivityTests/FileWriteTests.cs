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
            DsfFileWrite act = new DsfFileWrite { FileContents = "TestData", OutputPath = Path.Combine(TestContext.TestRunDirectory,"[[CompanyName]].txt"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void Write_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"));
                      

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out dataListShape, out dataListWithData);

            DsfFileWrite act = new DsfFileWrite { FileContents = "TestData", OutputPath = "[[FileNames(*).Name]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);


            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);    
        }

        #endregion
    }
}
