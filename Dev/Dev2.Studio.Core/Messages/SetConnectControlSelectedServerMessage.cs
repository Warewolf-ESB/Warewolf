using Dev2.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class SetConnectControlSelectedServerMessage : IMessage
    {
        public SetConnectControlSelectedServerMessage(IEnvironmentModel selectedServer, ConnectControlInstanceType connectControlInstanceType)
        {
            SelectedServer = selectedServer;
            ConnectControlInstanceType = connectControlInstanceType;
        }

        public IEnvironmentModel SelectedServer { get; set; }

        public ConnectControlInstanceType ConnectControlInstanceType { get; set; }
    }
}
