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
        private void BuildDataList()
        {
            BuildShapeAndTestData();

            var systemInformationCollection =
                ScenarioContext.Current.Get<List<GatherSystemInformationTO>>("systemInformationCollection");

            var dsfGatherSystemInformationActivity = new DsfGatherSystemInformationActivity
                {
                    SystemInformationCollection = systemInformationCollection
                };

            TestStartNode = new FlowStep
                {
                    Action = dsfGatherSystemInformationActivity
                };
        }

        [Given(@"I have a variable ""(.*)"" and I selected ""(.*)""")]
        public void GivenIHaveAVariableAndISelected(string variable, string informationType)
        {
            int row;

            bool isRowAdded = ScenarioContext.Current.TryGetValue("row", out row);
            if (isRowAdded)
            {
                ScenarioContext.Current.Add("row", row);
            }
            row++;

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, string.Empty));
            var type =
                (enTypeOfSystemInformationToGather)
                Enum.Parse(typeof (enTypeOfSystemInformationToGather), informationType);
            
            List<GatherSystemInformationTO> systemInformationCollection;
            ScenarioContext.Current.TryGetValue("systemInformationCollection", out systemInformationCollection);

            if (systemInformationCollection == null)
            {
                systemInformationCollection = new List<GatherSystemInformationTO>();
                ScenarioContext.Current.Add("systemInformationCollection", systemInformationCollection);
            }
            systemInformationCollection.Add(new GatherSystemInformationTO(type, variable, row));
        }

        [When(@"the gather system infomartion tool is executed")]
        public void WhenTheGatherSystemInfomartionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the value of the variable ""(.*)"" is a valid ""(.*)""")]
        public void ThenTheValueOfTheVariableIsAValid(string variable, string type)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                foreach (string recordSetValue in recordSetValues)
                {
                    Verify(type, recordSetValue, error);
                }
            }
            else
            {
                string actualValue;
                GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
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
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError,
                                           actual ? "did not occur" : "did occur with the following :-" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}