/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Tests.Activities.ActivityTests.Scripting
{
    /// <summary>
    /// Summary description for CalculateActivityTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfScriptingActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region JavaScript

        #region Should execute valid javascript

        [TestMethod]
        public void ExecuteWithValidJavascriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]",
                            @"return 1+1;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"var i = 1 + 1;return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript with a variable executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"var i = [[inputData]] + [[inputData]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"var i = [[inputData().field1]] + [[inputData().field1]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"var i = '[[inputData(*).field1]]';return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<string> dataListItems;
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out dataListItems, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("1", dataListItems[0], "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("2", dataListItems[1], "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("3", dataListItems[2], "Valid Javascript with datalist region executed incorrectly");
                Assert.AreEqual("4", dataListItems[3], "Valid Javascript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidJavascriptWithEmptyRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"var i = [[inputData(*).field1]] + [[inputData(*).field1]];return i;", enScriptType.JavaScript);

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<string> dataListItems;
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out dataListItems, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(string.Empty, dataListItems[0], "Valid Javascript with empty Recordset did not evaluate with blank");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        #endregion Should execute valid javascript

        #endregion JavaScript

        #region Ruby

        #region Should execute valid ruby script

        [TestMethod]
        public void ExecuteWithValidRubyExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"return 1+1;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyAndVariableExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"i = 1 + 1;return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with a variable executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithScalarDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData>1</inputData><Result>0</Result></DataList>", "<DataList><inputData/><Result/></DataList>", "[[Result]]", @"i = [[inputData]] + [[inputData]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithRecordAppendNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData><Result>0</Result></DataList>", "<DataList><inputData><field1/></inputData><Result/></DataList>", "[[Result]]", @"i = [[inputData().field1]] + [[inputData().field1]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("8", actual, "Valid Ruby with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1>1</field1></inputData><inputData><field1>2</field1></inputData><inputData><field1>3</field1></inputData><inputData><field1>4</field1></inputData></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = '[[inputData(*).field1]]';return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<string> dataListItems;
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out dataListItems, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("1", dataListItems[0], "Valid Rubyscript with datalist region executed incorrectly");
                Assert.AreEqual("2", dataListItems[1], "Valid Rubyscript with datalist region executed incorrectly");
                Assert.AreEqual("3", dataListItems[2], "Valid Rubyscript with datalist region executed incorrectly");
                Assert.AreEqual("4", dataListItems[3], "Valid Rubyscript with datalist region executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithValidRubyWithEmptyRecordStarNotationDataListRegionsInScriptExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "<DataList><inputData><field1/></inputData><Result><res/></Result></DataList>", "[[Result().res]]", @"i = [[inputData(*).field1]] + [[inputData(*).field1]];return i;", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            IList<string> dataListItems;
            GetRecordSetFieldValueFromDataList(result.Environment, "Result", "res", out dataListItems, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(string.Empty, dataListItems[0], "Valid Ruby with empty Recordset did not evaluate with blank");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteRubyWithNoReturnExpectedReturnsLastValue()
        {
            SetupArguments("<DataList><Result>0</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]", @"def Add(x,y); return x + y; end; Add(1,1);", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;

            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("2", actual, "Valid Ruby with empty Recordset did not evaluate without return keyword");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        #endregion Should execute valid ruby script

        #endregion Ruby

        #region Python

        /*
         * NOTE : You will find python test in the integration project because of faulty threading ;)
         *
         */

        #endregion Python

        #region Private Test Methods

        private void SetupArguments(string currentDl, string testData, string result, string script, enScriptType type)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfScriptingActivity { Result = result, Script = script, ScriptType = type }
            };

            CurrentDl = testData;
            TestData = currentDl;
        }

        #endregion Private Test Methods
    }
}