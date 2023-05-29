using System;

namespace Warewolf.Auditing
{
    public interface IStateListener : IDisposable
    {
        void LogAdditionalDetail(object detail, string callerName);
        void LogExecuteException(SerializableException e, object activity);
		void LogExecuteStartState(object activity);
		void LogExecuteCompleteState(object activity);
		void LogExecuteActivityStartState(object activity);
		void LogExecuteActivityCompleteState(object activity);
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
