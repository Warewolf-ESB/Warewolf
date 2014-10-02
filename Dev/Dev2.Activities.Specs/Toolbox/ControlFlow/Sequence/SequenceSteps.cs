
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Enums;
using Dev2.Diagnostics.Debug;
using Dev2.Enums;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Sequence
{
    [Binding]
    public class SequenceSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [Given(@"I have a Sequence ""(.*)""")]
        public void GivenIHaveASequence(string sequenceName)
        {
            var dsfSequence = new DsfSequenceActivity { DisplayName = sequenceName };
            ScenarioContext.Current.Add("activityList", new Dictionary<string, Activity>());
            ScenarioContext.Current.Add("activity", dsfSequence);
            CommonSteps.AddActivityToActivityList("", sequenceName, dsfSequence);
            Assert.AreEqual(enFindMissingType.Sequence, dsfSequence.GetFindMissingType());
        }

        [Given(@"""(.*)"" contains an Assign ""(.*)"" as")]
        public void GivenContainsAnAssignAs(string parentName, string assignName, Table table)
        {
            DsfMultiAssignActivity assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };

            foreach(var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if(value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = string.Format("!~calculation~!{0}!~~calculation~!", value);
                }

                List<ActivityDTO> fieldCollection;
                ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

                CommonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            CommonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [Given(@"""(.*)"" contains an Unique ""(.*)"" as")]
        public void GivenContainsAnUniqueAs(string parentName, string activityName, Table table)
        {
            DsfUniqueActivity activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var inFields = tableRow["In Field(s)"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];


                CommonSteps.AddVariableToVariableList(result);

                activity.Result = result;
                activity.ResultFields = returnFields;
                activity.InFields = inFields;
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }



        [Given(@"""(.*)"" contains Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            CommonSteps.AddVariableToVariableList(resultVariable);

            DsfCalculateActivity calculateActivity = new DsfCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            CommonSteps.AddActivityToActivityList(parentName, activityName, calculateActivity);

        }



        [Given(@"""(.*)"" contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenCountOnInto(string parentName, string activityName, string recordSet, string result)
        {
            CommonSteps.AddVariableToVariableList(result);

            DsfCountRecordsetActivity countRecordsetActivity = new DsfCountRecordsetActivity { CountNumber = result, RecordsetName = recordSet, DisplayName = activityName };

            CommonSteps.AddActivityToActivityList(parentName, activityName, countRecordsetActivity);
        }

        [Given(@"""(.*)"" contains Delete ""(.*)"" as")]
        public void GivenItContainsDeleteAs(string parentName, string activityName, Table table)
        {
            DsfDeleteRecordActivity activity = new DsfDeleteRecordActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var result = tableRow["result"];
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(result);
                activity.RecordsetName = variable;
                activity.Result = result;
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" search ""(.*)"" and result ""(.*)"" as")]
        public void GivenItContainsFindRecordIndexSearchAndResultAs(string parentName, string activityName, string recsetToSearch, string resultVariable, Table table)
        {
            var result = resultVariable;
            var recset = recsetToSearch;
            CommonSteps.AddVariableToVariableList(result);
            DsfFindRecordsMultipleCriteriaActivity activity = new DsfFindRecordsMultipleCriteriaActivity { RequireAllFieldsToMatch = false, RequireAllTrue = false, Result = result, FieldsToSearch = recset, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var matchType = tableRow["Match Type"];
                var matchValue = tableRow["Match"];

                activity.ResultsCollection.Add(new FindRecordsTO(matchValue, matchType, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains find unique ""(.*)"" as")]
        public void GivenItContainFindUniqueAs(string parentName, string activityName, Table table)
        {
            DsfUniqueActivity activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var inFields = tableRow["In Fields"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];

                activity.InFields = inFields;
                activity.ResultFields = returnFields;
                activity.Result = result;

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains case convert ""(.*)"" as")]
        public void GivenItContainsCaseConvertAs(string parentName, string activityName, Table table)
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var conversionType = tableRow["Type"];

                activity.ConvertCollection.Add(new CaseConvertTO(variableToConvert, conversionType, variableToConvert, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenItContainsGatherSystemInfoAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(variable);

                enTypeOfSystemInformationToGather systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Random ""(.*)"" as")]
        public void GivenItContainsRandomAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfRandomActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var type = (enRandomType)Enum.Parse(typeof(enRandomType), tableRow["Type"]);
                var from = tableRow["From"];
                var to = tableRow["To"];
                var result = tableRow["Result"];

                CommonSteps.AddVariableToVariableList(result);

                activity.RandomType = type;
                activity.To = to;
                activity.From = from;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Format Number ""(.*)"" as")]
        public void GivenItContainsFormatNumberAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfNumberFormatActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var number = tableRow["Number"];
                var roundTo = tableRow["Rounding To"];
                var roundingType = tableRow["Rounding Selected"];
                var decimalPlacesToShow = tableRow["Decimal to show"];
                var result = tableRow["Result"];

                CommonSteps.AddVariableToVariableList(result);

                activity.Expression = number;
                activity.RoundingType = roundingType;
                activity.RoundingDecimalPlaces = roundTo;
                activity.DecimalPlacesToShow = decimalPlacesToShow;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Date and Time ""(.*)"" as")]
        public void GivenItContainsDateAndTimeAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfDateTimeActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var input1 = tableRow["Input"];
                var outputFormat = tableRow["Output Format"];
                var inputFormat = tableRow["Input Format"];
                var timeModifierAmount = tableRow["Add Time"];
                var result = tableRow["Result"];

                CommonSteps.AddVariableToVariableList(result);

                activity.DateTime = input1;
                activity.InputFormat = inputFormat;
                activity.OutputFormat = outputFormat;
                activity.TimeModifierAmountDisplay = timeModifierAmount;
                activity.TimeModifierType = "Years";
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Date and Time Difference ""(.*)"" as")]
        public void GivenItContainsDateAndTimeDifferenceAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfDateTimeDifferenceActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var input1 = tableRow["Input1"];
                var input2 = tableRow["Input2"];
                var inputFormat = tableRow["Input Format"];
                var output = tableRow["Output In"];
                var result = tableRow["Result"];

                CommonSteps.AddVariableToVariableList(result);

                activity.Input1 = input1;
                activity.Input2 = input2;
                activity.InputFormat = inputFormat;
                activity.OutputType = output;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Data Split ""(.*)"" as")]
        public void GivenItContainsDataSplitAs(string parentName, string activityName, Table table)
        {
            DsfDataSplitActivity activity = new DsfDataSplitActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var valueToSplit = string.IsNullOrEmpty(tableRow["String"]) ? "" : tableRow["String"];
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["At"];
                var include = tableRow["Include"] == "Selected";
                //var escapeChar = tableRow["Escape"];
                CommonSteps.AddVariableToVariableList(variable);
                if(!string.IsNullOrEmpty(valueToSplit))
                {
                    activity.SourceString = valueToSplit;
                }
                CommonSteps.AddVariableToVariableList(variable);
                activity.ResultsCollection.Add(new DataSplitDTO(variable, type, at, 1, include, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Replace ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsReplaceIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfReplaceActivity activity = new DsfReplaceActivity { Result = resultVariable, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["In Fields"];
                var find = tableRow["Find"];
                var replaceValue = tableRow["Replace With"];

                activity.FieldsToSearch = variable;
                activity.Find = find;
                activity.ReplaceWith = replaceValue;
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Find Index ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsFindIndexIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfIndexActivity activity = new DsfIndexActivity { Result = resultVariable, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["In Fields"];
                var index = tableRow["Index"];
                var character = tableRow["Character"];
                var direction = tableRow["Direction"];

                activity.InField = variable;
                activity.Index = index;
                activity.Characters = character;
                activity.Direction = direction;
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Create ""(.*)"" as")]
        public void GivenContainsAnCreateAs(string parentName, string activityName, Table table)
        {
            DsfPathCreate activity = new DsfPathCreate { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["File or Folder"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = variable;

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains an Delete Folder ""(.*)"" as")]
        public void GivenContainsAnDeleteFolderAs(string parentName, string activityName, Table table)
        {
            DsfPathDelete activity = new DsfPathDelete { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Recordset"];
                //var userName = tableRow["Username"];
                //var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.InputPath = variable;
                //activity.Username = userName;
                //activity.Password = password;

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsDataMergeAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfDataMergeActivity activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Base convert ""(.*)"" as")]
        public void GivenItContainsBaseConvertAs(string parentName, string activityName, Table table)
        {
            DsfBaseConvertActivity activity = new DsfBaseConvertActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var from = tableRow["From"];
                var to = tableRow["To"];

                activity.ConvertCollection.Add(new BaseConvertTO(variableToConvert, from, to, variableToConvert, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [When(@"the Sequence tool is executed")]
        public void WhenTheSequenceIsExecuted()
        {
            BuildDataList();

            var sequence = SetupSequence();

            TestStartNode = new FlowStep
            {
                Action = sequence
            };
            string errorVariable;
            ScenarioContext.Current.TryGetValue("errorVariable", out errorVariable);

            string webserviceToCall;
            ScenarioContext.Current.TryGetValue("webserviceToCall", out webserviceToCall);

            if(!string.IsNullOrEmpty(errorVariable))
            {
                sequence.OnErrorVariable = errorVariable;
            }
            if(!string.IsNullOrEmpty(webserviceToCall))
            {
                sequence.OnErrorWorkflow = webserviceToCall;
            }

            PerformExecution();
        }

        void PerformExecution()
        {
            var esbChannel = new EsbServicesEndpoint();
            var testDebugWriter = new TestDebugWriter();
            var debugWriter = DebugDispatcher.Instance.Get(Guid.Empty);
            if(debugWriter != null)
            {
                DebugDispatcher.Instance.Remove(Guid.Empty);
            }
            DebugDispatcher.Instance.Add(Guid.Empty, testDebugWriter);
            IDSFDataObject result = ExecuteProcess(isDebug: true, channel: esbChannel, throwException: false);
            try
            {
                DebugDispatcher.Instance.Remove(Guid.Empty);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //May already been removed
            }
            var debugStates = testDebugWriter.DebugStates.ToList();
            testDebugWriter.DebugStates.Clear();
            ScenarioContext.Current.Add("result", result);
            CheckDebugStates(debugStates);

        }

        void CheckDebugStates(IEnumerable<IDebugState> debugStates)
        {
            DsfSequenceActivity sequence;
            ScenarioContext.Current.TryGetValue("activity", out sequence);
            if(sequence != null)
            {
                var innerActivitiesDebugStates = debugStates.Where(state => state.ParentID.ToString() == sequence.UniqueID);
                var count = innerActivitiesDebugStates.Count();
                Assert.IsTrue(count > 0);
            }

            ScenarioContext.Current.Add("DebugStates", debugStates);
        }

        static DsfSequenceActivity SetupSequence()
        {
            DsfSequenceActivity sequence;
            ScenarioContext.Current.TryGetValue("activity", out sequence);
            //            var activityList = CommonSteps.GetActivityList();
            //
            //            foreach(var activity in activityList)
            //            {
            //                sequence.Activities.Add(activity.Value);
            //            }
            return sequence;
        }



        [Given(@"I have a ForEach ""(.*)"" as ""(.*)"" executions ""(.*)""")]
        public void GivenIHaveAForEachAsExecutions(string activityName, string forEachType, string numberOfExecutions)
        {
            DsfForEachActivity forEachActivity = new DsfForEachActivity { DisplayName = activityName };
            enForEachType typeOfForEach;
            Enum.TryParse(forEachType, out typeOfForEach);

            forEachActivity.ForEachType = typeOfForEach;
            forEachActivity.NumOfExections = numberOfExecutions;

            ScenarioContext.Current.Add(activityName, forEachActivity);
        }

        [When(@"the ForEach ""(.*)"" tool is executed")]
        public void WhenTheToolIsExecuted(string activityName)
        {
            BuildDataList();

            DsfForEachActivity forEachActivity;
            ScenarioContext.Current.TryGetValue(activityName, out forEachActivity);

            var sequence = SetupSequence();
            var activityFunc = new ActivityFunc<string, bool> { Handler = sequence };
            forEachActivity.DataFunc = activityFunc;

            TestStartNode = new FlowStep
            {
                Action = forEachActivity
            };

            PerformExecution();

            List<IDebugState> debugStates;
            ScenarioContext.Current.TryGetValue("DebugStates", out debugStates);

            if(debugStates.Count > 0)
            {
                var sequenceDebugState = debugStates.Where(state => state.DisplayName == sequence.DisplayName);
                var debugStateOfSequence = sequenceDebugState as IDebugState[] ?? sequenceDebugState.ToArray();
                Assert.IsTrue(debugStateOfSequence.Any());
                Assert.IsTrue(debugStateOfSequence.All(state => state.ParentID.ToString() == forEachActivity.UniqueID));
            }

            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);
            activityList.Add(activityName, forEachActivity);
        }


        [Then(@"the ""(.*)"" debug inputs as")]
        public void ThenDebugInputsAs(string toolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);
            DsfSequenceActivity sequence;
            ScenarioContext.Current.TryGetValue("activity", out sequence);
            var sequenceActivity = sequence.Activities.ToList().FirstOrDefault(activity => activity.DisplayName == toolName) ?? activityList[toolName];
            var actualDebugItems = GetDebugInputItemResults(sequenceActivity);
            CommonSteps commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, actualDebugItems);
        }

        [Then(@"the ""(.*)"" debug outputs as")]
        public void ThenDebugOutputsAs(string toolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);
            DsfSequenceActivity sequence;
            ScenarioContext.Current.TryGetValue("activity", out sequence);
            var sequenceActivity = sequence.Activities.ToList().FirstOrDefault(activity => activity.DisplayName == toolName) ?? activityList[toolName];
            var actualDebugItems = GetDebugOutputItemResults(sequenceActivity);
            CommonSteps commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, actualDebugItems);
        }
    }

    internal class TestDebugWriter : IDebugWriter
    {
        readonly List<IDebugState> _debugStates;
        public List<IDebugState> DebugStates
        {
            get
            {
                return _debugStates;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TestDebugWriter()
        {
            _debugStates = new List<IDebugState>();
        }

        #region Implementation of IDebugWriter

        /// <summary>
        /// Writes the given state.
        /// <remarks>
        /// This must implement the one-way (fire and forget) message exchange pattern.
        /// </remarks>
        /// </summary>
        /// <param name="debugState">The state to be written.</param>
        public void Write(IDebugState debugState)
        {
            DebugStates.Add(debugState);
        }

        #endregion
    }
}
