#pragma warning disable
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
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.PathOperations;
using Dev2.Activities.Specs.Toolbox.FileAndFolder;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.PathOperations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using System.Reflection;
using Dev2.Activities.Designers2.AdvancedRecordset;
using System.Threading;
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.BaseTypes
{
    [Binding]
    public class CommonSteps : BaseActivityUnitTest
    {
        readonly ScenarioContext _scenarioContext;

        public CommonSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
        }

        public const string DestinationHolder = "destination";
        public const string ActualDestinationHolder = "actualDestination";
        public const string OverwriteHolder = "overwrite";
        public const string DestinationUsernameHolder = "destinationUsername";
        public const string DestinationPasswordHolder = "destinationPassword";
        public const string ResultVariableHolder = "resultVar";
        public const string SourceHolder = "source";
        public const string ActualSourceHolder = "actualSource";
        public const string SourcePrivatePublicKeyFile = "SourcePrivatePublicKeyFile";
        public const string DestinationPrivateKeyFile = "DestinationPrivateKeyFile";
        public const string SourceUsernameHolder = "sourceUsername";
        public const string SourcePasswordHolder = "sourcePassword";
        public const string ValidationErrors = "validationErrors";
        public const string ValidationMessage = "validationMessage";


        [Given(@"the advancerecodset execution has ""(.*)"" error")]
        [When(@"the advancerecodset execution has ""(.*)"" error")]
        [Then(@"the advancerecodset execution has ""(.*)"" error")]
        public void WhenTheAdvancerecodsetExecutionHasError(string anError)
        {

            var expectedError = anError.Equals("AN", StringComparison.OrdinalIgnoreCase);
            var result = _scenarioContext.Get<IDSFDataObject>("result");

            var fetchErrors = result.Environment.FetchErrors();
            var actuallyHasErrors = result.Environment.Errors.Count > 0 || result.Environment.AllErrors.Count > 0;
            var message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actuallyHasErrors ? "did not occur" : "did occur" + fetchErrors);

            var allErrors = new List<string>();
            allErrors.AddRange(result.Environment.Errors.ToList());
            allErrors.AddRange(result.Environment.AllErrors.ToList());

            if (expectedError)
            {
                var validateFromModelView = ValidateFromAdvancedRecordsetDesignerViewModel();
                if (validateFromModelView != null)
                {
                    foreach (var errorInfo in validateFromModelView)
                    {
                        allErrors.Add(errorInfo.Message);
                    }
                }

                var errorThrown = allErrors.Contains(fetchErrors);
                Assert.IsTrue(allErrors.Count > 0, "Expected " + anError + " error but the environment did not contain any.");
            }
        } 
        
        [Then(@"the execution has ""(.*)"" error")]
        [When(@"the execution has ""(.*)"" error")]
        public void ThenTheExecutionHasError(string anError)
        {
            var expectedError = anError.Equals("AN", StringComparison.OrdinalIgnoreCase);
            var expectedNoError = anError.Equals("NO", StringComparison.OrdinalIgnoreCase);
            var result = _scenarioContext.Get<IDSFDataObject>("result");

            var fetchErrors = result.Environment.FetchErrors();
            var actuallyHasErrors = result.Environment.Errors.Count > 0 || result.Environment.AllErrors.Count > 0;
            var message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actuallyHasErrors ? "did not occur" : "did occur" + fetchErrors);

            var allErrors = new List<string>();
            allErrors.AddRange(result.Environment.Errors.ToList());
            allErrors.AddRange(result.Environment.AllErrors.ToList());

            if (expectedError)
            {
                var validateFromModelView = ValidateFromModelView();
                if (validateFromModelView != null)
                {
                    foreach (var errorInfo in validateFromModelView)
                    {
                        allErrors.Add(errorInfo.Message);
                    }
                }
                Assert.IsTrue(allErrors.Count > 0, "Expected " + anError + " error but the environment did not contain any.");
            }
        }

        [Given(@"the debug inputs as")]
        [When(@"the debug inputs as")]
        [Then(@"the debug inputs as")]
        public void ThenTheDebugInputsAs(Table table)
        {
            var containsInnerActivity = _scenarioContext.ContainsKey("innerActivity");
            var containsKey = _scenarioContext.ContainsKey("activity");

            if (containsInnerActivity)
            {
                _scenarioContext.TryGetValue("innerActivity", out DsfNativeActivity<string> selectAndAppltTool);
                var result = _scenarioContext.Get<IDSFDataObject>("result");
                if (!result.Environment.HasErrors())
                {
                    var inputDebugItems = GetInputDebugItems(selectAndAppltTool, result.Environment);
                    ThenTheDebugInputsAs(table, inputDebugItems);
                }
            }
            else if (containsKey)
            {
                _scenarioContext.TryGetValue("activity", out object baseAct);
                var stringAct = baseAct as DsfFlowNodeActivity<string>;
                var boolAct = baseAct as DsfFlowNodeActivity<bool>;
                var baseBoolAct = baseAct as DsfActivityAbstract<bool>;
                var multipleFilesActivity = baseAct as DsfAbstractMultipleFilesActivity;
                if (stringAct != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfActivityAbstract<string>>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (!result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                }
                else if (boolAct != null || baseBoolAct != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfActivityAbstract<bool>>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (!result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                }
                else if (multipleFilesActivity != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfAbstractMultipleFilesActivity>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (!result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                }
            }
        }


        [Given(@"the debug inputs with errors as")]
        [When(@"the debug inputs with errors as")]
        [Then(@"the debug inputs with errors as")]
        public void ThenTheDebugInputsWithErrorsAs(Table table)
        {
            var containsInnerActivity = _scenarioContext.ContainsKey("innerActivity");
            var containsKey = _scenarioContext.ContainsKey("activity");

            if (containsInnerActivity)
            {
                _scenarioContext.TryGetValue("innerActivity", out DsfNativeActivity<string> selectAndAppltTool);
                var result = _scenarioContext.Get<IDSFDataObject>("result");
                if (result.Environment.HasErrors())
                {
                    var inputDebugItems = GetInputDebugItems(selectAndAppltTool, result.Environment);
                    ThenTheDebugInputsAs(table, inputDebugItems);
                }
                else
                {
                    Assert.Fail("expected errors");
                }
            }
            else if (containsKey)
            {
                _scenarioContext.TryGetValue("activity", out object baseAct);
                var stringAct = baseAct as DsfFlowNodeActivity<string>;
                var boolAct = baseAct as DsfFlowNodeActivity<bool>;
                var baseBoolAct = baseAct as DsfActivityAbstract<bool>;
                var multipleFilesActivity = baseAct as DsfAbstractMultipleFilesActivity;
                if (stringAct != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfActivityAbstract<string>>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                    else
                    {
                        Assert.Fail("expected errors");
                    }
                }
                else if (boolAct != null || baseBoolAct != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfActivityAbstract<bool>>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                    else
                    {
                        Assert.Fail("expected errors");
                    }
                }
                else if (multipleFilesActivity != null)
                {
                    var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfAbstractMultipleFilesActivity>("activity") : null;
                    var result = _scenarioContext.Get<IDSFDataObject>("result");
                    if (result.Environment.HasErrors())
                    {
                        var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                        ThenTheDebugInputsAs(table, inputDebugItems);
                    }
                    else
                    {
                        Assert.Fail("expected errors");
                    }
                }
            }
        }

        public void ThenTheDebugInputsAs(Table table, List<IDebugItemResult> inputDebugItems)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);
            CollectionsAssert(expectedDebugItems, inputDebugItems);
        }

        [Then(@"the debug output as")]
        [When(@"the debug output as")]
        public void ThenTheDebugOutputAs(Table table)
        {
            var result = _scenarioContext.Get<IDSFDataObject>("result");
            if (!result.Environment.HasErrors())
            {
                var outputDebugItems = GetOutputDebugItems(null, result.Environment);
                ThenTheDebugOutputAs(table, outputDebugItems);
            }
        }

        [Then(@"the debug output with errors as")]
        [When(@"the debug output with errors as")]
        public void ThenTheDebugOutputWithErrorsAs(Table table)
        {
            var result = _scenarioContext.Get<IDSFDataObject>("result");
            if (result.Environment.HasErrors())
            {
                var outputDebugItems = GetOutputDebugItems(null, result.Environment);
                ThenTheDebugOutputAs(table, outputDebugItems);
            }
        }

        public void ThenTheDebugOutputAs(Table table, List<IDebugItemResult> outputDebugItems)
        {
            ThenTheDebugOutputAs(table, outputDebugItems, false);
        }
        public void ThenTheDebugOutputAs(Table table, List<IDebugItemResult> outputDebugItems, bool isDataMerge)
        {
            ThenTheDebugOutputAs(table, outputDebugItems, isDataMerge, false);
        }
        public void ThenTheDebugOutputAs(Table table, List<IDebugItemResult> outputDebugItems, bool isDataMerge, bool sortItemsFirst)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);

            // Very specific case ;)
            if (isDataMerge && expectedDebugItems.Count == 2)
            {
                // chop the first one off ;)
                expectedDebugItems.RemoveAt(0);
            }

            if (sortItemsFirst)
            {
                expectedDebugItems.Sort((x, y) => x.Value.CompareTo(y.Value));
                outputDebugItems.Sort((x, y) => x.Value.CompareTo(y.Value));
            }

            CollectionsAssert(expectedDebugItems, outputDebugItems);
        }

        [Given(@"I have a source path ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveASourcePathWithValue(string pathVariable, string location)
        {
            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(pathVariable, FixBrokenHostname(location)));

            _scenarioContext.Add(SourceHolder, string.IsNullOrEmpty(pathVariable) ? location : pathVariable);
            _scenarioContext.Add(ActualSourceHolder, location);
        }

        string FixBrokenHostname(string location)
        {
            var brokenHostname = "SVRPDC.premier.local";
            if (location.Contains(brokenHostname) && location.IndexOf(brokenHostname) >= 6 && location.Substring(location.IndexOf(brokenHostname)-6,6) == "ftp://")
            {
                location = location.Replace(brokenHostname, Depends.GetIPAddress(brokenHostname));
            }
            return location;
        }

        public static string GetGuid()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 13);
        }

        public static string AddGuidToPath(string location, string GetGuid)
        {
            string getExtention;
            try
            {
                getExtention = Path.GetExtension(location);
            }
            catch (ArgumentException)
            {
                getExtention = null;
            }
            if (!string.IsNullOrEmpty(getExtention))
            {
                return location.Replace(getExtention, GetGuid + getExtention);
            }
            else if (location.EndsWith(@"\\"))
            {
                return location.TrimEnd('\\') + GetGuid + @"\\";
            }
            else if (location.EndsWith("\\"))
            {
                return location.TrimEnd('\\') + GetGuid + "\\";
            }
            return location + GetGuid;
        }

        [Given(@"use private public key for source is ""(.*)""")]
        public void GivenUsePrivatePublicKeyForSourceIs(string sourceKey) => _scenarioContext.Add(SourcePrivatePublicKeyFile, sourceKey);

        [Given(@"assign error to variable ""(.*)""")]
        public void GivenAssignErrorToVariable(string errorVariable)
        {
            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(errorVariable, ""));
            _scenarioContext.Add("errorVariable", errorVariable);
        }

        [Given(@"call the web service ""(.*)""")]
        public void GivenCallTheWebService(string onErrorWebserviceToCall)
        {
            if (Dev2.Activities.Specs.Toolbox.Data.DataSplit.DataSplitSteps._containerOps != null)
            {
                onErrorWebserviceToCall = onErrorWebserviceToCall.Replace("tst-ci-remote:3142", Toolbox.Data.DataSplit.DataSplitSteps._containerOps.Container.IP + ":" + Toolbox.Data.DataSplit.DataSplitSteps._containerOps.Container.Port);
            }
            _scenarioContext.Add("webserviceToCall", onErrorWebserviceToCall);
        }

        void CreateSourceFileWithSomeDummyData(int numberOfGuids = 1)
        {
            try
            {
                Dev2Logger.Debug(string.Format("Source File: {0}", _scenarioContext.Get<string>(ActualSourceHolder)), "Warewolf Debug");
                var broker = ActivityIOFactory.CreateOperationsBroker();
                var source = ActivityIOFactory.CreatePathFromString(_scenarioContext.Get<string>(ActualSourceHolder),
                    _scenarioContext.Get<string>(SourceUsernameHolder),
                    _scenarioContext.Get<string>(SourcePasswordHolder),
                    true, "");
                var sb = new StringBuilder();
                Enumerable.Range(1, numberOfGuids).ToList().ForEach(x => sb.Append(Guid.NewGuid().ToString()));
                var ops = ActivityIOFactory.CreatePutRawOperationTO(WriteType.Overwrite, sb.ToString());
                var sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
                if (sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.File)
                {
                    var result = broker.PutRaw(sourceEndPoint, ops);
                    if (result != "Success")
                    {
                        result = broker.PutRaw(sourceEndPoint, ops);
                        if (result != "Success")
                        {
                            Dev2Logger.Debug("Create Source File for file op test error", "Warewolf Debug");
                        }
                    }
                }
                else if (sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.Directory && source.Path.Contains("emptydir"))
                {
                    broker.Create(sourceEndPoint, new Dev2CRUDOperationTO(true, false), false);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Debug("Create Source File for file op test error", e, "Warewolf Debug");
                //throw
            }
        }

        [Given(@"source credentials as ""(.*)"" and ""(.*)""")]
        public void GivenSourceCredentialsAs(string userName, string password)
        {
            _scenarioContext.Add(SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            _scenarioContext.Add(SourcePasswordHolder, password.Replace('"', ' ').Trim());
            CreateSourceFileWithSomeDummyData();
        }

        [Given(@"source credentials as ""(.*)"" and ""(.*)"" for zip tests")]
        public void GivenSourceCredentialsAsAndForZipTests(string userName, string password)
        {
            _scenarioContext.Add(SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            _scenarioContext.Add(SourcePasswordHolder, password.Replace('"', ' ').Trim());
            CreateSourceFileWithSomeDummyData(1000);
        }

        [Given(@"I have a destination path ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveADestinationPathWithValue(string pathVariable, string location)
        {
            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(pathVariable, FixBrokenHostname(location)));

            _scenarioContext.Add(DestinationHolder, string.IsNullOrEmpty(pathVariable) ? location : pathVariable);
            _scenarioContext.Add(ActualDestinationHolder, location);
        }

        [Given(@"overwrite is ""(.*)""")]
        public void GivenOverwriteIs(string overwrite)
        {
            bool.TryParse(overwrite, out bool isOverwrite);
            _scenarioContext.Add(OverwriteHolder, isOverwrite);
        }

        [Given(@"destination credentials as ""(.*)"" and ""(.*)""")]
        public void GivenDestinationCredentialsAs(string userName, string password)
        {
            _scenarioContext.Add(DestinationUsernameHolder, userName.Replace('"', ' ').Trim());
            _scenarioContext.Add(DestinationPasswordHolder, password.Replace('"', ' ').Trim());
        }

        [Given(@"use private public key for destination is ""(.*)""")]
        public void GivenUsePrivatePublicKeyForDestinationIs(string destinationKey)
        {
            _scenarioContext.Add(DestinationPrivateKeyFile, destinationKey);
        }

        [Then(@"validation is ""(.*)""")]
        public void ThenValidationIs(string expectedValidationResult)
        {
            _scenarioContext.TryGetValue(ValidationErrors, out IList<IActionableErrorInfo> validationErrors);
            if (expectedValidationResult.Equals("True", StringComparison.OrdinalIgnoreCase))
            {
                if (validationErrors != null)
                {
                    Assert.AreNotEqual(0, validationErrors.Count, "Validation had one or more errors. First error: " + validationErrors[0].Message);
                }
            }
            else
            {
                if (validationErrors == null)
                {
                    Assert.IsNull(validationErrors);
                }
            }
        }

        [Then(@"validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string validationMessage)
        {
            _scenarioContext.TryGetValue(ValidationErrors, out IList<IActionableErrorInfo> validationErrors);
            if (string.IsNullOrEmpty(validationMessage.Trim()) || string.IsNullOrWhiteSpace(validationMessage.Trim()) || validationMessage == "\"\"")
            {
                if (validationErrors != null && validationErrors.Count > 0)
                {
                    Assert.AreEqual(0, validationErrors.Count, "Expected no errors but got one or more. First error: " + validationErrors[0].Message);
                }
            }
            else
            {
                if (validationErrors != null)
                {
                    Assert.IsNotNull(validationErrors);
                    var completeMessage = string.Join(";", validationErrors.Select(info => info.Message));
                    FixBreaks(ref validationMessage, ref completeMessage);
                    Assert.AreEqual(validationMessage, completeMessage);
                }
            }
        }

        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").ToString().Trim();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").ToString().Trim();
        }

        [Given(@"result as ""(.*)""")]
        public void GivenResultAs(string resultVar)
        {
            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(resultVar, ""));

            _scenarioContext.Add(ResultVariableHolder, resultVar);
        }

        [Then(@"the result from the web service ""(.*)"" will have the same data as variable ""(.*)""")]
        public void ThenTheResultFromTheWebServiceWillHaveTheSameDataAsVariable(string webservice, string errorVariable)
        {
            if (Toolbox.Data.DataSplit.DataSplitSteps._containerOps != null)
            {
                webservice = webservice.Replace("tst-ci-remote:3142", Toolbox.Data.DataSplit.DataSplitSteps._containerOps.Container.IP + ":" + Toolbox.Data.DataSplit.DataSplitSteps._containerOps.Container.Port);
            }
            var result = _scenarioContext.Get<IDSFDataObject>("result");

            //Get the error value
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(errorVariable),
                                       out string errorValue, out string error);
            errorValue = errorValue.Replace('"', ' ').Trim();

            //Send the error in error variable
            var onErrorWebserviceToCall = _scenarioContext.Get<string>("webserviceToCall").Replace("[[error]]", errorValue);
            var webClient = WebRequest.Create(onErrorWebserviceToCall);
            webClient.Credentials = new NetworkCredential("WarewolfAdmin", "W@rEw0lf@dm1n");
            ServicePointManager.DefaultConnectionLimit = 100;
            webClient.Timeout = Timeout.Infinite;
            webClient.GetResponse();

            var retryCount = 0;
            string webCallResult;

            do
            {
                retryCount++;
                //Call the service and get the error back
                webClient = WebRequest.Create(webservice);
                webClient.Credentials = new NetworkCredential("WarewolfAdmin", "W@rEw0lf@dm1n");
                webClient.Timeout = Timeout.Infinite;
                using (WebResponse response = (HttpWebResponse)webClient.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            webCallResult = reader.ReadToEnd();
                        }
                    }
                }
            }
            while (webCallResult.Contains("<FatalError>") && retryCount < 10);
            StringAssert.Contains(webCallResult, errorValue);
        }

        [Then(@"the result variable ""(.*)"" will be ""(.*)""")]
        public void ThenTheResultVariableWillBe(string variable, string expectedValue)
        {
            string error;
            var result = _scenarioContext.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(variable))
            {
                var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
                var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                var recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i) && i != "\"\"").ToList();
                expectedValue = expectedValue.Replace('"', ' ').Trim();

                if (string.IsNullOrEmpty(expectedValue))
                {
                    if (recordSetValues != null && recordSetValues.Count > 0)
                    {
                        Assert.Fail("Expecting no value but recordset result variable has one or more values. First value: " + recordSetValues[0]);
                    }
                }
                else
                {
                    if (recordSetValues != null && recordSetValues.Count > 0)
                    {
                        Assert.AreEqual(recordSetValues[0], expectedValue, "First recordset result variable value is not equal to expected value.");
                    }
                    else
                    {
                        Assert.Fail("Expecting value " + expectedValue + " but recordset " + variable + " has no values.");
                    }
                }
            }
            else
            {
                expectedValue = expectedValue.Replace('"', ' ').Trim();
                GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(variable),
                                           out string actualValue, out error);
                if (string.IsNullOrEmpty(expectedValue))
                {
                    actualValue = "";
                }
                if (actualValue != null)
                {
                    actualValue = actualValue.Replace('"', ' ').Trim();
                    var type = "";
                    if (expectedValue == "String" || expectedValue == "Int32" || expectedValue == "Guid" || expectedValue == "DateTime")
                    {
                        type = expectedValue;
                    }
                    if (string.IsNullOrEmpty(type))
                    {
                        Assert.AreEqual(expectedValue, actualValue, (error == string.Empty ? string.Empty : "There was an error getting the result from the datalist: " + error + "\n") + (result.Environment.AllErrors.FirstOrDefault() == null ? string.Empty : "The execution environment contains at least one error: " + result.Environment.AllErrors.FirstOrDefault() + ""));
                    }
                    else
                    {
                        var component = Type.GetType("System." + type);
                        if (component != null)
                        {
                            var converter = TypeDescriptor.GetConverter(component);

                            try
                            {
                                converter.ConvertFrom(actualValue);
                            }
                            catch (Exception e)
                            {
                                Assert.Fail("Cannot convert value \"" + actualValue + "\" of variable \"" + variable + "\" to type " + type + ". There was an exception: " + e.Message);
                            }
                        }
                    }
                }
                else
                {
                    Assert.Fail(error);
                }
            }
        }

        [Then(@"the output is approximately ""(.*)"" the size of the original input")]
        public void ThenTheOutputIsApproximatelyTheSizeOfTheOriginalInput(string compressionTimes)
        {
            var source = _scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(source.Environment, DataListUtil.RemoveLanguageBrackets(_scenarioContext.Get<string>(SourceHolder)),
                                       out string inputFilePath, out string error);

            GetScalarValueFromEnvironment(source.Environment, DataListUtil.RemoveLanguageBrackets(_scenarioContext.Get<string>(DestinationHolder)),
                                           out string outputFilePath, out error);

            var inputFile = new FileInfo(inputFilePath);
            var outputFile = new FileInfo(outputFilePath);
            var compressionTimesValue = double.Parse(compressionTimes);
            Assert.AreEqual(
                Math.Round(compressionTimesValue, 1),
                Math.Round(inputFile.Length / (double)outputFile.Length, 1));
        }

        public void AddVariableToVariableList(string resultVariable)
        {
            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
        }

        public void AddActivityToActivityList(string parentName, string activityName, Activity activity)
        {
            if (!_scenarioContext.TryGetValue("activityList", out Dictionary<string, Activity> activityList))
            {
                activityList = new Dictionary<string, Activity>();
                _scenarioContext.Add("activityList", activityList);
            }

            if (activityList.TryGetValue(parentName, out Activity parentActivity))
            {
                if (parentActivity is DsfSequenceActivity)
                {
                    var seq = parentActivity as DsfSequenceActivity;
                    seq.Activities.Add(activity);
                }
                else if (parentActivity is DsfForEachActivity)
                {
                    var forEachActivity = parentActivity as DsfForEachActivity;
                    var activityFunc = new ActivityFunc<string, bool> { Handler = activity };
                    forEachActivity.DataFunc = activityFunc;
                }
            }
            else
            {
                var findAllForEach = activityList.FirstOrDefault(pair =>
                {
                    var forEachActivity = pair.Value as DsfForEachActivity;
                    if (forEachActivity == null)
                    {
                        return false;
                    }

                    return forEachActivity.DataFunc.Handler != null && forEachActivity.DataFunc != null && forEachActivity.DataFunc.Handler as DsfForEachActivity != null;
                });
                if (findAllForEach.Value is DsfForEachActivity forEachParentActivity)
                {
                    var activityFunc = new ActivityFunc<string, bool> { Handler = activity };
                    DsfForEachActivity foundCorrectParentForEach = null;
                    while (forEachParentActivity != null)
                    {
                        if (forEachParentActivity.DataFunc?.Handler != null && forEachParentActivity.DataFunc.Handler.DisplayName == parentName)
                        {
                            foundCorrectParentForEach = forEachParentActivity.DataFunc.Handler as DsfForEachActivity;
                            break;
                        }
                        if (forEachParentActivity.DataFunc != null)
                        {
                            forEachParentActivity = forEachParentActivity.DataFunc.Handler as DsfForEachActivity;
                        }
                        else
                        {
                            forEachParentActivity = null;
                        }
                    }
                    if (foundCorrectParentForEach != null)
                    {
                        foundCorrectParentForEach.DataFunc = activityFunc;
                    }
                }
                else
                {
                    activityList.Add(activityName, activity);
                }


            }
        }

        public Dictionary<string, Activity> GetActivityList()
        {
            _scenarioContext.TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            return activityList;
        }

        public string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            var rawRef = DataListUtil.StripBracketsFromValue(value);
            var objRef = string.Empty;

            if (partType == enIntellisensePartType.RecordsetsOnly)
            {
                objRef = DataListUtil.ExtractRecordsetNameFromValue(rawRef);
            }
            else if (partType == enIntellisensePartType.RecordsetFields)
            {
                objRef = DataListUtil.ExtractFieldNameFromValue(rawRef);
            }

            return objRef;
        }



        public List<IDebugItemResult> GetInputDebugItems(Activity act, IExecutionEnvironment env)
        {
            var result = _scenarioContext.Get<IDSFDataObject>("result");

            try
            {
                if (act is DsfAbstractMultipleFilesActivity multipleFilesActivity)
                {
                    return DebugItemResults(multipleFilesActivity, result.Environment);
                }

                if (act is DsfActivityAbstract<string> dsfActivityAbstractString)
                {
                    return DebugItemResults(dsfActivityAbstractString, result.Environment);
                }

                if (act is DsfActivityAbstract<bool> dsfActivityAbstractBool)
                {
                    return DebugItemResults(dsfActivityAbstractBool, result.Environment);
                }

                var activity = _scenarioContext.Get<DsfActivityAbstract<string>>("activity");
                return DebugItemResults(activity, env);
            }
            catch (Exception ex)
            {
                var activity = _scenarioContext.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugInputs(result.Environment, 0)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
        }


        List<IDebugItemResult> DebugItemResults<T>(DsfActivityAbstract<T> dsfActivityAbstractString, IExecutionEnvironment dl)
        {
            var debugInputs = dsfActivityAbstractString.GetDebugInputs(dl, 0);
            return debugInputs
                .SelectMany(r => r.ResultsList)
                 .ToList();
        }

        public List<IDebugItemResult> GetOutputDebugItems(Activity act, IExecutionEnvironment dl)
        {

            try
            {
                var activity = act as DsfActivityAbstract<string> ?? _scenarioContext.Get<DsfActivityAbstract<string>>("activity");
                return activity.GetDebugOutputs(dl, 0)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
            catch (Exception ex)
            {

                var activity = _scenarioContext.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugOutputs(dl, 0)
                     .SelectMany(r => r.ResultsList)
                     .ToList();
            }
        }

        List<IDebugItemResult> BuildExpectedDebugItems(Table table)
        {
            var columnHeaders = table.Header.ToArray();
            var list = new List<IDebugItemResult>();


            foreach (TableRow row in table.Rows)
            {
                for (int index = 0; index < columnHeaders.Length; index++)
                {
                    var columnHeader = columnHeaders[index];
                    BuildDebugItems(row, index, columnHeader, list);
                }
            }
            return list.ToList();
        }



        void BuildDebugItems(TableRow row, int index, string columnHeader, List<IDebugItemResult> list)
        {
            if (!string.IsNullOrEmpty(row[index]))
            {
                var debugItemResult = new DebugItemResult { Label = columnHeader };
                var rowValue = row[index];
                if (columnHeader == "Username")
                {
                    rowValue = rowValue.ResolveDomain();
                }

                if (columnHeader == "Statement")
                {
                    debugItemResult.Value = rowValue;
                    debugItemResult.Type = DebugItemResultType.Value;
                    list.Add(debugItemResult);
                    return;
                }

                if (columnHeader == "Source Path" || columnHeader == "Destination Path")
                {
                    rowValue = FixBrokenHostname(rowValue);
                }

                if (rowValue.Contains(" ="))
                {
                    string[] multipleVarsOneLine;

                    if (rowValue.Split('=').Length < 3 || rowValue.StartsWith("="))
                    {
                        multipleVarsOneLine = new[] { rowValue };
                    }
                    else
                    {
                        multipleVarsOneLine = rowValue.Split(',');
                    }

                    if (multipleVarsOneLine.Length > 0)
                    {
                        foreach (var singleRow in multipleVarsOneLine)
                        {
                            AddSingleDebugResult(singleRow, debugItemResult);
                            list.Add(debugItemResult);
                        }
                    }
                }
                else if (rowValue.StartsWith("="))
                {
                    debugItemResult.Value = rowValue;
                    debugItemResult.Type = DebugItemResultType.Value;
                    debugItemResult.Variable = rowValue.Replace("=", "");
                    list.Add(debugItemResult);
                }
                else
                {
                    if (!string.IsNullOrEmpty(columnHeader) && columnHeader.Equals("#"))
                    {

                        debugItemResult.Label = rowValue;
                        debugItemResult.Value = "";
                        debugItemResult.Type = DebugItemResultType.Value;
                    }
                    else
                    {
                        // Handle async stuff
                        if (rowValue.Contains("asynchronously:"))
                        {
                            var endIdx = rowValue.IndexOf(":", StringComparison.Ordinal);
                            endIdx += 1;
                            var label = rowValue.Substring(0, endIdx);
                            var value = rowValue.Substring(endIdx);
                            debugItemResult.Label = label;
                            debugItemResult.Value = value;
                        }
                        else
                        {
                            debugItemResult.Value = rowValue.Replace("\\r", "").Replace("\n", Environment.NewLine);
                            debugItemResult.Type = DebugItemResultType.Value;
                        }
                    }


                    list.Add(debugItemResult);
                }
            }
        }

        void AddSingleDebugResult(string rowValue, DebugItemResult debugItemResult)
        {
            var variableValuePair = rowValue.Split(new[] { " =" }, StringSplitOptions.None);
            debugItemResult.Variable = variableValuePair[0];
            debugItemResult.Value = variableValuePair[1];
            debugItemResult.Type = DebugItemResultType.Variable;
            var variable = debugItemResult.Variable;
            if (DataListUtil.IsValueRecordset(variable))
            {
                var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(variable);
                if (!string.IsNullOrEmpty(indexRegionFromRecordset))
                {
                    int.TryParse(indexRegionFromRecordset, out int indexForRecset);

                    if (!_scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList))
                    {
                        return;
                    }

                    var indexType = enRecordsetIndexType.Star;
                    if (variableList.Find(tuple => tuple.Item1 == variable) != null)
                    {
                        indexType = enRecordsetIndexType.Numeric;
                    }

                    if (indexForRecset > 0 && indexType != enRecordsetIndexType.Numeric)
                    {
                        var indexOfOpenningBracket = variable.IndexOf("(", StringComparison.Ordinal) + 1;
                        debugItemResult.GroupIndex = indexForRecset;
                        var groupName = variable.Substring(0, indexOfOpenningBracket) + "*" + variable.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
                        debugItemResult.GroupName = variableList.Find(tuple => tuple.Item1 == groupName) != null ? groupName : groupName.Replace("*", "");
                    }
                }
            }
        }

        void CollectionsAssert(List<IDebugItemResult> expectedDebugItems, List<IDebugItemResult> inputDebugItems)
        {
            inputDebugItems.Count.Should().Be(expectedDebugItems.Count);

            RemoveTralingAndLeadingSpaces(expectedDebugItems, inputDebugItems);

            for (int i = 0; i < expectedDebugItems.Count; i++)
            {
                Verify(expectedDebugItems[i].Label ?? "", inputDebugItems[i].Label ?? "", "Labels", i);
                Verify(expectedDebugItems[i].Variable ?? "", inputDebugItems[i].Variable ?? "", "Variables", i);
                if (expectedDebugItems[i].Value != null && expectedDebugItems[i].Value.Equals("!!MoreLink!!"))
                {
                    Assert.IsFalse(string.IsNullOrEmpty(inputDebugItems[i].MoreLink));
                }
                else if (expectedDebugItems[i].Value != null && expectedDebugItems[i].Value.Equals("!!DateWithMS!!"))
                {
                    var dt = inputDebugItems[i].Value.Split(new[] { '/', ':', '.', ' ' });
                    Assert.IsTrue(dt.Length == 8);
                    Assert.IsTrue(inputDebugItems[i].Value.Contains("."));
                    Assert.IsTrue(int.TryParse(dt[6], out int val));
                    Assert.IsTrue(dt.Last().EndsWith("AM") || dt.Last().EndsWith("PM"));
                }
                else
                {
                    Verify(expectedDebugItems[i].Value ?? "", inputDebugItems[i].Value ?? "", "Values", i, inputDebugItems[i].Variable);
                }
            }
        }

        void Verify(string expectedValue, string actualValue, string name, int index, string variable = "")
        {
            expectedValue = expectedValue.Replace("‡", "=");
            var type = "";

            if (!string.IsNullOrEmpty(expectedValue) && !expectedValue.Equals(actualValue, StringComparison.InvariantCultureIgnoreCase))
            {
                if (expectedValue == "String" || expectedValue == "Int32" || expectedValue == "Guid" || expectedValue == "DateTime" || expectedValue == "Double")
                {
                    type = expectedValue;
                }
            }

            if (expectedValue.Contains("A.D."))
            {
                var eraValue = CultureInfo.InvariantCulture.DateTimeFormat.GetEra("A.D.");
                if (eraValue == -1) //The Era value does not use punctuation
                {
                    actualValue = actualValue.Replace("A.D.", "AD");
                }
                else
                {
                    actualValue = actualValue.Replace("AD", "A.D.");
                }
            }
            if (expectedValue.Contains("B.C."))
            {
                var eraValue = CultureInfo.InvariantCulture.DateTimeFormat.GetEra("A.D.");
                if (eraValue == -1) //The Era value does not use punctuation
                {
                    actualValue = actualValue.Replace("B.C.", "BC");
                }
                else
                {
                    actualValue = actualValue.Replace("BC", "B.C.");
                }
            }
            if (!string.IsNullOrEmpty(variable) && variable.Contains("@"))
            {
                var actualCleanJson = actualValue.Replace("\\r\\n", "").Replace(Environment.NewLine, "").Replace(" ", "");
                var expetedCleanJson = expectedValue.Replace("\\r\\n", "").Replace(Environment.NewLine, "").Replace(" ", "");
                StringAssert.Contains(actualCleanJson, expetedCleanJson);
            }
            else if (string.IsNullOrEmpty(type) && actualValue != null)
            {
                actualValue.Replace("\\r\\n", Environment.NewLine).Should().Be(expectedValue, name + " are not equal at index" + index);
            }
            else
            {
                var component = Type.GetType("System." + type);
                if (component != null)
                {
                    var converter = TypeDescriptor.GetConverter(component);

                    try
                    {
                        converter.ConvertFrom(actualValue);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Value is not expected type: "+ ex.Message);
                    }
                }
            }
        }

        static void RemoveTralingAndLeadingSpaces(List<IDebugItemResult> expectedDebugItems, List<IDebugItemResult> inputDebugItems)
        {
            for (int i = 0; i < expectedDebugItems.Count; i++)
            {
                if (expectedDebugItems[i].Label != null)
                {
                    expectedDebugItems[i].Label = expectedDebugItems[i].Label.Replace('"', ' ').Trim();
                }

                if (inputDebugItems[i].Label != null)
                {
                    inputDebugItems[i].Label = inputDebugItems[i].Label.Replace('"', ' ').Trim();
                }

                if (expectedDebugItems[i].Value != null)
                {
                    expectedDebugItems[i].Value = expectedDebugItems[i].Value.Replace('"', ' ').Trim();
                }

                if (inputDebugItems[i].Value != null)
                {
                    inputDebugItems[i].Value = inputDebugItems[i].Value.Replace('"', ' ').Trim();
                }

                if (expectedDebugItems[i].Variable != null)
                {
                    expectedDebugItems[i].Variable = expectedDebugItems[i].Variable.Replace('"', ' ').Trim();
                }

                if (inputDebugItems[i].Variable != null)
                {
                    inputDebugItems[i].Variable = inputDebugItems[i].Variable.Replace('"', ' ').Trim();
                }
            }
        }

        [Given(@"""(.*)"" tab is opened")]
        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = _scenarioContext.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [When(@"validating the delete tool from view model")]
        public void WhenValidatingTheDeleteToolFromViewModel()
        {
            ValidateFromModelView();
        }


        public List<IActionableErrorInfo> ValidateFromModelView()
        {
            if (_scenarioContext.ContainsKey("viewModel"))
            {
                var viewModel = _scenarioContext.Get<FileActivityDesignerViewModel>("viewModel");
                var currentViewModel = viewModel;
                currentViewModel.Validate();
                return currentViewModel.Errors;
            }
            return null;
        }

        public List<IActionableErrorInfo> ValidateFromAdvancedRecordsetDesignerViewModel()
        {
            if (_scenarioContext.ContainsKey("viewModel"))
            {
                var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
                var currentViewModel = viewModel;
                currentViewModel.Validate();
                return currentViewModel.Errors;
            }
            return null;
        }

        [BeforeTestRun]
        public static void CopyEncryptionKey()
        {
            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            }
            var keyPath = @"C:\Temp\key.opk";
            if (!File.Exists(keyPath))
            {
                var ToolsSpecsAssembly = Assembly.GetExecutingAssembly();
                using (Stream stream = ToolsSpecsAssembly.GetManifestResourceStream("Warewolf.Tools.Specs.key.opk"))
                {
                    using (var fileStream = File.Create(keyPath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
