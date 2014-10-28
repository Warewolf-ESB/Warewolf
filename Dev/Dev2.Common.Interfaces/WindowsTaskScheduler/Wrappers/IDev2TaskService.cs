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
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IDev2TaskService : IDisposable, IWrappedObject<TaskService>
    {
        /// <summary>
        ///     Gets a Boolean value that indicates if you are connected to the Task Scheduler service.
        /// </summary>
        [Browsable(false)]
        bool Connected { get; }

        /// <summary>
        ///     Gets the root ("\") folder. For Task Scheduler 1.0, this is the only folder.
        /// </summary>
        [Browsable(false)]
        ITaskFolder RootFolder { get; }

        /// <summary>
        ///     Gets the path to a folder of registered tasks.
        /// </summary>
        /// <param name="folderName">
        ///     The path to the folder to retrieve. Do not use a backslash following the last folder name in
        ///     the path. The root task folder is specified with a backslash (\). An example of a task folder path, under the root
        ///     task folder, is \MyTaskFolder. The '.' character cannot be used to specify the current task folder and the '..'
        ///     characters cannot be used to specify the parent task folder in the path.
        /// </param>
        /// <returns>
        ///     <see cref="TaskFolder" /> instance for the requested folder.
        /// </returns>
        /// <exception cref="Exception">Requested folder was not found.</exception>
        /// <exception cref="NotV1SupportedException">
        ///     Folder other than the root (\) was requested on a system not supporting Task
        ///     Scheduler 2.0.
        /// </exception>
        ITaskFolder GetFolder(string folderName);


        /// <summary>
        ///     Gets the task with the specified path.
        /// </summary>
        /// <param name="taskPath">The task path.</param>
        /// <returns>The task.</returns>
        IDev2Task GetTask(string taskPath);

        /// <summary>
        ///     Returns an empty task definition object to be filled in with settings and properties and then registered using the
        ///     <see
        ///         cref="TaskFolder.RegisterTaskDefinition(string, TaskDefinition)" />
        ///     method.
        /// </summary>
        /// <returns>
        ///     A <see cref="TaskDefinition" /> instance for setting properties.
        /// </returns>
        IDev2TaskDefinition NewTask();
    }
}