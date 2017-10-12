using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dev2.Runtime.ESB.Management.Services
{
    internal static class TaskExtentions
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(false);
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task.ConfigureAwait(false);
                }
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}