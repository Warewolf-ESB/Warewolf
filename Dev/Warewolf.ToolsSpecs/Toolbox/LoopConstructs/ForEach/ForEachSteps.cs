
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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach
{
    [Binding]
    public class ForEachSteps : RecordSetBases
    {
        private const string ResultRecordsetVariable = "[[r().v]]";

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultRecordsetVariable, ""));

            string outMapTo;
            if(ScenarioContext.Current.TryGetValue("outMapTo", out outMapTo))
            {
                variableList.Add(new Tuple<string, string>(outMapTo, ""));
            }

            BuildShapeAndTestData();

            var activityType = ScenarioContext.Current.Get<string>("activityType");

            dynamic activity;

            if(activityType.Equals("Tool"))
            {
                activity = new DsfRandomActivity
                    {
                        Result = ResultRecordsetVariable,
                        RandomType = enRandomType.Numbers,
                        From = "0",
                        To = "100"
                    };
            }
            else
            {
                activity = new DsfActivity
                    {
                        InputMapping = BuildInputMappings(),
                        OutputMapping = BuildOutputMappings(),
                        ServiceName = "SpecflowForeachActivityTest"
                    };
            }

            var activityFunction = new ActivityFunc<string, bool> { Handler = activity };
            var foreachType = ScenarioContext.Current.Get<enForEachType>("foreachType");

            string recordSet;
            if(!ScenarioContext.Current.TryGetValue("recordset", out recordSet))
            {
                recordSet = string.Empty;
            }

            string from;
            if(!ScenarioContext.Current.TryGetValue("from", out from))
            {
                from = string.Empty;
            }

            string to;
            if(!ScenarioContext.Current.TryGetValue("to", out to))
            {
                to = string.Empty;
            }

            string numberAs;
            if(!ScenarioContext.Current.TryGetValue("numberAs", out numberAs))
            {
                numberAs = string.Empty;
            }

            var dsfForEach = new DsfForEachActivity
                {
                    ForEachType = foreachType,
                    Recordset = recordSet,
                    From = from,
                    To = to,
                    CsvIndexes = numberAs,
                    NumOfExections = numberAs,
                    DataFunc = activityFunction
                };

            TestStartNode = new FlowStep
                {
                    Action = dsfForEach
                };

            ScenarioContext.Current.Add("activity", dsfForEach);
        }

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            var inMapTo = ScenarioContext.Current.Get<string>("inMapTo");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, inMapTo);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, inMapTo);

            var inMapFrom = ScenarioContext.Current.Get<string>("inMapFrom");
            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            var outMapFrom = ScenarioContext.Current.Get<string>("outMapFrom");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, outMapFrom);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, outMapFrom);

            var outMapTo = ScenarioContext.Current.Get<string>("outMapTo");
            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                outMapTo, inRecordset));

            outputMappings.Append("</Outputs>");
            return outputMappings.ToString();
        }

        [Given(@"There is a recordset in the datalist with this shape")]
        public void GivenThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if(rows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow tableRow in rows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Given(@"I have a variable ""(.*)"" with the value ""(.*)""")]
        public void GivenIHaveAVariableWithTheValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            if (!string.IsNullOrEmpty(variable))
            {
                variableList.Add(new Tuple<string, string>(variable, value));
            }
        }

        [Given(@"I have selected the foreach type as ""(.*)"" and used ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAndUsed(string foreachType, string recordSet)
        {
            var forEachType = (enForEachType)Enum.Parse(typeof(enForEachType), foreachType);
            ScenarioContext.Current.Add("foreachType", forEachType);
            switch(forEachType)
            {
                case enForEachType.NumOfExecution:
                    ScenarioContext.Current.Add("numberAs", recordSet);
                    break;
                case enForEachType.InRange:
                    ScenarioContext.Current.Add("from", recordSet);
                    ScenarioContext.Current.Add("to", recordSet);
                    break;
                case enForEachType.InCSV:
                    ScenarioContext.Current.Add("numberAs", recordSet);
                    break;
                case enForEachType.InRecordset:
                    ScenarioContext.Current.Add("recordset", recordSet);
                    break;
            }
        }

        [Given(@"I have selected the foreach type as ""(.*)"" from (.*) to (.*)")]
        public void GivenIHaveSelectedTheForeachTypeAsFromTo(string foreachType, string from, string to)
        {
            ScenarioContext.Current.Add("foreachType", (enForEachType)Enum.Parse(typeof(enForEachType), foreachType));
            ScenarioContext.Current.Add("from", from);
            ScenarioContext.Current.Add("to", to);
        }

        [Given(@"I have selected the foreach type as ""(.*)"" as ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAs(string foreachType, string numberAs)
        {
            ScenarioContext.Current.Add("foreachType", (enForEachType)Enum.Parse(typeof(enForEachType), foreachType));
            ScenarioContext.Current.Add("numberAs", numberAs);
        }

        [Given(@"the underlying dropped activity is a\(n\) ""(.*)""")]
        public void GivenTheUnderlyingDroppedActivityIsAN(string activityType)
        {
            ScenarioContext.Current.Add("activityType", activityType);
        }

        [When(@"the foreach tool is executed")]
        public void WhenTheForeachToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb());
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I Map the input recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheInputRecordsetTo(string inMapFrom, string inMapTo)
        {
            ScenarioContext.Current.Add("inMapFrom", inMapFrom);
            ScenarioContext.Current.Add("inMapTo", inMapTo);
        }

        [Given(@"I Map the output recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheOutputRecordsetTo(string outMapFrom, string outMapTo)
        {
            ScenarioContext.Current.Add("outMapFrom", outMapFrom);
            ScenarioContext.Current.Add("outMapTo", outMapTo);
        }

        [Then(@"The mapping uses the following indexes")]
        public void ThenTheMappingUsesTheFollowingIndexes(Table table)
        {

            var updateValues = ScenarioContext.Current.Get<List<int>>("indexUpdate").Select(a => a.ToString());
            foreach(var tableRow in table.Rows)
            {
              Assert.IsTrue(updateValues.Contains(tableRow[0]));
              
            }
        }

        [Then(@"the foreach executes (.*) times")]
        public void ThenTheForeachExecutesTimes(int numOfIterations)
        {
            string error;
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, ResultRecordsetVariable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, ResultRecordsetVariable);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            var recordSetValues = RetrieveAllRecordSetFieldValues(DataObject.Environment, recordset, column, out error);
            recordSetValues = Enumerable.Where<string>(recordSetValues, i => !string.IsNullOrEmpty(i)).ToList();
            Assert.AreEqual<int>(numOfIterations, recordSetValues.Count);
        }
    }

    public class mockEsb : IEsbChannel
    {
        #region Not Implemented

        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceID,
                                   out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return Guid.NewGuid();
        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public string FindServiceShape(Guid workspaceID, string serviceName, int update)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceID, Guid resourceID)
        {
            return null;
        }

        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(
            IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors,
                                    IDataListCompiler compiler)
        {
            throw new NotImplementedException();
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceID, string uri,
                                           out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            return null;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {
        }

        #endregion

        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs,
                                      out ErrorResultTO errors, int update,bool b)
        {
            List<string> inputList;
            List<string> outputList;
            List<int> updateValues;
            if (!ScenarioContext.Current.TryGetValue("indexUpdate", out updateValues))
            {
                updateValues = new List<int>();
                ScenarioContext.Current.Add("indexUpdate", updateValues);
            }

            if(!ScenarioContext.Current.TryGetValue("inputDefs", out inputList))
            {
                inputList = new List<string>();
                ScenarioContext.Current.Add("inputDefs", inputList);
            }

            if(!ScenarioContext.Current.TryGetValue("outputDefs", out outputList))
            {
                outputList = new List<string>();
                ScenarioContext.Current.Add("outputDefs", outputList);
            }

            inputList.Add(inputDefs);
            outputList.Add(outputDefs);
            updateValues.Add(update);
            errors = new ErrorResultTO();
            return dataObject.Environment;
        }
    }
}
