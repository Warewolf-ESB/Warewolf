using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Dev2.ExtMethods
{
    public static class TaskExtensions
    {
        // see http://blogs.planetsoftware.com.au/paul/archive/2010/12/05/waiting-for-a-task-donrsquot-block-the-main-ui-thread.aspx
        // see http://code.msdn.microsoft.com/ParExtSamples

        /// <summary>Waits for the task to complete execution, pumping in the meantime.</summary>
        /// <param name="task">The task for which to wait.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Infinite (-1) to wait indefinitely.</param>
        /// <remarks>This method is intended for usage with Windows Presentation Foundation.</remarks>
        public static bool WaitWithPumping(this Task task, int millisecondsTimeout = Timeout.Infinite)
        {
            if(task == null)
            {
                throw new ArgumentNullException("task");
            }

            var nestedFrame = new DispatcherFrame();
            task.ContinueWith(_ => nestedFrame.Continue = false);
            Dispatcher.PushFrame(nestedFrame);
            return task.Wait(millisecondsTimeout);
        }
    }
}
