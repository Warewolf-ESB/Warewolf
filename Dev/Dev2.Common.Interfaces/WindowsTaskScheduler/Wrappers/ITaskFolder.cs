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
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface ITaskFolder : IDisposable, IWrappedObject<TaskFolder>
    {
        /// <summary>
        ///     Gets the name that is used to identify the folder that contains a task.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the path to where the folder is stored.
        /// </summary>
        string Path { get; }


        /// <summary>
        ///     Gets a collection of all the tasks in the folder.
        /// </summary>
        ITaskCollection Tasks { get; }


        /// <summary>
        ///     Creates a folder for related tasks. Not available to Task Scheduler 1.0.
        /// </summary>
        /// <param name="subFolderName">
        ///     The name used to identify the folder. If "FolderName\SubFolder1\SubFolder2" is specified, the entire folder tree
        ///     will be created if the folders do not exist. This parameter can be a relative path to the current
        ///     <see
        ///         cref="TaskFolder" />
        ///     instance. The root task folder is specified with a backslash (\). An example of a task folder path, under the root
        ///     task folder, is \MyTaskFolder. The '.' character cannot be used to specify the current task folder and the '..'
        ///     characters cannot be used to specify the parent task folder in the path.
        /// </param>
        /// <param name="sddlForm">The security descriptor associated with the folder.</param>
        /// <returns>
        ///     A <see cref="TaskFolder" /> instance that represents the new subfolder.
        /// </returns>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        ITaskFolder CreateFolder(string subFolderName, string sddlForm = null);

        /// <summary>
        ///     Deletes a task from the folder.
        /// </summary>
        /// <param name="Name">
        ///     The name of the task that is specified when the task was registered. The '.' character cannot be
        ///     used to specify the current task folder and the '..' characters cannot be used to specify the parent task folder in
        ///     the path.
        /// </param>
        /// <param name="exceptionOnNotExists">
        ///     Set this value to false to avoid having an exception called if the task does not
        ///     exist.
        /// </param>
        void DeleteTask(string Name, bool exceptionOnNotExists = true);


        /// <summary>
        ///     Registers (creates) a task in a specified location using a <see cref="TaskDefinition" /> instance to define a task.
        /// </summary>
        /// <param name="Path">
        ///     The task name. If this value is NULL, the task will be registered in the root task folder and the
        ///     task name will be a GUID value that is created by the Task Scheduler service. A task name cannot begin or end with
        ///     a space character. The '.' character cannot be used to specify the current task folder and the '..' characters
        ///     cannot be used to specify the parent task folder in the path.
        /// </param>
        /// <param name="definition">
        ///     The <see cref="TaskDefinition" /> of the registered task.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" /> instance that represents the new task.
        /// </returns>
        IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition);

        /// <summary>
        ///     Registers (creates) a task in a specified location using a <see cref="TaskDefinition" /> instance to define a task.
        /// </summary>
        /// <param name="Path">
        ///     The task name. If this value is NULL, the task will be registered in the root task folder and the
        ///     task name will be a GUID value that is created by the Task Scheduler service. A task name cannot begin or end with
        ///     a space character. The '.' character cannot be used to specify the current task folder and the '..' characters
        ///     cannot be used to specify the parent task folder in the path.
        /// </param>
        /// <param name="definition">
        ///     The <see cref="TaskDefinition" /> of the registered task.
        /// </param>
        /// <param name="createType">
        ///     A union of <see cref="TaskCreation" /> flags.
        /// </param>
        /// <param name="UserId">The user credentials used to register the task.</param>
        /// <param name="password">The password for the userId used to register the task.</param>
        /// <param name="LogonType">
        ///     A <see cref="TaskLogonType" /> value that defines what logon technique is used to run the registered task.
        /// </param>
        /// <param name="sddl">
        ///     The security descriptor associated with the registered task. You can specify the access control list
        ///     (ACL) in the security descriptor for a task in order to allow or deny certain users and groups access to a task.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" /> instance that represents the new task.
        /// </returns>
        /// <exception cref="NotV1SupportedException">
        ///     This LogonType is not supported on Task Scheduler 1.0.
        ///     or
        ///     Security settings are not available on Task Scheduler 1.0.
        ///     or
        ///     Registration triggers are not available on Task Scheduler 1.0.
        ///     or
        ///     Xml validation not available on Task Scheduler 1.0.
        /// </exception>
        IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType,
            string UserId, string password = null,
            TaskLogonType LogonType = TaskLogonType.S4U, string sddl = null);

        #region additional methods and properties

        IList<IDev2Task> ValidTasks { get; }

        bool TaskExists(string name);

        #endregion
    }
}