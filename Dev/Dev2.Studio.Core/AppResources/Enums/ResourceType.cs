using Dev2.Studio.Core.AppResources.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum ResourceType
    {
        [TreeCategory("WORKFLOWS")]
        [IconLocation("pack://application:,,,/Dev2.Studio;component/images/workflowservice2.png")]
        [Description("Workflow service")]
        [Display(Order = 1)]
        WorkflowService,

        [TreeCategory("SERVICES")]
        [IconLocation("pack://application:,,,/Dev2.Studio;component/images/workerservice.png")]
        [Description("Worker Service")]
        [Display(Order = 2)]
        Service,

        [TreeCategory("SOURCES")]
        [IconLocation("pack://application:,,,/Dev2.Studio;component/images/source.png")]
        [Description("Source")]
        [Display(Order = 3)]
        Source,

        [Description("Unknown")]
        Unknown,

        [Description("Website")]
        Website,

        [Description("Human Interface Process")]
        HumanInterfaceProcess 
    }
}
