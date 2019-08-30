/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using Dev2.Common.Wrappers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Dev2
{
    public class QueueWorkerMonitor : IQueueProcessorMonitor
    {
        private readonly IQueueConfigLoader _queueConfigLoader;
        private readonly IProcessFactory _processFactory;

        private readonly List<(Thread Thread, string Name)> _processes = new List<(Thread Thread, string Name)>();
        private readonly IWriter _writer;
        private bool _running;

        public QueueWorkerMonitor(IProcessFactory processFactory, IQueueConfigLoader queueConfigLoader, IWriter writer)
        {
            _processFactory = processFactory;
            _queueConfigLoader = queueConfigLoader;
            _writer = writer;
        }

        public void Start()
        {
            _running = true;
            foreach (var config in _queueConfigLoader.Configs)
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        Start(config);
                    }
                    catch { }
                    });
                thread.Start();
                _processes.Add((thread, config));
            }

            ProcessMonitor();
        }

        public void Stop()
        {
            _running = false;
        }

        private void Start(string config)
        {
            var worker = GlobalConstants.QueueWorkerExe;
            var startInfo = new ProcessStartInfo(worker, $"-c '{config}'"); 
            using (var process = _processFactory.Start(startInfo))
            {
                while (!process.WaitForExit(1000))
                {
                    //TODO: check queue progress, kill if necessary
                }
                _writer.WriteLine($"{worker} has died, Error:" + JsonConvert.SerializeObject(config));
            }
        }

        private void ProcessMonitor()
        {
            while (_running)
            {
                var items = _processes.ToArray();
                foreach (var process in items)
                {
                    if (!process.Thread.IsAlive)
                    {
                        var thread = new Thread(() => {
                            try
                            {
                                Start(process.Name);
                            } catch { }
                        });
                        thread.Start();

                        _processes.Remove(process);
                        _processes.Add((thread, process.Name));
                    }
                };

                Thread.Sleep(1000);
            }
        }
    }
}
