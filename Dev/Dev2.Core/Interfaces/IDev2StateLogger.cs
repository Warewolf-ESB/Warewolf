using System;
using System.IO;

namespace Dev2.Interfaces
{
    public interface IDev2StateLogger : IDisposable
    {
        void LogPreExecuteState(IDev2Activity nextActivity);
        void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity);
        void LogExecuteException(Exception e, IDev2Activity activity);
        void LogExecuteCompleteState();
        void Close();
    }
}
