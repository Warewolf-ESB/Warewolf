using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.GatherSystemInformation
{
    [Binding]
    public class GatherSystemInformationSteps : RecordSetBases
    {
        private readonly List<GatherSystemInformationTO> _systemInformationCollection =
            new List<GatherSystemInformationTO>();

        private DsfGatherSystemInformationActivity _dsfGatherSystemInformationActivity;

        private int row;

        public GatherSystemInformationSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _dsfGatherSystemInformationActivity = new DsfGatherSystemInformationActivity();
            _dsfGatherSystemInformationActivity.SystemInformationCollection = _systemInformationCollection;

            TestStartNode = new FlowStep
                {
                    Action = _dsfGatherSystemInformationActivity
                };
        }

        [Given(@"I have a variable ""(.*)"" and I selected ""(.*)""")]
        public void GivenIHaveAVariableAndISelected(string variable, string informationType)
        {
            row++;
            _variableList.Add(new Tuple<string, string>(variable, string.Empty));
            var type =
                (enTypeOfSystemInformationToGather)
                Enum.Parse(typeof (enTypeOfSystemInformationToGather), informationType);
            _systemInformationCollection.Add(new GatherSystemInformationTO(type, variable, row));
        }

        [When(@"the gather system infomartion tool is executed")]
        public void WhenTheGatherSystemInfomartionToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
            row = 0;
            _systemInformationCollection.Clear();
        }

        [Then(@"the value of the variable ""(.*)"" is a valid ""(.*)""")]
        public void ThenTheValueOfTheVariableIsAValid(string variable, string type)
        {
            string error;

            if (DataListUtil.IsValueRecordset(variable))
            {
                var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
                var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                foreach (string recordSetValue in recordSetValues)
                {
                    Verify(type, recordSetValue, error);
                }
                 
            }
            else
            {
                string actualValue;
                GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                Verify(type, actualValue, error);
            }
        }

        private static void Verify(string type, string actualValue, string error)
        {
            Type component = Type.GetType("System." + type);
            TypeConverter converter = TypeDescriptor.GetConverter(component);
            object res = converter.ConvertFrom(actualValue);
            Assert.AreEqual(string.Empty, error);
        }

        [Then(@"gather system info execution has ""(.*)"" error")]
        public void ThenGatherSystemInfoExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

    }
}