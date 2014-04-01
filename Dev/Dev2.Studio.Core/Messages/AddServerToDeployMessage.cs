using Dev2.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerToDeployMessage
    {
        public IEnvironmentModel Server { get; set; }
        public ConnectControlInstanceType ConnectControlInstanceType { get; set; }

        public AddServerToDeployMessage(IEnvironmentModel server)
        {
            Server = server;
        }

        public AddServerToDeployMessage(IEnvironmentModel server, ConnectControlInstanceType connectControlInstanceType)
        {
            Server = server;
            ConnectControlInstanceType = connectControlInstanceType;
        }
    }
}
