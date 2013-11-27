using System;
using System.Threading.Tasks;
using Dev2.Threading;

namespace Dev2.Activities.Designers.Tests
{
    public class TestAsyncWorker : IAsyncWorker
    {
        public Task Start(Action backgroundAction, Action uiAction)
        {
            var task = new Task(() =>
            {
                backgroundAction.Invoke();
                uiAction.Invoke();
            });
            task.RunSynchronously();
            return task;
        }

        public Task Start<TBackgroundResult>(Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> uiAction)
        {
            var task = new Task(() =>
            {
                var result = backgroundFunc.Invoke();
                uiAction.Invoke(result);
            });
            task.RunSynchronously();
            return task;
        }
    }
}