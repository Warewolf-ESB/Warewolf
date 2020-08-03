#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;

namespace Dev2.Scheduler
{
    public class ScheduledResourceModel : IScheduledResourceModel
    {
        readonly string _debugHistoryPath;
        readonly ISecurityWrapper _securityWrapper;
        readonly ITaskServiceConvertorFactory _factory;
        readonly IDev2TaskService _taskService;
        readonly string _warewolfAgentPath;
        readonly string _warewolfFolderPath;
        readonly IFile _fileWrapper;
        readonly IDirectory _directory;
        readonly IDictionary<int, string> _taskStates;
        readonly Func<IScheduledResource, string> _pathResolve;
        const string Sebatchlogonright = "SeBatchLogonRight";
        const char NameSeperator = ':';
        const char ArgWrapper = '"';

        public ScheduledResourceModel(IDev2TaskService taskService, string warewolfFolderId, string warewolfAgentPath, ITaskServiceConvertorFactory taskServiceFactory, string debugHistoryPath, ISecurityWrapper securityWrapper, Func<IScheduledResource, string> pathResolve)
            : this(taskService, warewolfFolderId, warewolfAgentPath, taskServiceFactory, debugHistoryPath, securityWrapper, pathResolve, new FileWrapper(), new DirectoryWrapper())
        {
        }
        public ScheduledResourceModel(IDev2TaskService taskService, string warewolfFolderId, string warewolfAgentPath, ITaskServiceConvertorFactory taskServiceFactory, string debugHistoryPath, ISecurityWrapper securityWrapper, Func<IScheduledResource, string> pathResolve, IFile file, IDirectory directory)
        {
            _fileWrapper = file;
            _directory = directory;

            var nullables = new Dictionary<string, object>
                {
                    {"taskService", taskService},
                    {"warewolfFolderId", warewolfFolderId},
                    {"warewolfAgentPath", warewolfAgentPath},
                    {"taskServiceFactory", taskServiceFactory},
                    {"debugHistoryPath", debugHistoryPath},
                    {"securityWrapper", securityWrapper}
                };
            VerifyArgument.AreNotNull(nullables);

            _taskStates = new Dictionary<int, string>
                {
                    {102, "Task Completed"},
                    {100, "Task Started"},
                    {101, "Failed To Start"},
                    {103, "Job Failed"},
                    {104, "Logon Failed"}
                };
            _taskService = taskService;
            _warewolfFolderPath = warewolfFolderId;
            _warewolfAgentPath = warewolfAgentPath;
            _factory = taskServiceFactory;
            _debugHistoryPath = debugHistoryPath;
            _securityWrapper = securityWrapper;
            _pathResolve = pathResolve;
        }

        public IDev2TaskService TaskService => _taskService;

        public string WarewolfFolderPath => _warewolfFolderPath;

        public string WarewolfAgentPath => _warewolfAgentPath;

        public ITaskServiceConvertorFactory ConvertorFactory => _factory;

        public void DeleteSchedule(IScheduledResource resource)
        {
            var folder = TaskService.GetFolder(WarewolfFolderPath);
            if (folder.TaskExists(resource.Name))
            {
                folder.DeleteTask(resource.Name, false);
            }
        }

