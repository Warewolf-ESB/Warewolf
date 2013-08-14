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
    }
}
