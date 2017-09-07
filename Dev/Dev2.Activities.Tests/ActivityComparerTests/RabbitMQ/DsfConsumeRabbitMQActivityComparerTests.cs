using System;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Common.ExtMethods;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.RabbitMQ
{
    [TestClass]
    public class DsfConsumeRabbitMqActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyConsumeRabbitMQ_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(rabbitMqActivity);
            //---------------Execute Test ----------------------
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyConsumeRabbitMQ_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity();
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(rabbitMqActivity);
            //---------------Execute Test ----------------------
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void QueueName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" , QueueName = "a"};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "", QueueName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void QueueName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "A" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void QueueName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" , DisplayName = "a"};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "", DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", DisplayName = "A" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", DisplayName = "AAA" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Response_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" , QueueName = "a", Response = "klkl"};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "", QueueName = "a", Response = "klkl"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Response_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass", Response = "klkl" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass", Response = "klklkkkkk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Response_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA", Response = "KLKL" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA", Response = "klkl" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", ObjectName  = ""};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a" , ObjectName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ObjectName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", ObjectName = "A"};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , ObjectName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RabbitMQSourceResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "", RabbitMQSourceResourceId = Guid.Empty};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "", RabbitMQSourceResourceId = Guid.Empty };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RabbitMQSourceResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a", ObjectName  = "a", RabbitMQSourceResourceId = Guid.NewGuid()};
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "a" , ObjectName = "a", RabbitMQSourceResourceId = Guid.NewGuid()};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RabbitMQSourceResourceId_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", ObjectName = "A", RabbitMQSourceResourceId = Guid.Empty };
            var multiAssign1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , ObjectName = "a", RabbitMQSourceResourceId = Guid.Empty.ToString().ToLower().ToGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Prefetch_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Prefetch = "1";
            rabbitMqActivity1.Prefetch = "2";
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Prefetch_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Prefetch = "2";
            rabbitMqActivity1.Prefetch = "2";
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TimeOut_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1"};
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , Prefetch = "1"};
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.TimeOut = "1";
            rabbitMqActivity1.TimeOut = "2";
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TimeOut_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1" };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.TimeOut = "2";
            rabbitMqActivity1.TimeOut = "2";
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsObject_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1"};
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , Prefetch = "1", TimeOut = "1"};
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsObject = true;
            rabbitMqActivity1.IsObject =false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsObject_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1" };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsObject = true;
            rabbitMqActivity1.IsObject = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Acknowledge_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Acknowledge = true;
            rabbitMqActivity1.Acknowledge = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Acknowledge_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true};
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.Acknowledge = true;
            rabbitMqActivity1.Acknowledge = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReQueue_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.ReQueue = true;
            rabbitMqActivity1.ReQueue = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReQueue_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true};
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.ReQueue = true;
            rabbitMqActivity1.ReQueue = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RabbitSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.RabbitSource = new RabbitMQSource(){ResourceID = Guid.NewGuid()};
            rabbitMqActivity1.RabbitSource = new RabbitMQSource();
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RabbitSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true};
            var rabbitMqActivity1 = new DsfConsumeRabbitMQActivity() { UniqueID = uniqueId, Result = "A", Prefetch = "1", TimeOut = "1", IsObject = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.RabbitSource = new RabbitMQSource();
            rabbitMqActivity1.RabbitSource = new RabbitMQSource();
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
    }
}
