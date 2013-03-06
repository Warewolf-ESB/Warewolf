using Dev2.Network.Messages;

namespace Dev2.Runtime.Configuration
{
    public interface ISettingsMessage : INetworkMessage
    {
        ISettingsMessage Execute();
    }
}