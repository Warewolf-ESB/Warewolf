using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using ActivityUnitTests;
using Dev2.Data.PathOperations.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.BaseTypes
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
        public const string SourceUsernameHolder = "sourceUsername";
        public const string SourcePasswordHolder = "sourcePassword";

        [Then(@"the execution has ""(.*)"" error")]
        public void ThenTheExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            var comiler = DataListFactory.CreateDataListCompiler();
            // 06.01.2014 Line Below is Causing Compile Errors ;)
            string fetchErrors = comiler.FetchErrors(result.DataListID);
            //string fetchErrors = RecordSetBases.FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actual ? "did not occur" : "did occur" + fetchErrors);

            Assert.IsTrue(expected == actual, message);
        }

        [Then(@"the debug inputs as")]
        public void ThenTheDebugInputsAs(Table table)
        {
            var inputDebugItems = GetInputDebugItems();
            ThenTheDebugInputsAs(table, inputDebugItems);
        }

        public void ThenTheDebugInputsAs(Table table, List<DebugItemResult> inputDebugItems)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);
            CollectionsAssert(expectedDebugItems, inputDebugItems);
        }

        [Then(@"the debug output as")]
        public void ThenTheDebugOutputAs(Table table)
        {
            var outputDebugItems = GetOutputDebugItems();
            ThenTheDebugOutputAs(table, outputDebugItems);
        }

        public void ThenTheDebugOutputAs(Table table, List<DebugItemResult> outputDebugItems, bool isDataMerge = false)
        {
            var expectedDebugItems = BuildExpectedDebugItems(table);

            if(isDataMerge)
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

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(pathVariable, location));

            ScenarioContext.Current.Add(SourceHolder, string.IsNullOrEmpty(pathVariable) ? location : pathVariable);
            ScenarioContext.Current.Add(ActualSourceHolder, location);
        }

        [Given(@"assign error to variable ""(.*)""")]
        public void GivenAssignErrorToVariable(string errorVariable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
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

        void CreateSourceFileWithSomeDummyData()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            IActivityIOPath source = ActivityIOFactory.CreatePathFromString(ScenarioContext.Current.Get<string>(ActualSourceHolder),
                            ScenarioContext.Current.Get<string>(SourceUsernameHolder),
                            ScenarioContext.Current.Get<string>(SourcePasswordHolder),
                            true);
            var ops = ActivityIOFactory.CreatePutRawOperationTO(WriteType.Overwrite, Guid.NewGuid().ToString());
            IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
            if(sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.File)
            {
                broker.PutRaw(sourceEndPoint, ops);
            }

        }

        [Given(@"source credentials as '(.*)' and '(.*)'")]
        public void GivenSourceCredentialsAs(string userName, string password)
        {
            ScenarioContext.Current.Add(SourceUsernameHolder, userName.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add(SourcePasswordHolder, password.Replace('"', ' ').Trim());

            CreateSourceFileWithSomeDummyData();
        }

        [Given(@"I have a destination path '(.*)' with value '(.*)'")]
        public void GivenIHaveADestinationPathWithValue(string pathVariable, string location)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
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

        [Given(@"result as '(.*)'")]
        public void GivenResultAs(string resultVar)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
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
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(errorVariable),
                                       out errorValue, out error);
            errorValue = errorValue.Replace('"', ' ').Trim();

            //Call the service and get the result
            WebClient webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
            var webCallResult = webClient.DownloadString(webservice);
            StringAssert.Contains(webCallResult, errorValue);
        }


        [Then(@"the result variable '(.*)' will be '(.*)'")]
        public void ThenTheResultVariableWillBe(string variable, string value)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                value = value.Replace('"', ' ').Trim();

                if(string.IsNullOrEmpty(value))
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
                GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                var type = "";
                if(value == "String" || value == "Int32" || value == "Guid" || value == "DateTime")
                {
                    type = value;
                }
                if(string.IsNullOrEmpty(type))
                {
                    Assert.AreEqual(value, actualValue);
                }
                else
                {
                    Type component = Type.GetType("System." + type);
                    if(component != null)
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

        public static void AddVariableToVariableList(string resultVariable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
        }

        public static void AddActivityToActivityList(string parentName, string activityName, Activity activity)
        {
            Dictionary<string, Activity> activityList;
            if(!ScenarioContext.Current.TryGetValue("activityList", out activityList))
            {
                activityList = new Dictionary<string, Activity>();
                ScenarioContext.Current.Add("activityList", activityList);
            }

            Activity parentActivity;
            if(activityList.TryGetValue(parentName, out parentActivity))
            {
                if(parentActivity is DsfSequenceActivity)
                {
                    var seq = parentActivity as DsfSequenceActivity;
                    seq.Activities.Add(activity);
                }
            }
            else
            {
                activityList.Add(activityName, activity);
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

            if(partType == enIntellisensePartType.RecordsetsOnly)
            {
                objRef = DataListUtil.ExtractRecordsetNameFromValue(rawRef);
            }
            else if(partType == enIntellisensePartType.RecordsetFields)
            {
                objRef = DataListUtil.ExtractFieldNameFromValue(rawRef);
            }

            return objRef;
        }

        public static List<DebugItemResult> GetInputDebugItems(Activity act = null)
        {
            ErrorResultTO errors;
            var comiler = DataListFactory.CreateDataListCompiler();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            IBinaryDataList dl = comiler.FetchBinaryDataList(result.DataListID, out errors);

            try
            {
                DsfActivityAbstract<string> dsfActivityAbstractString = act as DsfActivityAbstract<string>;
                if(dsfActivityAbstractString != null)
                {
                    return DebugItemResults(dsfActivityAbstractString, dl);
                }
                DsfActivityAbstract<bool> dsfActivityAbstractBool = act as DsfActivityAbstract<bool>;
                if(dsfActivityAbstractBool != null)
                {
                    return DebugItemResults(dsfActivityAbstractBool, dl);
                }
                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<string>>("activity");
                return DebugItemResults(activity, dl);
            }
            catch
            {
                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugInputs(dl)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
        }

        static List<DebugItemResult> DebugItemResults<T>(DsfActivityAbstract<T> dsfActivityAbstractString, IBinaryDataList dl)
        {
            return dsfActivityAbstractString.GetDebugInputs(dl)
                .SelectMany(r => r.ResultsList)
                .ToList();
        }

        public static List<DebugItemResult> GetOutputDebugItems(Activity act = null)
        {
            ErrorResultTO errors;
            var comiler = DataListFactory.CreateDataListCompiler();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            IBinaryDataList dl = comiler.FetchBinaryDataList(result.DataListID, out errors);

            try
            {
                var activity = act as DsfActivityAbstract<string> ?? ScenarioContext.Current.Get<DsfActivityAbstract<string>>("activity");
                return activity.GetDebugOutputs(dl)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
            catch
            {

                var activity = ScenarioContext.Current.Get<DsfActivityAbstract<bool>>("activity");
                return activity.GetDebugOutputs(dl)
                    .SelectMany(r => r.ResultsList)
                    .ToList();
            }
        }

        static List<DebugItemResult> BuildExpectedDebugItems(Table table)
        {
            var columnHeaders = table.Header.ToArray();
            List<DebugItemResult> list = new List<DebugItemResult>();
            foreach(TableRow row in table.Rows)
            {
                for(int index = 0; index < columnHeaders.Length; index++)
                {
                    var columnHeader = columnHeaders[index];
                    BuildDebugItems(row, index, columnHeader, list);
                }
            }
            return list;
        }

        static void BuildDebugItems(TableRow row, int index, string columnHeader, List<DebugItemResult> list)
        {
            if(!string.IsNullOrEmpty(row[index]))
            {
                var debugItemResult = new DebugItemResult { Label = columnHeader };
                var rowValue = row[index];

                if(rowValue.Contains(" ="))
                {
                    string[] multipleVarsOneLine;

                    if(rowValue.Split('=').Length < 3 || rowValue.StartsWith("="))
                    {
                        multipleVarsOneLine = new[] { rowValue };
                    }
                    else
                    {
                        multipleVarsOneLine = rowValue.Split(',');
                    }

                    if(multipleVarsOneLine.Length > 0)
                    {
                        foreach(var singleRow in multipleVarsOneLine)
                        {
                            AddSingleDebugResult(singleRow, debugItemResult);
                            list.Add(debugItemResult);
                        }
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(columnHeader) && columnHeader.Equals("#"))
                    {
                        debugItemResult.Label = rowValue;
                        debugItemResult.Value = "";
                        debugItemResult.Type = DebugItemResultType.Value;
                    }
                    else
                    {
                        debugItemResult.Value = rowValue;
                        debugItemResult.Type = DebugItemResultType.Value;
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
            if(DataListUtil.IsValueRecordset(variable))
            {
                var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(variable);
                if(!string.IsNullOrEmpty(indexRegionFromRecordset))
                {
                    int indexForRecset;
                    int.TryParse(indexRegionFromRecordset, out indexForRecset);

                    List<Tuple<string, string>> variableList;
                    if(!ScenarioContext.Current.TryGetValue("variableList", out variableList))
                    {
                        return;
                    }

                    var indexType = enRecordsetIndexType.Star;
                    if(variableList.Find(tuple => tuple.Item1 == variable) != null)
                    {
                        indexType = enRecordsetIndexType.Numeric;
                    }

                    if(indexForRecset > 0 && indexType != enRecordsetIndexType.Numeric)
                    {
                        var indexOfOpenningBracket = variable.IndexOf("(", StringComparison.Ordinal) + 1;
                        debugItemResult.GroupIndex = indexForRecset;
                        var groupName = variable.Substring(0, indexOfOpenningBracket) + "*" + variable.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
                        debugItemResult.GroupName = variableList.Find(tuple => tuple.Item1 == groupName) != null ? groupName : groupName.Replace("*", "");
                    }
                }
            }
        }

        static void CollectionsAssert(List<DebugItemResult> expectedDebugItems, List<DebugItemResult> inputDebugItems)
        {
            Assert.AreEqual(expectedDebugItems.Count, inputDebugItems.Count);

            RemoveTralingAndLeadingSpaces(expectedDebugItems, inputDebugItems);

            for(int i = 0; i < expectedDebugItems.Count; i++)
            {
                Verify(expectedDebugItems[i].Label, inputDebugItems[i].Label, "Labels", i);
                Verify(expectedDebugItems[i].Value, inputDebugItems[i].Value, "Values", i);
                Verify(expectedDebugItems[i].Variable, inputDebugItems[i].Variable, "Variables", i);
            }
        }

        static void Verify(string expectedValue, string actualValue, string name, int index)
        {
            string type = "";

            if(!string.IsNullOrEmpty(expectedValue) && !expectedValue.Equals(actualValue, StringComparison.InvariantCultureIgnoreCase))
            {
                if(expectedValue == "String" || expectedValue == "Int32" || expectedValue == "Guid" || expectedValue == "DateTime")
                {
                    type = expectedValue;
                }
            }


            if(string.IsNullOrEmpty(type) && actualValue != null)
            {
                Assert.AreEqual(expectedValue, actualValue.Replace("\\r\\n", Environment.NewLine), name + " are not equal at index" + index);
            }
            else
            {
                Type component = Type.GetType("System." + type);
                if(component != null)
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

        static void RemoveTralingAndLeadingSpaces(List<DebugItemResult> expectedDebugItems, List<DebugItemResult> inputDebugItems)
        {
            for(int i = 0; i < expectedDebugItems.Count; i++)
            {
                if(expectedDebugItems[i].Label != null)
                {
                    expectedDebugItems[i].Label = expectedDebugItems[i].Label.Replace('"', ' ').Trim();
                }

                if(inputDebugItems[i].Label != null)
                {
                    inputDebugItems[i].Label = inputDebugItems[i].Label.Replace('"', ' ').Trim();
                }

                if(expectedDebugItems[i].Value != null)
                {
                    expectedDebugItems[i].Value = expectedDebugItems[i].Value.Replace('"', ' ').Trim();
                }

                if(inputDebugItems[i].Value != null)
                {
                    inputDebugItems[i].Value = inputDebugItems[i].Value.Replace('"', ' ').Trim();
                }

                if(expectedDebugItems[i].Variable != null)
                {
                    expectedDebugItems[i].Variable = expectedDebugItems[i].Variable.Replace('"', ' ').Trim();
                }

                if(inputDebugItems[i].Variable != null)
                {
                    inputDebugItems[i].Variable = inputDebugItems[i].Variable.Replace('"', ' ').Trim();
                }
            }
        }
    }
}