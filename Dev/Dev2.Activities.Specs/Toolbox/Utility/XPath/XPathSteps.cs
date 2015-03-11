
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.XPath
{
    [Binding]
    public class XPathSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            string xmlData;
            ScenarioContext.Current.TryGetValue("xmlData", out xmlData);

            var xPath = new DsfXPathActivity
                {
                    SourceString = xmlData
                };

            TestStartNode = new FlowStep
                {
                    Action = xPath
                };

            List<Tuple<string, string>> xpathDtos;
            ScenarioContext.Current.TryGetValue("xpathDtos", out xpathDtos);

            int row = 1;
            foreach(var variable in xpathDtos)
            {
                xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
            ScenarioContext.Current.Add("activity", xPath);
        }

        [Given(@"I have this xml '(.*)'")]
        public void GivenIHaveThisXml(string xmlData)
        {
            ScenarioContext.Current.Add("xmlData", xmlData);
        }

        [Given(@"I assign the variable ""(.*)"" as xml input")]
        public void GivenIAssignTheVariableAsXmlInput(string variable)
        {
            ScenarioContext.Current.Add("xmlData", variable);
        }


        [Given(@"I have a variable ""(.*)"" output with xpath ""(.*)""")]
        public void GivenIHaveAVariableOutputWithXpath(string variable, string xpath)
        {
            List<Tuple<string, string>> xpathDtos;
            ScenarioContext.Current.TryGetValue("xpathDtos", out xpathDtos);

            if(xpathDtos == null)
            {
                xpathDtos = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("xpathDtos", xpathDtos);
            }
            xpathDtos.Add(new Tuple<string, string>(variable, xpath));

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, ""));
        }

        [Given(@"I have this xml '(.*)' in a variable ""(.*)""")]
        public void GivenIHaveThisXmlInAVariable(string xml, string variable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, xml));
        }

        [When(@"the xpath tool is executed")]
        public void WhenTheXpathToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the variable ""(.*)"" should have a value ""(.*)""")]
        public void ThenTheVariableShouldHaveAValue(string variable, string value)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            if(string.IsNullOrEmpty(value))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(value, actualValue);
            }
        }

        [Then(@"the xpath result for this varibale ""(.*)"" will be")]
        public void ThenTheXpathResultForThisVaribaleWillBe(string variable, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][0], recordSetValues[i]);
            }
        }
    }
}
