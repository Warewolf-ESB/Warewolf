using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Unique
{
    [Binding]
    public class UniqueSteps : RecordSetBases
    {
        public UniqueSteps()
            : base(new List<Tuple<string, string>>())
        {
            
        }

        private string _inField;
        private string _returnField;
        private DsfUniqueActivity _unique;
        private string _resultVariable;


        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(_resultVariable, ""));
            
            _unique = new DsfUniqueActivity
            {
                InFields = _inField,
                ResultFields = _returnField,
                Result = _resultVariable
            };

            TestStartNode = new FlowStep
            {
                Action = _unique
            };
        }
        

        [Given(@"I have the following duplicated recordset")]
        public void GivenIHaveTheFollowingDuplicatedRecordset(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (var t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }
        
        [Given(@"I want to find unique in field ""(.*)"" with the return field ""(.*)""")]
        public void GivenIWantToFindUniqueInFieldWithTheReturnField(string inField, string returnField)
        {
            _inField = inField;
            _returnField = returnField;
        }

        [Given(@"The result variable is ""(.*)""")]
        public void GivenTheResultVariableIs(string resultVariable)
        {
            _resultVariable = resultVariable;
        }


        [When(@"the unique tool is executed")]
        public void WhenTheUniqueToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the unique result will be")]
        public void ThenTheUniqueResultWillBe(Table table)
        {
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, _resultVariable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, _resultVariable);

            string error;
            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }
    }
}
