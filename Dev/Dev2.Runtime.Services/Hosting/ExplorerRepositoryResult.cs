using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Runtime.Hosting
{
    public class ExplorerRepositoryResult : IExplorerRepositoryResult
    {
        public ExplorerRepositoryResult(ExecStatus status, string message)
        {
            Message = message;
            Status = status;
        }

        public ExecStatus Status { get; private set; }
        public string Message { get; private set; }
    }
}
