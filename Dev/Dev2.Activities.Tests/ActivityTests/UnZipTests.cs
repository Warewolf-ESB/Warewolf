using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ActivityUnitTests;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class UnZipTests : BaseActivityUnitTest
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Get Input/Output Tests

        [TestMethod]
        public void UnZipActivity_GetInputs_Expected_Seven_Input()
        {
            DsfUnZip testAct = new DsfUnZip();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(9, res);
        }

        [TestMethod]
        public void UnZipActivity_GetOutputs_Expected_One_Output()
        {
            DsfUnZip testAct = new DsfUnZip();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);
        }

        #endregion Get Input/Output Tests
        
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfUnZip_Constructor")]
        public void DsfUnZip_Constructor_DisplayName_Unzip()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var dsfUnZip = new DsfUnZip();

            //------------Assert Results-------------------------
            Assert.AreEqual("Unzip", dsfUnZip.DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfUnZip_Execute")]
        public void Unzip_Execute_Workflow_SourceFile_And_DestinationFile_Has_Separate_Passwords_Both_Passwords_Are_Sent_To_OperationBroker()
        {
            var fileNames = new List<string>();
            var guid = Guid.NewGuid();
            fileNames.Add(Path.Combine(TestContext.TestRunDirectory, guid + "Dev2.txt"));

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            foreach(string fileName in fileNames)
            {
                File.Delete(fileName);
            }

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();

            var act = new DsfUnZip
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

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                       ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Password, "destPWord");
            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Username, "destUName");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Password, "pWord");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Username, "uName");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfUnzip_Construct")]
        public void UnZip_Construct_Object_Must_Be_OfType_IDestinationUsernamePassword()
        {
            var unzip = new DsfUnZip();
            IDestinationUsernamePassword password = unzip;
            Assert.IsNotNull(password);
        }
    }
}
