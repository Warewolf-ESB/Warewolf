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
using System.Linq;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class TaskServiceConvertorFactory : ITaskServiceConvertorFactory
    {
        public ITaskFolder CreateRootFolder(TaskFolder taskFolder)
        {
            return new Dev2TaskFolder(this, taskFolder);
        }

        public IDev2Task CreateTask(Task task)
        {
            return new Dev2Task(this, task);
        }

        public IDev2TaskDefinition CreateTaskDefinition(TaskDefinition taskDefinition)
        {
            return new Dev2TaskDefinition(this, taskDefinition);
        }

        public IDev2TaskService CreateTaskService(TaskService taskService)
        {
            return new Dev2TaskService(this, taskService);
        }

        public IActionCollection CreateActionCollection(ActionCollection actionCollection)
        {
            return new Dev2ActionCollection(this, actionCollection);
        }

        public ITaskSettings CreateTaskSettings(TaskSettings taskSettings)
        {
            return new Dev2TaskSettings(taskSettings);
        }

        public ITriggerCollection CreateTriggerCollection(TriggerCollection triggerCollection)
        {
            return new Dev2TriggerCollection(this, triggerCollection);
        }

        public ITaskCollection CreateTaskCollection(TaskCollection taskCollection)
        {
            return new Dev2TaskCollection(this, taskCollection);
        }

        public ITrigger CreateTrigger(Trigger trigger)
        {
            return new Dev2Trigger(this, trigger);
        }

        public IRepetitionPattern CreateRepetitionPattern(RepetitionPattern repetitionPattern)
        {
            return new Dev2RepetitionPattern(repetitionPattern);
        }

        public IAction CreateAction(Microsoft.Win32.TaskScheduler.Action action)
        {
            return new Dev2Action(action);
        }

        public IDev2TaskService CreateTaskService(string targetServer, string userName, string accountDomain,
            string password, bool forceV1)
        {
            return new Dev2TaskService(this,
                new TaskService(targetServer, userName, accountDomain, password, forceV1));
        }


        public IExecAction CreateExecAction(string path, string arguments = null, string workingDirectory = null)
        {
            return new Dev2ExecAction(this, new ExecAction(path, arguments, workingDirectory));
        }

        public IExecAction CreateExecAction(IAction act)
        {
            return new Dev2ExecAction(this, act.Instance as ExecAction);
        }

        public ITaskEvent CreateTaskEvent(TaskEvent currentEvent)
        {
            return new Dev2TaskEvent(currentEvent);
        }

        public ITaskEventLog CreateTaskEventLog(string taskPath)
        {
            return new Dev2TaskEventLog(this,
                new TaskEventLog(DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0)), taskPath));
        }

        public ITaskEventLog CreateTaskEventLog(string taskPath, DateTime startDate)
        {
            return new Dev2TaskEventLog(this, new TaskEventLog(startDate, taskPath));
        }

        public TaskService CreateTaskService()
        {
            return new TaskService();
        }


        public ITrigger SanitiseTrigger(ITrigger resource)
        {
            ITrigger trigger = new Dev2Trigger(this, resource.Instance);
            Trigger serialisedTrigger = resource.Instance;
            switch (resource.Instance.TriggerType)
            {
                case TaskTriggerType.Boot:
                    trigger = new Dev2BootTrigger(this, new BootTrigger());
                    ExtractDelay(resource.Instance as ITriggerDelay, trigger.Instance as ITriggerDelay);
                    break;
                case TaskTriggerType.Custom:
                    trigger = new Dev2Trigger(this, new DailyTrigger());

                    break;
                case TaskTriggerType.Daily:
// ReSharper disable PossibleNullReferenceException
                    trigger = new Dev2DailyTrigger(this,
                        new DailyTrigger((serialisedTrigger as DailyTrigger).DaysInterval));
// ReSharper restore PossibleNullReferenceException

                    break;
                case TaskTriggerType.Event:
                    var evt = resource.Instance as EventTrigger;
                    if (evt != null)
                    {
                        int? eventId;
                        string source;
                        string log;
                        evt.GetBasic(out log, out source, out eventId);

                        trigger = new Dev2EventTrigger(this, new EventTrigger(log, source, eventId));
                    }


                    break;
                case TaskTriggerType.Idle:
                    trigger = new Dev2IdleTrigger(this, new IdleTrigger());

                    break;
                case TaskTriggerType.Logon:
                    var logonTrigger = resource.Instance as LogonTrigger;
                    if (logonTrigger != null)
                        trigger = new Dev2LogonTrigger(this, new LogonTrigger {UserId = logonTrigger.UserId});

                    break;
                case TaskTriggerType.Monthly:
                    var a = (serialisedTrigger as MonthlyTrigger);
// ReSharper disable PossibleNullReferenceException
                    trigger = new Dev2MonthlyTrigger(this, new MonthlyTrigger(a.DaysOfMonth.First(), a.MonthsOfYear));
// ReSharper restore PossibleNullReferenceException

                    break;
                case TaskTriggerType.MonthlyDOW:
                    var b = (serialisedTrigger as MonthlyDOWTrigger);
                    trigger = new Dev2MonthlyDowTrigger(this,
// ReSharper disable PossibleNullReferenceException
                        new MonthlyDOWTrigger(b.DaysOfWeek, b.MonthsOfYear,
                            b.WeeksOfMonth));
// ReSharper restore PossibleNullReferenceException

                    break;
                case TaskTriggerType.Registration:

                    trigger = new Dev2RegistrationTrigger(this, new RegistrationTrigger());

                    break;
                case TaskTriggerType.SessionStateChange:

                    var sessionStateChangeTrigger = resource.Instance as SessionStateChangeTrigger;
                    if (sessionStateChangeTrigger != null)
                        trigger = new Dev2Trigger(this,
                            new SessionStateChangeTrigger
                            {
                                UserId = sessionStateChangeTrigger.UserId,
                                StateChange = sessionStateChangeTrigger.StateChange
                            });
                    break;
                case TaskTriggerType.Time:
                    var y = (serialisedTrigger as TimeTrigger);
// ReSharper disable PossibleNullReferenceException
                    trigger = new Dev2TimeTrigger(this, new TimeTrigger(y.StartBoundary));
// ReSharper restore PossibleNullReferenceException

                    break;
                case TaskTriggerType.Weekly:
                    var z = (serialisedTrigger as WeeklyTrigger);
// ReSharper disable PossibleNullReferenceException
                    trigger = new Dev2WeeklyTrigger(this, new WeeklyTrigger(z.DaysOfWeek, z.WeeksInterval));
// ReSharper restore PossibleNullReferenceException

                    break;
                default:
                    trigger = null;
                    break;
            }
            if (trigger != null)
            {
                trigger.Enabled = resource.Enabled;
                trigger.EndBoundary = resource.EndBoundary;
                trigger.StartBoundary = resource.StartBoundary;
                trigger.Repetition.Duration = resource.Repetition.Duration;
                trigger.Repetition.Interval = resource.Repetition.Interval;
                trigger.Repetition.StopAtDurationEnd = resource.Repetition.StopAtDurationEnd;

                ExtractDelay(resource.Instance as ITriggerDelay, trigger.Instance as ITriggerDelay);
            }
            return trigger;
        }

        public ITaskFolder CreateRootFolder()
        {
            throw new NotImplementedException();
        }

        private static void ExtractDelay(ITriggerDelay resource, ITriggerDelay trigger)
        {
            ITriggerDelay daily = trigger;
            ITriggerDelay dailyr = resource;
            if (daily != null && dailyr != null)
            {
                daily.Delay = dailyr.Delay;
            }
        }
    }
}
