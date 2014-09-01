using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler
{
    public class ClientSchedulerFactory : IClientSchedulerFactory
    {
        readonly IDev2TaskService _service;
        readonly ITaskServiceConvertorFactory _serviceConvertorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ClientSchedulerFactory(IDev2TaskService service, ITaskServiceConvertorFactory serviceConvertorFactory)
        {
            _service = service;
            _serviceConvertorFactory = serviceConvertorFactory;
        }



        public IScheduleTrigger CreateTrigger(TaskState state, ITrigger trigger)
        {
            return new ScheduleTrigger(state, trigger, _service, _serviceConvertorFactory);
        }


        public IScheduleTrigger CreateTrigger(Trigger trigger)
        {
            switch(trigger.TriggerType)
            {
                case TaskTriggerType.Boot:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2BootTrigger(_serviceConvertorFactory, trigger as BootTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Daily:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2DailyTrigger(_serviceConvertorFactory, trigger as DailyTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Event:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2EventTrigger(_serviceConvertorFactory, trigger as EventTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Idle:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2IdleTrigger(_serviceConvertorFactory, trigger as IdleTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Logon:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2LogonTrigger(_serviceConvertorFactory, trigger as LogonTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Monthly:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2MonthlyTrigger(_serviceConvertorFactory, trigger as MonthlyTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.MonthlyDOW:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2MonthlyDowTrigger(_serviceConvertorFactory, trigger as MonthlyDOWTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Registration:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2RegistrationTrigger(_serviceConvertorFactory, trigger as RegistrationTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.SessionStateChange:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2Trigger(_serviceConvertorFactory, trigger as SessionStateChangeTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Time:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2TimeTrigger(_serviceConvertorFactory, trigger as TimeTrigger), _service, _serviceConvertorFactory);
                case TaskTriggerType.Weekly:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2WeeklyTrigger(_serviceConvertorFactory, trigger), _service, _serviceConvertorFactory);
                default:
                    return new ScheduleTrigger(TaskState.Ready, new Dev2Trigger(_serviceConvertorFactory, trigger), _service, _serviceConvertorFactory);

            }
        }

    }
}