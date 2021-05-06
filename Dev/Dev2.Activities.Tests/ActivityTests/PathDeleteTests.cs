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
using System.Linq;
using ActivityUnitTests;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Security.Encryption;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    
    public class PathDeleteTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Njabulo Nxele")]
        [TestCategory("DsfPathDelete_Credentials")]
        public void DsfPathDelete_TryExecuteConcreteAction_Credential_Variable()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[val1]]", "demo", false, 0);

            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "test.txt");

            var act = new TestDsfPathDelete { InputPath = inputPath, Result = "CompanyName" };
            act.Username = "[[val1]]";

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(env);
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            //------------Execute Test---------------------------
            ErrorResultTO to;
            var outputs = act.TestTryExecuteConcreteAction(mockDataObject.Object, out to, 0);
            
            Assert.IsTrue(outputs[1].OutPutDescription == "Username [ demo ]");
        }
        

        #region GetDebugInputs/Outputs

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfPathDelete_Execution")]
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        public void DsfPathDelete_Execution_FileNotFound_DebugOutputErrorMessageRelevant()
        {
            var dsfPathDelete = new DsfPathDelete { InputPath = TestContext.TestRunDirectory + "\\some file that doesnt exist.txt", Result = "[[res]]" };

            //------------Execute Test---------------------------
            var result = CheckPathOperationActivityDebugInputOutput(dsfPathDelete, "<ADL><FileNames><Name></Name></FileNames><res></res></ADL>",
                                                                "<ADL><FileNames><Name></Name></FileNames><res></res></ADL>", out List<DebugItem> inRes, out List<DebugItem> outRes);
            GetScalarValueFromEnvironment(result.Environment, "Dev2System.Dev2Error", out string actual, out string error);

            // Assert Debug Output Error Message Relevant
            Assert.IsTrue(string.IsNullOrEmpty(actual) || !actual.Contains("null reference"), "Irrelevent error displayed for file not found.");

            //clean
        }

        #endregion

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_MoreThan1Updates_DoesNotUpdates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var path = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            var tuple2 = new Tuple<string, string>(path, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachInputs")]
        public void DsfPathDelete_UpdateForEachInputs_1Update_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.InputPath);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

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
        [TestCategory("DsfPathDelete_UpdateForEachOutputs")]
        public void DsfPathDelete_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_GetForEachInputs")]
        public void DsfPathDelete_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new DsfPathDelete { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfPathDelete_GetForEachOutputs")]
        public void DsfPathDelete_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new DsfPathDelete { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfPathDelete_GetState")]
        public void DsfPathDelete_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfPathDelete
            {
                InputPath = "[[InputPath]]",
                Username = "Bob",
                PrivateKeyFile = "abcde",
                Result = "[[res]]"
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(4, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "InputPath",
                    Type = StateVariable.StateType.InputOutput,
                    Value = "[[InputPath]]"
                },
                new StateVariable
                {
                    Name = "Username",
                    Type = StateVariable.StateType.Input,
                    Value = "Bob"
                },
                new StateVariable
                {
                    Name = "PrivateKeyFile",
                    Type = StateVariable.StateType.Input,
                    Value = "abcde"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
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
    
    internal class TestDsfPathDelete : DsfPathDelete
    {
        public IList<OutputTO> TestTryExecuteConcreteAction(IDSFDataObject dataObject, out ErrorResultTO errorResultTO, int update)
        {
            return base.TryExecuteConcreteAction(dataObject, out errorResultTO, update);
        }
    }
}
