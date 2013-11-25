using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass][ExcludeFromCodeCoverage]
    public class FolderReadTests : BaseActivityUnitTest{
   

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Get Input/Output Tests

        [TestMethod]
        public void FolderReadActivity_GetInputs_Expected_Four_Input()
        {
            DsfFolderRead testAct = new DsfFolderRead();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(7,res);
        }

        [TestMethod]
        public void FolderReadActivity_GetOutputs_Expected_One_Output()
        {
            DsfFolderRead testAct = new DsfFolderRead();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1,res);
        }

        #endregion Get Input/Output Tests

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void FolderRead_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, "Dev2\\Dev2.txt"));           

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(TestContext.TestRunDirectory, "Dev2"));            

            foreach (string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }


            DsfFolderRead act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void FolderRead_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder\\testFile1.txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder\\testFile2.txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder2\\testFile3.txt"));
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder2\\testFile4.txt"));

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder"));
            directoryNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder2"));

            foreach(string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }
            

            string dataListWithData;
            string dataListShape;

            CreateDataListWithRecsetAndCreateShape(directoryNames, "FileNames", "Name", out dataListShape, out dataListWithData);

            DsfFolderRead act = new DsfFolderRead { InputPath = "[[FileNames(*).Name]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                                dataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);   
        }

        //2013.06.28: Ashley Lewis for bug 9708 - debug output for readfolder into a blank indexed recordset
        [TestMethod][Ignore]
        public void FolderReadWithBlankIndexedRecordsetExpectedFolderRead()
        {
            var tempPath = TestContext.TestRunDirectory;
            Directory.CreateDirectory(tempPath + "/CreateFileTest");
            File.Create(tempPath + "/CreateFileTest/TempFile1");
            File.Create(tempPath + "/CreateFileTest/TempFile2");
            File.Create(tempPath + "/CreateFileTest/TempFile3");
            DsfFolderRead act = new DsfFolderRead
            {
                Result = "[[Recset().field]]",
                InputPath = tempPath + "/CreateFileTest"
            };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, "<DL><Recset><field/></Recset></DL>",
                                                                "<root><ADL><Recset><field/></Recset></ADL></root>", out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            var getRecsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(outRes[0].ResultsList[3].Value);
            var getNextRecsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(outRes[0].ResultsList[6].Value);
            Assert.IsTrue(int.Parse(getRecsetIndex) < int.Parse(getNextRecsetIndex), "Recset indices don't increase as read folder reads into a recordset with a blank index");
            Assert.AreEqual("[[Recset(*).field]]", outRes[0].ResultsList[3].GroupName);
        } 

        [TestMethod]
        public void FolderReadWithFileNameExpectedFolderReadWithNoResult()
        {
            var tempPath = Path.GetTempPath();
            string path = tempPath + Guid.NewGuid();
            File.Create(path);
            File.Create(tempPath + Guid.NewGuid());
            File.Create(tempPath + Guid.NewGuid());
            DsfFolderRead act = new DsfFolderRead
            {
                Result = "[[Recset().field]]",
                InputPath = Path.GetTempFileName()
            };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckPathOperationActivityDebugInputOutput(act, "<DL><Recset><field/></Recset></DL>",
                                                                "<root><ADL><Recset><field/></Recset></ADL></root>", out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1,outRes.Count);
        } 

        #endregion

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(inputPath, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_1Update_UpdateInputPath()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_GetForEachInputs")]
        public void DsfFolderRead_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_GetForEachOutputs")]
        public void DsfFolderRead_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

    }
}
