
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities.Specs.Toolbox.Data.DataSplit
{
    [Binding]
    public class DataSplitSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            string stringToSplit;
            ScenarioContext.Current.TryGetValue("stringToSplit", out stringToSplit);

            List<DataSplitDTO> splitCollection;
            ScenarioContext.Current.TryGetValue("splitCollection", out splitCollection);

            var dataSplit = new DsfDataSplitActivity { SourceString = stringToSplit };


            int row = 1;
            foreach(var dto in splitCollection)
            {
                dto.IndexNumber = row;
                dataSplit.ResultsCollection.Add(dto);
                row++;
            }

            bool reverseOrder;
            bool skipBlankRows;
            ScenarioContext.Current.TryGetValue("ReverseOrder", out reverseOrder);
            ScenarioContext.Current.TryGetValue("SkipBlankRows", out skipBlankRows);
            dataSplit.ReverseOrder = reverseOrder;
            dataSplit.SkipBlankRows = skipBlankRows;
            TestStartNode = new FlowStep
                {
                    Action = dataSplit
                };

            string errorVariable;
            ScenarioContext.Current.TryGetValue("errorVariable", out errorVariable);

            string webserviceToCall;
            ScenarioContext.Current.TryGetValue("webserviceToCall", out webserviceToCall);

            dataSplit.OnErrorVariable = errorVariable;
            dataSplit.OnErrorWorkflow = webserviceToCall;
          
            ScenarioContext.Current.Add("activity", dataSplit);
        }

        [Given(@"A file ""(.*)"" to split")]
        public void GivenAFileToSplit(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Data.DataSplit.{0}",
                                                fileName);
            var stringToSplit = ReadFile(resourceName);
            ScenarioContext.Current.Add("stringToSplit", stringToSplit);
        }

        [Given(@"Skip Blanks rows is ""(.*)""")]
        public void GivenSkipBlanksRowsIs(string enabled)
        {
            var skipBlankRows = enabled.ToLower() == "enabled";
            ScenarioContext.Current.Add("SkipBlankRows", skipBlankRows);
        }


        [Given(@"A string to split with value ""(.*)""")]
        public void GivenAStringToSplitWithValue(string stringToSplit)
        {
            ScenarioContext.Current.Add("stringToSplit", stringToSplit);
        }

        [Given(@"the direction is ""(.*)""")]
        public void GivenTheDirectionIs(string direction)
        {
            if(!String.IsNullOrEmpty(direction) && direction.ToLower() == "backward")
            {
                ScenarioContext.Current.Add("ReverseOrder", true);
            }
            else
            {
                ScenarioContext.Current.Add("ReverseOrder", false);
            }
        }
        
        static void AddVariables(string variable, string splitType, string splitAt, bool include = false, string escape = "")
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
           // variableList.Add(new Tuple<string, string>(variable, ""));

            List<DataSplitDTO> splitCollection;
            ScenarioContext.Current.TryGetValue("splitCollection", out splitCollection);

            if(splitCollection == null)
            {
                splitCollection = new List<DataSplitDTO>();
                ScenarioContext.Current.Add("splitCollection", splitCollection);
            }
            DataSplitDTO dto = new DataSplitDTO { OutputVariable = variable, SplitType = splitType, At = splitAt, EscapeChar = escape, Include = include };
            splitCollection.Add(dto);
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)"" and Include ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAtAndInclude(string variable, string splitType, string splitAt, string include)
        {
            var included = include.ToLower() == "selected";
            AddVariables(variable, splitType, splitAt, included);
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)"" and Include ""(.*)"" and Escape '(.*)'")]
        [Given(@"assign to variable '(.*)' split type ""(.*)"" at '(.*)' and Include '(.*)' and Escape '(.*)'")]
        public void GivenAssignToVariableSplitTypeAtAndIncludeAndEscape(string variable, string splitType, string splitAt, string include, string escape)
        {
            var included = include.ToLower() == "selected";
            AddVariables(variable, splitType, splitAt, included, escape);

        }

         [Given(@"I have a variable ""(.*)"" with a value ""(.*)""")]
        public void GivenIHaveAVariableWithAValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value.ToString(CultureInfo.InvariantCulture)));

        }

         [Then(@"the split recordset ""(.*)"" will be")]
         public void ThenTheSplitRecordsetWillBe(string variable, Table table)
         {
             List<TableRow> tableRows = table.Rows.ToList();
             var recordSets = CurrentExecutionEnvironment.Eval(variable);
             if (recordSets.IsWarewolfAtomListresult)
             {
                 // ReSharper disable PossibleNullReferenceException
                 var recordSetValues = (recordSets as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item.ToList();
                 // ReSharper restore PossibleNullReferenceException
                 Assert.AreEqual(tableRows.Count, recordSetValues.Count);

                 for (int i = 0; i < tableRows.Count; i++)
                 {
                     Assert.AreEqual(tableRows[i][1], ExecutionEnvironment.WarewolfAtomToString(recordSetValues[i]).Trim());
                 }
             }

         }


        [When(@"the data split tool is executed")]
        public void WhenTheDataSplitToolIsExecuted()
        {
            BuildDataList();
            var esbChannel = new EsbServicesEndpoint();
            IDSFDataObject result = ExecuteProcess(isDebug: true,channel:esbChannel, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the split result will be")]
        public void ThenTheSplitResultWillBe(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;

            var recordset = ScenarioContext.Current.Get<string>("recordset");
            var field = ScenarioContext.Current.Get<string>("recordField");

            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, field, out error).ToList();

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for(int i = 0; i < tableRows.Count; i++)
            {
                var expected = tableRows[i][0];
                Assert.AreEqual(expected, recordSetValues[i]);
            }
        }

        [Then(@"the split result for ""(.*)"" will be ""(.*)""")]
        public void ThenTheSplitResultForWillBe(string variable, string value)
        {
            string actualValue;
            string error;
            value = value.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            actualValue = actualValue.Replace('"', ' ').Trim();
            Assert.AreEqual(value, actualValue);
        }
    }
}
