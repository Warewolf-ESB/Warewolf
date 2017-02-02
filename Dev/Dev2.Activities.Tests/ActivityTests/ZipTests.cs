/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using Dev2.Tests.Activities;
using Dev2.Tests.Activities.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTests
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ZipTests : BaseActivityUnitTest
    {
        static TestContext myTestContext;
        static string tempFile;
        const string NewFileName = "ZippedTempFile";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            myTestContext = testContext;
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
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

     

        #region Blank Output Test
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ActivityIOBroker_Zip")]
        public void ActivityIOBroker_Zip_WhenOverwriteSetTrue_ShouldOverwriteFile()
        {
            //------------Setup for test--------------------------
            tempFile = Path.GetTempFileName();
            var zipPathName = Path.GetTempPath() + NewFileName + ".zip";
            IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true,""));
            IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true,""));
            Dev2ZipOperationTO zipTO = ActivityIOFactory.CreateZipTO(null, null, null, true);
            File.WriteAllText(zipPathName, "");
            //------------Assert Preconditions-------------------
            Assert.IsTrue(zipTO.Overwrite);
            Assert.IsTrue(File.Exists(zipPathName));
            var readAllBytes = File.ReadAllBytes(zipPathName);
            Assert.AreEqual(0, readAllBytes.Length);
            //------------Execute Test---------------------------
            ActivityIOFactory.CreateOperationsBroker().Zip(scrEndPoint, dstEndPoint, zipTO);
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists(zipPathName));
            readAllBytes = File.ReadAllBytes(zipPathName);
            Assert.AreNotEqual(0, readAllBytes.Length);
            File.Delete(tempFile);
            File.Delete(zipPathName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Zip_Execute")]
        public void Zip_Execute_Workflow_SourceFile_And_DestinationFile_Has_Separate_Passwords_Both_Passwords_Are_Sent_To_OperationBroker()
        {
            var fileNames = new List<string>();
            Guid randomFileName = Guid.NewGuid();
            fileNames.Add(Path.Combine(myTestContext.TestRunDirectory, randomFileName + "Dev2.txt"));


            foreach(string fileName in fileNames)
            {
                // ReSharper disable LocalizableElement
                File.WriteAllText(fileName, "TestData");
                // ReSharper restore LocalizableElement
            }

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();

            DsfZip preact = new DsfZip
            {
                InputPath = @"c:\OldFile.txt",
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
            IDestinationUsernamePassword password = zip;
            Assert.IsNotNull(password);
        }

        #endregion
    }
}
