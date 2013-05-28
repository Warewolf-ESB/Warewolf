using System.Threading;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for CalculateActivityTest
    /// </summary>
    [TestClass]
    public class DsfScriptingJavaScriptActivityTests : BaseActivityUnitTest
    {
        public DsfScriptingJavaScriptActivityTests()
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

        [ClassInitialize()]
        public static void BaseActivityUnitTestInitialize(TestContext testContext)
        {
            //var pathToRedis = Path.Combine(testContext.DeploymentDirectory, "redis-server.exe");
            //if (_redisProcess == null) _redisProcess = Process.Start(pathToRedis);
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void BaseActivityUnitTestCleanup()
        {
            //if(_redisProcess != null)
            //{
            //    _redisProcess.Kill();
            //}
        }

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

        #region Can execute valid javascript

        [TestMethod]
        public void ExecuteWithValidJavascriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return 1+1;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"var i = 1 + 1;return i;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript with a variable executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptAndSingleSquareBracesAroundVariableNameExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"var [i] = 1 + 1;return [i];");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>SyntaxError: Expected identifier but found '['</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "The right error wasnt thrown");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptAndDoubleSquareBracesAroundVariableNameExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>","[[Result]]", @"var [[i]] = 1 + 1;return [[i]];");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError> [[i]] does not exist in your Data List</InnerError><InnerError> [[i]] does not exist in your Data List</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "The wrong errors were thrown");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"var i = [[inputData]] + [[inputData]];return i;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript with double square brace around a variable name executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"var i = [[inputData().field1]] + [[inputData().field1]];return i;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Javascript with double square brace around a variable name executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"var i = [[inputData(*).field1]] + 1;return i;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", dataListItems[0].TheValue, "Valid Javascript with double square brace around a variable name executed incorrectly");
                Assert.AreEqual("3", dataListItems[1].TheValue, "Valid Javascript with double square brace around a variable name executed incorrectly");
                Assert.AreEqual("4", dataListItems[2].TheValue, "Valid Javascript with double square brace around a variable name executed incorrectly");
                Assert.AreEqual("5", dataListItems[3].TheValue, "Valid Javascript with double square brace around a variable name executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }        

        #endregion

        #region Can not execute invalid javascript

        [TestMethod]
        public void ExecuteWithInValidScriptExpectedCorrectErrorReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"dasd;return a;");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>ReferenceError: dasd is not defined</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "The correct error wasnt thrown.");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region Get Debug Input/Output Tests

        [TestMethod]
        public void ScriptingJavaScriptGetDebugInputOutputWithRecordsetsExpectedPass()
        {
            DsfScriptingJavaScriptActivity act = new DsfScriptingJavaScriptActivity { Script = "return [[Numeric(1).num]],[[Numeric(2).num]];", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Script to execute", inRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("return [[Numeric(1).num]],[[Numeric(2).num]];", inRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[0].FetchResultsList()[2].Value);
            Assert.AreEqual("return 654,668416154;", inRes[0].FetchResultsList()[3].Value);            
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("[[res]]", outRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("=", outRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("668416154", outRes[0].FetchResultsList()[2].Value);
        }

        [TestMethod]
        public void ScriptingJavaScriptGetDebugInputOutputWithRecordsetsUsingStarExpectedPass()
        {
            DsfScriptingJavaScriptActivity act = new DsfScriptingJavaScriptActivity { Script = "return [[Numeric(*).num]]", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape, ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Script to execute", inRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(1).num]]", inRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[0].FetchResultsList()[2].Value);
            Assert.AreEqual("return 654", inRes[0].FetchResultsList()[3].Value);
            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("[[res]]", outRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("=", outRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("654", outRes[0].FetchResultsList()[2].Value);
        }

        #endregion

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string result, string script)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfScriptingJavaScriptActivity { Result = result, Script = script }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods

    }
}
