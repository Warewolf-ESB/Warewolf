using System;
using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Activities;
using System.Linq;
using Dev2.Common.State;
using Moq;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Activities.ActivityComparerTests.Exchange
{
    [TestClass]
    public class DsfSendEmailActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void UniqueIDEquals_EmptySendEmailActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void To_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void To_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void DeletePath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, To = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Cc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Cc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Cc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Cc = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Bcc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Bcc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Bcc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Bcc = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Subject_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Subject_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Subject_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Subject = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Attachments_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Attachments_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Attachments_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Attachments = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Body_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void BodyDifferent_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Body_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Body = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Password_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "a" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void PasswordDifferent_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Password_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Password = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void FromAccountDifferent_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, FromAccount = "A" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, FromAccount = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void FromAccount_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, FromAccount = "AAA" };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, FromAccount = "aaa" };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Priority_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Priority = Data.Interfaces.Enums.enMailPriorityEnum.High };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Priority = Data.Interfaces.Enums.enMailPriorityEnum.High };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Priority_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, Priority = Data.Interfaces.Enums.enMailPriorityEnum.High };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, Priority = Data.Interfaces.Enums.enMailPriorityEnum.Low };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void IsHtml_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, IsHtml = true };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, IsHtml = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void IsHtml_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfSendEmailActivity() { UniqueID = uniqueId, IsHtml = true };
            var emailActivity1 = new DsfSendEmailActivity() { UniqueID = uniqueId, IsHtml = false };
            //---------------Assert DsfSendEmailActivity----------------
            Assert.IsNotNull(emailActivity);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfSendEmailActivity_GetState")]
        public void DsfSendEmailActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var emailSource = new EmailSource
            {
                ResourceID = uniqueId
            };
            //------------Setup for test--------------------------
            const string expectedFromAccount = "[[FromAccount]]";
            const string expectedTo = "[[ToAccount]]";
            const string expectedCc = "[[CcAccount]]";
            const string expectedBcc = "[[BccAccount]]";
            const Dev2.Data.Interfaces.Enums.enMailPriorityEnum expectedPriority = Data.Interfaces.Enums.enMailPriorityEnum.Normal;
            const string expectedSubject = "[[Subject]]";
            const string expectedAttachments = "[[Attachments]]";
            const bool expectedIsHtml = true;
            const string expectedBody = "[[Body]]";
            const string expectedResult = "[[Res]]";
            var act = new DsfSendEmailActivity
            {
                SelectedEmailSource = emailSource,
                FromAccount = expectedFromAccount,
                To = expectedTo,
                Cc = expectedCc,
                Bcc = expectedBcc,
                Priority = expectedPriority,
                Subject = expectedSubject,
                Attachments = expectedAttachments,
                IsHtml = expectedIsHtml,
                Body = expectedBody,
                Result = expectedResult
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(11, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SelectedEmailSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = uniqueId.ToString()
                },
                new StateVariable
                {
                    Name = "FromAccount",
                    Type = StateVariable.StateType.Input,
                    Value = expectedFromAccount
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = expectedTo
                },
                new StateVariable
                {
                    Name = "Cc",
                    Type = StateVariable.StateType.Input,
                    Value = expectedCc
                },
                new StateVariable
                {
                    Name = "Bcc",
                    Type = StateVariable.StateType.Input,
                    Value = expectedBcc
                },
                new StateVariable
                {
                    Name = "Priority",
                    Type = StateVariable.StateType.Input,
                    Value = expectedPriority.ToString()
                },
                new StateVariable
                {
                    Name = "Subject",
                    Type = StateVariable.StateType.Input,
                    Value = expectedSubject
                },
                new StateVariable
                {
                    Name = "Attachments",
                    Type = StateVariable.StateType.Input,
                    Value = expectedAttachments
                },
                new StateVariable
                {
                    Name = "IsHtml",
                    Type = StateVariable.StateType.Input,
                    Value = expectedIsHtml.ToString()
                },
                new StateVariable
                {
                    Name = "Body",
                    Type = StateVariable.StateType.Input,
                    Value = expectedBody
                },
                new StateVariable
                {
                    Name = "Result",
                    Type = StateVariable.StateType.Output,
                    Value = expectedResult
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
