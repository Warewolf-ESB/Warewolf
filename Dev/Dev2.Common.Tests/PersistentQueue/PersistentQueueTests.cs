using System;
using System.Text;
using System.Threading;
using Dev2.Common.Container;
using Dev2.Common.Interfaces.Container;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class PersistentQueueTests
    {
        IPersistentQueue queue;

        [TestInitialize]
        public void init()
        {
            queue = new PersistentQueue("persistent_queue_test");
        }
        [TestCleanup]
        public void cleanup()
        {
            queue.Dispose();
            var d = new DirectoryWrapper();
            d.Delete("persistent_queue_test", true);
        }

        [TestMethod]
        public void PersistentQueue_Create_Success()
        {
            Assert.IsNotNull(queue);
        }

        [TestMethod]
        public void PersistentQueue_OpenSession_Success()
        {
            using (var session = queue.OpenSession())
            {

            }
        }

        [TestMethod]
        public void PersistentQueue_EnqueueDequeue_Success()
        {
            var expected = "test data";
            using (var session = queue.OpenSession())
            {
                session.Enqueue<string>(expected);
                session.Flush();
            }

            using (var session = queue.OpenSession())
            {
                var ob = session.Dequeue<string>();
                Assert.AreEqual(ob, expected);
                session.Flush();
            }
        }

        [TestMethod]
        public void PersistentQueue_Threaded_EnqueueDequeue_FlushShouldDelay_Success()
        {
            var startTime = DateTime.UtcNow;

            const string expected = "test data";

            Exception threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var session = queue.OpenSession())
                    {
                        string data = null;
                        do
                        {
                            data = session.Dequeue<string>();
                            Thread.Sleep(100);
                        } while (data is null);
                        Assert.AreEqual(data, expected);
                        var startTimeValue = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        Assert.IsTrue(startTimeValue > 1000);
                        session.Flush();
                    }
                }
                catch (Exception e)
                {
                    threadException = e;
                }
            })
            {
                IsBackground = true
            };

            thread.Start();

            using (var session = queue.OpenSession())
            {
                startTime = DateTime.UtcNow;
                session.Enqueue<string>(expected);
                Thread.Sleep(1000);
                session.Flush();
            }

            thread.Join();
            if (threadException != null)
            {
                throw threadException;
            }
        }
    }
}
