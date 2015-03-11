
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.PathOperations.Interfaces;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class PathMoveTests : BaseActivityUnitTest
    {
#pragma warning disable 649
        static string _tempFile;
#pragma warning restore 649

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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if(_tempFile != null)
            {
                try
                {
                    File.Delete(_tempFile);
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
                    File.Delete(Path.GetTempPath() + NewFileName);
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
        public void PathMoveActivity_GetInputs_Expected_Six_Input()
        {
            DsfPathMove testAct = new DsfPathMove();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(8, res);
        }

        [TestMethod]
        public void PathMoveActivity_GetOutputs_Expected_One_Output()
        {
            DsfPathMove testAct = new DsfPathMove();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);
        }

        #endregion Get Input/Output Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_UpdateForEachInputs")]
        public void DsfPathMove_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathMove { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
            Assert.AreEqual(outputPath, act.OutputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_UpdateForEachInputs")]
        public void DsfPathMove_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathMove { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(outputPath, "Test");
            var tuple2 = new Tuple<string, string>(inputPath, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.InputPath);
            Assert.AreEqual("Test", act.OutputPath);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_UpdateForEachOutputs")]
        public void DsfPathMove_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathMove { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_UpdateForEachOutputs")]
        public void DsfPathMove_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathMove { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_UpdateForEachOutputs")]
        public void DsfPathMove_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathMove { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_GetForEachInputs")]
        public void DsfPathMove_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathMove { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
            Assert.AreEqual(outputPath, dsfForEachItems[1].Name);
            Assert.AreEqual(outputPath, dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathMove_GetForEachOutputs")]
        public void DsfPathMove_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathMove { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfPathMove_Execute")]
        public void Move_Execute_Workflow_SourceFile_And_DestinationFile_Has_Separate_Passwords_Both_Passwords_Are_Sent_To_OperationBroker()
        {
            List<string> fileNames = new List<string>
            {
                Path.Combine(TestContext.TestRunDirectory, "NewFileFolder\\Dev2.txt")
            };

            List<string> directoryNames = new List<string>();
            directoryNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder"));
            directoryNames.Add(Path.Combine(TestContext.TestRunDirectory, "NewFileFolder2"));

            foreach(string directoryName in directoryNames)
            {
                Directory.CreateDirectory(directoryName);
            }

            foreach(string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();

            DsfPathMove act = new DsfPathMove
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

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                       ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Password, "destPWord");
            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Username, "destUName");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Password, "pWord");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Username, "uName");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfPathMove_Construct")]
        public void Move_Construct_Object_Must_Be_OfType_IDestinationUsernamePassword()
        {
            var pathMove = new DsfPathMove();
            IDestinationUsernamePassword password = pathMove;
            Assert.IsNotNull(password);
        }
    }
}
