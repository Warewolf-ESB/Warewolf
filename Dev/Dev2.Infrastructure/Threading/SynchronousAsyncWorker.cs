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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;

namespace Dev2.Threading
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SynchronousAsyncWorker : IAsyncWorker
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
        /// calls an error handler should an exception occur
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <param name="onError"></param>
        public Task Start(Action backgroundAction, Action uiAction, Action<Exception> onError)
        {
            var task = new Task(() =>
            {
                try
                {
                    backgroundAction.Invoke();
                    uiAction.Invoke();
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    onError(e);
                    
                }
            });
            task.RunSynchronously();
            return task;
        }

        public readonly List<Exception> Exceptions = new List<Exception>();
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
            var task = new Task(() =>
            {
                try
                {
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        backgroundAction.Invoke();
                        uiAction.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    onError(e);
                  
                }
            }, cancellationTokenSource.Token);
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
            var task = new Task(backgroundAction.Invoke);
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
            var task = new Task(() =>
            {
                try
                {
                    var result = backgroundFunc.Invoke();
                    uiAction.Invoke(result);
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    onError(e);
                   
                }
            });
            task.RunSynchronously();
            return task;
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
            var task = new Task(() =>
            {
                try
                {
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        var result = backgroundFunc.Invoke();
                        uiAction.Invoke(result);
                    }
                }
                catch (Exception e)
                {
                    Exceptions.Add(e);
                    onError(e);
                  
                }
            }, cancellationTokenSource.Token);
            task.RunSynchronously();
            return task;
        }
    }
}
