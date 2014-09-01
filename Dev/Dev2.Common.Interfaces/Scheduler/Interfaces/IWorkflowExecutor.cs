
namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IWorkflowExecutor
    {
        IResourceHistory RunWorkFlow(string serverUri, string workflowName); // permissions  need to find out about permissions
    }
}
