using System.Activities.Presentation;

namespace Dev2.Studio.Interfaces
{
    public interface IWorkflowDesignerWrapper
    {
        TServiceType GetService<TServiceType>(WorkflowDesigner wd);
    }
}
