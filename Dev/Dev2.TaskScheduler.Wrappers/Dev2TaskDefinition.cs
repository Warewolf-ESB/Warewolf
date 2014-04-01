using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskDefinition : IDev2TaskDefinition
    {
        private readonly TaskDefinition _taskDefinition;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TaskDefinition(ITaskServiceConvertorFactory taskServiceConvertorFactory,
                                  TaskDefinition taskDefinition)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _taskDefinition = taskDefinition;
        }

        public IActionCollection Actions
        {
            get { return _taskServiceConvertorFactory.CreateActionCollection(Instance.Actions); }
        }

        public string Data
        {
            get { return _taskDefinition.Data; }
            set { _taskDefinition.Data = value; }
        }

        public ITaskSettings Settings
        {
            get { return _taskServiceConvertorFactory.CreateTaskSettings(_taskDefinition.Settings); }
        }

        public ITriggerCollection Triggers
        {
            get { return _taskServiceConvertorFactory.CreateTriggerCollection(_taskDefinition.Triggers); }
        }

        public string XmlText
        {
            get { return _taskDefinition.XmlText; }
            set { _taskDefinition.XmlText = value; }
        }

        public bool IsValidDev2Task()
        {
            if (null == Actions || Actions.Count == 0)
            {
                return false;
            }
            if (Actions.First().ActionType != TaskActionType.Execute) return false;
            IExecAction action = _taskServiceConvertorFactory.CreateExecAction(Actions.First());
            if (action.Arguments != null)
            {
                List<string> output =
                    action.Arguments.Split(new[] {'"'}).Where(a => !String.IsNullOrEmpty(a.Trim())).ToList();
                if (output.Count() != 2 || !output.All(a => a.Contains(":")))
                    return false;
            }


            if (!action.Path.Contains( GlobalConstants.SchedulerAgentPath))
            {
                return false;
            }
            return true;
        }

        public IAction Action
        {
            get { return Actions.FirstOrDefault(); }
        }

        public ITrigger Trigger
        {
            get { return Triggers.FirstOrDefault(); }
        }

        public void AddAction(IAction action)
        {
            Actions.Add(action);
        }

        public void AddTrigger(ITrigger trigger)
        {
            Triggers.Add(trigger);
        }

        public void Dispose()
        {
            _taskDefinition.Dispose();
        }

        public TaskDefinition Instance
        {
            get { return _taskDefinition; }
        }

        public string UserName
        {
            get { return _taskDefinition.Principal.DisplayName; }
        }
    }
}