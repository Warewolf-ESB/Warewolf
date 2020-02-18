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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Warewolf.OS
{
    public abstract class ProcessThreadList : IProcessThreadList
    {
        public event ProcessDiedEvent OnProcessDied;

        readonly List<IProcessThread> _processThreads = new List<IProcessThread>();
        private bool _running;
        protected ProcessThreadList(IJobConfig config)
        {
            UpdateConfig(config);
            _running = true;
        }

        private readonly ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        private IJobConfig _config;
        private int _expectedNumProcesses;

        public IJobConfig Config
        {
            get
            {
                _configLock.EnterReadLock();
                try
                {
                    return _config;
                }
                finally
                {
                    _configLock.ExitReadLock();
                }
            }
            private set
            {
                _configLock.EnterWriteLock();
                try
                {
                    _config = value;
                }
                finally
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        public bool NeedUpdate { get => _expectedNumProcesses < 1 || _processThreads.Count != _expectedNumProcesses || _processThreads.Any(o => !o.IsAlive); }
        public IEnumerator<IProcessThread> GetEnumerator() => _processThreads.GetEnumerator();
        public void Kill() => _processThreads.ForEach(o => { o.Kill(); });

        // check that each process in list is running otherwise start it
        // check that the number of processes matches Concurrency
        public void Monitor()
        {
            if (!_running || !NeedUpdate)
            {
                return;
            }


            var numProcesses = _processThreads.Count;
            if (numProcesses > _expectedNumProcesses)
            {
                var processThreads = _processThreads.ToArray();
                for (int i = _expectedNumProcesses; i < numProcesses; i++)
                {
                    processThreads[i].Kill();
                    _processThreads.Remove(processThreads[i]);
                }
            }
            else
            {
                for (int i = numProcesses; i < _expectedNumProcesses; i++)
                {
                    var processThread = GetProcessThread();
                    processThread.OnProcessDied += (config) => OnProcessDied?.Invoke(config);
                    _processThreads.Add(processThread);
                }
            }
            foreach (var processThread in _processThreads)
            {
                processThread.Start();
            }
        }

        private int CalculateProcessCount()
        {
            _expectedNumProcesses = _config.Concurrency;
            var logicalProcessors = System.Environment.ProcessorCount;
            if (_expectedNumProcesses > logicalProcessors)
            {
                _expectedNumProcesses = logicalProcessors;
            }

            return _expectedNumProcesses;
        }

        protected abstract IProcessThread GetProcessThread();

        public void UpdateConfig(IJobConfig config)
        {
            if (config is null)
            {
                Stop();
                return;
            }
            Config = config;
            _expectedNumProcesses = CalculateProcessCount();
        }

        private void Stop()
        {
            _running = false;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
