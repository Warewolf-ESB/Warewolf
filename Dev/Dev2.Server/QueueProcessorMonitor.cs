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
using System.Threading;
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
                    WorkerRestart(config);

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

        protected override ProcessThread GetProcessThread() => new QueueProcessThread(_childProcessTracker, _processFactory, Config);
    }
    class QueueProcessThread : ProcessThread
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

    public abstract class WorkerMonitor : IProcessorMonitor
    {
        private readonly List<IProcessThreadList> _processLists = new List<IProcessThreadList>();

        private bool _running;

        public event ProcessDiedEvent OnProcessDied;

        protected void WorkerRestart(IJobConfig config)
        {
            var processList = _processLists.FirstOrDefault(o => o.Config.Id == config.Id);
            if (processList is null)
            {
                AddMissingMonitors();
                return;
            }
            processList.UpdateConfig(config);
        }
        protected void WorkerDeleted(Guid guid)
        {
            var process = _processLists.FirstOrDefault(o => o.Config.Id == guid);
            if (process is null)
            {
                return;
            }
            process.Kill();
            _processLists.Remove(process);
        }

        protected void WorkerCreated(Guid guid)
        {
            AddMissingMonitors();
        }

        private void AddMissingMonitors()
        {
            foreach (var config in GetConfigs())
            {
                if (_processLists.Exists(o => o.Config.Id == config.Id))
                {
                    continue;
                }
                var list = NewThreadList(config);
                list.OnProcessDied += (processDiedConfig) => OnProcessDied?.Invoke(processDiedConfig);
                _processLists.Add(list);
            }
        }

        protected abstract ProcessThreadList NewThreadList(IJobConfig config);
        protected abstract IEnumerable<IJobConfig> GetConfigs();

        public void Start()
        {
            _running = true;
            AddMissingMonitors();
            var monitor = new Thread(() =>
            {
                MonitorProcesses();
            })
            {
                IsBackground = true
            };
            monitor.Start();
        }

        public void Shutdown()
        {
            _running = false;
        }

        private void MonitorProcesses()
        {
            while (_running)
            {
                var lists = _processLists.ToArray();
                foreach (var processList in lists)
                {
                    processList.Monitor();
                }

                Thread.Sleep(1000);
            }
        }
    }
}
