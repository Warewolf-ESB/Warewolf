#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation;
#endif

namespace Dev2.Studio.Interfaces
{
    public interface IWorkflowDesignerWrapper
    {
#if WINDOWS || NETFRAMEWORK
        TServiceType GetService<TServiceType>(WorkflowDesigner wd);
#endif
    }
}
