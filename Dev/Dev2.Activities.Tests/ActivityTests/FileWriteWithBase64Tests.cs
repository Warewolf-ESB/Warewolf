/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class FileWriteWithBase64Tests : BaseActivityUnitTest
    {


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfFileWriteWithBase64 { FileContents = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.FileContents);
            Assert.AreEqual(outputPath, act.OutputPath);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfFileWriteWithBase64 { FileContents = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(outputPath, "Test");
            var tuple2 = new Tuple<string, string>(inputPath, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.FileContents);
            Assert.AreEqual("Test", act.OutputPath);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfFileWriteWithBase64 { FileContents = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfFileWriteWithBase64 { FileContents = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

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
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfFileWriteWithBase64 { FileContents = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var outputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfFileWriteWithBase64 { FileContents = inputPath, OutputPath = outputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(outputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(outputPath, dsfForEachItems[0].Value);
            Assert.AreEqual(inputPath, dsfForEachItems[1].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfFileWriteWithBase64 { FileContents = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt"), OutputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DsfFileWriteWithBase64))]
        public void DsfFileWriteWithBase64_GetState_ReturnsStateVariable()
        {
            var act = new DsfFileWriteWithBase64 {
                OutputPath = "Path",
                Overwrite = true,
                AppendTop = true,
                AppendBottom = true,
                FileContentsAsBase64 = true,
                FileContents = "some file contents",
                Username = "myuser",
                Password = "secret",
                PrivateKeyFile = "/path/to/secret",
                Result = "[[result]]"
            };

            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(9, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "OutputPath",
                    Value = "Path",
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "Overwrite",
                    Value = "True",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "AppendTop",
                    Value = "True",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "AppendBottom",
                    Value = "True",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "FileContents",
                    Value = "some file contents",
                    Type = StateVariable.StateType.InputOutput
                },
                new StateVariable
                {
                    Name = "FileContentsAsBase64",
                    Value = "True",
                    Type = StateVariable.StateType.InputOutput
                },
                new StateVariable
                {
                    Name = "Username",
                    Value = "myuser",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "PrivateKeyFile",
                    Value = "/path/to/secret",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = "[[result]]",
                    Type = StateVariable.StateType.Output
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
