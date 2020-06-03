using System;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.ExtMethods;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.RabbitMQ
{
    [TestClass]
    public class PublishRabbitMqActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_UniqueIDEquals_EmptyPublishRabbitMQ_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(rabbitMqActivity);
            //---------------Execute Test ----------------------
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_UniqueIDDifferent_EmptyPublishRabbitMQ_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity();
            var rabbitMqActivity1 = new PublishRabbitMQActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(rabbitMqActivity);
            //---------------Execute Test ----------------------
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_QueueName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" , QueueName = "a"};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "", QueueName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_QueueName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "A" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_QueueName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Message_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" , QueueName = "a", Message = "klkl"};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "", QueueName = "a", Message = "klkl"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Message_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass", Message = "klkl" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", QueueName = "ass", Message = "klklkkkkk" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Message_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA", Message = "KLKL" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", QueueName = "AAA", Message = "klkl" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_ObjectName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", DisplayName = ""};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a" , DisplayName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", DisplayName = "A"};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , DisplayName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_RabbitMQSourceResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "", RabbitMQSourceResourceId = Guid.Empty};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "", RabbitMQSourceResourceId = Guid.Empty };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_RabbitMQSourceResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a", DisplayName = "a", RabbitMQSourceResourceId = Guid.NewGuid()};
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "a" , DisplayName = "a", RabbitMQSourceResourceId = Guid.NewGuid()};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_RabbitMQSourceResourceId_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", DisplayName = "A", RabbitMQSourceResourceId = Guid.Empty };
            var multiAssign1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , DisplayName = "a", RabbitMQSourceResourceId = Guid.Empty.ToString().ToLower().ToGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_Equals_Given_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.DisplayName = "2";
            rabbitMqActivity1.DisplayName = "2";
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
       

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsDurable_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsDurable = true;
            rabbitMqActivity1.IsDurable = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsDurable_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A"};
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsDurable = true;
            rabbitMqActivity1.IsDurable = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsExclusive_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A"  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsExclusive = true;
            rabbitMqActivity1.IsExclusive = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsExclusive_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A",};
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsExclusive = true;
            rabbitMqActivity1.IsExclusive = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsAutoDelete_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A"  };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsAutoDelete = true;
            rabbitMqActivity1.IsAutoDelete = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_IsAutoDelete_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A"};
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.IsAutoDelete = true;
            rabbitMqActivity1.IsAutoDelete = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_RabbitSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A" , };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.RabbitMQSource = new RabbitMQSource(){ResourceID = Guid.NewGuid()};
            rabbitMqActivity1.RabbitMQSource = new RabbitMQSource();
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("PublishRabbitMqActivityComparer")]
        public void PublishRabbitMqActivityComparer_RabbitSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A",};
            var rabbitMqActivity1 = new PublishRabbitMQActivity() { UniqueID = uniqueId, Result = "A",};
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.RabbitMQSource = new RabbitMQSource();
            rabbitMqActivity1.RabbitMQSource = new RabbitMQSource();
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}