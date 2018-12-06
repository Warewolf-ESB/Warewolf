using System;
using System.Runtime.Serialization;
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

        [DataContract]
        public class BenchmarkObject
        {
            [DataMember]
            public int _number;
            [DataMember]
            public string _word;

            public override bool Equals(Object obj)
            {
                if (obj is BenchmarkObject benchmarkObject)
                {
                    return Equals(benchmarkObject);
                }
                return false;
            }
            public bool Equals(BenchmarkObject other)
            {
                var eq = true;
                eq &= _number == other._number;
                eq &= _word == other._word;
                return eq;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [TestMethod]
        public void WarewolfQueue_Threaded_EnqueueDequeue_FlushShouldDelay_Success()
        {
            var startTime = DateTime.UtcNow;

            var expected = new BenchmarkObject
            {
                _number = 123,
                _word = "test value"
            };

            using (var gate = new ManualResetEvent(false))
            {

                Exception threadException = null;
                var thread = new Thread((Object queueInstance) =>
                {
                    var queue = queueInstance as WarewolfQueue;

                    try
                    {
                        using (var session = queue.OpenSession())
                        {
                            BenchmarkObject data = null;
                            gate.WaitOne();

                            data = session.Dequeue<BenchmarkObject>();
                            Assert.IsNotNull(data);
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

                thread.Start(_queue);

                using (var session = _queue.OpenSession())
                {
                    startTime = DateTime.UtcNow;
                    session.Enqueue(expected);
                    Thread.Sleep(1000);
                    session.Flush();
                    gate.Set();
                }

                thread.Join();
                if (threadException != null)
                {
                    throw threadException;
                }
            }
        }

        [TestMethod]
        public void WarewolfQueue_Threaded_EnqueueDequeue_Benchmark_Success()
        {
            var expected = new BenchmarkObject
            {
                _number = 123,
                _word = "test value"
            };

            Exception threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var session = _queue.OpenSession())
                    {
                        for (var i = 0; i < 100000; i++)
                        {
                            BenchmarkObject data = null;
                            do
                            {
                                data = session.Dequeue<BenchmarkObject>();
                            } while (data is null);
                            Assert.AreEqual(data, expected);
                            session.Flush();
                        }
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
                for (var i = 0; i < 100000; i++)
                {
                    session.Enqueue(expected);
                    session.Flush();
                }
            }

            thread.Join();
            if (threadException != null)
            {
                throw threadException;
            }
        }
    }
}
