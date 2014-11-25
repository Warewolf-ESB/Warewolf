
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
using System.ComponentModel;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.GatherSystemInformation
{
    [Binding]
    public class GatherSystemInformationSteps : RecordSetBases
    {
        protected override void BuildDataList()
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
            ScenarioContext.Current.Add("activity", dsfGatherSystemInformationActivity);
        }

        [Given(@"I have a variable ""(.*)"" and I selected ""(.*)""")]
        public void GivenIHaveAVariableAndISelected(string variable, string informationType)
        {
            int row;

            bool isRowAdded = ScenarioContext.Current.TryGetValue("row", out row);
            if(isRowAdded)
            {
                ScenarioContext.Current.Add("row", row);
            }
            row++;

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, string.Empty));
            var type =
                (enTypeOfSystemInformationToGather)
                Enum.Parse(typeof(enTypeOfSystemInformationToGather), informationType);

            List<GatherSystemInformationTO> systemInformationCollection;
            ScenarioContext.Current.TryGetValue("systemInformationCollection", out systemInformationCollection);

            if(systemInformationCollection == null)
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
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the value of the variable ""(.*)"" is a valid ""(.*)""")]
        public void ThenTheValueOfTheVariableIsAValid(string variable, string type)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                foreach(string recordSetValue in recordSetValues)
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
            if(component != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(component);
                converter.ConvertFrom(actualValue);
            }
            Assert.AreEqual(string.Empty, error);
        }
    }
}
