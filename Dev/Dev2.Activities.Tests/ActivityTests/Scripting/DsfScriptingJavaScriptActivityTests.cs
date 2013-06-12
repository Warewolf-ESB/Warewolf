using System.Threading;
using System.Activities.Statements;
using System.Collections.Generic;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CalculateActivityTest
    /// </summary>
    [TestClass]
    public class DsfScriptingActivityTests : BaseActivityUnitTest
    {
        public DsfScriptingActivityTests()
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

        static object _testGuard = new object();

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

        #region JavaScript

        #region Should execute valid javascript

        [TestMethod]
        public void ExecuteWithValidJavascriptExpectedCorrectResultReturned()
        {

            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]",
                            @"return 1+1;", enScriptType.JavaScript);

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
                Assert.Fail(
                    string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}",
                                    error));
            }
            
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"var i = 1 + 1;return i;", enScriptType.JavaScript);

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
        public void ExecuteWithValidJavascriptWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"var i = [[inputData]] + [[inputData]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"var i = [[inputData().field1]] + [[inputData().field1]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"var i = [[inputData(*).field1]] + 1;return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", dataListItems[0].TheValue, "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("3", dataListItems[1].TheValue, "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("4", dataListItems[2].TheValue, "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("5", dataListItems[3].TheValue, "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithEmptyRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"var i = [[inputData(*).field1]] + [[inputData(*).field1]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);


            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(string.Empty, dataListItems[0].TheValue, "Valid Javascript with empty Recordset did not evaluate with blank");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region Should not execute invalid javascript

        [TestMethod]
        public void ExecuteWithNoReturnExpectedCorrectErrorReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"Add(1,1); function Add(x,y) { return x + y; }", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>There was an error when returning a value from your script, remember to use the 'Return' keyword when returning the result</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Javascript with unexpected datalist variable did not throw error");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }        

        [TestMethod]
        public void ExecuteWithUnexpectedReferenceExpectedCorrectErrorReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return dasd;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>ReferenceError: dasd is not defined</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Javascript with unexpected datalist variable did not throw error");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #endregion

        #region Ruby

        #region Should execute valid ruby script

        [TestMethod]
        public void ExecuteWithValidRubyExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return 1+1;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"i = 1 + 1;return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with a variable executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"i = [[inputData]] + [[inputData]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"i = [[inputData().field1]] + [[inputData().field1]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Ruby with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = [[inputData(*).field1]] + 1;return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", dataListItems[0].TheValue, "Valid Ruby with datalist region executed incorrectly");
                Assert.AreEqual("3", dataListItems[1].TheValue, "Valid Ruby with datalist region executed incorrectly");
                Assert.AreEqual("4", dataListItems[2].TheValue, "Valid Ruby with datalist region executed incorrectly");
                Assert.AreEqual("5", dataListItems[3].TheValue, "Valid Ruby with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithEmptyRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = [[inputData(*).field1]] + [[inputData(*).field1]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);


            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(string.Empty, dataListItems[0].TheValue, "Valid Ruby with empty Recordset did not evaluate with blank");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void ExecuteRubyWithNoReturnExpectedReturnsLastValue()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"def Add(x,y); return x + y; end; Add(1,1);", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>There was an error when returning a value from the javascript, remember to use the 'Return' keyword when returning the result</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with empty Recordset did not evaluate without return keyword");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region Should not execute invalid ruby script

        [TestMethod]
        public void ExecuteRubyWithUnexpectedReferenceExpectedCorrectErrorReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return dasd;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>undefined method `dasd'</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Ruby with unexpected datalist variable did not throw error");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #endregion

        #region Python

        // These test are not parallel safe. They function single threaded, but fail multi-threaded!

        #region Should execute valid python script

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return 1+1", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Python executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"i = 1 + 1;return i;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Python with a variable executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"i = [[inputData]] + [[inputData]];return i;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Python with datalist regions executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"i = [[inputData().field1]] + [[inputData().field1]];return i;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Python with recordset datalist regions executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = [[inputData(*).field1]] + 1;return i;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", dataListItems[0].TheValue, "Valid Python with datalist region executed incorrectly");
                Assert.AreEqual("3", dataListItems[1].TheValue, "Valid Python with datalist region executed incorrectly");
                Assert.AreEqual("4", dataListItems[2].TheValue, "Valid Python with datalist region executed incorrectly");
                Assert.AreEqual("5", dataListItems[3].TheValue, "Valid Python with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecuteWithValidPythonWithEmptyRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = [[inputData(*).field1]] + [[inputData(*).field1]];return i;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Result", "res", out dataListItems, out error);


            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(string.Empty, dataListItems[0].TheValue, "Valid Python with empty Recordset did not evaluate with blank");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecutePythonWithNestedFuntionExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"
    def Add(x,y):
        return x + y;
    return Add(1,1);", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;

            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Python with nested function did not evaluate");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region Should not execute invalid Python script

        [TestMethod]
        [Ignore]
        public void ExecutePythonWithUnexpectedReferenceExpectedCorrectErrorReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return dasd;", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>global name 'dasd' is not defined</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Python with unexpected datalist variable did not throw error");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        [Ignore]
        public void ExecutePythonWithNoReturnExpectedErrorReturned()
        {

            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"
    def Add(x,y):
        return x + y;
    Add(1,1);", enScriptType.Python);

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = @"<InnerError>There was an error when returning a value from the javascript, remember to use the 'Return' keyword when returning the result</InnerError>";
            string actual;

            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual, "Python with unexpected datalist variable did not throw error");
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #endregion

        #region Get Debug Input/Output Tests

        [TestMethod]
        public void ScriptingGetDebugInputOutputWithRecordsetsExpectedPass()
        {
            DsfScriptingActivity act = new DsfScriptingActivity { Script = "return [[Numeric(1).num]],[[Numeric(2).num]];", Result = "[[res]]", ScriptType = enScriptType.JavaScript };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

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
        public void ScriptingGetDebugInputOutputWithRecordsetsUsingStarExpectedPass()
        {
            DsfScriptingActivity act = new DsfScriptingActivity { Script = "return [[Numeric(*).num]]", Result = "[[res]]", ScriptType = enScriptType.JavaScript};

            List<DebugItem> inRes;
            List<DebugItem> outRes;

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

        void SetupArguments(string currentDL, string testData, string result, string script, enScriptType type)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfScriptingActivity { Result = result, Script = script, ScriptType = type }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods

    }
}
