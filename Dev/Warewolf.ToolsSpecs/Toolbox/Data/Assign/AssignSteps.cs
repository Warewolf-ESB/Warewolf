
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Activities.Specs.Toolbox.Data.Assign
{
    [Binding]
    public class AssignSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            List<ActivityDTO> fieldCollection;
            ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

            var multiAssign = new DsfMultiAssignActivity();

            TestStartNode = new FlowStep
                {
                    Action = multiAssign
                };

            foreach(var field in fieldCollection)
            {
                multiAssign.FieldsCollection.Add(field);
            }
            ScenarioContext.Current.Add("activity", multiAssign);
        }

        [Given(@"I assign the value (.*) to a variable ""(.*)""")]
        [Given(@"I assign the value (.*) to a variable '(.*)'")]
        public void GivenIAssignTheValueToAVariable(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if(value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            List<ActivityDTO> fieldCollection;
            ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            if(fieldCollection == null)
            {
                fieldCollection = new List<ActivityDTO>();
                ScenarioContext.Current.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new ActivityDTO(variable, value, 1, true));
        }


        [Given(@"I have a variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenIHaveAVariableWithAValueOf(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }


        [Then(@"the value of ""(.*)"" is null")]
        public void ThenTheValueOfIsNull(string variable)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            try
            {
                if (DataListUtil.IsValueRecordset(variable))
                {
                    result.Environment.EvalAsListOfStrings(variable, 0);
                }
                else
                {
                    string actualValue;
                    string error;
                    GetScalarValueFromEnvironment(result.Environment, variable,
                        out actualValue, out error);
                    

                }
                Assert.Fail("Should have thrown NullReferenceException");
            }
            catch (NullReferenceException)
            {
                Assert.IsTrue(true, "Exception thrown");
            }
        }


        [When(@"the assign tool is executed")]
        public void WhenTheAssignToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the value of ""(.*)"" equals (.*)")]
        public void ThenTheValueOfEquals(string variable, string value)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(variable))
            {
                var recordSetValues = result.Environment.EvalAsListOfStrings(variable, 0);
                recordSetValues = Enumerable.Where<string>(recordSetValues, i => !string.IsNullOrEmpty(i)).ToList();
                value = value.Replace('"', ' ').Trim();

                if(string.IsNullOrEmpty(value))
                {
                    Assert.IsTrue(recordSetValues.Count == 0);
                }
                else
                {
                    Assert.AreEqual<string>(recordSetValues[0], value);
                }
            }
            else
            {
                string actualValue;
                value = value.Replace('"', ' ').Trim();
                string error;
                GetScalarValueFromEnvironment(result.Environment,variable,
                                           out actualValue, out error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
        }
    }
}
