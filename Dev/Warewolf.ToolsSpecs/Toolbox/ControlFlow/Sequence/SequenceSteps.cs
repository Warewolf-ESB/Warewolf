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
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Sequence
{
    [Binding]
    public class SequenceSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;
        private readonly CommonSteps _commonSteps;

        public SequenceSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(this.scenarioContext);
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [Given(@"I have a Sequence ""(.*)""")]
        public void GivenIHaveASequence(string sequenceName)
        {
            var dsfSequence = new DsfSequenceActivity { DisplayName = sequenceName };
            scenarioContext.Add("activityList", new Dictionary<string, Activity>());
            scenarioContext.Add("activity", dsfSequence);
            _commonSteps.AddActivityToActivityList("", sequenceName, dsfSequence);
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
                scenarioContext.TryGetValue("fieldCollection", out fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
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


                _commonSteps.AddVariableToVariableList(result);

                activity.Result = result;
                activity.ResultFields = returnFields;
                activity.InFields = inFields;
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            _commonSteps.AddVariableToVariableList(resultVariable);

            DsfCalculateActivity calculateActivity = new DsfCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, calculateActivity);

        }

        [Given(@"""(.*)"" contains Aggregate Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenContainsAggregateCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            _commonSteps.AddVariableToVariableList(resultVariable);

            DsfAggregateCalculateActivity aggCalculateActivity = new DsfAggregateCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, aggCalculateActivity);
        }

        [Given(@"""(.*)"" contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenCountOnInto(string parentName, string activityName, string recordSet, string result)
        {
            _commonSteps.AddVariableToVariableList(result);

            DsfCountRecordsetNullHandlerActivity countRecordsetNullHandlerActivity = new DsfCountRecordsetNullHandlerActivity { CountNumber = result, RecordsetName = recordSet, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, countRecordsetNullHandlerActivity);
        }

        [Given(@"""(.*)"" contains Delete ""(.*)"" as")]
        public void GivenItContainsDeleteAs(string parentName, string activityName, Table table)
        {
            DsfDeleteRecordNullHandlerActivity nullHandlerActivity = new DsfDeleteRecordNullHandlerActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var result = tableRow["result"];
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(result);
                nullHandlerActivity.RecordsetName = variable;
                nullHandlerActivity.Result = result;
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, nullHandlerActivity);
        }

        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" search ""(.*)"" and result ""(.*)"" as")]
        public void GivenItContainsFindRecordIndexSearchAndResultAs(string parentName, string activityName, string recsetToSearch, string resultVariable, Table table)
        {
            var result = resultVariable;
            var recset = recsetToSearch;
            _commonSteps.AddVariableToVariableList(result);
            DsfFindRecordsMultipleCriteriaActivity activity = new DsfFindRecordsMultipleCriteriaActivity { RequireAllFieldsToMatch = false, RequireAllTrue = false, Result = result, FieldsToSearch = recset, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var matchType = tableRow["Match Type"];
                var matchValue = tableRow["Match"];

                activity.ResultsCollection.Add(new FindRecordsTO(matchValue, matchType, 1, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenItContainsGatherSystemInfoAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(variable);

                enTypeOfSystemInformationToGather systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);

                activity.RandomType = type;
                activity.To = to;
                activity.From = from;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);

                activity.Expression = number;
                activity.RoundingType = roundingType;
                activity.RoundingDecimalPlaces = roundTo;
                activity.DecimalPlacesToShow = decimalPlacesToShow;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);

                activity.DateTime = input1;
                activity.InputFormat = inputFormat;
                activity.OutputFormat = outputFormat;
                activity.TimeModifierAmountDisplay = timeModifierAmount;
                activity.TimeModifierType = "Years";
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);

                activity.Input1 = input1;
                activity.Input2 = input2;
                activity.InputFormat = inputFormat;
                activity.OutputType = output;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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
                _commonSteps.AddVariableToVariableList(variable);
                if(!string.IsNullOrEmpty(valueToSplit))
                {
                    activity.SourceString = valueToSplit;
                }
                _commonSteps.AddVariableToVariableList(variable);
                activity.ResultsCollection.Add(new DataSplitDTO(variable, type, at, 1, include, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
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
            scenarioContext.TryGetValue("errorVariable", out errorVariable);

            string webserviceToCall;
            scenarioContext.TryGetValue("webserviceToCall", out webserviceToCall);

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
            Thread.Sleep(2000);
            var states = testDebugWriter.DebugStates;
            var debugStates = states.Where(a=>a.StateType!=StateType.Duration).ToList();
            var duration = states.Where(a => a.StateType == StateType.Duration).ToList();
            scenarioContext.Add("duration", duration);
            scenarioContext.Add("result", result);
            CheckDebugStates(debugStates);
            try
            {
                DebugDispatcher.Instance.Remove(Guid.Empty);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //May already been removed
            }
        }

        void CheckDebugStates(IEnumerable<IDebugState> debugStates)
        {
            DsfSequenceActivity sequence;
            scenarioContext.TryGetValue("activity", out sequence);
            if(sequence != null)
            {
                var innerActivitiesDebugStates = debugStates.Where(state => state.ParentID.ToString() == sequence.UniqueID.ToString());
                var count = innerActivitiesDebugStates.Count();
                Assert.IsTrue(count > 0);
            }

            scenarioContext.Add("DebugStates", debugStates);
        }

        DsfSequenceActivity SetupSequence()
        {
            DsfSequenceActivity sequence;
            scenarioContext.TryGetValue("activity", out sequence);
            return sequence;
        }

        [Then(@"the Sequence Has a Duration")]
        public void ThenTheSequenceHasADuration()
        {
           var  dur =  scenarioContext.Get<IEnumerable<IDebugState>>("duration");
            Assert.IsNotNull(dur);
            Assert.IsTrue(dur.Count()==1);
            Assert.IsTrue(dur.First().EndTime.Subtract(DateTime.Now).Ticks < 10000);
        }

        [Given(@"I have a ForEach ""(.*)"" as ""(.*)"" executions ""(.*)""")]
        public void GivenIHaveAForEachAsExecutions(string activityName, string forEachType, string numberOfExecutions)
        {
            DsfForEachActivity forEachActivity = new DsfForEachActivity { DisplayName = activityName };
            enForEachType typeOfForEach;
            Enum.TryParse(forEachType, out typeOfForEach);

            forEachActivity.ForEachType = typeOfForEach;
            forEachActivity.NumOfExections = numberOfExecutions;

            scenarioContext.Add(activityName, forEachActivity);
        }

        [When(@"the ForEach ""(.*)"" tool is executed")]
        public void WhenTheToolIsExecuted(string activityName)
        {
            BuildDataList();

            DsfForEachActivity forEachActivity;
            scenarioContext.TryGetValue(activityName, out forEachActivity);

            var sequence = SetupSequence();
            var activityFunc = new ActivityFunc<string, bool> { Handler = sequence };
            forEachActivity.DataFunc = activityFunc;

            TestStartNode = new FlowStep
            {
                Action = forEachActivity
            };

            PerformExecution();

            List<IDebugState> debugStates;
            scenarioContext.TryGetValue("DebugStates", out debugStates);

            if(debugStates.Count > 0)
            {
                var sequenceDebugState = debugStates.Where(state => state.DisplayName == sequence.DisplayName);
                var debugStateOfSequence = sequenceDebugState as IDebugState[] ?? sequenceDebugState.ToArray();
                Assert.IsTrue(debugStateOfSequence.Any());
                Assert.IsTrue(debugStateOfSequence.All(state => state.ParentID.ToString() == forEachActivity.UniqueID));
            }

            Dictionary<string, Activity> activityList;
            scenarioContext.TryGetValue("activityList", out activityList);
            activityList.Add(activityName, forEachActivity);
        }


        [Then(@"the ""(.*)"" debug inputs as")]
        public void ThenDebugInputsAs(string toolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            scenarioContext.TryGetValue("activityList", out activityList);
            DsfSequenceActivity sequence;
            scenarioContext.TryGetValue("activity", out sequence);
            var sequenceActivity = sequence.Activities.ToList().FirstOrDefault(activity => activity.DisplayName == toolName) ?? activityList[toolName];
            var actualDebugItems = GetDebugInputItemResults(sequenceActivity);
            _commonSteps.ThenTheDebugInputsAs(table, actualDebugItems);
        }

        [Then(@"the ""(.*)"" debug outputs as")]
        public void ThenDebugOutputsAs(string toolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            scenarioContext.TryGetValue("activityList", out activityList);
            DsfSequenceActivity sequence;
            scenarioContext.TryGetValue("activity", out sequence);
            var sequenceActivity = sequence.Activities.ToList().FirstOrDefault(activity => activity.DisplayName == toolName) ?? activityList[toolName];
            var actualDebugItems = GetDebugOutputItemResults(sequenceActivity);
            _commonSteps.ThenTheDebugOutputAs(table, actualDebugItems);
        }
    }

    internal class TestDebugWriter : IDebugWriter
    {
        public List<IDebugState> DebugStates
        {
            get
            {
                return DebugMessageRepo.Instance.FetchDebugItems(Guid.Empty, Guid.Empty).ToList();
            }
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

        public void Write(string serializeObject)
        {
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var debugState = dev2JsonSerializer.Deserialize<DebugState>(serializeObject);
            Write(debugState);
        }

        
        #endregion
    }
}
