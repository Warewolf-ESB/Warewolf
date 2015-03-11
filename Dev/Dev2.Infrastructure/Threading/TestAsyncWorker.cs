
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Threading;

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
        /// calls an error handler should an exception occur
        /// </summary>
        /// <param name="backgroundAction">The background action.</param>
        /// <param name="uiAction">The UI action.</param>
        /// <param name="onError"></param>
        public Task Start(Action backgroundAction, Action uiAction, Action<Exception> onError)
        {
            var task = new Task(() =>
            {
                backgroundAction.Invoke();
                uiAction.Invoke();
                onError.Invoke(null);
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
    }
}
