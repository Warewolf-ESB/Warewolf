using System.Linq;
using Dev2.Scheduler.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Scheduler
{
    public class ScheduleTrigger : IScheduleTrigger
    {
        private IDev2TaskService _service;
        private ITaskServiceConvertorFactory _factory;
        public ScheduleTrigger(TaskState state, ITrigger trigger, IDev2TaskService service, ITaskServiceConvertorFactory factory)
        {
            _service = service;
            _factory = factory;
            State = state;
            Trigger = trigger;

        }
        [JsonIgnore]
        public ITrigger Trigger
        {
            get { return GetTriggerFromXml(NativeXML); }
            set { if (value != null) NativeXML = SetXmlFromTrigger(value); }
        }

        private string SetXmlFromTrigger(ITrigger value)
        {
            _factory= _factory ?? new TaskServiceConvertorFactory();
           _service= _service ?? new Dev2TaskService(_factory);
            using (var task = _service.NewTask())
            {
                task.AddAction(_factory.CreateExecAction("notepad"));
                task.AddTrigger(value);
              
                return task.XmlText;
            }
        }


        private ITrigger GetTriggerFromXml(string nativeXml)
        {
            _factory = _factory ?? new TaskServiceConvertorFactory();
            _service = _service ?? new Dev2TaskService(_factory);
            using (var task = _service.NewTask())
            {
                task.AddAction(_factory.CreateExecAction("notepad"));

                task.XmlText = nativeXml;
                return task.Triggers.FirstOrDefault();
            }
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public TaskState State { get; private set; }
        public string NativeXML { get; set; }
    }
}