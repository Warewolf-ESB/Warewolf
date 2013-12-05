using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Enums;
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
            : base(new List<Tuple<string, string, string, string>>())
        {
        }

        private DsfForEachActivity _dsfForEach;
        private enForEachType _foreachType;
        private string _recordSet;
        private string r = "[[r().v]]";
        private string _activity;

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string, string, string>(r, "", "", ""));

            dynamic activity;

            if (_activity.Equals("Tool"))
            {
                activity = new DsfRandomActivity
                    {
                        Result = r,
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
                    };
            }


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

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            foreach (Tuple<string, string, string, string> variable in _variableList)
            {
                if (variable.Item4.Equals("input"))
                {
                    inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>",
                                                       RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable.Item1), variable.Item1,
                                                       RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly,
                                                                                 variable.Item1)));
                }
            }

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Outputs>");

            foreach (Tuple<string, string, string, string> variable in _variableList)
            {
                if (variable.Item4.Equals("output"))
                {
                    inputMappings.Append(string.Format("<Output Name=\"{0}\"  MapsTo=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>",
                                                       RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable.Item1), variable.Item1,
                                                       RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly,
                                                                                 variable.Item1)));
                }
            }

            inputMappings.Append("</Outputs>");
            return inputMappings.ToString();
        }

        [Given(@"I there is a recordset in the datalist with this shape")]
        public void GivenIThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            var rows = table.Rows.ToList();
            foreach (TableRow tableRow in rows)
            {
                _variableList.Add(new Tuple<string, string, string, string>(tableRow[0], tableRow[1], tableRow[2], tableRow[3]));
            }
        }
        
        [Given(@"I have selected the foreach type as ""(.*)"" and used ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAndUsed(string foreachType, string recordSet)
        {            
            _foreachType = (enForEachType) Enum.Parse(typeof (enForEachType), foreachType);
            _recordSet = recordSet;
        }

        [Given(@"the underlying dropped activity is a\(n\) ""(.*)""")]
        public void GivenTheUnderlyingDroppedActivityIsAN(string activity)
        {
            _activity = activity;
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
