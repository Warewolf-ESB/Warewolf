/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Wrappers;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Dev2.Common.Interfaces.Triggers;

namespace Dev2
{
    public class QueueWorkerMonitor : IQueueProcessorMonitor
    {
        private readonly IQueueConfigLoader _queueConfigLoader;
        private readonly IProcessFactory _processFactory;
        private readonly ITriggersCatalog _triggersCatalog;

        private readonly List<ProcessThread> _processes = new List<ProcessThread>();
        private readonly IWriter _writer;
        private bool _running;

        public QueueWorkerMonitor(IProcessFactory processFactory, IQueueConfigLoader queueConfigLoader, IWriter writer, ITriggersCatalog triggersCatalog)
        {
            _processFactory = processFactory;
            _queueConfigLoader = queueConfigLoader;
            _writer = writer;

            _triggersCatalog = triggersCatalog;
            _triggersCatalog.OnChanged += WorkerRestart;
            _triggersCatalog.OnDeleted += WorkerDeleted;
            _triggersCatalog.OnCreated += WorkerCreated;
        }

        private void WorkerRestart(string guid)
        {
            var process = _processes.FirstOrDefault(o => o.TriggerId == guid);
            if (process is null)
            {
                AddMissingMonitors();
                return;
            }
            var p = Process.GetProcessById(process.Pid);
            p.Kill();
        }
        private void WorkerDeleted(string guid)
        {
            var process = _processes.FirstOrDefault(o => o.TriggerId == guid);
            if (process is null)
            {
                return;
            }
            process.Kill();
            _processes.Remove(process);
        }

        private void WorkerCreated(string guid)
        {
            AddMissingMonitors();
        }

        private void AddMissingMonitors()
        {
            foreach (var guid in _queueConfigLoader.Configs)
            {
                if (_processes.Exists(o => o.TriggerId == guid))
                {
                    continue;
                }
                var t = new ProcessThread(_processFactory, _writer, guid);
                _processes.Add(t);
            }
        }

        public void Start()
        {
            _running = true;
            AddMissingMonitors();
            new Thread(() =>
            {
                MonitorProcesses();
            }).Start();
        }

        public void Shutdown()
        {
            _running = false;
        }

        private void MonitorProcesses()
        {
            while (_running)
            {
                var items = _processes.ToArray();
                foreach (var process in items)
                {
                    if (!process.IsAlive)
                    {
                        process.Start();
                    }
                };

                Thread.Sleep(1000);
            }
        }

        class ProcessThread
        {
            Thread _thread;
            private readonly IProcessFactory _processFactory;
            private readonly IWriter _writer;

            public int Pid { get; private set; }
            public string TriggerId { get; private set; }
            public bool IsAlive { get => _thread?.IsAlive ?? false; }

            public ProcessThread(IProcessFactory processFactory, IWriter writer, string guid)
            {
                TriggerId = guid;
                _processFactory = processFactory;
                _writer = writer;
            }
            public void Start()
            {
                _thread = new Thread(() =>
                {
                    try
                    {
                        var worker = GlobalConstants.QueueWorkerExe;
                        var startInfo = new ProcessStartInfo(worker, $"-c \"{TriggerId}\"");
                        using (var process = _processFactory.Start(startInfo))
                        {
                            Pid = process.Id;
                            while (!process.WaitForExit(1000))
                            {
                                //TODO: check queue progress, kill if necessary
                            }
                            _writer.WriteLine($"{worker} has died, Error:" + JsonConvert.SerializeObject(TriggerId));
                        }
                    }
                    catch { }
                });
                _thread.Start();
            }

            internal void Kill()
            {
                var p = Process.GetProcessById(Pid);
                p.Kill();
            }
        }
    }
}
