using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class SelectedServerConnectedMessage : IMessage
    {

        public IEnvironmentModel SelectedServer { get; set; }

        public SelectedServerConnectedMessage(IEnvironmentModel selectedServer)
        {
            SelectedServer = selectedServer;
        }
    }
}
