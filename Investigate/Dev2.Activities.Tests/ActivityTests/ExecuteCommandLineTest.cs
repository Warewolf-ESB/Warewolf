using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dev2.Activities;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTest
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class ExecuteCommandLineTest : BaseActivityUnitTest
    {
        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion

        [TestMethod]
        public void ExecuteCommandLineShouldHaveInputProperty()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = GetRandomString();
            //------------Execute Test---------------------------
            activity.CommandFileName = randomString;
            //------------Assert Results-------------------------
            Assert.AreEqual(randomString,activity.CommandFileName);
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
            var randomString = TestContext.DeploymentDirectory+"\\ConsoleAppToTestExecuteCommandLineActivity.exe";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(executeProcess.DataListID));
            GetScalarValueFromDataList(executeProcess.DataListID, "OutVar1", out actual, out error);
            StringAssert.Contains(actual,"");
        }

        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResult()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(executeProcess.DataListID));
            GetScalarValueFromDataList(executeProcess.DataListID, "OutVar1", out actual, out error);
            StringAssert.Contains(actual, "This is output from the user");
           
        } 
        
        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultCmddirc()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = @"cmd.exe /c dir C:\";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "Cannot execute CMD from tool.");
           
        }
      
        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultNotepad()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = @"notepad.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };         
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "The process required user input.");
        }

        [TestMethod]
        [Ignore]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultWordpad()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = @"C:\Windows\write.exe";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "Process tried to start another process wordpad.exe");
        }

        [TestMethod]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultExplorer()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = @"C:\Windows\explorer.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "Cannot execute explorer from tool.");
        }

        [TestMethod]
        [Ignore]
        public void OnExecuteWhereConsoleOutputsExpectOutputForResultuninstall()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = @"C:\Program Files (x86)\Notepad++\uninstall.exe";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[OutVar1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };         
            TestData = "<root><OutVar1 /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "Process tried to start another process Au_.exe");
        }

        [TestMethod]
        public void OnExecuteWhereConsoleErrorsExpectErrorInDatalist()
        {
           // ------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe error";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            GetScalarValueFromDataList(executeProcess.DataListID, "OutVar1", out actual, out error);
            StringAssert.Contains(actual, "This is error");
            var fetchErrors = Compiler.FetchErrors(executeProcess.DataListID);
            StringAssert.Contains(fetchErrors, "The console errored");
            
        }

        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithNoIndexWithConsoleOutputsExpectOutputForResultAppendedToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
          
        }
      
    
        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithStarIndexWithConsoleOutputsExpectOutputForResultOverwriteToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
           
        } 
        
        [TestMethod]
        public void OnExecuteWhereOutputToRecordWithSpecificIndexWithConsoleOutputsExpectOutputForResultInsertsToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
            activity.CommandFileName = randomString;
            activity.CommandResult = "[[recset1(1).field1]]";
            SetUpForExecution(activity, "<root></root>", "<ADL><recset1><field1/></recset1></ADL>");
            var expected = new List<string> { "This is output from the user" };
            string error;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);

            var actualArray = actual.ToArray();
            actual.Clear();

            actual.AddRange(actualArray.Select(s => s.Trim()));

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());

          
        }
        
        [TestMethod]
        public void OnExecuteWhereInputFromRecordWithSpecificIndexWithConsoleOutputsExpectOutputForResultInsertsToRecordsets()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var randomString = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
            activity.CommandFileName = "[[recset1(1).rec1]]";
            activity.CommandResult = "[[recset1(1).field1]]";
            var testData = "<root><recset1><field1></field1><rec1>" + randomString + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><field1></field1><rec1></rec1></recset1></ADL>");
            var expected = new List<string> { "This is output from the user" };
            string error;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);
            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToRecordSetExpectOutputResultsToMultipleRowsInRecordSet()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var command1 = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
            var command2 = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[recset2().field1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><recset2><field1></field1></recset2></ADL>");
            var expected = new List<string> { "This is output from the user", "This is a different output from the user" };
            string error;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset2", "field1", out error);
            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }

        [TestMethod]
        public void OnExecuteWhereMultipleInputFromRecordSetWithOutputToScalarExpectOutputResultOfLastCommandinScalar()
        {
            //------------Setup for test--------------------------
            var activity = new DsfExecuteCommandLineActivity();
            var command1 = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe output";
            var command2 = TestContext.DeploymentDirectory + "\\ConsoleAppToTestExecuteCommandLineActivity.exe differentoutput";
            activity.CommandFileName = "[[recset1(*).rec1]]";
            activity.CommandResult = "[[OutVar1]]";
            var testData = "<root><recset1><rec1>" + command1 + "</rec1></recset1><recset1><rec1>" + command2 + "</rec1></recset1></root>";
            SetUpForExecution(activity, testData, "<ADL><recset1><rec1></rec1></recset1><OutVar1/></ADL>");
            const string expected = "This is a different output from the user";
            string error;
            string actual;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            GetScalarValueFromDataList(executeProcess.DataListID, "OutVar1", out actual, out error);
            StringAssert.Contains(actual,expected);
        }

        [TestMethod]
        [Ignore]
        public void ExecuteCommandLineGetDebugInputOutputExpectedCorrectResults()       
        {            
            DsfExecuteCommandLineActivity act = new DsfExecuteCommandLineActivity { CommandFileName = "ping rsaklfsvrgendev",CommandResult = "[[CompanyName]]"};

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);



            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(2, inRes[0].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);            
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
    }

}