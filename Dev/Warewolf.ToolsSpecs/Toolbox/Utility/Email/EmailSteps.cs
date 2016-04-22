
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
using Dev2.Data.Util;
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

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
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
            bool isHtml;
            ScenarioContext.Current.TryGetValue("isHtml", out isHtml);

            var server = SimpleSmtpServer.Start(25);
            ScenarioContext.Current.Add("server", server);

            var selectedEmailSource = new EmailSource
            {
                Host = "localhost",
                Port = 25,
                UserName = "",
                Password = "",
                ResourceName = Guid.NewGuid().ToString(),
                ResourceID = Guid.NewGuid()
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, selectedEmailSource);
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
            ScenarioContext.Current.Add("activity", sendEmail);
        }

        [Given(@"the from account is ""(.*)""")]
        public void GivenTheFromAccountIs(string fromAccount)
        {
            ScenarioContext.Current.Add("fromAccount", fromAccount);
        }

        [Given(@"to address is ""(.*)""")]
        public void GivenToAddressIs(string to)
        {
            ScenarioContext.Current.Add("to", to);
        }

        [Given(@"the subject is ""(.*)""")]
        public void GivenTheSubjectIs(string subject)
        {
            ScenarioContext.Current.Add("subject", subject);
        }
        [Given(@"the email is html")]
        public void GivenTheEmailIsHtml()
        {
            ScenarioContext.Current.Add("IsHtml", true);
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

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
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

            if(variableList == null)
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
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the email result will be ""(.*)""")]
        public void ThenTheEmailResultWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
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
