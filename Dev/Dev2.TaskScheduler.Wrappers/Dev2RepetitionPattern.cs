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
    public class Dev2RepetitionPattern : IRepetitionPattern
    {
        private readonly RepetitionPattern _nativeInstance;

        public Dev2RepetitionPattern(RepetitionPattern nativeInstance)
        {
            _nativeInstance = nativeInstance;
        }

        public void Dispose()
        {
            Instance.Dispose();
        }

        public RepetitionPattern Instance
        {
            get { return _nativeInstance; }
        }

        public TimeSpan Duration
        {
            get { return Instance.Duration; }
            set { Instance.Duration = value; }
        }

        public TimeSpan Interval
        {
            get { return Instance.Interval; }
            set { Instance.Interval = value; }
        }

        public bool StopAtDurationEnd
        {
            get { return Instance.StopAtDurationEnd; }
            set { Instance.StopAtDurationEnd = value; }
        }

        public bool IsSet()
        {
            return Instance.IsSet();
        }
    }
}