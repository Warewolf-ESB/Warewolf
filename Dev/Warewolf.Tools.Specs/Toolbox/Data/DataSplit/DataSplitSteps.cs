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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Tools.Specs.BaseTypes;
using Warewolf.UnitTestAttributes;

namespace Dev2.Activities.Specs.Toolbox.Data.DataSplit
{
    [Binding]
    public class DataSplitSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;
        public static Depends _containerOps;

        public DataSplitSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("stringToSplit", out string stringToSplit);

            scenarioContext.TryGetValue("splitCollection", out List<DataSplitDTO> splitCollection);

            var dataSplit = new DsfDataSplitActivity { SourceString = stringToSplit };


            var row = 1;
            foreach (var dto in splitCollection)
            {
                dto.IndexNumber = row;
                dataSplit.ResultsCollection.Add(dto);
                row++;
            }
            scenarioContext.TryGetValue("ReverseOrder", out bool reverseOrder);
            scenarioContext.TryGetValue("SkipBlankRows", out bool skipBlankRows);
            dataSplit.ReverseOrder = reverseOrder;
            dataSplit.SkipBlankRows = skipBlankRows;
            TestStartNode = new FlowStep
                {
                    Action = dataSplit
                };

            scenarioContext.TryGetValue("errorVariable", out string errorVariable);

            scenarioContext.TryGetValue("webserviceToCall", out string webserviceToCall);

            dataSplit.OnErrorVariable = errorVariable;
            dataSplit.OnErrorWorkflow = webserviceToCall;
          
            scenarioContext.Add("activity", dataSplit);
        }

        [Given(@"A file ""(.*)"" to split")]
        public void GivenAFileToSplit(string fileName)
        {
            var resourceName = string.Format("Warewolf.Tools.Specs.Toolbox.Data.DataSplit.{0}",
                                                fileName);
            var stringToSplit = ReadFile(resourceName);
            scenarioContext.Add("stringToSplit", stringToSplit.Replace("\n","\r\n"));
        }

        [Given(@"Skip Blanks rows is ""(.*)""")]
        public void GivenSkipBlanksRowsIs(string enabled)
        {
            var skipBlankRows = enabled.ToLower() == "enabled";
            scenarioContext.Add("SkipBlankRows", skipBlankRows);
        }


        [Given(@"A string to split with value ""(.*)""")]
        public void GivenAStringToSplitWithValue(string stringToSplit)
        {
            scenarioContext.Add("stringToSplit", stringToSplit);
        }

        [Given(@"A string to split with new line value")]
        public void GivenAStringToSplitWithNewLineValue()
        {
            var stringToSplit = "a" + Environment.NewLine + "2ff";

            scenarioContext.Add("stringToSplit", stringToSplit);
        }


        [Given(@"the direction is ""(.*)""")]
        public void GivenTheDirectionIs(string direction)
        {
            if(!String.IsNullOrEmpty(direction) && direction.ToLower() == "backward")
            {
                scenarioContext.Add("ReverseOrder", true);
            }
            else
            {
                scenarioContext.Add("ReverseOrder", false);
            }
        }
        
        void AddVariables(string variable, string splitType, string splitAt, bool include = false, string escape = "")
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            scenarioContext.TryGetValue("splitCollection", out List<DataSplitDTO> splitCollection);

            if (splitCollection == null)
            {
                splitCollection = new List<DataSplitDTO>();
                scenarioContext.Add("splitCollection", splitCollection);
            }
            var dto = new DataSplitDTO { OutputVariable = variable, SplitType = splitType, At = splitAt, EscapeChar = escape, Include = include };
            splitCollection.Add(dto);
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)"" and Include ""(.*)"" without escaping")]
        public void GivenAssignToVariableSplitTypeAtAndInclude(string variable, string splitType, string splitAt, string include)
        {
            var included = include.ToLower() == "selected";
            AddVariables(variable, splitType, splitAt, included);
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)"" and Include ""(.*)"" and Escape ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAtAndIncludeAndEscape(string variable, string splitType, string splitAt, string include, string escape)
        {
            var included = include.ToLower() == "selected";
            AddVariables(variable, splitType, splitAt, included, escape);
        }

        [Given(@"I have a variable ""(.*)"" with a value ""(.*)""")]
        public void GivenIHaveAVariableWithAValue(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value.ToString(CultureInfo.InvariantCulture)));
        }
        
         [Then(@"the split recordset ""(.*)"" will be")]
         public void ThenTheSplitRecordsetWillBe(string variable, Table table)
         {
             var result = scenarioContext.Get<IDSFDataObject>("result");
             var tableRows = table.Rows.ToList();
            var recordSets = result.Environment.Eval(variable, 0);
             if (recordSets.IsWarewolfAtomListresult)
             {
                 
                 var recordSetValues = (recordSets as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item.ToList();
                 
                 Assert.AreEqual<int>(tableRows.Count, recordSetValues.Count);

                 for (int i = 0; i < tableRows.Count; i++)
                 {
                     Assert.AreEqual<string>(tableRows[i][1], ExecutionEnvironment.WarewolfAtomToString(recordSetValues[i]).Trim());
                 }
             }

         }


        [When(@"the data split tool is executed")]
        public void WhenTheDataSplitToolIsExecuted()
        {
            BuildDataList();
            var esbChannel = new EsbServicesEndpoint();
            var result = ExecuteProcess(isDebug: true,channel:esbChannel, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the split result will be")]
        public void ThenTheSplitResultWillBe(Table table)
        {
            var tableRows = table.Rows.ToList();

            var recordset = scenarioContext.Get<string>("recordset");
            var field = scenarioContext.Get<string>("recordField");

            var result = scenarioContext.Get<IDSFDataObject>("result");
            var recordSetValues = Enumerable.ToList<string>(RetrieveAllRecordSetFieldValues(result.Environment, recordset, field, out string error));

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
            value = value.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(variable),
                                       out string actualValue, out string error);
            if (!string.IsNullOrEmpty(actualValue))
            {
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
            if (string.IsNullOrEmpty(value))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
        }

        [Given(@"remote server container has started")]
        public void GivenRemoteServerContainerHasStarted() => _containerOps = new Depends(Depends.ContainerType.CIRemote);

        [AfterScenario]
        public void CleanUp() => _containerOps?.Dispose();
    }
}
