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
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2RegistrationTrigger : Dev2Trigger, ITriggerDelay, IWrappedObject<RegistrationTrigger>
    {
        public Dev2RegistrationTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory,
            RegistrationTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return Instance.Delay; }
            set { Instance.Delay = value; }
        }

        public new RegistrationTrigger Instance
        {
            get { return (RegistrationTrigger) base.Instance; }
        }
    }
}
