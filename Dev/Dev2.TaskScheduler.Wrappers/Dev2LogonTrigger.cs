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
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2LogonTrigger : Dev2Trigger, ILogonTrigger
    {
        public Dev2LogonTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, LogonTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return (Instance).Delay; }
            set { (Instance).Delay = value; }
        }

        public string UserId
        {
            get { return Instance.UserId; }
            set { Instance.UserId = value; }
        }

        public new LogonTrigger Instance
        {
            get { return (LogonTrigger) base.Instance; }
        }
    }
}