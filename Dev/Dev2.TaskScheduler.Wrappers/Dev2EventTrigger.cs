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
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
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

        [ExcludeFromCodeCoverage]
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