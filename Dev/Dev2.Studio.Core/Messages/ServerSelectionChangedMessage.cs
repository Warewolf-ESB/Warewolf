using Dev2.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class ServerSelectionChangedMessage : IMessage
    {

        public ConnectControlInstanceType ConnectControlInstanceType { get; set; }

        public ServerSelectionChangedMessage(IEnvironmentModel selectedServer, ConnectControlInstanceType connectControlInstanceType)
        {
            ConnectControlInstanceType = connectControlInstanceType;
            SelectedServer = selectedServer;
        }

        public IEnvironmentModel SelectedServer { get; set; }
    }
}
