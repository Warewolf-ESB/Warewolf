/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Utility.GatherSystemInformation
{
    [Binding]
    public class GatherSystemInformationSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public GatherSystemInformationSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var systemInformationCollection =
                scenarioContext.Get<List<GatherSystemInformationTO>>("systemInformationCollection");

            var dsfGatherSystemInformationActivity = new DsfGatherSystemInformationActivity
                {
                    SystemInformationCollection = systemInformationCollection
                };

            TestStartNode = new FlowStep
                {
                    Action = dsfGatherSystemInformationActivity
                };
            scenarioContext.Add("activity", dsfGatherSystemInformationActivity);
        }

        [Given(@"I have a variable ""(.*)"" and I selected ""(.*)""")]
        public void GivenIHaveAVariableAndISelected(string variable, string informationType)
        {
            int row;

            bool isRowAdded = scenarioContext.TryGetValue("row", out row);
            if(isRowAdded)
            {
                scenarioContext.Add("row", row);
            }
            row++;

            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, string.Empty));
            var type =
                (enTypeOfSystemInformationToGather)
                Enum.Parse(typeof(enTypeOfSystemInformationToGather), informationType);

            List<GatherSystemInformationTO> systemInformationCollection;
            scenarioContext.TryGetValue("systemInformationCollection", out systemInformationCollection);

            if(systemInformationCollection == null)
            {
                systemInformationCollection = new List<GatherSystemInformationTO>();
                scenarioContext.Add("systemInformationCollection", systemInformationCollection);
            }
            systemInformationCollection.Add(new GatherSystemInformationTO(type, variable, row));
        }

        [When(@"the gather system infomartion tool is executed")]
        public void WhenTheGatherSystemInfomartionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the value of the variable ""(.*)"" is a valid ""(.*)""")]
        public void ThenTheValueOfTheVariableIsAValid(string variable, string type)
        {
            string error;
            var result = scenarioContext.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
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
                GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                Verify(type, actualValue, error);
            }
        }

        void Verify(string type, string actualValue, string error)
        {
            if(type == "DateTime")
            {
                Assert.IsTrue(actualValue.Contains("."));
            }
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
