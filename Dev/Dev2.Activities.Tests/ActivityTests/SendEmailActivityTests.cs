
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using ActivityUnitTests;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class SendEmailActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [TestCategory("SendEmail_Constructor")]
        public void SendEmail_Constructor_TypeIsAbstractString()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var activity = GetSendEmailActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(DsfActivityAbstract<string>));
        }

        [TestMethod]
        [TestCategory("SendEmail_Constructor")]
        public void SendEmail_Constructor_EmailSenderIsNotAssigned_GetsNonNullValue()
        {
            //------------Setup for test--------------------------
            var activity = GetSendEmailActivity();
            //------------Execute Test---------------------------
            var emailSender = activity.EmailSender;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(emailSender, typeof(EmailSender));
        }

        [TestMethod]
        [TestCategory("SendEmail_Constructor")]
        public void SendEmail_Constructor_EmailSenderIsAssigned_GetsGivenValue()
        {
            //------------Setup for test--------------------------
            var activity = GetSendEmailActivity();
            var emailSender = new Mock<IEmailSender>().Object;
            activity.EmailSender = emailSender;
            //------------Execute Test---------------------------
            var actual = activity.EmailSender;
            //------------Assert Results-------------------------
            Assert.AreEqual(emailSender, actual);
            Assert.IsNotInstanceOfType(actual, typeof(EmailSender));
        }

        [TestMethod]
        [TestCategory("SendEmail_GetFindMissingType")]
        public void SendEmail_GetFindMissingType_StaticActivityType()
        {
            //------------Setup for test--------------------------
            var activity = GetSendEmailActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, findMissingType);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_StaticValues_CorrectResults()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            MailMessage mailMessage = null;
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                {
                    mailMessage = message;
                });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "BodyValue";
            activity.FromAccount = "from.someone@amail.account";
            activity.To = "to.someone@amail.account";
            activity.Subject = "SubJectValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(activity.Body, mailMessage.Body);
            Assert.AreEqual(activity.FromAccount, mailMessage.From.Address);
            Assert.AreEqual(activity.To, mailMessage.To[0].Address);
            Assert.AreEqual(activity.Subject, mailMessage.Subject);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_ScalarValuesExpectCorrectResults()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            MailMessage mailMessage = null;
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                {
                    mailMessage = message;
                });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.To = "[[ToAccount]]";
            activity.Subject = "[[Subject]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><ToAccount>to.someone@amail.account</ToAccount></root>";
            CurrentDl = "<ADL><Subject></Subject><Body></Body><FromAccount></FromAccount><ToAccount></ToAccount></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("from.someone@amail.account", mailMessage.From.Address);
            Assert.AreEqual("to.someone@amail.account", mailMessage.To[0].Address);
            Assert.AreEqual("BodyValue", mailMessage.Body);
            Assert.AreEqual("SubJectValue", mailMessage.Subject);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_ScalarValuesHasToList_CorrectResults()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            MailMessage mailMessage = null;
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                {
                    mailMessage = message;
                });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.To = "[[ToAccount]]";
            activity.Subject = "[[Subject]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><ToAccount>to.someone@amail.account,to1.someone@amail.account,to.someone1@amail.account;to.someone@amail1.account;to.so2meone@amail.account,,</ToAccount></root>";
            CurrentDl = "<ADL><Subject></Subject><Body></Body><FromAccount></FromAccount><ToAccount></ToAccount></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("to.someone@amail.account", mailMessage.To[0].Address);
            Assert.AreEqual("to1.someone@amail.account", mailMessage.To[1].Address);
            Assert.AreEqual("to.someone1@amail.account", mailMessage.To[2].Address);
            Assert.AreEqual("to.someone@amail1.account", mailMessage.To[3].Address);
            Assert.AreEqual("to.so2meone@amail.account", mailMessage.To[4].Address);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_ScalarValuesHasBCCList_CorrectResults()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            MailMessage mailMessage = null;
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                {
                    mailMessage = message;
                });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.FromAccount = "[[FromAccount]]";
            activity.Bcc = "[[BCC]]";
            activity.Subject = "[[Subject]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><BCC>to.someone@amail.account,to1.someone@amail.account,to.someone1@amail.account;to.someone@amail1.account;to.so2meone@amail.account,,</BCC></root>";
            CurrentDl = "<ADL><Subject></Subject><Body></Body><FromAccount></FromAccount><BCC></BCC></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("to.someone@amail.account", mailMessage.Bcc[0].Address);
            Assert.AreEqual("to1.someone@amail.account", mailMessage.Bcc[1].Address);
            Assert.AreEqual("to.someone1@amail.account", mailMessage.Bcc[2].Address);
            Assert.AreEqual("to.someone@amail1.account", mailMessage.Bcc[3].Address);
            Assert.AreEqual("to.so2meone@amail.account", mailMessage.Bcc[4].Address);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_ScalarValuesHasCCList_CorrectResults()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            MailMessage mailMessage = null;
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                {
                    mailMessage = message;
                });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.Cc = "[[CC]]";
            activity.Subject = "[[Subject]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><CC>to.someone@amail.account,to1.someone@amail.account,to.someone1@amail.account;to.someone@amail1.account;to.so2meone@amail.account,,</CC></root>";
            CurrentDl = "<ADL><Subject></Subject><Body></Body><FromAccount></FromAccount><CC></CC></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("to.someone@amail.account", mailMessage.CC[0].Address);
            Assert.AreEqual("to1.someone@amail.account", mailMessage.CC[1].Address);
            Assert.AreEqual("to.someone1@amail.account", mailMessage.CC[2].Address);
            Assert.AreEqual("to.someone@amail1.account", mailMessage.CC[3].Address);
            Assert.AreEqual("to.so2meone@amail.account", mailMessage.CC[4].Address);
        }

        [TestMethod]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_MixedScalarsRecordsetData_CorrectExcecution()
        {
            var emailSourceForTesting = EmailSourceForTesting();
            var esbChannelMock = CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                { });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[mails(*).to]]" + Environment.NewLine + "[[Body]]";
            activity.To = "[[mails(*).to]]";
            activity.Subject = "This is the subject!";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Body>Body TextQWERXC@#$%</Body><FromAccount>from.someone@amail.account</FromAccount><mails><to>to.someone@amail.account</to></mails><mails><to>to1.someone@amail.account</to></mails><mails><to>to.someone1@amail.account</to></mails><mails><to>to.someone@amail1.account</to></mails></root>";
            CurrentDl = "<ADL><Body></Body><FromAccount></FromAccount><mails><to/></mails></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object, isDebug: true);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.Send(emailSourceForTesting, It.IsAny<MailMessage>()), Times.Exactly(4));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SendEmail_Execute")]
        public void SendEmail_Execute_FromAccount_EmailSourceIsCorrect()
        {
            Verify_Execute_FromAccount_EmailSourceIsCorrect(isFromAccountGiven: true);
            Verify_Execute_FromAccount_EmailSourceIsCorrect(isFromAccountGiven: false);
        }

        void Verify_Execute_FromAccount_EmailSourceIsCorrect(bool isFromAccountGiven)
        {
            //------------Setup for test--------------------------
            const string TestSourceAccount = "someone@mydomain.com";
            const string TestSourcePassword = "xxxx";
            const string TestFromAccount = "someonelse@mydomain.com";
            const string TestFromPassword = "yyyy";

            var testSource = new EmailSource
            {
                Host = "TestHost",
                UserName = TestSourceAccount,
                Password = TestSourcePassword,
                ResourceName = Guid.NewGuid().ToString(),
                ResourceID = Guid.NewGuid()
            };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, testSource);
            EmailSource sendSource = null;
            MailMessage sendMessage = null;
            var emailSender = new Mock<IEmailSender>();
            emailSender.Setup(sender => sender.Send(It.IsAny<EmailSource>(), It.IsAny<MailMessage>()))
                .Callback<EmailSource, MailMessage>((source, message) =>
                {
                    sendSource = source;
                    sendMessage = message;
                });

            var activity = GetSendEmailActivity(emailSender);
            activity.SelectedEmailSource = testSource;
            activity.Body = "Hello world";
            activity.To = "myrecipient@mydomain.com";
            activity.Subject = "This is the subject!";
            if(isFromAccountGiven)
            {
                activity.FromAccount = TestFromAccount;
                activity.Password = TestFromPassword;
            }
            else
            {
                activity.FromAccount = string.Empty;
                activity.Password = string.Empty;
            }

            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root></root>";
            CurrentDl = "<ADL></ADL>";
            var esbChannelMock = CreateMockEsbChannel(testSource);

            //------------Execute Test---------------------------
            var result = ExecuteProcess(channel: esbChannelMock.Object, isDebug: true);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //------------Assert Results-------------------------
            emailSender.Verify(sender => sender.Send(It.IsAny<EmailSource>(), It.IsAny<MailMessage>()), Times.Once());
            Assert.IsNotNull(sendSource);
            Assert.IsNotNull(sendMessage);
            Assert.AreNotSame(testSource, sendSource);
            if(isFromAccountGiven)
            {
                Assert.AreEqual(TestFromAccount, sendSource.UserName);
                Assert.AreEqual(TestFromPassword, sendSource.Password);
                Assert.AreEqual(TestFromAccount, sendMessage.From.Address);
            }
            else
            {
                Assert.AreEqual(TestSourceAccount, sendSource.UserName);
                Assert.AreEqual(TestSourcePassword, sendSource.Password);
                Assert.AreEqual(TestSourceAccount, sendMessage.From.Address);
            }
        }

        [TestMethod]
        [TestCategory("SendEmail_UpdateForEachInputs")]
        public void SendEmail_UpdateForEachInputs_ScalarValuesShouldSetValues()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            CreateMockEsbChannel(emailSourceForTesting);
            var mock = new Mock<IEmailSender>();
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                { });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.FromAccount = "[[FromAccount]]";
            activity.Cc = "[[CC]]";
            activity.Subject = "[[Subject]]";
            activity.Password = "[[Password]]";
            activity.To = "[[To]]";
            activity.Bcc = "[[Bcc]]";
            activity.Attachments = "[[Attachments]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            activity.UpdateForEachInputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[FromAccount]]", "from.someone@amail.account"),
                new Tuple<string, string>("[[Body]]", "BodyValue"),
                new Tuple<string, string>("[[CC]]", "to.someone@amail.account"),
                new Tuple<string, string>("[[Subject]]", "SubJectValue"),
                new Tuple<string, string>("[[Password]]", "PasswordValue"),
                new Tuple<string, string>("[[To]]", "ToValue"),
                new Tuple<string, string>("[[Bcc]]", "BccValue"),
                new Tuple<string, string>("[[Attachments]]", "AttachmentsValue"),

            }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("BodyValue", activity.Body);
            Assert.AreEqual("SubJectValue", activity.Subject);
            Assert.AreEqual("from.someone@amail.account", activity.FromAccount);
            Assert.AreEqual("to.someone@amail.account", activity.Cc);
            Assert.AreEqual("BccValue", activity.Bcc);
            Assert.AreEqual("AttachmentsValue", activity.Attachments);
            Assert.AreEqual("ToValue", activity.To);
            Assert.AreEqual("PasswordValue", activity.Password);
        }

        [TestMethod]
        [TestCategory("SendEmail_UpdateForEachOutsputs")]
        public void SendEmail_UpdateForEachOutsputs_ScalarValuesShouldSetValues()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var mock = new Mock<IEmailSender>();
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                { });
            var activity = GetSendEmailActivity(mock);
            activity.Result = "[[Result]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            activity.UpdateForEachOutputs(new List<Tuple<string, string>>
            {
                new Tuple<string, string>("[[Result]]", "TheResult"),

            }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("TheResult", activity.Result);
        }

        static Mock<IEsbChannel> CreateMockEsbChannel(EmailSource emailSourceForTesting)
        {
            Mock<IEsbChannel> esbChannelMock = new Mock<IEsbChannel>();
            ErrorResultTO errorResultTO;
            esbChannelMock.Setup(channel => channel.FetchServerModel<EmailSource>(
                It.IsAny<IDSFDataObject>(),
                It.IsAny<Guid>(),
                out errorResultTO)).Returns(emailSourceForTesting);
            return esbChannelMock;
        }

        static DsfSendEmailActivity GetSendEmailActivity(Mock<IEmailSender> mock)
        {
            var emailSender = mock.Object;
            var activity = GetSendEmailActivity();
            activity.EmailSender = emailSender;
            return activity;
        }

        static DsfSendEmailActivity GetSendEmailActivity()
        {
            var activity = new DsfSendEmailActivity();
            return activity;
        }

        static EmailSource EmailSourceForTesting()
        {
            var emailSourceForTesting = new EmailSource();
            emailSourceForTesting.ResourceName = Guid.NewGuid().ToString();
            emailSourceForTesting.ResourceID = Guid.NewGuid();
            emailSourceForTesting.Host = "TestHost";
            emailSourceForTesting.UserName = "from.someone@amail.account";
            emailSourceForTesting.Password = "TestPassword";
            ResourceCatalog.Instance.SaveResource(Guid.Empty, emailSourceForTesting);
            return emailSourceForTesting;
        }
    }


}
