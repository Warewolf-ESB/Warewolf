using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddServerToExplorerMessage : IMessage
    {
        public AddServerToExplorerMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
    }
}