using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ConfigureActivityMessage : IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }
        public ModelItem ModelItem { get; set; }
    }
}