using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IScheduleTrigger
    {

        /// <summary>
        /// task scheduler library trigger. 
        /// </summary>
        [JsonIgnore]
        ITrigger Trigger { get; set; }

        /// <summary>
        /// State according to windows. 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        TaskState State { get; }

        /// <summary>
        /// used for going across the wire
        /// </summary>
        string NativeXML { get; set; }

    }
}
