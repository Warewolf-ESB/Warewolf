using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.State;
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfExchangeEmailNewActivity_GetState")]
        public void DsfExchangeEmailNewActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid();
            var mockExchangeSource = new Mock<IExchangeSource>();
            mockExchangeSource.Setup(source => source.ResourceID).Returns(uniqueId);

            var expectedSavedSource = mockExchangeSource.Object;
            var expectedTo = "testTo@test.com";
            var expectedCc = "testCc@test.com";
            var expectedBcc = "testBcc@test.com";
            var expectedSubject = "test Email";
            var expectedAttachments = "att_1;att_2;att_3";
            var expectedBody = "Email Body";
            var expectedResult = "[[res]]";

            var act = new DsfExchangeEmailNewActivity
            {
                SavedSource = expectedSavedSource,
                To = expectedTo,
                Cc = expectedCc,
                Bcc = expectedBcc,
                Subject = expectedSubject,
                Attachments = expectedAttachments,
                IsHtml = true,
                Body = expectedBody,
                Result = expectedResult
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(9, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SavedSource.ResourceID",
                    Type=StateVariable.StateType.Input,
                    Value= uniqueId.ToString()
                },
                new StateVariable
                {
                    Name="To",
                    Type=StateVariable.StateType.Input,
                    Value= expectedTo
                },
                new StateVariable
                {
                    Name="Cc",
                    Type=StateVariable.StateType.Input,
                    Value= expectedCc
                },
                new StateVariable
                {
                    Name="Bcc",
                    Type=StateVariable.StateType.Input,
                    Value= expectedBcc
                },
                new StateVariable
                {
                    Name="Subject",
                    Type=StateVariable.StateType.Input,
                    Value= expectedSubject
                },
                new StateVariable
                {
                    Name="Attachments",
                    Type=StateVariable.StateType.Input,
                    Value= expectedAttachments
                },
                new StateVariable
                {
                    Name="IsHtml",
                    Type=StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Body",
                    Type=StateVariable.StateType.Input,
                    Value= expectedBody
                },
                new StateVariable
                {
                    Name="Result",
                    Type=StateVariable.StateType.Output,
                    Value= expectedResult
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
