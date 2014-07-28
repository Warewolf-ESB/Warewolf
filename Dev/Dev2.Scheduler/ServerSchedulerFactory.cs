using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Scheduler.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler
{
    public class ServerSchedulerFactory : IServerSchedulerFactory
    {
        private IDev2TaskService _service;
        private ITaskServiceConvertorFactory _factory;
        private readonly string _agentPath = string.Format("{0}\\{1}", Environment.CurrentDirectory, GlobalConstants.SchedulerAgentPath);
        private readonly string _debugOutputPath = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GlobalConstants.SchedulerDebugPath);
  
        private IDirectoryHelper _dir;

        public ServerSchedulerFactory(IDev2TaskService service, ITaskServiceConvertorFactory factory, IDirectoryHelper directory)
        {
            var nullables = new Dictionary<string, object>
                {
                    {"service", service},
                    {"factory", factory},
                    {"directory", directory},

                };
            VerifyArgument.AreNotNull(nullables);
            _service = service;
            _factory = factory;
            _dir = directory;
            CreateDir();
        }

        private void CreateDir()
        {
            _dir.CreateIfNotExists(_debugOutputPath);
        }

        public ServerSchedulerFactory()
        {
            _factory = new TaskServiceConvertorFactory();
            _service = new Dev2TaskService(ConvertorFactory);
            _dir = new DirectoryHelper();
            CreateDir();
        }

        public IDev2TaskService TaskService
        {
            get { return _service; }
        }

        public ITaskServiceConvertorFactory ConvertorFactory
        {
            get { return _factory; }
        }

        public IScheduledResourceModel CreateModel(string schedulerFolderId, ISecurityWrapper securityWrapper)
        {

            return new ScheduledResourceModel(TaskService, schedulerFolderId, _agentPath, ConvertorFactory, _debugOutputPath, securityWrapper);
        }

        public IScheduleTrigger CreateTrigger(Trigger trigger)
        {
            switch(trigger.TriggerType)
            {
                case TaskTriggerType.Boot:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2BootTrigger(ConvertorFactory, trigger as BootTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Daily:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(ConvertorFactory, trigger as DailyTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Event:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2EventTrigger(ConvertorFactory, trigger as EventTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Idle:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2IdleTrigger(ConvertorFactory, trigger as IdleTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Logon:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2LogonTrigger(ConvertorFactory, trigger as LogonTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Monthly:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2MonthlyTrigger(ConvertorFactory, trigger as MonthlyTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.MonthlyDOW:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2MonthlyDowTrigger(ConvertorFactory, trigger as MonthlyDOWTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Registration:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2RegistrationTrigger(ConvertorFactory, trigger as RegistrationTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.SessionStateChange:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2Trigger(ConvertorFactory, trigger as SessionStateChangeTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Time:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2TimeTrigger(ConvertorFactory, trigger as TimeTrigger), TaskService, ConvertorFactory);
                case TaskTriggerType.Weekly:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2WeeklyTrigger(ConvertorFactory, trigger), TaskService, ConvertorFactory);
                default:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2Trigger(ConvertorFactory, trigger), TaskService, ConvertorFactory);

            }
        }

        public IScheduledResource CreateResource(string name, SchedulerStatus status, Trigger trigger,
                                                 string workflowName)
        {
            return new ScheduledResource(name, status, DateTime.MinValue, CreateTrigger(trigger), workflowName);
        }

        public void Dispose()
        {
            _service.Dispose();
        }
    }
}