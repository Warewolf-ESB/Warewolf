/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Warewolf.OS;

namespace Dev2
{
    public class HangfireServerMonitorWithRestart : WorkerMonitor
    {
        private readonly IChildProcessTracker _childProcessTracker;
        private readonly IProcessFactory _processFactory;

        public HangfireServerMonitorWithRestart(IChildProcessTracker childProcessTracker, IProcessFactory processFactory)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
        }

        protected override IEnumerable<IJobConfig> GetConfigs()
        {
            return Config.Persistence.Enable ? new IJobConfig[] {new JobConfig(GlobalConstants.HangfireServerProcessId, 1)} : new IJobConfig[] { };
        }

        protected override ProcessThreadList NewThreadList(IJobConfig config) => new HangfireServerThreadList(_childProcessTracker, _processFactory, config);
    }

    public class HangfireServerThreadList : ProcessThreadList
    {
        private readonly IChildProcessTracker _childProcessTracker;
        private readonly IProcessFactory _processFactory;
        private readonly IJobConfig _jobConfig;

        public HangfireServerThreadList(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig jobConfig)
            : base(jobConfig)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
            _jobConfig = jobConfig;
        }

        protected override IProcessThread GetProcessThread() => new HangfireServerMonitor(_childProcessTracker, _processFactory, _jobConfig);
    }

    public class HangfireServerMonitor : ProcessMonitor
    {
        public HangfireServerMonitor(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(childProcessTracker, processFactory, config)
        {
        }

        protected override ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo(GlobalConstants.HangfireServerExe);
        }
    }
}