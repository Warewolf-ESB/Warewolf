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
    public class WarewolfQueueTests
    {
        IWarewolfQueue _queue;

        [TestInitialize]
        public void init()
        {
            _queue = new WarewolfQueue();
        }
        [TestCleanup]
        public void cleanup()
        {
            _queue.Dispose();
        }

        [TestMethod]
        public void WarewolfQueue_Create_Success()
        {
            Assert.IsNotNull(_queue);
        }

        [TestMethod]
        public void WarewolfQueue_OpenSession_Success()
        {
            using (var session = _queue.OpenSession())
            {

            }
        }

        [TestMethod]
        public void WarewolfQueue_EnqueueDequeue_Success()
        {
            var expected = "test data";
            using (var session = _queue.OpenSession())
            {
                session.Enqueue<string>(expected);
                session.Flush();
            }

            using (var session = _queue.OpenSession())
            {
                var ob = session.Dequeue<string>();
                Assert.AreEqual(ob, expected);
                session.Flush();
            }
        }

        [TestMethod]
        public void WarewolfQueue_Threaded_EnqueueDequeue_FlushShouldDelay_Success()
        {
            var startTime = DateTime.UtcNow;

            const string expected = "test data";

            Exception threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var session = _queue.OpenSession())
                    {
                        string data = null;
                        do
                        {
                            data = session.Dequeue<string>();
                            Thread.Sleep(100);
                        } while (data is null);
                        Assert.AreEqual(data, expected);
                        var startTimeValue = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        Assert.IsTrue(startTimeValue > 1000, "flush does not define enqueue timing");
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

            using (var session = _queue.OpenSession())
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