        public bool Save(IScheduledResource resource, out string errorMessage)
        {
            try
            {
                Save(resource, resource.UserName, resource.Password);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            errorMessage = "";
            return true;
        }

        public void Save(IScheduledResource resource, string userName, string password)
        {
            if (!_securityWrapper.IsWindowsAuthorised(Sebatchlogonright, userName))
            {
                throw new SecurityException(Warewolf.Studio.Resources.Languages.Core.SchedulerLogOnAsBatchError);
            }
            if (!_securityWrapper.IsWarewolfAuthorised(Sebatchlogonright, userName, resource.ResourceId))
            {
                throw new SecurityException(String.Format(Warewolf.Studio.Resources.Languages.Core.SchedulerExecutePermissionError, resource.WorkflowName));
            }
            if (resource.Name.Any(a => "\\/:*?\"<>|".Contains(a)))
            {
                throw new Exception(Warewolf.Studio.Resources.Languages.Core.SchedulerInvalidCharactersError + " \\/:*?\"<>| .");
            }

            var folder = TaskService.GetFolder(WarewolfFolderPath);
            var created = CreateNewTask(resource);
            created.Settings.Enabled = resource.Status == SchedulerStatus.Enabled;
            folder.RegisterTaskDefinition(resource.Name, created, TaskCreation.CreateOrUpdate, userName, password, TaskLogonType.InteractiveTokenOrPassword);
        }

        public ObservableCollection<IScheduledResource> ScheduledResources
        {
            get { return GetScheduledResources(); }
            set { throw new NotImplementedException(); }
        }

        public string DebugHistoryPath => _debugHistoryPath;

        public ObservableCollection<IScheduledResource> GetScheduledResources()
        {
            try
            {
                var folder = TaskService.GetFolder(WarewolfFolderPath);
                var allTasks = folder.ValidTasks; // we have the tasks with at least one action
                return allTasks.Where(a => a.IsValidDev2Task()).Select(TryCreateScheduledResource).ToObservableCollection();
            }
            catch (FileNotFoundException)
            {
                // if the folder does not exist we should create it
                TaskService.RootFolder.CreateFolder(WarewolfFolderPath);
                return new ObservableCollection<IScheduledResource>();
            }
        }

        IExecAction BuildAction(IScheduledResource resource) => ConvertorFactory.CreateExecAction(WarewolfAgentPath,
                $"\"Workflow:{resource.WorkflowName.Trim()}\" \"TaskName:{resource.Name.Trim()}\" \"ResourceId:{resource.ResourceId}\"");

        IScheduledResource TryCreateScheduledResource(IDev2Task arg)
        {
            ITrigger trigger;
            DateTime nextDate;
            List<string> output;
            using (var action = ConvertorFactory.CreateExecAction(arg.Action))
            {
                trigger = arg.Trigger;
                nextDate = trigger.StartBoundary;
                output = action.Arguments.Split(ArgWrapper).Where(a => !String.IsNullOrEmpty(a.Trim())).ToList();
            }
            if (output.Count == ArgCount && output.All(a => a.Contains(NameSeperator)))
            {
                var split = output.SelectMany(a => a.Split(NameSeperator)).ToList();
                try
                {
                    return CreateScheduledResource(arg, trigger, nextDate, split);
                }
                finally
                {
                    arg.Dispose();
                }
            }
            if (output.Count == ArgCountOld && output.All(a => a.Contains(NameSeperator)))
            {
                var split = output.SelectMany(a => a.Split(NameSeperator)).ToList();
                try
                {
                    return new ScheduledResource(arg.Definition.Data,
                                                 arg.Definition.Settings.Enabled ? SchedulerStatus.Enabled : SchedulerStatus.Disabled,
                                                 nextDate,
                                                 new ScheduleTrigger(arg.State, _factory.SanitiseTrigger(trigger),
                                                                     _taskService, _factory), split[1], Guid.Empty.ToString())
                    {
                        Status = arg.Definition.Settings.Enabled ? SchedulerStatus.Enabled : SchedulerStatus.Disabled,
                        RunAsapIfScheduleMissed = arg.Definition.Settings.StartWhenAvailable,
                        UserName = arg.Definition.UserName
                    };
                }
                finally
                {
                    arg.Dispose();
                }
            }

            throw new Exception($"Invalid resource found:{arg.Definition.Data}"); // this should not be reachable because isvaliddev2task checks same conditions
        }

        private IScheduledResource CreateScheduledResource(IDev2Task arg, ITrigger trigger, DateTime nextDate, List<string> split)
        {
            var id = split[5];
            Guid.TryParse(id, out Guid resourceId);

            var res = new ScheduledResource(arg.Definition.Data,
                                         arg.Definition.Settings.Enabled ? SchedulerStatus.Enabled : SchedulerStatus.Disabled,
                                         nextDate,
                                         new ScheduleTrigger(arg.State, _factory.SanitiseTrigger(trigger),
                                                             _taskService, _factory), split[1], split[5])
            {
                Status = arg.Definition.Settings.Enabled ? SchedulerStatus.Enabled : SchedulerStatus.Disabled,
                RunAsapIfScheduleMissed = arg.Definition.Settings.StartWhenAvailable,
                UserName = arg.Definition.UserName,

            };

            string resWorkflowName;
            if (resourceId == Guid.Empty)
            {
                resWorkflowName = split[1];
            }
            else
            {
                try
                {
                    resWorkflowName = _pathResolve(res);
                }
                catch (NullReferenceException)
                {
                    resWorkflowName = split[1];
                    res.Errors.AddError($"Workflow: {resWorkflowName} not found. Task is invalid.");
                }
            }
            res.WorkflowName = resWorkflowName;

            return res;
        }

        public int ArgCount => 3;

        public int ArgCountOld => 2;

        public IList<IResourceHistory> CreateHistory(IScheduledResource resource)
        {
            var evt = _factory.CreateTaskEventLog($"\\{_warewolfFolderPath}\\" + resource.Name);
            var groupings = from a in evt.Where(x => !string.IsNullOrEmpty(x.Correlation)
                            && !string.IsNullOrEmpty(x.TaskCategory) && _taskStates.Values.Contains(x.TaskCategory))
                            group a by a.Correlation into corrGroup
                            select new
                            {
                                StartDate = corrGroup.Min(a => a.TimeCreated),
                                EndDate = corrGroup.Max(a => a.TimeCreated),
                                EventId = corrGroup.Max(a => a.EventId),
                                corrGroup.Key
                            };
            // for each grouping get the data and debug output
            IList<IResourceHistory> eventList = groupings.OrderBy(a => a.StartDate).Reverse()
                .Take(resource.NumberOfHistoryToKeep == 0 ? int.MaxValue : resource.NumberOfHistoryToKeep)
                .Select(a =>
                {
                    TimeSpan duration;
                    DateTime start;
                    DateTime end;
                    var debugOutput = CreateDebugHistory(DebugHistoryPath, a.Key);
                    var output = debugOutput.FirstOrDefault();
                    if (output != null)
                    {
                        duration = new TimeSpan(output.Duration.Hours, output.Duration.Minutes, output.Duration.Seconds);
                        start = output.StartTime;
                        end = output.EndTime;
                    }
                    else
                    {
                        start = a.StartDate.Value;
                        end = a.EndDate.Value;
                        duration = a.StartDate.HasValue && a.EndDate.HasValue ? a.EndDate.Value.Subtract(a.StartDate.Value) : TimeSpan.MaxValue;
                    }
                    return new ResourceHistory("", debugOutput,
                        new EventInfo(start, duration, end, GetRunStatus(a.EventId, DebugHistoryPath, a.Key), a.Key, a.EventId < 103 ? "" : _taskStates[a.EventId]), GetUserName(DebugHistoryPath, a.Key))
                        as IResourceHistory;
                }).ToList();
            var result = eventList.Where(history => history.TaskHistoryOutput.Success != ScheduleRunStatus.Unknown).ToList();
            return result;
        }

        ScheduleRunStatus GetRunStatus(int eventId, string debugHistoryPath, string key)
        {
            var debugExists = DebugHistoryExists(debugHistoryPath, key);
            var debugHasErrors = DebugHasErrors(debugHistoryPath, key);
            var winSuccess = eventId < 103;
            if (debugExists && !debugHasErrors && winSuccess)
            {
                return ScheduleRunStatus.Success;
            }

            if (!debugExists)
            {
                return ScheduleRunStatus.Unknown;
            }

            return ScheduleRunStatus.Error;
        }

        bool DebugHasErrors(string debugHistoryPath, string correlationId)
        {
            var serializer = new Dev2JsonSerializer();
            var file = _directory.GetFiles(debugHistoryPath).FirstOrDefault(a => a.Contains(correlationId));

            if (file == null)
            {
                return false;
            }

            return serializer.Deserialize<List<IDebugState>>(_fileWrapper.ReadAllText(file)).Last().HasError;
        }

        bool DebugHistoryExists(string debugHistoryPath, string correlationId) => _directory.GetFiles(debugHistoryPath).FirstOrDefault(a => a.Contains(correlationId)) != null;

        string GetUserName(string debugHistoryPath, string correlationId)
        {
            var file = _directory.GetFiles(debugHistoryPath).FirstOrDefault(a => a.Contains(correlationId));
            if (file != null)
            {
                return file.Split('_').Last();
            }

            return "";
        }

        IList<IDebugState> CreateDebugHistory(string debugHistoryPath, string correlationId)
        {
            var serializer = new Dev2JsonSerializer();
            var file = _directory.GetFiles(debugHistoryPath).FirstOrDefault(a => a.Contains(correlationId));

            if (file == null)
            {
                return new List<IDebugState>();
            }

            return serializer.Deserialize<List<IDebugState>>(_fileWrapper.ReadAllText(file));
        }

        IDev2TaskDefinition CreateNewTask(IScheduledResource resource)
        {
            var created = TaskService.NewTask();
            created.Data = $"{resource.Name}~{resource.NumberOfHistoryToKeep}";

            var trigger = _factory.SanitiseTrigger(resource.Trigger.Trigger);

            created.AddTrigger(trigger);
            created.AddAction(BuildAction(resource));
            created.Settings.StartWhenAvailable = resource.RunAsapIfScheduleMissed;
            created.Settings.MultipleInstances = resource.AllowMultipleIstances ? TaskInstancesPolicy.Parallel : TaskInstancesPolicy.Queue;
            created.Settings.Hidden = true;
            if (created.Instance?.Principal != null)
            {
                created.Instance.Principal.RunLevel = TaskRunLevel.Highest;
            }
            return created;
        }

        public void Dispose()
        {

        }
    }
}
