
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
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2IdleTrigger : Dev2Trigger, IIdleTrigger
    {
        public Dev2IdleTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, IdleTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public new IdleTrigger Instance
        {
            get { return (IdleTrigger) base.Instance; }
        }
    }
}
