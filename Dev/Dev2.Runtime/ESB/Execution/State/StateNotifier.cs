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
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogAdditionalDetail(detail, callerName);
            }
        }

        public void LogExecuteCompleteState()
        {
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogExecuteCompleteState();
            }
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogExecuteException(e, activity);
            }
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogPostExecuteState(previousActivity, nextActivity);
            }
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogPreExecuteState(nextActivity);
            }
        }

        public void LogStopExecutionState()
        {
            foreach (var stateListener in stateListeners)
            {
                stateListener.LogStopExecutionState();
            }
        }

        readonly IList<IStateListener> stateListeners = new List<IStateListener>();
        public void Subscribe(IStateListener listener)
        {
            stateListeners.Add(listener);
        }
    }
}
