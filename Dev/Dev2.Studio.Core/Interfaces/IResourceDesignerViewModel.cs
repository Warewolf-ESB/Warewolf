using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Interfaces {
    public interface IDesignerViewModel {
        IEnvironmentModel EnvironmentModel { get; }
        string ServiceDefinition { get; set; }
        IContextualResourceModel ResourceModel { get; set; }
        void BindToModel();
    }
}