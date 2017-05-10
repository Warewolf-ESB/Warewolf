namespace Dev2.Runtime
{
    public interface IExecutionManager
    {
        void StartRefresh();

        void StopRefresh();

        void AddExecution(IEsbExecutionContainer container);

        void PerformExecution();

        void CompleteAllCurrentExecutions();

        bool IsRefreshing { get;}
    }
}