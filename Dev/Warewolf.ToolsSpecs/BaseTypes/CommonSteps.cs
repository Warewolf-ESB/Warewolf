﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Specs.Toolbox.FileAndFolder;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Warewolf.Tools.Specs.BaseTypes
{
    [Binding]
    public class CommonSteps : BaseActivityUnitTest
    {
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

        [Then(@"the execution has ""(.*)"" error")]
        [Then(@"the execution has '(.*)' error")]
        public void ThenTheExecutionHasError(string anError)
        {
            bool expectedError = anError.Equals("AN",StringComparison.OrdinalIgnoreCase);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            string fetchErrors =  result.Environment.FetchErrors();
            bool actuallyHasErrors = result.Environment.Errors.Count > 0 || result.Environment.AllErrors.Count > 0;
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actuallyHasErrors ? "did not occur" : "did occur" + fetchErrors);

            var hasAnError = expectedError == actuallyHasErrors;
            var errorMessageMatches = anError.Equals(fetchErrors, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(hasAnError || errorMessageMatches, message);
        }

        [Then(@"the debug inputs as")]
        public void ThenTheDebugInputsAs(Table table)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            if(!result.Environment.HasErrors())
            {
                var inputDebugItems = GetInputDebugItems(null, result.Environment);
                ThenTheDebugInputsAs(table, inputDebugItems);
            }
        }

        public void ThenTheDebugInputsAs(Table table, List<IDebugItemResult> inputDebugItems, bool isDataMerge = false)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);
            CollectionsAssert(expectedDebugItems, inputDebugItems);
        }

        [Then(@"the debug output as")]
        public void ThenTheDebugOutputAs(Table table)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            if(!result.Environment.HasErrors())
            {
                var outputDebugItems = GetOutputDebugItems(null, result.Environment);
                ThenTheDebugOutputAs(table, outputDebugItems);
            }
        }

        public void ThenTheDebugOutputAs(Table table, List<IDebugItemResult> outputDebugItems, bool isDataMerge = false)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);

            // Very specific case ;)
            if (isDataMerge && expectedDebugItems.Count == 2)
            {
                // chop the first one off ;)
                expectedDebugItems.RemoveAt(0);
            }

            CollectionsAssert(expectedDebugItems, outputDebugItems);
        }

        [Given(@"I have a source path '(.*)' with value '(.*)'")]
        public void GivenIHaveASourcePathWithValue(string pathVariable, string location)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(pathVariable, location));

            ScenarioContext.Current.Add(SourceHolder, string.IsNullOrEmpty(pathVariable) ? location : pathVariable);
            ScenarioContext.Current.Add(ActualSourceHolder, location);
        }

        [Given(@"use private public key for source is '(.*)'")]
        public void GivenUsePrivatePublicKeyForSourceIs(string sourceKey)
        {
            ScenarioContext.Current.Add(SourcePrivatePublicKeyFile, sourceKey);
        }


        [Given(@"assign error to variable ""(.*)""")]
        public void GivenAssignErrorToVariable(string errorVariable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(errorVariable, ""));
            ScenarioContext.Current.Add("errorVariable", errorVariable);
        }

        [Given(@"call the web service ""(.*)""")]
        public void GivenCallTheWebService(string onErrorWebserviceToCall)
        {
            ScenarioContext.Current.Add("webserviceToCall", onErrorWebserviceToCall);
        }

        void CreateSourceFileWithSomeDummyData(int numberOfGuids = 1)
        {
            try
            {
                Dev2Logger.Debug(string.Format("Source File: {0}", ScenarioContext.Current.Get<string>(ActualSourceHolder)));
                var broker = ActivityIOFactory.CreateOperationsBroker();
                IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(ActualSourceHolder),
                    ScenarioContext.Current.Get<string>(SourceUsernameHolder),
                    ScenarioContext.Current.Get<string>(SourcePasswordHolder),
                    true,"");
                StringBuilder sb = new StringBuilder();
                Enumerable.Range(1, numberOfGuids).ToList().ForEach(x => sb.Append(Guid.NewGuid().ToString()));
                var ops = ActivityIOFactory.CreatePutRawOperationTO(WriteType.Overwrite, sb.ToString());
                IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
                if (sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.File)
                {
                    var result = broker.PutRaw(sourceEndPoint, ops);
                    if (result != "Success")
                    {
                        result = broker.PutRaw(sourceEndPoint, ops);
                        if (result != "Success")
                        {
                            Dev2Logger.Debug("Create Source File for file op test error");
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
                Dev2Logger.Debug("Create Source File for file op test error", e);
            }

        }

        [Given(@"source credentials as '(.*)' and '(.*)'")]
        public void GivenSourceCredentialsAs(string userName, string password)
        {
            ScenarioContext.Current.Add(SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add(SourcePasswordHolder, password.Replace('"', ' ').Trim());

            CreateSourceFileWithSomeDummyData();
        }

        [Given(@"source credentials as '(.*)' and '(.*)' for zip tests")]
        public void GivenSourceCredentialsAsAndForZipTests(string userName, string password)
        {
            ScenarioContext.Current.Add(SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add(SourcePasswordHolder, password.Replace('"', ' ').Trim());

            CreateSourceFileWithSomeDummyData(1000);
        }

        [Given(@"I have a destination path '(.*)' with value '(.*)'")]
        public void GivenIHaveADestinationPathWithValue(string pathVariable, string location)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(pathVariable, location));

            ScenarioContext.Current.Add(DestinationHolder, string.IsNullOrEmpty(pathVariable) ? location : pathVariable);
            ScenarioContext.Current.Add(ActualDestinationHolder, location);
        }

        [Given(@"overwrite is '(.*)'")]
        public void GivenOverwriteIs(string overwrite)
        {
            bool isOverwrite;
            bool.TryParse(overwrite, out isOverwrite);
            ScenarioContext.Current.Add(OverwriteHolder, isOverwrite);
        }

        [Given(@"destination credentials as '(.*)' and '(.*)'")]
        public void GivenDestinationCredentialsAs(string userName, string password)
        {
            ScenarioContext.Current.Add(DestinationUsernameHolder, userName.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add(DestinationPasswordHolder, password.Replace('"', ' ').Trim());
        }

        [Given(@"use private public key for destination is '(.*)'")]
        public void GivenUsePrivatePublicKeyForDestinationIs(string destinationKey)
        {
            ScenarioContext.Current.Add(DestinationPrivateKeyFile, destinationKey);
        }

        [When(@"validating the tool")]
        public void WhenValidatingTheTool()
        {
            var dev2Activity = TestStartNode.Action as IDev2Activity;
            if (dev2Activity != null)
            {
                var validationErrors = dev2Activity.PerformValidation();
                ScenarioContext.Current.Add(ValidationErrors, validationErrors);
            }
        }

        [Then(@"validation is '(.*)'")]
        public void ThenValidationIs(string expectedValidationResult)
        {
            IList<IActionableErrorInfo> validationErrors;
            ScenarioContext.Current.TryGetValue(ValidationErrors, out validationErrors);
            if (expectedValidationResult.Equals("True", StringComparison.OrdinalIgnoreCase))
            {
                if (validationErrors != null)
                {
                    Assert.AreEqual(0, validationErrors.Count);
                }
            }
            else
            {
                Assert.IsNotNull(validationErrors);
                Assert.AreNotEqual(0, validationErrors.Count);
            }
        }

        [Then(@"validation message is '(.*)'")]
        public void ThenValidationMessageIs(string validationMessage)
        {
            IList<IActionableErrorInfo> validationErrors;
            ScenarioContext.Current.TryGetValue(ValidationErrors, out validationErrors);
            if (string.IsNullOrEmpty(validationMessage))
            {
                if (validationErrors != null)
                {
                    Assert.AreEqual(0, validationErrors.Count);
                }
            }
            else
            {
                Assert.IsNotNull(validationErrors);
                var completeMessage = string.Join(";", validationErrors.Select(info => info.Message));
                Assert.AreEqual(validationMessage, completeMessage);
            }
        }

        [Given(@"result as '(.*)'")]
        public void GivenResultAs(string resultVar)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(resultVar, ""));

            ScenarioContext.Current.Add(ResultVariableHolder, resultVar);
        }

        [Then(@"the result from the web service ""(.*)"" will have the same data as variable ""(.*)""")]
        public void ThenTheResultFromTheWebServiceWillHaveTheSameDataAsVariable(string webservice, string errorVariable)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            //Get the error value
            string errorValue;
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(errorVariable),
                                       out errorValue, out error);
            errorValue = errorValue.Replace('"', ' ').Trim();

            var retryCount = 0;
            string webCallResult;

            do
            {
                retryCount++;
                //Call the service and get the result
                WebClient webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
                webCallResult = webClient.DownloadString(webservice);
            } while (webCallResult.Contains("<FatalError>") && retryCount < 10);
            StringAssert.Contains(webCallResult, errorValue);
        }


        [Then(@"the result variable '(.*)' will be '(.*)'")]
        public void ThenTheResultVariableWillBe(string variable, string value)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                value = value.Replace('"', ' ').Trim();

                if (string.IsNullOrEmpty(value))
                {
                    Assert.IsTrue(recordSetValues.Count == 0);
                }
                else
                {
                    Assert.AreEqual(recordSetValues[0], value);
                }
            }
            else
            {
                string actualValue;
                value = value.Replace('"', ' ').Trim();
                GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                if (string.IsNullOrEmpty(value))
                {
                    actualValue = "";
                }
                actualValue = actualValue.Replace('"', ' ').Trim();
                var type = "";
                if (value == "String" || value == "Int32" || value == "Guid" || value == "DateTime")
                {
                    type = value;
                }
                if (string.IsNullOrEmpty(type))
                {
                    Assert.AreEqual(value, actualValue);
                }
                else
                {
                    Type component = Type.GetType("System." + type);
                    if (component != null)
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(component);

                        try
                        {
                            converter.ConvertFrom(actualValue);
                        }
                        catch
                        {
                            Assert.Fail("Value is not expected type");
                        }
                    }
                }

            }
        }

        [Then(@"the output is approximately '(.*)' the size of the original input")]
        public void ThenTheOutputIsApproximatelyTheSizeOfTheOriginalInput(string compressionTimes)
        {
            var source = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string inputFilePath, outputFilePath, error;

            GetScalarValueFromEnvironment(source.Environment, DataListUtil.RemoveLanguageBrackets(ScenarioContext.Current.Get<string>(SourceHolder)),
                                       out inputFilePath, out error);

            GetScalarValueFromEnvironment(source.Environment, DataListUtil.RemoveLanguageBrackets(ScenarioContext.Current.Get<string>(DestinationHolder)),
                                           out outputFilePath, out error);

            var inputFile = new FileInfo(inputFilePath);
            var outputFile = new FileInfo(outputFilePath);
            double compressionTimesValue = double.Parse(compressionTimes);
            Assert.AreEqual(
                Math.Round(compressionTimesValue, 1),
                Math.Round(inputFile.Length / (double)outputFile.Length, 1));
        }

        public static void AddVariableToVariableList(string resultVariable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
        }

        public static void AddActivityToActivityList(string parentName, string activityName, Activity activity)
        {
            Dictionary<string, Activity> activityList;
            if (!ScenarioContext.Current.TryGetValue("activityList", out activityList))
            {
                activityList = new Dictionary<string, Activity>();
                ScenarioContext.Current.Add("activityList", activityList);
            }

            Activity parentActivity;
            if (activityList.TryGetValue(parentName, out parentActivity))
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
                        return false;
                    return forEachActivity.DataFunc.Handler != null && forEachActivity.DataFunc != null && forEachActivity.DataFunc.Handler as DsfForEachActivity != null;
                });
                var forEachParentActivity = findAllForEach.Value as DsfForEachActivity;
                if (forEachParentActivity != null)
                {
                    var activityFunc = new ActivityFunc<string, bool> { Handler = activity };
                    DsfForEachActivity foundCorrectParentForEach = null;
                    while (forEachParentActivity != null)
                    {
                        if (forEachParentActivity.DataFunc != null && forEachParentActivity.DataFunc.Handler != null && forEachParentActivity.DataFunc.Handler.DisplayName == parentName)
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

        public static Dictionary<string, Activity> GetActivityList()
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);
            return activityList;
        }

        public static string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            string rawRef = DataListUtil.StripBracketsFromValue(value);
            string objRef = string.Empty;

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

        public static List<IDebugItemResult> GetInputDebugItems(Activity act, IExecutionEnvironment env)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            try
            {
                DsfActivityAbstract<string> dsfActivityAbstractString = act as DsfActivityAbstract<string>;
                if (dsfActivityAbstractString != null)
                {
                    return DebugItemResults<string>(dsfActivityAbstractString, result.Environment);
                }
                DsfActivityAbstract<bool> dsfActivityAbstractBool = act as DsfActivityAbstract<bool>;
                if (dsfActivityAbstractBool != null)
                {
                    return DebugItemResults<bool>(dsfActivityAbstractBool, result.Environment);
                }
                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<string>>("activity");
                return DebugItemResults<string>(activity, env);
            }
            catch
            {
                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugInputs(result.Environment, 0)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
        }


        static List<IDebugItemResult> DebugItemResults<T>(DsfActivityAbstract<T> dsfActivityAbstractString, IExecutionEnvironment dl)
        {
            return dsfActivityAbstractString.GetDebugInputs(dl, 0)
                .SelectMany(r => r.ResultsList)
                .ToList();
        }
        public static List<IDebugItemResult> GetOutputDebugItems(Activity act, IExecutionEnvironment dl)
        {




            try
            {
                var activity = act as DsfActivityAbstract<string> ?? ScenarioContext.Current.Get<DsfActivityAbstract<string>>("activity");
                return activity.GetDebugOutputs(dl, 0)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
            catch
            {

                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugOutputs(dl, 0)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
        }

        static List<IDebugItemResult> BuildExpectedDebugItems(Table table)
        {
            var columnHeaders = table.Header.ToArray();
            List<IDebugItemResult> list = new List<IDebugItemResult>();


            foreach (TableRow row in table.Rows)
            {
                for (int index = 0; index < columnHeaders.Length; index++)
                {
                    var columnHeader = columnHeaders[index];
                    BuildDebugItems(row, index, columnHeader, list);
                }
            }
            return list;
        }



        static void BuildDebugItems(TableRow row, int index, string columnHeader, List<IDebugItemResult> list)
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
                            debugItemResult.Value = rowValue;
                            debugItemResult.Type = DebugItemResultType.Value;
                        }
                    }


                    list.Add(debugItemResult);
                }
            }
        }

        static void AddSingleDebugResult(string rowValue, DebugItemResult debugItemResult)
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
                    int indexForRecset;
                    int.TryParse(indexRegionFromRecordset, out indexForRecset);

                    List<Tuple<string, string>> variableList;
                    if (!ScenarioContext.Current.TryGetValue("variableList", out variableList))
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

        static void CollectionsAssert(List<IDebugItemResult> expectedDebugItems, List<IDebugItemResult> inputDebugItems)
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
                    int val;
                    Assert.IsTrue(int.TryParse(dt[6], out val));
                    Assert.IsTrue(Enumerable.Last<string>(dt).EndsWith("AM") || Enumerable.Last<string>(dt).EndsWith("PM"));

                }
                //2016/01/06 08:00:01.68
                else
                {
                    Verify(expectedDebugItems[i].Value ?? "", inputDebugItems[i].Value ?? "", "Values", i);
                }
            }
        }

        static void Verify(string expectedValue, string actualValue, string name, int index)
        {
            expectedValue = expectedValue.Replace("‡", "=");
            string type = "";

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
            if (string.IsNullOrEmpty(type) && actualValue != null)
            {
                actualValue.Replace("\\r\\n", Environment.NewLine).Should().Be(expectedValue, name + " are not equal at index" + index);
            }
            else
            {
                Type component = Type.GetType("System." + type);
                if (component != null)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(component);

                    try
                    {
                        converter.ConvertFrom(actualValue);
                    }
                    catch
                    {
                        Assert.Fail("Value is not expected type");
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
    }
}
