using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2EventTrigger : Dev2Trigger, IEventTrigger
    {
        public Dev2EventTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, EventTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return Instance.Delay; }
            set { Instance.Delay = value; }
        }

        public new EventTrigger Instance
        {
            get { return (EventTrigger) base.Instance; }
        }

        public string Subscription
        {
            get { return Instance.Subscription; }
            set { Instance.Subscription = value; }
        }

        public NamedValueCollection ValueQueries
        {
            get { return Instance.ValueQueries; }
        }

        
        public bool GetBasic(out string log, out string source, out int? eventId)
        {
            return Instance.GetBasic(out log, out source, out eventId);
        }

        public void SetBasic(string log, string source, int? eventId)
        {
            Instance.SetBasic(log, source, eventId);
        }
    }
}