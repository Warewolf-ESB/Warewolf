using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities.Specs.Toolbox.Recordset.Count;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.LoopConstructs
{
    [Binding]
    public class ForEachSteps : BaseActivityUnitTest
    {
        private readonly List<string> _variableList = new List<string>();
        private const string ResultVariable = "[[result]]";
        private IDSFDataObject _result;
        private string _recordSetName;
        private DsfForEachActivity _dsfForEach;
        private enForEachType _forEachType;
        private string _from = "";
        private string _numberOfExecutes = "";
        private string _to = "";
        private string _csvIndexes;
        private CountSteps countSteps = new CountSteps();

        private void BuildDataList()
        {
            var data = new StringBuilder();
            data.Append("<root>");


            //int row = 1;
            //foreach (var variable in _variableList)
            //{
            //    string variableName = DataListUtil.RemoveLanguageBrackets(variable);
            //    if (variableName.Contains("(") && variableName.Contains(")"))
            //    {
            //        variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
            //        var variableNameSplit = variableName.Split(".".ToCharArray());
            //        data.Append(string.Format("<{0}>", variableNameSplit[0]));
            //        data.Append(string.Format("<{0}/>", variableNameSplit[1]));
            //        data.Append(string.Format("</{0}>", variableNameSplit[0]));
            //        _recordSetName = variableNameSplit[0];
            //    }
            //    else
            //    {
            //        data.Append(string.Format("<{0}/>", variableName));
            //    }
            //    row++;
            //}


            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            _dsfForEach = new DsfForEachActivity
            {
                ForEachType = _forEachType,
                From = _from,
                To = _to,
                NumOfExections = _numberOfExecutes,
                CsvIndexes = _csvIndexes,
                DataFunc = new ActivityFunc<string, bool>(),
                
            };

            TestStartNode = new FlowStep
            {
                Action = _dsfForEach
            };

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have the foreach type as ""(.*)""")]
        public void GivenIHaveTheForeachTypeAs(string forEachType)
        {
            _forEachType = enForEachType.NumOfExecution;
        }
        
        [Given(@"I have the number of executes as ""(.*)""")]
        public void GivenIHaveTheNumberOfExecutesAs(string numberOfExecutes)
        {
            _numberOfExecutes = numberOfExecutes;
        }
        
        [When(@"the foreach tool is executed")]
        public void WhenTheForeachToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        //[Given(@"I have an ""(.*)"" tool to execute")]
        //public void GivenIHaveAnToolToExecute(Table table)
        //{
        //    countSteps.GivenIHaveARecordsetWithThisShape(table);
        //}

        [Given(@"I have the count to execute a recordset with this shape")]
        public void GivenIHaveTheCountToExecuteARecordsetWithThisShape(Table table)
        {
            countSteps.GivenIHaveARecordsetWithThisShape(table);
        }


        //[BeforeFeature("NumberOfRecordsInARecordset")]
        //public static void ExecuteExternalScenarion()
        //{

        //}

        [Then(@"the foreach result should be as follows ""(.*)""")]
        public void ThenTheForeachResultShouldBeAsFollows(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
