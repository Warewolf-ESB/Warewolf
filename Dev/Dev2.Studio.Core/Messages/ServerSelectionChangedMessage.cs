using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class ServerSelectionChangedMessage : IMessage
    {

        public ServerSelectionChangedMessage(IEnvironmentModel selectedServer)
        {
            SelectedServer = selectedServer;
        }

        public IEnvironmentModel SelectedServer { get; set; }
    }
}
