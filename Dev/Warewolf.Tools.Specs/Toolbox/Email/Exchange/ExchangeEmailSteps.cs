using Dev2.Activities.Exchange;
using Dev2.Data.Util;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using netDumbster.smtp;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Exchange.Email
{
    [Binding]
    public class ExchangeEmailSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public ExchangeEmailSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string body;
            scenarioContext.TryGetValue("body", out body);
            string subject;
            scenarioContext.TryGetValue("subject", out subject);
            string password;
            scenarioContext.TryGetValue("password", out password);
            string simulationOutput;
            scenarioContext.TryGetValue("simulationOutput", out simulationOutput);
            string to;
            scenarioContext.TryGetValue("to", out to);

            var server = SimpleSmtpServer.Start(25);
            scenarioContext.Add("server", server);

            var resourceName = Guid.NewGuid();
            var resourceID = Guid.NewGuid();
            var selectedEmailSource = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                UserName = "bernartdt@dvtdev.onmicrosoft.com",
                Password = "Kailey@40",
                ResourceName = resourceName.ToString(),
                ResourceID = resourceID
            };

            
            var emailSource = new ExchangeSource
            {
                AutoDiscoverUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                UserName = "bernartdt@dvtdev.onmicrosoft.com",
                Password = "Kailey@40",
                ResourceName = resourceName.ToString(),
                ResourceID = resourceID
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, emailSource, "");
            var emailSender = new Mock<IDev2EmailSender>();
            // ReSharper disable once RedundantAssignment
            var eR = new ErrorResultTO();
            emailSender
                .Setup(sender => sender.SendEmail(It.IsAny<IExchange>(), It.IsAny<IWarewolfListIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), out eR))
                .Returns("Success")
                .Callback((IExchange source,IWarewolfListIterator listIterator,IWarewolfIterator i1,IWarewolfIterator i2,IWarewolfIterator i3,IWarewolfIterator i4,IWarewolfIterator i5, IWarewolfIterator i6, ErrorResultTO errors)=>
            {
                listIterator.FetchNextValue(i1);
                listIterator.FetchNextValue(i2);
                listIterator.FetchNextValue(i3);
                listIterator.FetchNextValue(i4);
                listIterator.FetchNextValue(i5);
                listIterator.FetchNextValue(i6);
            });
            var sendEmail = new DsfExchangeEmailActivity(emailSender.Object)
            {
                Result = ResultVariable,
                Body = string.IsNullOrEmpty(body) ? "" : body,
                Subject = string.IsNullOrEmpty(subject) ? "" : subject,
                To = string.IsNullOrEmpty(to) ? "" : to,
                SavedSource = selectedEmailSource,
                Cc = string.Empty,
                Bcc = String.Empty,
                Attachments = String.Empty
            };

            TestStartNode = new FlowStep
            {
                Action = sendEmail
            };
            scenarioContext.Add("activity", sendEmail);
        }

        protected  void BuildDataList(string result)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string body;
            scenarioContext.TryGetValue("body", out body);
            string subject;
            scenarioContext.TryGetValue("subject", out subject);
            string password;
            scenarioContext.TryGetValue("password", out password);
            string simulationOutput;
            scenarioContext.TryGetValue("simulationOutput", out simulationOutput);
            string to;
            scenarioContext.TryGetValue("to", out to);

            var server = SimpleSmtpServer.Start(25);
            scenarioContext.Add("server", server);

            var resourceID = Guid.NewGuid();
            var resourceName = Guid.NewGuid();
            var selectedEmailSource = new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                UserName = "bernartdt@dvtdev.onmicrosoft.com",
                Password = "Kailey@40",
                ResourceName = resourceName.ToString(),
                ResourceID = resourceID
            };

            var emailSource = new ExchangeSource
            {
                AutoDiscoverUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                UserName = "bernartdt@dvtdev.onmicrosoft.com",
                Password = "Kailey@40",
                ResourceName = resourceName.ToString(),
                ResourceID = resourceID
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, emailSource, "");
            var emailSender = new Mock<IDev2EmailSender>();
            // ReSharper disable once RedundantAssignment
            var eR = new ErrorResultTO();
            emailSender
                .Setup(sender => sender.SendEmail(It.IsAny<IExchange>(), It.IsAny<IWarewolfListIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), out eR))
                .Returns(result)
                .Callback((IExchangeSource source,IWarewolfListIterator listIterator,IWarewolfIterator i1,IWarewolfIterator i2,IWarewolfIterator i3,IWarewolfIterator i4,IWarewolfIterator i5, IWarewolfIterator i6, ErrorResultTO errors)=>
            {
                listIterator.FetchNextValue(i1);
                listIterator.FetchNextValue(i2);
                listIterator.FetchNextValue(i3);
                listIterator.FetchNextValue(i4);
                listIterator.FetchNextValue(i5);
                listIterator.FetchNextValue(i6);
                
            });
            var sendEmail = new DsfExchangeEmailActivity(emailSender.Object)
            {
                Result = ResultVariable,
                Body = string.IsNullOrEmpty(body) ? "" : body,
                Subject = string.IsNullOrEmpty(subject) ? "" : subject,
                To = string.IsNullOrEmpty(to) ? "" : to,
                SavedSource = selectedEmailSource,
                Cc = string.Empty,
                Bcc = String.Empty,
                Attachments = String.Empty
            };

            TestStartNode = new FlowStep
            {
                Action = sendEmail
            };
            scenarioContext.Add("activity", sendEmail);
        }

        [Given(@"I have an exchange email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAnExchangEmailVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"to exchange address is ""(.*)""")]
        public void GivenToExchangeAddressIs(string to)
        {
            scenarioContext.Add("to", to);
        }

        [Given(@"the exchange subject is ""(.*)""")]
        public void GivenTheExchangeSubjectIs(string subject)
        {
            scenarioContext.Add("subject", subject);
        }

        [Given(@"exchange body is ""(.*)""")]
        public void GivenExchangeBodyIs(string body)
        {
            scenarioContext.Add("body", body);
        }

        [When(@"the exchange email tool is executed")]
        public void WhenTheExchangeEmailToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }
        [When(@"the exchange email tool is executed ""(.*)""")]
        public void WhenTheExchangeEmailToolIsExecuted(string p0)
        {
            BuildDataList(p0);
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }


        [Then(@"the exchange email result will be ""(.*)""")]
        public void ThenTheExchangeEmailResultWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
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
            var result = scenarioContext.Get<IDSFDataObject>("result");

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
            scenarioContext.Add("to", to);
        }
    }
}