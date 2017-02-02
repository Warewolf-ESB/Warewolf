using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.Scripting
{
    [TestClass]
    public class DsfJavascriptActivityTests : BaseActivityUnitTest
    {
        [ClassCleanup]
        public static void Cleaner()
        {
            try
            {
                File.Delete(GetJsTmpFile());
            }
            catch (Exception)
            {
                //supress exceptio
            }
        }
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            try
            {
                File.WriteAllBytes(GetJsTmpFile(), Encoding.ASCII.GetBytes(@"if (!String.prototype.endsWith) 
                        {
                            String.prototype.endsWith = function(searchString, position) 
                            {
                                var subjectString = this.toString();
                                if (typeof position !== 'number' || !isFinite(position) || Math.floor(position) !== position || position > subjectString.length)
                                {
                                    position = subjectString.length;
                                }
                                position -= searchString.length;
                                var lastIndex = subjectString.indexOf(searchString, position);
                                return lastIndex !== -1 && lastIndex === position;
                            };
                       }"));
            }
            catch (Exception ex)
            {
                //supress exceptio
                Assert.Fail(ex.Message);
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Attribute_GivenIsNew_ShouldhaveCorrectValues()
        {
            //---------------Set up test pack-------------------
            var act = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(act);
            //---------------Execute Test ----------------------
            var toolDescriptorInfo = typeof(DsfJavascriptActivity).GetCustomAttributes(typeof(ToolDescriptorInfo), false).Single() as ToolDescriptorInfo;
            //---------------Test Result -----------------------
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual("Scripting", toolDescriptorInfo.Category );
            Assert.AreEqual("javascript script JSON JS", toolDescriptorInfo.FilterTag );
            Assert.AreEqual("Scripting-JavaScript", toolDescriptorInfo.Icon );
            Assert.AreEqual("JavaScript", toolDescriptorInfo.Name );
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_GivenType_ShouldInheritCorrectly()
        {
            //---------------Set up test pack-------------------
            var act = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_GivenIsNew_ShouldSetJavascript()
        {
            //---------------Set up test pack-------------------
            var act = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
            //---------------Execute Test ----------------------
            var displayName = act.DisplayName;
            //---------------Test Result -----------------------
            Assert.AreEqual("JavaScript", displayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Script_GivenIsNew_ShouldBeEmpty()
        {
            //---------------Set up test pack-------------------
            var act = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(act, typeof(DsfActivityAbstract<string>));
            //---------------Execute Test ----------------------
            var displayName = act.Script;
            //---------------Test Result -----------------------
            Assert.AreEqual("", displayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ScriptType_GivenIsNew_ShouldSetJavascript()
        {
            //---------------Set up test pack-------------------
            var act = new DsfJavascriptActivity();
            //---------------Assert Precondition----------------
            Assert.AreEqual("JavaScript", act.DisplayName);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enScriptType.JavaScript, act.ScriptType);

        }


        #region Should execute valid javascript

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GivenExternalFile_Execute_Javascript_ShouldExecuteExternalFunction()
        {
            var activity = new DsfJavascriptActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
        }
       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GivenFunctionNotInExternalFile_Execute_Javascript_ShouldNotExecuteFunction()
        {
            var activity = new DsfJavascriptActivity();
            Assert.IsNotNull(activity);
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(1, DataObject.Environment.Errors.Count);
        }
       


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GivenAnEscapeCharInString_ExecuteWithEscapeCharecters_Javascript_ShouldReturnGivenString()
        {
            SetupArguments("<DataList><testScript>\"C:\test\"</testScript><Result></Result></DataList>", "<DataList><testScript/><Result/></DataList>", "[[Result]]",
                            "return [[testScript]]", enScriptType.JavaScript, true);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("C:\test", actual, "Valid Javascript executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GivenAnEscapeCharInString_ExecuteWithEscapeCharectersInVariable_Javascript_EscapeFalse_ShouldReturnGivenString()
        {
            SetupArguments("<DataList><testScript>\"C:\test\"</testScript><Result></Result></DataList>", "<DataList><testScript/><Result/></DataList>", "[[Result]]",
                            "return [[testScript]]", enScriptType.JavaScript, true);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("C:\test", actual, "Valid Javascript executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
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
        [Owner("Nkosinathi Sangweni")]
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

        #region Private Test Methods

        private void SetupArguments(string currentDl, string testData, string result, string script, enScriptType type, bool escape = false)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfJavascriptActivity { Result = result, Script = script, ScriptType = type, EscapeScript = escape }
            };

            CurrentDl = testData;
            TestData = currentDl;
        }

        private static string GetJsTmpFile()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var directoryName = Path.GetDirectoryName(codeBase);
            return directoryName + "\\jsonFile1.js";
        }

        #endregion Private Test Methods
    }
}
