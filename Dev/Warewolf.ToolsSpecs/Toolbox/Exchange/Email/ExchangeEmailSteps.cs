using Dev2.Activities.Exchange;
using Dev2.Data.Util;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using netDumbster.smtp;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Exchange.Email
{
    [Binding]
    public class ExchangeEmailSteps : RecordSetBases
    {
        protected override void BuildDataList()
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
            string password;
            ScenarioContext.Current.TryGetValue("password", out password);
            string simulationOutput;
            ScenarioContext.Current.TryGetValue("simulationOutput", out simulationOutput);
            string to;
            ScenarioContext.Current.TryGetValue("to", out to);

            var server = SimpleSmtpServer.Start(25);
            ScenarioContext.Current.Add("server", server);

            var selectedEmailSource = new ExchangeSource()
            {
                AutoDiscoverUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                UserName = "bernartdt@dvtdev.onmicrosoft.com",
                Password = "Kailey@40",
                ResourceName = Guid.NewGuid().ToString(),
                ResourceID = Guid.NewGuid()
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, selectedEmailSource);
            var sendEmail = new DsfExchangeEmailActivity()
            {
                Result = ResultVariable,
                Body = string.IsNullOrEmpty(body) ? "" : body,
                Subject = string.IsNullOrEmpty(subject) ? "" : subject,
                To = string.IsNullOrEmpty(to) ? "" : to,
                SavedSource = selectedEmailSource,
            };

            TestStartNode = new FlowStep
            {
                Action = sendEmail
            };
            ScenarioContext.Current.Add("activity", sendEmail);
        }

        [Given(@"I have an exchange email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAnExchangEmailVariableEqualTo(string variable, string value)
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

        [Given(@"to exchange address is ""(.*)""")]
        public void GivenToExchangeAddressIs(string to)
        {
            ScenarioContext.Current.Add("to", to);
        }

        [Given(@"the exchange subject is ""(.*)""")]
        public void GivenTheExchangeSubjectIs(string subject)
        {
            ScenarioContext.Current.Add("subject", subject);
        }

        [Given(@"exchange body is ""(.*)""")]
        public void GivenExchangeBodyIs(string body)
        {
            ScenarioContext.Current.Add("body", body);
        }

        [When(@"the exchange email tool is executed")]
        public void WhenTheExchangeEmailToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the exchange email result will be ""(.*)""")]
        public void ThenTheExchangeEmailResultWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsNull(actualValue);
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }

        [Then(@"the exchange execution has ""(.*)"" error")]
        public void ThenTheExchangeExecutionHasError(string anError)
        {
            bool expectedError = anError.Equals("AN", StringComparison.OrdinalIgnoreCase);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            string fetchErrors = string.Join(Environment.NewLine, result.Environment.AllErrors);
            bool actuallyHasErrors = result.Environment.Errors.Count > 0 || result.Environment.AllErrors.Count > 0;
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actuallyHasErrors ? "did not occur" : "did occur" + fetchErrors);

            var hasAnError = expectedError == actuallyHasErrors;
            var errorMessageMatches = anError.Equals(fetchErrors, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(hasAnError || errorMessageMatches, message);
        }

        [Given(@"exchange to address is ""(.*)""")]
        public void GivenExchangeToAddressIs(string to)
        {
            ScenarioContext.Current.Add("to", to);
        }
    }
}