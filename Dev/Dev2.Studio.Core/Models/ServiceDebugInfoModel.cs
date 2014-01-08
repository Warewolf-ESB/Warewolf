using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class ServiceDebugInfoModel : IServiceDebugInfoModel
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public string ServiceInputData { get; set; }
        public DebugMode DebugModeSetting { get; set; }
        public bool RememberInputs { get; set; }
    }
}
