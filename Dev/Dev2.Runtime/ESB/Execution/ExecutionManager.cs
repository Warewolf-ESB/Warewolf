using System;
using System.Collections.Generic;
using System.Threading;

namespace Dev2.Runtime.ESB.Execution
{
    public class ExecutionManager : IExecutionManager
    {
        private static readonly Lazy<ExecutionManager> LazyCat = new Lazy<ExecutionManager>(() =>
        {
            var c = new ExecutionManager();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        private bool _isRefreshing;
        private int _isExecuting;
        private readonly List<AutoResetEvent> _waitHandles = new List<AutoResetEvent>();

        private ExecutionManager()
        {
            _isRefreshing = false;
            _isExecuting = 0;          
        }
        public static ExecutionManager Instance => LazyCat.Value;

        public void StartRefresh()
        {
            _isRefreshing = true;
            while (_isExecuting > 0)
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
            _isRefreshing = false;
        }
        public void AddExecution()
        {
            _isExecuting++;

        }

        public bool IsRefreshing => _isRefreshing;

        public void CompleteExecution()
        {
            if (_isExecuting > 0)
            {
                _isExecuting--;
            }
        }

        public void AddWait(AutoResetEvent eventPulse)
        {
            _waitHandles.Add(eventPulse);
        }
    }
}
