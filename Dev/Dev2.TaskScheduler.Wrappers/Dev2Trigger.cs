using System;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2Trigger : ITrigger
    {
        private readonly Trigger _instance;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2Trigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, Trigger instance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _instance = instance;
        }

        public void Dispose()
        {
            Instance.Dispose();
        }


        public Trigger Instance
        {
            get { return _instance; }
        }

        public bool Enabled
        {
            get { return Instance.Enabled; }
            set { Instance.Enabled = value; }
        }

        public DateTime EndBoundary
        {
            get { return Instance.EndBoundary; }
            set { Instance.EndBoundary = value; }
        }

        public TimeSpan ExecutionTimeLimit
        {
            get { return Instance.ExecutionTimeLimit; }
            set { Instance.ExecutionTimeLimit = value; }
        }

        public string Id
        {
            get { return Instance.Id; }
            set { Instance.Id = value; }
        }

        public IRepetitionPattern Repetition
        {
            get { return _taskServiceConvertorFactory.CreateRepetitionPattern(Instance.Repetition); }
        }

        public DateTime StartBoundary
        {
            get { return Instance.StartBoundary; }
            set { Instance.StartBoundary = value; }
        }

        public TaskTriggerType TriggerType
        {
            get { return Instance.TriggerType; }
        }

        public override string ToString()
        {
            return Instance.ToString();
        }
    }
}