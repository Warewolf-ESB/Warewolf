using System;
using System.Threading.Tasks;
using Dev2.Threading;
using Moq;

namespace Dev2.Core.Tests.Utils
{
    public class AsyncWorkerTests
    {
        public static Mock<IAsyncWorker> CreateSynchronousAsyncWorker()
        {
            var mockWorker = new Mock<IAsyncWorker>();
            mockWorker.Setup(r => r.Start(It.IsAny<Action>(), It.IsAny<Action>()))
                .Returns((Action backgroundAction, Action foregroundAction) =>
                {
                    var task = new Task(() =>
                    {
                        backgroundAction.Invoke();
                        foregroundAction.Invoke();
                    });
                    task.RunSynchronously();
                    return task;
                });
            return mockWorker;
        }

        public static Mock<IAsyncWorker> CreateVerifiableAsyncWorker()
        {
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Action>(), It.IsAny<Action>())).Returns(new Task(() => { })).Verifiable();

            return asyncWorker;
        }

        public static Mock<IAsyncWorker> CreateSynchronousAsyncWorkerWithResult<TBackgroundResult>()
        {
            var mockWorker = new Mock<IAsyncWorker>();
            mockWorker.Setup(r => r.Start(It.IsAny<Func<TBackgroundResult>>(), It.IsAny<Action<TBackgroundResult>>()))
                .Returns((Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> foregroundAction) =>
                {
                    var task = new Task(() =>
                    {
                        var result = backgroundFunc.Invoke();
                        foregroundAction.Invoke(result);
                    });
                    task.RunSynchronously();
                    return task;
                });
            return mockWorker;
        }

        public static Mock<IAsyncWorker> CreateVerifiableAsyncWorkerWithResult<TBackgroundResult>()
        {
            var asyncWorker = new Mock<IAsyncWorker>();
            asyncWorker.Setup(w => w.Start(It.IsAny<Func<TBackgroundResult>>(), It.IsAny<Action<TBackgroundResult>>())).Returns(new Task(() => { })).Verifiable();

            return asyncWorker;
        }
    }
}
