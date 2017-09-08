using System;
using Dev2.Activities.Exchange;
using Dev2.Common.Interfaces.Core;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Exchange
{
    [TestClass]
    public class DsfExchangeEmailActivityComparerTests
    {
        [TestMethod]
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
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
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
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
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
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
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
            exchangeEmailActivity1.SavedSource = new ExchangeSourceDefinition();;
            var @equals = exchangeEmailActivity.Equals(exchangeEmailActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
    }
}
