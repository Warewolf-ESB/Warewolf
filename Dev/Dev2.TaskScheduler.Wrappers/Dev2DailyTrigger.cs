/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    public class Dev2DailyTrigger : Dev2Trigger, IDailyTrigger
    {
        public Dev2DailyTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, Trigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public new DailyTrigger Instance => (DailyTrigger) base.Instance;

        public short DaysInterval
        {
            get { return Instance.DaysInterval; }
            set { Instance.DaysInterval = value; }
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }
            set { Instance.RandomDelay = value; }
        }
    }
}
