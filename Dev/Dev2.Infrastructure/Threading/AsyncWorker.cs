using System;
using System.Threading.Tasks;

namespace Dev2.Threading
{
    public class AsyncWorker : IAsyncWorker
    {
        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/08</date>
        public Task Start(Action backgroundAction, Action uiAction)
        {
            var scheduler = GetTaskScheduler();
            return Task.Factory.StartNew(backgroundAction).ContinueWith(_ => uiAction(), scheduler);
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
            return Task.Factory.StartNew(backgroundAction);
        }

        /// <summary>
        /// Starts the specified background function and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundFunc">The background function - returns the result to be processed on the UI thread.</param>
        /// <param name="uiAction">The UI action to be taken on the given background result.</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/10/12</date>
        public Task Start<TBackgroundResult>(Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> uiAction)
        {
            var scheduler = GetTaskScheduler();
            return Task.Factory.StartNew(backgroundFunc).ContinueWith(task => uiAction(task.Result), scheduler);
        }


        static TaskScheduler GetTaskScheduler()
        {
            // Get the UI thread's context
            // NOTE: System.Threading.SynchronizationContext.Current is null when testing so use default instead
            var context = System.Threading.SynchronizationContext.Current != null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Default;
            return context;
        }
    }
}