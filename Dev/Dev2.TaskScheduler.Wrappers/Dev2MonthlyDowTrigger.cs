
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
    public class Dev2MonthlyDowTrigger : Dev2Trigger, IMonthlyDOWTrigger
    {
        public Dev2MonthlyDowTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory,
                                     MonthlyDOWTrigger instance) : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return ((ITriggerDelay) Instance).Delay; }
            set { ((ITriggerDelay) Instance).Delay = value; }
        }

        public new MonthlyDOWTrigger Instance
        {
            get { return (MonthlyDOWTrigger) base.Instance; }
        }

        public DaysOfTheWeek DaysOfWeek
        {
            get { return Instance.DaysOfWeek; }
          
        }

        public MonthsOfTheYear MonthsOfYear
        {
            get { return Instance.MonthsOfYear; }
       
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }

        }

        public bool RunOnLastWeekOfMonth
        {
            get { return Instance.RunOnLastWeekOfMonth; }
      
        }

        public WhichWeek WeeksOfMonth
        {
            get { return Instance.WeeksOfMonth; }
           
        }
    }
}
