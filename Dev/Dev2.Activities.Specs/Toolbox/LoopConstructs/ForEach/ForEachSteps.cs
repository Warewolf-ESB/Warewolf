using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Enums;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach
{
    [Binding]
    public class ForEachSteps : RecordSetBases
    {
        public ForEachSteps()
            : base(new List<Tuple<string, string>>())
        {
            
        }

        private DsfForEachActivity _dsfForEach;
        private enForEachType _foreachType;
        private string _recordSet;
        private string r = "[[r().v]]";

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(r, ""));

            var activity = new DsfRandomActivity
                {
                    Result =  r,
                    RandomType = enRandomType.Numbers,
                    From = "0",
                    To = "100"
                };

            var activityFunction = new ActivityFunc<string, bool>{Handler  = activity};

            _dsfForEach = new DsfForEachActivity
                {
                    ForEachType = _foreachType,
                    Recordset = _recordSet,
                    DataFunc = activityFunction
                };

            TestStartNode = new FlowStep
            {
                Action = _dsfForEach
            };

           }

        [Given(@"I there is a recordset in the datalist with this shape")]
        public void GivenIThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            var rows = table.Rows.ToList();
            foreach (TableRow tableRow in rows)
            {
                _variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }
        
        [Given(@"I have selected the foreach type as ""(.*)"" and used ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAndUsed(string foreachType, string recordSet)
        {            
            _foreachType = (enForEachType) Enum.Parse(typeof (enForEachType), foreachType);
            _recordSet = recordSet;
        }
        
        [When(@"the foreach tool is executed")]
        public void WhenTheForeachToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
       
        [Then(@"the foreach will loop over (.*) records")]
        public void ThenTheForeachWillLoopOverRecords(int numOfIterations)
        {
            string error;
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, r);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, r);
            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
            Assert.AreEqual(numOfIterations, recordSetValues.Count);
        }

    }
}
