using Dev2.Interfaces;
using System;
using System.Collections.Generic;

namespace Dev2.Runtime.Auditing
{
    public class StateNotifier : IStateNotifier
    {
        public void Dispose()
        {
            foreach (var listener in _stateListeners)
            {
                if (listener is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogAdditionalDetail(detail, callerName);
            }
        }

        public void LogExecuteCompleteState(IDev2Activity activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogExecuteCompleteState(activity);
            }
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogExecuteException(e, activity);
            }
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogPostExecuteState(previousActivity, nextActivity);
            }
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogPreExecuteState(nextActivity);
            }
        }

        public void LogStopExecutionState(IDev2Activity activity)
        {
            foreach (var stateListener in _stateListeners)
            {
                stateListener.LogStopExecutionState(activity);
            }
        }

        readonly IList<IStateListener> _stateListeners = new List<IStateListener>();
        public void Subscribe(IStateListener listener)
        {
            _stateListeners.Add(listener);
        }
    }
}
