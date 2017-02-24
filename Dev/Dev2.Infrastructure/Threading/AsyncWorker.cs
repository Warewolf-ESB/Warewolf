/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;

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
            return Task.Run(backgroundAction).ContinueWith(_ => uiAction(), scheduler);
        }

        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// calls an error handler should an exception occur
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <param name="onError"></param>
        public Task Start(Action backgroundAction, Action uiAction, Action<Exception> onError)
        {
            var scheduler = GetTaskScheduler();
            return Task.Run(backgroundAction).ContinueWith(_ =>
            {
                if(_.Exception != null)
                {
                    onError(_.Exception.Flatten());
                }
                else
                {
                    uiAction();
                }
            }, scheduler);
        }

        /// <summary>
        /// Starts the specified background action and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <param name="cancellationTokenSource">Allows the task to be cancelled.</param>
        /// <param name="onError"></param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/08/08</date>
        public Task Start(Action backgroundAction, Action uiAction, CancellationTokenSource cancellationTokenSource, Action<Exception> onError)
        {
            var scheduler = GetTaskScheduler();
            return Task.Run(backgroundAction, cancellationTokenSource.Token).ContinueWith(_ =>
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (_.Exception != null)
                    {
                        onError(_.Exception.Flatten());
                    }
                    else
                    {
                        uiAction();
                    }
                }
            }, scheduler);
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
            return Task.Run(backgroundAction);
        }

        /// <summary>
        /// Starts the specified background function and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundFunc">The background function - returns the result to be processed on the UI thread.</param>
        /// <param name="uiAction">The UI action to be taken on the given background result.</param>
        /// <param name="onError">ACtion to perform if an error occurs during execution of the Task</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/10/12</date>
        public Task Start<TBackgroundResult>(Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> uiAction, Action<Exception> onError)
        {
            var scheduler = GetTaskScheduler();
            return Task.Run(backgroundFunc).ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    onError(task.Exception.Flatten());
                }
                else
                {
                    uiAction(task.Result);
                }
                
            }, scheduler);
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
            return Task.Run(backgroundFunc).ContinueWith(task =>
            {
                uiAction(task.Result);
            }, scheduler);
        }

        /// <summary>
        /// Starts the specified background function and continues with the UI action 
        /// on the thread this was invoked from (typically the UI thread).
        /// </summary>
        /// <param name="backgroundFunc">The background function - returns the result to be processed on the UI thread.</param>
        /// <param name="uiAction">The UI action to be taken on the given background result.</param>
        /// <param name="cancellationTokenSource">Allows the task to be cancelled.</param>
        /// <param name="onError">ACtion to perform if an error occurs during execution of the Task</param>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/10/12</date>
        public Task Start<TBackgroundResult>(Func<TBackgroundResult> backgroundFunc, Action<TBackgroundResult> uiAction, CancellationTokenSource cancellationTokenSource, Action<Exception> onError)
        {
            var scheduler = GetTaskScheduler();
            return Task.Run(backgroundFunc, cancellationTokenSource.Token).ContinueWith(task =>
            {
                if(!cancellationTokenSource.IsCancellationRequested)
                {
                    if (task.Exception != null)
                    {
                        onError(task.Exception.Flatten());
                    }
                    else
                    {
                        uiAction(task.Result);
                    }
                }
            }, scheduler);
        }

        static TaskScheduler GetTaskScheduler()
        {
            // Get the UI thread's context
            // NOTE: System.Threading.SynchronizationContext.Current is null when testing so use default instead
            var context = SynchronizationContext.Current != null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Default;
            return context;
        }
    }
}
