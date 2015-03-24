
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(variable))
            {
                var recordSetValues =CurrentExecutionEnvironment.EvalAsListOfStrings(variable);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                value = value.Replace('"', ' ').Trim();

                if(string.IsNullOrEmpty(value))
                {
                    Assert.IsTrue(recordSetValues.Count == 0);
                }
                else
                {
                    Assert.AreEqual(recordSetValues[0], value);
                }
            }
            else
            {
                string actualValue;
                value = value.Replace('"', ' ').Trim();
                GetScalarValueFromEnvironment(variable,
                                           out actualValue, out error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
        }
    }
}
