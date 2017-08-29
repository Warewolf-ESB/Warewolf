using System;
using System.Collections.Generic;
using System.Threading;

namespace Dev2.Runtime.ESB.Execution
{
    public class ExecutionManager : IExecutionManager
    {
        static readonly Lazy<ExecutionManager> LazyCat = new Lazy<ExecutionManager>(() =>
        {
            var c = new ExecutionManager();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        bool _isRefreshing;
        int _currentExecutions;
        readonly List<ManualResetEvent> _waitHandles = new List<ManualResetEvent>();

        private ExecutionManager()
        {
            _isRefreshing = false;
            _currentExecutions = 0;          
        }
        public static ExecutionManager Instance => LazyCat.Value;

        public void StartRefresh()
        {
            _isRefreshing = true;
            while (_currentExecutions > 0)
            {
                Thread.Sleep(10);
            }
        }
        public void StopRefresh()
        {
            foreach(var autoResetEvent in _waitHandles)
            {
                autoResetEvent.Set();
            }
            _waitHandles.Clear();
            _isRefreshing = false;
        }
        public void AddExecution()
        {
            _currentExecutions++;

        }

        public bool IsRefreshing => _isRefreshing;

        public void CompleteExecution()
        {
            if (_currentExecutions > 0)
            {
                _currentExecutions--;
            }
        }

        public void AddWait(ManualResetEvent eventPulse)
        {
            _waitHandles.Add(eventPulse);
        }
    }
}
