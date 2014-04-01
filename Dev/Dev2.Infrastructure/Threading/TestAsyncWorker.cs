using System;
using System.Threading.Tasks;

namespace Dev2.Threading
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

        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/08</date>
        public Task Start(Action backgroundAction)
        {
            return null;
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