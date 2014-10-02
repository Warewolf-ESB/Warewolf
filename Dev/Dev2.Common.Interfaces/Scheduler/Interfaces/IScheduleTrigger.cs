
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
