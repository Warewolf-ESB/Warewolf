
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IDev2Task : IWrappedObject<Task>, IDisposable
    {
        /// <summary>
        ///     Gets the definition of the task.
        /// </summary>
        IDev2TaskDefinition Definition { get; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates if the registered task is enabled.
        /// </summary>
        /// <remarks>
        ///     As of version 1.8.1, under V1 systems (prior to Vista), this method will immediately set the enabled property and re-save the current task. If changes have been made to the
        ///     <see
        ///         cref="TaskDefinition" />
        ///     , then those changes will be saved.
        /// </remarks>
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this task instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this task instance is active; otherwise, <c>false</c>.
        /// </value>
        bool IsActive { get; }

        /// <summary>
        ///     Gets the time the registered task was last run.
        /// </summary>
        /// <value>
        ///     Returns <see cref="DateTime.MinValue" /> if there are no prior run times.
        /// </value>
        DateTime LastRunTime { get; }

        /// <summary>
        ///     Gets the results that were returned the last time the registered task was run.
        /// </summary>
        int LastTaskResult { get; }

        /// <summary>
        ///     Gets the name of the registered task.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the time when the registered task is next scheduled to run.
        /// </summary>
        /// <value>
        ///     Returns <see cref="DateTime.MinValue" /> if there are no future run times.
        /// </value>
        /// <remarks>
        ///     Potentially breaking change in release 1.8.2. For Task Scheduler 2.0, the return value prior to 1.8.2 would be Dec 30, 1899
        ///     if there were no future run times. For 1.0, that value would have been <c>DateTime.MinValue</c>. In release 1.8.2 and later, all
        ///     versions will return <c>DateTime.MinValue</c> if there are no future run times. While this is different from the native 2.0
        ///     library, it was deemed more appropriate to have consistency between the two libraries and with other .NET libraries.
        /// </remarks>
        DateTime NextRunTime { get; }

        /// <summary>
        ///     Gets the number of times the registered task has missed a scheduled run.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        int NumberOfMissedRuns { get; }

        /// <summary>
        ///     Gets the path to where the registered task is stored.
        /// </summary>
        string Path { get; }

        /// <summary>
        ///     Gets the operational state of the registered task.
        /// </summary>
        TaskState State { get; }

        /// <summary>
        ///     Updates the task with any changes made to the <see cref="Definition" /> by calling
        ///     <see
        ///         cref="TaskFolder.RegisterTaskDefinition(string, TaskDefinition)" />
        ///     from the currently registered folder using the currently registered name.
        /// </summary>
        /// <exception cref="System.Security.SecurityException">Thrown if task was previously registered with a password.</exception>
        void RegisterChanges();

        #region Additional methods

        IAction Action { get; }

        ITrigger Trigger { get; }
        bool IsValidDev2Task();

        #endregion
    }
}
