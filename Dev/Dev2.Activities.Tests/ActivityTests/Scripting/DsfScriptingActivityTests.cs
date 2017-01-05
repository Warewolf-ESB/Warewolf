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
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using Dev2.Development.Languages.Scripting;
using Dev2.Interfaces;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.Scripting
{
    /// <summary>
    /// Summary description for CalculateActivityTest
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DsfScriptingActivityTests : BaseActivityUnitTest
    {
        private static string GetJsTmpFile()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var directoryName = Path.GetDirectoryName(codeBase);
            return directoryName + "\\jsonFile.js";
       }
        private static string GetRbTmpFile()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var directoryName = Path.GetDirectoryName(codeBase);
            return directoryName + "\\rubyFile.rb";
       }
        private static string GetPyTmpFile()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var directoryName = Path.GetDirectoryName(codeBase);
            return directoryName + "\\pythonFile.py";
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
                File.WriteAllBytes(GetRbTmpFile(), Encoding.ASCII.GetBytes(@"def greaterBalanceThanFive(other) return 5 < other end"));
                File.WriteAllBytes(GetPyTmpFile(), Encoding.ASCII.GetBytes(@"def GreaterThanFive(value):return 5<value;"));
            }
            catch (Exception ex)
            {
                //supress exceptio
                Assert.Fail(ex.Message);
            }
        }
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

        [TestMethod]
        public void DsfScriptingActivity_ShouldReturnInputs()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
            var debugOutputs = activity.GetForEachInputs();
            Assert.IsNotNull(debugOutputs);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetOutputs")]
        public void GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfScriptingActivity
            {
                IncludeFile = GetJsTmpFile(),
                Script = "return \"someValue\".endsWith(\"e\")",
                ScriptType = enScriptType.JavaScript,
                Result = "[[scrRes]]"
            };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[scrRes]]", outputs[0]);
        }
        [TestMethod]
        public void DsfScriptingActivity_ShouldReturnResults()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
            var debugOutputs = activity.GetForEachOutputs();
            Assert.IsNotNull(debugOutputs);
        }

        [TestMethod]
        public void DsfScriptingActivity_ShouldReturnDebugOutPuts()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
            var debugOutputs = activity.GetDebugOutputs(DataObject.Environment, 0);
            Assert.IsNotNull(debugOutputs);
        }

        [TestMethod]
        public void DsfScriptingActivity_ShouldReturn_DebugInputs()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
            var debugInputs = activity.GetDebugInputs(DataObject.Environment, 0);
            Assert.IsNotNull(debugInputs);
        }


        [TestMethod]
        public void DsfScriptingActivity_GivenInvalidScript_SholdReturnException()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
            var debugInputs = activity.GetDebugInputs(DataObject.Environment, 0);
            Assert.IsNotNull(debugInputs);
        }
        [TestMethod]
        public void DsfScriptingActivity_GivenInvalidScript_SholdUpdateForEachOutputs()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Result = "Test1";
            activity.Execute(DataObject, 0);
            var tuple1 = new Tuple<string, string>("Test1", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test");
            activity.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });            
            Assert.AreEqual(enScriptType.JavaScript, activity.ScriptType);
        }

        [TestMethod]
        public void DsfScriptingActivity_GivenInvalidScript_SholdUpdateForEachInputs()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            const string script = "return \"someValue\".endsWith(\"e\")";
            activity.Script = script;
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            var tuple1 = new Tuple<string, string>(script, "Test");
            activity.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            Assert.AreEqual(enScriptType.JavaScript, activity.ScriptType);
        }

        #region JavaScript

        #region Should execute valid javascript

        [TestMethod]
        public void GivenExternalFile_Execute_Javascript_ShouldExecuteExternalFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetJsTmpFile();
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);            
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
        }
        
        [TestMethod]
        public void GivenExternalFile_Execute_Rubyscript_ShouldExecuteExternalFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetRbTmpFile();
            activity.Script = "return greaterBalanceThanFive(10)";
            activity.ScriptType = enScriptType.Ruby;
            activity.Execute(DataObject, 0);            
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
        }

        [TestMethod]
        public void ScriptingContext_GivenJavaScript_ShouldReturnJavaScriptHandleType()
        {
            var context = new ScriptingEngineRepo();
            var scriptingContext = context.CreateEngine(enScriptType.Python, new StringScriptSources()) as Dev2PythonContext;
            if(scriptingContext != null)
                Assert.AreEqual(enScriptType.Python, scriptingContext.HandlesType());
        }
        [TestMethod]
        public void ScriptingContext_GivenRubyScript_ShouldReturnRubyScriptHandleType()
        {
            var context = new ScriptingEngineRepo();
            var scriptingContext = context.CreateEngine(enScriptType.Ruby, new StringScriptSources()) as RubyContext;
            if (scriptingContext != null)
                Assert.AreEqual(enScriptType.Ruby, scriptingContext.HandlesType());
        }

        [TestMethod]
        public void ScriptingContext_GivenRubyScript_ShouldSetNestedClassValues()
        {
            var context = new ScriptingEngineRepo();
            var scriptingContext = context.CreateEngine(enScriptType.Ruby, new StringScriptSources()) as RubyContext;
            Assert.IsNotNull(scriptingContext);
            var prObject = new PrivateObject(scriptingContext);
            Assert.IsNotNull(prObject);
            Assert.IsNull(scriptingContext.RuntimeSetup);
            prObject.Invoke("CreateRubyEngine");
            Assert.IsNotNull(scriptingContext.RuntimeSetup);
        }

        [TestMethod]
        public void ScriptingContext_GivenPythonScript_ShouldReturnPythonScriptHandleType()
        {
            var context = new ScriptingEngineRepo();
            var scriptingContext = context.CreateEngine(enScriptType.JavaScript, new StringScriptSources()) as JavaScriptContext;
            if (scriptingContext != null)
                Assert.AreEqual(enScriptType.JavaScript, scriptingContext.HandlesType());
        }
        

        [TestMethod]
        public void GivenExternalFile_Execute_Pythonscript_ShouldExecuteExternalFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.IncludeFile = GetPyTmpFile();
            activity.Script = "return GreaterThanFive(10)";
            activity.ScriptType = enScriptType.Python;
            activity.Execute(DataObject, 0);            
            Assert.AreEqual(0, DataObject.Environment.Errors.Count);
        }
        

        [TestMethod]
        public void GivenFunctionNotInExternalFile_Execute_Javascript_ShouldNotExecuteFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.JavaScript;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(1, DataObject.Environment.Errors.Count);
        }

        [TestMethod]
        public void GivenFunctionNotInExternalFile_Execute_Rubyscript_ShouldNotExecuteFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.Ruby;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(1, DataObject.Environment.Errors.Count);
        }

        [TestMethod]
        public void GivenFunctionNotInExternalFile_Execute_Pythonscript_ShouldNotExecuteFunction()
        {
            var activity = new DsfScriptingActivity();
            Assert.IsNotNull(activity);
            activity.Script = "return \"someValue\".endsWith(\"e\")";
            activity.ScriptType = enScriptType.Python;
            activity.Execute(DataObject, 0);
            Assert.AreEqual(1, DataObject.Environment.Errors.Count);
        }
        

        [TestMethod]
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
        public void RubytmpHost_ShouldSetDefaultValues()
        {
            var win8Pal = new RubyContext.TmpHost.Win8PAL();
            Assert.IsNotNull(win8Pal);
            Assert.IsFalse(win8Pal.FileExists(win8Pal.CurrentDirectory));
            Assert.IsFalse(win8Pal.DirectoryExists(win8Pal.CurrentDirectory));
        }
        [TestMethod]
        public void RubyOptionsAttribute_ShouldSetDefaultValues()
        {
            var optionsAttribute = new RubyContext.OptionsAttribute();
            Assert.IsNotNull(optionsAttribute);
            Assert.IsFalse(optionsAttribute.NoRuntime);
            Assert.IsFalse(optionsAttribute.PrivateBinding);
            Assert.IsNull(optionsAttribute.Pal);
        }

        [TestMethod]
        public void ExecuteWithEscapeCharecters_RubyExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>\"C:\test\"</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]",
                            "return [[Result]]", enScriptType.Ruby, true);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("C:\test", actual, "Valid Ruby executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ExecuteWithoutEscapeCharecters_RubyExpectedCorrectResultReturned()
        {
            SetupArguments("<DataList><Result>C:\test</Result></DataList>", "<DataList><Result/></DataList>", "[[Result]]",
                            "return \"C:\\test\"", enScriptType.Ruby);

            IDSFDataObject result = ExecuteProcess();

            string error;
            string actual;
            GetScalarValueFromEnvironment(result.Environment, "Result", out actual, out error);

            // remove test datalist ;)

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual("C:\test", actual, "Valid Ruby executed incorrectly");
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

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

        private void SetupArguments(string currentDl, string testData, string result, string script, enScriptType type, bool escape = false)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfScriptingActivity { Result = result, Script = script, ScriptType = type, EscapeScript = escape }
            };

            CurrentDl = testData;
            TestData = currentDl;
        }

        #endregion Private Test Methods
    }
}