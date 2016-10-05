/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Permissions;
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
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void WaitWithPumping(this Task task, int millisecondsTimeout)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var frame = new DispatcherFrame();
            task.ContinueWith(_ => frame.Continue = true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
            task.Wait(millisecondsTimeout);
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}