/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Warewolf.OS;

namespace Dev2
{

    public class LoggingServiceMonitorWithRestart : WorkerMonitor
    {
        private readonly ChildProcessTrackerWrapper _childProcessTracker;
        private readonly ProcessWrapperFactory _processFactory;

        public LoggingServiceMonitorWithRestart(ChildProcessTrackerWrapper childProcessTracker, ProcessWrapperFactory processFactory)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
        }

        protected override IEnumerable<IJobConfig> GetConfigs()
            => new IJobConfig[] { new JobConfig(Guid.NewGuid(),1) };
        protected override ProcessThreadList NewThreadList(IJobConfig config) => new LoggingServiceThreadList(_childProcessTracker, _processFactory, config);
    }
    public class LoggingServiceThreadList : ProcessThreadList
    {
        private readonly ChildProcessTrackerWrapper _childProcessTracker;
        private readonly ProcessWrapperFactory _processFactory;
        private readonly IJobConfig _jobConfig;

        public LoggingServiceThreadList(ChildProcessTrackerWrapper childProcessTracker, ProcessWrapperFactory processFactory, IJobConfig jobConfig)
            : base(jobConfig)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
            _jobConfig = jobConfig;
        }

        protected override IProcessThread GetProcessThread() => new LoggingServiceMonitor(_childProcessTracker, _processFactory, _jobConfig);
    }
    public class LoggingServiceMonitor : ProcessMonitor
    {
        public LoggingServiceMonitor(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(childProcessTracker, processFactory, config)
        {
        }

        protected override ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo("WarewolfLogger.exe");
        }
    }
}
