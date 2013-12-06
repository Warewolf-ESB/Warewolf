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
        private string _body;
        private string _fromAccount;
        private string _password;
        private DsfSendEmailActivity _sendEmail;
        private string _serverName;
        private string _simulationOutput;
        private string _subject;
        private string _to;
        
        public EmailSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _sendEmail = new DsfSendEmailActivity
                {
                    Result = ResultVariable,
                    Body = _body,
                    Subject = _subject,
                    FromAccount = _fromAccount,
                    Password = _password,
                    IsSimulationEnabled = true,
                    SimulationOutput = _simulationOutput,
                    To = _to
                };

            TestStartNode = new FlowStep
                {
                    Action = _sendEmail
                };
        }

        [Given(@"I have an email address input ""(.*)""")]
        public void GivenIHaveAnEmailAddressInput(string emailAddress)
        {
            _to = emailAddress;
        }

        [Given(@"the from account is ""(.*)""")]
        public void GivenTheFromAccountIs(string fromAccount)
        {
            _fromAccount = fromAccount;
        }

        [Given(@"the subject is ""(.*)""")]
        public void GivenTheSubjectIs(string subject)
        {
            _subject = subject;
        }


        [Given(@"the sever name is ""(.*)"" with password as ""(.*)""")]
        public void GivenTheSeverNameIsWithPasswordAs(string serverName, string password)
        {
            _serverName = serverName;
            _password = password;
        }

        [Given(@"I have an email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAnEmailVariableEqualTo(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, string.Empty));
        }

        [Given(@"body is ""(.*)""")]
        public void GivenBodyIs(string body)
        {
            _body = body;
        }

        [Given(@"I have a variable ""(.*)"" with this email address ""(.*)""")]
        public void GivenIHaveAVariableWithThisEmailAddress(string variable, string emailAddress)
        {
            _variableList.Add(new Tuple<string, string>(variable, emailAddress));
        }

        [When(@"the email tool is executed")]
        public void WhenTheEmailToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the email result will be ""(.*)""")]
        public void ThenTheEmailResultWillBe(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }

        [Then(@"email execution has ""(.*)"" error")]
        public void ThenEmailExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

    }
}