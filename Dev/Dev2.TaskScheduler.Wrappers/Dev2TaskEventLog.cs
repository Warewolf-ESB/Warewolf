
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    [ExcludeFromCodeCoverage] // cant really test this. 
    public class Dev2TaskEventLog : ITaskEventLog
    {
        private readonly ITaskServiceConvertorFactory _factory;
        private readonly TaskEventLog _taskLog;

        public Dev2TaskEventLog(ITaskServiceConvertorFactory factory, TaskEventLog taskLog)
        {
            _taskLog = taskLog;
            _factory = factory;
        }

        public long Count
        {
            get { return _taskLog.Count; }
        }


        public IEnumerator<ITaskEvent> GetEnumerator()
        {
            IEnumerator<TaskEvent> en = _taskLog.GetEnumerator();
            while (en.MoveNext())
            {
                yield return _factory.CreateTaskEvent(en.Current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
