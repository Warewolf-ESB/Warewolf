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
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    /// <summary>
    ///     This interface acts as a convertor between native to wrapped objects.
    /// </summary>
    public interface ITaskServiceConvertorFactory
    {
        ITaskFolder CreateRootFolder(TaskFolder taskFolder);

        IDev2Task CreateTask(Task task);

        IDev2TaskDefinition CreateTaskDefinition(TaskDefinition taskDefinition);

        IDev2TaskService CreateTaskService(TaskService taskService);

        IActionCollection CreateActionCollection(ActionCollection actionCollection);

        ITriggerCollection CreateTriggerCollection(TriggerCollection triggerCollection);

        ITaskSettings CreateTaskSettings(TaskSettings taskSettings);

        ITaskCollection CreateTaskCollection(TaskCollection taskCollection);

        ITrigger CreateTrigger(Trigger trigger);

        IRepetitionPattern CreateRepetitionPattern(RepetitionPattern repetitionPattern);

        IAction CreateAction(Microsoft.Win32.TaskScheduler.Action action);

        IDev2TaskService CreateTaskService(string targetServer, string userName, string accountDomain, string password,
            bool forceV1);

        TaskService CreateTaskService();

        IExecAction CreateExecAction(string path, string arguments = null, string workingDirectory = null);

        IExecAction CreateExecAction(IAction act);

        ITaskEvent CreateTaskEvent(TaskEvent currentEvent);

        ITaskEventLog CreateTaskEventLog(string taskPath);

        ITaskEventLog CreateTaskEventLog(string taskPath, DateTime date);

        ITrigger SanitiseTrigger(ITrigger resource);
    }
}