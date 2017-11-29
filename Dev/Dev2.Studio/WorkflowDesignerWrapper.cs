using Dev2.Studio.Interfaces;
using System.Activities.Presentation;

namespace Dev2
{
    public class WorkflowDesignerWrapper : IWorkflowDesignerWrapper
    {
        public TServiceType GetService<TServiceType>(WorkflowDesigner wd)
        {
            var modelService=wd.Context.Services.GetService<TServiceType>();
            return modelService;
        }
    }
}
