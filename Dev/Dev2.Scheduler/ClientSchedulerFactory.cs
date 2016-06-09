
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
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
    }
}
