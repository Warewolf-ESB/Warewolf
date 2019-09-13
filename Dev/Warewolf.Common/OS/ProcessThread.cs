/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics;
using System.Threading;
using Warewolf.Triggers;

namespace Warewolf.OS
{
    public abstract class ProcessThread : IProcessThread
    {
        Thread _thread;
        private readonly IProcessFactory _processFactory;
        public IJobConfig Config { get; }

        public event ProcessDiedEvent OnProcessDied;

        public int Pid => _process.Id;

        private IProcess _process;
        private readonly IChildProcessTracker _childProcessTracker;

        public bool IsAlive => _thread?.IsAlive ?? false;

        protected ProcessThread(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IJobConfig config)
        {
            _childProcessTracker = childProcessTracker;
            _processFactory = processFactory;
            Config = config;
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
                    var startInfo = GetProcessStartInfo();
                    using (var process = _processFactory.Start(startInfo))
                    {
                        _childProcessTracker.Add(process);
                        _process = process;
                        while (!process.WaitForExit(1000))
                        {
                            //TODO: check queue progress, kill if necessary
                        }
                        OnProcessDied?.Invoke(Config);
                    }
                }
                catch { }
            })
            {
                IsBackground = true
            };
            _thread.Start();
        }
        protected abstract ProcessStartInfo GetProcessStartInfo();

        public void Kill()
        {
            if (_process.HasExited)
            {
                return;
            }
            var p = Process.GetProcessById(Pid);
            p.Kill();
        }
    }
}