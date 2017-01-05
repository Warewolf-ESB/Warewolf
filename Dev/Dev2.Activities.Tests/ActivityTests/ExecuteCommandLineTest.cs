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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class ExecuteCommandLineTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        public string CommandLineToolName = "ConsoleAppToTestExecuteCommandLineActivity.exe";

        [TestMethod]
        public void ExecuteCommandLineShouldHaveInputProperty()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = GetRandomString();
            //------------Execute Test---------------------------
            activity.CommandFileName = randomString;
            //------------Assert Results-------------------------
            Assert.AreEqual(randomString, activity.CommandFileName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_GetOutputs")]
        public void DsfExecuteCommandLineActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfExecuteCommandLineActivity { CommandResult = "[[Bob]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[Bob]]", outputs[0]);
        }

        static string GetRandomString()
        {
            return new Random().Next(0, 1000).ToString(CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void ExecuteCommandLineShouldHaveCommandResultProperty()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = GetRandomString();
            //------------Execute Test---------------------------
            activity.CommandResult = randomString;
            //------------Assert Results-------------------------
            Assert.AreEqual(randomString, activity.CommandResult);
        }

        [TestMethod]
        public void OnExecuteWhereConsoleDoesNothingExpectNothingForResult()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\"";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------

            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            // remove test datalist ;)
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void OnExecuteWhereConsolePathHasSpacesIsNotWrappedInQuotesExpectError()
        {
            //------------Setup for test--------------------------
            const string ExeName = "ConsoleAppToTestExecuteCommandLineActivity.exe";
            var destDir = Path.Combine(TestContext.DeploymentDirectory, Guid.NewGuid() + " Temp");

            var sourceFile = Path.Combine(TestContext.DeploymentDirectory, ExeName);
            if (!File.Exists(sourceFile))
            {
                sourceFile = Path.Combine(Environment.CurrentDirectory, ExeName);
            }
            var destFile = Path.Combine(destDir, ExeName);

            Directory.CreateDirectory(destDir);
            File.Copy(sourceFile, destFile, true);

            var activity = new DsfExecuteCommandLineActivity { CommandFileName = destFile, CommandResult = "[[OutVar1]]" };

            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            var res = result.Environment.HasErrors();

            // remove test datalist ;)
            //------------Assert Results-------------------------
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void OnExecuteWhereConsolePathHasNoSpacesIsNotWrappedInQuotesExpectSuccess()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" \"output\"";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            var fetchErrors = DataObject.Environment.FetchErrors();
            if (fetchErrors != string.Empty)
            {
                Assert.Fail(fetchErrors);
            }
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            StringAssert.Contains(actual, "");
        }

        [TestMethod]
        public void OnExecute_WhereConsolePathStartsWithExplorer_ShouldError()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity { CommandFileName = "explorer C:\\", CommandResult = "[[OutVar1]]" };
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            var fetchErrors = result.Environment.FetchErrors();
            if (fetchErrors == string.Empty)
            {
                Assert.Fail("no error");
            }

            StringAssert.Contains(fetchErrors, "Cannot execute explorer from tool.");
        }

        [TestMethod]
        public void OnExecute_WhereConsolePathStartsWithCmd_ShouldError()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity { CommandFileName = "cmd  C:\\", CommandResult = "[[OutVar1]]" };
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            var fetchErrors = result.Environment.FetchErrors();
            if (fetchErrors == string.Empty)
            {
                Assert.Fail("no error");
            }

            StringAssert.Contains(fetchErrors, "Cannot execute CMD from tool.");
        }


        [TestMethod]
        public void OnExecuteWhereConsoleOutputsWithArgsWrappedInQuotesExpectSuccess()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" \"output\"";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            var fetchErrors = DataObject.Environment.FetchErrors();
            if (fetchErrors != string.Empty)
            {
                Assert.Fail(fetchErrors);
            }
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            StringAssert.Contains(actual, "This is output from the user");

        }

        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResult()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            var fetchErrors = DataObject.Environment.FetchErrors();
            if (fetchErrors != string.Empty)
            {
                Assert.Fail(fetchErrors);
            }
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            // remove test datalist ;)

            StringAssert.Contains(actual, "This is output from the user");

        }

        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultCmddirc()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            const string RandomString = @"cmd.exe /c dir C:\";
            activity.CommandFileName = RandomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(result.Environment.HasErrors());
            var fetchErrors = DataObject.Environment.FetchErrors();
            // remove test datalist ;)

            StringAssert.Contains(fetchErrors, "Cannot execute CMD from tool.");

        }


        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultExplorer()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            const string RandomString = @"C:\Windows\explorer.exe";
            activity.CommandFileName = RandomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(result.Environment.HasErrors());
            var fetchErrors = DataObject.Environment.FetchErrors();
            // remove test datalist ;)
            StringAssert.Contains(fetchErrors, "Cannot execute explorer from tool.");
        }


        [TestMethod]
        public void OnExecuteWhereConsoleErrorsExpectErrorInDatalist()
        {
            // ------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" error";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            string actual;
            string error;
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(result.Environment.HasErrors());
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);
            Assert.IsNull(actual);
            var fetchErrors = DataObject.Environment.FetchErrors();
            // remove test datalist ;)
            StringAssert.Contains(fetchErrors, "The console errored");

        }

        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithNoIndexWithConsoleOutputsExpectOutputForResultAppendedToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1().field1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            var expected = new List<string> { "This is output from the user" };
            string error;
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());

        }

        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithStarIndexWithConsoleOutputsExpectOutputForResultOverwriteToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1(*).field1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            var expected = new List<string> { "This is output from the user" };
            string error;
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());

        }

        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithSpecificIndexWithConsoleOutputsExpectOutputForResultInsertsToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var randomString = "\"" + toolPath + "\" output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1(1).field1]]";
            SetUpForExecution(activity, "<root></root>", "<ADL><recset1><field1/></recset1></ADL>");
            var expected = new List<string> { "This is output from the user" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());


        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToRecordSetExpectOutputResultsToMultipleRowsInRecordSet()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var command1 = "\"" + toolPath + "\" output";
            var command2 = "\"" + toolPath + "\" differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[recset2().field1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><recset2><field1></field1></recset2></ADL>");
            var expected = new List<string> { "This is output from the user", "This is a different output from the user" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.Environment, "recset2", "field1", out error);

            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToScalarExpectOutputResultOfLastCommandinScalar()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var toolPath = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            if (!File.Exists(toolPath))
            {
                toolPath = Environment.CurrentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe";
            }
            var command1 = "\"" + toolPath + "\" output";
            var command2 = "\"" + toolPath + "\" differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[OutVar1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><OutVar1/></ADL>");
            const string Expected = "This is a different output from the user";
            string error;
            string actual;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            GetScalarValueFromEnvironment(result.Environment, "OutVar1", out actual, out error);

            StringAssert.Contains(actual, Expected);
        }
        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]", CommandPriority = ProcessPriorityClass.RealTime };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(command1, act.CommandFileName);
            Assert.AreEqual(ProcessPriorityClass.RealTime, act.CommandPriority);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]", CommandPriority = ProcessPriorityClass.RealTime };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(command1, "Test2");
            var tuple3 = new Tuple<string, string>(ProcessPriorityClass.RealTime.ToString(), ProcessPriorityClass.High.ToString());

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.CommandFileName);
            Assert.AreEqual(ProcessPriorityClass.High, act.CommandPriority);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachInputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachInputs_UpdatesNotMatching_DoesNotUpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]", CommandPriority = ProcessPriorityClass.RealTime };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual(command1, act.CommandFileName);
            Assert.AreEqual(ProcessPriorityClass.RealTime, act.CommandPriority);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_UpdateForEachOutputs")]
        public void DsfExecuteCommandLineActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            var tuple1 = new Tuple<string, string>("[[OutVar1]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.CommandResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_GetForEachInputs")]
        public void DsfExecuteCommandLineActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const ProcessPriorityClass commandPriority = ProcessPriorityClass.RealTime;
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            var act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = "[[OutVar1]]", CommandPriority = commandPriority };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(command1, dsfForEachItems[0].Name);
            Assert.AreEqual(command1, dsfForEachItems[0].Value);
            Assert.AreEqual(commandPriority.ToString(), dsfForEachItems[1].Name);
            Assert.AreEqual(commandPriority.ToString(), dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfExecuteCommandLineActivity_GetForEachOutputs")]
        public void DsfExecuteCommandLineActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var command1 = "\"" + TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe\" output";
            const string result = "[[OutVar1]]";
            DsfExecuteCommandLineActivity act = new DsfExecuteCommandLineActivity { CommandFileName = command1, CommandResult = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        void SetUpForExecution(DsfExecuteCommandLineActivity activity, string testData, string currentDl)
        {
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = testData;
            CurrentDl = currentDl;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DsfExecuteCommandLineActivity_Priority")]
        public void DsfExecuteCommandLineActivity_Priority_Initialization_Normal()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();

            //------------Execute Test---------------------------
            ProcessPriorityClass priority = activity.CommandPriority;

            //------------Assert Results-------------------------
            Assert.AreEqual(ProcessPriorityClass.Normal, priority);

        }
    }
}
