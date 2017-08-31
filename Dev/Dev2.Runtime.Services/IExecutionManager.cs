using System.Threading;

namespace Dev2.Runtime
{
    public interface IExecutionManager
    {
        void StartRefresh();

        void StopRefresh();

        void AddExecution();

        bool IsRefreshing { get;}

        void CompleteExecution();

        void Wait();        
    }
}