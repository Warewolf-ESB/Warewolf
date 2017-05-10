using Dev2.Data.TO;

namespace Dev2.Runtime
{
    public interface IExecutionManager
    {
        void StartRefresh();

        void StopRefresh();

        void AddExecution(IEsbExecutionContainer container);

        ErrorResultTO PerformExecution(int update);

        void CompleteAllCurrentExecutions();
    }
}