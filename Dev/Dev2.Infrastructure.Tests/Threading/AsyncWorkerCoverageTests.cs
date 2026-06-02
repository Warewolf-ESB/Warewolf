/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*/
using System;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Threading
{
    [TestClass]
    public class SynchronousAsyncWorkerCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(SynchronousAsyncWorker);

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_BothInvoked()
        {
            var worker = new SynchronousAsyncWorker();
            var bg = 0; var ui = 0;
            var task = worker.Start(() => bg++, () => ui++);
            task.Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_OnError_NoException_BothInvoked()
        {
            var worker = new SynchronousAsyncWorker();
            var bg = 0; var ui = 0; Exception caught = null;
            worker.Start(() => bg++, () => ui++, e => caught = e).Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
            Assert.IsNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_OnError_BgThrows_OnErrorCalled()
        {
            var worker = new SynchronousAsyncWorker();
            Exception caught = null;
            worker.Start(() => throw new InvalidOperationException("boom"),
                         () => { },
                         e => caught = e).Wait();
            Assert.IsNotNull(caught);
            Assert.IsInstanceOfType(caught, typeof(InvalidOperationException));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_Cancellation_NotCancelled_Runs()
        {
            var worker = new SynchronousAsyncWorker();
            var cts = new CancellationTokenSource();
            var bg = 0; var ui = 0;
            worker.Start(() => bg++, () => ui++, cts, _ => { }).Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_Cancellation_AlreadyCancelled_Skipped()
        {
            var worker = new SynchronousAsyncWorker();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var bg = 0; var ui = 0;
            try { worker.Start(() => bg++, () => ui++, cts, _ => { }).Wait(); }
            catch { /* RunSynchronously may throw with a pre-cancelled token */ }
            Assert.AreEqual(0, bg);
            Assert.AreEqual(0, ui);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_Only_Runs()
        {
            var worker = new SynchronousAsyncWorker();
            var bg = 0;
            worker.Start(() => bg++).Wait();
            Assert.AreEqual(1, bg);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_RoundTripsResult()
        {
            var worker = new SynchronousAsyncWorker();
            int received = 0;
            worker.Start<int>(() => 42, r => received = r).Wait();
            Assert.AreEqual(42, received);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_OnError_BgThrows_CallsOnError()
        {
            var worker = new SynchronousAsyncWorker();
            Exception caught = null;
            worker.Start<int>(() => throw new InvalidOperationException("nope"),
                              r => { },
                              e => caught = e).Wait();
            Assert.IsNotNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_OnError_NoThrow_CallsUI()
        {
            var worker = new SynchronousAsyncWorker();
            int received = 0; Exception caught = null;
            worker.Start<int>(() => 7, r => received = r, e => caught = e).Wait();
            Assert.AreEqual(7, received);
            Assert.IsNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_Cancellation_NotCancelled_Runs()
        {
            var worker = new SynchronousAsyncWorker();
            var cts = new CancellationTokenSource();
            int received = 0;
            worker.Start<int>(() => 9, r => received = r, cts, _ => { }).Wait();
            Assert.AreEqual(9, received);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_Cancellation_AlreadyCancelled_Skipped()
        {
            var worker = new SynchronousAsyncWorker();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            int received = 0;
            try { worker.Start<int>(() => 1, r => received = r, cts, _ => { }).Wait(); }
            catch { }
            Assert.AreEqual(0, received);
        }
    }

    [TestClass]
    public class AsyncWorkerCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(AsyncWorker);

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_BothInvoked()
        {
            var worker = new AsyncWorker();
            var bg = 0; var ui = 0;
            worker.Start(() => Interlocked.Increment(ref bg),
                         () => Interlocked.Increment(ref ui)).Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_Only_Runs()
        {
            var worker = new AsyncWorker();
            var bg = 0;
            worker.Start(() => Interlocked.Increment(ref bg)).Wait();
            Assert.AreEqual(1, bg);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_OnError_NoThrow_CallsUI()
        {
            var worker = new AsyncWorker();
            var bg = 0; var ui = 0; Exception caught = null;
            worker.Start(() => Interlocked.Increment(ref bg),
                         () => Interlocked.Increment(ref ui),
                         e => caught = e).Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
            Assert.IsNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_OnError_BgThrows_CallsOnError()
        {
            var worker = new AsyncWorker();
            Exception caught = null;
            worker.Start(() => throw new InvalidOperationException("x"),
                         () => { },
                         e => caught = e).Wait();
            Assert.IsNotNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Action_UI_Cancellation_NotCancelled_Runs()
        {
            var worker = new AsyncWorker();
            var cts = new CancellationTokenSource();
            var bg = 0; var ui = 0;
            worker.Start(() => Interlocked.Increment(ref bg),
                         () => Interlocked.Increment(ref ui),
                         cts, _ => { }).Wait();
            Assert.AreEqual(1, bg);
            Assert.AreEqual(1, ui);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_RoundTripsResult()
        {
            var worker = new AsyncWorker();
            int received = 0;
            worker.Start<int>(() => 99, r => received = r).Wait();
            Assert.AreEqual(99, received);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_OnError_NoThrow_CallsUI()
        {
            var worker = new AsyncWorker();
            int received = 0; Exception caught = null;
            worker.Start<int>(() => 5, r => received = r, e => caught = e).Wait();
            Assert.AreEqual(5, received);
            Assert.IsNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_OnError_BgThrows_CallsOnError()
        {
            var worker = new AsyncWorker();
            Exception caught = null;
            worker.Start<int>(() => throw new InvalidOperationException("y"),
                              r => { }, e => caught = e).Wait();
            Assert.IsNotNull(caught);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Start_Generic_Func_UI_Cancellation_NotCancelled_Runs()
        {
            var worker = new AsyncWorker();
            var cts = new CancellationTokenSource();
            int received = 0;
            worker.Start<int>(() => 11, r => received = r, cts, _ => { }).Wait();
            Assert.AreEqual(11, received);
        }
    }
}
