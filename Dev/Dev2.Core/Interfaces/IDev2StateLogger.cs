using System;
using System.IO;

namespace Dev2.Interfaces
{
    public interface IDev2StateLogger : IDisposable
    {
        void Subscribe(IStateLoggerListener listener);
        void LogPreExecuteState(IDev2Activity nextActivity);
        void LogAdditionalDetail(object detail, string callerName);
        void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity);
        void LogExecuteException(Exception e, IDev2Activity activity);
        void LogExecuteCompleteState();
        void LogStopExecutionState();
        void Close();
    }
    public interface IStateLoggerListener
    {
        bool Notify(string type, object payload);
    }
}
