/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using System.Collections.Generic;
using System;
using Warewolf.OS;
using Warewolf.Triggers;
using Dev2.Common;

using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace Dev2
{
    public class QueueWorkerMonitor : WorkerMonitor
    {
        private readonly IQueueConfigLoader _queueConfigLoader;
        private readonly IChildProcessTracker _childProcessTracker;
        private readonly IProcessFactory _processFactory;

        public QueueWorkerMonitor(IProcessFactory processFactory, IQueueConfigLoader queueConfigLoader, ITriggersCatalog triggersCatalog, IChildProcessTracker childProcessTracker)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
            _queueConfigLoader = queueConfigLoader;

            triggersCatalog.OnChanged += (triggerId) =>
            {
                try
                {
                    var configs = _queueConfigLoader.Configs;
                    var config = configs.First(o => o.Id == triggerId);
                    WorkerDeleted(config.Id);
                    WorkerCreated(config.Id);
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn(e.Message, "");
                }
            };

            triggersCatalog.OnDeleted += WorkerDeleted;
            triggersCatalog.OnCreated += WorkerCreated;
        }

        protected override IEnumerable<IJobConfig> GetConfigs() => _queueConfigLoader.Configs;
        protected override ProcessThreadList NewThreadList(IJobConfig config) => new QueueProcessThreadList(_childProcessTracker, _processFactory, config);
    }

    class QueueProcessThreadList : ProcessThreadList
    {
        private readonly IChildProcessTracker _childProcessTracker;
        private readonly IProcessFactory _processFactory;

        public QueueProcessThreadList(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(config)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
        }

        protected override IProcessThread GetProcessThread() => new QueueProcessThread(_childProcessTracker, _processFactory, Config);
    }
    class QueueProcessThread : ProcessMonitor
    {
        public QueueProcessThread(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
            : base(childProcessTracker, processFactory, config)
        {
        }

        protected override ProcessStartInfo GetProcessStartInfo()
        {
            var worker = GlobalConstants.QueueWorkerExe;
            return new ProcessStartInfo(worker, $"-c \"{Config.Id}\"");
        }
    }
}
