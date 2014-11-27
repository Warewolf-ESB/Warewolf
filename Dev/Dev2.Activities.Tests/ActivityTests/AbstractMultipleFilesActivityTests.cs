
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ActivityUnitTests;
using Dev2.Diagnostics;
using Dev2.Tests.Activities.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class AbstractMultipleFilesActivityTests : BaseActivityUnitTest
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AbstractMultipleFiles_Execute")]
        public void AbstractMultipleFiles_Execute_WhenInputPathNotIsRooted_ExceptionCaughtErrorAdded()
        {
            //---------------Setup----------------------------------------------
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

            var act = new MockAbstractMultipleFilesActivity("Mock")
            {
                InputPath = @"OldFile.txt",
                OutputPath = Path.Combine(TestContext.TestRunDirectory, "NewName.txt"),
                Result = "[[res]]",
                DestinationUsername = "destUName",
                DestinationPassword = "destPWord",
                Username = "uName",
                Password = "pWord",
                GetOperationBroker = () => activityOperationBrokerMock
            };
            //-------------------------Execute-----------------------------------------------
            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            //-------------------------Assertions---------------------------------------------
            Assert.AreEqual(1, outRes.Count);
            var outputResultList = outRes[0].FetchResultsList();
            Assert.AreEqual(1, outputResultList.Count);
            Assert.AreEqual("", outputResultList[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("AbstractMultipleFiles_Execute")]
        public void AbstractMultipleFiles_Execute_WhenOutputPathNotIsRooted_ExceptionCaughtErrorAdded()
        {
            //---------------Setup----------------------------------------------
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

            var act = new MockAbstractMultipleFilesActivity("Mock")
            {
                InputPath = @"c:\OldFile.txt",
                OutputPath = "NewTest.zip",
                Result = "[[res]]",
                DestinationUsername = "destUName",
                DestinationPassword = "destPWord",
                Username = "uName",
                Password = "pWord",
                GetOperationBroker = () => activityOperationBrokerMock
            };
            //-------------------------Execute-----------------------------------------------
            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            //-------------------------Assertions---------------------------------------------
            Assert.AreEqual(1, outRes.Count);
            var outputResultList = outRes[0].FetchResultsList();
            Assert.AreEqual(1, outputResultList.Count);
            Assert.AreEqual("", outputResultList[0].Value);
        }

    }
}
