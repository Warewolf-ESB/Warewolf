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

using Dev2.Common.Interfaces.Queue;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Common.Interfaces.Queue
{
    public interface IQueueTrigger
    {
        /// <summary>
        ///     Queue library trigger.
        /// </summary>
        [JsonIgnore]
        ITrigger Trigger { get; set; }

        /// <summary>
        ///     State according to windows.
        /// </summary>
        [JsonConverter(typeof (StringEnumConverter))]
        QueueStatus State { get; }

        /// <summary>
        ///     used for going across the wire
        /// </summary>
        string NativeXML { get; set; }
    }
}