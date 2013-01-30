using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Interfaces {
    public interface IResourceDesignerViewModel {
        IEnvironmentModel ResourceEnvironment { get; }
        string ServiceDefinition { get; set; }
        IResourceModel ResourceModel { get; set; }
        void BindToModel();
    }
}