using System;
using System.Collections.Generic;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests.Exchange
{
    [TestClass]
    public class DsfExchangeEmailNewActivityTest
    {
        [TestMethod]
        public void DsfExchangeEmailNewActivity_InvalidRuntimeSource()
        {
            var env = new ExecutionEnvironment();
            var dataMock = new Mock<IDSFDataObject>();
            dataMock.Setup(o => o.Environment).Returns(() => env);

            var act = new DsfExchangeEmailNewActivity
            {
                SavedSource = new Mock<IExchangeSource>().Object
            };

            act.Execute(dataMock.Object, 0);

            Assert.AreEqual(1, env.Errors.Count);
            Assert.AreEqual("Invalid Email Source", env.FetchErrors());
        }

        [TestMethod]
        public void DsfExchangeEmailNewActivity_UpdateForEachInputs_ExpectReplacedValues()
        {
            var act = new DsfExchangeEmailNewActivity
            {
                To = "ToValue1",
                Cc = "CcValue1",
                Bcc = "BccValue1",
                Subject = "SubjectValue1",
                Attachments = "AttachmentsValue1",
                Body = "BodyValue1",
                SavedSource = new Mock<IExchangeSource>().Object
            };
            List<Tuple<string, string>> updates = null;

            // test that null input is okay
            act.UpdateForEachInputs(updates);

            // test all headers can be updated
            updates = new List<Tuple<string, string>>
            {
                new Tuple<string,string>("ToValue1", "ToValue2"),
                new Tuple<string,string>("CcValue1", "CcValue2"),
                new Tuple<string,string>("BccValue1", "BccValue2"),
                new Tuple<string,string>("SubjectValue1", "SubjectValue2"),
                new Tuple<string,string>("AttachmentsValue1", "AttachmentsValue2"),
                new Tuple<string,string>("BodyValue1", "BodyValue2")
            };
            act.UpdateForEachInputs(updates);

            Assert.AreEqual("ToValue2", act.To);
            Assert.AreEqual("CcValue2", act.Cc);
            Assert.AreEqual("BccValue2", act.Bcc);
            Assert.AreEqual("SubjectValue2", act.Subject);
            Assert.AreEqual("AttachmentsValue2", act.Attachments);
            Assert.AreEqual("BodyValue2", act.Body);
        }
    }
}
