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

namespace Warewolf.OS
{
    public abstract class WorkerMonitor : IProcessMonitor
    {
        private readonly List<IProcessThreadList> _processLists = new List<IProcessThreadList>();

        private bool _running;

        public event ProcessDiedEvent OnProcessDied;
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
                Name = this.GetType().Name,
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
