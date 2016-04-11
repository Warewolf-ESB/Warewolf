using ActivityUnitTests;
using Dev2.Data.Util;
using NUnit.Framework;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.AssignObject
{
    [Binding]
    public class AssignObjectSteps : BaseActivityUnitTest
    {
        [Given(@"I assign the value ""(.*)"" to a json object ""(.*)""")]
        public void GivenIAssignTheValueToAJsonObject(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            List<ActivityDTO> fieldCollection;
            ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<ActivityDTO>();
                ScenarioContext.Current.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new ActivityDTO(variable, value, 1, true));
        }

        [When(@"the assign object tool is executed")]
        public void WhenTheAssignObjectToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the json object ""(.*)"" equals ""(.*)""")]
        public void ThenTheJsonObjectEquals(string variable, string value)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            //if (DataListUtil.IsValueRecordset(variable))
            //{
            var recordSetValues = result.Environment.EvalAsListOfStrings(variable, 0);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
            value = value.Replace('"', ' ').Trim();

            if (string.IsNullOrEmpty(value))
            {
                Assert.IsTrue(recordSetValues.Count == 0);
            }
            else
            {
                Assert.AreEqual(recordSetValues[0], value);
            }
            //}
            //else
            //{
            //    string actualValue;
            //    value = value.Replace('"', ' ').Trim();
            //    string error;
            //    GetScalarValueFromEnvironment(result.Environment, variable,
            //                               out actualValue, out error);
            //    actualValue = actualValue.Replace('"', ' ').Trim();
            //    Assert.AreEqual(value, actualValue);
            //}
        }

        protected void BuildDataList()
        {
            BuildShapeAndTestData();

            List<ActivityDTO> fieldCollection;
            ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

            var multiAssign = new DsfMultiAssignObjectActivity();

            TestStartNode = new FlowStep
            {
                Action = multiAssign
            };

            foreach (var field in fieldCollection)
            {
                multiAssign.FieldsCollection.Add(field);
            }
            ScenarioContext.Current.Add("activity", multiAssign);
        }

        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");

            // ReSharper disable NotAccessedVariable
            int row = 0;
            dynamic variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList != null)
            {
                foreach (dynamic variable in variableList)
                {
                    if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                    {
                        string value = variable.Item2 == "blank" ? "" : variable.Item2;
                        if (value.ToUpper() == "NULL")
                        {
                            DataObject.Environment.AssignDataShape(variable.Item1);
                        }
                        else
                        {
                            DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), value, 0);
                        }
                    }
                    row++;
                }
            }

            List<Tuple<string, string>> emptyRecordset;
            bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
            if (isAdded)
            {
                foreach (Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(emptyRecord.Item1), emptyRecord.Item2, 0);
                }
            }

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
    }
}