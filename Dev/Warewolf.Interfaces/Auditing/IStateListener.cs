using System;

namespace Warewolf.Auditing
{
    public interface IStateListener : IDisposable
    {
        void LogAdditionalDetail(object detail, string callerName);
        void LogExecuteException(Exception e, object activity);
        void LogExecuteCompleteState(object activity);
        void LogStopExecutionState(object activity);
        void LogActivityExecuteState(object nextActivityObject);
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
