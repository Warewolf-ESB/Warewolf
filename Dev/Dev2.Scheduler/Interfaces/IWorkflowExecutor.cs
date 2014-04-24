
namespace Dev2.Scheduler.Interfaces
{
    public interface IWorkflowExecutor
    {
        IResourceHistory RunWorkFlow(string serverUri, string workflowName); // permissions  need to find out about permissions
    }
}
