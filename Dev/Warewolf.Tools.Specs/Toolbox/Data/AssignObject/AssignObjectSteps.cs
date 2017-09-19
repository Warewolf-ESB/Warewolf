using ActivityUnitTests;
using Dev2.Data.Util;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.AssignObject
{
    [Binding]
    public class AssignObjectSteps : BaseActivityUnitTest
    {
        private readonly ScenarioContext scenarioContext;

        public AssignObjectSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        [Given(@"I assign the string ""(.*)"" to a json object ""(.*)""")]
        public void GivenIAssignTheStringAToAJsonObject(string value, string variable)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<AssignObjectDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new AssignObjectDTO(variable, value, 1, true));
        }

        [Given(@"I assign the value ""(.*)"" to a json object ""(.*)""")]
        public void GivenIAssignTheValueToAJsonObject(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<AssignObjectDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new AssignObjectDTO(variable, value, 1, true));
        }

        [Given(@"I assign the value (.*) to a json object ""(.*)""")]
        public void GivenIAssignTheValueToAJsonObject(int value, string variable)
        {
            GivenIAssignTheValueToAJsonObject(value.ToString(), variable);
        }

        [When(@"the assign object tool is executed")]
        public void WhenTheAssignObjectToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the json object ""(.*)"" equals ""(.*)""")]
        public void ThenTheJsonObjectEquals(string variable, string value)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
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
        }

        [Then(@"the variable ""(.*)"" equals ""(.*)""")]
        public void ThenTheVariableEquals(string variable, string value)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(variable))
            {
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
            }
            else
            {
                value = value.Replace('"', ' ').Trim();
                GetScalarValueFromEnvironment(result.Environment, variable,
                                           out string actualValue, out string error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
        }

        [Given(@"I assign the json object ""(.*)"" to a json object ""(.*)""")]
        public void GivenIAssignTheJsonObjectToAJsonObject(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<AssignObjectDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new AssignObjectDTO(variable, value, 1, true));
        }

        [Given(@"I assign a json object value ""(.*)"" to a variable ""(.*)""")]
        public void GivenIAssignAJsonObjectValueToAVariable(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<AssignObjectDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new AssignObjectDTO(variable, value, 1, true));
        }

        [Given(@"I assign a json object value ""(.*)"" to a json object ""(.*)""")]
        public void GivenIAssignAJsonObjectValueToAJsonObject(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<AssignObjectDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new AssignObjectDTO(variable, value, 1, true));
        }

        protected void BuildDataList()
        {
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("fieldCollection", out List<AssignObjectDTO> fieldCollection);

            var multiAssign = new DsfMultiAssignObjectActivity();

            TestStartNode = new FlowStep
            {
                Action = multiAssign
            };

            foreach (var field in fieldCollection)
            {
                multiAssign.FieldsCollection.Add(field);
            }
            scenarioContext.Add("activity", multiAssign);
        }

        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");

            
            int row = 0;
            scenarioContext.TryGetValue("variableList", out dynamic variableList);

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

            bool isAdded = scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
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