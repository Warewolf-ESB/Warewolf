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
using Dev2.Triggers;
using System;
using System.Collections;

namespace Dev2
{
    public class QueueWorkerMonitor : IQueueProcessorMonitor
    {
        private readonly IQueueConfigLoader _queueConfigLoader;
        private readonly IChildProcessTracker _childProcessTracker;
        private readonly IProcessFactory _processFactory;

        private readonly List<IProcessThreadList> _processLists = new List<IProcessThreadList>();
        private readonly IWriter _writer;
        private bool _running;

        public QueueWorkerMonitor(IProcessFactory processFactory, IQueueConfigLoader queueConfigLoader, IWriter writer, ITriggersCatalog triggersCatalog, IChildProcessTracker childProcessTracker)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
            _queueConfigLoader = queueConfigLoader;
            _writer = writer;

            triggersCatalog.OnChanged += WorkerRestart;
            triggersCatalog.OnDeleted += WorkerDeleted;
            triggersCatalog.OnCreated += WorkerCreated;
        }

        private void WorkerRestart(Guid guid)
        {
            var processList = _processLists.FirstOrDefault(o => o.TriggerId == guid);
            if (processList is null)
            {
                AddMissingMonitors();
                return;
            }
            foreach (var process in processList)
            {
                var p = Process.GetProcessById(process.Pid);
                p.Kill();
            }
        }
        private void WorkerDeleted(Guid guid)
        {
            var process = _processLists.FirstOrDefault(o => o.TriggerId == guid);
            if (process is null)
            {
                return;
            }
            process.Kill();
            _processLists.Remove(process);
        }

        private void WorkerCreated(Guid guid)
        {
            AddMissingMonitors();
        }

        private void AddMissingMonitors()
        {
            foreach (var config in _queueConfigLoader.Configs)
            {
                if (_processLists.Exists(o => o.TriggerId == config.TriggerId))
                {
                    continue;
                }
                var list = new ProcessThreadList(_childProcessTracker, _processFactory, _writer, config);
                _processLists.Add(list);
            }
        }

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
                    if (!processList.IsAlive)
                    {
                        processList.Start();
                    }
                }

                Thread.Sleep(1000);
            }
        }

        interface IProcessThreadList : IEnumerable<ProcessThread>
        {
            Guid TriggerId { get; }
            bool IsAlive { get; }

            void Kill();
            void Start();
        }
        class ProcessThreadList : IProcessThreadList
        {
            readonly List<ProcessThread> _processThreads = new List<ProcessThread>();
            private readonly IChildProcessTracker _childProcessTracker;
            private readonly IProcessFactory _processFactory;
            private readonly IWriter _writer;
            private readonly ITrigger _config;

            public ProcessThreadList(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IWriter writer, ITrigger config)
            {
                _childProcessTracker = childProcessTracker;
                _processFactory = processFactory;
                _writer = writer;
                _config = config;
            }

            public Guid TriggerId { get => _config.TriggerId; }
            public bool IsAlive { get => _processThreads.Count >= Concurrency && _processThreads.All(o => o.IsAlive); }
            private int Concurrency => _config.Concurrency;

            public IEnumerator<ProcessThread> GetEnumerator() => _processThreads.GetEnumerator();
            public void Kill() => _processThreads.ForEach(o => { o.Kill(); });

            // check that each process in list is running otherwise start it
            // check that the number of processes matches Concurrency
            public void Start()
            {
                var expectedNumProcesses = Concurrency;
                var numProcesses = _processThreads.Count;
                if (numProcesses > expectedNumProcesses)
                {
                    var processThreads = _processThreads.ToArray();
                    for (int i = expectedNumProcesses; i < expectedNumProcesses; i++)
                    {
                        processThreads[i].Kill();
                        _processThreads.Remove(processThreads[i]);
                    }
                }
                else
                {
                    for (int i = numProcesses; i < expectedNumProcesses; i++)
                    {
                        var processThread = new ProcessThread(_childProcessTracker, _processFactory, _writer, TriggerId);
                        _processThreads.Add(processThread);
                    }
                }
                foreach (var processThread in _processThreads)
                {
                    processThread.Start();
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        class ProcessThread
        {
            Thread _thread;
            private readonly IProcessFactory _processFactory;
            private readonly IWriter _writer;

            public int Pid { get; private set; }

            private readonly IChildProcessTracker _childProcessTracker;

            public Guid TriggerId { get; private set; }
            public bool IsAlive => _thread?.IsAlive ?? false;

            public ProcessThread(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IWriter writer, Guid guid)
            {
                _childProcessTracker = childProcessTracker;
                TriggerId = guid;
                _processFactory = processFactory;
                _writer = writer;
            }
            public void Start()
            {
                if (IsAlive)
                {
                    return;
                }
                _thread = new Thread(() =>
                {
                    try
                    {
                        var worker = GlobalConstants.QueueWorkerExe;
                        var startInfo = new ProcessStartInfo(worker, $"-c \"{TriggerId}\"");
                        using (var process = _processFactory.Start(startInfo))
                        {
                            _childProcessTracker.Add(process);
                            Pid = process.Id;
                            while (!process.WaitForExit(1000))
                            {
                                //TODO: check queue progress, kill if necessary
                            }
                            _writer.WriteLine($"{worker} has died, Error:" + JsonConvert.SerializeObject(TriggerId));
                        }
                    }
                    catch { }
                })
                {
                    IsBackground = true
                };
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
