using System;
using System.Collections.Concurrent;
using System.Threading;
using Dev2.Data.TO;

namespace Dev2.Runtime.ESB.Execution
{
    public class ExecutionManager : IExecutionManager
    {
        private static readonly Lazy<ExecutionManager> LazyCat = new Lazy<ExecutionManager>(() =>
        {
            var c = new ExecutionManager();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        readonly ConcurrentQueue<IEsbExecutionContainer> _currentExecutions;
        readonly ConcurrentQueue<IEsbExecutionContainer> _queuedExecutions;
        private bool _isRefreshing;
        private int _isExecuting;

        private ExecutionManager()
        {
            _currentExecutions = new ConcurrentQueue<IEsbExecutionContainer>();
            _queuedExecutions = new ConcurrentQueue<IEsbExecutionContainer>();
            _isRefreshing = false;
            _isExecuting = 0;          
        }
        public static ExecutionManager Instance => LazyCat.Value;

        public void StartRefresh()
        {
            _isRefreshing = true;
            CompleteAllCurrentExecutions();
        }

        public void StopRefresh()
        {
            IEsbExecutionContainer container;
            while (_queuedExecutions.TryDequeue(out container))
            {
                _currentExecutions.Enqueue(container);
            }
            _isRefreshing = false;
            CompleteAllCurrentExecutions();            
        }
        public void AddExecution(IEsbExecutionContainer container)
        {
            if (!_isRefreshing)
            {
                _currentExecutions.Enqueue(container);
            }
            else
            {

                _queuedExecutions.Enqueue(container);
            }
        }

        public void PerformExecution()
        {
//            while(_isRefreshing)
//            {
//                if (!_isSleeping)
//                {
//                    _isSleeping = true;
//                    Thread.Sleep(100);
//                    _isSleeping = false;
//                }               
//            }
            DoExecutions();
        }

        private void DoExecutions()
        {
            IEsbExecutionContainer container;
            if(_currentExecutions.TryDequeue(out container))
            {
                _isExecuting++;
                ErrorResultTO errors;                
                container.Execute(out errors, 0);
                _isExecuting--;
                
            }
        }

        public void CompleteAllCurrentExecutions()
        {
            while (_isExecuting > 0)
            {
                if (!_currentExecutions.IsEmpty)
                {
                    DoExecutions();
                }
            }
        }

        public bool IsRefreshing => _isRefreshing;
    }
}
