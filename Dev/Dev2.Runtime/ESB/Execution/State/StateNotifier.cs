using Dev2.Interfaces;
using System;
using System.Collections.Generic;

namespace Dev2.Runtime.ESB.Execution.State
{
    class StateNotifier : IStateNotifier, IDisposable
    {
        public void Dispose()
        {
            foreach (var listener in stateListeners)
            {
                if (listener is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            Notify(nameof(LogAdditionalDetail), new
            {
                CallerName = callerName,
                Detail = detail,
            });
        }

        public void LogExecuteCompleteState()
        {
            Notify(nameof(LogExecuteCompleteState), null);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            Notify(nameof(LogExecuteException), new
            {
                Activity = activity,
                Exception = e,
            });
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            Notify(nameof(LogPostExecuteState), new
            {
                previousActivity,
                nextActivity,
            });
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            Notify(nameof(LogPreExecuteState), nextActivity);
        }

        public void LogStopExecutionState()
        {
            Notify(nameof(LogStopExecutionState), null);
        }

        readonly IList<IStateListener> stateListeners = new List<IStateListener>();
        public void Subscribe(IStateListener listener)
        {
            stateListeners.Add(listener);
        }

        private void Notify(string callerName, object payload)
        {
            foreach (var stateListener in stateListeners)
            {
                var result = stateListener.Notify(callerName, payload);
                if (!result)
                {
                    throw new StateLoggerStoppedException();
                }
            }
        }
        class StateLoggerStoppedException : Exception { }
    }
}
