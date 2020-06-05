using System;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Exchange
{
    [TestClass]
    public class DsfExchangeNewEmailActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void IsHtml_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, IsHtml = true };
            var emailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, IsHtml = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            Assert.IsTrue(emailActivity.IsHtml);
            Assert.IsTrue(emailActivity1.IsHtml);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void IsNotHtml_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var emailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId };
            var emailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(emailActivity);
            Assert.IsFalse(emailActivity.IsHtml);
            Assert.IsFalse(emailActivity1.IsHtml);
            //---------------Execute Test ----------------------
            var @equals = emailActivity.Equals(emailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void To_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void To_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void DeletePath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, To = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Cc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Cc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Cc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Cc = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Bcc_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Bcc_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Bcc_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Bcc = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Subject_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Subject_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Subject_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Subject = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Attachments_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Attachments_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Attachments_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Attachments = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Body_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "a" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void BodyDifferent_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void Body_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "AAA" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Body = "aaa" };
            //---------------Assert DsfExchangeEmailNewActivity----------------
            Assert.IsNotNull(exchangeEmailActivity);
            //---------------Execute Test ----------------------
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void SavedSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(exchangeEmailActivity.Equals(exchangeEmailActivity1));
            //---------------Execute Test ----------------------
            exchangeEmailActivity.SavedSource = new ExchangeSourceDefinition { AutoDiscoverUrl = "a" };
            exchangeEmailActivity1.SavedSource = new ExchangeSourceDefinition();
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void SavedSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "A" };
            var exchangeEmailActivity1 = new DsfExchangeEmailNewActivity { UniqueID = uniqueId, Result = "A" };
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
        [Owner("Rory McGuire")]
        public void Null_Objects_Not_Equal_Same_Object_Equal()
        {
            //---------------Set up test pack-------------------
            var exchangeEmailActivity = new DsfExchangeEmailNewActivity { };
            Object other = exchangeEmailActivity;
            //---------------Assert Precondition----------------
            Assert.IsFalse(exchangeEmailActivity.Equals(null));
            Assert.IsFalse(other.Equals(null));
            Assert.IsFalse(other.Equals(1));
            Assert.IsFalse(exchangeEmailActivity.Equals(1));
            //---------------Execute Test ----------------------

            Assert.IsTrue(exchangeEmailActivity.Equals(other));
            Assert.IsTrue(exchangeEmailActivity.Equals(exchangeEmailActivity));
        }
    }
}
