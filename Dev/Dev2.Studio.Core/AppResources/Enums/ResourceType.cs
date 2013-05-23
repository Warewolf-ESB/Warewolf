using Dev2.Studio.Core.AppResources.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Dev2.Studio.Core.AppResources.ExtensionMethods;

namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum ResourceType
    {
        [TreeCategory("WORKFLOWS")]
        [IconLocation("pack://application:,,,/Warewolf Studio;component/images/workflowservice2.png")]
        [Description("Workflow service")]
        [Display(Order = 1)]
        WorkflowService,

        [TreeCategory("SERVICES")]
        [IconLocation("pack://application:,,,/Warewolf Studio;component/images/workerservice.png")]
        [Description("Worker Service")]
        [Display(Order = 2)]
        Service,

        [TreeCategory("SOURCES")]
        [IconLocation("pack://application:,,,/Warewolf Studio;component/images/source.png")]
        [Description("Source")]
        [Display(Order = 3)]
        Source,

        [Description("Unknown")]
        Unknown,

        [Description("Website")]
        Website,

        [Description("Human Interface Process")]
        HumanInterfaceProcess,
        Server
    }

    public static class ResourceTypeExtensions
    {        
        public static WorkSurfaceContext ToWorkSurfaceContext(this ResourceType resourceType)
        {            
            switch (resourceType)
            {
                 case ResourceType.WorkflowService:
                    return Enums.WorkSurfaceContext.Workflow;
                 case ResourceType.Service:
                    return Enums.WorkSurfaceContext.Service;
                 case ResourceType.Source:
                    return Enums.WorkSurfaceContext.SourceManager;
                 case ResourceType.Unknown:
                    return Enums.WorkSurfaceContext.Unknown;
                 case ResourceType.Website:
                    return Enums.WorkSurfaceContext.Website;
                 case ResourceType.HumanInterfaceProcess:
                    return Enums.WorkSurfaceContext.Webpage;
                default: return Enums.WorkSurfaceContext.Unknown;
            }
        }

    }
}
