using System;
using Microsoft.Win32.TaskScheduler;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
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

        IAction CreateAction(Action action);

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