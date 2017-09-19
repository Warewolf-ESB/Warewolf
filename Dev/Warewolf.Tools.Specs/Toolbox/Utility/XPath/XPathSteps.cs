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
using System.Linq;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Utility.XPath
{
    [Binding]
    public class XPathSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public XPathSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("xmlData", out string xmlData);

            var xPath = new DsfXPathActivity
                {
                    SourceString = xmlData
                };

            TestStartNode = new FlowStep
                {
                    Action = xPath
                };

            scenarioContext.TryGetValue("xpathDtos", out List<Tuple<string, string>> xpathDtos);

            int row = 1;
            foreach(var variable in xpathDtos)
            {
                xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
            scenarioContext.Add("activity", xPath);
        }

        [Given(@"I have this xml ""(.*)""")]
        public void GivenIHaveThisXml(string xmlData)
        {
            scenarioContext.Add("xmlData", xmlData);
        }

        [Given(@"I assign the variable ""(.*)"" as xml input")]
        public void GivenIAssignTheVariableAsXmlInput(string variable)
        {
            scenarioContext.Add("xmlData", variable);
        }

        [Given(@"I have a variable ""(.*)"" output with xpath ""(.*)""")]
        public void GivenIHaveAVariableOutputWithXpath(string variable, string xpath)
        {
            scenarioContext.TryGetValue("xpathDtos", out List<Tuple<string, string>> xpathDtos);

            if (xpathDtos == null)
            {
                xpathDtos = new List<Tuple<string, string>>();
                scenarioContext.Add("xpathDtos", xpathDtos);
            }
            xpathDtos.Add(new Tuple<string, string>(variable, xpath));

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
        }

        [Given(@"I have this xml ""(.*)"" in a variable '(.*)'")]
        public void GivenIHaveThisXmlInAVariable(string xml, string variable)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, xml));
        }

        [When(@"the xpath tool is executed")]
        public void WhenTheXpathToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the variable ""(.*)"" should have a value ""(.*)""")]
        public void ThenTheVariableShouldHaveAValue(string variable, string value)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(variable),
                                       out string actualValue, out string error);
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

            var result = scenarioContext.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
                                                                           out string error);
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
