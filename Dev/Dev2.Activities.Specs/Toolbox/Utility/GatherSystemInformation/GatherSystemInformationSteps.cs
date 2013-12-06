using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
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
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Type component = Type.GetType("System." + type);
            TypeConverter converter = TypeDescriptor.GetConverter(component);
            object res = converter.ConvertFrom(actualValue);
            Assert.AreEqual(string.Empty, error);
        }
    }
}