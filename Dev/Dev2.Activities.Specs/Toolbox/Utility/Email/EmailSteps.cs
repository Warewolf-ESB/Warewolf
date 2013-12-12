using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.Email
{
    [Binding]
    public class EmailSteps : RecordSetBases
    {

        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string body;
            ScenarioContext.Current.TryGetValue("body", out body);
            string subject;
            ScenarioContext.Current.TryGetValue("subject", out subject);
            string fromAccount;
            ScenarioContext.Current.TryGetValue("fromAccount", out fromAccount);
            string password;
            ScenarioContext.Current.TryGetValue("password", out password);
            string simulationOutput;
            ScenarioContext.Current.TryGetValue("simulationOutput", out simulationOutput);
            string to;
            ScenarioContext.Current.TryGetValue("to", out to);


            var sendEmail = new DsfSendEmailActivity
                {
                    Result = ResultVariable,
                    Body = body,
                    Subject = subject,
                    FromAccount = fromAccount,
                    Password = password,
                    IsSimulationEnabled = true,
                    SimulationOutput = simulationOutput,
                    To = to
                };

            TestStartNode = new FlowStep
                {
                    Action = sendEmail
                };
        }

        [Given(@"I have an email address input ""(.*)""")]
        public void GivenIHaveAnEmailAddressInput(string to)
        {
            ScenarioContext.Current.Add("to", to);
        }

        [Given(@"the from account is ""(.*)""")]
        public void GivenTheFromAccountIs(string fromAccount)
        {
            ScenarioContext.Current.Add("fromAccount", fromAccount);
        }

        [Given(@"the subject is ""(.*)""")]
        public void GivenTheSubjectIs(string subject)
        {
            ScenarioContext.Current.Add("subject", subject);
        }


        [Given(@"the sever name is ""(.*)"" with password as ""(.*)""")]
        public void GivenTheSeverNameIsWithPasswordAs(string serverName, string password)
        {
            ScenarioContext.Current.Add("serverName", serverName);
            ScenarioContext.Current.Add("password", password);
        }

        [Given(@"I have an email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAnEmailVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, string.Empty));
        }

        [Given(@"body is ""(.*)""")]
        public void GivenBodyIs(string body)
        {
            ScenarioContext.Current.Add("body", body);
        }

        [Given(@"I have a variable ""(.*)"" with this email address ""(.*)""")]
        public void GivenIHaveAVariableWithThisEmailAddress(string variable, string emailAddress)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, emailAddress));
        }

        [When(@"the email tool is executed")]
        public void WhenTheEmailToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the email result will be ""(.*)""")]
        public void ThenTheEmailResultWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"email execution has ""(.*)"" error")]
        public void ThenEmailExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError,
                                           actual ? "did not occur" : "did occur" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}