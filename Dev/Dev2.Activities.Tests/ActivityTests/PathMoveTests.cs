using System;
using System.IO;
using System.Threading;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class PathMoveTests : BaseActivityUnitTest
    {

        static TestContext myTestContext;
        static string tempFile;
        const string NewFileName = "MovedTempFile";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfPathMove" /> is overwrite.
        /// </summary> 
        [Inputs("Overwrite")]
        public bool Overwrite
        {
            get;
            set;
        }

        public PathMoveTests()
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
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            if (tempFile != null)
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(FileNotFoundException))// file not found is fine cos we're deleting
                    {
                        throw;
                    }
                }

                try
                {
                    File.Delete(Path.GetTempPath() + NewFileName);
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(FileNotFoundException))// file not found is fine cos we're deleting
                    {
                        throw;
                    }
                }
            }
        }
    

        #region Get Input/Output Tests

        [TestMethod]
        public void PathMoveActivity_GetInputs_Expected_Six_Input()
        {
            DsfPathMove testAct = new DsfPathMove();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 6);
        }

        [TestMethod]
        public void PathMoveActivity_GetOutputs_Expected_One_Output()
        {
            DsfPathMove testAct = new DsfPathMove();

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
        public void Move_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder\\Dev2.txt"));            

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder"));
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2"));

            foreach (string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            DsfPathMove act = new DsfPathMove { InputPath = Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder", "[[CompanyName]].txt"), OutputPath = Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2", "[[CompanyName]].txt"), Result = "[[res]]" };

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
        public void Move_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder", Guid.NewGuid() + ".txt"));
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder", Guid.NewGuid() + ".txt"));            

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder"));
            directoryNames.Add(Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2"));

            foreach (string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out dataListShape, out dataListWithData);

            DsfPathMove act = new DsfPathMove { InputPath = "[[FileNames(*).Name]]", OutputPath = Path.Combine(myTestContext.TestRunDirectory, "NewFileFolder2", Guid.NewGuid() + ".txt"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[3].FetchResultsList().Count);            

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region Blank Output Test

        //2013.05.29: Ashley Lewis for bug 9507 - null output defaults to input
        [TestMethod]
        public void MoveFileWithBlankOutputPathExpectedDefaultstoInputPath()
        {
            tempFile = Path.GetTempFileName();
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(NewFileName, string.Empty, null, true));

            var moveTO = new Dev2CRUDOperationTO(Overwrite);
            ActivityIOFactory.CreateOperationsBroker().Move(scrEndPoint, dstEndPoint, moveTO);
            Assert.IsTrue(File.Exists(Path.GetTempPath() + NewFileName));
        }

        #endregion

    }
}
