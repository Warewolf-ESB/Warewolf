using System.Threading;

namespace Dev2.Runtime.ESB.Execution
{
    public class ExecutionManager : IExecutionManager
    {       
        volatile bool _isRefreshing;
        volatile int _currentExecutions;
        static ManualResetEvent _eventPulse = new ManualResetEvent(false);

        public void StartRefresh()
        {
            if(_isRefreshing)
            {
                return;
            }
            _isRefreshing = true;
            while (_currentExecutions > 0)
            {
                Thread.Sleep(1);
            }
        }

        public void Wait()
        {
            _eventPulse.WaitOne();
        }
        public void StopRefresh()
        {
            _eventPulse.Set();
            _isRefreshing = false;
        }
        public void AddExecution()
        {
            Interlocked.Increment(ref _currentExecutions);
        }

        public bool IsRefreshing => _isRefreshing;

        public void CompleteExecution()
        {
            if (_currentExecutions > 0)
            {
                Interlocked.Decrement(ref _currentExecutions);
            }
        }        
    }
}
