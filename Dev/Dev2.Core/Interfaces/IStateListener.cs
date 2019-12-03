using System;
using System.IO;

namespace Dev2.Interfaces
{
    public interface IStateListener : IDisposable
    {
        void LogPreExecuteState(IDev2Activity nextActivity);
        void LogAdditionalDetail(object detail, string callerName);
        void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity);
        void LogExecuteException(Exception e, IDev2Activity activity);
        void LogExecuteCompleteState(IDev2Activity activity);
        void LogStopExecutionState(IDev2Activity activity);
    }
    public interface IStateNotifier : IStateListener
    {
        void Subscribe(IStateListener listener);
    }
    public interface IStateNotifierRequired
    {
        void SetStateNotifier(IStateNotifier stateNotifier);
    }
}
