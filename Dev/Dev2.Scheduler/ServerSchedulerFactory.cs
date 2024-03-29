#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler
{
    public class ServerSchedulerFactory : IServerSchedulerFactory
    {
        readonly IDev2TaskService _service;
        readonly ITaskServiceConvertorFactory _factory;
        readonly string _agentPath = string.Format("{0}\\{1}", Environment.CurrentDirectory, GlobalConstants.SchedulerAgentPath);
        readonly string _debugOutputPath = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), GlobalConstants.SchedulerDebugPath);

        readonly IDirectory _dir;
        readonly Func<IScheduledResource, string> _pathResolve;

        public ServerSchedulerFactory(IDev2TaskService service, ITaskServiceConvertorFactory factory, IDirectory directory, Func<IScheduledResource, string> pathResolve)
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
            _pathResolve = pathResolve;
            CreateDir();
        }

        void CreateDir()
        {
            _dir.CreateIfNotExists(_debugOutputPath);
        }
       
        public ServerSchedulerFactory(Func<IScheduledResource, string> pathResolve)
        {
            _pathResolve = pathResolve;
            _factory = new TaskServiceConvertorFactory();
            _service = new Dev2TaskService(ConvertorFactory);
            _dir = new DirectoryWrapper();
            CreateDir();
        }

        public IDev2TaskService TaskService => _service;

        public ITaskServiceConvertorFactory ConvertorFactory => _factory;

        public IScheduledResourceModel CreateModel(string schedulerFolderId, ISecurityWrapper securityWrapper) => new ScheduledResourceModel(TaskService, schedulerFolderId, _agentPath, ConvertorFactory, _debugOutputPath, securityWrapper, _pathResolve);

#pragma warning disable S1541 // Methods and properties should not be too complex
        public IScheduleTrigger CreateTrigger(Trigger trigger)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (trigger.TriggerType)
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
                case TaskTriggerType.Custom:
                    return null;
                default:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2Trigger(ConvertorFactory, trigger), TaskService, ConvertorFactory);

            }
        }


        public IScheduledResource CreateResource(string name, SchedulerStatus status, Trigger trigger,
                                                 string workflowName, string resourceId) => new ScheduledResource(name, status, DateTime.MinValue, CreateTrigger(trigger), workflowName, resourceId);

        public void Dispose()
        {
            _service.Dispose();
        }
    }
}
