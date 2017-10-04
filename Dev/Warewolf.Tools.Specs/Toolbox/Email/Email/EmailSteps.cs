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
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using netDumbster.smtp;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.Email
{
    [Binding]
    public class EmailSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public EmailSteps(ScenarioContext scenarioContext)
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
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("body", out string body);
            scenarioContext.TryGetValue("subject", out string subject);
            scenarioContext.TryGetValue("fromAccount", out string fromAccount);
            scenarioContext.TryGetValue("password", out string password);
            scenarioContext.TryGetValue("simulationOutput", out string simulationOutput);
            scenarioContext.TryGetValue("to", out string to);
            scenarioContext.TryGetValue("isHtml", out bool isHtml);

            var server = SimpleSmtpServer.Start(25);
            scenarioContext.Add("server", server);

            var selectedEmailSource = new EmailSource
            {
                Host = "localhost",
                Port = 25,
                UserName = "",
                Password = "",
                ResourceName = Guid.NewGuid().ToString(),
                ResourceID = Guid.NewGuid()
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, selectedEmailSource,"");
            var sendEmail = new DsfSendEmailActivity
                {
                    Result = ResultVariable,
                    Body = string.IsNullOrEmpty(body) ? "" : body,
                    Subject = string.IsNullOrEmpty(subject) ? "" : subject,
                    FromAccount = string.IsNullOrEmpty(fromAccount) ? "" : fromAccount,
                    To = string.IsNullOrEmpty(to) ? "" : to,
                    SelectedEmailSource = selectedEmailSource,
                    IsHtml = isHtml
                };

            TestStartNode = new FlowStep
                {
                    Action = sendEmail
                };
            scenarioContext.Add("activity", sendEmail);
        }

        [Given(@"the from account is ""(.*)""")]
        public void GivenTheFromAccountIs(string fromAccount)
        {
            scenarioContext.Add("fromAccount", fromAccount);
        }

        [Given(@"to address is ""(.*)""")]
        public void GivenToAddressIs(string to)
        {
            scenarioContext.Add("to", to);
        }

        [Given(@"the subject is ""(.*)""")]
        public void GivenTheSubjectIs(string subject)
        {
            scenarioContext.Add("subject", subject);
        }
        [Given(@"the email is html")]
        public void GivenTheEmailIsHtml()
        {
            scenarioContext.Add("IsHtml", true);
        }


        [Given(@"the sever name is ""(.*)"" with password as ""(.*)""")]
        public void GivenTheSeverNameIsWithPasswordAs(string serverName, string password)
        {
            scenarioContext.Add("serverName", serverName);
            scenarioContext.Add("password", password);
        }

        [Given(@"I have an email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAnEmailVariableEqualTo(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"body is ""(.*)""")]
        public void GivenBodyIs(string body)
        {
            scenarioContext.Add("body", body);
        }

        [Given(@"I have a variable ""(.*)"" with this email address ""(.*)""")]
        public void GivenIHaveAVariableWithThisEmailAddress(string variable, string emailAddress)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, emailAddress));
        }

        [When(@"the email tool is executed")]
        public void WhenTheEmailToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the email result will be ""(.*)""")]
        public void ThenTheEmailResultWillBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);
            if(string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsNull(actualValue);
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }
    }
}
