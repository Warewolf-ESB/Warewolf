using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using netDumbster.smtp;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.Toolbox.Exchange
{
    [Binding]
    public class ExchangeEmailSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;
        delegate void myDelegate(IExchange source, IWarewolfListIterator listIterator, IWarewolfIterator i1, IWarewolfIterator i2, IWarewolfIterator i3, IWarewolfIterator i4, IWarewolfIterator i5, IWarewolfIterator i6, out ErrorResultTO errors, bool _isHtml);

        public ExchangeEmailSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException(nameof(scenarioContext));
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
            scenarioContext.TryGetValue("password", out string password);
            scenarioContext.TryGetValue("simulationOutput", out string simulationOutput);
            scenarioContext.TryGetValue("to", out string to);
            scenarioContext.TryGetValue("isHtml", out string isHtml);

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

            var eR = new ErrorResultTO();
            emailSender
                .Setup(sender => sender.SendEmail(It.IsAny<IExchange>(), It.IsAny<IWarewolfListIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), out eR, It.IsAny<bool>()))
                .Returns("Success")
                .Callback(new myDelegate((IExchange source, IWarewolfListIterator listIterator, IWarewolfIterator i1, IWarewolfIterator i2, IWarewolfIterator i3, IWarewolfIterator i4, IWarewolfIterator i5, IWarewolfIterator i6, out ErrorResultTO errors, bool _isHtml) =>
                {
                    listIterator.FetchNextValue(i1);
                    listIterator.FetchNextValue(i2);
                    listIterator.FetchNextValue(i3);
                    listIterator.FetchNextValue(i4);
                    listIterator.FetchNextValue(i5);
                    listIterator.FetchNextValue(i6);
                    isHtml = _isHtml ? "true" : "false";
                    errors = null;
                }));
            var sendEmail = new DsfExchangeEmailNewActivity(emailSender.Object)
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
            if (isHtml == "true")
            {
                sendEmail.IsHtml = true;
            }
            else
            {
                sendEmail.IsHtml = false;
            }

            TestStartNode = new FlowStep
            {
                Action = sendEmail
            };
            scenarioContext.Add("activity", sendEmail);
        }

        protected void BuildDataList(string result)
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
            scenarioContext.TryGetValue("password", out string password);
            scenarioContext.TryGetValue("simulationOutput", out string simulationOutput);
            scenarioContext.TryGetValue("to", out string to);
            scenarioContext.TryGetValue("isHtml", out string isHtml);

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

            var eR = new ErrorResultTO();
            emailSender
                .Setup(sender => sender.SendEmail(It.IsAny<IExchange>(), It.IsAny<IWarewolfListIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), It.IsAny<IWarewolfIterator>(), out eR, It.IsAny<bool>()))
                .Returns(result)
                .Callback((IExchangeSource source, IWarewolfListIterator listIterator, IWarewolfIterator i1, IWarewolfIterator i2, IWarewolfIterator i3, IWarewolfIterator i4, IWarewolfIterator i5, IWarewolfIterator i6, ErrorResultTO errors) =>
                {
                    listIterator.FetchNextValue(i1);
                    listIterator.FetchNextValue(i2);
                    listIterator.FetchNextValue(i3);
                    listIterator.FetchNextValue(i4);
                    listIterator.FetchNextValue(i5);
                    listIterator.FetchNextValue(i6);

                });
            var sendEmail = new DsfExchangeEmailNewActivity(emailSender.Object)
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

        [Given(@"I have a new exchange email variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveANewExchangeEmailVariableEqualTo(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"new to exchange address is ""(.*)""")]
        public void GivenNewToExchangeAddressIs(string to)
        {
            scenarioContext.Add("to", to);
        }

        [Given(@"the new exchange subject is ""(.*)""")]
        public void GivenTheNewExchangeSubjectIs(string subject)
        {
            scenarioContext.Add("subject", subject);
        }

        [Given(@"new exchange body is ""(.*)""")]
        public void GivenNewExchangeBodyIs(string body)
        {
            scenarioContext.Add("body", body);
        }

        [Given(@"new exchange IsHtml is ""(.*)""")]
        public void GivenNewExchangeIsHtmlIs(string isHtml)
        {
            scenarioContext.Add("isHtml", isHtml);
        }

        [When(@"the new exchange email tool is executed")]
        public void WhenTheNewExchangeEmailToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the new exchange email result will be ""(.*)""")]
        public void ThenTheNewExchangeEmailResultWillBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }

        [Then(@"the new exchange execution has ""(.*)"" error")]
        public void ThenTheNewExchangeExecutionHasError(string anError)
        {
            var expectedError = anError.Equals("AN", StringComparison.OrdinalIgnoreCase);
            var result = scenarioContext.Get<IDSFDataObject>("result");

            var fetchErrors = string.Join(Environment.NewLine, result.Environment.AllErrors);
            var actuallyHasErrors = result.Environment.Errors.Count > 0 || result.Environment.AllErrors.Count > 0;
            var message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actuallyHasErrors ? "did not occur" : "did occur" + fetchErrors);

            var hasAnError = expectedError == actuallyHasErrors;
            var errorMessageMatches = anError.Equals(fetchErrors, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(hasAnError || errorMessageMatches, message);
        }

        [Given(@"new exchange to address is ""(.*)""")]
        public void GivenNewExchangeToAddressIs(string to)
        {
            scenarioContext.Add("to", to);
        }

        [When(@"the new exchange email tool is executed ""(.*)""")]
        public void WhenTheNewExchangeEmailToolIsExecuted(string p0)
        {
            BuildDataList(p0);
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"new to address is ""(.*)""")]
        public void GivenNewToAddressIs(string to)
        {
            scenarioContext.Add("to", to);
        }

    }
}
