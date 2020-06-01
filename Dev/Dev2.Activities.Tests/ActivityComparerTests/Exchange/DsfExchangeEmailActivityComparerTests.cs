using System;
using System.Linq;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityComparerTests.Exchange
{
    [TestClass]
    public class DsfExchangeEmailActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void To_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void To_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DeletePath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, To = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Cc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Cc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Cc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Cc = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Bcc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Bcc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Bcc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Bcc = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Subject_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Subject_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Subject_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Subject = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Attachments_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Attachments_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Attachments_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Attachments = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Body_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void BodyDifferent_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Body_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity() { UniqueID = uniqueId, Body = "aaa" };
            //---------------Assert DsfExchangeEmailActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void SavedSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(exchangeEmailActivity.Equals(exchangeEmailActivity1));
            //---------------Execute Test ----------------------
            exchangeEmailActivity.SavedSource = new ExchangeSourceDefinition(){AutoDiscoverUrl = "a"};
            exchangeEmailActivity1.SavedSource = new ExchangeSourceDefinition();
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void SavedSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailActivity { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(exchangeEmailActivity.Equals(exchangeEmailActivity1));
            //---------------Execute Test ----------------------
            exchangeEmailActivity.SavedSource = new ExchangeSourceDefinition();
            exchangeEmailActivity1.SavedSource = new ExchangeSourceDefinition();
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfExchangeEmailActivity_GetState")]
        public void DsfExchangeEmailActivity_GetState_ReturnsStateVariable()
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

            var act = new DsfExchangeEmailActivity
            {
                SavedSource = expectedSavedSource,
                To = expectedTo,
                Cc = expectedCc,
                Bcc = expectedBcc,
                Subject = expectedSubject,
                Attachments = expectedAttachments,
                Body = expectedBody,
                Result = expectedResult
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(8, stateItems.Count());

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

        [TestMethod]
        [Timeout(60000)]
        public void SavedSource_Null_Object_Is_NotEqual()
        {
            //---------------Set up test pack-------------------
            var exchangeEmailActivity = new ExchangeSourceDefinition
            {
                ResourceID = Guid.NewGuid(),
                Path = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(exchangeEmailActivity.Equals(null), "Equals operator can't compare to null.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SavedSource_Itself_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var exchangeEmailActivity = new ExchangeSourceDefinition
            {
                ResourceID = Guid.NewGuid(),
                Path = "A"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(exchangeEmailActivity.Equals(exchangeEmailActivity), "Equals operator can't compare to itself.");
        }
    }
}
