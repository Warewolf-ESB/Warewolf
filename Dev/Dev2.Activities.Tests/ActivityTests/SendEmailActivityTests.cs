using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SendEmailActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void SendEmailActivityWhereEmailSenderIsNullExpectConcreateImplementation()
        {
            //------------Setup for test--------------------------
            var activity = GetSendEmailActivity();
            //------------Execute Test---------------------------
            var emailSender = activity.EmailSender;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(emailSender, typeof(EmailSender));
        }

        [TestMethod]
        public void SendEmailActivityWhereConstructedExpectIsAbstractString()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var activity = GetSendEmailActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(DsfActivityAbstract<string>));
        }

        [TestMethod]
        public void EmailSenderWhereGivenAnIEmailSenderExpectGetGivenValue()
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
        public void GetFindMissingTypeExpectDataGridActivityType()
        {
            //------------Setup for test--------------------------
            var activity = GetSendEmailActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, findMissingType);
        }

        [TestMethod]
        public void SendEmailExecuteWhereStaticValuesExpectCorrectResults()
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
        public void SendEmailExecuteWhereScalarValuesExpectCorrectResults()
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
        public void SendEmailExecuteWhereScalarValuesHasToListExpectCorrectResults()
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
        public void SendEmailExecuteWhereScalarValuesHasBCCListExpectCorrectResults()
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
        public void SendEmailExecuteWhereScalarValuesHasCCListExpectCorrectResults()
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


        [TestMethod]
        public void EmailSenderGetDebugInputOutputExpectedCorrectResults()
        {
            var emailSourceForTesting = EmailSourceForTesting();
            var mock = new Mock<IEmailSender>();
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                { });
            var activity = GetSendEmailActivity(mock);
            activity.SelectedEmailSource = emailSourceForTesting;
            activity.Body = "[[Body]]";
            activity.FromAccount = "[[FromAccount]]";
            activity.To = "[[CC]]";
            activity.Subject = "[[Subject]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><CC>to.someone@amail.account,to1.someone@amail.account,to.someone1@amail.account;to.someone@amail1.account;to.so2meone@amail.account,,</CC></root>";
            CurrentDl = "<ADL><Subject></Subject><Body></Body><FromAccount></FromAccount><CC></CC></ADL>";
            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(activity, "<root><Subject></Subject><Body></Body><FromAccount></FromAccount><CC></CC></root>",
                "<root><Subject>SubJectValue</Subject><Body>BodyValue</Body><FromAccount>from.someone@amail.account</FromAccount><CC>to.someone@amail.account,to1.someone@amail.account,to.someone1@amail.account;to.someone@amail1.account;to.so2meone@amail.account,,</CC></root>", out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, outRes.Count);
            var fetchResultsList = outRes[0].FetchResultsList();
            Assert.AreEqual(4, fetchResultsList.Count);

            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[0].Type);
            Assert.AreEqual("1", fetchResultsList[0].Value);


            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[2].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, fetchResultsList[2].Value);

            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[3].Type);
            Assert.AreEqual("Success", fetchResultsList[3].Value);

        }


        [TestMethod]
        public void SendEmailUpdateForEachInputsWhereScalarValuesShouldSetValues()
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
        public void SendEmailUpdateForEachOutsputsWhereScalarValuesShouldSetValues()
        {
            //------------Setup for test--------------------------
            var emailSourceForTesting = EmailSourceForTesting();
            var mock = new Mock<IEmailSender>();
            mock.Setup(sender =>
                sender.Send(emailSourceForTesting, It.IsAny<MailMessage>())).
                Callback<EmailSource, MailMessage>((client, message) =>
                { });
            var activity = GetSendEmailActivity(mock);
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

        [TestMethod]
        public void ExecuteWhereMixedScalarsRecordsetDataExpectCorrectExcecution()
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
            activity.FromAccount = "[[FromAccount]]";
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
            emailSourceForTesting.Host = "TestHost";
            emailSourceForTesting.UserName = "TestUserName";
            emailSourceForTesting.Password = "TestPassword";
            return emailSourceForTesting;
        }
    }


}
