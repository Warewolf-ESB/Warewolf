using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using TechTalk.SpecFlow;
using System.Linq;
using System.Text;

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


        public EmailSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            var builder = new StringBuilder();
            ((List<Tuple<string, string>>) _variableList).ForEach(t => builder.Append(string.Format("{0}{1}", t.Item2, ";")));

            _sendEmail = new DsfSendEmailActivity
                {
                    Result = ResultVariable,
                    Body = _body,
                    Subject = _subject,
                    FromAccount = _fromAccount,
                    Password = _password,
                    IsSimulationEnabled = true,
                    SimulationOutput = _simulationOutput,
                    To = builder.ToString()
                };

            TestStartNode = new FlowStep
                {
                    Action = _sendEmail
                };
        }

        [Given(@"the from account is ""(.*)"" with the subject ""(.*)""")]
        public void GivenTheFromAccountIsWithTheSubject(string fromAccount, string subject)
        {
            _fromAccount = fromAccount;
            _subject = subject;
        }

        [Given(@"the sever name is ""(.*)"" with password as ""(.*)""")]
        public void GivenTheSeverNameIsWithPasswordAs(string serverName, string password)
        {
            _serverName = serverName;
            _password = password;
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

        [Then(@"the number of emails sent will be ""(.*)""")]
        public void ThenTheNumberOfEmailsSentWillBe(string numberSent)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            // Assert.IsTrue(actualValue.Contains(result));
        }
    }
}