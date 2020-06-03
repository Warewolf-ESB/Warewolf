/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActivityUnitTests;
using Dev2.Common.State;
using Dev2.Data.Interfaces;
using Dev2.Diagnostics;
using Dev2.Tests.Activities.Mocks;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]

    public class PathRenameTests : BaseActivityUnitTest
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }



        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_UpdateForEachInputs")]
        public void DsfPathRename_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathRename { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
            Assert.AreEqual(outputPath, act.OutputPath);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_UpdateForEachInputs")]
        public void DsfPathRename_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathRename { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(outputPath, "Test");
            var tuple2 = new Tuple<string, string>(inputPath, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.InputPath);
            Assert.AreEqual("Test", act.OutputPath);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_UpdateForEachOutputs")]
        public void DsfPathRename_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathRename
            {
                InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"),
                OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"),
                Result = result
            };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_UpdateForEachOutputs")]
        public void DsfPathRename_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathRename
            {
                InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"),
                OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"),
                Result = result
            };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_UpdateForEachOutputs")]
        public void DsfPathRename_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathRename { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_GetForEachInputs")]
        public void DsfPathRename_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathRename { InputPath = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

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
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathRename_GetForEachOutputs")]
        public void DsfPathRename_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathRename
            {
                InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"),
                OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"),
                Result = result
            };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfPathRename_Execute")]
        public void Rename_Execute_Workflow_SourceFile_And_DestinationFile_Has_Separate_Passwords_Both_Passwords_Are_Sent_To_OperationBroker()
        {
            var fileNames = new List<string>
                {
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"),
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt")
                };

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out string dataListShape,
                                                   out string dataListWithData);

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();

            var act = new DsfPathRename
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

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                       dataListWithData, out List<DebugItem> inRes, out List<DebugItem> outRes);

            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Password, "destPWord");
            Assert.AreEqual(activityOperationBrokerMock.Destination.IOPath.Username, "destUName");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Password, "pWord");
            Assert.AreEqual(activityOperationBrokerMock.Source.IOPath.Username, "uName");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfPathRename_Construct")]
        public void Rename_Construct_Object_Must_Be_OfType_IDestinationUsernamePassword()
        {
            var pathRename = new DsfPathRename();
            IDestinationUsernamePassword password = pathRename;
            Assert.IsNotNull(password);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("DsfPathRename_GetState()")]
        public void DsfPathRename_GetState()
        {
            var fileNames = new List<string>
                {
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt"),
                    Path.Combine(TestContext.TestRunDirectory, Guid.NewGuid() + ".txt")
                };

            foreach (string fileName in fileNames)
            {
                File.WriteAllText(fileName, @"TestData");
            }

            CreateDataListWithRecsetAndCreateShape(fileNames, "FileNames", "Name", out string dataListShape,
                                                   out string dataListWithData);

            var activityOperationBrokerMock = new ActivityOperationBrokerMock();
            var inputPath = @"c:\OldFile.txt";
            var outputPath = Path.Combine(TestContext.TestRunDirectory, "NewName.txt");
            var result = "[[res]]";
            var destinationUsername = "[[DestUsername]]";
            var destinationPassword = "[[DestPassword]]";
            var username = "[[username]]";
            var password = "[[Password]]";
            var overwrite = false;
            var privateKeyFile = "[[KeyFile]]";
            var destinationPrivateKeyFile = "[[DestKeyFile]]";
            var act = new DsfPathRename
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                Result = result,
                Overwrite = overwrite,
                DestinationUsername = destinationUsername,
                DestinationPassword = destinationPassword,
                Username = username,
                Password = password,
                PrivateKeyFile = privateKeyFile,
                DestinationPrivateKeyFile = destinationPrivateKeyFile,
                GetOperationBroker = () => activityOperationBrokerMock
            };

            CheckPathOperationActivityDebugInputOutput(act, dataListShape,
                                                       dataListWithData, out List<DebugItem> inRes, out List<DebugItem> outRes);

            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(8, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="InputPath",
                    Type = StateVariable.StateType.Input,
                    Value = inputPath
                },
                new StateVariable
                {
                    Name="OutputPath",
                    Type = StateVariable.StateType.Output,
                    Value = outputPath
                },
                new StateVariable
                {
                    Name="DestinationUsername",
                    Type = StateVariable.StateType.Input,
                    Value =destinationUsername
                },
                new StateVariable
                {
                    Name="Username",
                    Type = StateVariable.StateType.Input,
                    Value = username
                },
                new StateVariable
                {
                    Name="Overwrite",
                    Type = StateVariable.StateType.Input,
                    Value = overwrite.ToString()
                },
                new StateVariable
                {
                    Name="PrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value = privateKeyFile
                },
                new StateVariable
                {
                    Name="DestinationPrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value =destinationPrivateKeyFile
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
