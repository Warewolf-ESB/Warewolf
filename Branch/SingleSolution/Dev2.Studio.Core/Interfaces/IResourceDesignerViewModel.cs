using System.Text;

namespace Dev2.Studio.Core.Interfaces {

    public interface IDesignerViewModel {

        IEnvironmentModel EnvironmentModel { get; }
        StringBuilder ServiceDefinition { get; set; }
        IContextualResourceModel ResourceModel { get; set; }
    }
}