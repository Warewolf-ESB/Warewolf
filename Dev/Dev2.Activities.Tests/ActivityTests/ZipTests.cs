using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using Dev2.Tests.Activities;
using Dev2.Tests.Activities.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
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
                    if(e.GetType() != typeof(FileNotFoundException))// file not found is fine cos we're deleting
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

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void ZipActivity_GetInputs_Expected_Eight_Input()
        {
            DsfZip testAct = new DsfZip();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 11);
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
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void Zip_Get_Debug_Input_Output_With_Scalar_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            List<string> fileNames = new List<string>();
            Guid randomFileName = Guid.NewGuid();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2.txt"));

            List<string> zipfileNames = new List<string>();
            zipfileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2Zip.zip"));

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            DsfZip preact = new DsfZip { InputPath = Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "[[CompanyName]].txt"), OutputPath = Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "[[CompanyName]]Zip.zip"), Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(preact, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(9, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(4, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[5].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[6].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[7].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[8].FetchResultsList().Count);
            
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        [Owner("Massimo Guerrera")]
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

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }
            List<List<string>> recsetList = new List<List<string>>();
            recsetList.Add(fileNames);
            recsetList.Add(zipfileNames);

            List<string> recsetnames = new List<string> { "FileNames", "ZipNames" };

            List<string> fieldnames = new List<string> { "Name", "Zips" };

            string dataListWithData;
            string dataListShape;

            CreateDataListWithMultipleRecsetAndCreateShape(recsetList, recsetnames, fieldnames, out dataListShape, out dataListWithData);

            DsfZip preact = new DsfZip { InputPath = "[[FileNames(*).Name]]", OutputPath = "[[ZipNames(*).Zips]]", Result = "[[res]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(preact, dataListShape,
                                                                dataListWithData, out inRes, out outRes);


            Assert.AreEqual(9, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Input Path", inRes[0].ResultsList[0].Value);
            Assert.AreEqual("[[FileNames(1).Name]]", inRes[0].ResultsList[1].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[2].Value);
            Assert.IsFalse(string.IsNullOrEmpty(inRes[0].ResultsList[3].Value));
            Assert.AreEqual("[[FileNames(2).Name]]", inRes[0].ResultsList[4].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[5].Value);
            Assert.IsFalse(string.IsNullOrEmpty(inRes[0].ResultsList[6].Value));
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual("Username", inRes[1].ResultsList[0].Value);
            Assert.AreEqual(2, inRes[2].FetchResultsList().Count);
            Assert.AreEqual("Password", inRes[2].ResultsList[0].Value);
            Assert.AreEqual(7, inRes[3].FetchResultsList().Count);
            Assert.AreEqual("Output Path", inRes[3].ResultsList[0].Value);
            Assert.AreEqual("[[ZipNames(1).Zips]]", inRes[3].ResultsList[1].Value);
            Assert.AreEqual("=", inRes[3].ResultsList[2].Value);
            Assert.IsFalse(string.IsNullOrEmpty(inRes[3].ResultsList[3].Value));
            Assert.AreEqual("[[ZipNames(2).Zips]]", inRes[3].ResultsList[4].Value);
            Assert.AreEqual("=", inRes[3].ResultsList[5].Value);
            Assert.IsFalse(string.IsNullOrEmpty(inRes[3].ResultsList[6].Value)); 
            Assert.AreEqual(1, inRes[4].FetchResultsList().Count);
            Assert.AreEqual("Username", inRes[4].ResultsList[0].Value);
            Assert.AreEqual(2, inRes[5].FetchResultsList().Count);
            Assert.AreEqual("Password", inRes[5].ResultsList[0].Value);
            Assert.AreEqual(2, inRes[6].FetchResultsList().Count);
            Assert.AreEqual("Overwrite", inRes[6].ResultsList[0].Value);
            Assert.AreEqual(2, inRes[6].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("[[res]]", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("Success", outRes[0].ResultsList[2].Value);
      
        }

        #endregion

        #region Blank Output Test
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ActivityIOBroker_Zip")]
        public void ActivityIOBroker_Zip_WhenOverwriteSetTrue_ShouldOverwriteFile()
        {
            //------------Setup for test--------------------------
            tempFile = Path.GetTempFileName();
            var zipPathName = Path.GetTempPath() + NewFileName + ".zip";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true));
            Dev2ZipOperationTO zipTO = ActivityIOFactory.CreateZipTO(null, null, null, true);
            File.WriteAllText(zipPathName, "");
            //------------Assert Preconditions-------------------
            Assert.IsTrue(zipTO.Overwrite);
            Assert.IsTrue(File.Exists(zipPathName));
            var readAllBytes = File.ReadAllBytes(zipPathName);
            Assert.AreEqual(0, readAllBytes.Count());
            //------------Execute Test---------------------------
            ActivityIOFactory.CreateOperationsBroker().Zip(scrEndPoint, dstEndPoint, zipTO);
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists(zipPathName));
            readAllBytes = File.ReadAllBytes(zipPathName);
            Assert.AreNotEqual(0, readAllBytes.Count());
            
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Zip_Execute")]
        public void Zip_Execute_Workflow_SourceFile_And_DestinationFile_Has_Separate_Passwords_Both_Passwords_Are_Sent_To_OperationBroker()
        {
            var fileNames = new List<string>();
            Guid randomFileName = Guid.NewGuid();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2.txt"));

            var zipfileNames = new List<string>();
            zipfileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName.ToString() + "Dev2Zip.zip"));

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, "TestData");
            }

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();

            DsfZip preact = new DsfZip
            {
                InputPath = "OldFile.txt",
                OutputPath = Path.Combine(TestContext.TestRunDirectory, "NewName.txt"),
                Result = "[[res]]",
                DestinationUsername = "destUName",
                DestinationPassword = "destPWord",
                Username = "uName",
                Password = "pWord",
                GetOperationBroker = () => activityOperationBrokerMock
            };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(preact, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Password, "destPWord");
            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Username, "destUName");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Password, "pWord");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Username, "uName");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dsfzip_Construct")]
        public void Zip_Construct_Object_Must_Be_OfType_IDestinationUsernamePassword()
        {
            var zip = new DsfZip();
            Assert.IsTrue(zip is IDestinationUsernamePassword);
        }

        #endregion
    }
}
